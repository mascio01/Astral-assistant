# -*- coding: utf-8 -*-
# routing_engine.py - Routing dinamico pesato dei modelli.
# Dati REALI (dal 2026-09-14): qualita' da LiveBench (file statici livebench.ai)
# intrecciata con OpenRouter (prezzi/contesti: FONTE UNICA per il costo);
# parafrasi delle categorie in 3 gruppi in benchmark_data.
# Confidenza del classificatore = GATE/modulatore (mai componente additiva):
#   conf < 0.55          -> fallback conservativo (si resta sul modello corrente)
#   0.55 <= conf < 0.75  -> switch solo con margine dinamico + isteresi (2 conferme)
#   conf >= 0.75         -> switch immediato se lo score e' superiore
# Ogni decisione e' auditata in .routing_audit.jsonl (verdetto: telemetria estesa + audit).
# Pool dinamico: top-2 per gruppo tra i modelli con dati reali e prezzo sostenibile
# (benchmark_data.pool_suggerito); fallback su ROUTING_POOL legacy se offline.
# Il sync 24h (release LiveBench + catalogo OpenRouter) vive in benchmark_data.

import json
import math
import os
import re
import sqlite3
import threading
import time
from datetime import datetime

from core_io import log_error
from price_map import cost_usd, get_price

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
AUDIT_FILE = os.path.join(BASE_DIR, ".routing_audit.jsonl")

# Pesi: qualita' (60% categoria + 40% affidabilita) 0.7 + costo REALE OpenRouter 0.3.
PESI = {"qualita": 0.7, "costo": 0.3}
GATE_MIN = 0.55          # sotto: fallback conservativo
SWITCH_IMMEDIATO = 0.75  # sopra: switch immediato
MARGINE_MINIMO = 0.05    # margine base per cambiare modello (fascia media: dinamico)
STREAK_NECESSARI = 2     # isteresi nella fascia media
SHADOW_MODE = False      # True = calcola e logga ma NON applica (Fase 1 del verdetto)
# Il punteggio non e' una gara di utilizzo: la telemetria serve esclusivamente
# a misurare il costo reale a parita' di token nei tre orizzonti temporali.
PERSONAL_SCORES_FILE = os.path.join(BASE_DIR, ".routing_personal_scores.json")
TELEMETRY_DB = os.path.join(BASE_DIR, "telemetry.db")
COST_WINDOWS = {"breve": 7 * 86400, "medio": 30 * 86400, "lungo": 90 * 86400}
# Correzione personale: delta piccolo, appreso dagli esiti utili e non da un
# voto statico. Il benchmark resta dominante; il delta non supera +/-0.35.
PERSONAL_DELTA_CAP = 0.35
PERSONAL_DELTA_STEP = 0.08
PERSONAL_DELTA_MIN_STEP = 0.015
_PERSONAL_LOCK = threading.RLock()

# Pool operativo: LE STESSE IA di prima (specchio di llm_core.DYNAMIC_MODELS_POOL).
# _pool_dinamico() arricchisce con benchmark_data.pool_suggerito() SOLO le scelte
# per gruppo, ma il candidato e' sempre selezionato dentro questo stesso set.
ROUTING_POOL = [
    "deepseek/deepseek-v4-flash-0731",
    "z-ai/glm-5.3-flash",
    "openai/gpt-5.6-luna",
]

# Valori fittizi iniziali (scala 0-10; costo: 10 = piu' economico)
DEFAULT_BENCHMARK = {
    "deepseek/deepseek-v4-flash-0731": {"conversazione": 8.0, "codice": 7.0, "affidabilita": 8.5, "costo": 9.0},
    "z-ai/glm-5.3-flash": {"conversazione": 7.0, "codice": 8.5, "affidabilita": 8.0, "costo": 9.0},
    "openai/gpt-5.6-sol": {"conversazione": 9.5, "codice": 9.0, "affidabilita": 9.5, "costo": 5.0},
    "deepseek/deepseek-v4-pro": {"conversazione": 8.5, "codice": 8.0, "affidabilita": 8.0, "costo": 6.5},
    "z-ai/glm-5.3": {"conversazione": 8.0, "codice": 8.5, "affidabilita": 8.5, "costo": 7.0},
    "openai/gpt-5.6-luna": {"conversazione": 8.5, "codice": 8.0, "affidabilita": 8.5, "costo": 7.5},
}

_CODE_MARKERS = ["`", "def ", "class ", "import ", "function", "=>", "const ",
                 "var ", "let ", "public static", "#include"]
_CODICE_KW = [
    "codice", "script", "programma", "sviluppa", "funzione", "algoritmo",
    "python", "powershell", "bash", "cmd", "batch", ".bat", ".ps1", ".py", ".exe",
    "javascript", "typescript", "html", "css", "sql", "c#", "c++", "rust", "golang",
    "debug", "bug", "errore", "exception", "traceback", "syntax", "refactor",
    "rifattorizza", "compila", "build", "pyinstaller", "pip", "npm", "git", "regex",
    "json", "endpoint", "api rest", "query", "database", "terminale", "classe",
]
_CODE_TOOL_NAMES = {
    "apply_code_patch", "test_python_file", "tools_patch", "tools_test",
    "repair_from_log", "selfmap", "py_compile",
}

_state = {"previous": None, "streak": {}, "last": None, "context_signature": "", "context_epoch": 0}
_telemetry_enabled = True
# Il routing deve essere leggero: benchmark e telemetria cambiano lentamente, non
# vanno riletti da disco/SQLite a ogni singolo messaggio.
# Il primo messaggio non deve mai aspettare dati remoti: il benchmark reale
# viene idratato in background mentre la richiesta e' gia' in viaggio.
_BENCH_CACHE = None
_BENCH_CACHE_TS = 0.0
_BENCH_LOADING = False
_POOL_CACHE = list(ROUTING_POOL)
_POOL_CACHE_TS = 0.0
_TELEMETRY_COST_CACHE = None
_TELEMETRY_COST_CACHE_TS = 0.0
# Benchmark e pool cambiano con il sync giornaliero; solo la telemetria resta breve.
_BENCH_CACHE_TTL = 24 * 3600.0
_ROUTING_CACHE_TTL = 20.0


def get_personal_scores() -> dict:
    """Restituisce gli score personali senza esporre la persistenza al chiamante."""
    return _personal_scores()


def set_personal_score(model: str, categoria: str, score: float) -> bool:
    """Compatibilita' legacy: converte un voto 0-10 in un piccolo delta iniziale.

    Il routing normale non usa piu' il voto come qualita' assoluta: gli esiti
    osservati aggiornano gradualmente il delta tramite record_personal_outcome.
    """
    if not model or not categoria:
        return False
    try:
        value = max(0.0, min(10.0, float(score)))
        delta = max(-PERSONAL_DELTA_CAP, min(PERSONAL_DELTA_CAP,
                                             (value - 5.0) * 0.04))
        with _PERSONAL_LOCK:
            current = _personal_scores()
            entry = current.setdefault(str(model), {}).setdefault(str(categoria), {})
            if not isinstance(entry, dict):
                entry = {}
                current[str(model)][str(categoria)] = entry
            entry.update({"delta": round(delta, 4), "manual_seed": True,
                          "last_event": "manual_seed"})
            _save_personal_scores(current)
        return True
    except (OSError, TypeError, ValueError):
        return False


def _save_personal_scores(scores: dict) -> None:
    """Persistenza atomica del profilo personale."""
    tmp = PERSONAL_SCORES_FILE + ".tmp"
    with open(tmp, "w", encoding="utf-8") as f:
        json.dump(scores, f, ensure_ascii=False, indent=2)
    os.replace(tmp, PERSONAL_SCORES_FILE)


def get_telemetry_enabled() -> bool:
    return _telemetry_enabled


def set_telemetry_enabled(value) -> None:
    global _telemetry_enabled
    _telemetry_enabled = bool(value)


def save_telemetry(model_name, prompt_tokens, completion_tokens, total_tokens=None) -> bool:
    """Registra token e retention nel DB posseduto dal routing service."""
    try:
        prompt_tokens = int(prompt_tokens or 0)
        completion_tokens = int(completion_tokens or 0)
        total_tokens = int(total_tokens or (prompt_tokens + completion_tokens))
        conn = sqlite3.connect(TELEMETRY_DB, timeout=1)
        conn.execute("INSERT INTO telemetry VALUES (?,?,?,?,?)",
                     (model_name, time.time(), prompt_tokens, completion_tokens, total_tokens))
        conn.execute("DELETE FROM telemetry WHERE ts < ?", (time.time() - 90 * 86400,))
        conn.commit()
        conn.close()
        return True
    except (OSError, sqlite3.Error, TypeError, ValueError) as exc:
        log_error("routing_service/save_telemetry", exc)
        return False


def telemetry_stats(model_filter=None, days=30) -> str:
    """Restituisce statistiche aggregate, senza dipendere dal layer LLM."""
    try:
        cutoff = time.time() - int(days) * 86400
        conn = sqlite3.connect(TELEMETRY_DB, timeout=1)
        if model_filter:
            rows = conn.execute(
                "SELECT model_name, COUNT(*), SUM(prompt_tokens), SUM(completion_tokens), SUM(total_tokens) "
                "FROM telemetry WHERE ts > ? AND model_name = ? GROUP BY model_name "
                "ORDER BY SUM(total_tokens) DESC", (cutoff, model_filter)).fetchall()
        else:
            rows = conn.execute(
                "SELECT model_name, COUNT(*), SUM(prompt_tokens), SUM(completion_tokens), SUM(total_tokens) "
                "FROM telemetry WHERE ts > ? GROUP BY model_name ORDER BY SUM(total_tokens) DESC",
                (cutoff,)).fetchall()
        conn.close()
        if not rows:
            return "Nessun dato telemetria disponibile."
        lines = ["[Telemetry] Statistiche (ultimi %d giorni):" % int(days)]
        for model, count, p_in, p_out, p_total in rows:
            avg_total = p_total // count if count else 0
            lines.append(f"  [{model}] {count} chiamate | Input: {p_in} | Output: {p_out} | Totale: {p_total} token | Media/turno: {avg_total}")
        return "\n".join(lines)
    except (OSError, sqlite3.Error, TypeError, ValueError) as exc:
        log_error("routing_service/telemetry_stats", exc)
        return "Errore nel recupero statistiche: %s" % exc


_QUICK_HINTS = ("veloce", "rapido", "solo il risultato", "in breve", "una riga", "al volo")
_LONG_HINTS = ("in dettaglio", "passo passo", "analizza tutto", "approfondito", "completo", "molti file", "grande progetto")
_ACTION_HINTS = ("esegui", "modifica", "crea", "scrivi", "correggi", "installa", "lancia", "sposta", "cancella")
_REASONING_HINTS = ("confronta", "progetta", "pianifica", "architettura", "decidi", "valuta", "spiega perché")


def _tool_phase(context=None) -> str | None:
    """Riconosce il tipo di lavoro gia' iniziato dai tool, non solo dal prompt.

    Dopo il primo tool call il testo dell'utente resta invariato: senza questa
    informazione il router puo' riclassificare erroneamente una modifica al
    codice come semplice conversazione e mantenere DeepSeek per tutti i follow-up.
    """
    for message in context or []:
        if not isinstance(message, dict):
            continue
        for tool_call in message.get("tool_calls") or []:
            function = tool_call.get("function", {}) if isinstance(tool_call, dict) else {}
            name = function.get("name", "") if isinstance(function, dict) else ""
            if name in _CODE_TOOL_NAMES:
                return "codice"
        content = str(message.get("content", ""))
        if message.get("role") == "tool" and any(
            marker in content.lower() for marker in ("apply_code_patch", "test_python_file", "traceback", "syntaxerror")
        ):
            return "codice"
    return None


def _context_features(user_input: str, context=None) -> dict:
    """Estrae il profilo operativo della richiesta e dello storico, senza chiamare una IA."""
    text = user_input or ""
    lower = text.lower()
    history = context or []
    user_turns = [str(m.get("content", "")) for m in history
                  if isinstance(m, dict) and m.get("role") == "user"]
    previous = user_turns[-1] if user_turns else ""
    words = set(re.findall(r"[a-zàèéìòù0-9_]{4,}", lower))
    prev_words = set(re.findall(r"[a-zàèéìòù0-9_]{4,}", previous.lower()))
    overlap = len(words & prev_words) / max(1, len(words | prev_words))
    tool_phase = _tool_phase(history)
    code = classify_input(text)[0] == "codice" or tool_phase == "codice"
    return {
        "quick": any(h in lower for h in _QUICK_HINTS),
        "long": any(h in lower for h in _LONG_HINTS),
        "action": any(h in lower for h in _ACTION_HINTS),
        "reasoning": any(h in lower for h in _REASONING_HINTS),
        "code": code,
        "tool_phase": tool_phase,
        "input_chars": len(text),
        "history_turns": len(history),
        "history_chars": sum(len(str(m.get("content", ""))) for m in history if isinstance(m, dict)),
        "overlap": round(overlap, 3),
        "changed": bool(previous) and overlap < 0.04,
    }


def _profile_bonus(model: str, features: dict, categoria: str) -> float:
    """Modulatore operativo (scala score 0-10), volutamente piccolo rispetto ai benchmark."""
    bonus = 0.0
    is_deepseek = model == "deepseek/deepseek-v4-flash-0731"
    is_glm = model == "z-ai/glm-5.3-flash"
    is_luna = model == "openai/gpt-5.6-luna"
    if features["quick"]:
        bonus += 0.22 if (is_deepseek or is_glm) else -0.08
    if features["long"] or features["history_chars"] > 18000:
        bonus += 0.35 if is_luna else (0.12 if is_deepseek else -0.04)
    if features["code"]:
        bonus += 0.75 if is_glm else (0.18 if is_luna else -0.12)
    if features.get("tool_phase") == "codice":
        # Durante una modifica gia' avviata il cambio deve essere applicabile
        # subito: non lasciamo che l'isteresi mantenga il modello conversazionale.
        bonus += 0.55 if is_glm else (-0.10 if is_deepseek else 0.12)
    if features["reasoning"]:
        bonus += 0.30 if is_luna else 0.05
    if features["action"]:
        bonus += 0.12 if is_glm else 0.04
    if features["changed"]:
        bonus += 0.08 if (is_luna or is_glm) else 0.0
    # Continuità: evita di cambiare IA per una semplice prosecuzione del filo.
    if features["overlap"] >= 0.15 and model == _state.get("previous"):
        bonus += 0.22
    return bonus


def classify_input(text: str):
    """Ritorna (categoria, confidenza 0-1). 'codice' se rilevati marker/keyword;
    la confidenza cresce col numero di segnali indipendenti (piu' segnali = gate aperto)."""
    lower = (text or "").lower()
    n_kw = sum(1 for kw in _CODICE_KW if re.search(r"\b" + re.escape(kw) + r"\b", lower))
    n_mk = sum(1 for m in _CODE_MARKERS if m in (text or ""))
    if n_kw + n_mk == 0:
        return "conversazione", 0.85
    conf = min(0.95, 0.52 + 0.16 * n_kw + 0.10 * n_mk)
    return "codice", conf


def _hydrate_bench() -> None:
    """Carica benchmark e pool una volta al giorno, senza bloccare il REPL."""
    global _BENCH_CACHE, _BENCH_CACHE_TS, _BENCH_LOADING
    global _POOL_CACHE, _POOL_CACHE_TS
    try:
        from benchmark_data import get_models, pool_suggerito
        models = get_models()
        if models:
            _BENCH_CACHE = models
            _BENCH_CACHE_TS = time.monotonic()
            try:
                suggested = pool_suggerito()
                dynamic_pool = []
                for group in ("codice", "conversazione"):
                    for model in suggested.get(group, []):
                        if model not in dynamic_pool:
                            dynamic_pool.append(model)
                if dynamic_pool:
                    _POOL_CACHE = dynamic_pool
                _POOL_CACHE_TS = _BENCH_CACHE_TS
            except Exception as e:
                log_error("routing_engine/pool", e)
    except Exception as e:
        log_error("routing_engine/bench", e)
    finally:
        _BENCH_LOADING = False


def _bench() -> dict:
    """Benchmark immediato da cache/fallback; idratazione remota in background."""
    global _BENCH_CACHE, _BENCH_CACHE_TS, _BENCH_LOADING
    now = time.monotonic()
    if _BENCH_CACHE is None:
        _BENCH_CACHE = dict(DEFAULT_BENCHMARK)
    if not _BENCH_LOADING and now - _BENCH_CACHE_TS >= _BENCH_CACHE_TTL:
        _BENCH_LOADING = True
        threading.Thread(target=_hydrate_bench, name="routing-bench", daemon=True).start()
    return _BENCH_CACHE


def _personal_scores() -> dict:
    """Legge il profilo appreso senza premiare la frequenza d'uso.

    Formato nuovo: model -> categoria -> {delta, evidenze, positive, negative,
    recoverable_errors}. I numeri del vecchio formato restano leggibili ma non
    vengono piu' trattati come un voto assoluto.
    """
    try:
        with open(PERSONAL_SCORES_FILE, encoding="utf-8") as f:
            data = json.load(f)
        return data if isinstance(data, dict) else {}
    except (OSError, ValueError, TypeError):
        return {}


def _save_outcome(model: str, categoria: str, signal: float, weight: float,
                  label: str) -> dict:
    """Aggiorna il delta con apprendimento a passo decrescente e cap rigido."""
    with _PERSONAL_LOCK:
        current = _personal_scores()
        model_data = current.setdefault(str(model), {})
        entry = model_data.setdefault(str(categoria), {})
        if not isinstance(entry, dict):
            entry = {"delta": 0.0, "legacy_value": entry}
            model_data[str(categoria)] = entry
        evidence = max(0.0, min(1.0, float(weight)))
        attempts = int(entry.get("evidenze", 0) or 0)
        # Passo piu' grande all'inizio, poi piu' prudente: evita che un singolo
        # test o una singola risposta sposti il routing in modo sproporzionato.
        learning_rate = max(PERSONAL_DELTA_MIN_STEP,
                            PERSONAL_DELTA_STEP / math.sqrt(1.0 + attempts))
        change = learning_rate * max(-1.0, min(1.0, float(signal))) * evidence
        old_delta = float(entry.get("delta", 0.0) or 0.0)
        new_delta = max(-PERSONAL_DELTA_CAP,
                        min(PERSONAL_DELTA_CAP, old_delta + change))
        entry["delta"] = round(new_delta, 4)
        entry["evidenze"] = attempts + 1
        entry["positive"] = int(entry.get("positive", 0) or 0) + int(signal > 0.25)
        entry["negative"] = int(entry.get("negative", 0) or 0) + int(signal < -0.25)
        entry["recoverable_errors"] = int(entry.get("recoverable_errors", 0) or 0) + int(label == "recoverable_error")
        entry["last_event"] = label
        entry["last_change"] = round(change, 4)
        _save_personal_scores(current)
        return {"delta": round(new_delta, 4), "change": round(change, 4),
                "label": label, "evidenze": entry["evidenze"]}


def _classify_outcome(result) -> tuple:
    """Classifica un risultato tool: errori di test sono penalizzati, ma attenuati."""
    if not isinstance(result, dict):
        return 0.0, 0.0, "neutral"
    error = result.get("error")
    status = str(result.get("status", "")).lower()
    text = str(error or result.get("stderr") or result.get("traceback") or "").lower()
    test_error = (result.get("mode") in ("compile", "run") or any(
        word in text for word in (
            "test", "compile", "syntax", "traceback", "assert", "dry-run", "dry run")))
    failed = (bool(error) or status in ("fail", "error")
              or result.get("exit_code") not in (None, 0))
    if failed:
        # Un test fallito e' un segnale reale, ma attenuato: Astral e' progettata
        # per correggersi e un errore durante la riparazione non prova da solo che
        # il modello sia inaffidabile in produzione.
        if test_error:
            return -0.18, 0.35, "recoverable_error"
        return -1.0, 1.0, "failure"
    useful = (
        status in ("success", "ok")
        or result.get("exit_code") == 0
        or result.get("stdout") is not None
        or "found_items" in result
        or "message" in result
        or "blocks" in result
    )
    return (1.0, 1.0, "useful_success") if useful else (0.0, 0.0, "neutral")


def record_personal_outcome(model: str, categoria: str, evidence=None) -> dict:
    """Apprende dall'esito reale della risposta/task, non dalla sola frequenza.

    - successo di un tool/task utile: premio pieno;
    - errore operativo: penalita' piena;
    - errore di test/compilazione: penalita' lieve, perche' Astral puo' ripararsi;
    - risposta conversazionale senza feedback esplicito: neutra.
    """
    if not model or not categoria:
        return {}
    evidence = evidence or {}
    results = evidence.get("tool_results", []) if isinstance(evidence, dict) else []
    classified = [_classify_outcome(item) for item in results]
    classified = [item for item in classified if item[1] > 0]
    if not classified:
        return {}
    signal = sum(s * w for s, w, _ in classified) / sum(w for _, w, _ in classified)
    label = ("useful_success" if signal > 0.25 else
             "recoverable_error" if signal < 0 else "failure")
    weight = min(1.0, sum(w for _, w, _ in classified) / len(classified))
    return _save_outcome(model, categoria, signal, weight, label)


def learn_from_user_feedback(text: str, context=None) -> dict:
    """Usa la correzione spontanea dell'utente sull'ultima risposta come segnale.

    Le parole vengono considerate feedback solo se esiste gia' una risposta
    assistant: cosi' 'correggi questo errore' non penalizza preventivamente.
    """
    if not text or not _state.get("last"):
        return {}
    history = context or []
    if not any(isinstance(item, dict) and item.get("role") == "assistant" for item in history):
        return {}
    lower = text.lower()
    negative = ("hai sbagliato", "è sbagliato", "e' sbagliato", "non funziona",
                "non va", "ancora errore", "risposta errata", "sbagliata")
    positive = ("perfetto", "funziona", "risolto", "esatto", "ottimo", "bene, grazie")
    if any(marker in lower for marker in negative):
        return _save_outcome(_state["last"].get("scelto"),
                             _state["last"].get("categoria", "conversazione"),
                             -1.0, 0.85, "user_negative")
    if any(marker in lower for marker in positive):
        return _save_outcome(_state["last"].get("scelto"),
                             _state["last"].get("categoria", "conversazione"),
                             1.0, 0.65, "user_positive")
    return {}


def _telemetry_costs(bench: dict) -> dict:
    """Costo reale per 1.000 token, aggregato su tre finestre temporali.

    La fonte e' ``memory_meta.usage_log``: contiene l'usage dichiarato da
    OpenRouter e quindi evita di confondere frequenza d'uso, token stimati e
    prezzi di modelli diversi. Il costo di ogni riga e' calcolato con l'ID
    esatto tramite :func:`price_map.cost_usd`.
    """
    global _TELEMETRY_COST_CACHE, _TELEMETRY_COST_CACHE_TS
    now = time.time()
    if (_TELEMETRY_COST_CACHE is not None
            and now - _TELEMETRY_COST_CACHE_TS < _ROUTING_CACHE_TTL):
        return _TELEMETRY_COST_CACHE
    out = {name: {} for name in COST_WINDOWS}
    try:
        from memory_meta import usage_rows
        rows = usage_rows(days=max(COST_WINDOWS.values()) / 86400.0)
    except (OSError, sqlite3.Error, TypeError, ValueError):
        rows = []

    for name, seconds in COST_WINDOWS.items():
        for row in rows:
            ts = float(row.get("ts_utc", 0) or 0)
            if now - ts > seconds:
                continue
            model = str(row.get("model") or "")
            prompt = max(0, int(row.get("prompt_tokens", 0) or 0))
            completion = max(0, int(row.get("completion_tokens", 0) or 0))
            tokens = prompt + completion
            dollars = cost_usd(model, prompt, completion)
            # Prezzo sconosciuto: non inventare un costo e non falsare il
            # confronto con uno zero apparente.
            if not tokens or dollars is None:
                continue
            cur = out[name].setdefault(model, [0.0, 0])
            cur[0] += dollars
            cur[1] += tokens

    _TELEMETRY_COST_CACHE = {
        window: {m: cost / tokens * 1000 for m, (cost, tokens) in values.items()
                 if tokens > 0}
        for window, values in out.items()
    }
    _TELEMETRY_COST_CACHE_TS = now
    return _TELEMETRY_COST_CACHE


def _score(bench: dict, modello: str, categoria: str, personal: dict,
           telemetry_costs: dict, pool=None) -> tuple:
    """Score benchmark + delta personale appreso, penalizzato dal costo/token."""
    b = bench.get(modello) or DEFAULT_BENCHMARK.get(modello) or {}
    ufficiale = 0.6 * b.get(categoria, 7.0) + 0.4 * b.get("affidabilita", 7.0)
    p = personal.get(modello) or {}
    personale_entry = p.get(categoria)
    if isinstance(personale_entry, dict):
        correzione_personale = max(-PERSONAL_DELTA_CAP, min(
            PERSONAL_DELTA_CAP, float(personale_entry.get("delta", 0.0) or 0.0)))
        personale = correzione_personale
    elif isinstance(personale_entry, (int, float)):
        # Compatibilita' con il vecchio file: valore storico -> influenza minima.
        personale = float(personale_entry)
        correzione_personale = max(-PERSONAL_DELTA_CAP, min(
            PERSONAL_DELTA_CAP, (personale - 5.0) * 0.04))
    else:
        personale = None
        correzione_personale = 0.0
    qualita = ufficiale + correzione_personale
    costs = [v[modello] for v in telemetry_costs.values() if modello in v]

    def fallback_cost(model, values):
        prices = get_price(model)
        if prices:
            # Se non abbiamo osservazioni per il modello, usiamo il mix reale
            # prompt/completion globale; 85/15 e' il default prudente.
            return (prices[0] * 0.85 + prices[1] * 0.15) / 1000.0
        pin = float(values.get("prezzo_in", 0) or 0)
        pout = float(values.get("prezzo_out", 0) or 0)
        return (pin * 0.85 + pout * 0.15) / 1000.0

    if costs:
        costo_k = sum(costs) / len(costs)
    else:
        costo_k = fallback_cost(modello, b)

    # Confronto solo tra candidati del pool: nessun modello storico estraneo
    # puo' abbassare artificialmente il baseline a zero.
    candidate_pool = list(pool or bench.keys())
    peers = []
    for model in candidate_pool:
        observed = [v[model] for v in telemetry_costs.values() if model in v]
        model_bench = bench.get(model) or DEFAULT_BENCHMARK.get(model) or {}
        peers.append(sum(observed) / len(observed)
                     if observed else fallback_cost(model, model_bench))
    peers = [value for value in peers if value > 0]
    baseline = max(1e-9, min(peers or [costo_k]))
    # Logaritmica: il costo pesa, ma non annulla la qualita'.
    costo_penalty = min(3.0, 0.9 * math.log1p(max(0.0, costo_k / baseline - 1.0)))
    score = PESI["qualita"] * qualita + PESI["costo"] * max(0.0, 10.0 - costo_penalty)
    return (round(score, 3), round(ufficiale, 3),
            round(personale, 3) if personale is not None else None,
            round(correzione_personale, 3), round(costo_k, 6))





def _audit(record: dict):
    try:
        with open(AUDIT_FILE, "a", encoding="utf-8") as f:
            f.write(json.dumps(record, ensure_ascii=False) + "\n")
    except Exception:
        pass


def _slim_audit(record: dict) -> dict:
    """Telemetria snella (exception encoding): stesso contenuto informativo,
    meno byte.

    - ``telemetry_cost_windows``: le finestre coincidono quasi sempre con i
      costi gia' presenti in ``score_details``; si salvano SOLO i modelli che
      divergono (>5% di scarto), cosi' il dato mancante resta implicito.
    - ``pool``: costante quasi sempre; salvato solo quando cambia rispetto al
      record precedente (assenza = pool invariato).
    - ``shadow``: ridondante, e' gia' marcato nel ``motivo``.
    - ``fonte``: derivabile da benchmark_data in lettura, non serve duplicarlo.
    """
    out = dict(record)
    det = out.get("score_details") or {}
    base = {m: (d or {}).get("costo_per_1000_token") or 0.0
            for m, d in det.items()}
    win = out.pop("telemetry_cost_windows", None)
    if win:
        diff = {}
        for nome, w in win.items():
            d = {m: round(v, 8) for m, v in w.items()
                 if abs(v - base.get(m, 0.0)) > max(base.get(m, 0.0) * 0.05, 1e-9)}
            if d:
                diff[nome] = d
        if diff:
            out["cost_windows_diff"] = diff
    prev = _state.get("last") or {}
    if out.get("pool") == prev.get("pool"):
        out.pop("pool", None)
    return out


def _pool_dinamico() -> list:
    """Restituisce il pool gia' calcolato in memoria.

    Il pool dinamico viene aggiornato insieme ai benchmark, al massimo una volta
    ogni 24 ore, sempre in background. Durante una richiesta non si fanno
    letture, import costosi o chiamate a benchmark_data.
    """
    return list(_POOL_CACHE or ROUTING_POOL)


def decide_model(user_input: str, context=None) -> dict:
    """Decisione pesata su richiesta, carico previsto e continuità dello storico."""
    bench = _bench()
    categoria, conf = classify_input(user_input)
    tool_phase = _tool_phase(context)
    if tool_phase == "codice":
        # La fase operativa prevale sul testo originale: il lavoro puo' essere
        # diventato codice dopo una lettura, una diagnosi o un primo tool call.
        categoria, conf = "codice", max(conf, 0.90)
    features = _context_features(user_input, context)
    pool = _pool_dinamico()
    personal = _personal_scores()
    telemetry_costs = _telemetry_costs(bench)
    score_details = {}
    base_scores = {}
    for m in pool:
        base_scores[m], ufficiale, personale, correzione_personale, costo_k = _score(
            bench, m, categoria, personal, telemetry_costs, pool=pool)
        score_details[m] = {
            "ufficiale": ufficiale,
            "personale": personale,
            "correzione_personale": correzione_personale,
            "costo_per_1000_token": costo_k,
        }
    scores = {m: round(base_scores[m] + _profile_bonus(m, features, categoria), 3) for m in pool}
    best = max(scores, key=lambda k: scores[k])
    prev = _state.get("previous")
    if prev not in scores:
        prev = pool[0]
    ordinati = sorted(scores.values(), reverse=True)
    margine = round(ordinati[0] - ordinati[1], 3) if len(ordinati) > 1 else 0.0

    if conf < GATE_MIN:
        scelta, motivo = prev, f"gate conf {conf:.2f} < {GATE_MIN}: fallback conservativo"
    else:
        soglia = 0.01 if conf >= SWITCH_IMMEDIATO else MARGINE_MINIMO * (1 + 2 * (1 - conf))
        if best == prev:
            scelta, motivo = prev, "conferma modello corrente"
            _state["streak"] = {}
        elif (scores[best] - scores[prev] >= soglia
              and (conf >= SWITCH_IMMEDIATO or _state["streak"].get(best, 0) + 1 >= STREAK_NECESSARI)):
            scelta = best
            motivo = f"switch: margine {margine} >= soglia {soglia:.2f}"
            _state["streak"] = {}
        else:
            _state["streak"][best] = _state["streak"].get(best, 0) + 1
            scelta = prev
            motivo = (f"attesa isteresi ({_state['streak'][best]}/{STREAK_NECESSARI}) "
                      f"o margine {margine} < soglia {soglia:.2f}")

    if SHADOW_MODE:
        motivo += f" [SHADOW: sarebbe {best}]"
        scelta = prev

    _state["previous"] = scelta
    signature = f"{categoria}:{features['changed']}:{features['history_turns']}"
    if signature != _state.get("context_signature") and features["changed"]:
        _state["context_epoch"] += 1
    _state["context_signature"] = signature
    record = {
        "ts": datetime.now().isoformat(timespec="seconds"),
        "categoria": categoria, "confidenza": round(conf, 3),
        "profilo": features, "context_epoch": _state["context_epoch"],
        "scores": scores, "score_details": score_details,
        "telemetry_cost_windows": telemetry_costs,
        "scelto": scelta, "best": best,
        "margine": margine, "motivo": motivo,
    }
    if SHADOW_MODE:
        record["pool"] = pool  # in shadow il pool e' contesto rilevante
    _state["last"] = record
    _audit(_slim_audit(record))
    return record


def route(user_input: str, context=None) -> str:
    """Entry point: sceglie il modello usando richiesta e contesto conversazionale."""
    return decide_model(user_input, context=context)["scelto"]


def descrivi_ultima_decisione(active_model: str) -> str:
    """Riga diagnostica per la console di astral.py."""
    d = _state.get("last") or {}
    if not d:
        return f"Routing: {active_model}"
    scelto = str(d.get("scelto", active_model)).split("/")[-1]
    sc = d.get("scores", {}).get(d.get("scelto"), 0)
    # Il routing service possiede direttamente il flag: nessun import inverso
    # verso llm_core (evita dipendenza circolare e side effect del client LLM).
    telemetry_active = get_telemetry_enabled()
    best = str(d.get("best", scelto)).split("/")[-1]
    # Se scelto != best aggiungo solo il motivo del "perche' non lui";
    # se e' il vincitore, la riga finisce li': nulla da aggiungere.
    nota = "" if scelto == best else f" [dim]< {best}: {d.get('motivo', '')}[/dim]"
    return (f"{d.get('categoria', '?')} {d.get('confidenza', 0):.0%} "
            f"-> [bold dark_orange]{scelto}[/] {sc:.2f}{nota}")


if __name__ == "__main__":
    # Golden set offline (verdetto: validare il classificatore prima del rollout)
    golden = [
        ("debug di questo script python con traceback", "codice"),
        ("scrivi una funzione python che legge un csv", "codice"),
        ("query sql con join su due database", "codice"),
        ("correggi il bug nella classe python", "codice"),
        ("il comando powershell per listare i processi", "codice"),
        ("come stai? raccontami una curiosita", "conversazione"),
        ("qual e la capitale della francia", "conversazione"),
        ("spiegami la teoria della relativita", "conversazione"),
        ("consigliami un film da stasera", "conversazione"),
        ("buongiorno, che tempo fa oggi?", "conversazione"),
    ]
    ok = sum(1 for t, exp in golden if classify_input(t)[0] == exp)
    print(f"Golden set: {ok}/{len(golden)} corrette ({ok / len(golden):.0%})")
    try:
        from benchmark_data import info
        print("Dati:", info())
    except Exception:
        pass
    d = decide_model("scrivi una funzione python che parsa un csv")
    print("Demo:", d["categoria"], "->", d["scelto"], "| pool:", d["pool"])
