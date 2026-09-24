# -*- coding: utf-8 -*-
# memory_meta.py - Layer meta per il log azioni (Fase 1, design maggioranza consiglio)
# Tabella memory_meta 1:1 con le righe di logs/executions.jsonl (log raw append-only,
# MAI riscritto) + junction memory_tags. Payload per riferimento (src_ref), mai duplicato.

import json
import os
import sqlite3
import threading
import time
from datetime import datetime, timezone

from core_io import BASE_DIR, log_error

# P1: snapshot versionati in logs/, retention ultimi N
LOGS_DIR = os.path.join(BASE_DIR, "logs")
_SNAPSHOT_KEEP = 5

META_DB = os.path.join(BASE_DIR, "memory_meta.db")
_EXEC_LOG = os.path.join(BASE_DIR, "logs", "executions.jsonl")
_RETENTION_DAYS = 30       # retention default; i pinned sono IMMUNI
_SOFTDELETE_DAYS = 7       # dopo quanto un soft-deleted viene spazzato (con snapshot)
_LOCK = threading.RLock()  # rientrante: prune -> snapshot

def _conn():
    c = sqlite3.connect(META_DB, timeout=10)
    c.execute("PRAGMA foreign_keys=ON")
    c.execute("PRAGMA journal_mode=WAL")
    c.execute("PRAGMA busy_timeout=10000")
    return c


_GLOBAL_COUNTER_DDL = """CREATE TABLE IF NOT EXISTS global_counters (
    key TEXT PRIMARY KEY,
    value INTEGER NOT NULL DEFAULT 0,
    updated_utc INTEGER NOT NULL
)"""


def next_global_counter(key, step=1):
    """Incrementa atomicamente un contatore condiviso tra sessioni/processi.

    Restituisce il nuovo valore; in caso di errore ritorna None senza bloccare
    la conversazione. Il chiamante puo' quindi usare un fallback locale.
    """
    try:
        key = str(key or "").strip()
        if not key:
            return None
        step = int(step)
        with _LOCK:
            c = _conn()
            try:
                c.execute("BEGIN IMMEDIATE")
                c.execute(_GLOBAL_COUNTER_DDL)
                row = c.execute(
                    "SELECT value FROM global_counters WHERE key=?", (key,)
                ).fetchone()
                value = int(row[0]) if row else 0
                value += step
                c.execute(
                    "INSERT INTO global_counters(key, value, updated_utc) VALUES(?,?,?) "
                    "ON CONFLICT(key) DO UPDATE SET value=excluded.value, "
                    "updated_utc=excluded.updated_utc",
                    (key, value, int(time.time())),
                )
                c.commit()
                return value
            finally:
                c.close()
    except Exception as e:
        try:
            log_error("memory_meta.global_counter", e)
        except Exception:
            pass
        return None


def init_meta():
    """Schema + indici (idempotente, BEGIN IMMEDIATE per init concorrenziale)."""
    with _LOCK:
        c = _conn()
        try:
            c.execute("BEGIN IMMEDIATE")
            c.execute("""CREATE TABLE IF NOT EXISTS memory_meta (
                src_ref TEXT PRIMARY KEY,
                session_id TEXT,
                tool TEXT,
                event_type TEXT,
                ts_utc INTEGER,
                status TEXT,
                duration_ms REAL,
                pinned INTEGER DEFAULT 0,
                deleted INTEGER DEFAULT 0
            )""")
            c.execute("""CREATE TABLE IF NOT EXISTS memory_tags (
                src_ref TEXT NOT NULL,
                tag TEXT NOT NULL,
                UNIQUE(src_ref, tag)
            )""")
            c.execute("CREATE INDEX IF NOT EXISTS idx_meta_session_ts ON memory_meta(session_id, ts_utc)")
            c.execute("CREATE INDEX IF NOT EXISTS idx_meta_tool_ts ON memory_meta(tool, ts_utc)")
            c.execute("CREATE INDEX IF NOT EXISTS idx_meta_type_ts ON memory_meta(event_type, ts_utc)")
            c.execute("CREATE INDEX IF NOT EXISTS idx_meta_pinned ON memory_meta(pinned) WHERE pinned=1")
            c.execute("CREATE INDEX IF NOT EXISTS idx_tags_tag ON memory_tags(tag)")
            # [FIX G16] Tombstone dei purgati: senza di esso audit_meta rilegge il
            # log raw (append-only, mai toccato) e REIMPORTA le righe purgate,
            # annullando la retention a ogni manutenzione.
            c.execute("""CREATE TABLE IF NOT EXISTS memory_purged (
                src_ref TEXT PRIMARY KEY,
                purged_utc INTEGER
            )""")
            c.commit()
        finally:
            c.close()

def _get_session_id():
    """ID sessione unificato con history_store: ogni istanza di Astral
    (anche multi-instance nella stessa finestra) ha un SESSION_ID proprio,
    cosi' chat, telemetria e usage restano tracciabili separatamente.
    Fallback: file .session_id persistente per compatibilita' storica."""
    try:
        from history_store import SESSION_ID as _hist_sid
        if _hist_sid:
            return _hist_sid
    except Exception:
        pass
    f = os.path.join(BASE_DIR, ".session_id")
    try:
        if os.path.exists(f):
            with open(f, "r", encoding="utf-8") as fh:
                s = fh.read().strip()
                if s:
                    return s
        import uuid
        s = uuid.uuid4().hex[:12]
        with open(f, "w", encoding="utf-8") as fh:
            fh.write(s)
        return s
    except Exception:
        return "unknown"

def attach_event_from_exec(tool, entry, line_no):
    """Hook da exec_logger: meta della riga appena scritta nel JSONL.
    Payload MAI duplicato: solo src_ref ('ln:<N>') + metadati. Atomico."""
    try:
        ts_raw = entry.get("ts") or ""
        try:
            ts_utc = int(datetime.fromisoformat(ts_raw).timestamp())
        except Exception:
            ts_utc = int(time.time())
        sid = _get_session_id()
        etype = "error" if entry.get("error") else "action"
        with _LOCK:
            c = _conn()
            try:
                c.execute("BEGIN IMMEDIATE")
                c.execute("INSERT OR REPLACE INTO memory_meta "
                          "(src_ref, session_id, tool, event_type, ts_utc, status, duration_ms) "
                          "VALUES (?,?,?,?,?,?,?)",
                          ("ln:%d" % line_no, sid, str(tool)[:120], etype, ts_utc,
                           entry.get("status") or "ok", entry.get("duration_ms")))
                c.commit()
            finally:
                c.close()
    except Exception as e:
        log_error("memory_meta.attach", e)


def pin(src_ref):
    with _LOCK:
        c = _conn()
        try:
            c.execute("UPDATE memory_meta SET pinned=1 WHERE src_ref=?", (src_ref,))
            c.commit()
            return c.total_changes > 0
        finally:
            c.close()

def unpin(src_ref):
    with _LOCK:
        c = _conn()
        try:
            c.execute("UPDATE memory_meta SET pinned=0 WHERE src_ref=?", (src_ref,))
            c.commit()
            return c.total_changes > 0
        finally:
            c.close()

def tag(src_ref, tags):
    """Aggiunge tag(s) in junction. `tags` str o lista. Normalizzati lower/64ch."""
    tl = [tags] if isinstance(tags, str) else list(tags or [])
    tl = [t.strip().lower()[:64] for t in tl if t and t.strip()]
    if not tl:
        return False
    with _LOCK:
        c = _conn()
        try:
            c.execute("BEGIN IMMEDIATE")
            c.executemany("INSERT OR IGNORE INTO memory_tags (src_ref, tag) VALUES (?,?)",
                          [(src_ref, t) for t in tl])
            c.commit()
            return True
        finally:
            c.close()

def list_events(session_id=None, tool=None, event_type=None, tag_filter=None,
                include_deleted=False, limit=50, offset=0):
    """API recall: filtri per sessione/tool/tipo/tag, paginato, dal piu' recente.
    I pinned restano visibili anche senza include_deleted."""
    cols = ("src_ref", "session_id", "tool", "event_type", "ts_utc", "status", "duration_ms", "pinned")
    q = ["SELECT m.src_ref, m.session_id, m.tool, m.event_type, m.ts_utc, m.status, m.duration_ms, m.pinned",
         "FROM memory_meta m"]
    params = []
    if tag_filter:
        q.append("JOIN memory_tags g ON g.src_ref=m.src_ref AND g.tag=?")
        params.append(str(tag_filter).strip().lower())
    q.append("WHERE 1=1")
    if session_id:
        q.append("AND m.session_id=?")
        params.append(session_id)
    if tool:
        q.append("AND m.tool=?")
        params.append(tool)
    if event_type:
        q.append("AND m.event_type=?")
        params.append(event_type)
    if not include_deleted:
        q.append("AND (m.deleted=0 OR m.pinned=1)")
    q.append("ORDER BY m.ts_utc DESC LIMIT ? OFFSET ?")
    params.extend([int(limit), int(offset)])
    with _LOCK:
        c = _conn()
        try:
            rows = c.execute("\n".join(q), params).fetchall()
            return [dict(zip(cols, r)) for r in rows]
        finally:
            c.close()

def stats(session_id=None, days=7):
    """Statistiche finestra `days`: conteggi per tool/event_type/status, durata media."""
    since = int(time.time() - days * 86400)
    where = ["ts_utc >= ?", "deleted=0"]
    params = [since]
    if session_id:
        where.append("session_id = ?")
        params.append(session_id)
    w = " AND ".join(where)
    cols = ("src_ref", "session_id", "tool", "event_type", "ts_utc", "status", "duration_ms", "pinned")
    with _LOCK:
        c = _conn()
        try:
            by_tool = dict(c.execute("SELECT tool, COUNT(*) FROM memory_meta WHERE " + w + " GROUP BY tool", params).fetchall())
            by_type = dict(c.execute("SELECT event_type, COUNT(*) FROM memory_meta WHERE " + w + " GROUP BY event_type", params).fetchall())
        finally:
            c.close()
    with _LOCK:
        c = _conn()
        try:
            by_status = dict(c.execute("SELECT status, COUNT(*) FROM memory_meta WHERE " + w + " GROUP BY status", params).fetchall())
            avg_ms = c.execute("SELECT AVG(duration_ms) FROM memory_meta WHERE " + w + " AND duration_ms IS NOT NULL", params).fetchone()[0]
            total = c.execute("SELECT COUNT(*) FROM memory_meta WHERE " + w, params).fetchone()[0]
            return {"total": total, "by_tool": by_tool, "by_type": by_type, "by_status": by_status, "avg_duration_ms": round(avg_ms, 1) if avg_ms is not None else None, "days": days}
        finally:
            c.close()

def soft_delete(src_ref, reason=""):
    """Soft-delete (never hard delete nel flusso normale). I pinned NON si eliminano."""
    with _LOCK:
        c = _conn()
        try:
            cur = c.execute("UPDATE memory_meta SET deleted=1 WHERE src_ref=? AND pinned=0", (src_ref,))
            c.commit()
            return cur.rowcount > 0
        finally:
            c.close()

def snapshot_before_prune(rows, path=None):
    """Snapshot JSONL pre-prune VERSIONATO (prescrizione verdetto).
    Nome con timestamp: mai sovrascritto. Rotazione: conserva gli ultimi _SNAPSHOT_KEEP.
    """
    try:
        if path is None:
            os.makedirs(LOGS_DIR, exist_ok=True)
            path = os.path.join(LOGS_DIR, time.strftime("memory_meta_snapshot_%Y%m%d_%H%M%S.json"))
        tmp = path + ".tmp"
        with open(tmp, "w", encoding="utf-8") as f:
            json.dump(rows, f, ensure_ascii=False, default=str)
        os.replace(tmp, path)  # scrittura atomica
        # rotazione: elimina i piu' vecchi oltre retention
        try:
            olds = sorted(
                (os.path.join(LOGS_DIR, n) for n in os.listdir(LOGS_DIR)
                 if n.startswith("memory_meta_snapshot_") and n.endswith(".json")),
                key=os.path.getmtime)
            for old in olds[:-_SNAPSHOT_KEEP]:
                os.remove(old)
        except OSError:
            pass
        return path
    except Exception as e:
        log_error("memory_meta.snapshot", e)
        return None

def prune(retention_days=_RETENTION_DAYS, max_entries=20000, hard_after_days=_SOFTDELETE_DAYS, snapshot=True):
    """Retention soft-delete + purge. NON tocca il log raw.
    1) soft-delete delle righe oltre retention/limite (pinned IMMUNI);
    2) purge fisico dei soft-deleted piu' vecchi di hard_after_days, con snapshot JSONL pre-purge.
    Ritorna {pinned, soft_deleted, purged, snapshot}.
    """
    now = int(time.time())
    out = {"pinned": 0, "soft_deleted": 0, "purged": 0, "snapshot": None}
    # P1: se in init_mode NON ci sono righe da purgare, snapshot superfluo (niente rischio di perdita)
    with _LOCK:
        c = _conn()
        try:
            out["pinned"] = c.execute("SELECT COUNT(*) FROM memory_meta WHERE pinned=1").fetchone()[0]
            cutoff = now - retention_days * 86400
            cur = c.execute("UPDATE memory_meta SET deleted=1 WHERE deleted=0 AND pinned=0 AND ts_utc < ?", (cutoff,))
            out["soft_deleted"] = cur.rowcount
            # purge: solo soft-deleted che non sono pinnati e oltre hard_after_days
            if hard_after_days is not None:
                hdays = now - hard_after_days * 86400
                rows = c.execute("SELECT src_ref, session_id, tool, event_type, ts_utc, status, duration_ms, pinned FROM memory_meta WHERE deleted=1 AND pinned=0 AND ts_utc < ?", (hdays,)).fetchall()
                if rows and snapshot:
                    spath = snapshot_before_prune(rows)
                    if spath:
                        out["snapshot"] = spath
                for r in rows:
                    # [FIX G16] Tombstone PRIMA della cancellazione fisica: segna
                    # la riga come purgata in modo che audit_meta non la reimporti.
                    c.execute("INSERT OR REPLACE INTO memory_purged (src_ref, purged_utc) VALUES (?,?)", (r[0], now))
                    c.execute("DELETE FROM memory_tags WHERE src_ref=? AND NOT EXISTS (SELECT 1 FROM memory_meta mm WHERE mm.src_ref=memory_tags.src_ref AND mm.pinned=1)", (r[0],))
                    c.execute("DELETE FROM memory_meta WHERE src_ref=? AND pinned=0", (r[0],))
                    out["purged"] += 1
            c.commit()
            return out
        finally:
            c.close()

def _migrate_v1(src_ref):
    """Migrazione backfill (versionata, idempotente): classifica il log raw in meta."""
    try:
        with open(_EXEC_LOG, "r", encoding="utf-8") as f:
            for i, line in enumerate(f, 1):
                if "ln:%d" % i == src_ref:
                    e = json.loads(line)
                    break
            else:
                return False
        ts_raw = e.get("ts") or ""
        try:
            ts_utc = int(datetime.fromisoformat(ts_raw).timestamp())
        except Exception:
            ts_utc = int(time.time())
        with _LOCK:
            c = _conn()
            try:
                c.execute("BEGIN IMMEDIATE")
                c.execute("INSERT OR REPLACE INTO memory_meta "
                          "(src_ref, session_id, tool, event_type, ts_utc, status, duration_ms) VALUES (?,?,?,?,?,?,?)",
                          ("ln:%d" % i, "migrated", str(e.get("tool"))[:120],
                           "error" if e.get("error") else "action", ts_utc,
                           e.get("status") or "ok", e.get("duration_ms")))
                c.commit()
                return True
            finally:
                c.close()
    except Exception as ex:
        log_error("memory_meta.migrate", ex)
        return False

def bootstrap_meta():
    """Init schema + backfill idempotente delle righe raw non ancora in meta."""
    init_meta()
    try:
        if not os.path.exists(_EXEC_LOG):
            return {"scanned": 0, "imported": 0, "skipped": 0, "error": 0}
        with _LOCK:
            c = _conn()
            try:
                have = {r[0] for r in c.execute("SELECT src_ref FROM memory_meta").fetchall()}
            finally:
                c.close()
        with open(_EXEC_LOG, "r", encoding="utf-8") as f:
            total = sum(1 for _ in f)
        missing = ["ln:%d" % i for i in range(1, total + 1) if ("ln:%d" % i) not in have]
        for ref in missing:
            _migrate_v1(ref)
        return {"scanned": total, "imported": len(missing),
                "skipped": total - len(missing), "error": 0}
    except Exception as e:
        log_error("memory_meta.bootstrap", e)
        return {"scanned": 0, "imported": 0, "skipped": 0, "error": 1}


def audit_meta(watermark=None):
    """Audit raw/meta (P0): drift raw vs meta, watermark reimport, orfani meta.
    - reimporta le righe raw non ancora classificate (protezione gap post-prune);
    - watermark opzionale persistito in logs/.meta_watermark.json (mai tocca il raw);
    - conta i meta orfani (src_ref non piu' presente nel raw: solo diagnostica).
    Ritorna dict con raw_lines, meta_rows, missing_meta, reimported, orphan_meta, watermark.
    """
    out = {"raw_lines": 0, "meta_rows": 0, "missing_meta": 0,
           "orphan_meta": 0, "reimported": 0, "watermark": watermark}
    try:
        with _LOCK:
            c = _conn()
            try:
                out["meta_rows"] = c.execute("SELECT COUNT(*) FROM memory_meta").fetchone()[0]
                meta_refs = {r[0] for r in c.execute("SELECT src_ref FROM memory_meta").fetchall()}
                # [FIX G16] I purgati intenzionalmente NON vanno ri-reimportati.
                try:
                    purged_refs = {r[0] for r in c.execute(
                        "SELECT src_ref FROM memory_purged").fetchall()}
                except Exception:
                    purged_refs = set()
            finally:
                c.close()
        raw_refs = set()
        if os.path.exists(_EXEC_LOG):
            with open(_EXEC_LOG, "r", encoding="utf-8") as f:
                for i, _line in enumerate(f, 1):
                    raw_refs.add("ln:%d" % i)
        out["raw_lines"] = len(raw_refs)
        missing = raw_refs - meta_refs - purged_refs
        out["missing_meta"] = len(missing)
        out["skipped_purged"] = len((raw_refs - meta_refs) & purged_refs)
        if missing:
            for ref in sorted(missing, key=lambda r: int(r.split(":")[1])):
                if _migrate_v1(ref):
                    out["reimported"] += 1
        out["orphan_meta"] = len(meta_refs - raw_refs)
        wm_path = os.path.join(BASE_DIR, "logs", ".meta_watermark.json")
        if watermark is not None:
            os.makedirs(os.path.dirname(wm_path), exist_ok=True)
            with open(wm_path, "w", encoding="utf-8") as f:
                json.dump({"ts": int(time.time()), "raw_lines": out["raw_lines"]}, f)
        elif os.path.exists(wm_path):
            try:
                with open(wm_path, "r", encoding="utf-8") as f:
                    out["watermark"] = json.load(f)
            except Exception:
                out["watermark"] = None
        return out
    except Exception as e:
        log_error("memory_meta.audit", e)
        out["error"] = str(e)
        return out


# ---------------------------------------------------------------------------
# Fase 3 - UsageTracker: contabilita' token per sessione/modello (SILENZIOSO).
# Tabella usage_log nello stesso DB (memory_meta.db). Nessun output a schermo:
# record best-effort, mai solleva eccezioni, mai blocca il flusso conversazione.
# ---------------------------------------------------------------------------

_USAGE_DDL = """CREATE TABLE IF NOT EXISTS usage_log (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_id TEXT,
    model TEXT,
    prompt_tokens INTEGER,
    completion_tokens INTEGER,
    total_tokens INTEGER,
    ts_utc INTEGER
)"""
_USAGE_IDX = "CREATE INDEX IF NOT EXISTS idx_usage_session_ts ON usage_log(session_id, ts_utc)"
_usage_ready = False


def _uget(usage, key, default=0):
    """Lettura campo da usage OpenAI-SDK (attributo) o dict: robustezza doppia."""
    if isinstance(usage, dict):
        return usage.get(key, default)
    return getattr(usage, key, default)


def record_usage(model, usage, session_id=None):
    """Registra i token di una risposta (response.usage). Best-effort: mai solleva.
    Silenzioso per design (Fase 3): nessun print, nessun console output."""
    try:
        if usage is None:
            return
        global _usage_ready
        pt = int(_uget(usage, "prompt_tokens", 0) or 0)
        ct = int(_uget(usage, "completion_tokens", 0) or 0)
        tt = int(_uget(usage, "total_tokens", 0) or (pt + ct))
        sid = session_id or _get_session_id()
        with _LOCK:
            c = _conn()
            try:
                if not _usage_ready:
                    c.execute(_USAGE_DDL)
                    c.execute(_USAGE_IDX)
                    c.commit()
                    _usage_ready = True
                c.execute("INSERT INTO usage_log "
                          "(session_id, model, prompt_tokens, completion_tokens, total_tokens, ts_utc) "
                          "VALUES (?,?,?,?,?,?)",
                          (sid, str(model or "unknown")[:120], pt, ct, tt, int(time.time())))
                c.commit()
            finally:
                c.close()
    except Exception as e:
        try:
            log_error("memory_meta.usage", e)
        except Exception:
            pass


def usage_stats(days=7, session_id=None):
    """Aggregate token per finestra `days` (API di query, NON stampate).
    Ritorna {'days', 'by_model': [...], 'totals': {...}}."""
    since = int(time.time() - days * 86400)
    sid_clause = " AND session_id=?" if session_id else ""
    params = [since] + ([session_id] if session_id else [])
    with _LOCK:
        c = _conn()
        try:
            c.execute(_USAGE_DDL)
            c.commit()
            rows = c.execute(
                "SELECT model, COUNT(*), SUM(prompt_tokens), SUM(completion_tokens), SUM(total_tokens) "
                "FROM usage_log WHERE ts_utc >= ?" + sid_clause + " GROUP BY model",
                params).fetchall()
            tot = c.execute(
                "SELECT SUM(prompt_tokens), SUM(completion_tokens), SUM(total_tokens) "
                "FROM usage_log WHERE ts_utc >= ?" + sid_clause,
                params).fetchone()
            return {
                "days": days,
                "by_model": [{"model": r[0], "calls": r[1],
                              "prompt_tokens": r[2] or 0, "completion_tokens": r[3] or 0,
                              "total_tokens": r[4] or 0} for r in rows],
                "totals": {"prompt_tokens": tot[0] or 0,
                           "completion_tokens": tot[1] or 0,
                           "total_tokens": tot[2] or 0},
            }
        finally:
            c.close()


def usage_rows(days=7, session_id=None):
    """Righe raw usage per finestra `days` (export CSV / report costi P3).
    Ordinate per ts_utc crescente. Ritorna lista di dict."""
    since = int(time.time() - days * 86400)
    sid_clause = " AND session_id=?" if session_id else ""
    params = [since] + ([session_id] if session_id else [])
    with _LOCK:
        c = _conn()
        try:
            c.execute(_USAGE_DDL)
            c.commit()
            rows = c.execute(
                "SELECT ts_utc, session_id, model, prompt_tokens, completion_tokens, total_tokens "
                "FROM usage_log WHERE ts_utc >= ?" + sid_clause + " ORDER BY ts_utc ASC",
                params).fetchall()
            return [{"ts_utc": r[0], "session_id": r[1], "model": r[2],
                     "prompt_tokens": r[3] or 0, "completion_tokens": r[4] or 0,
                     "total_tokens": r[5] or 0} for r in rows]
        finally:
            c.close()
