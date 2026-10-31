# -*- coding: utf-8 -*-
"""Adapter unico di configurazione (P0-2 audit azione ciclo1).

Regola architetturale: ``config.db`` e' l'unica fonte di verita'.
``config.json`` sopravvive solo come *legacy read-only* per non perdere i
valori gia' presenti sulla macchina dell'utente: alla prima lettura un valore
trovato solo nel JSON viene promosso (lazy migration) dentro SQLite e da quel
momento nessun modulo deve piu' aprire il file direttamente.

Tutti i call site devono passare da ``get_value`` / ``set_value``.
"""
import json
import os

from core.core_io import BASE_DIR, log_error
from core.memory_store import get_config, set_config

CONFIG_FILE = os.path.join(BASE_DIR, "config.json")

# Cache del solo file legacy: evita di rileggere il JSON a ogni get.
_LEGACY_CACHE = None


def _legacy():
    """Contenuto di config.json (cache per processo). {} se assente o corrotto."""
    global _LEGACY_CACHE
    if _LEGACY_CACHE is None:
        data = {}
        try:
            if os.path.exists(CONFIG_FILE):
                with open(CONFIG_FILE, "r", encoding="utf-8") as f:
                    loaded = json.load(f)
                if isinstance(loaded, dict):
                    data = loaded
        except Exception as e:
            log_error("config_json_read", e)
        _LEGACY_CACHE = data
    return _LEGACY_CACHE


def _decode(raw):
    """Riporta a dict/list i valori serializzati dall'adapter."""
    if isinstance(raw, str) and raw[:1] in ("{", "["):
        try:
            return json.loads(raw)
        except Exception:
            return raw
    return raw


def get_value(key, default=None):
    """Legge ``key`` da config.db; se assente la promuove dal JSON legacy.

    Contratto di tipo: i contenitori (dict/list) tornano decodificati, gli
    scalari tornano come stringa perche' SQLite li memorizza come TEXT.
    Per i numeri usare ``get_int``.
    """
    raw = get_config(key, None)
    if raw is not None:
        return _decode(raw)
    legacy = _legacy().get(key)
    if legacy is not None:
        # Migrazione lazy: il valore legacy diventa SSOT su SQLite.
        set_config(key, json.dumps(legacy) if isinstance(legacy, (dict, list)) else legacy)
        # Si rilegge dal DB invece di restituire il valore JSON grezzo: cosi'
        # il tipo restituito e' identico alla prima e alla seconda lettura.
        return _decode(get_config(key, default))
    return default


def set_value(key, value):
    """Scrive ``key`` su config.db (SSOT). Ritorna True se il salvataggio riesce."""
    payload = json.dumps(value) if isinstance(value, (dict, list)) else value
    return bool(set_config(key, payload))


def get_int(key, default):
    """Come get_value ma coercizza a intero non negativo; default se non valido."""
    raw = get_value(key, None)
    if raw is None:
        return default
    try:
        val = int(str(raw).strip())
    except Exception:
        return default
    return val if val >= 0 else default


def invalidate_cache():
    """Svuota la cache del file legacy (usato dai test)."""
    global _LEGACY_CACHE
    _LEGACY_CACHE = None