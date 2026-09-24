# -*- coding: utf-8 -*-
# test_memory_fixes.py - Fase 4 (G14, G16, G17, G20).
# Offline: solo tempdir e monkeypatch, nessuna rete, nessun processo reale.
import os
import sqlite3
import sys
import time
from datetime import datetime

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import exec_logger as EL
import memory_meta as MM
import memory_store as MS
import task_tracker as TT
from _test_support import Patcher, cleanup, run_all, temp_dir


def _iso(ts):
    return datetime.fromtimestamp(ts).isoformat(timespec="seconds")


# ------------------------------------------------------------------ G14 ---
def test_g14_perdita_meta_non_silenziosa():
    """Se l'aggiornamento meta fallisce, la riga raw resta ma la perdita NON
    deve sparire nel silenzio: va tracciata per il recupero (audit_meta)."""
    d = temp_dir()
    try:
        logp = os.path.join(d, "exec.jsonl")
        with Patcher() as p:
            p.set(EL, "_LOG_DIR", d)
            p.set(EL, "_LOG_PATH", logp)
            p.set(EL, "_line_no", 0)
            p.set(EL, "_pending_meta", [])

            def boom(*a, **k):
                raise RuntimeError("meta non disponibile")

            p.set(EL, "attach_event_from_exec", boom)
            EL.log_execution("scan_dir", {"x": 1})  # non deve sollevare
            assert os.path.exists(logp), "la riga raw deve comunque essere scritta"
            assert EL._pending_meta, "la perdita meta non e' stata tracciata"
    finally:
        cleanup(d)


# ------------------------------------------------------------------ G16 ---
def test_g16_prune_non_reimportato():
    """Una riga purgata NON deve tornare viva al successivo audit_meta
    (il log raw e' append-only e la fa 'mancare' di nuovo)."""
    d = temp_dir()
    try:
        db = os.path.join(d, "meta.db")
        raw = os.path.join(d, "exec.jsonl")
        with Patcher() as p:
            p.set(MM, "META_DB", db)
            p.set(MM, "_EXEC_LOG", raw)
            p.set(MM, "LOGS_DIR", d)
            MM.init_meta()
            with open(raw, "w", encoding="utf-8") as f:
                f.write(json_line({"ts": _iso(time.time() - 40 * 86400),
                                   "tool": "scan_dir", "status": "ok"}))
            MM.bootstrap_meta()
            out = MM.prune(retention_days=0, max_entries=0,
                           hard_after_days=0, snapshot=False)
            assert out["purged"] >= 1, out
            c = sqlite3.connect(db)
            try:
                n = c.execute("SELECT COUNT(*) FROM memory_purged").fetchone()[0]
            finally:
                c.close()
            assert n >= 1, "manca il tombstone dei purgati"
            au = MM.audit_meta()
            assert au["reimported"] == 0, "retention annullata dal reimport: %s" % au
            assert au.get("skipped_purged", 0) >= 1, au
    finally:
        cleanup(d)


def json_line(obj):
    import json
    return json.dumps(obj) + "\n"


# ------------------------------------------------------------------ G17 ---
def test_g17_fallimento_azzera_flag():
    """Un fallimento test NON deve lasciare in vita il test_passed=1 di un giro
    precedente: il flag va azzerato (niente verde stale)."""
    d = temp_dir()
    try:
        db = os.path.join(d, "tasks.db")
        with Patcher() as p:
            p.set(TT, "TASKS_DB", db)
            p.set(TT, "_git", lambda *a, **k: (True, ""))
            TT.init_tasks()
            TT.create_task(1, "titolo")
            p.set(TT, "_run_tests", lambda: (True, ["ok"]))
            assert TT.mark_test_passed(1, run_tests=True)["ok"] is True
            assert _test_passed(db, 1) == 1
            p.set(TT, "_run_tests", lambda: (False, ["boom"]))
            r = TT.mark_test_passed(1, run_tests=True)
            assert r["ok"] is False
            assert _test_passed(db, 1) == 0, "il verde stale e' sopravvissuto"
    finally:
        cleanup(d)


def _test_passed(db, task_id):
    c = sqlite3.connect(db)
    try:
        return c.execute("SELECT test_passed FROM tasks WHERE id=?", (task_id,)).fetchone()[0]
    finally:
        c.close()

# ------------------------------------------------------------------ G20 ---
def test_g20_manutenzione_esplicita():
    """La manutenzione deve ripulire recall.db/checkpoints.db su richiesta,
    non solo come effetto collaterale del salvataggio."""
    d = temp_dir()
    try:
        old = time.time() - 40 * 86400
        with Patcher() as p:
            p.set(MS, "RECALL_DB", os.path.join(d, "recall.db"))
            p.set(MS, "CHECKPOINT_DB", os.path.join(d, "checkpoints.db"))
            p.set(MS, "TELEMETRY_DB", os.path.join(d, "telemetry.db"))
            p.set(MS, "CONFIG_DB", os.path.join(d, "config.db"))
            MS.init_db()
            c = sqlite3.connect(MS.RECALL_DB)
            c.execute("INSERT INTO recall (hash, data, ts, preview) VALUES (?,?,?,?)",
                      ("old1", b"x", old, "p"))
            c.commit()
            c.close()
            c = sqlite3.connect(MS.CHECKPOINT_DB)
            c.execute("INSERT INTO checkpoints (ts, summary, words) VALUES (?,?,?)",
                      (old, "vecchio", "vecchio"))
            c.commit()
            c.close()
            out = MS.maintain_memory()
            assert out["recall"] >= 1, out
            assert out["checkpoints"] >= 1, out
            c = sqlite3.connect(MS.RECALL_DB)
            n = c.execute("SELECT COUNT(*) FROM recall").fetchone()[0]
            c.close()
            assert n == 0, "recall scaduto non rimosso"
    finally:
        cleanup(d)


if __name__ == "__main__":
    run_all(dict(globals()), title="test_memory_fixes")

