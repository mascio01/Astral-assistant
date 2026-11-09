# -*- coding: utf-8 -*-
"""ui/peer_window.py - Finestra figlia di chat peer-to-peer per Astral.

Perche' un processo separato invece di un widget nella console:
il bridge LAN vive nel processo Astral, ma i messaggi arrivano su disco e li
vede solo chi chiama un tool dal REPL principale, che e' bloccato sull'input.
Questa finestra e' un figlio detached che resta aperta accanto alla console e
parla con i peer IN PARALLELO alla chat normale di Astral.

Scelte progettuali:
1) NON avvia mai il server HTTP/UDP (lo fa il padre all'avvio): usa solo le
   funzioni client di peer_bridge. Un secondo server creerebbe un nodo doppio.
2) NON consuma i messaggi (nessun move in inbox/read): legge sia inbox sia
   inbox/read e tiene un set locale dei gia' visti, cosi' non ruba messaggi
   ai tool del REPL e viceversa.
3) Polling con root.after ogni POLL_MS ms; l'invio gira in un thread e torna
   sulla UI via queue, per non bloccare Tk sulle chiamate HTTP.
4) Vita indipendente dal padre: chiuso il terminale la finestra resta finche'
   l'utente non la chiude con X o Esc.
"""

import json
import os
import queue
import subprocess
import sys
import threading

PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
if PROJECT_ROOT not in sys.path:
    sys.path.insert(0, PROJECT_ROOT)

from tools import peer_bridge as pb   # noqa: E402

POLL_MS = 1500
PID_FILE = os.path.join(pb.BASE, "window.pid")
MAX_SEEN_KEEP = 2000


def _inbox_dirs():
    """Cartelle da cui leggere la cronologia locale del nodo."""
    cfg = pb.load_config()
    base = os.path.join(pb.BASE, "inbox", cfg["node_id"])
    return [base, os.path.join(base, "read")]


def _read_messages():
    """Legge i messaggi locali ordinati cronologicamente senza consumarli."""
    out = []
    for d in _inbox_dirs():
        if not os.path.isdir(d):
            continue
        tag = "read" if d.endswith("read") else "new"
        for fn in sorted(os.listdir(d)):
            fp = os.path.join(d, fn)
            if not fn.endswith(".json") or not os.path.isfile(fp):
                continue
            try:
                with open(fp, "r", encoding="utf-8") as f:
                    m = json.load(f)
            except Exception:
                continue
            m["_file"] = fn
            m["_src"] = tag
            out.append(m)
    out.sort(key=lambda m: ((m.get("ts") or ""), m["_src"], m["_file"]))
    return out


def _pending_count():
    cfg = pb.load_config()
    d = os.path.join(pb.BASE, "inbox", cfg["node_id"])
    try:
        return len([f for f in os.listdir(d) if f.endswith(".json")])
    except Exception:
        return 0


def _pid_alive(pid):
    try:
        out = subprocess.run(["tasklist", "/FI", "PID eq %d" % int(pid)],
                             capture_output=True, text=True,
                             creationflags=getattr(subprocess, "CREATE_NO_WINDOW", 0))
        return str(int(pid)) in (out.stdout or "")
    except Exception:
        return False


def running_pid():
    """PID della finestra gia' aperta, oppure None."""
    try:
        with open(PID_FILE, "r", encoding="utf-8") as f:
            pid = int((f.read() or "0").strip())
    except Exception:
        return None
    return pid if pid and _pid_alive(pid) else None


def open_window():
    """Lancia la finestra come processo detached (pythonw). Idempotente."""
    pid = running_pid()
    if pid:
        return {"ok": True, "already_open": True, "pid": pid}
    exe = sys.executable
    pyw = os.path.join(os.path.dirname(exe), "pythonw.exe")
    if os.path.isfile(pyw):
        exe = pyw
    try:
        proc = subprocess.Popen(
            [exe, os.path.abspath(__file__)],
            cwd=PROJECT_ROOT,
            creationflags=getattr(subprocess, "CREATE_NEW_PROCESS_GROUP", 0)
            | getattr(subprocess, "DETACHED_PROCESS", 0),
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )
    except Exception as exc:
        return {"error": "%s: %s" % (type(exc).__name__, exc)}
    try:
        os.makedirs(pb.BASE, exist_ok=True)
        with open(PID_FILE, "w", encoding="utf-8") as f:
            f.write(str(proc.pid))
    except Exception:
        pass
    return {"ok": True, "pid": proc.pid}


# ------------------------------------------------------------------ GUI
class PeerWindow(object):
    """Chat peer con polling automatico e invio non bloccante."""

    def __init__(self):
        import tkinter as tk
        from tkinter import ttk
        from tkinter.scrolledtext import ScrolledText

        self.tk = tk
        self.root = tk.Tk()
        cfg = pb.load_config()
        self.node_id_local = cfg["node_id"]
        self.seen = set()
        self.results = queue.Queue()

        self.root.title("Astral Peer - %s" % (cfg.get("display_name") or self.node_id_local))
        self.root.geometry("780x560")
        self.root.minsize(520, 360)
        self.root.protocol("WM_DELETE_WINDOW", self.close)
        self.root.bind("<Escape>", lambda _e: self.close())

        top = tk.Frame(self.root)
        top.pack(fill="x", padx=8, pady=(8, 4))
        tk.Label(top, text="Nodo:").pack(side="left")
        tk.Label(top, text=self.node_id_local,
                 font=("Segoe UI", 9, "bold")).pack(side="left", padx=(4, 14))
        tk.Label(top, text="Peer:").pack(side="left")
        self.peer_var = tk.StringVar()
        self.peer_box = ttk.Combobox(top, textvariable=self.peer_var,
                                     width=26, state="readonly")
        self.peer_box.pack(side="left", padx=4)
        tk.Button(top, text="Aggiorna", command=self.refresh_peers).pack(side="left", padx=4)
        self.status_var = tk.StringVar(value="avvio...")
        tk.Label(top, textvariable=self.status_var, fg="#555").pack(side="right")

        body = tk.Frame(self.root)
        body.pack(fill="both", expand=True, padx=8)
        self.chat = ScrolledText(body, wrap="word", state="disabled",
                                 font=("Consolas", 10), height=20)
        self.chat.pack(fill="both", expand=True)
        self.chat.tag_config("in", foreground="#0b5cad")
        self.chat.tag_config("out", foreground="#1a7f37")
        self.chat.tag_config("sys", foreground="#8a6d3b")

        bottom = tk.Frame(self.root)
        bottom.pack(fill="x", padx=8, pady=8)
        self.entry = tk.Entry(bottom, font=("Segoe UI", 10))
        self.entry.pack(side="left", fill="x", expand=True)
        self.entry.bind("<Return>", lambda _e: self.send())
        tk.Button(bottom, text="Invia", command=self.send).pack(side="left", padx=(6, 0))
        tk.Button(bottom, text="Allega...", command=self.attach).pack(side="left", padx=(6, 0))

    # -- helpers UI
    def log(self, text, tag="sys"):
        self.chat.configure(state="normal")
        self.chat.insert("end", text + "\n", tag)
        self.chat.see("end")
        self.chat.configure(state="disabled")

    def refresh_peers(self):
        peers = pb.list_peers() or []
        labels = ["%s @ %s:%s" % (p.get("node_id"), p.get("host") or "?", p.get("port"))
                  for p in peers]
        current = self.peer_var.get()
        self.peer_box["values"] = labels
        if labels and current not in labels:
            if len(labels) == 1 or not current:
                current = labels[0]
            else:
                current = labels[0]
            self.peer_var.set(current)

    def target_node(self):
        label = (self.peer_var.get() or "").strip()
        return label.split(" @ ")[0] if label else None

    # -- invio (thread -> queue -> UI)
    def send(self):
        text = (self.entry.get() or "").strip()
        if not text:
            return
        node = self.target_node()
        if not node:
            self.log("[!] nessun peer selezionato: premi Aggiorna o registra un peer.", "sys")
            return
        kind = "task" if text.startswith("/task ") else "chat"
        if kind == "task":
            text = text[len("/task "):].strip()
            if not text:
                return
        self.entry.delete(0, "end")
        self.log("[%s] -> %s: %s" % (self._now(), node, text), "out")
        threading.Thread(target=self._send_worker, args=(node, text, kind),
                         daemon=True).start()

    def _send_worker(self, node, text, kind):
        try:
            res = pb.send_message(text, to=node, kind=kind)
        except Exception as exc:
            res = {"error": "%s: %s" % (type(exc).__name__, exc)}
        self.results.put(("send", res))

    def attach(self):
        from tkinter import filedialog
        node = self.target_node()
        if not node:
            self.log("[!] nessun peer selezionato.", "sys")
            return
        path = filedialog.askopenfilename(title="File da inviare al peer")
        if not path:
            return
        self.log("[%s] file -> %s: %s" % (self._now(), node, os.path.basename(path)), "out")
        threading.Thread(target=self._file_worker, args=(node, path),
                         daemon=True).start()

    def _file_worker(self, node, path):
        try:
            res = pb.send_file(path, to=node)
        except Exception as exc:
            res = {"error": "%s: %s" % (type(exc).__name__, exc)}
        self.results.put(("file", res))

    @staticmethod
    def _now():
        import time as _t
        return _t.strftime("%H:%M:%S")

    # -- polling inbox
    def poll(self):
        try:
            for m in _read_messages():
                key = (m.get("_src"), m.get("_file"))
                if key in self.seen:
                    continue
                self.seen.add(key)
                sender = m.get("from") or "?"
                body = (m.get("text") or "").strip() or "(vuoto)"
                kind = m.get("kind") or "chat"
                tag = "in" if kind != "alert" else "sys"
                self.log("[%s] %s (%s): %s" % (m.get("ts", "?"), sender, kind, body), tag)
            if len(self.seen) > MAX_SEEN_KEEP:
                self.seen = set(list(self.seen)[-MAX_SEEN_KEEP:])
            pending = _pending_count()
            peers = len(pb.list_peers() or [])
            self.status_var.set("%d in coda \u00b7 %d peer \u00b7 nodo attivo" % (pending, peers))
        except Exception as exc:
            self.status_var.set("errore polling: %s" % exc)
        finally:
            self.root.after(POLL_MS, self.poll)

    def drain_results(self):
        try:
            while True:
                what, res = self.results.get_nowait()
                if isinstance(res, dict) and res.get("error"):
                    self.log("[!] invio fallito (%s): %s" % (what, res["error"]), "sys")
                else:
                    dest = (res or {}).get("sent_to", "?")
                    extra = (res or {}).get("filename", "")
                    self.log("[%s] consegnato a %s%s" % (self._now(), dest,
                                                          (" - " + extra) if extra else ""), "sys")
        except queue.Empty:
            pass
        finally:
            self.root.after(400, self.drain_results)

    def close(self):
        try:
            os.remove(PID_FILE)
        except Exception:
            pass
        try:
            self.root.destroy()
        except Exception:
            pass

    def run(self):
        self.refresh_peers()
        self.log("Finestra peer pronta. Nodo locale: %s. Esc per chiudere."
                 % self.node_id_local, "sys")
        self.poll()
        self.drain_results()
        self.root.mainloop()


def main():
    """Entry point del processo figlio: apre la finestra e resta in ascolto."""
    try:
        os.makedirs(pb.BASE, exist_ok=True)
        with open(PID_FILE, "w", encoding="utf-8") as f:
            f.write(str(os.getpid()))
    except Exception:
        pass
    PeerWindow().run()


if __name__ == "__main__":
    main()
