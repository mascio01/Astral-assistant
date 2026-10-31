# -*- coding: utf-8 -*-
"""latency_stats.py - Latenza misurata sul campo + limiter adattivo (AIMD).

Perche' esiste questo modulo
-----------------------------
Il rate limit del provider upstream non e' leggibile da nessuna parte (ne' API,
ne' dashboard): l'unico modo per non indovinarlo e' MISURARLO sul campo.
Qui raccogliamo due segnali reali e li usiamo per correggere il comportamento:

1) latenza media per modello (EWMA sui tempi di risposta delle chiamate riuscite);
2) esito delle chiamate (ok / RATE_LIMIT), usato da un controllore AIMD che
   regola il tetto di chiamate del limiter interno attorno al valore di config.

Il valore di config resta la BASE dichiarata dall'utente; il fattore AIMD la
corregge in tempo semireale dentro limiti stretti [MIN_FACTOR, MAX_FACTOR].
Tutto e' best-effort: qualunque errore qui non deve mai rompere una chiamata LLM.
"""
import sqlite3

import threading

import time

from core.memory_store import TELEMETRY_DB

# --- parametri del controllore ------------------------------------------------
ALPHA = 0.25            # peso del campione nuovo nell'EWMA di latenza
MIN_FACTOR = 0.35       # il limiter non scende sotto il 35% della base
MAX_FACTOR = 1.00       # ... ne' sale sopra la base dichiarata dall'utente
AI_STEP = 0.02          # additive increase per chiamata riuscita
MD_FACTOR = 0.70        # multiplicative decrease su RATE_LIMIT
DECAY_SECONDS = 900.0   # dopo 15 min senza eventi il fattore torna verso 1.0

_LOCK = threading.Lock()
# [ITEM#2] Cache per-modello: chiave = model_name ("" = globale).
_CACHE = {}
_SCHEMA_READY = False


def _connect():
    conn = sqlite3.connect(TELEMETRY_DB, timeout=5)
    return conn


def ensure_schema():
    """Crea tabelle/colonne necessarie. Idempotente e silenzioso."""
    global _SCHEMA_READY
    if _SCHEMA_READY:
        return True
    try:
        with _connect() as conn:
            conn.execute(
                "CREATE TABLE IF NOT EXISTS latency_stats ("
                "model_name TEXT PRIMARY KEY, ewma_ms REAL NOT NULL, "
                "samples INTEGER NOT NULL DEFAULT 0, updated_ts REAL NOT NULL)")
            conn.execute(
                "CREATE TABLE IF NOT EXISTS limiter_events ("
                "id INTEGER PRIMARY KEY AUTOINCREMENT, ts REAL NOT NULL, "
                "model_name TEXT, wait_s REAL NOT NULL, max_calls INTEGER NOT NULL, "
                "window_s INTEGER NOT NULL, reason TEXT)")
            conn.execute(
                "CREATE TABLE IF NOT EXISTS limiter_state ("
                "key TEXT PRIMARY KEY, value TEXT NOT NULL)")
            # Campioni grezzi: servono per analisi a posteriori (mediana, p95,
            # trend), l'EWMA da sola non basta a ricostruire la distribuzione.
            # [ITEM#1] Ogni campione porta anche l'ESITO della chiamata: senza
            # questo dato non si puo' distinguere una latenza buona da un 429
            # veloce ne' calcolare un tasso di errore per modello.
            conn.execute(
                "CREATE TABLE IF NOT EXISTS latency_samples ("
                "id INTEGER PRIMARY KEY AUTOINCREMENT, ts REAL NOT NULL, "
                "model_name TEXT NOT NULL, latency_ms REAL NOT NULL)")
            cols = {r[1] for r in conn.execute(
                "PRAGMA table_info(latency_samples)").fetchall()}
            if "outcome" not in cols:
                conn.execute("ALTER TABLE latency_samples ADD COLUMN outcome TEXT")
            if "error_code" not in cols:
                conn.execute("ALTER TABLE latency_samples ADD COLUMN error_code TEXT")
            conn.execute(
                "CREATE INDEX IF NOT EXISTS idx_latency_samples_model_ts "
                "ON latency_samples(model_name, ts)")
            # [ITEM#3] Stato runtime per modello: cooldown dopo un 429 e
            # timestamp dell'ultima chiamata (pacing). Persistito cosi' un
            # riavvio non azzera un cooldown ancora attivo lato provider.
            conn.execute(
                "CREATE TABLE IF NOT EXISTS model_state ("
                "model_name TEXT PRIMARY KEY, cooldown_until REAL NOT NULL DEFAULT 0,"
                " last_call_ts REAL NOT NULL DEFAULT 0)")
            conn.commit()
        _SCHEMA_READY = True
        return True
    except Exception:
        return False


# --- latenza ------------------------------------------------------------------
def record_latency(model_name, latency_ms=None):
    return record_outcome(model_name or "", "ok", latency_ms)


# Esiti ammessi in ``latency_samples.outcome``.
OUTCOMES = ("ok", "RATE_LIMIT", "TIMEOUT", "AUTH", "QUOTA", "SERVER",
            "SCONOSCIUTO")


def record_outcome(model_name, outcome="ok", latency_ms=None, error_code=None):
    """Registra un campione con ESITO e latenza (cuore dell'item #1).

    L'EWMA di latenza viene aggiornata SOLO sugli esiti ``ok``: mescolare le
    latenze dei 429 (che tornano in pochi ms) abbasserebbe la media e farebbe
    sottostimare il backoff. Il campione grezzo viene invece salvato sempre,
    cosi' il tasso di errore per modello resta calcolabile a posteriori.
    """
    try:
        if not model_name:
            return False
        if outcome not in OUTCOMES:
            outcome = "SCONOSCIUTO"
        if not ensure_schema():
            return False
        now = time.time()
        with _LOCK:
            with _connect() as conn:
                conn.execute(
                    "INSERT INTO latency_samples(ts, model_name, latency_ms, "
                    "outcome, error_code) VALUES(?,?,?,?,?)",
                    (now, model_name,
                     float(latency_ms) if latency_ms is not None else -1.0,
                     outcome, error_code))
                if outcome == "ok" and latency_ms is not None and latency_ms >= 0:
                    row = conn.execute(
                        "SELECT ewma_ms FROM latency_stats WHERE model_name=?",
                        (model_name,)).fetchone()
                    new = float(latency_ms) if row is None else (
                        ALPHA * float(latency_ms) + (1 - ALPHA) * float(row[0]))
                    conn.execute(
                        "INSERT INTO latency_stats(model_name, ewma_ms, samples, updated_ts) "
                        "VALUES(?,?,1,?) ON CONFLICT(model_name) DO UPDATE SET "
                        "ewma_ms=excluded.ewma_ms, samples=samples+1, "
                        "updated_ts=excluded.updated_ts",
                        (model_name, new, now))
                conn.commit()
        return True
    except Exception:
        return False



def get_avg_latency(model_name=None):
    """Latenza media EWMA in ms per il modello (o media globale se None)."""
    try:
        if not ensure_schema():
            return None
        with _connect() as conn:
            if model_name:
                row = conn.execute(
                    "SELECT ewma_ms FROM latency_stats WHERE model_name=?",
                    (model_name,)).fetchone()
                return float(row[0]) if row else None
            row = conn.execute(
                "SELECT AVG(ewma_ms) FROM latency_stats").fetchone()
            return float(row[0]) if row and row[0] is not None else None
    except Exception:
        return None


# --- controllore AIMD (per modello) -------------------------------------------
def _factor_key(model_name):
    return "aimd_factor:%s" % model_name if model_name else "aimd_factor"


def _load_factor(model_name=None):
    try:
        with _connect() as conn:
            row = conn.execute(
                "SELECT value FROM limiter_state WHERE key=?",
                (_factor_key(model_name),)).fetchone()
        if row:
            return max(MIN_FACTOR, min(MAX_FACTOR, float(row[0])))
    except Exception:
        pass
    return MAX_FACTOR


def _store_factor(value, model_name=None):
    try:
        with _connect() as conn:
            conn.execute(
                "INSERT INTO limiter_state(key, value) VALUES(?, ?) "
                "ON CONFLICT(key) DO UPDATE SET value=excluded.value",
                (_factor_key(model_name), "%.4f" % value))
            conn.commit()
    except Exception:
        pass


def get_factor(model_name=None):
    """Fattore corrente applicato al tetto di chiamate (1.0 = base piena)."""
    with _LOCK:
        now = time.time()
        key = model_name or ""
        entry = _CACHE.setdefault(key, {"factor": None, "last_event": 0.0})
        if entry["factor"] is None:
            ensure_schema()
            entry["factor"] = _load_factor(model_name)
            entry["last_event"] = now
        elif now - entry["last_event"] > DECAY_SECONDS and entry["factor"] < MAX_FACTOR:
            # Ripresa lenta dopo un periodo di quiete: si torna verso la base.
            entry["factor"] = min(MAX_FACTOR, entry["factor"] + AI_STEP * 5)
            entry["last_event"] = now
            _store_factor(entry["factor"], model_name)
        return entry["factor"]


def effective_max_calls(base_max, model_name=None):
    """Tetto di chiamate corretto dal fattore AIMD del modello (mai sotto 1)."""
    try:
        base_max = int(base_max)
    except (TypeError, ValueError):
        return base_max
    return max(1, int(round(base_max * get_factor(model_name))))


def on_success(model_name=None):
    """Chiamata riuscita: additive increase molto lento.

    [FIX] Non si tiene _LOCK mentre si chiama get_factor (che lo riprende):
    il lock e' un threading.Lock non rientrante e causava deadlock.
    """
    cur = get_factor(model_name)
    new = min(MAX_FACTOR, cur + AI_STEP)
    if abs(new - cur) > 1e-9:
        with _LOCK:
            entry = _CACHE.setdefault(model_name or "",
                                      {"factor": None, "last_event": 0.0})
            entry["factor"] = new
            entry["last_event"] = time.time()
        _store_factor(new, model_name)


def on_rate_limit(model_name=None):
    """429 ricevuto: multiplicative decrease immediato. Ritorna il nuovo fattore.

    [FIX] Come on_success: nessun _LOCK trattenuto durante get_factor.
    """
    cur = get_factor(model_name)
    new = max(MIN_FACTOR, cur * MD_FACTOR)
    with _LOCK:
        entry = _CACHE.setdefault(model_name or "",
                                  {"factor": None, "last_event": 0.0})
        entry["factor"] = new
        entry["last_event"] = time.time()
    _store_factor(new, model_name)
    return new


# --- cooldown per modello (item #3) -------------------------------------------
COOLDOWN_BASE = 2.0      # secondi minimi di pausa dopo un 429
COOLDOWN_MAX = 120.0     # tetto: oltre non ha senso bloccare il modello


def set_cooldown(model_name, seconds):
    """Blocca un modello fino a ``now+seconds`` (persistito)."""
    try:
        if not model_name:
            return False
        if not ensure_schema():
            return False
        seconds = max(0.0, min(COOLDOWN_MAX, float(seconds)))
        until = time.time() + seconds
        with _LOCK:
            with _connect() as conn:
                conn.execute(
                    "INSERT INTO model_state(model_name, cooldown_until, last_call_ts) "
                    "VALUES(?,?,0) ON CONFLICT(model_name) DO UPDATE SET "
                    "cooldown_until=MAX(cooldown_until, excluded.cooldown_until)",
                    (model_name, until))
                conn.commit()
        return True
    except Exception:
        return False


def get_cooldown_remaining(model_name):
    """Secondi residui di cooldown per il modello (0.0 se libero)."""
    try:
        if not model_name or not ensure_schema():
            return 0.0
        with _connect() as conn:
            row = conn.execute(
                "SELECT cooldown_until FROM model_state WHERE model_name=?",
                (model_name,)).fetchone()
        if not row:
            return 0.0
        return max(0.0, float(row[0]) - time.time())
    except Exception:
        return 0.0


def note_call(model_name):
    """Registra il timestamp dell'ultima chiamata (base del pacing)."""
    try:
        if not model_name or not ensure_schema():
            return False
        with _LOCK:
            with _connect() as conn:
                conn.execute(
                    "INSERT INTO model_state(model_name, cooldown_until, last_call_ts) "
                    "VALUES(?,0,?) ON CONFLICT(model_name) DO UPDATE SET "
                    "last_call_ts=excluded.last_call_ts", (model_name, time.time()))
                conn.commit()
        return True
    except Exception:
        return False


# --- stimatore di latenza robusto (item #4) -----------------------------------
def get_latency_percentile(model_name=None, pct=95, days=14):
    """Percentile di latenza sui campioni OK recenti (ms). None se pochi dati."""
    try:
        if not ensure_schema():
            return None
        since = time.time() - days * 86400
        q = ("SELECT latency_ms FROM latency_samples WHERE ts>=? AND outcome='ok' "
             "AND latency_ms>=0")
        args = [since]
        if model_name:
            q += " AND model_name=?"
            args.append(model_name)
        with _connect() as conn:
            vals = [float(r[0]) for r in conn.execute(q, args).fetchall()]
        if len(vals) < 5:
            return None
        vals.sort()
        idx = min(len(vals) - 1, int(round((pct / 100.0) * (len(vals) - 1))))
        return vals[idx]
    except Exception:
        return None


def estimate_backoff_ms(model_name=None):
    """Attesa consigliata prima di ritentare: usa il p95 reale del modello."""
    p95 = get_latency_percentile(model_name, 95)
    if p95 is None:
        p95 = get_avg_latency(model_name)
    if p95 is None:
        return None
    return max(500.0, min(8000.0, float(p95)))


# --- eventi del limiter -------------------------------------------------------
def record_limiter_wait(wait_s, max_calls=None, window_s=None,
                        model_name=None, reason="window"):
    """Registra un'attesa imposta dal limiter interno per analisi a posteriori."""
    try:
        if not ensure_schema():
            return False
        with _connect() as conn:
            conn.execute(
                "INSERT INTO limiter_events(ts, model_name, wait_s, max_calls, "
                "window_s, reason) VALUES(?,?,?,?,?,?)",
                (time.time(), model_name or "", float(wait_s),
                 int(max_calls or 0), int(window_s or 0), reason))
            conn.commit()
        return True
    except Exception:
        return False


def stats_summary(days=7):
    """Riepilogo testuale compatto per diagnostica."""
    try:
        if not ensure_schema():
            return "latency_stats: schema non disponibile"
        since = time.time() - days * 86400
        with _connect() as conn:
            lat = conn.execute(
                "SELECT model_name, ewma_ms FROM latency_stats "
                "ORDER BY ewma_ms ASC").fetchall()
            evt = conn.execute(
                "SELECT COUNT(*), COALESCE(SUM(wait_s),0), COALESCE(AVG(wait_s),0) "
                "FROM limiter_events WHERE ts>=?", (since,)).fetchone()
        lines = ["latenza media EWMA per modello:"]
        for name_, ms in lat[:10]:
            lines.append("  %-38s %7.0f ms" % (name_, ms))
        lines.append("attese limiter (%sg): %d eventi, %.1fs totali, %.2fs medie"
                     % (days, evt[0], evt[1], evt[2]))
        lines.append("fattore AIMD globale: %.2f" % get_factor())
        # [ITEM#1] Tasso di errore per modello sui campioni con esito noto.
        try:
            with _connect() as conn:
                rows = conn.execute(
                    "SELECT model_name, COUNT(*), "
                    "SUM(CASE WHEN outcome='ok' THEN 1 ELSE 0 END) "
                    "FROM latency_samples WHERE ts>=? AND outcome IS NOT NULL "
                    "GROUP BY model_name ORDER BY COUNT(*) DESC", (since,)).fetchall()
            if rows:
                lines.append("esiti (%sg):" % days)
                for name_, tot, ok in rows[:10]:
                    err = tot - (ok or 0)
                    lines.append("  %-38s %5d chiamate  %5.1f%% errore"
                                 % (name_, tot, 100.0 * err / tot if tot else 0.0))
        except Exception:
            pass
        return "\n".join(lines)
    except Exception as e:
        return "latency_stats: errore %s" % e


if __name__ == "__main__":
    print(stats_summary())
