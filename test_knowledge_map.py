# -*- coding: utf-8 -*-
# test_knowledge_map.py - Fase 1 (KM01-KM10 + G03): integrita' della knowledge map.
# Copre: conservazione dei dati curati, migrazione v1, disponibilita' e freschezza
# distinte, corruzione JSON, confinamento delle fonti, writer, ranking, budget.
# Standalone: "py test_knowledge_map.py" e compatibile pytest. Offline, solo tempdir.
import json
import os
import sys
import threading

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import knowledge_map as KM
from _test_support import Patcher, cleanup, run_all, temp_dir


class Sandbox(object):
    """Isola knowledge_map in una cartella temporanea (nessun dato reale toccato)."""

    def __init__(self):
        self.folder = temp_dir("astral_km_")
        self.patcher = Patcher()

    def __enter__(self):
        keys = {"ROOT": self.folder, "CATALOG_JSON": self.path("knowledge_catalog.json"),
                "CATALOG_LOCAL": self.path("knowledge_catalog.local.json"),
                "MAP_JSON": self.path("knowledge_map.json"),
                "MAP_MD": self.path("knowledge_map.md"),
                "LOCK_FILE": self.path(".knowledge_map.lock")}
        for name, value in keys.items():
            self.patcher.set(KM, name, value)
        KM._last.update({"error": "", "refresh_at": 0.0, "warnings": [], "partial": False})
        return self

    def __exit__(self, *exc):
        self.patcher.restore()
        cleanup(self.folder)
        return False

    def path(self, name):
        return os.path.join(self.folder, name)

    def write(self, name, text):
        with open(self.path(name), "w", encoding="utf-8") as stream:
            stream.write(text)
        return self.path(name)

    def write_json(self, name, payload):
        return self.write(name, json.dumps(payload, ensure_ascii=False))

    def snapshot(self):
        with open(self.path("knowledge_map.json"), encoding="utf-8") as stream:
            return json.load(stream)

    def record(self, rid):
        for record in self.snapshot()["records"]:
            if record["id"] == rid:
                return record
        raise AssertionError("record mancante: %s" % rid)

    def touch(self, name, content="dato"):
        self.write(name, content)
        return name


# --- KM01: il refresh conserva record, alias, note e stato personalizzati -----
def test_km01_refresh_conserva_custom_e_alias():
    with Sandbox() as box:
        box.write_json("knowledge_catalog.local.json", {
            "schema_version": KM.SCHEMA_VERSION,
            "records": [
                {"id": "api_pricing", "aliases": ["mio alias"], "status": "archived",
                 "description": "nota personale riscritta"},
                {"id": "note_progetto", "topic": "Note di progetto", "aliases": ["note"],
                 "description": "record creato a mano", "status": "active",
                 "sources": [{"pattern": "price_map.py", "role": "data", "required": False}]},
            ]})
        assert not isinstance(KM.refresh_knowledge_map(), dict), "refresh fallito"
        snapshot = box.snapshot()
        ids = [record["id"] for record in snapshot["records"]]
        assert ids == ["api_pricing", "model_benchmarks", "routing_history", "note_progetto"]
        pricing = box.record("api_pricing")
        assert "mio alias" in pricing["aliases"], "alias personale perso"
        assert "tariffe" in pricing["aliases"], "alias di default perso"
        assert pricing["status"] == "archived", "stato editoriale non conservato"
        assert pricing["semantic"]["description"] == "nota personale riscritta"
        patterns = [item["pattern"] for item in pricing["source_defs"]]
        full = [item["pattern"] for item in box.record("note_progetto")["source_defs"]]
        assert full == ["price_map.py"], "fonti del record personale perse"
        assert ".or_models_cache.json" in patterns, "fonti di default del record noto perse"
        assert box.record("note_progetto")["origin"] == "custom"
        assert box.record("api_pricing")["origin"] == "default"


def test_km01_migrazione_v1_senza_perdita():
    with Sandbox() as box:
        box.write_json("knowledge_map.json", {
            "version": 1, "generated_at": "2026-01-01T00:00:00",
            "records": [
                {"id": "api_pricing", "topic": "Tariffe API e costi modelli",
                 "aliases": ["tariffe"],
                 "semantic": {"description": "nota v1", "review_due": "2026-12-01"}},
                {"id": "custom_v1", "topic": "Personale", "aliases": ["mio"],
                 "status": "archived", "sources": ["benchmark_data.py"],
                 "semantic": {"description": "riga personale"}},
            ]})
        KM.refresh_knowledge_map()
        local = json.load(open(box.path("knowledge_catalog.local.json"), encoding="utf-8"))
        by_id = {record["id"]: record for record in local["records"]}
        assert "custom_v1" in by_id, "record personale v1 perduto"
        assert by_id["custom_v1"]["status"] == "archived"
        assert by_id["api_pricing"]["description"] == "nota v1"
        assert [item["pattern"] for item in by_id["custom_v1"]["sources"]] == ["benchmark_data.py"]
        assert box.record("custom_v1")["status"] == "archived"
        assert box.snapshot()["schema_version"] == KM.SCHEMA_VERSION


def test_km01_versione_futura_non_sovrascritta():
    with Sandbox() as box:
        payload = {"schema_version": 99, "records": [{"id": "x"}]}
        box.write_json("knowledge_map.json", payload)
        KM.refresh_if_needed(force=True)
        assert json.load(open(box.path("knowledge_map.json"), encoding="utf-8")) == payload
        assert not os.path.exists(box.path("knowledge_catalog.local.json"))


# --- KM02: disponibilita' separata dallo stato editoriale ---------------------
def test_km02_disponibilita_esplicita():
    with Sandbox() as box:
        KM.refresh_knowledge_map()
        pricing = box.record("api_pricing")
        assert pricing["status"] == "active" and pricing["availability"] == "missing"
        assert pricing["freshness"] == "unknown"
        missing = {item["pattern"]: item for item in pricing["sources_missing"]}
        assert missing[".or_models_cache.json"]["required"]
        assert missing[".lb_table_*.csv"]["reason"] == "not_found"
        box.touch("benchmark_data.py")
        KM.refresh_if_needed(force=True)
        benchmark = box.record("model_benchmarks")
        assert benchmark["availability"] == "partial", benchmark["availability"]


def test_km02_rimozione_fonte_segnalata():
    with Sandbox() as box:
        box.touch(".routing_personal_scores.json")
        KM.refresh_knowledge_map()
        os.remove(box.path(".routing_personal_scores.json"))
        KM.refresh_if_needed(force=True)
        removed = box.record("routing_history")["auto"]["removed_since_last_snapshot"]
        assert [item["path"] for item in removed] == [".routing_personal_scores.json"]
        assert removed[0]["sha256"], "evidenza della fonte rimossa persa"
        kinds = [item["kind"] for item in KM.diagnostics()["problems"]]
        assert "source_removed" in kinds


# --- KM03: freschezza e revisione distinte ------------------------------------
def test_km03_freschezza_usa_data_fonte_e_policy():
    with Sandbox() as box:
        box.write_json(".or_models_cache.json", {"ts": 100.0, "models": []})
        box.touch(".lb_table_2026_06_25.csv")
        KM.refresh_knowledge_map()
        pricing = box.record("api_pricing")
        assert pricing["freshness"] == "expired", pricing["freshness"]
        or_source = [item for item in pricing["sources"] if item["path"] == ".or_models_cache.json"][0]
        assert or_source["source_updated_at"].startswith("1970-01-01")
        assert or_source["observed_at"] == or_source["source_updated_at"]


def test_km03_hash_cambiato_richiede_revisione():
    with Sandbox() as box:
        box.touch("price_map.py", "uno")
        KM.refresh_knowledge_map()
        data = box.snapshot()
        for record in data["records"]:
            record["semantic"]["reviewed_at"] = "2026-09-01T00:00:00+00:00"
            record["semantic"]["reviewed_sources"] = {
                item["id"]: item["sha256"] for item in record["sources"]}
        box.write_json("knowledge_map.json", data)
        KM.refresh_if_needed(force=True)
        assert box.record("api_pricing")["review"] == "current"
        box.touch("price_map.py", "due")
        KM.refresh_if_needed(force=True)
        pricing = box.record("api_pricing")
        assert pricing["review"] == "needs_review"
        assert any(item.get("changed_since_last_snapshot") for item in pricing["sources"])


# --- KM04: corruzione e struttura invalida ------------------------------------
def test_km04_json_troncato_conserva_originale():
    with Sandbox() as box:
        box.write("knowledge_map.json", "{broken")
        result = KM.knowledge_map_lookup("tariffe")
        assert result.get("count") == 1, result
        backups = [name for name in os.listdir(box.folder) if ".bak" in name]
        assert backups, "copia dell'originale corrotto non conservata"
        assert box.snapshot()["records"][0]["availability"] == "missing"


def test_km04_records_non_lista_non_crasha():
    with Sandbox() as box:
        box.write("knowledge_map.json", json.dumps({"schema_version": 2, "records": None}))
        outcome = KM.refresh_knowledge_map()
        assert not isinstance(outcome, dict), "refresh non ha recuperato lo snapshot invalido"
        assert box.snapshot()["records"]
        box.write("knowledge_map.json", json.dumps({"schema_version": 2, "records": ["bad"]}))
        result = KM.knowledge_map_lookup("tariffe")
        assert result.get("count") == 1, result


# --- KM05: ricerca deterministica e argomenti validati ------------------------
def test_km05_ricerca_non_restituisce_tutto():
    with Sandbox() as box:
        KM.refresh_knowledge_map()
        assert KM.knowledge_map_lookup("a")["count"] == 0
        assert KM.knowledge_map_lookup("!!!").get("error")
        assert KM.knowledge_map_lookup("")["error"] == "Specificare un topic."
        exact = KM.knowledge_map_lookup("tariffe")
        assert [item["id"] for item in exact["results"]] == ["api_pricing"]
        assert "alias esatto" in exact["results"][0]["match_reason"]
        natural = KM.knowledge_map_lookup("quanto costano i modelli")
        assert natural["results"][0]["id"] == "api_pricing", natural
        routing = KM.knowledge_map_lookup("punteggi routing")
        assert routing["results"][0]["id"] == "routing_history"


def test_km05_record_archiviato_escluso():
    with Sandbox() as box:
        box.write_json("knowledge_catalog.local.json", {
            "schema_version": KM.SCHEMA_VERSION,
            "records": [{"id": "api_pricing", "status": "archived"}]})
        KM.refresh_knowledge_map()
        result = KM.knowledge_map_lookup("tariffe")
        assert result["count"] == 0 and result["candidates"] == 0


def test_km05_max_results_validato():
    with Sandbox() as box:
        KM.refresh_knowledge_map()
        for bad in ("bad", 0, 99, True, 2.5):
            result = KM.knowledge_map_lookup("tariffe", bad)
            assert result.get("error"), "argomento %r non validato" % bad
        assert KM.knowledge_map_lookup("tariffe", None)["count"] == 1
        assert KM.knowledge_map_lookup("tariffe", "2")["count"] >= 1


# --- KM06: budget del payload --------------------------------------------------
def test_km06_payload_entro_il_budget():
    with Sandbox() as box:
        records = []
        for index in range(12):
            records.append({"id": "voce_%02d" % index, "topic": "Tariffe voce %02d" % index,
                            "aliases": ["tariffe"],
                            "description": "x" * 20000, "status": "active",
                            "sources": [{"pattern": ".lb_cost_*.csv", "role": "data"}]})
        box.write_json("knowledge_catalog.local.json",
                       {"schema_version": KM.SCHEMA_VERSION, "records": records})
        KM.refresh_knowledge_map()
        response = KM.knowledge_map_lookup("tariffe", 5)
        size = len(json.dumps(response, ensure_ascii=False))
        assert size <= KM.PAYLOAD_BUDGET, size
        assert response["count"] <= 5
        assert len(response["results"][0]["description"]) <= 400
        assert response["truncated"] is True


def test_km06_snapshot_ha_generation_id_e_conteggi():
    with Sandbox() as box:
        KM.refresh_knowledge_map()
        snapshot = box.snapshot()
        assert snapshot["generation_id"] and snapshot["catalog_hash"]
        counts = snapshot["diagnostics"]["counts"]
        assert counts["records"] == 3 and counts["availability"]["missing"] == 3


# --- KM07: confinamento delle fonti -------------------------------------------
def test_km07_pattern_non_autorizzato_rifiutato():
    with Sandbox() as box:
        outside = temp_dir("astral_km_out_")
        try:
            box.write("knowledge_catalog.json", json.dumps({
                "schema_version": KM.SCHEMA_VERSION,
                "records": [{"id": "malevolo", "topic": "Malevolo",
                             "description": "fuori root",
                             "sources": [{"pattern": "../*.json", "role": "data"}]}]}))
            KM.refresh_knowledge_map()
            ids = [record["id"] for record in box.snapshot()["records"]]
            assert "malevolo" not in ids, "pattern fuori root accettato"
            status = KM.diagnostics()
            assert status["catalog"]["state"] == "invalid"
            assert any("non autorizzato" in error for error in status["catalog"]["errors"])
        finally:
            cleanup(outside)


def test_km07_symlink_rifiutato():
    with Sandbox() as box:
        outside = temp_dir("astral_km_out_")
        try:
            secret = os.path.join(outside, "secret.json")
            with open(secret, "w", encoding="utf-8") as stream:
                stream.write('{"ts": 1, "segreti": true}')
            link = box.path(".or_models_cache.json")
            try:
                os.symlink(secret, link)
            except (OSError, NotImplementedError, AttributeError):
                return  # symlink non disponibili: test saltato (Windows senza privilegi)
            KM.refresh_knowledge_map()
            pricing = box.record("api_pricing")
            assert all(item["path"] != ".or_models_cache.json" for item in pricing["sources"])
            rejected = pricing["auto"]["rejected"]
            assert any("symlink" in item["reason"] for item in rejected), rejected
            assert "segreti" not in json.dumps(box.snapshot())
        finally:
            cleanup(outside)


# --- KM08: writer coordinati ---------------------------------------------------
def test_km08_scritture_concorrenti_producono_json_valido():
    with Sandbox() as box:
        box.touch(".or_models_cache.json", json.dumps({"ts": 100.0}))
        errors = []

        def worker():
            try:
                KM.refresh_if_needed(force=True)
            except Exception as exc:  # pragma: no cover - diagnostica del test
                errors.append(exc)

        threads = [threading.Thread(target=worker) for _ in range(4)]
        for thread in threads:
            thread.start()
        for thread in threads:
            thread.join()
        assert not errors, errors
        snapshot = box.snapshot()
        assert snapshot["records"] and snapshot["generation_id"]
        assert not os.path.exists(box.path(".knowledge_map.lock")), "lock non rilasciato"
        leftovers = [name for name in os.listdir(box.folder) if name.endswith(".tmp")]
        assert not leftovers, leftovers


def test_km08_refresh_non_tocca_snapshot_a_catalogo_invalido():
    with Sandbox() as box:
        KM.refresh_knowledge_map()
        before = open(box.path("knowledge_map.json"), encoding="utf-8").read()
        box.write("knowledge_catalog.json", "{rotto")
        KM.refresh_if_needed(force=True)
        assert open(box.path("knowledge_map.json"), encoding="utf-8").read() == before
        assert any(".bak" in name for name in os.listdir(box.folder))


# --- KM09: topic allineati alle fonti reali ------------------------------------
def test_km09_topic_allineati_alle_fonti():
    with Sandbox() as box:
        box.touch("price_map.py")
        box.touch("benchmark_data.py")
        box.touch(".routing_audit.jsonl", "{}\n")
        KM.refresh_knowledge_map()
        pricing = box.record("api_pricing")
        assert "price_map.py" in [item["path"] for item in pricing["sources"]]
        assert "prices_override.json" in [item["pattern"] for item in pricing["sources_missing"]]
        assert "price_map" in pricing["precedence"]
        routing = box.record("routing_history")
        assert "routing_audit" in [item["id"] for item in routing["sources"]]
        assert "audit" not in routing["topic"].lower()
        roles = {item["id"]: item["role"] for item in routing["sources"]}
        assert roles["routing_audit"] == "aggregate"


# --- KM10: diagnostica ---------------------------------------------------------
def test_km10_diagnostica_e_status():
    with Sandbox() as box:
        KM.refresh_knowledge_map()
        info = KM.diagnostics()
        assert info["snapshot"]["available"] and info["snapshot"]["generation_id"]
        assert info["catalog"]["state"] == "ok" and info["counts"]["records"] == 3
        kinds = {item["kind"] for item in info["problems"]}
        assert "missing_required" in kinds
        assert "knowledge map:" in KM.status_line()
        assert info["paths"]["catalog"].endswith("knowledge_catalog.json")


def test_km10_lookup_espone_snapshot_degradato():
    with Sandbox() as box:
        box.write("knowledge_map.json", "{rotto")
        response = KM.knowledge_map_lookup("tariffe")
        assert response["snapshot"]["degraded"] is True
        assert response["snapshot"]["generation_id"]


# --- G03: refresh leggero e watcher -------------------------------------------
def test_g03_refresh_leggero_saltato_se_nulla_e_cambiato():
    with Sandbox() as box:
        KM.refresh_knowledge_map()
        first = box.snapshot()["generation_id"]
        assert KM.refresh_if_needed(force=True)
        KM._last["refresh_at"] = 0.0
        assert KM.refresh_if_needed()
        assert box.snapshot()["generation_id"] != first
        stable = box.snapshot()["generation_id"]
        KM.refresh_if_needed()
        assert box.snapshot()["generation_id"] == stable, "refresh inutile eseguito"
        box.touch("benchmark_data.py")
        KM.refresh_if_needed()
        assert box.snapshot()["generation_id"] != stable, "modifica non rilevata"


def test_g03_invalidazione_esplicita():
    with Sandbox() as box:
        KM.refresh_knowledge_map()
        first = box.snapshot()["generation_id"]
        KM.invalidate()
        assert box.snapshot()["generation_id"] != first


if __name__ == "__main__":
    run_all(globals(), "test_knowledge_map")

