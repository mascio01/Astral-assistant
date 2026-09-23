# -*- coding: utf-8 -*-
# test_task_hook.py - Copertura del dispatcher degli hook git (policy Astral):
# accettazione/rifiuto dei messaggi, merge e revert esenti, log dei bypass.
# Standalone ("py test_task_hook.py") e compatibile pytest.
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import task_hook as H
from _test_support import Patcher, cleanup, run_all, temp_dir


class Hook(object):
    """Isola task_hook su una cartella temporanea (log bypass e repo finto)."""

    def __init__(self):
        self.tmp = temp_dir()
        self.p = Patcher()

    def __enter__(self):
        self.p.set(H, "BASE_DIR", self.tmp)
        os.makedirs(os.path.join(self.tmp, ".git"), exist_ok=True)
        return self

    def scrivi(self, contenuto):
        percorso = os.path.join(self.tmp, "COMMIT_EDITMSG")
        with open(percorso, "w", encoding="utf-8") as f:
            f.write(contenuto)
        return percorso

    def bypass_log(self):
        percorso = os.path.join(self.tmp, ".git", "task_bypass.log")
        if not os.path.exists(percorso):
            return ""
        with open(percorso, encoding="utf-8") as f:
            return f.read()

    def __exit__(self, *exc):
        self.p.restore()
        cleanup(self.tmp)
        return False


# ------------------------------------------------------------------ commit-msg

def test_commit_msg_accetta_la_convenzione():
    with Hook() as h:
        assert H.cmd_commit_msg(h.scrivi("feat(#42): aggiunge tracker\n")) == 0
        assert h.bypass_log() == ""


def test_commit_msg_rifiuta_senza_id_e_logga():
    with Hook() as h:
        assert H.cmd_commit_msg(h.scrivi("aggiunge tracker senza id\n")) == 1
        log = h.bypass_log()
        assert "RIFIUTATO" in log
        assert "aggiunge tracker senza id" in log


def test_commit_msg_esenta_merge_e_revert():
    with Hook() as h:
        assert H.cmd_commit_msg(h.scrivi("Merge branch 'task/1-x'\n")) == 0
        assert H.cmd_commit_msg(h.scrivi("Revert \"feat(#1): x\"\n")) == 0
        assert h.bypass_log() == ""


def test_commit_msg_file_inesistente_non_blocca():
    with Hook() as h:
        # Non riuscire a leggere il messaggio non deve impedire il commit.
        assert H.cmd_commit_msg(os.path.join(h.tmp, "manca.txt")) == 0


def test_commit_msg_usa_la_prima_riga_per_le_esenzioni():
    with Hook() as h:
        assert H.cmd_commit_msg(h.scrivi("feat(#1): titolo\n\nMerge di altra roba\n")) == 0


# ------------------------------------------------------------------ post-merge

def test_post_merge_non_scrive_mai_sul_registro():
    with Hook() as h:
        # Il post-merge e' SOLO notifica: nessuna scrittura su DB o file.
        prima = os.listdir(h.tmp)
        assert H.cmd_post_merge("0") == 0
        assert set(os.listdir(h.tmp)) == set(prima)


def test_post_merge_tollera_un_repo_non_git():
    with Hook() as h:
        # BASE_DIR temporaneo non e' un repo: git fallisce, l'hook non deve crashare.
        assert H.cmd_post_merge("1") == 0


# ---------------------------------------------------------------------- main

def test_main_dispatch_e_argomenti_insufficienti():
    with Hook() as h:
        assert H.main(["task_hook.py"]) == 0
        assert H.main(["task_hook.py", "azione-ignota"]) == 0
        assert H.main(["task_hook.py", "commit-msg", h.scrivi("feat(#1): ok\n")]) == 0
        assert H.main(["task_hook.py", "commit-msg", h.scrivi("no id\n")]) == 1


if __name__ == "__main__":
    run_all(globals(), title="test_task_hook.py")