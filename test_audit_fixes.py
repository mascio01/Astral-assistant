# -*- coding: utf-8 -*-
# test_audit_fixes.py - Regressione per i rilievi dell'audit sicurezza/affidabilita'
# (task #5): A-01 command injection, A-02 SSRF/redirect, A-03 path traversal,
# A-04 redazione log, A-05 race budget, A-06 idempotenza timeout, A-07 slot
# fail-closed, A-08 atomicita' task/Git, A-09 crash handler robusto.
# Standalone ("py test_audit_fixes.py") e compatibile pytest.
import os
import sys
import time

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from _test_support import Patcher, cleanup, run_all, temp_dir


# --- A-01: nessuna interpolazione del path nel comando PowerShell -----------
def test_a01_move_to_trash_no_interpolation():
    import tools_exec as E
    captured = {}

    class FakeRes:
        returncode = 0
        stderr = ""
        stdout = ""

    def fake_run(cmd, **kw):
        captured["cmd"] = cmd
        captured["env"] = kw.get("env")
        return FakeRes()

    evil = os.path.join(temp_dir(), "a'); Start-Process calc; ('b")
    os.makedirs(evil, exist_ok=True)
    p = Patcher()
    try:
        p.set(E.subprocess, "run", fake_run)
        res = E._execute_tool_impl("move_to_trash", {"path": evil})
        assert res.get("status") == "success", res
        joined = " ".join(captured["cmd"])
        assert evil not in joined, "path interpolato nel comando!"
        assert "Start-Process" not in joined, "payload iniettato nel comando!"
        assert captured["env"]["ASTRAL_TRASH_PATH"] == evil
    finally:
        p.restore()
        cleanup(os.path.dirname(evil))


# --- A-02: validazione di ogni redirect prima di seguirlo -------------------
def test_a02_safe_get_validates_each_hop():
    import tools_scrape as S

    class Resp:
        def __init__(self, status, location=None):
            self.status_code = status
            self.headers = {"location": location} if location else {}

    calls = []

    def do_get(url):
        calls.append(url)
        if url.endswith("/start"):
            return Resp(302, "http://127.0.0.1/evil")
        return Resp(200)

    try:
        S._safe_get(do_get, "http://example.com/start")
        assert False, "redirect verso loopback non bloccato"
    except ValueError as e:
        assert "bloccato" in str(e).lower()
    assert calls == ["http://example.com/start"], calls


def test_a02_safe_get_hop_limit():
    import tools_scrape as S

    class Resp:
        status_code = 302
        headers = {"location": "http://example.com/loop"}

    def do_get(url):
        return Resp()

    try:
        S._safe_get(do_get, "http://example.com/loop", max_hops=3)
        assert False, "loop di redirect non limitato"
    except ValueError as e:
        assert "redirect" in str(e).lower()


# --- A-03: confinamento lettura file subagent -------------------------------
def test_a03_read_file_rel_blocks_traversal():
    import subagents.roles as R
    out = R._read_file_rel("../../../../Windows/System32/drivers/etc/hosts")
    assert "bloccato" in out or "non consentito" in out, out


def test_a03_read_file_rel_blocks_absolute():
    import subagents.roles as R
    out = R._read_file_rel("C:\\Windows\\win.ini")
    assert "non consentito" in out, out


def test_a03_read_file_rel_allows_internal():
    import subagents.roles as R
    out = R._read_file_rel("core_io.py")
    assert "bloccato" not in out and "non consentito" not in out


# --- A-04: redazione dei segreti nei log ------------------------------------
def test_a04_redact_keys_and_values():
    import exec_logger as L
    red = L._redact_value({
        "api_key": "sk-abcdef1234567890",
        "password": "hunter2",
        "note": "Bearer abcdef1234567890",
        "nested": {"token": "xyz"},
        "safe": "ciao",
    })
    assert red["api_key"] == L._REDACTED
    assert red["password"] == L._REDACTED
    assert red["nested"]["token"] == L._REDACTED
    assert "Bearer abcdef1234567890" not in red["note"]
    assert red["safe"] == "ciao"


# --- A-05: budget atomico e lock --------------------------------------------
def test_a05_budget_respects_limit():
    import subagents.jobspec as J
    tmp = temp_dir()
    p = Patcher()
    try:
        p.set(J, "STATE_PATH", os.path.join(tmp, ".state.json"))
        p.set(J, "_LOCK_PATH", os.path.join(tmp, ".state.json.lock"))
        allowed = 0
        for _ in range(J.MAX_JOBS_PER_RUN + 3):
            ok, _msg = J.check_budget()
            if ok:
                allowed += 1
        assert allowed == J.MAX_JOBS_PER_RUN, allowed
    finally:
        p.restore()
        cleanup(tmp)


def test_a05_save_state_atomic_no_corruption():
    import subagents.jobspec as J
    tmp = temp_dir()
    p = Patcher()
    try:
        path = os.path.join(tmp, ".state.json")
        p.set(J, "STATE_PATH", path)
        J._save_state({"jobs_started": 3, "window_start": 123.0})
        import json
        with open(path, "r", encoding="utf-8") as f:
            assert json.load(f)["jobs_started"] == 3
    finally:
        p.restore()
        cleanup(tmp)


# --- A-06: idempotenza dei tool con effetti collaterali ---------------------
def test_a06_duplicate_side_effect_blocked():
    import tools_gateway as G
    G._INFLIGHT.clear()
    calls = {"n": 0}

    def slow():
        calls["n"] += 1
        time.sleep(0.3)
        return {"ok": True}

    ok1, res1 = G._with_timeout(slow, 0.05, "fake_tool", side_effect=True,
                                args={"x": 1})
    assert res1.get("status") == "in_progress", res1
    ok2, res2 = G._with_timeout(slow, 0.05, "fake_tool", side_effect=True,
                                args={"x": 1})
    assert res2.get("status") == "in_progress", res2
    assert calls["n"] == 1, calls
    time.sleep(0.4)
    G._INFLIGHT.clear()


# --- A-08: atomicita' task/Git ----------------------------------------------
def test_a08_duplicate_id_rejected():
    import task_tracker as T
    tmp = temp_dir()
    p = Patcher()
    try:
        p.set(T, "TASKS_DB", os.path.join(tmp, "tasks.db"))
        T.init_tasks()
        p.set(T, "_git", lambda *a, **k: (True, ""))
        r1 = T.create_task(9001, "Prima")
        assert r1.get("ok"), r1
        r2 = T.create_task(9001, "Seconda")
        assert "gia' esistente" in r2.get("error", ""), r2
    finally:
        p.restore()
        cleanup(tmp)


def test_a08_git_failure_rolls_back():
    import task_tracker as T
    tmp = temp_dir()
    p = Patcher()
    try:
        p.set(T, "TASKS_DB", os.path.join(tmp, "tasks.db"))
        T.init_tasks()
        p.set(T, "_git", lambda *a, **k: (False, "boom"))
        r = T.create_task(9002, "Orfana")
        assert "error" in r, r
        # get_task ritorna {"error": ...} se la task non esiste
        assert "error" in T.get_task(9002), "task orfana rimasta nel DB"
    finally:
        p.restore()
        cleanup(tmp)


# --- A-09: crash handler robusto a errori I/O -------------------------------
def test_a09_crash_handler_survives_log_failure():
    import core_io as C
    p = Patcher()
    started = {"n": 0}
    try:
        p.set(C, "BASE_DIR", os.path.join(temp_dir(), "nonexistent_dir_xyz"))
        p.set(C, "launch_self_repair", lambda restart=False: started.__setitem__("n", started["n"] + 1) or True)
        try:
            raise RuntimeError("crash simulato")
        except RuntimeError:
            C.global_exception_handler(*sys.exc_info())
        assert started["n"] == 1, "recovery non avviato dopo errore I/O del log"
    finally:
        p.restore()


if __name__ == "__main__":
    run_all(globals(), title="test_audit_fixes")

