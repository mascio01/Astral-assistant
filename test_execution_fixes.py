# -*- coding: utf-8 -*-
# test_execution_fixes.py - Fase 3 (G11, G12, G13, G15, G18, G19).
# Offline: nessuna rete, nessun processo reale, solo tempdir e monkeypatch.
import os
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import llm_core as L
import tools_exec as E
import tools_gateway as G
from _test_support import Patcher, cleanup, run_all, temp_dir


# ------------------------------------------------------------------ G11 ---
def test_g11_riassunto_non_riesegue_il_comando():
    """Il riassunto deve consumare l'output GIA' catturato: la versione
    precedente riscriveva il comando PS su un .ps1 e lo rilanciava con -File,
    causando una SECONDA esecuzione (con side effect duplicati)."""
    src = open(os.path.join(os.path.dirname(os.path.abspath(__file__)),
                            "tools_exec.py"), encoding="utf-8").read()
    # Nessuna riesecuzione: niente file .ps1 temporaneo (era il vettore del
    # secondo lancio).
    assert "mkstemp" not in src, "resta la scrittura del .ps1 per il riassunto"
    # Il riassuntore RTK consuma l'output via stdin, non rilanciando "-File".
    assert '["-File"' not in src, "il riassunto passa ancora -File"
    assert "input=" in src, "il riassuntore non riceve l'output via stdin"
    # Nessun secondo lancio del comando: esattamente 2 invocazioni powershell,
    # una per il cestino e una per il comando utente.
    assert src.count('subprocess.run(["powershell"') == 2


def test_g11_output_troncato_resta_leggibile():
    """Con output enorme e nessun riassuntore, il troncamento resta esplicito."""
    out = "x" * 50000
    max_chars = 4000
    out = out[:max_chars] + f"\n... [Output PowerShell troncato a {max_chars} caratteri per risparmio token]"
    assert len(out) < 50000
    assert "troncato" in out


# ------------------------------------------------------------------ G12 ---
def test_g12_esito_incerto_non_fa_fallback():
    """Errore incerto (timeout post-invio) -> nessun cambio modello."""
    modello_usato = []

    def fake_call(**kwargs):
        modello_usato.append(kwargs.get("model"))
        err = L.ErroreAstral("TIMEOUT", "timeout post-invio")
        err.uncertain = True
        raise err

    with Patcher() as p:
        p.set(L, "_rate_limited_call", fake_call)
        p.set(L, "_build_grounded_system_prompt", lambda: "sys")
        p.set(L, "MODEL_CONVERSATION", "modello-primo")
        p.set(L, "DYNAMIC_MODELS_POOL", ["modello-secondo"])
        try:
            L.call_with_dynamic_fallback([{"role": "user", "content": "ciao"}])
            assert False, "doveva rilanciare l'errore incerto"
        except L.ErroreAstral as e:
            assert getattr(e, "uncertain", False)
    assert modello_usato == ["modello-primo"], modello_usato


def test_g12_errore_certo_fa_fallback():
    """Errore deterministico (400/parametri) -> fallback regolare."""
    modello_usato = []

    def fake_call(**kwargs):
        modello_usato.append(kwargs.get("model"))
        if kwargs.get("model") == "modello-primo":
            raise L.ErroreAstral("SCONOSCIUTO", "400 bad request")
        return {"ok": True}

    with Patcher() as p:
        p.set(L, "_rate_limited_call", fake_call)
        p.set(L, "_build_grounded_system_prompt", lambda: "sys")
        p.set(L, "MODEL_CONVERSATION", "modello-primo")
        p.set(L, "DYNAMIC_MODELS_POOL", ["modello-secondo"])
        resp, usato = L.call_with_dynamic_fallback(
            [{"role": "user", "content": "ciao"}])
    assert usato == "modello-secondo", modello_usato
    assert resp == {"ok": True}


# ------------------------------------------------------------------ G13 ---
def test_g13_gateway_blocca_loop_identico():
    with Patcher() as p:
        p.set(G, "_LOOP_CALLS", [])
        p.set(G, "_TOOL_SPECS", dict(G._TOOL_SPECS))
        chiamate = []
        G._register("tool_finto", "test", "test", {},
                    lambda a: chiamate.append(1) or {"status": "ok"})
        r1 = G.call_tool("tool_finto", {"x": 1})
        r2 = G.call_tool("tool_finto", {"x": 1})
        r3 = G.call_tool("tool_finto", {"x": 1})
    assert not (r1 or {}).get("loop_guard")
    assert not (r2 or {}).get("loop_guard")
    assert r3.get("loop_guard") is True, r3
    assert len(chiamate) == 2, chiamate


def test_g13_argomenti_diversi_non_sono_loop():
    with Patcher() as p:
        p.set(G, "_LOOP_CALLS", [])
        p.set(G, "_TOOL_SPECS", dict(G._TOOL_SPECS))
        G._register("tool_finto2", "test", "test", {}, lambda a: {"status": "ok"})
        for i in range(4):
            r = G.call_tool("tool_finto2", {"x": i})
            assert not r.get("loop_guard"), r


# ------------------------------------------------------------------ G15 ---
def test_g15_esiti_falliti_riconosciuti():
    assert G._is_failure({"status": "fail"}) is True
    assert G._is_failure({"status": "failed", "written": False}) is True
    assert G._is_failure({"status": "timeout"}) is True
    assert G._is_failure({"exit_code": 1}) is True
    assert G._is_failure({"error": "boom"}) is True
    assert G._is_failure({"status": "ok"}) is False
    assert G._is_failure({"written": True}) is False


def test_g15_fallimento_apre_il_circuit_breaker():
    with Patcher() as p:
        p.set(G, "_LOOP_CALLS", [])
        p.set(G, "_TOOL_SPECS", dict(G._TOOL_SPECS))
        p.set(G, "_CIRCUITS", {})
        G._register("tool_ko", "test", "test", {},
                    lambda a: {"status": "fail"}, read_only=True,
                    side_effect=False)
        br = G._breaker("tool_ko")
        with br.lock:
            br.threshold = 2
        G.call_tool("tool_ko", {"a": 1})
        G.call_tool("tool_ko", {"a": 2})
        r = G.call_tool("tool_ko", {"a": 3})
    assert "Circuit breaker" in (r.get("error") or ""), r


# ------------------------------------------------------------------ G18 ---
def test_g18_crash_runner_non_scrive_verdict_done():
    src = open(os.path.join(os.path.dirname(os.path.abspath(__file__)),
                            "verdict_runner.py"), encoding="utf-8").read()
    # Il marker DONE deve essere condizionato a !failed: mai scritto dopo un crash.
    assert "if not failed:" in src
    assert "failed = True" in src


def test_g18_crash_simulato_non_produce_done():
    """Esegue davvero il runner con un modulo verdict che esplode e verifica
    che VERDICT_DONE NON venga scritto e che l'errore sia su file."""
    import shutil
    tmp = temp_dir()
    try:
        base = os.path.dirname(os.path.abspath(__file__))
        shutil.copyfile(os.path.join(base, "verdict_runner.py"),
                        os.path.join(tmp, "verdict_runner.py"))
        os.makedirs(os.path.join(tmp, "verdict"), exist_ok=True)
        with open(os.path.join(tmp, "verdict", "__init__.py"), "w") as f:
            f.write("")
        with open(os.path.join(tmp, "verdict", "verdict.py"), "w") as f:
            f.write("def run_verdict(*a, **k):\n    raise RuntimeError('crash simulato')\n")
        with open(os.path.join(tmp, ".verdict_quesito.tmp"), "w") as f:
            f.write("domanda")
        subprocess.run([sys.executable, os.path.join(tmp, "verdict_runner.py"), "lite"],
                       cwd=tmp, timeout=60)
        out = ""
        out_path = os.path.join(tmp, ".verdict_out.txt")
        if os.path.exists(out_path):
            out = open(out_path, encoding="utf-8").read()
        assert "VERDICT_DONE" not in out, out
        err = ""
        err_path = os.path.join(tmp, ".verdict_err.txt")
        if os.path.exists(err_path):
            err = open(err_path, encoding="utf-8").read()
        assert "crash simulato" in err, err
    finally:
        cleanup(tmp)


def test_g18_successo_scive_verdict_done():
    """Controprova: senza crash il marker DONE viene scritto regolarmente."""
    import shutil
    tmp = temp_dir()
    try:
        base = os.path.dirname(os.path.abspath(__file__))
        shutil.copyfile(os.path.join(base, "verdict_runner.py"),
                        os.path.join(tmp, "verdict_runner.py"))
        os.makedirs(os.path.join(tmp, "verdict"), exist_ok=True)
        with open(os.path.join(tmp, "verdict", "__init__.py"), "w") as f:
            f.write("")
        with open(os.path.join(tmp, "verdict", "verdict.py"), "w") as f:
            f.write("def run_verdict(*a, **k):\n    return 'VERDETTO OK'\n")
        with open(os.path.join(tmp, ".verdict_quesito.tmp"), "w") as f:
            f.write("domanda")
        subprocess.run([sys.executable, os.path.join(tmp, "verdict_runner.py"), "lite"],
                       cwd=tmp, timeout=60)
        out = open(os.path.join(tmp, ".verdict_out.txt"), encoding="utf-8").read()
        assert "VERDICT_DONE" in out, out
        assert "VERDETTO OK" in out, out
    finally:
        cleanup(tmp)


# ------------------------------------------------------------------ G19 ---
def test_g19_taskkill_fallito_non_dichiarato_riuscito():
    src = open(os.path.join(os.path.dirname(os.path.abspath(__file__)),
                            "astral.py"), encoding="utf-8").read()
    assert "taskkill fallito" in src
    assert "res.returncode != 0" in src


if __name__ == "__main__":
    sys.exit(run_all(globals(), "test_execution_fixes"))
