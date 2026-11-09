# -*- coding: utf-8 -*-
"""Regressione G2 (audit azione ciclo1): coerenza di lettura di error_log.

Il difetto storico era: l'init del watchdog prendeva la PRIMA riga ERROR del
log, il loop l'ULTIMA -> falsa positivita' deterministica (riparava un errore
vecchio gia' risolto). Dopo la migrazione al bus watchdog il log resta solo
come percorso legacy (``repair_from_log``), e deve leggere sempre l'ultimo
blocco. Questi test bloccano una regressione.
"""
import os
import re
import sys

_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
if _ROOT not in sys.path:
    sys.path.insert(0, _ROOT)

from repair_loop import _latest_error_block  # noqa: E402


def _write_log(path, blocks):
    with open(path, "w", encoding="utf-8") as f:
        for i, body in enumerate(blocks):
            f.write(f"[2026-10-31 10:0{i}:00] ERROR:\n{body}\n{'-' * 50}\n")


def test_latest_error_block_returns_last(tmp_path=None):
    import tempfile
    d = tempfile.mkdtemp()
    try:
        p = os.path.join(d, "error_log.txt")
        _write_log(p, ["PRIMO-ERRORE", "SECONDO-ERRORE", "TERZO-ERRORE"])
        block = _latest_error_block(p)
        assert "TERZO-ERRORE" in block, block
        assert "PRIMO-ERRORE" not in block, block
    finally:
        import shutil
        shutil.rmtree(d, ignore_errors=True)


def test_nessun_lettore_usa_la_prima_error():
    """Nessun modulo attivo deve indicizzare starts[0] sul pattern ERROR.

    Lo scan e' limitato al codice PROPRIO del progetto: le directory di
    terze parti / vendor / artefatti (rea/, re/, backups/, graphify-out/, ...)
    contengono sorgenti non nostri in cui 'starts[0]' ha significati
    totalmente diversi (es. header ELF in pwntools) e non sono lettori del
    nostro error_log.txt.
    """
    offenders = []
    _SKIP_DIRS = (
        "ecc_skills", "__pycache__", "residui", "scratch", ".git", "logs",
        # codice di terze parti o artefatti non versionati
        "rea", "re", "backups", "graphify-out", ".venv", "venv",
        "ghidra_projects", "ps_jobs", ".astral_bus",
    )
    for root, dirs, files in os.walk(_ROOT):
        dirs[:] = [x for x in dirs if x not in _SKIP_DIRS]
        for name in files:
            if not name.endswith(".py"):
                continue
            path = os.path.join(root, name)
            if os.path.abspath(path) == os.path.abspath(__file__):
                continue  # il pattern compare nel test stesso
            try:
                src = open(path, encoding="utf-8", errors="replace").read()
            except OSError:
                continue
            if re.search(r"starts\[0\]|\[0\]\.start\(", src):
                offenders.append(os.path.relpath(path, _ROOT))
    assert not offenders, "lettura 'prima ERROR' residua in: %s" % offenders


if __name__ == "__main__":
    test_latest_error_block_returns_last()
    test_nessun_lettore_usa_la_prima_error()
    print("OK")