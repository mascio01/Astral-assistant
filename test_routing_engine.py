# -*- coding: utf-8 -*-
# test_routing_engine.py - Copertura routing_engine: classificazione, categorie
# attive, bonus di profilo, score, delta personale, gate/isteresi, audit snello.
# Standalone ("py test_routing_engine.py") e compatibile pytest.
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import routing_engine as R
from _test_support import Patcher, cleanup, run_all, temp_dir

POOL = list(R.ROUTING_POOL)


class Ambiente(object):
    """Isola routing_engine su file temporanei e su benchmark/pool controllati.

    decide_model legge disco e cache globali: senza questo isolamento i test
    dipenderebbero dal profilo personale reale e dal benchmark scaricato.
    """

    def __init__(self, bench=None, pool=None, personal=None, costs=None):
        self.tmp = temp_dir()
        self.personal = personal if personal is not None else {}
        self.audit = []
        self.bench = bench or self._bench_piatto()
        self.pool = pool or POOL
        self.costs = costs or {"breve": {m: 0.001 for m in POOL}, "medio": {}, "lungo": {}}

    def _bench_piatto(self):
        return {m: {"conversazione": 7.0, "codice": 7.0, "affidabilita": 7.0}
                for m in POOL}

    def __enter__(self):
        self.p = Patcher()
        self.p.set(R, "PERSONAL_SCORES_FILE", os.path.join(self.tmp, "personal.json"))
        self.p.set(R, "AUDIT_FILE", os.path.join(self.tmp, "audit.jsonl"))
        # Copia profonda in lettura/scrittura: _save_outcome muta il dict
        # ricevuto, quindi senza copia il fake di persistenza si svuoterebbe.
        self.p.set(R, "_personal_scores", self._load_personal)
        self.p.set(R, "_save_personal_scores", self._save_personal)
        self.p.set(R, "_bench", lambda: self.bench)
        self.p.set(R, "_pool_dinamico", lambda: list(self.pool))
        self.p.set(R, "_telemetry_costs", lambda bench: self.costs)
        self.p.set(R, "_audit", lambda record: self.audit.append(record))
        self.reset()
        return self

    def _load_personal(self):
        return json.loads(json.dumps(self.personal))

    def _save_personal(self, scores):
        self.personal.clear()
        self.personal.update(json.loads(json.dumps(scores)))

    def reset(self, previous=None):
        R._state.update({"previous": previous, "streak": {}, "last": None,
                         "context_signature": "", "context_epoch": 0})

    def __exit__(self, *exc):
        self.p.restore()
        cleanup(self.tmp)
        return False


# --------------------------------------------------------------- classificazione

def test_classify_input_codice():
    cat, conf = R.classify_input("scrivi uno script python che rinomina i file")
    assert cat == "codice"
    assert 0.55 <= conf <= 0.95


def test_classify_input_informativo():
    cat, conf = R.classify_input("che differenza c'e' tra DNS e DHCP")
    assert cat == "informativo"
    assert 0.55 <= conf <= 0.90


def test_classify_input_conversazione():
    assert R.classify_input("ciao, come stai oggi?")[0] == "conversazione"


def test_classify_input_vuoto_o_none_non_esplode():
    assert R.classify_input("")[0] == "conversazione"
    assert R.classify_input(None)[0] == "conversazione"


def test_classify_input_confidenza_cresce_coi_segnali():
    una_sola, c1 = R.classify_input("il bug")
    molte, c2 = R.classify_input("il bug nel codice python: traceback, debug, refactor")
    assert una_sola == molte == "codice"
    assert c2 > c1


def test_classify_input_marker_di_codice_senza_keyword():
    cat, conf = R.classify_input("def foo():\n    return 1")
    assert cat == "codice"
    assert conf < 0.75   # un solo marker: gate non completamente aperto


def test_macro_categoria_mappa_i_gruppi():
    assert R._macro_categoria("codice") == "coding"
    assert R._macro_categoria("conversazione") == "language"
    assert R._macro_categoria("ragionamento") == "reasoning"
    # Categoria senza mappatura: resta se' stessa, senza sollevare eccezioni.
    assert R._macro_categoria("categoria-ignota") == "categoria-ignota"


def test_answer_format_hint_dipende_dalla_categoria():
    assert "conversazionale" in R.answer_format_hint("che differenza c'e'")
    assert "colloquiale" in R.answer_format_hint("ciao come va")
    # Il codice non riceve direttive di formato.
    assert R.answer_format_hint("scrivi uno script python") == ""
    # Categoria esplicita: non viene riclassificata.
    assert R.answer_format_hint("testo qualsiasi", categoria="informativo") != ""


# --------------------------------------------------------------------- profilo

def test_context_features_riconosce_i_segnali():
    f = R._context_features("esegui subito, solo il risultato")
    assert f["action"] is True
    assert f["quick"] is True
    assert f["code"] is False


def test_context_features_continuita_tra_turni():
    storia = [{"role": "user", "content": "parliamo del routing dei modelli"},
              {"role": "assistant", "content": "ok"}]
    f = R._context_features("continuiamo sul routing dei modelli", context=storia)
    assert f["overlap"] > 0.15
    assert f["changed"] is False
    f2 = R._context_features("argomento completamente diverso qui", context=storia)
    assert f2["changed"] is True


def test_context_features_history_chars_e_turns():
    storia = [{"role": "user", "content": "x" * 100}, {"role": "assistant", "content": "y" * 50}]
    f = R._context_features("ciao", context=storia)
    assert f["history_turns"] == 2
    assert f["history_chars"] == 150


def test_tool_phase_riconosce_i_tool_di_codice():
    storia = [{"role": "assistant", "content": "", "tool_calls": [
        {"id": "1", "function": {"name": "apply_code_patch", "arguments": "{}"}}]}]
    assert R._tool_phase(storia) == "codice"
    # Result di tool con marker di codice: riconosciuto anche senza tool_calls.
    storia2 = [{"role": "tool", "tool_call_id": "1", "content": "Traceback (most recent call last)"}]
    assert R._tool_phase(storia2) == "codice"
    assert R._tool_phase([{"role": "user", "content": "ciao"}]) is None
    assert R._tool_phase(None) is None


def test_tool_phase_tollera_messaggi_malformati():
    storia = ["stringa", None, {"tool_calls": ["non-un-dict"]},
              {"tool_calls": [{"function": "non-un-dict"}]}]
    assert R._tool_phase(storia) is None


def test_active_categories_primaria_e_pesi():
    features = R._context_features("scrivi uno script python")
    attive = R._active_categories("scrivi uno script python", features)
    nomi = [n for n, _ in attive]
    assert nomi[0] == "codice"
    assert dict(attive)["codice"] == 1.0
    assert "azione" in nomi          # "scrivi" e' un verbo d'azione
    assert len(nomi) == len(set(nomi))   # nessun doppio conteggio


def test_active_categories_aggiunge_contesto_senza_duplicare():
    features = {"code": True, "tool_phase": "codice", "action": True,
                "reasoning": True, "long": True, "quick": True, "overlap": 0.5}
    attive = R._active_categories("testo", features, primary="conversazione")
    pesi = dict(attive)
    assert pesi["conversazione"] == 1.0
    assert pesi["codice"] == 0.80      # segnale di codice, ma categoria primaria diversa
    assert pesi["tool"] == 0.70
    assert all(0.0 <= w <= 1.0 for w in pesi.values())
    assert len(pesi) == len(attive)


def test_profile_bonus_premia_i_modelli_giusti():
    quick = R._context_features("dammi solo il risultato veloce")
    assert R._profile_bonus("deepseek/deepseek-v4-flash-0731", quick, "conversazione") > 0
    code = R._context_features("scrivi uno script python")
    assert R._profile_bonus("deepseek/deepseek-v4.1-flash", code, "codice") > \
        R._profile_bonus("deepseek/deepseek-v4-flash-0731", code, "codice")
    lungo = R._context_features("analizza tutto in dettaglio")
    assert R._profile_bonus("openai/gpt-5.6-luna", lungo, "codice") > 0


# ----------------------------------------------------------------------- score

def test_score_senza_profilo_personale():
    score, ufficiale, personale, correzione, costo, gen, cat = R._score(
        dict(R.DEFAULT_BENCHMARK), "deepseek/deepseek-v4.1-flash", "codice",
        {}, {}, pool=POOL)
    assert personale is None
    assert correzione == 0.0
    assert gen == 0.0
    assert cat == {"codice": 0.0}
    assert score > 0


def test_score_somma_delta_generale_e_categorie():
    personal = {"deepseek/deepseek-v4.1-flash": {
        "delta_generale": {"delta": 0.2},
        "delta_categorie": {"codice": {"delta": 0.3}}}}
    _, _, _, correzione, _, gen, cat = R._score(
        dict(R.DEFAULT_BENCHMARK), "deepseek/deepseek-v4.1-flash", "codice",
        personal, {}, pool=POOL, active_categories=[("codice", 1.0)])
    assert gen == 0.2
    # Il contributo di categoria e' limitato da PERSONAL_CATEGORY_CAP (0.22),
    # poi la somma con il generale e' limitata da PERSONAL_DELTA_CAP (0.35).
    assert cat == {"codice": R.PERSONAL_CATEGORY_CAP}
    assert abs(correzione - 0.35) < 1e-6


def test_score_applica_i_pesi_anti_doppio_conteggio():
    personal = {"openai/gpt-5.6-luna": {
        "delta_categorie": {"codice": {"delta": 0.20}}}}
    _, _, _, _, _, _, cat = R._score(
        dict(R.DEFAULT_BENCHMARK), "openai/gpt-5.6-luna", "codice", personal, {},
        pool=POOL, active_categories=[("codice", 0.5)])
    assert cat == {"codice": 0.1}    # 0.20 * 0.5


def test_score_cap_sulle_categorie():
    personal = {"openai/gpt-5.6-luna": {
        "delta_generale": {"delta": 5.0},
        "delta_categorie": {"codice": {"delta": 5.0}}}}
    _, _, _, correzione, _, gen, _ = R._score(
        dict(R.DEFAULT_BENCHMARK), "openai/gpt-5.6-luna", "codice", personal, {},
        pool=POOL, active_categories=[("codice", 1.0)])
    assert gen == R.PERSONAL_GENERAL_CAP
    assert abs(correzione) <= R.PERSONAL_DELTA_CAP + 1e-9


def test_score_formato_legacy_piatto_ancora_leggibile():
    personal = {"deepseek/deepseek-v4.1-flash": {"codice": {"delta": 0.15}}}
    _, _, _, correzione, _, _, _ = R._score(
        dict(R.DEFAULT_BENCHMARK), "deepseek/deepseek-v4.1-flash", "codice",
        personal, {}, pool=POOL, active_categories=[("codice", 1.0)])
    assert abs(correzione - 0.15) < 1e-6


def test_score_modello_ignoto_usa_default_senza_eccezioni():
    score, _, _, _, _, _, _ = R._score({}, "modello/ignoto", "codice", {}, {}, pool=POOL)
    assert score > 0


def test_score_preferisce_il_costo_osservato():
    costoso = {"breve": {"openai/gpt-5.6-luna": 0.05}}
    economico = {"breve": {"openai/gpt-5.6-luna": 0.0001}}
    _, _, _, _, costo_caro, _, _ = R._score(
        dict(R.DEFAULT_BENCHMARK), "openai/gpt-5.6-luna", "codice", {}, costoso, pool=POOL)
    _, _, _, _, costo_eco, _, _ = R._score(
        dict(R.DEFAULT_BENCHMARK), "openai/gpt-5.6-luna", "codice", {}, economico, pool=POOL)
    assert costo_caro > costo_eco


# ------------------------------------------------------------ esiti e apprendimento

def test_classify_outcome():
    assert R._classify_outcome({"exit_code": 0, "stdout": "ok"}) == (1.0, 1.0, "useful_success")
    assert R._classify_outcome({"mode": "compile", "error": "SyntaxError"}) == \
        (-0.18, 0.35, "recoverable_error")
    assert R._classify_outcome({"error": "disco pieno"}) == (-1.0, 1.0, "failure")
    assert R._classify_outcome({}) == (0.0, 0.0, "neutral")
    assert R._classify_outcome("non-un-dict") == (0.0, 0.0, "neutral")


def test_record_personal_outcome_senza_evidenze_non_scrive():
    with Ambiente() as amb:
        assert R.record_personal_outcome("deepseek/deepseek-v4.1-flash", "codice") == {}
        assert amb.personal == {}


def test_record_personal_outcome_aggiorna_delta_e_categorie():
    with Ambiente() as amb:
        esito = R.record_personal_outcome(
            "deepseek/deepseek-v4.1-flash", "codice",
            evidence={"input": "scrivi uno script python",
                      "tool_results": [{"exit_code": 0, "stdout": "ok"}]})
        assert esito["generale"]["delta"] > 0
        assert esito["generale"]["label"] == "useful_success"
        entry = amb.personal["deepseek/deepseek-v4.1-flash"]
        assert entry["delta_generale"]["evidenze"] == 1
        assert "codice" in entry["delta_categorie"]


def test_record_personal_outcome_errore_di_test_penalizza_meno():
    with Ambiente() as amb:
        R.record_personal_outcome(
            "deepseek/deepseek-v4.1-flash", "codice",
            evidence={"input": "testa il file",
                      "tool_results": [{"mode": "compile", "error": "SyntaxError"}]})
        delta_test = amb.personal["deepseek/deepseek-v4.1-flash"]["delta_generale"]["delta"]
        amb.personal.clear()
        R.record_personal_outcome(
            "deepseek/deepseek-v4.1-flash", "codice",
            evidence={"input": "esegui il comando",
                      "tool_results": [{"error": "permesso negato"}]})
        delta_ops = amb.personal["deepseek/deepseek-v4.1-flash"]["delta_generale"]["delta"]
        assert delta_test > delta_ops
        assert abs(delta_ops) > abs(delta_test)


def test_record_personal_outcome_cap_sul_delta():
    """Il delta memorizzato e' limitato da PERSONAL_DELTA_CAP.

    Nota: PERSONAL_GENERAL_CAP (0.20) e' applicato in LETTURA da _score, non
    nella scrittura. Il valore su disco puo' quindi essere piu' alto del cap
    generale: e' una scelta deliberata (il generale viene ri-limitato quando
    entra nel punteggio), non un bug, ma va verificata per non perderla.
    """
    with Ambiente() as amb:
        for _ in range(40):
            R.record_personal_outcome(
                "openai/gpt-5.6-luna", "codice",
                evidence={"input": "x",
                          "tool_results": [{"exit_code": 0, "stdout": "ok"}]})
        delta = amb.personal["openai/gpt-5.6-luna"]["delta_generale"]["delta"]
        assert 0 < delta <= R.PERSONAL_DELTA_CAP
        # In lettura il routing lo riporta dentro il cap generale.
        _, _, _, _, _, gen, _ = R._score(
            dict(R.DEFAULT_BENCHMARK), "openai/gpt-5.6-luna", "codice",
            amb.personal, {}, pool=POOL, active_categories=[("codice", 1.0)])
        assert gen == R.PERSONAL_GENERAL_CAP


def test_record_personal_outcome_migra_il_formato_piatto():
    legacy = {"deepseek/deepseek-v4.1-flash": {"codice": {"delta": 0.1}}}
    with Ambiente(personal=legacy) as amb:
        R.record_personal_outcome(
            "deepseek/deepseek-v4.1-flash", "codice",
            evidence={"input": "scrivi codice", "tool_results": [{"exit_code": 0}]})
        entry = amb.personal["deepseek/deepseek-v4.1-flash"]
        assert "delta_categorie" in entry
        assert "codice" in entry["delta_categorie"]
        assert "codice" not in entry    # la chiave piatta non resta in superficie


def test_set_personal_score_legacy_seed():
    with Ambiente() as amb:
        assert R.set_personal_score("deepseek/deepseek-v4.1-flash", "codice", 10.0) is True
        entry = amb.personal["deepseek/deepseek-v4.1-flash"]["delta_categorie"]["codice"]
        assert entry["manual_seed"] is True
        assert entry["delta"] > 0
        assert R.set_personal_score("", "codice", 5.0) is False
        assert R.set_personal_score("modello", "", 5.0) is False


def test_learn_from_user_feedback_solo_dopo_una_risposta():
    with Ambiente() as amb:
        R._state["last"] = {"scelto": "deepseek/deepseek-v4.1-flash",
                            "categoria": "codice", "categorie_attive": [("codice", 1.0)]}
        # Nessun assistant in storico: il feedback non deve penalizzare in anticipo.
        assert R.learn_from_user_feedback("non funziona", context=[]) == {}
        esito = R.learn_from_user_feedback(
            "non funziona", context=[{"role": "assistant", "content": "ecco"}])
        assert esito["generale"]["label"] == "user_negative"
        assert esito["generale"]["delta"] < 0


def test_learn_from_user_feedback_positivo():
    with Ambiente() as amb:
        R._state["last"] = {"scelto": "deepseek/deepseek-v4.1-flash",
                            "categoria": "codice", "categorie_attive": [("codice", 1.0)]}
        esito = R.learn_from_user_feedback(
            "perfetto, funziona", context=[{"role": "assistant", "content": "ecco"}])
        assert esito["generale"]["label"] == "user_positive"
        assert esito["generale"]["delta"] > 0


def test_learn_from_user_feedback_neutro():
    with Ambiente() as amb:
        R._state["last"] = {"scelto": "x", "categoria": "codice"}
        assert R.learn_from_user_feedback(
            "ok grazie, altro tema", context=[{"role": "assistant", "content": "ecco"}]) == {}


# ------------------------------------------------------------ gate, isteresi, audit

def test_gate_confidenza_bassa_resta_sul_modello_corrente():
    with Ambiente() as amb:
        amb.reset(previous=POOL[0])
        with Patcher() as p:
            p.set(R, "classify_input", lambda text: ("conversazione", 0.40))
            rec = R.decide_model("qualsiasi cosa", context=[])
        assert rec["scelto"] == POOL[0]
        assert "fallback conservativo" in rec["motivo"]


def test_isteresi_richiede_due_conferme_nella_fascia_media():
    bench = {m: {"conversazione": 7.0, "codice": 7.0, "affidabilita": 7.0} for m in POOL}
    bench[POOL[1]]["conversazione"] = 9.5
    bench[POOL[1]]["affidabilita"] = 9.5
    with Ambiente(bench=bench) as amb:
        amb.reset(previous=POOL[0])
        with Patcher() as p:
            p.set(R, "classify_input", lambda text: ("conversazione", 0.60))
            primo = R.decide_model("parliamo del tempo", context=[])
            assert primo["scelto"] == POOL[0]
            assert primo["best"] == POOL[1]
            assert "isteresi" in primo["motivo"]
            secondo = R.decide_model("parliamo del tempo", context=[])
            assert secondo["scelto"] == POOL[1]
            assert "switch" in secondo["motivo"]


def test_confidenza_alta_switch_immediato():
    bench = {m: {"conversazione": 7.0, "codice": 7.0, "affidabilita": 7.0} for m in POOL}
    bench[POOL[1]]["conversazione"] = 9.5
    bench[POOL[1]]["affidabilita"] = 9.5
    with Ambiente(bench=bench) as amb:
        amb.reset(previous=POOL[0])
        with Patcher() as p:
            p.set(R, "classify_input", lambda text: ("conversazione", 0.90))
            rec = R.decide_model("x", context=[])
        assert rec["scelto"] == POOL[1]
        assert rec["best"] == POOL[1]


def test_la_fase_tool_forza_la_categoria_codice():
    with Ambiente() as amb:
        amb.reset(previous=POOL[0])
        storia = [{"role": "assistant", "content": "", "tool_calls": [
            {"id": "1", "function": {"name": "apply_code_patch", "arguments": "{}"}}]}]
        rec = R.decide_model("ok procedi", context=storia)
        assert rec["categoria"] == "codice"
        assert rec["confidenza"] >= 0.90
        assert rec["profilo"]["tool_phase"] == "codice"


def test_route_ritorna_solo_il_nome_modello():
    with Ambiente() as amb:
        amb.reset(previous=POOL[0])
        scelto = R.route("ciao", context=[])
        assert scelto in POOL
        assert isinstance(scelto, str)


def test_record_decisione_completo_e_auditato():
    with Ambiente() as amb:
        amb.reset(previous=POOL[0])
        rec = R.decide_model("scrivi uno script python", context=[])
        for chiave in ("ts", "categoria", "confidenza", "profilo", "categorie_attive",
                       "scores", "score_details", "scelto", "best", "margine", "motivo"):
            assert chiave in rec, chiave
        assert rec["categoria"] == "codice"
        assert len(amb.audit) == 1
        assert R._state["last"] is rec


def test_slim_audit_rimuove_il_pool_invariato():
    with Ambiente() as amb:
        R._state["last"] = {"pool": POOL}
        rec = {"pool": POOL,
               "score_details": {m: {"costo_per_1000_token": 0.001} for m in POOL},
               "telemetry_cost_windows": {"breve": {m: 0.001 for m in POOL}}}
        slim = R._slim_audit(rec)
        assert "pool" not in slim
        assert "cost_windows_diff" not in slim   # nessuna divergenza rilevante


def test_slim_audit_salva_solo_le_finestre_che_divergono():
    with Ambiente() as amb:
        R._state["last"] = {"pool": ["altro"]}
        rec = {"pool": POOL,
               "score_details": {m: {"costo_per_1000_token": 0.001} for m in POOL},
               "telemetry_cost_windows": {"breve": {m: 0.002 for m in POOL}}}
        slim = R._slim_audit(rec)
        assert "pool" in slim                     # pool cambiato: va conservato
        assert set(slim["cost_windows_diff"]["breve"]) == set(POOL)


def test_descrivi_ultima_decisione():
    with Ambiente() as amb:
        amb.reset(previous=POOL[0])
        riga = R.descrivi_ultima_decisione("modello/nessuno")
        assert "Routing" in riga                  # nessuna decisione ancora registrata
        R.decide_model("scrivi uno script python", context=[])
        riga2 = R.descrivi_ultima_decisione(POOL[0])
        assert "codice" in riga2
        assert "coding" in riga2


def test_set_budget_tier_modula_i_pesi():
    originale = dict(R.PESI)
    try:
        assert R.set_budget_tier(10.0)["costo"] == 0.55
        assert R.set_budget_tier(30.0)["costo"] == 0.35
        assert R.set_budget_tier(100.0)["costo"] == 0.3
        assert R.set_budget_tier(None)["costo"] == 0.3
        pesi = R.set_budget_tier(10.0)
        assert abs(pesi["qualita"] + pesi["costo"] - 1.0) < 1e-9
    finally:
        R.PESI.update(originale)


def test_telemetry_flag():
    originale = R.get_telemetry_enabled()
    try:
        R.set_telemetry_enabled(False)
        assert R.get_telemetry_enabled() is False
        R.set_telemetry_enabled(True)
        assert R.get_telemetry_enabled() is True
    finally:
        R.set_telemetry_enabled(originale)


if __name__ == "__main__":
    run_all(globals(), title="test_routing_engine.py")