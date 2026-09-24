# -*- coding: utf-8 -*-
# test_arch_fixes.py - Fase 5 (G05, G08, G09).
# Offline: solo tempdir e monkeypatch.
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import tools_patch as TP
from _test_support import Patcher, cleanup, run_all, temp_dir


# ------------------------------------------------------------------ G05 ---
def test_g05_scrittura_atomica():
    """apply_code_patch deve scrivere in modo atomico: se la scrittura fallisce
    il file originale resta INTATTO (niente file corrotto a meta')."""
    d = temp_dir()
    try:
        p = os.path.join(d, "target.py")
        with open(p, "w", encoding="utf-8") as f:
            f.write("A = 1\n")
        r = TP.apply_code_patch(p, [{"search": "A = 1", "replace": "A = 2"}])
        assert r.get("written") is True, r
        assert open(p, encoding="utf-8").read() == "A = 2\n"
        # Nessun file .tmp residuo nella cartella.
        leftovers = [n for n in os.listdir(d) if n.endswith(".tmp")]
        assert not leftovers, leftovers
    finally:
        cleanup(d)


def test_g05_fallimento_non_corrompe():
    """Se un blocco fallisce, il file NON deve essere toccato."""
    d = temp_dir()
    try:
        p = os.path.join(d, "target.py")
        with open(p, "w", encoding="utf-8") as f:
            f.write("A = 1\n")
        r = TP.apply_code_patch(p, [
            {"search": "A = 1", "replace": "A = 2"},
            {"search": "INESISTENTE", "replace": "x"},
        ])
        assert r.get("written") is False, r
        assert open(p, encoding="utf-8").read() == "A = 1\n"
    finally:
        cleanup(d)


# ------------------------------------------------------------------ G08 ---
def test_g08_ui_esportata():
    """Il package ui deve esporre i simboli usati da astral.py."""
    import ui
    for name in ("banner", "prompt_label", "confirm", "table", "error",
                 "voice_confirm"):
        assert hasattr(ui, name), "ui non esporta %s" % name
    assert os.path.exists(os.path.join(os.path.dirname(ui.__file__), "__init__.py"))


# ------------------------------------------------------------------ G09 ---
def test_g09_config_error_non_silenzioso():
    """Il salvataggio config non deve fallire in silenzio: l'errore va segnalato."""
    src = open(os.path.join(os.path.dirname(os.path.abspath(__file__)),
                            "astral.py"), encoding="utf-8").read()
    assert "Impossibile salvare la config" in src, "errore config ancora silenzioso"
    assert "save_config_model" in src, "manca il log dell'errore config"


if __name__ == "__main__":
    run_all(dict(globals()), title="test_arch_fixes")

