# -*- coding: utf-8 -*-
# subagents/watchdog.py - Watchdog leggero: hash pre/post su file critici dopo job subagent.
# Verdetto 2026-09-15: hash su astral.py e tools_gateway.py, nessun demone pesante.
import os
import hashlib

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CRITICI = ["astral.py", "tools_gateway.py", "verdict_runner.py"]
HASH_PATH = os.path.join(BASE, ".subagents_hashes.json")


def _hash_file(p: str) -> str:
    h = hashlib.sha256()
    with open(p, "rb") as f:
        for chunk in iter(lambda: f.read(65536), b""):
            h.update(chunk)
    return h.hexdigest()


def snapshot() -> dict:
    """Salva hash correnti dei file critici (chiamare PRIMA dei job)."""
    import json
    st = {}
    for name in CRITICI:
        p = os.path.join(BASE, name)
        if os.path.isfile(p):
            st[name] = _hash_file(p)
    try:
        with open(HASH_PATH, "w", encoding="utf-8") as f:
            json.dump(st, f)
    except Exception:
        pass
    return st


def verify() -> list:
    """Confronta hash correnti con snapshot. Ritorna lista di file modificati."""
    import json
    try:
        with open(HASH_PATH, "r", encoding="utf-8") as f:
            prev = json.load(f)
    except Exception:
        return []
    changed = []
    for name, old in prev.items():
        p = os.path.join(BASE, name)
        if not os.path.isfile(p):
            changed.append(f"{name} (RIMOSSO)")
            continue
        if _hash_file(p) != old:
            changed.append(name)
    return changed
