# -*- coding: utf-8 -*-
# test_config_store.py - Adapter unico di configurazione (P0-2 audit azione ciclo1).
# Standalone: "py test_config_store.py" (exit 1 se un test fallisce) e compatibile pytest.
# --- bootstrap path (file spostati in sottocartelle) ---
import os as _os_boot, sys as _sys_boot
_ROOT_BOOT = _os_boot.path.dirname(_os_boot.path.dirname(_os_boot.path.abspath(__file__)))
if _ROOT_BOOT not in _sys_boot.path:
    _sys_boot.path.insert(0, _ROOT_BOOT)
# --- fine bootstrap ---
import json
import os
import sqlite3
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from core import config_store as C
from core import memory_store as M
from _test_support import Patcher, cleanup, run_all, temp_dir


def _isolated():
    """Isola DB e JSON legacy in una cartella temporanea. Ritorna (tmp, patcher)."""
    tmp = temp_dir()
    p = Patcher()
    p.set(M, "CONFIG_DB", os.path.join(tmp, "config.db"))
    # init_db() tocca anche gli altri DB: vanno isolati per non sporcare la root.
    p.set(M, "RECALL_DB", os.path.join(tmp, "recall.db"))
    p.set(M, "CHECKPOINT_DB", os.path.join(tmp, "checkpoint.db"))
    p.set(M, "TELEMETRY_DB", os.path.join(tmp, "telemetry.db"))
    p.set(C, "CONFIG_FILE", os.path.join(tmp, "config.json"))
    M._WAL_READY.clear()
    C.invalidate_cache()
    # Schema minimo: senza init_db la tabella config non esiste nel DB isolato.
    M.init_db()
    return tmp, p


def _restore(p, tmp):
    p.restore()
    M._WAL_READY.clear()
    C.invalidate_cache()
    cleanup(tmp)


def test_set_e_get_su_db():
    tmp, p = _isolated()
    try:
        assert C.set_value("chiave", "valore") is True
        assert C.get_value("chiave") == "valore"
        # Il valore vive su SQLite, non su file JSON.
        assert not os.path.exists(C.CONFIG_FILE)
    finally:
        _restore(p, tmp)


def test_default_quando_assente():
    tmp, p = _isolated()
    try:
        assert C.get_value("manca") is None
        assert C.get_value("manca", "fallback") == "fallback"
        assert C.get_int("manca", 42) == 42
    finally:
        _restore(p, tmp)


def test_migrazione_lazy_da_json_legacy():
    tmp, p = _isolated()
    try:
        with open(C.CONFIG_FILE, "w", encoding="utf-8") as f:
            json.dump({"tool_token_budget": 1234}, f)
        C.invalidate_cache()
        # Prima lettura: il valore arriva dal JSON (scalare -> stringa)...
        assert C.get_int("tool_token_budget", 0) == 1234
        # ...e viene promosso su config.db (SSOT).
        assert M.get_config("tool_token_budget") == "1234"
        # Da qui in poi il DB vince anche se il JSON cambia.
        with open(C.CONFIG_FILE, "w", encoding="utf-8") as f:
            json.dump({"tool_token_budget": 9999}, f)
        C.invalidate_cache()
        assert C.get_int("tool_token_budget", 0) == 1234
    finally:
        _restore(p, tmp)


def test_dict_serializzato_e_riletto():
    tmp, p = _isolated()
    try:
        dati = {"a": {"b": 1}, "c": [1, 2]}
        assert C.set_value("struttura", dati) is True
        assert C.get_value("struttura") == dati
    finally:
        _restore(p, tmp)


def test_get_int_coercizione_e_rifiuto_negativi():
    tmp, p = _isolated()
    try:
        C.set_value("buono", "7")
        C.set_value("sporco", "abc")
        C.set_value("negativo", "-3")
        assert C.get_int("buono", 0) == 7
        assert C.get_int("sporco", 5) == 5
        assert C.get_int("negativo", 5) == 5
    finally:
        _restore(p, tmp)


def test_wal_attivo_su_config_db():
    tmp, p = _isolated()
    try:
        M.init_db()
        conn = sqlite3.connect(M.CONFIG_DB)
        try:
            modo = conn.execute("PRAGMA journal_mode").fetchone()[0]
            assert modo.lower() == "wal"
            conn.execute("INSERT OR REPLACE INTO config (key, value) VALUES ('k','v')")
            conn.commit()
            riga = conn.execute("SELECT value FROM config WHERE key='k'").fetchone()
            assert riga[0] == "v"
        finally:
            conn.close()
        # Il PRAGMA e' persistente sul file: vale anche per connessioni esterne.
        conn2 = sqlite3.connect(M.CONFIG_DB)
        try:
            modo2 = conn2.execute("PRAGMA journal_mode").fetchone()[0]
            assert modo2.lower() == "wal"
        finally:
            conn2.close()
    finally:
        _restore(p, tmp)


if __name__ == "__main__":
    run_all(globals(), title="test_config_store.py")