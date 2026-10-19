# -*- coding: utf-8 -*-
# test_prompt_cache.py - Copertura cache del system prompt fisso e lettura
# bootstrap senza effetto collaterale su disco.
# Standalone ("py test_prompt_cache.py") e compatibile pytest.
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import llm_core as L
import selfmap as S
from _test_support import Patcher, run_all


def _fresh_cache(p):
    p.set(L, "_STATIC_PROMPT_CACHE", {"signature": None, "builder": None, "text": ""})


def test_cached_builder_chiamato_una_volta():
    """Due letture consecutive dello stesso contesto -> un solo build."""
    chiamate = []
    with Patcher() as p:
        _fresh_cache(p)
        p.set(L, "_build_grounded_system_prompt",
              lambda: chiamate.append(1) or "REGOLE")
        p.set(S, "context_signature", lambda: (("identity.md", 1, 1),))
        primo = L._system_prompt_static()
        secondo = L._system_prompt_static()
    assert len(chiamate) == 1, chiamate
    assert primo == secondo and "REGOLE" in primo


def test_cache_invalidata_da_firma_diversa():
    """Cambio di firma (file di contesto modificato) -> ricostruzione."""
    chiamate = []
    firme = [("a",), ("b",)]
    with Patcher() as p:
        _fresh_cache(p)
        p.set(L, "_build_grounded_system_prompt",
              lambda: chiamate.append(1) or "S")
        p.set(S, "context_signature", lambda: firme.pop(0) if firme else ("b",))
        L._system_prompt_static()
        L._system_prompt_static()
    assert len(chiamate) == 2, chiamate


def test_cache_invalidata_da_builder_sostituito():
    """Monkeypatch del builder (pattern dei test) -> niente cache stantia."""
    with Patcher() as p:
        _fresh_cache(p)
        p.set(S, "context_signature", lambda: (("fisso",),))
        p.set(L, "_build_grounded_system_prompt", lambda: "PRIMO")
        assert "PRIMO" in L._system_prompt_static()
        p.set(L, "_build_grounded_system_prompt", lambda: "SECONDO")
        assert "SECONDO" in L._system_prompt_static()


def test_firma_assente_non_avvelena_la_cache():
    """Se la firma non e' disponibile si ricostruisce, senza memorizzare."""
    def boom():
        raise RuntimeError("selfmap non disponibile")

    chiamate = []
    with Patcher() as p:
        _fresh_cache(p)
        p.set(L, "_build_grounded_system_prompt",
              lambda: chiamate.append(1) or "S")
        p.set(S, "context_signature", boom)
        L._system_prompt_static()
        L._system_prompt_static()
    assert len(chiamate) == 2, chiamate
    assert L._STATIC_PROMPT_CACHE["text"] == ""


def test_refresh_bootstrap_write_false_non_tocca_il_disco():
    """Il percorso caldo resta in sola lettura."""
    p_ = S.BOOTSTRAP_JSON
    mtime = os.stat(p_).st_mtime_ns if os.path.exists(p_) else None
    data = S.refresh_bootstrap(write=False)
    assert isinstance(data, dict) and "selfmap" in data
    if mtime is not None:
        assert os.stat(p_).st_mtime_ns == mtime


def test_load_bootstrap_context_non_riscrive_le_viste():
    mtime = os.stat(S.BOOTSTRAP_JSON).st_mtime_ns
    testo = S.load_bootstrap_context()
    assert testo.startswith("[ASTRAL BOOTSTRAP]")
    assert os.stat(S.BOOTSTRAP_JSON).st_mtime_ns == mtime


def test_call_with_dynamic_fallback_non_ricostruisce_ogni_volta():
    """Il costo fisso non si ripaga a ogni turno."""
    chiamate = []
    with Patcher() as p:
        _fresh_cache(p)
        p.set(L, "_build_grounded_system_prompt",
              lambda: chiamate.append(1) or "REGOLE")
        p.set(S, "context_signature", lambda: (("x",),))
        p.set(L, "_rate_limited_call", lambda **kw: {"ok": True})
        p.set(L, "MODEL_CONVERSATION", "modello-unico")
        p.set(L, "DYNAMIC_MODELS_POOL", [])
        for _ in range(3):
            L.call_with_dynamic_fallback([{"role": "user", "content": "ciao"}])
    assert len(chiamate) == 1, chiamate


def test_collapse_repetition_frase_lunga_oltre_otto_parole():
    """P2 audit: blocchi >8 parole ripetuti non devono passare intatti."""
    frase = "the cat sat on the mat and looked around quietly"
    testo = (frase + " ") * 4 + "fine"
    out = L._collapse_repetition(testo)
    assert out.count(frase) == 1, out
    assert "degenerato" in out.lower() or "compattato" in out.lower()


def test_collapse_repetition_blocchi_multipli():
    """P2 audit: piu' di un blocco ripetuto nello stesso testo."""
    testo = (("alpha beta gamma ") * 4 + ("delta epsilon zeta ") * 4).strip()
    out = L._collapse_repetition(testo)
    assert out.count("alpha beta gamma") == 1, out
    assert out.count("delta epsilon zeta") == 1, out


def test_collapse_repetition_testo_normale_intatto():
    """Nessun falso positivo su testo naturale."""
    testo = "ciao come stai oggi tutto bene grazie e tu procediamo con il lavoro richiesto ora"
    assert L._collapse_repetition(testo) == testo


if __name__ == "__main__":
    sys.exit(run_all(globals(), "test_prompt_cache"))
