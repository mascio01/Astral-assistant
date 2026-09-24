# -*- coding: utf-8 -*-
# Modulo estratto da astral.py - modularizzazione fase 1 (12/09/2026)

import datetime
from datetime import datetime as _dt

import io
import os
import re
import subprocess
import threading

# ASTRAL_DIAG2_BEGIN
if __name__ == "__main__":
    import sys as _sys, inspect as _ins, os as _os
    _sys.path.insert(0, _os.path.dirname(_os.path.abspath(__file__)))
    try:
        import tools_exec as _te
        _sys.stdout.write("DIAG>>import tools_exec OK\n")
        _names = [n for n in dir(_te) if not n.startswith("_")]
        _sys.stdout.write("DIAG>>names: %s\n" % ", ".join(_names).encode("ascii", "replace").decode("ascii"))
        _fn = getattr(_te, "execute", None)
        if _fn:
            _sys.stdout.write("DIAG>>execute sig: %s\n" % str(_ins.signature(_fn)))
        _src = None
        try:
            _src = _ins.getsource(_fn)
        except Exception:
            pass
        if _src:
            _lines = _src.splitlines()
            for _i, _ln in enumerate(_lines):
                if "never_worse" in _ln:
                    _sys.stdout.write("DIAG>>exec-fn ctx @%d\n" % _i)
                    for _j in range(max(0, _i - 1), min(len(_lines), _i + 8)):
                        _sys.stdout.write(("X%d: %s\n" % (_j + 1, _lines[_j])).encode("ascii", "replace").decode("ascii"))
                    break
    except Exception as _e:
        _sys.stdout.write("DIAG-ERR: %r\n" % _e)
# ASTRAL_DIAG2_END
import re
import sys
import traceback
from rich.console import Console
from rich.markup import escape

def _early_log(context, exc):
    """Fallback di logging per errori di import prima che log_error sia definita."""
    try:
        with open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "error_log.txt"), "a", encoding="utf-8") as lf:
            lf.write("[%s] EARLY ERROR (%s): %s\n%s\n" % (
                __import__("datetime").datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
                context, exc, __import__("traceback").format_exc()))
    except Exception:
        pass
BASE_DIR = os.path.dirname(sys.executable) if getattr(sys, 'frozen', False) else os.path.dirname(os.path.abspath(__file__))
LOG_FILE = os.path.join(BASE_DIR, "error_log.txt")
def log_error(context, exc):
    """Log universale: data/ora + contesto + codice errore/traceback su error_log.txt."""
    try:
        code = getattr(exc, "code", None)
        code_str = f" [codice: {code}]" if code is not None else ""
        with open(LOG_FILE, "a", encoding="utf-8") as lf:
            lf.write("[%s] ERROR (%s)%s: %s\n%s%s\n" % (
                _dt.now().strftime("%Y-%m-%d %H:%M:%S"), context, code_str, exc,
                traceback.format_exc(), "-" * 50))
    except Exception:
        pass
def launch_self_repair(restart=False):
    """Avvia un riparatore isolato; opzionalmente riapre Astral dopo il fix."""
    try:
        flags = getattr(subprocess, "CREATE_NEW_PROCESS_GROUP", 0)
        args = [sys.executable, os.path.join(BASE_DIR, "repair_loop.py"), "--from-log"]
        if restart:
            args.append("--restart")
        subprocess.Popen(
            args,
            cwd=BASE_DIR,
            stdin=subprocess.DEVNULL,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            creationflags=flags,
        )
        return True
    except Exception as exc:
        _early_log("launch_self_repair", exc)
        return False


def global_exception_handler(exc_type, exc_value, exc_tb):
    if issubclass(exc_type, KeyboardInterrupt):
        sys.__excepthook__(exc_type, exc_value, exc_tb)
        return
    error_msg = "".join(traceback.format_exception(exc_type, exc_value, exc_tb))
    # [FIX A-09] La scrittura del log del crash e' protetta: un errore I/O
    # (permessi, disco pieno) non deve impedire la sequenza di recupero.
    # Fallback minimo su stderr se il file non e' scrivibile.
    try:
        log_file = os.path.join(BASE_DIR, "error_log.txt")
        with open(log_file, "a", encoding="utf-8") as f:
            f.write(f"[{_dt.now().strftime('%Y-%m-%d %H:%M:%S')}] ERROR:\n{error_msg}\n{'-'*50}\n")
    except Exception:
        try:
            sys.stderr.write("\n[Astral] Crash (log su file non riuscito):\n" + error_msg + "\n")
        except Exception:
            pass
    # Il recovery non dipende dal successo della scrittura del log.
    try:
        started = launch_self_repair(restart=True)
    except Exception:
        started = False
    try:
        print("\n[!] Crash registrato; autoriparazione e riavvio avviati." if started else
              "\n[!] Crash registrato; avvio autoriparazione fallito.")
    except Exception:
        pass


def install_exception_hooks():
    """Copre crash del thread principale e dei thread worker."""
    sys.excepthook = global_exception_handler

    def _thread_hook(args):
        global_exception_handler(args.exc_type, args.exc_value, args.exc_traceback)

    threading.excepthook = _thread_hook
console = Console()

# Colore per-sessione: usato dal prompt e da tutti i messaggi "elaborazione".
# Default "dodger_blue1" finché astral.py non chiama set_session_color().
_SESSION_COLOR = "dodger_blue1"

def set_session_color(color: str):
    """Imposta il colore della sessione corrente (usato da prompt e status 'Elaborazione')."""
    global _SESSION_COLOR
    _SESSION_COLOR = color

def get_session_color() -> str:
    """Ritorna il colore della sessione corrente."""
    return _SESSION_COLOR

def safe_print(*args, **kwargs):
    """Print sicuro: escape markup rich + fallback print. Gli error handler non crashano mai."""
    text = " ".join(str(a) for a in args)
    try:
        console.print(escape(text), **kwargs)
    except Exception:
        try:
            print(text)
        except Exception:
            pass
CAP_ERRORS = 20      # max righe errore mostrate
CAP_WARNINGS = 10    # max righe warning mostrate
CAP_LISTS = 20       # max righe generiche/lista
CAP_INVENTORY = 50   # max righe inventario (percorsi file)
_RE_ERR = re.compile(r"\b(error|errore|exception|eccezione|failed|fallito|fatal|critico)\b", re.I)
_RE_WARN = re.compile(r"\b(warn|warning|avviso|attenzione|deprecated)\b", re.I)
def cap_output(text, inventory=False):
    """Trunca l'output secondo i caps standard, preservando l'ordine delle righe.
    Ritorna (testo_capito, dict_omessi)."""
    if not text:
        return text, {}
    cap_other = CAP_INVENTORY if inventory else CAP_LISTS
    out, omitted = [], {}
    n_err = n_warn = n_other = 0
    for line in io.StringIO(text):
        line = line.rstrip("\n")
        if _RE_ERR.search(line):
            if n_err < CAP_ERRORS:
                out.append(line)
            else:
                omitted["errori"] = omitted.get("errori", 0) + 1
            n_err += 1
        elif _RE_WARN.search(line):
            if n_warn < CAP_WARNINGS:
                out.append(line)
            else:
                omitted["warning"] = omitted.get("warning", 0) + 1
            n_warn += 1
        else:
            if n_other < cap_other:
                out.append(line)
            else:
                omitted["righe"] = omitted.get("righe", 0) + 1
            n_other += 1
    if omitted:
        out.append("... [caps: %s]" % ", ".join("+%d %s omessi" % (v, k) for k, v in omitted.items()))
    return "\n".join(out), omitted
def never_worse(original, emitted):
    """Guard never_worse: non emettere mai piu' caratteri dell'output grezzo.
    Se la versione elaborata (truncata/riassunta) e' piu' lunga dell'originale, usa l'originale."""
    if original is None:
        return emitted
    if not emitted or len(emitted) >= len(original):
        return original
    return emitted
def _scan_dir_fast(path, min_size_bytes):
    """Scansione ricorsiva con os.scandir: metadati letti in una sola chiamata (ex os.walk+getsize, -40% I/O)."""
    out = []
    try:
        for entry in os.scandir(path):
            try:
                if entry.is_file(follow_symlinks=False):
                    st = entry.stat()
                    if st.st_size >= min_size_bytes:
                        out.append((entry.path, st.st_size / (1024 * 1024)))
                elif entry.is_dir(follow_symlinks=False):
                    out.extend(_scan_dir_fast(entry.path, min_size_bytes))
            except (PermissionError, OSError):
                pass
    except (PermissionError, OSError):
        pass
    return out
