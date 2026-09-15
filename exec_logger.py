# -*- coding: utf-8 -*-
# exec_logger.py - n8n Pattern 3: log esecuzioni JSONL append-only
import os
import json
import time
import threading
from datetime import datetime

from core_io import BASE_DIR
from memory_meta import attach_event_from_exec

_LOG_DIR = os.path.join(BASE_DIR, "logs")
_LOG_PATH = os.path.join(_LOG_DIR, "executions.jsonl")
_lock = threading.Lock()
_line_no = None  # contatore righe scritte (1-based) per src_ref meta


def _ensure_line_no():
    """Conta una sola volta le righe esistenti per allineare src_ref al file."""
    global _line_no
    if _line_no is None:
        try:
            with open(_LOG_PATH, "r", encoding="utf-8") as f:
                _line_no = sum(1 for _ in f)
        except Exception:
            _line_no = 0


def log_execution(tool, args, error=None, duration_ms=None, extra=None):
    """Append di una entry JSONL. Non solleva mai eccezioni (fail-safe)."""
    try:
        safe_args = args if isinstance(args, dict) else {"raw": str(args)}
        try:
            if len(json.dumps(safe_args, default=str)) > 500:
                safe_args = {"_keys": list(safe_args.keys()), "_note": "args troncati"}
        except Exception:
            safe_args = {"_raw": str(args)[:200]}
        entry = {
            "ts": datetime.now().isoformat(timespec="seconds"),
            "tool": str(tool),
            "args": safe_args,
            "status": "error" if error else "ok",
            "error": str(error)[:500] if error else None,
            "duration_ms": round(duration_ms, 1) if duration_ms is not None else None,
        }
        if extra:
            entry.update(extra)
        os.makedirs(_LOG_DIR, exist_ok=True)
        line = json.dumps(entry, ensure_ascii=False, default=str)
        with _lock:
            _ensure_line_no()
            with open(_LOG_PATH, "a", encoding="utf-8") as f:
                f.write(line + "\n")
                _line_no += 1
            ln = _line_no
        try:
            attach_event_from_exec(str(tool), entry, ln)
        except Exception:
            pass
    except Exception:
        pass


# --- Pattern 4: circuit breaker persistente per il pool modelli -------------
_CB_FILE = os.path.join(BASE_DIR, "logs", "circuit_breaker.json")
CB_THRESHOLD = 3          # fallimenti consecutivi prima dell'apertura
CB_COOLDOWN_SEC = 900     # 15 min di pausa per un modello aperto
_cb_lock = threading.Lock()


def cb_open(model: str) -> bool:
    """Registra un fallimento consecutivo per `model`; apre il circuito oltre soglia.
    Ritorna True solo alla transizione chiuso->aperto (per dedup alert).
    Non solleva mai eccezioni (fail-safe, come log_execution)."""
    try:
        with _cb_lock:
            st = _cb_load()
            e = st.get(model, {"fails": 0, "opened_at": None})
            fails = int(e.get("fails", 0)) + 1
            opened = e.get("opened_at")
            had_open = bool(opened and _cb_still_open(opened))
            if fails >= CB_THRESHOLD and not had_open:
                opened = time.time()
                log_execution("circuit_breaker", {"model": model},
                              error=RuntimeError(f"CIRCUITO APERTO dopo {fails} fallimenti consecutivi"),
                              extra={"pattern": "P4"})
            st[model] = {"fails": fails, "opened_at": opened}
            _cb_save(st)
            return opened is not None and _cb_still_open(opened) and fails >= CB_THRESHOLD and not had_open
    except Exception:
        return False


def cb_available(model: str) -> bool:
    """True se il modello e' utilizzabile (circuito chiuso o cooldown scaduto)."""
    try:
        with _cb_lock:
            st = _cb_load()
            e = st.get(model)
            if not e:
                return True
            opened = e.get("opened_at")
            if not opened:
                return True
            if _cb_still_open(opened):
                return False
            # cooldown scaduto -> half-open: resetta e riaccetta il modello
            st[model] = {"fails": 0, "opened_at": None}
            _cb_save(st)
        return True
    except Exception:
        return True


def cb_close(model: str) -> None:
    """Reset fail-counter al primo successo (half-open -> closed)."""
    try:
        with _cb_lock:
            st = _cb_load()
            st[model] = {"fails": 0, "opened_at": None}
            _cb_save(st)
    except Exception:
        pass


def _cb_load() -> dict:
    try:
        with open(_CB_FILE, "r", encoding="utf-8") as f:
            d = json.load(f)
            return d if isinstance(d, dict) else {}
    except Exception:
        return {}


def _cb_save(st: dict) -> None:
    try:
        os.makedirs(os.path.dirname(_CB_FILE), exist_ok=True)
        with open(_CB_FILE, "w", encoding="utf-8") as f:
            json.dump(st, f)
    except Exception:
        pass


def _cb_still_open(opened) -> bool:
    """True se il circuito e' ancora nel periodo di cooldown."""
    try:
        return (time.time() - float(opened)) < CB_COOLDOWN_SEC
    except Exception:
        return False


def read_executions(limit=50, tool=None, status=None):
    """Ultime N esecuzioni (dalla piu' recente), con filtri opzionali."""
    out = []
    try:
        if not os.path.exists(_LOG_PATH):
            return out
        with open(_LOG_PATH, "r", encoding="utf-8") as f:
            lines = f.readlines()
        for line in reversed(lines):
            if len(out) >= limit:
                break
            try:
                e = json.loads(line)
            except Exception:
                continue
            if tool and e.get("tool") != tool:
                continue
            if status and e.get("status") != status:
                continue
            out.append(e)
    except Exception:
        pass
    return out
