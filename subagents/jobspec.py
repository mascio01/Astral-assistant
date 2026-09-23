# -*- coding: utf-8 -*-
# subagents/jobspec.py - Specifica job subagent: budget, profondita', permessi per ruolo.
# Verdetto 2026-09-15: budget max 8 job/run, profondita' max 1 (i child non generano child).
import os
import json
import time

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
STATE_PATH = os.path.join(BASE, ".subagents_state.json")

MAX_JOBS_PER_RUN = 8
MAX_DEPTH = 1

# Permessi per ruolo (whitelist tool). scout = read-only, reviewer = read-only sul diff.
RUOLI = {
    "scout": {
        "descrizione": "Esplora il codebase (selfmap + file) e riporta fatti strutturali.",
        "tools": ["read_file", "selfmap", "search"],
        # Fallback solo se il router dinamico non e' disponibile.
        "modello_fallback": "deepseek/deepseek-v4-flash-0731",
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
    try:
        with open(STATE_PATH, "w", encoding="utf-8") as f:
            json.dump(st, f)
    except Exception:
        pass


def check_budget() -> tuple:
    """Ritorna (consentito, motivo). Contatore atomico su file (thread-safe via rename implicito)."""
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
