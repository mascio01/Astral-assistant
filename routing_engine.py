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
from budget_guard import check_credits
from price_map import cost_usd, get_price

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
AUDIT_FILE = os.path.join(BASE_DIR, ".routing_audit.jsonl")

# Pesi base: qualita' (60% categoria + 40% affidabilita) 0.7 + costo REALE
# OpenRouter 0.3. Il peso del costo e' poi modulato dinamicamente dal tier di
# budget in base al credito residuo (set_budget_tier, chiamato da astral.py
# una volta per sessione; vedi _effective_pes()).
PESI = {"qualita": 0.7, "costo": 0.3}
# Tier budget: sotto soglie di credito residuo il costo pesa di piu'.
# (soglia_usd, peso_costo): il primo tier applicabile vince; None = peso base.
BUDGET_TIERS = [
    (20.0, 0.55),   # credito quasi esaurito: risparmio aggressivo
    (50.0, 0.35),   # credito in esaurimento: pressione moderata
]


def set_budget_tier(remaining, total=None):
    """Aggiorna PESI in base al credito residuo (chiamato a inizio sessione)."""
    peso = 0.3
    if remaining is not None:
        # Anche con total=None (pagamento a consumo illimitato) si applicano
        # le soglie assolute: e' il saldo reale, e' quello che conta.
        for soglia, p in BUDGET_TIERS:
            if remaining < soglia:
                peso = p
                break
    PESI["costo"] = peso
    PESI["qualita"] = round(1.0 - peso, 2)
    return dict(PESI)
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
# Profilo personale a due livelli: generale + categorie Astral.
PERSONAL_GENERAL_CAP = 0.20
PERSONAL_CATEGORY_CAP = 0.22
PERSONAL_DELTA_STEP = 0.08
PERSONAL_DELTA_MIN_STEP = 0.015
# Mean-reversion del delta personale: senza decay, ogni successo (la norma)
# accumula e il delta sale monotonamente al cap senza mai scendere.
PERSONAL_DELTA_DECAY = 0.97
_PERSONAL_LOCK = threading.RLock()

# Pool operativo (specchio di llm_core.DYNAMIC_MODELS_POOL).
# _pool_dinamico() arricchisce con benchmark_data.pool_suggerito() SOLO le scelte
# per gruppo, ma il candidato e' sempre selezionato dentro questo stesso set.
# POLICY DI COSTO: 2 modelli, non 3. Il 0731 e' il default (input 0.04/M);
# il 4.1-flash entra solo su codice complesso (input 0.10/M, 2.5x).
# gpt-5.6-luna rimosso: input 0.20/M (5x il 0731) senza vantaggio sulle
# conversazioni, che sono il 96% dei token in input.
ROUTING_POOL = [
    "deepseek/deepseek-v4-flash-0731",
    "deepseek/deepseek-v4.1-flash",
]

# Valori fittizi iniziali (scala 0-10; costo: 10 = piu' economico)
DEFAULT_BENCHMARK = {
    "deepseek/deepseek-v4.1-flash": {"conversazione": 8.12, "codice": 8.0, "affidabilita": 7.0, "costo": 8.5},
    "openai/gpt-6-luna": {"conversazione": 7.38, "codice": 7.9, "affidabilita": 5.59, "costo": 8.5},
    "deepseek/deepseek-v4-flash-vision-exp": {"conversazione": 8.04, "codice": 6.82, "affidabilita": 7.1, "costo": 6.5},
    "deepseek/deepseek-v4-flash-0731": {"conversazione": 7.92, "codice": 7.5, "affidabilita": 6.55, "costo": 9.0},
    "openai/gpt-5.6-sol": {"conversazione": 9.5, "codice": 9.0, "affidabilita": 9.5, "costo": 5.0},
    "deepseek/deepseek-v4-pro": {"conversazione": 8.5, "codice": 8.0, "affidabilita": 8.0, "costo": 6.5},
    "z-ai/glm-5.3": {"conversazione": 8.0, "codice": 8.5, "affidabilita": 8.5, "costo": 7.0},
    "openai/gpt-5.6-luna": {"conversazione": 7.26, "codice": 8.29, "affidabilita": 6.01, "costo": 7.5},
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
# -inf: il primo controllo di scadenza deve SEMPRE risultare scaduto, cosi' il
# benchmark reale viene idratato al primo giro anche su macchine accese da poco.
# Con 0.0 e TTL 24h, time.monotonic() < 86400 rendeva now - ts < TTL e il refresh
# non partiva mai: il routing restava sui pesi DEFAULT_BENCHMARK.
_BENCH_CACHE_TS = float("-inf")
_BENCH_LOADING = False
_POOL_CACHE = list(ROUTING_POOL)
_POOL_CACHE_TS = float("-inf")
_TELEMETRY_COST_CACHE = None
_TELEMETRY_COST_CACHE_TS = 0.0
# Benchmark e pool cambiano con il sync giornaliero; solo la telemetria resta breve.
_BENCH_CACHE_TTL = 24 * 3600.0
# Se l'idratazione fallisce (rete assente / get_models() vuoto) non ha senso
# aspettare 24h: si ritenta dopo questo intervallo.
_BENCH_RETRY_TTL = 60.0
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
            model_data = current.setdefault(str(model), {})
            categories = model_data.setdefault("delta_categorie", {})
            entry = categories.setdefault(str(categoria), {})
            if not isinstance(entry, dict):
                entry = {}
                categories[str(categoria)] = entry
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

# Segnali di domanda informativa/generalista (come da verdetto: terza categoria
# "informativo" tra conversazione e codice). Valutati SOLO se nessun segnale
# codice fa fuoco: una domanda sul codice resta sempre "codice".
_INFO_HINTS = (
    "differenza", "differenze", "cos'è", "cos'e", "cosa sono", "che cos",
    "come funziona", "come funzionano", "perché", "perche", "qual è", "qual e",
    "qual'", "esempio di", "esempi di", "significato", "spiega", "confronta",
    " vs ", "meglio", "quando usare", "come si usa", "come si gioca",
    "regole del", "regole della", "in parole povere", "riassumi",
)
_LONG_HINTS = ("in dettaglio", "passo passo", "analizza tutto", "approfondito", "completo", "molti file", "grande progetto")
_ACTION_HINTS = ("esegui", "modifica", "crea", "scrivi", "correggi", "installa", "lancia", "sposta", "cancella")

# --- Policy di costo (obiettivo: risparmiare) ---------------------------------
# Il 0731 costa 0.04 USD/M in input contro 0.10 del 4.1-flash, e l'input e' ~96%
# dei token di Astral: il 0731 e' quindi il DEFAULT per tutto. Il 4.1-flash entra
# solo su "codice complesso", dove la qualita' in piu' (LiveBench coding 8.0 vs
# 7.5) ripaga il sovrapprezzo. Il divario di score ufficiale tra i due e' ~0.15
# (conversazione) e ~0.28 (codice): i bonus sotto lo coprono con margine.
_POLICY_DEFAULT_BONUS = 0.45          # premio al 0731 quando NON c'e' codice complesso
_POLICY_41_ON_DEFAULT = -0.45         # speculare: il 4.1-flash paga il suo costo
_POLICY_0731_CODE_COMPLEX = 0.30      # il 0731 non e' escluso, ma resta dietro
_POLICY_41_CODE_COMPLEX = 0.95        # qui il 4.1-flash deve vincere
_COMPLEX_CHARS = 400                  # prompt lungo = richiesta articolata
_LONG_HISTORY_CHARS = 18000           # contesto ampio = lavoro in corso
_COMPLEX_HINTS = (
    "rifattorizza", "refactor", "ottimizza", "architettura", "algoritmo",
    "debug", "traceback", "errore", "bug", "performance", "migrazione",
    "multifile", "piu file", "regex", "concorrenza", "thread", "database",
)
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
    history_chars = sum(len(str(m.get("content", ""))) for m in history
                        if isinstance(m, dict))
    # "Codice complesso": l'unico caso in cui il modello piu' caro in input
    # (v4.1-flash) vale il costo. Serve un segnale forte: tool gia' in fase
    # codice, refactor/debug/architettura, prompt lungo o contesto ampio.
    # Una richiesta breve e diretta ("scrivi uno script") NON qualifica.
    code_complex = bool(code) and (
        tool_phase == "codice"
        or any(h in lower for h in _COMPLEX_HINTS)
        or len(text) > _COMPLEX_CHARS
        or history_chars > _LONG_HISTORY_CHARS
    )
    return {
        "code_complex": code_complex,
        "quick": any(h in lower for h in _QUICK_HINTS),
        "long": any(h in lower for h in _LONG_HINTS),
        "action": any(h in lower for h in _ACTION_HINTS),
        "reasoning": any(h in lower for h in _REASONING_HINTS),
        "code": code,
        "tool_phase": tool_phase,
        "input_chars": len(text),
        "history_turns": len(history),
        "history_chars": history_chars,
        "overlap": round(overlap, 3),
        "changed": bool(previous) and overlap < 0.04,
    }


def _active_categories(user_input: str, features: dict, primary=None) -> list:
    """Restituisce le categorie Astral attive con peso anti-doppio-conteggio."""
    detected = []
    primary = primary or classify_input(user_input or "")[0]

    def add(name, weight):
        if name and name not in [item[0] for item in detected]:
            detected.append((name, round(max(0.0, min(1.0, weight)), 3)))

    add(primary, 1.0)
    if features.get("code") and primary != "codice":
        add("codice", 0.80)
    if features.get("tool_phase") == "codice":
        add("tool", 0.70)
    if features.get("action"):
        add("azione", 0.65)
    if features.get("reasoning"):
        add("ragionamento", 0.60)
    if features.get("long") or features.get("history_chars", 0) > 18000:
        add("approfondimento", 0.60)
    if features.get("quick"):
        add("rapidita", 0.55)
    if features.get("overlap", 0.0) >= 0.15:
        add("continuita", 0.45)
    return detected


def _profile_bonus(model: str, features: dict, categoria: str) -> float:
    """Modulatore operativo (scala score 0-10), volutamente piccolo rispetto ai benchmark."""
    bonus = 0.0
    is_0731 = model == "deepseek/deepseek-v4-flash-0731"
    is_ds41 = model == "deepseek/deepseek-v4.1-flash"
    code_complex = bool(features.get("code_complex"))
    # Blocco di policy: decide QUALE dei due modelli e' il default. E' dominante
    # rispetto ai micro-modulatori sotto, che restano come tie-breaker.
    if is_0731:
        bonus += _POLICY_0731_CODE_COMPLEX if code_complex else _POLICY_DEFAULT_BONUS
    elif is_ds41:
        bonus += _POLICY_41_CODE_COMPLEX if code_complex else _POLICY_41_ON_DEFAULT
    if features["quick"]:
        bonus += 0.22 if is_0731 else -0.08
    if features["long"] or features["history_chars"] > _LONG_HISTORY_CHARS:
        bonus += 0.12 if is_0731 else 0.02
    if features.get("tool_phase") == "codice":
        # Durante una modifica gia' avviata il cambio deve essere applicabile
        # subito: non lasciamo che l'isteresi mantenga il modello conversazionale.
        bonus += 0.10 if is_0731 else 0.30
    if features["reasoning"]:
        bonus += 0.05 if is_0731 else 0.10
    if features["action"]:
        bonus += 0.04 if is_0731 else 0.12
    if features["changed"]:
        bonus += 0.0 if is_0731 else 0.08
    # Continuità: evita di cambiare IA per una semplice prosecuzione del filo.
    if features["overlap"] >= 0.15 and model == _state.get("previous"):
        bonus += 0.22
    return bonus


def classify_input(text: str):
    """Ritorna (categoria, confidenza 0-1). 'codice' se rilevati marker/keyword;
    'informativo' per domande generalista/esplicative senza segnali codice;
    la confidenza cresce col numero di segnali indipendenti (piu' segnali = gate aperto)."""
    lower = (text or "").lower()
    n_kw = sum(1 for kw in _CODICE_KW if re.search(r"\b" + re.escape(kw) + r"\b", lower))
    n_mk = sum(1 for m in _CODE_MARKERS if m in (text or ""))
    if n_kw + n_mk == 0:
        n_info = sum(1 for h in _INFO_HINTS if h in lower)
        if n_info >= 1:
            return "informativo", min(0.90, 0.60 + 0.08 * n_info)
        return "conversazione", 0.85
    conf = min(0.95, 0.52 + 0.16 * n_kw + 0.10 * n_mk)
    return "codice", conf


def answer_format_hint(text: str, categoria: str | None = None) -> str:
    """Separazione routing/formato (verdetto): il classificatore decide COSA
    (categoria/routing), questo layer decide COME rispondere (tono e formato).
    Ritorna una direttiva da appendere al system prompt ('' se nessuna)."""
    cat = categoria or classify_input(text)[0]
    if cat == "informativo":
        return ("FORMATO RISPOSTA (domanda informativa/generalista): rispondi in modo "
                "conversazionale e naturale, come in una chiacchierata. NIENTE tabelle, "
                "NIENTE elenchi puntati lunghi, NIENTE tono da documentazione tecnica. "
                "Prosa breve (max ~150 parole), al massimo 2-3 punti chiave solo se "
                "davvero utili. Vai al sodo.")
    if cat == "conversazione":
        return ("FORMATO RISPOSTA (conversazione): tono naturale e colloquiale, "
                "risposta breve; niente strutture da documento (tabelle/header) "
                "salvo richiesta esplicita.")
    return ""


def _hydrate_bench() -> None:
    """Carica benchmark e pool una volta al giorno, senza bloccare il REPL."""
    global _BENCH_CACHE, _BENCH_CACHE_TS, _BENCH_LOADING
    global _POOL_CACHE, _POOL_CACHE_TS
    ok = False
    try:
        from benchmark_data import get_models, pool_suggerito
        models = get_models()
        if models:
            _BENCH_CACHE = models
            _BENCH_CACHE_TS = time.monotonic()
            ok = True
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
        if not ok:
            # Nessun dato reale caricato: ritenta tra _BENCH_RETRY_TTL, non tra 24h.
            _BENCH_CACHE_TS = time.monotonic() - _BENCH_CACHE_TTL + _BENCH_RETRY_TTL
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

    Il formato corrente e' model -> ``delta_generale`` + ``delta_categorie``.
    Le chiavi piatte del formato precedente restano leggibili e vengono migrate
    nel contenitore delle categorie al primo aggiornamento, senza perderne i dati.
    """
    try:
        with open(PERSONAL_SCORES_FILE, encoding="utf-8") as f:
            data = json.load(f)
        return data if isinstance(data, dict) else {}
    except (OSError, ValueError, TypeError):
        return {}


def _save_outcome(model: str, categoria: str, signal: float, weight: float,
                  label: str, active_categories=None) -> dict:
    """Aggiorna il delta con apprendimento a passo decrescente e cap rigido."""
    with _PERSONAL_LOCK:
        current = _personal_scores()
        model_data = current.setdefault(str(model), {})
        categories = model_data.setdefault("delta_categorie", {})
        for key in list(model_data):
            if key not in {"delta_generale", "delta_categorie"}:
                categories.setdefault(key, model_data.pop(key))
        entry = model_data.setdefault("delta_generale", {})
        if not isinstance(entry, dict):
            entry = {"delta": 0.0, "legacy_value": entry}
            model_data["delta_generale"] = entry
        evidence = max(0.0, min(1.0, float(weight)))
        attempts = int(entry.get("evidenze", 0) or 0)
        # Passo piu' grande all'inizio, poi piu' prudente: evita che un singolo
        # test o una singola risposta sposti il routing in modo sproporzionato.
        learning_rate = max(PERSONAL_DELTA_MIN_STEP,
                            PERSONAL_DELTA_STEP / math.sqrt(1.0 + attempts))
        change = learning_rate * max(-1.0, min(1.0, float(signal))) * evidence
        old_delta = float(entry.get("delta", 0.0) or 0.0)
        # Il delta riflette le prestazioni RECENTI, non un accumulo monotonico:
        # a ogni evento il vecchio delta decade, cosi' una serie di successi
        # converge a un equilibrio invece di incollarsi al cap.
        new_delta = max(-PERSONAL_DELTA_CAP,
                        min(PERSONAL_DELTA_CAP,
                            old_delta * PERSONAL_DELTA_DECAY + change))
        entry["delta"] = round(new_delta, 4)
        entry["evidenze"] = attempts + 1
        entry["positive"] = int(entry.get("positive", 0) or 0) + int(signal > 0.25)
        entry["negative"] = int(entry.get("negative", 0) or 0) + int(signal < -0.25)
        entry["recoverable_errors"] = int(entry.get("recoverable_errors", 0) or 0) + int(label == "recoverable_error")
        entry["last_event"] = label
        entry["last_change"] = round(change, 4)
        category_results = {}
        for name, category_weight in active_categories or [(categoria, 1.0)]:
            category = categories.setdefault(str(name), {})
            if not isinstance(category, dict):
                category = {"delta": 0.0, "legacy_value": category}
                categories[str(name)] = category
            category_results[str(name)] = {"delta": category.get("delta", 0.0),
                                           "peso": category_weight}
        _save_personal_scores(current)
        return {"generale": {"delta": round(new_delta, 4), "change": round(change, 4),
                              "label": label, "evidenze": entry["evidenze"]},
                "categorie": category_results}


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


def record_personal_outcome(model: str, categoria: str, evidence=None, context=None) -> dict:
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
    text = evidence.get("input", "") if isinstance(evidence, dict) else ""
    features = _context_features(text, context or evidence.get("context"))
    active = _active_categories(text, features, primary=categoria)
    return _save_outcome(model, categoria, signal, weight, label,
                         active_categories=active)


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
                             -1.0, 0.85, "user_negative",
                             active_categories=_state["last"].get("categorie_attive"))
    if any(marker in lower for marker in positive):
        return _save_outcome(_state["last"].get("scelto"),
                             _state["last"].get("categoria", "conversazione"),
                             1.0, 0.65, "user_positive",
                             active_categories=_state["last"].get("categorie_attive"))
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
           telemetry_costs: dict, pool=None, active_categories=None) -> tuple:
    """Score benchmark + delta personale appreso, penalizzato dal costo/token."""
    b = bench.get(modello) or DEFAULT_BENCHMARK.get(modello) or {}
    ufficiale = 0.6 * b.get(categoria, 7.0) + 0.4 * b.get("affidabilita", 7.0)
    p = personal.get(modello) or {}
    general_entry = p.get("delta_generale")
    general_delta = (float(general_entry.get("delta", 0.0) or 0.0)
                     if isinstance(general_entry, dict) else 0.0)
    general_delta = max(-PERSONAL_GENERAL_CAP, min(PERSONAL_GENERAL_CAP, general_delta))
    categories = p.get("delta_categorie")
    if not isinstance(categories, dict):
        categories = {k: v for k, v in p.items()
                      if k not in {"delta_generale", "delta_categorie"}}
    category_contrib = {}
    for name, category_weight in active_categories or [(categoria, 1.0)]:
        entry = categories.get(name)
        delta = float(entry.get("delta", 0.0) or 0.0) if isinstance(entry, dict) else 0.0
        delta = max(-PERSONAL_CATEGORY_CAP, min(PERSONAL_CATEGORY_CAP, delta))
        category_contrib[name] = round(delta * float(category_weight), 4)
    correzione_personale = max(-PERSONAL_DELTA_CAP, min(
        PERSONAL_DELTA_CAP, general_delta + sum(category_contrib.values())))
    personale = correzione_personale if (general_entry is not None or categories) else None
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
            round(correzione_personale, 3), round(costo_k, 6),
            round(general_delta, 3), category_contrib)





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
    # "informativo" non esiste nei benchmark: per la scelta del modello pesa
    # come "conversazione", ma resta categoria a se' per learning/formato.
    bench_cat = "conversazione" if categoria == "informativo" else categoria
    tool_phase = _tool_phase(context)
    if tool_phase == "codice":
        # La fase operativa prevale sul testo originale: il lavoro puo' essere
        # diventato codice dopo una lettura, una diagnosi o un primo tool call.
        categoria, conf = "codice", max(conf, 0.90)
    features = _context_features(user_input, context)
    active_categories = _active_categories(user_input, features, primary=categoria)
    features["categorie_attive"] = active_categories
    pool = _pool_dinamico()
    personal = _personal_scores()
    telemetry_costs = _telemetry_costs(bench)
    score_details = {}
    base_scores = {}
    for m in pool:
        (base_scores[m], ufficiale, personale, correzione_personale, costo_k,
         delta_generale, delta_categorie) = _score(
            bench, m, bench_cat, personal, telemetry_costs, pool=pool,
            active_categories=active_categories)
        score_details[m] = {
            "ufficiale": ufficiale,
            "personale": personale,
            "correzione_personale": correzione_personale,
            "delta_generale": delta_generale,
            "delta_categorie": delta_categorie,
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
        "profilo": features, "categorie_attive": active_categories,
        "context_epoch": _state["context_epoch"],
        "pesi": dict(PESI),
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


def _macro_categoria(categoria: str) -> str:
    """Mappa la categoria di routing alla macro LiveBench (es. codice -> coding)."""
    try:
        from benchmark_data import GROUP_ROUTING
        return GROUP_ROUTING.get(categoria, categoria)
    except Exception:
        return categoria


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
    cat = d.get("categoria", "?")
    return (f"{cat} {d.get('confidenza', 0):.0%} "
            f"-> [bold dark_orange]{scelto}[/] {sc:.2f} "
            f"[dim]({_macro_categoria(cat)})[/]{nota}")


if __name__ == "__main__":
    # Golden set offline (verdetto: validare il classificatore prima del rollout)
    golden = [
        ("debug di questo script python con traceback", "codice"),
        ("scrivi una funzione python che legge un csv", "codice"),
        ("query sql con join su due database", "codice"),
        ("correggi il bug nella classe python", "codice"),
        ("il comando powershell per listare i processi", "codice"),
        ("come stai? raccontami una curiosita", "conversazione"),
        ("qual e la capitale della francia", "informativo"),
        ("spiegami la teoria della relativita", "informativo"),
        ("qual e' la differenza tra sit and go e tavoli cash nel poker", "informativo"),
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
    print("Demo:", d["categoria"], "->", d["scelto"], "| pool:", d.get("pool", []))
