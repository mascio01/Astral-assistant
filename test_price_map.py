# -*- coding: utf-8 -*-
# test_price_map.py - Copertura price_map: match esatto, catalogo, override, costi.
# Standalone: "py test_price_map.py" (exit 1 se un test fallisce) e compatibile pytest.
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import price_map as P
from _test_support import Patcher, cleanup, run_all, temp_dir


def test_tabella_statica_vince_sul_catalogo():
    # "deepseek" esiste sia in PRICES sia come prefisso nel catalogo: vince PRICES.
    assert P.get_price("deepseek") == P.PRICES["deepseek"]


def test_normalizzazione_case_e_spazi():
    atteso = P.PRICES["deepseek"]
    assert P.get_price("DEEPSEEK") == atteso
    assert P.get_price("  deepseek  ") == atteso


def test_modello_sconosciuto_ritorna_none():
    assert P.get_price("modello-che-non-esiste-xyz") is None
    assert P.get_price(None) is None
    assert P.get_price("") is None


def test_catalogo_id_esatto_non_per_sottostringa():
    # Il catalogo non fa piu' match per sottostringa: un ID parziale non risolve.
    with Patcher() as p:
        tmp = temp_dir()
        cache = os.path.join(tmp, "catalog.json")
        with open(cache, "w", encoding="utf-8") as f:
            json.dump({"models": [
                {"id": "vendor/model-one", "prompt": 0.0000005, "completion": 0.0000015},
                {"id": "vendor/model-two", "prompt": 0.000002, "completion": 0.000004},
            ]}, f)
        p.set(P, "_CACHE_FILE", cache)
        assert P.get_price("vendor/model-one") == (0.5, 1.5)
        assert P.get_price("model-one") is None
        assert P.get_price("vendor") is None
        cleanup(tmp)


def test_catalogo_scarta_voci_incomplete():
    with Patcher() as p:
        tmp = temp_dir()
        cache = os.path.join(tmp, "catalog.json")
        with open(cache, "w", encoding="utf-8") as f:
            json.dump({"models": [
                {"id": "ok/model", "prompt": 0.000001, "completion": 0.000002},
                {"id": "senza/completion", "prompt": 0.000001},
                {"prompt": 0.000001, "completion": 0.000002},
                {"id": "nullo/model", "prompt": None, "completion": 0.000002},
            ]}, f)
        p.set(P, "_CACHE_FILE", cache)
        prezzi = P._catalog_prices()
        assert set(prezzi) == {"ok/model"}, prezzi
        cleanup(tmp)


def test_catalogo_illeggibile_non_esplode():
    with Patcher() as p:
        tmp = temp_dir()
        cache = os.path.join(tmp, "catalog.json")
        with open(cache, "w", encoding="utf-8") as f:
            f.write("{ non json")
        p.set(P, "_CACHE_FILE", cache)
        assert P._catalog_prices() == {}
        p.set(P, "_CACHE_FILE", os.path.join(tmp, "assente.json"))
        assert P._catalog_prices() == {}
        cleanup(tmp)


def test_alias_scontato():
    assert P.is_discounted_alias("~vendor/model-latest") is True
    assert P.is_discounted_alias("vendor/model") is False
    assert P.is_discounted_alias("") is False
    assert P.is_discounted_alias(None) is False


def test_cost_usd_noto_e_sconosciuto():
    prezzo = P.PRICES["deepseek"]
    atteso = 1.0 * prezzo[0] + 1.0 * prezzo[1]   # 1M prompt + 1M completion
    assert abs(P.cost_usd("deepseek", 1000000, 1000000) - atteso) < 1e-9
    assert P.cost_usd("deepseek", 0, 0) == 0.0
    assert P.cost_usd("modello-inesistente-xyz", 1000, 1000) is None
    # None/negativi sui token non devono far esplodere il calcolo.
    assert P.cost_usd("deepseek", None, None) == 0.0


def test_all_prices_ordinato_e_coerente():
    righe = P.all_prices()
    assert len(righe) == len(P.PRICES)
    assert [r[0] for r in righe] == sorted(P.PRICES)
    for nome, pin, pout in righe:
        assert (pin, pout) == P.PRICES[nome]


def test_override_utente_caricato_all_import():
    # Il modulo legge prices_override.json accanto a se' stesso: per verificarlo
    # senza toccare la root del progetto si importa una COPIA isolata.
    import importlib.util
    import shutil
    tmp = temp_dir()
    sorgente = os.path.join(os.path.dirname(os.path.abspath(__file__)), "price_map.py")
    copia = os.path.join(tmp, "price_map.py")
    shutil.copyfile(sorgente, copia)
    with open(os.path.join(tmp, "prices_override.json"), "w", encoding="utf-8") as f:
        json.dump({"modello-di-prova": [1.5, 2.5]}, f)
    spec = importlib.util.spec_from_file_location("price_map_isolato", copia)
    modulo = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(modulo)
    assert modulo.PRICES.get("modello-di-prova") == (1.5, 2.5)
    assert modulo.get_price("modello-di-prova") == (1.5, 2.5)
    # L'override non deve contaminare il modulo realmente importato.
    assert "modello-di-prova" not in P.PRICES
    cleanup(tmp)


if __name__ == "__main__":
    run_all(globals(), title="test_price_map.py")