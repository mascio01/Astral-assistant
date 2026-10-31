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
INDEX_HTML = '\ufeff<!DOCTYPE html>\n<html lang="it">\n<head>\n<meta charset="UTF-8">\n<meta name="viewport" content="width=device-width, initial-scale=1">\n<title>Chat Hub</title>\n<style>\n  * { box-sizing: border-box; margin: 0; padding: 0; }\n  body {\n    font-family: -apple-system, "Segoe UI", Roboto, Arial, sans-serif;\n    background: var(--bg, #14161a);\n    color: var(--fg, #e6e6e6);\n    height: 100vh; display: flex; flex-direction: column; overflow: hidden;\n  }\n  #top {\n    padding: 10px 14px; background: #1d2026; border-bottom: 1px solid #2c3038;\n    display: flex; justify-content: space-between; align-items: center;\n    gap: 12px; flex-wrap: wrap;\n  }\n  #top h1 { font-size: 15px; font-weight: 600; }\n  .ctl { display: flex; gap: 12px; align-items: center; font-size: 11px; }\n  .ctl input[type=color] {\n    width: 26px; height: 26px; border: none; background: none;\n    cursor: pointer; padding: 0;\n  }\n  #who { font-size: 12px; opacity: .75; }\n  #chat {\n    flex: 1; overflow-y: auto; padding: 16px;\n    display: flex; flex-direction: column; gap: 8px;\n  }\n  .msg {\n    max-width: 72%; padding: 8px 12px; border-radius: 12px;\n    word-wrap: break-word; overflow-wrap: anywhere;\n    background: #262a31;\n  }\n  .msg.own { align-self: flex-end; background: #0078d4; color: #fff; }\n  .msg.other { align-self: flex-start; }\n  .msg .meta { font-size: 10px; opacity: .65; margin-bottom: 3px; }\n  .msg img { max-width: 100%; border-radius: 8px; margin-top: 6px; display: block; }\n  .msg a.dl {\n    display: inline-block; margin-top: 6px; color: inherit;\n    text-decoration: underline; font-size: 13px;\n  }\n  .sys {\n    align-self: center; font-size: 12px; opacity: .55;\n    font-style: italic; text-align: center;\n  }\n  #bar {\n    padding: 10px 14px; background: #1d2026;\n    border-top: 1px solid #2c3038;\n    display: flex; gap: 8px; align-items: center;\n  }\n  #bar input[type=text] {\n    flex: 1; padding: 9px 14px; border-radius: 20px;\n    border: 1px solid #3a3f48; background: #262a31;\n    color: inherit; outline: none; font-size: 14px;\n  }\n  #bar input[type=text]:focus { border-color: #0078d4; }\n  button {\n    cursor: pointer; border: none; border-radius: 20px;\n    padding: 9px 16px; background: #0078d4; color: #fff;\n    font-size: 14px; font-weight: 500;\n  }\n  button:hover { opacity: .88; }\n  #ebtn, #fbtn { background: #333842; padding: 9px 12px; }\n  #emoji {\n    display: none; position: absolute; bottom: 64px; left: 14px;\n    background: #262a31; border: 1px solid #3a3f48; border-radius: 10px;\n    padding: 8px; width: 268px; max-height: 190px; overflow-y: auto;\n    grid-template-columns: repeat(8, 1fr); gap: 2px;\n    box-shadow: 0 6px 24px rgba(0,0,0,.5); z-index: 50;\n  }\n  .eb {\n    background: none; border: none; font-size: 19px;\n    cursor: pointer; padding: 4px; border-radius: 5px;\n    line-height: 1;\n  }\n  .eb:hover { background: #3a3f48; }\n  #drop {\n    position: fixed; inset: 0; display: none;\n    align-items: center; justify-content: center;\n    background: rgba(0,120,212,.22);\n    border: 4px dashed #0078d4; z-index: 999;\n    font-size: 22px; font-weight: 600; color: #fff;\n    pointer-events: none;\n  }\n</style>\n</head>\n<body>\n<div id="top">\n  <h1>Chat Hub</h1>\n  <div class="ctl">\n    <span id="who"></span>\n    <label>Bg <input type="color" id="bgc" value="#14161a"></label>\n    <label>Testo <input type="color" id="fgc" value="#e6e6e6"></label>\n  </div>\n</div>\n<div id="chat"></div>\n<div id="emoji"></div>\n<div id="bar">\n  <button id="ebtn" title="Emoji">&#128512;</button>\n  <button id="fbtn" title="Allega file">&#128206;</button>\n  <input type="text" id="inp" placeholder="Scrivi un messaggio..." autocomplete="off">\n  <button id="sendb">Invia</button>\n</div>\n<input type="file" id="fsel" multiple style="display:none">\n<div id="drop">Rilascia i file qui</div>\n<script src="/app.js"></script>\n</body>\n</html>\n'

APP_JS = '\ufeff(function () {\n  "use strict";\n\n  var ME = null;\n  var lastId = 0;\n  var pollTimer = null;\n\n  var chatEl = document.getElementById("chat");\n  var inpEl = document.getElementById("inp");\n  var whoEl = document.getElementById("who");\n  var emojiEl = document.getElementById("emoji");\n  var dropEl = document.getElementById("drop");\n\n  function api(path, body) {\n    return fetch(path, {\n      method: "POST",\n      headers: { "Content-Type": "application/json" },\n      body: JSON.stringify(body || {})\n    }).then(function (r) { return r.json(); });\n  }\n\n  function esc(s) {\n    return String(s).replace(/[&<>"\']/g, function (c) {\n      return { "&": "&amp;", "<": "&lt;", ">": "&gt;", \'"\': "&quot;", "\'": "&#39;" }[c];\n    });\n  }\n\n  function hhmm(ts) {\n    var d = new Date((ts || Date.now() / 1000) * 1000);\n    return d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });\n  }\n\n  function render(m) {\n    var div = document.createElement("div");\n\n    if (m.type === "system") {\n      div.className = "sys";\n      div.textContent = m.content;\n      chatEl.appendChild(div);\n      return;\n    }\n\n    div.className = "msg " + (m.user === ME ? "own" : "other");\n\n    var meta = document.createElement("div");\n    meta.className = "meta";\n    meta.textContent = m.user + " \\u00b7 " + hhmm(m.ts);\n    div.appendChild(meta);\n\n    if (m.type === "text") {\n      var p = document.createElement("div");\n      p.textContent = m.content;\n      div.appendChild(p);\n    } else if (m.type === "file") {\n      var isImg = (m.mime || "").indexOf("image/") === 0;\n      if (isImg) {\n        var img = document.createElement("img");\n        img.src = "data:" + m.mime + ";base64," + m.data;\n        img.alt = m.name;\n        div.appendChild(img);\n      }\n      var a = document.createElement("a");\n      a.className = "dl";\n      a.textContent = (isImg ? "\\u2b07\\ufe0f " : "\\ud83d\\udcce ") + m.name;\n      a.href = "data:" + (m.mime || "application/octet-stream") + ";base64," + m.data;\n      a.download = m.name;\n      div.appendChild(a);\n    }\n\n    chatEl.appendChild(div);\n  }\n\n  function scrollDown() {\n    chatEl.scrollTop = chatEl.scrollHeight;\n  }\n\n  function poll() {\n    fetch("/poll?since=" + lastId + "&user=" + encodeURIComponent(ME || ""))\n      .then(function (r) { return r.json(); })\n      .then(function (data) {\n        (data.messages || []).forEach(function (m) {\n          render(m);\n          if (m.id > lastId) lastId = m.id;\n        });\n        if ((data.messages || []).length) scrollDown();\n        whoEl.textContent = "\\ud83d\\udfe2 " + (data.users || []).length + " online";\n        pollTimer = setTimeout(poll, 900);\n      })\n      .catch(function () {\n        whoEl.textContent = "\\u26a0\\ufe0f disconnesso, riprovo...";\n        pollTimer = setTimeout(poll, 2500);\n      });\n  }\n\n  function sendText() {\n    var text = inpEl.value.trim();\n    if (!text) return;\n    inpEl.value = "";\n    api("/send", { user: ME, type: "text", content: text }).then(function () {\n      poll();\n    });\n  }\n\n  function sendFile(file) {\n    if (file.size > 50 * 1024 * 1024) {\n      alert("File troppo grande (max 50 MB): " + file.name);\n      return;\n    }\n    var reader = new FileReader();\n    reader.onload = function (ev) {\n      var b64 = String(ev.target.result).split(",")[1] || "";\n      api("/send", {\n        user: ME,\n        type: "file",\n        name: file.name,\n        mime: file.type || "application/octet-stream",\n        data: b64\n      }).then(function () { poll(); });\n    };\n    reader.readAsDataURL(file);\n  }\n\n  var EMOJIS = ["\\ud83d\\ude00","\\ud83d\\ude02","\\ud83d\\ude0d","\\ud83d\\ude0e","\\ud83d\\udc4d","\\u2764\\ufe0f","\\ud83d\\udd25","\\ud83c\\udf89","\\ud83d\\udcaf","\\ud83d\\udc4b","\\ud83e\\udd14","\\ud83d\\ude22","\\ud83d\\ude21","\\ud83d\\udc40","\\ud83d\\ude80","\\ud83d\\udca1","\\u2705","\\u274c","\\u26a1","\\ud83c\\udf1f","\\ud83c\\udfb5","\\ud83d\\udcf7","\\ud83d\\udcc1","\\ud83d\\udd12","\\ud83c\\udf0d","\\u2600\\ufe0f","\\ud83c\\udf19","\\ud83c\\udf55","\\ud83c\\udf7a","\\ud83c\\udfae","\\ud83c\\udfc6","\\ud83d\\udcbb"];\n\n  function buildEmoji() {\n    EMOJIS.forEach(function (e) {\n      var b = document.createElement("button");\n      b.className = "eb";\n      b.type = "button";\n      b.textContent = e;\n      b.onclick = function () {\n        inpEl.value += e;\n        inpEl.focus();\n        emojiEl.style.display = "none";\n      };\n      emojiEl.appendChild(b);\n    });\n  }\n\n  function bind() {\n    document.getElementById("sendb").onclick = sendText;\n    inpEl.addEventListener("keydown", function (e) {\n      if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); sendText(); }\n    });\n\n    document.getElementById("ebtn").onclick = function (e) {\n      e.stopPropagation();\n      emojiEl.style.display = emojiEl.style.display === "grid" ? "none" : "grid";\n    };\n    document.addEventListener("click", function (e) {\n      if (!emojiEl.contains(e.target)) emojiEl.style.display = "none";\n    });\n\n    var fsel = document.getElementById("fsel");\n    document.getElementById("fbtn").onclick = function () { fsel.click(); };\n    fsel.onchange = function () {\n      Array.prototype.forEach.call(fsel.files, sendFile);\n      fsel.value = "";\n    };\n\n    var bgc = document.getElementById("bgc");\n    var fgc = document.getElementById("fgc");\n    bgc.oninput = function () {\n      document.documentElement.style.setProperty("--bg", bgc.value);\n      localStorage.setItem("chathub_bg", bgc.value);\n    };\n    fgc.oninput = function () {\n      document.documentElement.style.setProperty("--fg", fgc.value);\n      localStorage.setItem("chathub_fg", fgc.value);\n    };\n    var sb = localStorage.getItem("chathub_bg");\n    var sf = localStorage.getItem("chathub_fg");\n    if (sb) { bgc.value = sb; document.documentElement.style.setProperty("--bg", sb); }\n    if (sf) { fgc.value = sf; document.documentElement.style.setProperty("--fg", sf); }\n\n    var dragDepth = 0;\n    window.addEventListener("dragenter", function (e) {\n      e.preventDefault(); dragDepth++; dropEl.style.display = "flex";\n    });\n    window.addEventListener("dragover", function (e) { e.preventDefault(); });\n    window.addEventListener("dragleave", function (e) {\n      e.preventDefault(); dragDepth--;\n      if (dragDepth <= 0) { dragDepth = 0; dropEl.style.display = "none"; }\n    });\n    window.addEventListener("drop", function (e) {\n      e.preventDefault(); dragDepth = 0; dropEl.style.display = "none";\n      if (e.dataTransfer && e.dataTransfer.files) {\n        Array.prototype.forEach.call(e.dataTransfer.files, sendFile);\n      }\n    });\n\n    window.addEventListener("beforeunload", function () {\n      navigator.sendBeacon("/leave", JSON.stringify({ user: ME }));\n    });\n  }\n\n  function boot() {\n    var nick = prompt("Il tuo nickname:", localStorage.getItem("chathub_nick") || "");\n    if (nick === null) { document.body.innerHTML = "<p style=\'padding:20px\'>Chat annullata.</p>"; return; }\n    nick = (nick || "").trim().slice(0, 24) || ("Utente" + Math.floor(Math.random() * 900 + 100));\n    localStorage.setItem("chathub_nick", nick);\n\n    api("/join", { user: nick }).then(function (res) {\n      ME = res.user || nick;\n      document.title = "Chat Hub - " + ME;\n      buildEmoji();\n      bind();\n      poll();\n    }).catch(function () {\n      document.body.innerHTML = "<p style=\'padding:20px\'>Server non raggiungibile.</p>";\n    });\n  }\n\n  if (document.readyState === "loading") {\n    document.addEventListener("DOMContentLoaded", boot);\n  } else {\n    boot();\n  }\n})();\n'



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
