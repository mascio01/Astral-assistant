#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Chat Hub - chat di rete locale senza dipendenze esterne.

Endpoint:
  GET  /            pagina web
  GET  /app.js      javascript client
  GET  /poll?since=N messaggi con id > N + utenti online
  POST /join        {user}
  POST /send        {user, type, content|name+data+mime}
  POST /leave       {user}

La stanza si chiude EMPTY_GRACE secondi dopo l'uscita dell'ultimo utente.
"""

import argparse
import json
import os
import platform
import socket
import subprocess
import sys
import threading
import time
import uuid
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import parse_qs, urlparse

PORT = int(os.environ.get("CHAT_HUB_PORT", "8765"))
MAX_FILE_MB = 50
MAX_FILE_BYTES = MAX_FILE_MB * 1024 * 1024
EMPTY_GRACE = 10.0
MAX_MESSAGES = 500
MAX_TEXT_LEN = 4000
POLL_TIMEOUT = 25.0

_lock = threading.Lock()
_messages = []          # lista di dict con 'id'
_next_id = [1]
users = {}              # user -> last_seen (epoch)
_empty_since = [None]
_server_ref = [None]
_shutdown_started = [False]


def now():
    return time.time()


def add_message(msg):
    with _lock:
        msg["id"] = _next_id[0]
        _next_id[0] += 1
        msg["ts"] = now()
        _messages.append(msg)
        if len(_messages) > MAX_MESSAGES:
            del _messages[: len(_messages) - MAX_MESSAGES]
    return msg["id"]


def touch_user(user):
    if not user:
        return
    with _lock:
        users[user] = now()
        _empty_since[0] = None


def drop_user(user):
    if not user:
        return False
    with _lock:
        existed = users.pop(user, None) is not None
        if existed and not users and _empty_since[0] is None:
            _empty_since[0] = now()
    return existed


def prune_stale_users(max_idle=45.0):
    """Rimuove utenti che non fanno polling da troppo tempo (tab chiusa)."""
    cutoff = now() - max_idle
    with _lock:
        stale = [u for u, t in users.items() if t < cutoff]
        for u in stale:
            users.pop(u, None)
        if stale and not users and _empty_since[0] is None:
            _empty_since[0] = now()
    return stale


def snapshot(since):
    with _lock:
        msgs = [m for m in _messages if m["id"] > since]
        online = sorted(users.keys())
    return {"messages": msgs, "users": online}


def local_ip():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.connect(("8.8.8.8", 80))
        return s.getsockname()[0]
    except Exception:
        return "127.0.0.1"
    finally:
        s.close()


# Cartella dei file statici: funziona sia da sorgente che da exe (PyInstaller).
if getattr(sys, "frozen", False):
    _BASE = getattr(sys, "_MEIPASS", os.path.dirname(sys.executable))
else:
    _BASE = os.path.dirname(os.path.abspath(__file__))

STATIC_DIR = os.path.join(_BASE, "static")


def read_static(name):
    """Legge un file statico, con fallback su piu' percorsi."""
    candidates = [
        os.path.join(STATIC_DIR, name),
        os.path.join(_BASE, name),
        os.path.join(os.path.dirname(os.path.abspath(__file__)), "static", name),
    ]
    for path in candidates:
        try:
            with open(path, "r", encoding="utf-8") as fh:
                return fh.read()
        except OSError:
            continue
    return None


class Handler(BaseHTTPRequestHandler):
    server_version = "ChatHub/1.0"

    def log_message(self, fmt, *args):
        pass

    def _send(self, code, body, ctype="application/json; charset=utf-8"):
        if isinstance(body, str):
            body = body.encode("utf-8")
        self.send_response(code)
        self.send_header("Content-Type", ctype)
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        try:
            self.wfile.write(body)
        except (BrokenPipeError, ConnectionResetError):
            pass

    def _json(self, obj, code=200):
        self._send(code, json.dumps(obj))

    def _read_json(self):
        try:
            length = int(self.headers.get("Content-Length", "0"))
        except ValueError:
            return {}
        if length <= 0 or length > MAX_FILE_BYTES * 2:
            return {}
        raw = self.rfile.read(length)
        try:
            data = json.loads(raw.decode("utf-8"))
            return data if isinstance(data, dict) else {}
        except (ValueError, UnicodeDecodeError):
            return {}

    def do_GET(self):
        parsed = urlparse(self.path)
        path = parsed.path

        if path in ("/", "/index.html"):
            html = read_static("index.html")
            if html is None:
                self._send(500, "index.html mancante", "text/plain; charset=utf-8")
                return
            self._send(200, html, "text/html; charset=utf-8")
            return

        if path == "/app.js":
            js = read_static("app.js")
            if js is None:
                self._send(404, "app.js mancante", "text/plain; charset=utf-8")
                return
            self._send(200, js, "application/javascript; charset=utf-8")
            return

        if path == "/poll":
            qs = parse_qs(parsed.query)
            try:
                since = int(qs.get("since", ["0"])[0])
            except ValueError:
                since = 0
            user = (qs.get("user", [""])[0] or "").strip()[:24]
            touch_user(user)
            prune_stale_users()
            data = snapshot(since)
            data["ok"] = True
            self._json(data)
            return

        if path == "/health":
            with _lock:
                n_users = len(users)
                n_msgs = len(_messages)
            self._json({"ok": True, "users": n_users, "messages": n_msgs})
            return

        self._send(404, "not found", "text/plain; charset=utf-8")

    def do_POST(self):
        parsed = urlparse(self.path)
        path = parsed.path
        payload = self._read_json()

        if path == "/join":
            user = str(payload.get("user", "")).strip()[:24]
            if not user:
                user = "Utente" + str(int(time.time()) % 1000)
            touch_user(user)
            add_message({"type": "system", "content": user + " \u00e8 entrato nella stanza."})
            self._json({"ok": True, "user": user})
            return

        if path == "/leave":
            user = str(payload.get("user", "")).strip()[:24]
            if drop_user(user):
                add_message({"type": "system", "content": user + " \u00e8 uscito."})
            self._json({"ok": True})
            return

        if path == "/send":
            user = str(payload.get("user", "")).strip()[:24]
            mtype = payload.get("type")
            touch_user(user)

            if mtype == "text":
                content = str(payload.get("content", ""))[:MAX_TEXT_LEN].strip()
                if not content:
                    self._json({"ok": False, "error": "testo vuoto"}, 400)
                    return
                add_message({"type": "text", "user": user, "content": content})
                self._json({"ok": True})
                return

            if mtype == "file":
                name = os.path.basename(str(payload.get("name", "file")))[:120] or "file"
                mime = str(payload.get("mime", "application/octet-stream"))[:100]
                data = payload.get("data", "")
                if not isinstance(data, str) or not data:
                    self._json({"ok": False, "error": "dati mancanti"}, 400)
                    return
                approx = len(data) * 3 // 4
                if approx > MAX_FILE_BYTES:
                    self._json({"ok": False, "error": "file oltre %d MB" % MAX_FILE_MB}, 413)
                    return
                add_message({"type": "file", "user": user, "name": name,
                             "mime": mime, "data": data})
                self._json({"ok": True})
                return

            self._json({"ok": False, "error": "tipo non valido"}, 400)
            return

        self._send(404, json.dumps({"ok": False, "error": "not found"}))


def watchdog():
    """Chiude il server quando la stanza resta vuota oltre EMPTY_GRACE."""
    while True:
        time.sleep(1.0)
        prune_stale_users()
        with _lock:
            empty = not users
            since = _empty_since[0]
            started = _shutdown_started[0]
        if empty and since is not None and not started:
            if now() - since >= EMPTY_GRACE:
                with _lock:
                    _shutdown_started[0] = True
                print("\n[Chat Hub] Stanza vuota da %ds: chiusura." % int(EMPTY_GRACE))
                srv = _server_ref[0]
                if srv is not None:
                    threading.Thread(target=srv.shutdown, daemon=True).start()
                return


def open_browser(url):
    try:
        if sys.platform.startswith("win"):
            os.startfile(url)  # type: ignore[attr-defined]
        elif sys.platform == "darwin":
            subprocess.Popen(["open", url])
        else:
            subprocess.Popen(["xdg-open", url])
    except Exception as exc:
        print("[Chat Hub] Impossibile aprire il browser: %s" % exc)


def main():
    parser = argparse.ArgumentParser(description="Chat Hub - stanza locale")
    parser.add_argument("--no-browser", action="store_true", help="Non aprire il browser automaticamente")
    parser.add_argument("--port", type=int, default=None, help="Porta (sovrascrive CHAT_HUB_PORT)")
    args = parser.parse_args()

    port = args.port if args.port else PORT
    try:
        server = ThreadingHTTPServer(("0.0.0.0", port), Handler)
    except OSError as exc:
        print("[Chat Hub] Porta %d occupata (%s). Provo la successiva..." % (port, exc))
        server = ThreadingHTTPServer(("0.0.0.0", port + 1), Handler)
        port = port + 1

    server.daemon_threads = True
    _server_ref[0] = server

    ip = local_ip()
    url_local = "http://127.0.0.1:%d/" % port
    url_lan = "http://%s:%d/" % (ip, port)

    print("=" * 52)
    print("  CHAT HUB - stanza unica globale")
    print("=" * 52)
    print("  Locale : %s" % url_local)
    print("  In rete: %s" % url_lan)
    print("-" * 52)
    print("  La stanza si chiude %ds dopo l'uscita dell'ultimo utente." % int(EMPTY_GRACE))
    print("  Ctrl+C per terminare subito.")
    print("=" * 52)

    threading.Thread(target=watchdog, daemon=True).start()
    if not args.no_browser:
        threading.Timer(1.2, lambda: open_browser(url_local)).start()

    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\n[Chat Hub] Interrotto dall'utente.")
    finally:
        server.server_close()
        print("[Chat Hub] Server chiuso.")


if __name__ == "__main__":
    main()