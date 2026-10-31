#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Chat Hub - versione SINGLE-FILE autocontenuta (Windows / macOS / Linux).

Avvio:
    python chat_hub_single.py

Apri http://localhost:8765 sul tuo PC, oppure l'indirizzo di rete mostrato
a schermo da altri dispositivi sulla stessa rete locale.
Nessuna dipendenza esterna: solo libreria standard Python 3.7+.
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
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import parse_qs, urlparse

PORT = int(os.environ.get("CHAT_HUB_PORT", "8765"))
MAX_FILE_MB = 50
MAX_FILE_BYTES = MAX_FILE_MB * 1024 * 1024
MAX_MESSAGES = 500
MAX_TEXT_LEN = 4000

_lock = threading.Lock()
_messages = []
_next_id = [1]
users = {}

# --- Asset web incorporati (nessun file esterno richiesto) ---
INDEX_HTML = __INDEX_HTML__

APP_JS = __APP_JS__



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


def drop_user(user):
    if not user:
        return False
    with _lock:
        return users.pop(user, None) is not None


def prune_stale_users(max_idle=45.0):
    cutoff = now() - max_idle
    with _lock:
        stale = [u for u, t in users.items() if t < cutoff]
        for u in stale:
            users.pop(u, None)
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
            self._send(200, INDEX_HTML, "text/html; charset=utf-8")
            return

        if path == "/app.js":
            self._send(200, APP_JS, "application/javascript; charset=utf-8")
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
            add_message({"type": "system",
                         "content": user + " \u00e8 entrato nella stanza."})
            self._json({"ok": True, "user": user})
            return

        if path == "/leave":
            user = str(payload.get("user", "")).strip()[:24]
            if drop_user(user):
                add_message({"type": "system",
                             "content": user + " \u00e8 uscito."})
            self._json({"ok": True})
            return

        if path == "/send":
            user = str(payload.get("user", "")).strip()[:24]
            msg_type = str(payload.get("type", "text"))
            if not user:
                self._json({"ok": False, "error": "user required"}, 400)
                return

            if msg_type == "text":
                content = str(payload.get("content", "")).strip()[:MAX_TEXT_LEN]
                if not content:
                    self._json({"ok": False, "error": "empty message"}, 400)
                    return
                add_message({"type": "text", "user": user,
                             "content": content})
            elif msg_type == "file":
                name = str(payload.get("name", "file")).strip()[:128]
                mime = str(payload.get("mime",
                                       "application/octet-stream")).strip()
                data_b64 = str(payload.get("data", ""))
                if len(data_b64) > MAX_FILE_BYTES * 2:
                    self._json({"ok": False, "error": "file too large"}, 413)
                    return
                add_message({"type": "file", "user": user,
                             "name": name, "mime": mime,
                             "data": data_b64})
            else:
                self._json({"ok": False, "error": "unknown type"}, 400)
                return

            self._json({"ok": True})
            return

        self._send(404, "not found", "text/plain; charset=utf-8")


def open_browser(url):
    try:
        system_name = platform.system()
        if system_name == "Darwin":
            subprocess.Popen(["open", url])
        elif system_name == "Windows":
            os.startfile(url)  # noqa: solo Windows
        else:
            subprocess.Popen(["xdg-open", url])
    except Exception:
        pass


def main():
    parser = argparse.ArgumentParser(
        description="Chat Hub - chat di rete locale")
    parser.add_argument("--port", type=int, default=PORT,
                        help="porta di ascolto (default %d)" % PORT)
    parser.add_argument("--no-browser", action="store_true",
                        help="non aprire il browser automaticamente")
    args = parser.parse_args()

    port = args.port
    try:
        server = ThreadingHTTPServer(("0.0.0.0", port), Handler)
    except OSError as exc:
        print("Impossibile aprire la porta %d: %s" % (port, exc))
        print("Suggerimento: python chat_hub_single.py --port %d" % (port + 1))
        sys.exit(1)

    ip_addr = local_ip()
    url_localhost = "http://localhost:%d" % port
    url_lan = "http://%s:%d" % (ip_addr, port)

    print("=" * 54)
    print(" Chat Hub avviato")
    print("   Questo PC : %s" % url_localhost)
    print("   Rete      : %s" % url_lan)
    print("   Gli altri utenti sulla stessa rete aprono l'indirizzo Rete.")
    print("   Ctrl+C per fermare.")
    print("=" * 54)

    if not args.no_browser:
        threading.Timer(1.0,
                        lambda: open_browser(url_localhost)).start()

    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\nChiusura in corso...")
    finally:
        server.shutdown()


if __name__ == "__main__":
    main()
