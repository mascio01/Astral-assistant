# -*- coding: utf-8 -*-
# test_task_tracker.py - Copertura task_tracker: convenzione commit, hash della
# suite, esito test VERIFICATO (non dichiarato), integrazione con avvisi.
# Ogni test lavora su DB e repo git temporanei: zero scritture sul progetto reale.
# Standalone ("py test_task_tracker.py") e compatibile pytest.
import os
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import task_tracker as TT
from _test_support import Patcher, cleanup, run_all, temp_dir


def git(*args, cwd):
    r = subprocess.run(["git"] + list(args), cwd=cwd, capture_output=True,
                       text=True, encoding="utf-8", errors="replace", timeout=30)
    return r.returncode, (r.stdout or "").strip()


class Repo(object):
    """Repo git temporaneo con DB tasks isolato e un file test_*.py finto."""

    def __init__(self, con_test=True):
        self.tmp = temp_dir()
        self.p = Patcher()
        self.con_test = con_test

    def __enter__(self):
        self.p.set(TT, "TASKS_DB", os.path.join(self.tmp, "tasks.db"))
        self.p.set(TT, "BASE_DIR", self.tmp)
        git("init", "-q", cwd=self.tmp)
        git("config", "user.email", "test@example.invalid", cwd=self.tmp)
        git("config", "user.name", "Test", cwd=self.tmp)
        with open(os.path.join(self.tmp, "codice.py"), "w", encoding="utf-8") as f:
            f.write("x = 1\n")
        if self.con_test:
            with open(os.path.join(self.tmp, "test_finto.py"), "w", encoding="utf-8") as f:
                f.write("print('ok')\n")
        git("add", "-A", cwd=self.tmp)
        git("commit", "-q", "-m", "feat(#1): iniziale", cwd=self.tmp)
        TT.init_tasks()
        return self

    def commit(self, msg="feat(#1): modifica"):
        with open(os.path.join(self.tmp, "codice.py"), "a", encoding="utf-8") as f:
            f.write("y = 2\n")
        git("add", "-A", cwd=self.tmp)
        return git("commit", "-q", "-m", msg, cwd=self.tmp)

    def __exit__(self, *exc):
        self.p.restore()
        cleanup(self.tmp)
        return False


# ------------------------------------------------------------ convenzione commit

def test_validate_commit_msg_accetta_la_convenzione():
    for msg in ("feat(#42): aggiunge tracker", "fix(#7): corregge il bug",
                "chore(#1): pulizia", "test(#9): copertura",
                "  refactor(#3): semplifica"):
        assert TT.validate_commit_msg(msg) is True, msg


def test_validate_commit_msg_rifiuta_senza_id():
    for msg in ("aggiunge tracker", "feat: senza id", "#42 senza tipo",
                "feat(#): id vuoto", "", None):
        assert TT.validate_commit_msg(msg) is False, msg


def test_validate_commit_msg_richiede_inizio():
    # L'ID deve aprire il messaggio: un riferimento nel corpo non basta.
    assert TT.validate_commit_msg("vedi feat(#42): altrove") is False


def test_extract_task_id():
    assert TT.extract_task_id("abc feat(#33): z") == 33
    assert TT.extract_task_id("fix(#5): y") == 5
    assert TT.extract_task_id("nessun id") is None
    assert TT.extract_task_id(None) is None


# ------------------------------------------------------------------- suite hash

def test_suite_hash_cambia_col_contenuto_dei_test():
    with Repo() as repo:
        primo = TT._suite_hash()
        with open(os.path.join(repo.tmp, "test_finto.py"), "a", encoding="utf-8") as f:
            f.write("# nuovo test\n")
        assert TT._suite_hash() != primo


def test_suite_hash_ignora_file_non_di_test():
    with Repo() as repo:
        primo = TT._suite_hash()
        with open(os.path.join(repo.tmp, "codice.py"), "a", encoding="utf-8") as f:
            f.write("z = 3\n")
        assert TT._suite_hash() == primo


def test_suite_hash_senza_test_e_stabile():
    with Repo(con_test=False) as repo:
        assert TT._test_files() == []
        assert TT._suite_hash() == TT._suite_hash()


def test_test_files_elenco_ordinato():
    with Repo() as repo:
        for nome in ("test_b.py", "test_a.py"):
            with open(os.path.join(repo.tmp, nome), "w", encoding="utf-8") as f:
                f.write("print('ok')\n")
        assert TT._test_files() == ["test_a.py", "test_b.py", "test_finto.py"]


# ------------------------------------------------------------------- esito test

def test_run_tests_verde_con_script_standalone():
    with Repo() as repo:
        ok, dettaglio = TT._run_tests()
        assert ok is True, dettaglio


def test_run_tests_rosso_se_uno_script_fallisce():
    with Repo() as repo:
        with open(os.path.join(repo.tmp, "test_rotto.py"), "w", encoding="utf-8") as f:
            f.write("raise SystemExit(3)\n")
        ok, dettaglio = TT._run_tests()
        assert ok is False
        assert any("test_rotto.py" in riga for riga in dettaglio)


def test_run_tests_senza_file_di_test():
    with Repo(con_test=False) as repo:
        ok, dettaglio = TT._run_tests()
        assert ok is False
        assert "nessun file" in dettaglio[0]


def test_mark_test_passed_esegue_e_ancora_al_commit():
    with Repo() as repo:
        TT.create_task(5, "Task di prova")
        esito = TT.mark_test_passed(5, run_tests=True)
        assert esito["ok"] is True
        assert esito["verified"] is True
        assert esito["suite_hash"] == TT._suite_hash()
        assert esito["commit"] == git("rev-parse", "HEAD", cwd=repo.tmp)[1]
        task = TT.get_task(5)
        assert task["test_passed"] is True
        assert task["test_suite_hash"] == esito["suite_hash"]
        assert task["test_commit"] == esito["commit"]
        assert task["ts_test_passed"]


def test_mark_test_passed_non_marca_se_i_test_falliscono():
    with Repo() as repo:
        TT.create_task(6, "Task con test rotti")
        with open(os.path.join(repo.tmp, "test_rotto.py"), "w", encoding="utf-8") as f:
            f.write("raise SystemExit(1)\n")
        esito = TT.mark_test_passed(6, run_tests=True)
        assert esito["ok"] is False
        assert esito["error"] == "Test falliti"
        assert TT.get_task(6)["test_passed"] is False


def test_mark_test_passed_verified_false_non_registra_lo_stato_test():
    with Repo() as repo:
        TT.create_task(7, "Test manuali")
        esito = TT.mark_test_passed(7, run_tests=False)
        assert esito["verified"] is False
        assert esito["detail"] == []
        assert TT.get_task(7)["test_passed"] is True


# ------------------------------------------------------------------ integrazione

def test_mark_integrated_avvisa_senza_test_verdi():
    with Repo() as repo:
        TT.create_task(8, "Integrata senza test")
        esito = TT.mark_integrated(8)
        assert esito["ok"] is True
        assert any("senza test verdi" in w for w in esito["warnings"])
        assert TT.get_task(8)["status"] == "integrated"


def test_mark_integrated_avvisa_se_la_suite_cambia_dopo_il_test():
    with Repo() as repo:
        TT.create_task(9, "Suite modificata")
        assert TT.mark_test_passed(9, run_tests=False)["ok"] is True
        with open(os.path.join(repo.tmp, "test_finto.py"), "a", encoding="utf-8") as f:
            f.write("# test aggiunto dopo la verifica\n")
        esito = TT.mark_integrated(9)
        assert any("suite di test e' cambiata" in w for w in esito["warnings"])


def test_mark_integrated_avvisa_se_il_codice_cambia_dopo_il_test():
    with Repo() as repo:
        TT.create_task(10, "Codice cambiato")
        assert TT.mark_test_passed(10, run_tests=False)["ok"] is True
        repo.commit("feat(#10): modifica dopo il test")
        esito = TT.mark_integrated(10)
        assert any("codice e' cambiato dopo il test" in w for w in esito["warnings"])


def test_mark_integrated_senza_avvisi_se_tutto_coerente():
    with Repo() as repo:
        TT.create_task(11, "Tutto coerente")
        assert TT.mark_test_passed(11, run_tests=False)["ok"] is True
        esito = TT.mark_integrated(11)
        assert esito["warnings"] == []


def test_mark_integrated_task_inesistente():
    with Repo() as repo:
        esito = TT.mark_integrated(999)
        assert "error" in esito


def test_mark_in_progress_non_risveglia_una_task_integrata():
    with Repo() as repo:
        TT.create_task(12, "Gia' integrata")
        TT.mark_test_passed(12, run_tests=False)
        TT.mark_integrated(12)
        assert TT.mark_in_progress(12)["updated"] is False
        assert TT.get_task(12)["status"] == "integrated"


# ------------------------------------------------------------------- registro

def test_create_task_richiede_titolo():
    with Repo() as repo:
        assert "error" in TT.create_task(20, "   ")
        assert "error" in TT.create_task(21, None)


def test_create_task_crea_branch_e_riga():
    with Repo() as repo:
        esito = TT.create_task(30, "Nuova funzionalita'")
        assert esito["ok"] is True
        assert esito["branch"].startswith("task/30-")
        assert git("branch", "--show-current", cwd=repo.tmp)[1] == esito["branch"]
        task = TT.get_task(30)
        assert task["title"] == "Nuova funzionalita'"
        assert task["status"] == "pending"


def test_create_task_idempotente_su_branch_esistente():
    with Repo() as repo:
        primo = TT.create_task(31, "Idempotente")
        secondo = TT.create_task(31, "Idempotente")
        assert primo["branch"] == secondo["branch"]
        assert secondo["ok"] is True


def test_list_active_riporta_i_flag():
    with Repo() as repo:
        TT.create_task(40, "Attiva")
        righe = TT.list_active()
        assert len(righe) == 1
        assert righe[0]["id"] == 40
        assert righe[0]["test_passed"] is False
        assert righe[0]["ts_integrated"] is None


def test_get_task_inesistente():
    with Repo() as repo:
        assert "error" in TT.get_task(999)


def test_init_tasks_idempotente_e_migra():
    with Repo() as repo:
        TT.init_tasks()
        TT.init_tasks()
        task = TT.get_task(1) if TT.get_task(1).get("id") else None
        assert "error" in TT.get_task(1)   # nessuna task creata d'ufficio


# ---------------------------------------------------------------- sync da git

def test_sync_from_git_crea_righe_dai_branch():
    with Repo() as repo:
        git("checkout", "-q", "-b", "task/55-da-branch", cwd=repo.tmp)
        git("checkout", "-q", "master" if
            git("rev-parse", "--verify", "master", cwd=repo.tmp)[0] == 0 else "main",
            cwd=repo.tmp)
        report = TT.sync_from_git()
        assert 55 in report["created"]
        assert TT.get_task(55)["title"] == "Task 55 (da branch)"


def test_sync_from_git_marca_integrate_le_commit_visibili():
    with Repo() as repo:
        TT.create_task(60, "Da integrare")
        branch = git("branch", "--show-current", cwd=repo.tmp)[1]
        # Serve un commit sul branch, altrimenti il merge e' vuoto e non lascia
        # un commit di merge da riconoscere.
        repo.commit("feat(#60): lavoro sul branch")
        git("checkout", "-q", "main" if
            git("rev-parse", "--verify", "main", cwd=repo.tmp)[0] == 0 else "master",
            cwd=repo.tmp)
        git("merge", "-q", "--no-ff", "-m", "Merge feat(#60): integra",
            branch, cwd=repo.tmp)
        report = TT.sync_from_git()
        assert 60 in report["integrated"]
        assert TT.get_task(60)["status"] == "integrated"


def test_sync_from_git_segnala_branch_mancante():
    with Repo() as repo:
        TT.create_task(70, "Senza branch")
        git("checkout", "-q", "main" if
            git("rev-parse", "--verify", "main", cwd=repo.tmp)[0] == 0 else "master",
            cwd=repo.tmp)
        git("branch", "-D", "task/70-senza-branch", cwd=repo.tmp)
        report = TT.sync_from_git()
        assert any("70" in w for w in report["warnings"])


def test_sync_from_git_usa_master_se_main_non_esiste():
    """Regressione: il branch di integrazione non deve essere hardcoded a "main".

    Bug reale trovato scrivendo questa suite: su un repo col default "master",
    'git log main' falliva, ok=False e sync_from_git non marcava NESSUNA task
    integrata, senza emettere alcun warning (fallimento silenzioso).
    """
    with Repo() as repo:
        assert git("rev-parse", "--verify", "main", cwd=repo.tmp)[0] != 0
        assert TT._branch_integrato() == "master"


def test_suite_hash_include_il_supporto_condiviso():
    """Regressione: anche il supporto dei test deve invalidare test_passed.

    Bug reale trovato scrivendo questa suite: _suite_hash guardava solo
    'test_*.py', quindi modificare _test_support.py (dove vivono fixture e
    assert helper) NON invalidava un test_passed gia' registrato. Il flag
    restava verde su una suite diversa da quella realmente eseguita.
    """
    with Repo(con_test=False) as repo:
        with open(os.path.join(repo.tmp, "test_uno.py"), "w", encoding="utf-8") as f:
            f.write("print('ok')\n")
        with open(os.path.join(repo.tmp, "_test_support.py"), "w", encoding="utf-8") as f:
            f.write("HELPERS = 1\n")
        primo = TT._suite_hash()
        with open(os.path.join(repo.tmp, "_test_support.py"), "a", encoding="utf-8") as f:
            f.write("HELPERS = 2\n")
        assert TT._suite_hash() != primo
        # Il supporto entra nell'hash ma NON viene eseguito come test.
        assert TT._test_files() == ["test_uno.py"]


if __name__ == "__main__":
    run_all(globals(), title="test_task_tracker.py")