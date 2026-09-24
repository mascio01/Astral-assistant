# -*- coding: utf-8 -*-
# test_benchmark_data.py - Fase 2 (G02, G04): nome dei file LiveBench e
# precedenza dei prezzi. Offline: nessuna rete, solo tempdir.
import glob
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import benchmark_data as B
import price_map as P
from _test_support import Patcher, cleanup, run_all, temp_dir

CATS = {
    "Coding": ["code_generation"],
    "Reasoning": ["theory_of_mind"],
}
TABLE = ("model,code_generation,theory_of_mind\n"
         "Test Model A,50,70\n"
         "Test Model B,20,40\n")


def _sandbox(p):
    """Isola BASE_DIR/state e neutralizza rete e knowledge map."""
    tmp = temp_dir()
    p.set(B, "BASE_DIR", tmp)
    p.set(B, "STATE_FILE", os.path.join(tmp, ".lb_sync_state.json"))
    p.set(B, "_state", {})
    p.set(B, "_invalidate_knowledge_map", lambda: None)

    def fake_get(url, timeout=20, binary=False):
        if url.endswith("table_2026_06_25.csv"):
            return TABLE, 200
        if url.endswith("categories_2026_06_25.json"):
            return json.dumps(CATS), 200
        return None, 404

    p.set(B, "_http_get", fake_get)
    return tmp


def test_g02_fetch_release_salva_nomi_leggibili_dal_lettore():
    # Il writer deve produrre esattamente i nomi che _lb_files() cerca:
    # se diverge, la release scaricata resta invisibile al lettore.
    with Patcher() as p:
        tmp = _sandbox(p)
        assert B._fetch_release("2026-06-25") is True
        tabs = sorted(glob.glob(os.path.join(tmp, ".lb_table_*.csv")))
        cats = sorted(glob.glob(os.path.join(tmp, ".lb_categories_*.json")))
        assert len(tabs) == 1, tabs
        assert len(cats) == 1, cats
        tp, cp = B._lb_files()
        assert tp == tabs[0] and cp == cats[0]
        cleanup(tmp)


def test_g02_release_scaricata_e_leggibile_da_load_lb():
    with Patcher() as p:
        tmp = _sandbox(p)
        assert B._fetch_release("2026-06-25") is True
        modelli, release = B._load_lb()
        assert release == "2026_06_25", release
        assert "Test Model A" in modelli, modelli
        # 50/70 su scala 0-100 -> 5.0/7.0 su scala 0-10.
        assert abs(modelli["Test Model A"]["coding"] - 5.0) < 1e-9
        assert abs(modelli["Test Model A"]["reasoning"] - 7.0) < 1e-9
        cleanup(tmp)


def test_g02_release_incompleta_non_lascia_file_parziali():
    with Patcher() as p:
        tmp = _sandbox(p)

        def solo_table(url, timeout=20, binary=False):
            if url.endswith("table_2026_06_25.csv"):
                return TABLE, 200
            return None, 404

        p.set(B, "_http_get", solo_table)
        assert B._fetch_release("2026-06-25") is False
        assert glob.glob(os.path.join(tmp, ".lb_*")) == []
        cleanup(tmp)


def test_g04_catalogo_non_vince_sulla_tabella_statica():
    # PRICES (listino curato) precede il catalogo OpenRouter: stessa chiave
    # presente in entrambi, vince la tabella statica.
    with Patcher() as p:
        tmp = temp_dir()
        cache = os.path.join(tmp, "catalog.json")
        with open(cache, "w", encoding="utf-8") as f:
            json.dump({"models": [{"id": "deepseek", "prompt": 9.0,
                                    "completion": 9.0}]}, f)
        p.set(P, "_CACHE_FILE", cache)
        assert P.get_price("deepseek") == P.PRICES["deepseek"]
        # ID assente da PRICES: risolve dal catalogo, che espone $/token.
        with open(cache, "w", encoding="utf-8") as f:
            json.dump({"models": [{"id": "solo-catalogo", "prompt": 0.0000005,
                                    "completion": 0.0000015}]}, f)
        assert P.get_price("solo-catalogo") == (0.5, 1.5)
        cleanup(tmp)


def test_g04_documentazione_allineata_alla_precedenza_reale():
    doc = (P.__doc__ or "").lower()
    for voce in ("prices_override.json", "price_map.prices",
                 ".or_models_cache.json"):
        assert voce in doc, voce
    # La precedenza e' documentata nello stesso ordine in cui e' applicata.
    assert doc.index("prices_override.json") \
        < doc.index("price_map.prices") \
        < doc.index(".or_models_cache.json")
    # Il catalogo non fa piu' match per sottostringa: nessuna promessa contraria.
    assert "non si usa il match per sottostringa" in doc


if __name__ == "__main__":
    sys.exit(run_all(globals(), "test_benchmark_data"))
