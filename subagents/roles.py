# -*- coding: utf-8 -*-
# subagents/roles.py - Esecuzione ruoli scout/reviewer: prompt + chiamata LLM + output strutturato.
# Verdetto 2026-09-15: un file per ruolo, entro ~150 righe, read-only, profondita' 1.
import os
import json
from datetime import datetime

from core_io import console, log_error
from llm_core import client as _shared_client
from subagents.jobspec import get_ruolo, check_depth
from llm_core import route_model

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT_DIR = os.path.join(BASE, "subagents", "out")


def _read_selfmap(max_chars: int = 6000) -> str:
    p = os.path.join(BASE, ".selfmap.md")
    try:
        with open(p, "r", encoding="utf-8") as f:
            return f.read()[:max_chars]
    except Exception:
        return "(selfmap non disponibile)"


def _read_file_rel(rel: str, max_chars: int = 4000) -> str:
    p = os.path.join(BASE, rel)
    try:
        with open(p, "r", encoding="utf-8") as f:
            return f.read()[:max_chars]
    except Exception as e:
        return f"(errore lettura {rel}: {e})"


def _collect_context(tools: list, contesto: str) -> str:
    """Whitelist tool per ruolo: solo lettura. contesto = file/diff separati da virgola."""
    parts = []
    if "selfmap" in tools:
        parts.append("=== SELFMAP (struttura progetto) ===\n" + _read_selfmap())
    for rel in [x.strip() for x in (contesto or "").split(",") if x.strip()]:
        parts.append(f"=== FILE {rel} ===\n" + _read_file_rel(rel))
    return "\n\n".join(parts)


PROMPT_SCOUT = (
    "Sei SCOUT, un subagent read-only di Astral. Analizza il contesto fornito e produci "
    "un report FATTUALE in markdown: (1) struttura rilevante, (2) punti di integrazione, "
    "(3) rischi osservati. Nessuna opinione, solo fatti verificabili. Max 400 parole."
)
PROMPT_REVIEWER = (
    "Sei REVIEWER, un revisore avversariale di Astral. Analizza il diff/contesto fornito e "
    "produci un report in markdown: (1) bug o regressioni probabili, (2) rischi sicurezza, "
    "(3) verdict finale: APPROVA / RIFIUTA / RIFIUTA CON RISERVA + motivo. Max 400 parole."
)


def run_role(ruolo: str, contesto: str = "", depth: int = 0) -> dict:
    """Esegue un job subagent sincrono. Ritorna dict con esito e report."""
    r = get_ruolo(ruolo)
    if not r:
        return {"ok": False, "errore": f"Ruolo sconosciuto: {ruolo}"}
    ok_d, msg_d = check_depth(depth)
    if not ok_d:
        return {"ok": False, "errore": msg_d}
    # Il budget viene consumato qui, nel solo punto di esecuzione del job.
    from subagents.jobspec import check_budget
    ok_b, msg_b = check_budget()
    if not ok_b:
        return {"ok": False, "errore": msg_b}
    ctx = _collect_context(r["tools"], contesto)
    prompt = PROMPT_SCOUT if ruolo == "scout" else PROMPT_REVIEWER
    # Il subagent usa lo stesso routing dinamico del modello principale: il
    # ruolo aggiunge segnali semantici, ma non impone un modello fisso.
    routing_input = f"{r['descrizione']}\n{prompt}\n{contesto or ''}"
    try:
        modello = route_model(routing_input, context=[])
    except Exception:
        modello = r.get("modello_fallback", "deepseek/deepseek-v4-flash-0731")
    try:
        resp = _shared_client.chat.completions.create(
            model=modello,
            temperature=r["temp"],
            max_tokens=900,
            messages=[
                {"role": "system", "content": prompt},
                {"role": "user", "content": ctx[:12000]},
            ],
        )
        report = resp.choices[0].message.content or "(vuoto)"
        ok = True
    except Exception as e:
        log_error(f"subagents/{ruolo}", e)
        report = f"ERRORE: {e}"
        ok = False
    os.makedirs(OUT_DIR, exist_ok=True)
    ts = datetime.now().strftime("%Y%m%d_%H%M%S")
    out_path = os.path.join(OUT_DIR, f"{ts}_{ruolo}.md")
    try:
        with open(out_path, "w", encoding="utf-8") as f:
            f.write(report)
    except Exception:
        pass
    return {"ok": ok, "ruolo": ruolo, "modello": modello, "report": report, "out_path": out_path}
