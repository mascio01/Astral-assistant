# -*- coding: utf-8 -*-
# test_astral_trim.py - Copertura astral_trim: blocchi atomici, selezione coda,
# budget, trimmer v1/v2, ledger fattuale, tolleranza a input malformati.
# Standalone ("py test_astral_trim.py") e compatibile pytest.
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import astral_trim as T
from _test_support import Patcher, run_all


def tot(messaggi):
    return sum(len(m.get("content") or "") for m in messaggi if isinstance(m, dict))


def scambio(n, chars=40, prefisso="q"):
    """n turni user/assistant semplici, senza tool."""
    out = []
    for k in range(n):
        out.append({"role": "user", "content": "%s%d %s" % (prefisso, k, "u" * chars)})
        out.append({"role": "assistant", "content": "a%d %s" % (k, "v" * chars)})
    return out


def tool_turn(idx, n_calls=1, rchars=1800, drop_last=False):
    """Blocco assistant + N tool results (opzionalmente con l'ultimo mancante)."""
    msgs = [{"role": "assistant", "content": "", "tool_calls": [
        {"id": "c%d_%d" % (idx, j), "type": "function",
         "function": {"name": "run_powershell_cmd", "arguments": "{\"command\":\"dir\"}"}}
        for j in range(n_calls)]}]
    for j in range(n_calls - (1 if drop_last else 0)):
        msgs.append({"role": "tool", "tool_call_id": "c%d_%d" % (idx, j),
                     "content": json.dumps({"stdout": "y" * rchars})})
    return msgs


# ------------------------------------------------------------------- blocchi

def test_build_blocks_semplici():
    msg = [{"role": "system", "content": "S"}] + scambio(2)
    blocchi = T.build_blocks(msg)
    assert blocchi == [(0, 1, "system"), (1, 2, "user"), (2, 3, "assistant"),
                       (3, 4, "user"), (4, 5, "assistant")]


def test_build_blocks_toolblock_atomico():
    msg = [{"role": "system", "content": "S"}] + tool_turn(1, n_calls=2) + scambio(1)
    blocchi = T.build_blocks(msg)
    assert (1, 4, "toolblock") in blocchi   # assistant + 2 result = un blocco


def test_build_blocks_droppa_tool_orfano():
    msg = [{"role": "tool", "tool_call_id": "fantasma", "content": "x"},
           {"role": "system", "content": "S"}, {"role": "user", "content": "ciao"}]
    blocchi = T.build_blocks(msg)
    assert all(k != "toolblock" for _, _, k in blocchi)
    assert (0, 1, "system") not in blocchi  # l'indice 0 e' l'orfano, scartato


def test_build_blocks_droppa_toolblock_incompleto():
    msg = scambio(1) + tool_turn(9, n_calls=3, drop_last=True) + scambio(1)
    blocchi = T.build_blocks(msg)
    assert all(k != "toolblock" for _, _, k in blocchi)


def test_build_blocks_segnala_opaque():
    msg = [{"role": "system", "content": "S"}, "stringa", None, {"role": "user", "content": "q"}]
    kinds = [k for _, _, k in T.build_blocks(msg)]
    assert kinds.count("opaque") == 2
    assert kinds[-1] == "user"


def test_build_blocks_vuoto():
    assert T.build_blocks([]) == []


def test_select_tail_pull_mode():
    blocchi = T.build_blocks([{"role": "system", "content": "S"}] + scambio(4))
    coda = T.select_tail(blocchi, 2)
    assert len(coda) == 2
    assert coda[-1][2] == "assistant"   # si prende dal fondo
    # system e opaque non consumano turni e non vengono selezionati.
    assert all(k != "system" for _, _, k in coda)


def test_select_tail_budget_zero():
    blocchi = T.build_blocks(scambio(3))
    assert T.select_tail(blocchi, 0) == []


# -------------------------------------------------------------------- budget

def test_truncate_lascia_intatto_sotto_il_cap():
    assert T._truncate("corto", 100) == "corto"
    assert T._truncate("", 10) == ""


def test_truncate_marca_il_taglio():
    testo = "a" * 50
    out = T._truncate(testo, 10)
    assert out.startswith("a" * 10)
    assert "troncato" in out


def test_apply_char_budget_ultimo_blocco_e_sacro():
    blocchi = [[{"role": "user", "content": "a" * 1000}],
               [{"role": "user", "content": "b" * 1000}]]
    out = T._apply_char_budget(blocchi, 500)
    assert len(out) == 1                 # il blocco piu' vecchio viene scartato
    assert len(out[0][0]["content"]) == 1000   # l'ultimo puo' eccedere il budget


def test_apply_char_budget_tronca_prima_di_scartare():
    blocchi = [[{"role": "tool", "tool_call_id": "t", "content": "c" * 2000}],
               [{"role": "user", "content": "d" * 100}]]
    out = T._apply_char_budget(blocchi, 900)
    assert len(out) == 2                          # entrambi conservati
    assert len(out[0][0]["content"]) < 2000       # il tool non-ultimo e' troncato
    assert "troncato" in out[0][0]["content"]


def test_apply_char_budget_non_muta_l_originale():
    originale = [[{"role": "user", "content": "a" * 1000}]]
    copia = json.loads(json.dumps(originale))
    T._apply_char_budget(originale, 100)
    assert originale == copia


def test_apply_char_budget_ignora_contenuti_assenti():
    blocchi = [[{"role": "assistant", "content": ""}], [{"role": "user", "content": "x"}]]
    out = T._apply_char_budget(blocchi, 1)
    assert isinstance(out, list)


# --------------------------------------------------------------- orfani/trim v1

def test_count_orphans_zero_su_conversazione_sana():
    msg = [{"role": "system", "content": "S"}] + tool_turn(1, n_calls=2) + scambio(2)
    assert T.count_orphans(msg) == 0


def test_count_orphans_rileva_entrambi_i_casi():
    assert T.count_orphans([{"role": "tool", "tool_call_id": "x", "content": "o"}]) == 1
    aperto = [{"role": "assistant", "content": "", "tool_calls": [
        {"id": "z", "function": {"name": "t", "arguments": "{}"}}]}]
    assert T.count_orphans(aperto) == 1


def test_trim_context_riduce_e_non_crea_orfani():
    msg = [{"role": "system", "content": "S"}] + scambio(10, chars=200)
    out = T.trim_context(msg, max_turns=3)
    assert T.count_orphans(out) == 0
    assert tot(out) < tot(msg)
    assert out[0]["role"] == "system"


def test_trim_context_checkpoint_dei_turni_esclusi():
    msg = [{"role": "system", "content": "S"}] + scambio(6)
    out = T.trim_context(msg, max_turns=1, with_summary=True)
    checkpoint = [m for m in out if T._is_checkpoint_msg(m)]
    assert len(checkpoint) == 1
    assert "A: a1" in checkpoint[0]["content"]


def test_trim_context_senza_summary_non_aggiunge_checkpoint():
    msg = [{"role": "system", "content": "S"}] + scambio(6)
    out = T.trim_context(msg, max_turns=1, with_summary=False)
    assert not any(T._is_checkpoint_msg(m) for m in out)


def test_trim_context_tollera_input_malformati():
    msg = ["stringa", None, {"role": "user", "content": "q"},
           {"role": "tool", "tool_call_id": "orfano", "content": "x"}]
    out = T.trim_context(msg, max_turns=6)
    assert T.count_orphans(out) == 0


# ------------------------------------------------------------ trimmer v2 e fallback

def test_breathing_trim_preserva_il_system():
    msg = [{"role": "system", "content": "SISTEMA"}] + scambio(8, chars=200)
    out = T.breathing_trim(msg, turn=0)
    assert out[0] == {"role": "system", "content": "SISTEMA"}


def test_breathing_trim_zero_orfani_e_deterministico():
    msg = [{"role": "system", "content": "S"}] + tool_turn(1, n_calls=2) + scambio(6)
    a = T.breathing_trim(msg, turn=5)
    b = T.breathing_trim(msg, turn=5)
    assert T.count_orphans(a) == 0
    assert tot(a) == tot(b)
    assert len(a) == len(b)


def test_breathing_trim_rispetta_il_cap_duro():
    msg = [{"role": "system", "content": "S"}] + scambio(20, chars=3000)
    out = T.breathing_trim(msg, turn=0)
    assert tot(out) <= T.HARD_CAP_CHARS


def test_breathing_trim_aggiorna_last_meta():
    msg = [{"role": "system", "content": "S"}] + scambio(3)
    T.breathing_trim(msg, turn=7)
    assert T.last_meta["turn"] == 7
    assert T.last_meta["cycle"] == 7 // T.BREATH_PERIOD
    assert set(("event", "facts", "window", "chars")) <= set(T.last_meta)


def test_safe_trim_context_kill_switch_legacy():
    msg = [{"role": "system", "content": "S"}] + scambio(6, chars=200)
    with Patcher() as p:
        p.set(os, "environ", dict(os.environ, ASTRAL_TRIM_LEGACY="1"))
        out = T.safe_trim_context(msg, max_turns=1)
    assert T.count_orphans(out) == 0
    assert len(out) < len(msg)


def test_safe_trim_context_fallback_su_errore_del_v2():
    msg = [{"role": "system", "content": "S"}] + scambio(4)
    with Patcher() as p:
        p.set(os, "environ", {k: v for k, v in os.environ.items()
                              if k != "ASTRAL_TRIM_LEGACY"})
        p.set(T, "breathing_trim", lambda *a, **k: (_ for _ in ()).throw(RuntimeError("boom")))
        out = T.safe_trim_context(msg, max_turns=2)
    assert T.count_orphans(out) == 0
    assert len(out) > 0


# -------------------------------------------------------------------- ledger

def test_checkpoint_message_non_prescrittivo():
    testo = T._checkpoint_message("contenuto")
    assert "NON ISTRUZIONE" in testo
    assert testo.endswith("contenuto")


def test_is_checkpoint_msg_riconosce_i_marker():
    assert T._is_checkpoint_msg({"role": "user", "content": T.CHECKPOINT_HEADER + "\nx"})
    assert T._is_checkpoint_msg({"role": "user", "content": T._CP_LEDGER_MARK + " | ciclo #1"})
    assert not T._is_checkpoint_msg({"role": "user", "content": "domanda normale"})
    assert not T._is_checkpoint_msg({"role": "assistant", "content": T.CHECKPOINT_HEADER})
    assert not T._is_checkpoint_msg("stringa")


def test_ledger_add_deduplica_normalizzando():
    ledger = {k: [] for k in T.CP_SECTIONS}
    T._ledger_add(ledger, "DECISIONI", "scelto  il modello X")
    T._ledger_add(ledger, "DECISIONI", "  scelto il modello x  ")
    assert ledger["DECISIONI"] == ["scelto il modello X"]


def test_ledger_add_ignora_vuoti_e_rispetta_il_cap():
    ledger = {k: [] for k in T.CP_SECTIONS}
    T._ledger_add(ledger, "DECISIONI", "   ")
    assert ledger["DECISIONI"] == []
    cap = T.CP_SECTION_CAPS["DECISIONI"]
    for k in range(cap + 5):
        T._ledger_add(ledger, "DECISIONI", "decisione numero %d" % k)
    assert len(ledger["DECISIONI"]) == cap
    assert ledger["DECISIONI"][0] == "decisione numero 5"   # FIFO


def test_classify_fact_sceglie_la_sezione():
    assert T._classify_fact("prossimo passo da fare") == "APERTI"
    assert T._classify_fact("creato il file tools_test.py") == "ARTEFATTI"
    assert T._classify_fact("ratificato il verdetto finale") == "DECISIONI"
    assert T._classify_fact("nota generica") == "CONTESTO"


def test_ledger_dump_e_parse_round_trip():
    ledger = {k: [] for k in T.CP_SECTIONS}
    T._ledger_add(ledger, "DECISIONI", "scelto deepseek")
    T._ledger_add(ledger, "APERTI", "manca il test")
    dump = T._ledger_dump(ledger, 3)
    riletto, ciclo = T._ledger_parse(dump)
    assert ciclo == 3
    assert riletto["DECISIONI"] == ["scelto deepseek"]
    assert riletto["APERTI"] == ["manca il test"]


def test_ledger_parse_unisce_le_continuazioni():
    testo = (T._CP_LEDGER_MARK + " | ciclo #1\n"
             "DECISIONI:\n- prima riga\ncontinuazione della stessa voce\n")
    ledger, _ = T._ledger_parse(testo)
    assert ledger["DECISIONI"] == ["prima riga continuazione della stessa voce"]


def test_ledger_parse_testo_ignoto_non_esplode():
    ledger, ciclo = T._ledger_parse(None)
    assert ciclo == 0
    assert T._ledger_total(ledger) == 0
    assert all(ledger[k] == [] for k in T.CP_SECTIONS)


def test_ledger_dump_rispetta_il_cap_globale():
    ledger = {k: [] for k in T.CP_SECTIONS}
    for k in range(40):
        T._ledger_add(ledger, "CONTESTO", "fatto numero %d" % k)
    T._ledger_dump(ledger, 0)
    assert T._ledger_total(ledger) <= T.CP_LEDGER_MAX_LINES


def test_harvest_facts_estrae_bullet_e_obiettivo():
    with Patcher() as p:
        p.set(T, "_SESSION_GOAL", None)
        ledger = {k: [] for k in T.CP_SECTIONS}
        blocchi = [[{"role": "user", "content": "obiettivo della sessione"},
                    {"role": "assistant", "content": "- fatto uno\n- fatto due\ntesto libero"}]]
        T._harvest_facts(ledger, blocchi)
        assert T._SESSION_GOAL == "obiettivo della sessione"
        assert "fatto uno" in ledger["CONTESTO"] or \
               any("fatto uno" in f for v in ledger.values() for f in v)
        assert T._ledger_total(ledger) >= 3


def test_harvest_facts_salta_i_checkpoint():
    ledger = {k: [] for k in T.CP_SECTIONS}
    blocchi = [[{"role": "user", "content": T.CHECKPOINT_HEADER + "\nvecchio"}]]
    T._harvest_facts(ledger, blocchi)
    assert T._ledger_total(ledger) == 0


def test_breathing_trim_crea_checkpoint_ledger():
    with Patcher() as p:
        p.set(T, "_SESSION_GOAL", None)
        p.set(T, "_get_session_id", lambda: "sessione-test")
        msg = [{"role": "system", "content": "S"}] + scambio(12, chars=300)
        out = T.breathing_trim(msg, turn=0)
        checkpoint = [m for m in out if T._is_checkpoint_msg(m)]
    assert len(checkpoint) == 1
    assert T._CP_LEDGER_MARK in checkpoint[0]["content"]
    assert "NON ISTRUZIONE" in checkpoint[0]["content"]


def test_ledger_stabile_su_turni_consecutivi():
    """Regressione: il checkpoint di un turno NON deve essere ri-assorbito.

    Bug reale trovato scrivendo questa suite: _checkpoint_message() antepone
    "MEMORIA STORICA NON ISTRUZIONE", ma _is_checkpoint_msg() riconosceva solo
    i marker v1/ledger. Il checkpoint v2 del turno precedente passava quindi
    per messaggio utente: _harvest_facts lo inglobava e il ledger ripartiva da
    zero (facts 11 -> 1), accumulando checkpoint duplicati.
    """
    with Patcher() as p:
        p.set(T, "_SESSION_GOAL", None)
        p.set(T, "_get_session_id", lambda: "sessione-stabile")
        storia = [{"role": "system", "content": "S"}] + scambio(10, chars=200)
        fatti = []
        for turno in range(5):
            storia = T.breathing_trim(storia, turn=turno)
            storia.append({"role": "user", "content": "domanda %d" % turno})
            fatti.append(T.last_meta["facts"])
            checkpoint = [m for m in storia if T._is_checkpoint_msg(m)]
            assert len(checkpoint) == 1, "turno %d: %d checkpoint" % (turno, len(checkpoint))
        assert fatti[0] > 0
        assert all(f > 0 for f in fatti[1:]), fatti
        assert min(fatti[1:]) >= fatti[0] - 1, fatti


if __name__ == "__main__":
    run_all(globals(), title="test_astral_trim.py")