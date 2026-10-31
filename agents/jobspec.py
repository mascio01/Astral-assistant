# -*- coding: utf-8 -*-
# subagents/jobspec.py - Specifica job subagent: budget, profondita', permessi per ruolo.
# Verdetto 2026-09-15: budget max 8 job/run, profondita' max 1 (i child non generano child).
import os
import json
import time
import tempfile

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
STATE_PATH = os.path.join(BASE, ".subagents_state.json")
_LOCK_PATH = STATE_PATH + ".lock"

MAX_JOBS_PER_RUN = 8
MAX_DEPTH = 1

# Permessi per ruolo (whitelist tool). scout = read-only, reviewer = read-only sul diff.
RUOLI = {
    "scout": {
        "descrizione": "Esplora il codebase (selfmap + file) e riporta fatti strutturali.",
        "tools": ["read_file", "selfmap", "search"],
        # Fallback solo se il router dinamico non e' disponibile.
        "modello_fallback": "deepseek/deepseek-v4-flash-vision-exp",
        "temp": 0.3,
    },
    "reviewer": {
        "descrizione": "Revisione avversariale di un diff/patch: rischi, bug, regressioni.",
        "tools": ["read_file", "search"],
        # Fallback solo se il router dinamico non e' disponibile.
        "modello_fallback": "deepseek/deepseek-v4.1-flash",
        "temp": 0.4,
    },
}


def _load_state():
    try:
        with open(STATE_PATH, "r", encoding="utf-8") as f:
            return json.load(f)
    except Exception:
        return {"jobs_started": 0, "window_start": time.time()}


def _save_state(st):
    # [FIX A-05] scrittura atomica: file temporaneo + os.replace.
    # Evita stato JSON corrotto se il processo muore a meta' scrittura.
    try:
        d = os.path.dirname(STATE_PATH)
        fd, tmp = tempfile.mkstemp(dir=d, prefix=".subagents_state_", suffix=".tmp")
        try:
            with os.fdopen(fd, "w", encoding="utf-8") as f:
                json.dump(st, f)
            os.replace(tmp, STATE_PATH)
        except Exception:
            try:
                os.unlink(tmp)
            except Exception:
                pass
            raise
    except Exception:
        pass


class _FileLock:
    """[FIX A-05] Lock interprocesso basato su file (O_CREAT|O_EXCL).
    Serializza lettura-incremento-scrittura del contatore budget."""

    def __init__(self, path: str, timeout: float = 5.0):
        self.path = path
        self.timeout = timeout
        self._fd = None

    def __enter__(self):
        deadline = time.time() + self.timeout
        while True:
            try:
                self._fd = os.open(self.path, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
                return self
            except FileExistsError:
                if time.time() > deadline:
                    # lock stantio: rimuovi e ritenta una volta
                    try:
                        os.unlink(self.path)
                    except Exception:
                        pass
                    deadline = time.time() + self.timeout
                time.sleep(0.05)

    def __exit__(self, *exc):
        if self._fd is not None:
            try:
                os.close(self._fd)
            except Exception:
                pass
        try:
            os.unlink(self.path)
        except Exception:
            pass
        return False


def check_budget() -> tuple:
    """Ritorna (consentito, motivo). [FIX A-05] lettura-incremento-scrittura
    protetta da lock interprocesso + scrittura atomica: nessuna race che
    superi MAX_JOBS_PER_RUN."""
    with _FileLock(_LOCK_PATH):
        st = _load_state()
        # finestra rolling di 1 ora
        if time.time() - st.get("window_start", 0) > 3600:
            st = {"jobs_started": 0, "window_start": time.time()}
        if st["jobs_started"] >= MAX_JOBS_PER_RUN:
            return False, f"Budget esaurito: {st['jobs_started']}/{MAX_JOBS_PER_RUN} job nell'ultima ora."
        st["jobs_started"] += 1
        _save_state(st)
        return True, f"Job {st['jobs_started']}/{MAX_JOBS_PER_RUN} autorizzato."


def check_depth(depth: int) -> tuple:
    if depth >= MAX_DEPTH:
        return False, f"Profondita' {depth} >= MAX_DEPTH ({MAX_DEPTH}): i subagent non generano subagent."
    return True, "OK"


def get_ruolo(nome: str):
    return RUOLI.get((nome or "").strip().lower())
