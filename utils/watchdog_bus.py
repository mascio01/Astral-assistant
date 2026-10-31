# -*- coding: utf-8 -*-
"""watchdog_bus.py - Bus di eventi su file per il watchdog di Astral.

Perche' esiste: il vecchio watchdog faceva tail di error_log.txt e confrontava
i primi 30 caratteri di una riga. Perdeva il contesto (tipo di errore, traceback,
file target) e poteva agganciare l'errore sbagliato. Ora l'errore viene
pubblicato QUI, nel punto in cui viene catturato, con tutto il contesto.

Code (tutte sotto .astral_bus/):
  events/          eventi nuovi: un JSON per errore, con contesto completo
  events/claimed/  eventi presi in carico da un subagente di riparazione
  events/done/     eventi chiusi (esito in results/)
  notices/         notifiche BROADCAST: ogni processo Astral attivo le legge
  results/         esito indirizzato al processo che ha generato l'errore
  cursors/         cursore di lettura per singolo processo
  processes/       heartbeat dei processi Astral attivi

Regole: best-effort assoluto (nessuna eccezione al chiamante), scritture atomiche.
"""

import hashlib
import json
import os
import re
import tempfile
import time
import uuid

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BUS = os.path.join(BASE, ".astral_bus")
EVENTS = os.path.join(BUS, "events")
CLAIMED = os.path.join(EVENTS, "claimed")
DONE = os.path.join(EVENTS, "done")
NOTICES = os.path.join(BUS, "notices")
RESULTS = os.path.join(BUS, "results")
CURSORS = os.path.join(BUS, "cursors")
PROC_DIR = os.path.join(BUS, "processes")

NOTICE_TTL_S = 3600.0        # notifiche piu' vecchie di 1h non vengono mostrate
DEDUP_WINDOW_S = 120.0       # stesso errore entro 2 min: non ripubblicato
CLAIM_STALE_S = 900.0        # evento preso in carico e mai chiuso: rimesso in coda
PROC_ALIVE_S = 30.0          # heartbeat oltre questa eta' = processo considerato morto


def _ensure():
    for d in (BUS, EVENTS, CLAIMED, DONE, NOTICES, RESULTS, CURSORS, PROC_DIR):
        try:
            os.makedirs(d, exist_ok=True)
        except Exception:
            pass


def _atomic_write_json(path, payload):
    try:
        d = os.path.dirname(path) or "."
        fd, tmp = tempfile.mkstemp(dir=d, prefix=".bus_", suffix=".tmp")
        try:
            with os.fdopen(fd, "w", encoding="utf-8") as f:
                json.dump(payload, f, ensure_ascii=False, default=str)
            os.replace(tmp, path)
            return True
        except Exception:
            try:
                os.unlink(tmp)
            except Exception:
                pass
            return False
    except Exception:
        return False


def _read_json(path):
    try:
        with open(path, "r", encoding="utf-8") as f:
            return json.load(f)
    except Exception:
        return None


def _list_json(folder):
    out = []
    try:
        names = sorted(os.listdir(folder))
    except Exception:
        return out
    for name in names:
        if not name.endswith(".json"):
            continue
        data = _read_json(os.path.join(folder, name))
        if isinstance(data, dict):
            data.setdefault("_path", os.path.join(folder, name))
            out.append(data)
    return out


def target_from_traceback(error_text, base=None):
    """Ultimo frame .py dentro il progetto: e' il file che va riparato."""
    root = os.path.normcase(os.path.abspath(base or BASE)) + os.sep
    try:
        for candidate in reversed(re.findall(r'File "([^"]+\.py)", line \d+', error_text or "")):
            path = os.path.abspath(candidate)
            if os.path.normcase(path).startswith(root) and os.path.isfile(path):
                return path
    except Exception:
        pass
    return ""


# --- eventi ---------------------------------------------------------------

def publish_event(error_text, error_type="unknown", context="", target_file="",
                  pid=None, slot=None):
    """Pubblica un errore sul bus. Ritorna l'evento (o None se vuoto/fallito)."""
    _ensure()
    text = (error_text or "")[:8000]
    if not text.strip():
        return None
    pid = pid or os.getpid()
    if not target_file:
        target_file = target_from_traceback(text)
    digest = hashlib.sha256(
        (str(error_type) + "|" + text[:2000]).encode("utf-8", "replace")
    ).hexdigest()[:16]
    now = time.time()
    for ev in _list_json(EVENTS):
        if ev.get("digest") == digest and (now - float(ev.get("ts") or 0)) < DEDUP_WINDOW_S:
            return ev  # stesso errore gia' in coda: niente doppioni
    event = {
        "id": "%d_%s" % (int(now * 1000), uuid.uuid4().hex[:8]),
        "ts": now,
        "pid": pid,
        "slot": slot,
        "context": str(context or ""),
        "error_type": str(error_type or "unknown"),
        "error_text": text,
        "target_file": str(target_file or ""),
        "digest": digest,
        "status": "new",
    }
    _atomic_write_json(os.path.join(EVENTS, event["id"] + ".json"), event)
    return event


def pending_events(limit=5):
    """Eventi nuovi, dal piu' vecchio al piu' recente."""
    _ensure()
    events = [e for e in _list_json(EVENTS) if e.get("status", "new") == "new"]
    events.sort(key=lambda e: float(e.get("ts") or 0))
    return events[:max(1, int(limit))]


def claim_event(event_id, owner_pid=None):
    """Presa in carico atomica: sposta l'evento in claimed/. None se gia' preso."""
    if not event_id:
        return None
    _ensure()
    src = os.path.join(EVENTS, str(event_id) + ".json")
    dst = os.path.join(CLAIMED, str(event_id) + ".json")
    try:
        os.replace(src, dst)
    except Exception:
        return None
    event = _read_json(dst) or {}
    event["status"] = "claimed"
    event["claimed_by"] = owner_pid or os.getpid()
    event["claimed_ts"] = time.time()
    _atomic_write_json(dst, event)
    return event


def requeue_stale(max_age_s=CLAIM_STALE_S):
    """Rimette in coda gli eventi presi in carico e mai chiusi (agente morto)."""
    _ensure()
    now = time.time()
    moved = 0
    for ev in _list_json(CLAIMED):
        if now - float(ev.get("claimed_ts") or 0) < max_age_s:
            continue
        event_id = ev.get("id")
        if not event_id:
            continue
        try:
            os.replace(os.path.join(CLAIMED, event_id + ".json"),
                       os.path.join(EVENTS, event_id + ".json"))
            moved += 1
        except Exception:
            pass
    return moved


def close_event(event_id, status="done"):
    """Chiude un evento: lo sposta in done/ con lo stato finale."""
    if not event_id:
        return None
    _ensure()
    for folder in (CLAIMED, EVENTS):
        src = os.path.join(folder, str(event_id) + ".json")
        if not os.path.exists(src):
            continue
        event = _read_json(src) or {}
        event["status"] = status
        event["closed_ts"] = time.time()
        _atomic_write_json(os.path.join(DONE, str(event_id) + ".json"), event)
        try:
            os.remove(src)
        except Exception:
            pass
        return event
    return None


# --- notifiche broadcast --------------------------------------------------

def publish_notice(kind, title, message, origin_pid=None, event_id=None, target_pid=None):
    """Notifica INDIRIZZATA: la legge un solo processo (default: chi l'ha generata).

    Non e' piu' un broadcast: `target_pid` individua il destinatario e
    `read_notices()` scarta tutto cio' che non e' indirizzato a se stesso.
    Se `target_pid` non e' indicato si usa `origin_pid`, cioe' il processo che
    ha generato l'evento: e' lui l'unico a dover sapere com'e' andata.
    """
    _ensure()
    now = time.time()
    if target_pid is None:
        target_pid = origin_pid if origin_pid is not None else os.getpid()
    notice = {
        "id": "%d_%s" % (int(now * 1000), uuid.uuid4().hex[:8]),
        "ts": now,
        "kind": str(kind or "watchdog"),
        "title": str(title or "Astral - Watchdog"),
        "message": str(message or "")[:600],
        "origin_pid": origin_pid or os.getpid(),
        "event_id": event_id,
        "target_pid": target_pid,
    }
    _atomic_write_json(os.path.join(NOTICES, notice["id"] + ".json"), notice)
    return notice


def _cursor_path(pid=None):
    return os.path.join(CURSORS, "%s.json" % (pid or os.getpid()))


def read_notices(pid=None, advance=True):
    """Notifiche indirizzate a QUESTO processo, non ancora viste.

    Le notifiche con `target_pid` diverso dal proprio vengono ignorate: un
    errore nato nella sessione 2 non disturba la sessione 1.
    """
    _ensure()
    pid = pid or os.getpid()
    cursor = _read_json(_cursor_path(pid)) or {}
    since = float(cursor.get("ts") or 0)
    seen = set(cursor.get("ids") or [])
    now = time.time()
    out = []
    for notice in _list_json(NOTICES):
        target = notice.get("target_pid")
        if target is not None and int(target) != int(pid):
            continue
        ts = float(notice.get("ts") or 0)
        if now - ts > NOTICE_TTL_S:
            continue
        if ts < since:
            continue
        if ts == since and notice.get("id") in seen:
            continue
        out.append(notice)
    out.sort(key=lambda n: float(n.get("ts") or 0))
    if advance and out:
        last_ts = float(out[-1].get("ts") or 0)
        ids = [n.get("id") for n in out if float(n.get("ts") or 0) == last_ts]
        _atomic_write_json(_cursor_path(pid), {"ts": last_ts, "ids": ids[-50:]})
    return out


# --- esiti ----------------------------------------------------------------

def write_result(event_id, payload):
    """Esito della riparazione, indirizzato al processo che ha generato l'errore."""
    _ensure()
    data = dict(payload or {})
    data.setdefault("event_id", event_id)
    data.setdefault("ts", time.time())
    return _atomic_write_json(os.path.join(RESULTS, str(event_id) + ".json"), data)


def read_result(event_id):
    return _read_json(os.path.join(RESULTS, str(event_id) + ".json"))


# --- collaborazione processo <-> watchdog ----------------------------------
# Il processo che ha visto l'errore non "spara e dimentica": puo' chiedere in
# che stato e' la riparazione e decidere se proseguire o aspettare il restart.

def event_state(event_id):
    """Stato corrente di un evento, senza consumarlo.

    Ritorna un dict con `phase`:
      pending   in coda, nessuno l'ha ancora preso
      repairing qualcuno l'ha in carico (claim atomico gia' avvenuto)
      done      chiuso, con esito in `result`
      unknown   id inesistente o gia' potato
    """
    _ensure()
    if not event_id:
        return {"phase": "unknown", "event_id": event_id}
    key = str(event_id) + ".json"
    for folder, phase in ((CLAIMED, "repairing"), (EVENTS, "pending")):
        event = _read_json(os.path.join(folder, key))
        if event is not None:
            out = {"phase": phase, "event_id": event_id}
            out.update({k: event.get(k) for k in
                        ("ts", "pid", "context", "error_type", "target_file")})
            return out
    event = _read_json(os.path.join(DONE, key))
    if event is not None:
        return {"phase": "done", "event_id": event_id,
                "status": event.get("status"),
                "result": read_result(event_id) or {},
                "error_type": event.get("error_type"),
                "target_file": event.get("target_file")}
    return {"phase": "unknown", "event_id": event_id}


def wait_for_repair(event_id, timeout=90.0, poll=1.0):
    """Attende l'esito della riparazione entro `timeout` secondi.

    Serve al processo d'origine per scegliere: se entro il timeout la
    riparazione e' riuscita puo' proseguire (o farsi riaprire), altrimenti
    prosegue da solo segnalando l'anomalia. Non solleva mai eccezioni.
    """
    import time as _time
    deadline = _time.time() + max(0.0, float(timeout))
    while True:
        state = event_state(event_id)
        if state.get("phase") == "done":
            return state
        if _time.time() >= deadline:
            state["timed_out"] = True
            return state
        _time.sleep(max(0.2, float(poll)))


def global_cooldown_ok(window_s=60.0):
    """Anti-storm globale: True se nessun riparatore e' partito nell'ultima finestra.

    Sostituisce il cooldown per-processo: con piu' sessioni attive ognuna
    aveva il proprio timer e partivano N riparatori in parallelo.
    """
    _ensure()
    marker = os.path.join(BUS, ".last_repair_ts")
    now = time.time()
    try:
        last = float((_read_json(marker) or {}).get("ts") or 0)
        if last and now - last < float(window_s):
            return False
        tmp = marker + ".%d.tmp" % os.getpid()
        _atomic_write_json(tmp, {"ts": now})
        os.replace(tmp, marker)
        return True
    except Exception:
        return True


# --- processi -------------------------------------------------------------

def register_process(pid=None, slot=None):
    """Heartbeat del processo: serve al crash handler per sapere se altri sono vivi."""
    _ensure()
    pid = pid or os.getpid()
    return _atomic_write_json(os.path.join(PROC_DIR, "%s.json" % pid),
                              {"pid": pid, "slot": slot, "ts": time.time()})


def active_processes(max_age_s=PROC_ALIVE_S):
    """Processi Astral con heartbeat recente."""
    _ensure()
    now = time.time()
    out = []
    for proc in _list_json(PROC_DIR):
        if now - float(proc.get("ts") or 0) <= max_age_s:
            out.append(proc)
    return out


def prune(max_age_s=86400.0):
    """Pulizia periodica: rimuove notifiche/esiti/eventi chiusi piu' vecchi di 24h."""
    _ensure()
    now = time.time()
    for folder in (NOTICES, RESULTS, DONE, PROC_DIR):
        try:
            names = os.listdir(folder)
        except Exception:
            continue
        for name in names:
            path = os.path.join(folder, name)
            try:
                if now - os.path.getmtime(path) > max_age_s:
                    os.remove(path)
            except Exception:
                pass