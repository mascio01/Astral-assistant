# -*- coding: utf-8 -*-
# Modulo estratto da astral.py - modularizzazione fase 1 (12/09/2026)

import json
import os
import time
import uuid
from core_io import BASE_DIR, console, log_error
from memory_store import recall_save

# Ogni processo ha una cronologia privata: evita che due sessioni si
# sovrascrivano il rispettivo contesto mentre memoria/checkpoint restano condivisi.
SESSION_ID = os.environ.get("ASTRAL_SESSION_ID") or uuid.uuid4().hex[:12]
HISTORY_FILE = os.path.join(BASE_DIR, f"chat_history_{SESSION_ID}.json")
CONFIG_FILE = os.path.join(BASE_DIR, "config.json")
MAX_DAYS_HISTORY = 14
MAX_HISTORY_WINDOW_TURNS = 6
def get_history():
    """Carica la cronologia recente comprimendo i vecchi messaggi a max 400 caratteri per risparmiare token."""
    try:
        if not os.path.exists(HISTORY_FILE):
            return []
        with open(HISTORY_FILE, "r", encoding="utf-8") as f:
            data = json.load(f)
        now = time.time()
        valid = [e for e in data if (now - e.get("timestamp", now)) < (MAX_DAYS_HISTORY * 86400)]
        
        hist = []
        for e in valid[-MAX_HISTORY_WINDOW_TURNS:]:
            text = e.get("text", "")
            if len(text) > 1500:
                text = text[:1500] + " ...[troncato per risparmio token]"
            hist.append({"role": e.get("role", "user"), "content": text})
        return hist
    except Exception as e:
        log_error("load_history", e)
        return []
def save_persistent_history(messages):
    """Salva fino a 20 messaggi utente/assistente nello storico JSON."""
    try:
        now = time.time()
        to_save = []
        for m in messages:
            if isinstance(m, dict):
                role = m.get("role")
                content = m.get("content")
                ts = m.get("timestamp", now)
            else:
                role = getattr(m, "role", None)
                content = getattr(m, "content", None)
                ts = now
            if role in ["user", "assistant"] and content:
                to_save.append({"role": role, "text": content, "timestamp": ts})
        with open(HISTORY_FILE, "w", encoding="utf-8") as f:
            json.dump(to_save[-20:], f, indent=4)
    except Exception as e:
        log_error("save_persistent_history", e)
def clear_persistent_history():
    if os.path.exists(HISTORY_FILE):
        os.remove(HISTORY_FILE)
    console.print("[dim][*] Memoria azzerata.[/dim]")
TOOL_RESULT_OFFLOAD_CHARS = 2000   # soglia offload risultati tool in history
SUMMARY_CHECKPOINT_CHARS = 4000    # soglia riassunto turni vecchi
def maybe_offload_tool_result(content, tool_name=""):
    """Se il risultato tool e' grande, lo salva in recall e mette un placeholder
    con hash in history (ispirato a nanobot context_governance). Il modello puo'
    recuperare il contenuto completo con recall_get(h)."""
    try:
        # EVITA nidificazione: il tool recall riporterebbe dati gia' in recall
        if tool_name == "recall":
            return content
        if len(content) <= TOOL_RESULT_OFFLOAD_CHARS:
            return content
        h = recall_save(content)
        if not h:
            return content
        return (f"[Output completo ({len(content)} caratteri) salvato in recall store. "
                f"Hash: {h}. Usa recall_get('{h}') per recuperarlo se necessario. "
                f"Preview: {content[:300]}]")
    except Exception as e:
        log_error("maybe_offload_tool_result", e)
        return content
def compact_history_with_summary(messages):
    """Summary checkpoint (ispirato a nanobot autocompact): invece di buttare i turni
    vecchi, genera un checkpoint testuale che resta nel contesto."""
    try:
        user_asst = [m for m in messages if isinstance(m, dict) and m.get("role") in ("user", "assistant")]
        if len(user_asst) <= MAX_HISTORY_WINDOW_TURNS:
            return messages
        old = user_asst[:-MAX_HISTORY_WINDOW_TURNS]
        parts = []
        for m in old:
            role = "U" if m.get("role") == "user" else "A"
            txt = (m.get("content") or "").strip()
            if txt:
                parts.append(f"{role}: {txt[:200]}")
        if not parts:
            return prune_messages(messages, max_turns=MAX_HISTORY_WINDOW_TURNS)
        checkpoint = "[CHECKPOINT CONTESTO - turni precedenti compressi]\n" + "\n".join(parts[-8:])
        sys_msgs = [m for m in messages if isinstance(m, dict) and m.get("role") == "system"]
        recent = user_asst[-MAX_HISTORY_WINDOW_TURNS:]
        return sys_msgs + [{"role": "user", "content": checkpoint}] + recent
    except Exception as e:
        log_error("compact_history_with_summary", e)
        return prune_messages(messages, max_turns=MAX_HISTORY_WINDOW_TURNS)
def prune_messages(messages, max_turns=6):
    """Mantiene la sliding window a max_turns scambi. O(N) singolo passaggio + slicing (ex O(N^2))."""
    idxs = [i for i, m in enumerate(messages) if isinstance(m, dict) and m.get("role") in ("user", "assistant")]
    if len(idxs) <= max_turns:
        return messages
    cut_idx = idxs[-max_turns]
    messages = messages[cut_idx:]
    # Rimuovi eventuali 'tool' orfani all'inizio
    start = 0
    while start < len(messages) and isinstance(messages[start], dict) and messages[start].get("role") == "tool":
        start += 1
    return messages[start:]
