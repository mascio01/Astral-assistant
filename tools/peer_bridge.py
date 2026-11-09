# -*- coding: utf-8 -*-
"""peer_bridge.py - Bridge LAN peer-to-peer tra sessioni Astral su PC diversi.

Perche' esiste: due sessioni Astral su macchine diverse devono potersi passare
messaggi, file e contesto di lavoro senza servizi esterni e senza cloud.

Trasporto: HTTP su LAN (ThreadingHTTPServer in thread daemon) + discovery UDP
broadcast. Autenticazione: token condiviso generato al primo avvio, obbligatorio
su OGNI endpoint (header X-Astral-Token). Il server ascolta su 0.0.0.0 ma senza
il token non risponde a nulla: non e' un servizio aperto.

Archivio su disco (.astral_peer/):
  config.json          node_id, token, porta http/udp, peer noti
  inbox/<node>/        messaggi in arrivo (un JSON per messaggio)
  outbox/<node>/       file in uscita (serviti via GET /file)
  context/<node>.json  ultimo snapshot di contesto pubblicato dal nodo

Regole: best-effort, nessuna eccezione al chiamante dei tool, scritture atomiche.
"""

import json
import os
import re
import secrets
import socket
import threading
import time
import uuid
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen

PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BASE = os.path.join(PROJECT_ROOT, ".astral_peer")
DEFAULT_HTTP_PORT = 8787
DEFAULT_UDP_PORT = 8788
MAX_INLINE_BYTES = 32 * 1024 * 1024   # oltre: si serve via /file, non inline
MAX_TEXT_CHARS = 200000
_DISCOVER_MSG = b"ASTRAL_PEER_DISCOVER_V1"

_SERVER = {"httpd": None, "thread": None, "udp": None, "port": None}


# --------------------------------------------------------------------- config
def _ensure_dirs(node=None):
    for sub in ("inbox", "outbox", "context", "offers"):
        d = os.path.join(BASE, sub)
        os.makedirs(d, exist_ok=True)
    if node:
        os.makedirs(os.path.join(BASE, "inbox", node), exist_ok=True)
        os.makedirs(os.path.join(BASE, "outbox", node), exist_ok=True)


def _atomic_write(path, data):
    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8") as f:
        f.write(data)
    os.replace(tmp, path)


def _slug(text):
    s = re.sub(r"[^a-z0-9]+", "_", (text or "").lower()).strip("_")
    return s or "node"


def config_path():
    return os.path.join(BASE, "config.json")


def load_config():
    """Legge (o crea) la config locale. Idempotente e sicura in multi-thread."""
    _ensure_dirs()
    p = config_path()
    if os.path.exists(p):
        try:
            with open(p, "r", encoding="utf-8") as f:
                cfg = json.load(f)
            if cfg.get("token"):
                return cfg
        except Exception:
            pass
    cfg = {
        "node_id": _slug(socket.gethostname()),
        "display_name": socket.gethostname(),
        "token": secrets.token_hex(24),
        "port": DEFAULT_HTTP_PORT,
        "udp_port": DEFAULT_UDP_PORT,
        "peers": [],
        "created": time.strftime("%Y-%m-%dT%H:%M:%S"),
    }
    _atomic_write(p, json.dumps(cfg, indent=2))
    return cfg


def save_config(cfg):
    _ensure_dirs()
    _atomic_write(config_path(), json.dumps(cfg, indent=2))
    return cfg


def node_id():
    return load_config()["node_id"]


def local_ips():
    """IPv4 locali utili (esclusi loopback e link-local 169.254.*)."""
    ips = []
    try:
        for info in socket.getaddrinfo(socket.gethostname(), None, socket.AF_INET):
            ip = info[4][0]
            if ip.startswith("127.") or ip.startswith("169.254."):
                continue
            if ip not in ips:
                ips.append(ip)
    except Exception:
        pass
    if not ips:
        try:
            s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            s.connect(("8.8.8.8", 80))
            ips.append(s.getsockname()[0])
            s.close()
        except Exception:
            pass
    return ips


# ------------------------------------------------------------------ discovery
def udp_discover(timeout=1.6, port=None):
    """Broadcast UDP: ogni peer Astral attivo risponde con la sua identita'."""
    cfg = load_config()
    port = port or cfg.get("udp_port", DEFAULT_UDP_PORT)
    found = {}
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
    try:
        sock.settimeout(0.4)
        for bcast in ("255.255.255.255", "192.168.1.255"):
            try:
                sock.sendto(_DISCOVER_MSG, (bcast, port))
            except Exception:
                pass
        deadline = time.time() + timeout
        while time.time() < deadline:
            try:
                data, addr = sock.recvfrom(4096)
            except socket.timeout:
                continue
            except Exception:
                break
            try:
                info = json.loads(data.decode("utf-8"))
            except Exception:
                continue
            if info.get("proto") != "astral_peer_v1" or info.get("node_id") == cfg["node_id"]:
                continue
            info["ip"] = addr[0]
            found[info["node_id"]] = info
    finally:
        sock.close()
    return list(found.values())


def _udp_server():
    cfg = load_config()
    port = cfg.get("udp_port", DEFAULT_UDP_PORT)
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    try:
        sock.bind(("", port))
    except Exception:
        sock.close()
        return None
    payload = json.dumps({
        "proto": "astral_peer_v1",
        "node_id": cfg["node_id"],
        "display_name": cfg.get("display_name") or cfg["node_id"],
        "port": cfg.get("port", DEFAULT_HTTP_PORT),
    }).encode("utf-8")
    while True:
        try:
            data, addr = sock.recvfrom(1024)
            if data.strip() == _DISCOVER_MSG:
                sock.sendto(payload, addr)
        except Exception:
            time.sleep(0.05)


# --------------------------------------------------------------------- server
def _safe_name(name):
    name = os.path.basename(str(name or "")).replace("\\", "_")
    return re.sub(r"[^\w.\- ]+", "_", name)[:180] or "file.bin"


def _file_sha256(path):
    import hashlib
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def _auth_tokens(cfg):
    """Token accettati: il proprio + quelli dei peer (pairing bidirezionale)."""
    toks = set()
    if cfg.get("token"):
        toks.add(cfg["token"])
    for t in cfg.get("accepted_tokens") or []:
        if t:
            toks.add(t)
    for p in cfg.get("peers") or []:
        if p.get("token"):
            toks.add(p["token"])
    return toks


class _Handler(BaseHTTPRequestHandler):
    protocol_version = "HTTP/1.1"
    server_version = "AstralPeer/1.0"

    def log_message(self, fmt, *a):   # silenzioso: niente spam su stderr
        pass

    # -- helpers
    def _send(self, code, obj=None, raw=None, ctype="application/json"):
        body = raw if raw is not None else json.dumps(obj, ensure_ascii=False).encode("utf-8")
        self.send_response(code)
        self.send_header("Content-Type", ctype)
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        try:
            self.wfile.write(body)
        except Exception:
            pass

    def _auth(self):
        cfg = load_config()
        tok = self.headers.get("X-Astral-Token", "")
        ok = False
        for cand in _auth_tokens(cfg):
            try:
                if secrets.compare_digest(tok, cand):
                    ok = True
                    break
            except Exception:
                continue
        if not ok:
            self._send(401, {"error": "token non valido"})
            return None
        return cfg

    def _body(self, limit=MAX_INLINE_BYTES):
        try:
            n = int(self.headers.get("Content-Length") or 0)
        except Exception:
            n = 0
        if n <= 0 or n > limit:
            return None
        return self.rfile.read(n)

    # -- routes
    def do_GET(self):
        cfg = self._auth()
        if not cfg:
            return
        path = self.path.split("?")[0]
        if path == "/health":
            return self._send(200, {"ok": True, "node_id": cfg["node_id"],
                                    "display_name": cfg.get("display_name"),
                                    "port": cfg.get("port")})
        if path == "/inbox":
            items = []
            d = os.path.join(BASE, "inbox", cfg["node_id"])
            for fn in sorted(os.listdir(d)) if os.path.isdir(d) else []:
                if not fn.endswith(".json"):
                    continue
                try:
                    with open(os.path.join(d, fn), "r", encoding="utf-8") as f:
                        m = json.load(f)
                    m["_file"] = fn
                    items.append(m)
                except Exception:
                    continue
            return self._send(200, {"messages": items[-50:]})
        if path.startswith("/file/"):
            # /file/<nome> servito da outbox
            name = _safe_name(path[len("/file/"):])
            fp = os.path.join(BASE, "outbox", cfg["node_id"], name)
            if not os.path.isfile(fp):
                return self._send(404, {"error": "file non trovato", "name": name})
            with open(fp, "rb") as f:
                data = f.read()
            return self._send(200, raw=data, ctype="application/octet-stream")
        return self._send(404, {"error": "endpoint sconosciuto"})

    def do_POST(self):
        cfg = self._auth()
        if not cfg:
            return
        path = self.path.split("?")[0]
        if path == "/message":
            raw = self._body()
            if raw is None:
                return self._send(400, {"error": "body mancante o troppo grande"})
            try:
                msg = json.loads(raw.decode("utf-8"))
            except Exception as e:
                return self._send(400, {"error": f"json non valido: {e}"})
            msg.setdefault("ts", time.strftime("%Y-%m-%dT%H:%M:%S"))
            msg["from"] = msg.get("from") or "?"
            d = os.path.join(BASE, "inbox", cfg["node_id"])
            os.makedirs(d, exist_ok=True)
            fn = "%d_%s.json" % (int(time.time() * 1000), uuid.uuid4().hex[:8])
            _atomic_write(os.path.join(d, fn), json.dumps(msg, ensure_ascii=False, indent=2))
            return self._send(200, {"ok": True, "stored": fn})
        if path == "/context":
            raw = self._body(2 * 1024 * 1024)
            if raw is None:
                return self._send(400, {"error": "body mancante"})
            try:
                snap = json.loads(raw.decode("utf-8"))
            except Exception as e:
                return self._send(400, {"error": f"json non valido: {e}"})
            src = _slug(snap.get("from") or "peer")
            fp = os.path.join(BASE, "context", src + ".json")
            snap["received"] = time.strftime("%Y-%m-%dT%H:%M:%S")
            _atomic_write(fp, json.dumps(snap, ensure_ascii=False, indent=2))
            return self._send(200, {"ok": True, "from": src})
        if path == "/pair":
            raw = self._body(64 * 1024)
            if raw is None:
                return self._send(400, {"error": "body mancante"})
            try:
                info = json.loads(raw.decode("utf-8"))
            except Exception as e:
                return self._send(400, {"error": "json non valido: %s" % e})
            if not info.get("node_id") or not info.get("token"):
                return self._send(400, {"error": "servono node_id e token"})
            add_peer(info["node_id"], info.get("host"), info.get("port"), info["token"])
            return self._send(200, {"ok": True, "registered": info["node_id"]})
        if path == "/upload":
            raw = self._body()
            if raw is None:
                return self._send(400, {"error": "file mancante o troppo grande"})
            name = _safe_name(self.headers.get("X-Filename") or "upload.bin")
            src = _slug(self.headers.get("X-From") or "peer")
            d = os.path.join(BASE, "inbox", cfg["node_id"], "files")
            os.makedirs(d, exist_ok=True)
            fp = os.path.join(d, src + "__" + name)
            with open(fp, "wb") as f:
                f.write(raw)
            return self._send(200, {"ok": True, "path": fp, "size": len(raw),
                                    "sha256": _file_sha256(fp)})
        return self._send(404, {"error": "endpoint sconosciuto"})


# --------------------------------------------------------------- start / stop
def start_server(port=None):
    """Avvia il server HTTP+UDP in background (idempotente)."""
    if _SERVER["httpd"] is not None:
        return {"ok": True, "already_running": True, "port": _SERVER["port"],
                "node_id": node_id()}
    cfg = load_config()
    port = int(port or cfg.get("port", DEFAULT_HTTP_PORT))
    httpd = None
    last_err = None
    for candidate in range(port, port + 12):
        try:
            httpd = ThreadingHTTPServer(("0.0.0.0", candidate), _Handler)
            port = candidate
            break
        except Exception as e:
            last_err = e
    if httpd is None:
        return {"error": f"nessuna porta libera tra {port} e {port + 11}: {last_err}"}
    httpd.daemon_threads = True
    t = threading.Thread(target=httpd.serve_forever, name="astral-peer-http", daemon=True)
    t.start()
    u = threading.Thread(target=_udp_server, name="astral-peer-udp", daemon=True)
    u.start()
    _SERVER.update({"httpd": httpd, "thread": t, "udp": u, "port": port})
    if port != cfg.get("port"):
        cfg["port"] = port
        save_config(cfg)
    return {"ok": True, "port": port, "node_id": cfg["node_id"],
            "display_name": cfg.get("display_name"),
            "token": cfg["token"], "ips": local_ips()}


def stop_server():
    httpd = _SERVER.get("httpd")
    if not httpd:
        return {"ok": True, "running": False}
    try:
        httpd.shutdown()
        httpd.server_close()
    except Exception:
        pass
    _SERVER.update({"httpd": None, "thread": None, "udp": None, "port": None})
    return {"ok": True, "running": False}


def ensure_running():
    """Avvia il server al primo uso di un tool peer (lazy, non blocca l'avvio)."""
    if _SERVER["httpd"] is None:
        return start_server()
    return {"ok": True, "already_running": True, "port": _SERVER["port"]}


def status():
    cfg = load_config()
    d = os.path.join(BASE, "inbox", cfg["node_id"])
    pending = len([f for f in os.listdir(d) if f.endswith(".json")]) if os.path.isdir(d) else 0
    return {
        "node_id": cfg["node_id"],
        "display_name": cfg.get("display_name"),
        "running": _SERVER["httpd"] is not None,
        "port": _SERVER["port"] or cfg.get("port"),
        "ips": local_ips(),
        "pending_messages": pending,
        "peers": cfg.get("peers", []),
        "base_dir": BASE,
    }


# ----------------------------------------------------------------- peer table
def add_peer(node, host=None, port=None, token=None):
    """Registra/aggiorna un peer. node puo' essere node_id o hostname."""
    cfg = load_config()
    node = _slug(node)
    port = int(port or DEFAULT_HTTP_PORT)
    for p in cfg["peers"]:
        if p["node_id"] == node:
            if host:
                p["host"] = host
            p["port"] = port
            if token:
                p["token"] = token
            p["updated"] = time.strftime("%Y-%m-%dT%H:%M:%S")
            save_config(cfg)
            return {"ok": True, "peer": p, "updated": True}
    peer = {"node_id": node, "host": host or "", "port": port,
            "token": token or cfg["token"],
            "added": time.strftime("%Y-%m-%dT%H:%M:%S")}
    cfg["peers"].append(peer)
    save_config(cfg)
    return {"ok": True, "peer": peer, "updated": False}


def list_peers():
    return load_config().get("peers", [])


def pair(host, port=None, peer_token=None, node=None, timeout=15):
    """Pairing bidirezionale: registra il peer e fa registrare noi dal peer."""
    ensure_running()
    cfg = load_config()
    port = int(port or DEFAULT_HTTP_PORT)
    token = peer_token or cfg["token"]
    tmp = {"node_id": node or "peer", "host": host, "port": port, "token": token}
    r = _http(tmp, "GET", "/health", timeout=timeout)
    if "error" in r:
        return {"error": "pairing fallito: " + str(r["error"]),
                "hint": "il peer deve avere il bridge attivo (peer_start) e il token usato "
                        "deve essere uno di quelli che accetta"}
    peer_id = r.get("node_id") or node or "peer"
    add_peer(peer_id, host, port, token)
    me = {"node_id": cfg["node_id"], "host": (local_ips() or [host])[0],
          "port": cfg.get("port"), "token": cfg["token"]}
    back = _http(tmp, "POST", "/pair",
                 data=json.dumps(me, ensure_ascii=False).encode("utf-8"),
                 headers={"Content-Type": "application/json"}, timeout=timeout)
    return {"ok": True, "paired_with": peer_id, "peer_health": r,
            "back_registered": "error" not in back, "back_detail": back,
            "local": cfg["node_id"]}


def _peer(node=None):
    """Risolve il peer target: se node e' None e c'e' un solo peer, lo usa."""
    peers = list_peers()
    if node:
        want = _slug(node)
        for p in peers:
            if p["node_id"] == want or _slug(p.get("host", "")) == want:
                return p
        return None
    return peers[0] if len(peers) == 1 else None


# ------------------------------------------------------------ client (outgoing)
def _http(peer, method, path, data=None, headers=None, timeout=20):
    cfg = load_config()
    host = peer.get("host") or ""
    if not host:
        return {"error": f"peer {peer.get('node_id')} senza host: usa add_peer con host"}
    url = "http://%s:%s%s" % (host, peer.get("port", DEFAULT_HTTP_PORT), path)
    hdrs = {"X-Astral-Token": peer.get("token") or cfg["token"]}
    hdrs.update(headers or {})
    req = Request(url, data=data, headers=hdrs, method=method)
    try:
        with urlopen(req, timeout=timeout) as r:
            raw = r.read()
        try:
            return json.loads(raw.decode("utf-8"))
        except Exception:
            return {"ok": True, "raw_len": len(raw)}
    except HTTPError as e:
        try:
            return {"error": e.read().decode("utf-8", "replace")[:400], "status": e.code}
        except Exception:
            return {"error": f"HTTP {e.code}"}
    except URLError as e:
        return {"error": f"peer non raggiungibile ({url}): {e.reason}",
                "hint": "verifica che il server sia avviato sull'altro PC e che la regola firewall sia attiva"}
    except Exception as e:
        return {"error": f"{type(e).__name__}: {e}"}


def send_message(text, to=None, kind="chat", meta=None):
    """Invia un messaggio testuale a un peer."""
    peer = _peer(to)
    if peer is None:
        return {"error": "peer non specificato o non registrato",
                "peers": list_peers()}
    if not str(text or "").strip():
        return {"error": "testo vuoto"}
    payload = {"from": node_id(), "kind": kind,
               "text": str(text)[:MAX_TEXT_CHARS],
               "meta": meta or {}}
    body = json.dumps(payload, ensure_ascii=False).encode("utf-8")
    res = _http(peer, "POST", "/message", data=body,
                headers={"Content-Type": "application/json"})
    if "error" not in res:
        res["sent_to"] = peer["node_id"]
    return res


def fetch_messages(consume=True):
    """Legge i messaggi in arrivo nella inbox locale."""
    cfg = load_config()
    d = os.path.join(BASE, "inbox", cfg["node_id"])
    out = []
    if os.path.isdir(d):
        for fn in sorted(os.listdir(d)):
            if not fn.endswith(".json"):
                continue
            fp = os.path.join(d, fn)
            try:
                with open(fp, "r", encoding="utf-8") as f:
                    m = json.load(f)
            except Exception:
                continue
            m["_file"] = fn
            m["_priority"] = ("alta" if any(k in (m.get("text") or "").upper()
                                             for k in ("BLOCCO", "EMERGENZA")) else "normale")
            out.append(m)
            if consume:
                done = os.path.join(BASE, "inbox", cfg["node_id"], "read")
                os.makedirs(done, exist_ok=True)
                try:
                    os.replace(fp, os.path.join(done, fn))
                except Exception:
                    pass
    return {"count": len(out), "messages": out}


def send_file(path, to=None, note=""):
    """Invia un file a un peer via POST /upload."""
    peer = _peer(to)
    if peer is None:
        return {"error": "peer non specificato o non registrato"}
    if not os.path.isfile(path):
        return {"error": f"file non trovato: {path}"}
    size = os.path.getsize(path)
    if size > MAX_INLINE_BYTES:
        return {"error": f"file troppo grande per invio diretto ({size} byte > {MAX_INLINE_BYTES})",
                "hint": "usa share_file + l'altro scarica via GET /file"}
    name = _safe_name(os.path.basename(path))
    with open(path, "rb") as f:
        data = f.read()
    res = _http(peer, "POST", "/upload", data=data,
                headers={"Content-Type": "application/octet-stream",
                         "X-Filename": name,
                         "X-From": node_id()},
                timeout=max(20, int(size / (1024 * 1024)) + 20))
    if "error" not in res:
        res["filename"] = name
        res["size"] = size
        res["sent_to"] = peer["node_id"]
        if note:
            res["note"] = note
    return res


def share_file(path, to=None):
    """Mette un file nella outbox locale e restituisce l'URL di download."""
    if not os.path.isfile(path):
        return {"error": f"file non trovato: {path}"}
    r = ensure_running()
    if "error" in r:
        return r
    cfg = load_config()
    d = os.path.join(BASE, "outbox", cfg["node_id"])
    os.makedirs(d, exist_ok=True)
    name = _safe_name(os.path.basename(path))
    dst = os.path.join(d, name)
    if os.path.abspath(path) != os.path.abspath(dst):
        import shutil
        shutil.copy2(path, dst)
    peer = _peer(to)
    host = (peer or {}).get("host") or (local_ips() or ["127.0.0.1"])[0]
    port = _SERVER["port"] or cfg.get("port")
    return {"ok": True, "filename": name, "size": os.path.getsize(dst),
            "url": "http://%s:%s/file/%s" % (host, port, name),
            "node_id": cfg["node_id"],
            "sha256": _file_sha256(dst)}


def download_file(url, dest_dir=None, token=None):
    """Scarica un file da un URL peer (usa il token locale se non indicato)."""
    token = token or load_config()["token"]
    dest_dir = dest_dir or os.path.join(BASE, "downloads")
    os.makedirs(dest_dir, exist_ok=True)
    req = Request(url, headers={"X-Astral-Token": token})
    try:
        with urlopen(req, timeout=120) as r:
            data = r.read()
    except Exception as e:
        return {"error": f"download fallito: {e}"}
    name = _safe_name(os.path.basename(url.split("?")[0]) or "download.bin")
    fp = os.path.join(dest_dir, name)
    with open(fp, "wb") as f:
        f.write(data)
    return {"ok": True, "path": fp, "size": len(data), "sha256": _file_sha256(fp)}


# ------------------------------------------------------------------- contesto
def build_context_snapshot(extra=None):
    """Fotografia compatta dello stato di lavoro di questa sessione."""
    import subprocess
    cfg = load_config()
    snap = {
        "from": cfg["node_id"],
        "display_name": cfg.get("display_name"),
        "ts": time.strftime("%Y-%m-%dT%H:%M:%S"),
        "cwd_hint": PROJECT_ROOT,
    }
    try:
        from core import memory_store as _ms
        snap["checkpoint"] = _ms.checkpoint_pull()
    except Exception:
        pass
    try:
        with open(os.path.join(PROJECT_ROOT, ".selfmap.md"), "r", encoding="utf-8", errors="replace") as f:
            head = f.read(1200)
        snap["selfmap_head"] = head
    except Exception:
        pass
    try:
        out = subprocess.run(["git", "-C", PROJECT_ROOT, "status", "--porcelain"],
                             capture_output=True, text=True, timeout=20)
        snap["git_status"] = (out.stdout or "").splitlines()[:40]
        snap["git_branch"] = subprocess.run(
            ["git", "-C", PROJECT_ROOT, "rev-parse", "--abbrev-ref", "HEAD"],
            capture_output=True, text=True, timeout=15).stdout.strip()
    except Exception:
        pass
    # ultimi eventi dal bus interno (contesto condiviso tra processi locali)
    try:
        from utils import watchdog_bus as _wb
        snap["bus_notices"] = _wb.read_notices(advance=False)[:10]
    except Exception:
        pass
    if extra:
        snap["extra"] = extra
    return snap


def push_context(to=None, extra=None):
    """Pubblica lo snapshot locale sul peer."""
    peer = _peer(to)
    if peer is None:
        return {"error": "peer non specificato o non registrato"}
    snap = build_context_snapshot(extra)
    body = json.dumps(snap, ensure_ascii=False).encode("utf-8")
    res = _http(peer, "POST", "/context", data=body,
                headers={"Content-Type": "application/json"})
    if "error" not in res:
        res["pushed_to"] = peer["node_id"]
    return res


def pull_context(node=None):
    """Legge gli snapshot di contesto ricevuti dai peer."""
    d = os.path.join(BASE, "context")
    out = {}
    if os.path.isdir(d):
        for fn in os.listdir(d):
            if not fn.endswith(".json"):
                continue
            try:
                with open(os.path.join(d, fn), "r", encoding="utf-8") as f:
                    snap = json.load(f)
            except Exception:
                continue
            if node and _slug(node) not in fn:
                continue
            out[fn[:-5]] = snap
    return {"count": len(out), "contexts": out}


# ------------------------------------------------------------- firewall helper
def firewall_rule(port=None, name="AstralPeer"):
    """Comando PowerShell per aprire le porte del bridge nel firewall Windows."""
    cfg = load_config()
    port = port or cfg.get("port", DEFAULT_HTTP_PORT)
    udp = cfg.get("udp_port", DEFAULT_UDP_PORT)
    return {
        "requires_admin": True,
        "commands": [
            f'netsh advfirewall firewall delete rule name="{name}-HTTP" >nul 2>&1',
            f'netsh advfirewall firewall add rule name="{name}-HTTP" dir=in action=allow protocol=TCP localport={port}',
            f'netsh advfirewall firewall delete rule name="{name}-UDP" >nul 2>&1',
            f'netsh advfirewall firewall add rule name="{name}-UDP" dir=in action=allow protocol=UDP localport={udp}',
        ],
    }
