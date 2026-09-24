# -*- coding: utf-8 -*-
"""Servizio di avvio e attesa del consiglio dei giudici (verdict).

Il processo madre non esegue mai run_verdict() inline: qui si scrive il
dossier, si lancia verdict_runner.py in detached e si attende l'esito con
polling bounded. Il budget temporale resta quello adattivo di
verdict.verdict.estimate_verdict_timeout, unica fonte di verita'.

API pubblica:
    start_verdict(question, profile, context) -> dict
    wait_verdict(max_wait, job_id)            -> dict
    verdict_status()                          -> dict
    cancel_verdict()                          -> dict
"""
import json
import os
import subprocess
import sys
import time
import uuid
from datetime import datetime

from core_io import BASE_DIR, log_error

QUESITO_FILE = os.path.join(BASE_DIR, ".verdict_quesito.tmp")
OUT_FILE = os.path.join(BASE_DIR, ".verdict_out.txt")
ERR_FILE = os.path.join(BASE_DIR, ".verdict_err.txt")
LOCK_FILE = os.path.join(BASE_DIR, ".verdict_runner.lock")
JOB_FILE = os.path.join(BASE_DIR, ".verdict_job.json")
RUNNER_FILE = os.path.join(BASE_DIR, "verdict_runner.py")
VERDICTS_DIR = os.path.join(BASE_DIR, "verdicts")

PROFILES = ("standard", "lite")
DEFAULT_WAIT = 25.0
MAX_WAIT = 60.0
POLL_INTERVAL = 3.0
MAX_RESULT_CHARS = 28000
START_GRACE = 15.0

__all__ = ["start_verdict", "wait_verdict", "verdict_status", "cancel_verdict",
           "build_dossier", "estimate_timeout"]


def _profile(value):
    value = str(value or "").strip().lower()
    return value if value in PROFILES else "standard"


def _read(path):
    try:
        with open(path, "r", encoding="utf-8", errors="replace") as stream:
            return stream.read()
    except OSError:
        return ""


def _remove(*paths):
    for path in paths:
        try:
            os.remove(path)
        except OSError:
            pass


def _read_state():
    try:
        with open(JOB_FILE, "r", encoding="utf-8") as stream:
            data = json.load(stream)
        return data if isinstance(data, dict) else {}
    except (OSError, ValueError, TypeError):
        return {}


def _write_state(state):
    tmp = JOB_FILE + ".tmp"
    try:
        with open(tmp, "w", encoding="utf-8") as stream:
            json.dump(state, stream, ensure_ascii=False)
        os.replace(tmp, JOB_FILE)
        return True
    except OSError as exc:
        log_error("verdict/state", exc)
        return False


def _pid_alive(pid):
    if not pid:
        return False
    try:
        out = subprocess.run(["tasklist", "/FI", "PID eq %s" % pid, "/NH"],
                             capture_output=True, text=True, timeout=10)
        return str(pid) in (out.stdout or "")
    except Exception:
        return False


def _runner_alive(state):
    """Vivo se il lock esiste oppure il PID del runner risponde."""
    if os.path.exists(LOCK_FILE):
        return True
    return _pid_alive(state.get("pid"))


def build_dossier(question, context="", max_chars=18000):
    """Costruisce il dossier del consiglio senza inventare contesto."""
    question = " ".join(str(question or "").split()).strip()
    context = str(context or "").strip()
    if len(context) > max_chars:
        context = context[-max_chars:]
    return (
        "DOMANDA ORIGINALE DELL'UTENTE:\n" + question +
        "\n\nCONTESTO FORNITO DAL CHIAMANTE (puo' essere incompleto):\n" +
        (context or "(nessun contesto aggiuntivo disponibile)") +
        "\n\nISTRUZIONI PER IL CONSIGLIO:\n"
        "Rispondi alla domanda originale usando il contesto solo quando e' pertinente. "
        "Non inventare dati mancanti e segnala le assunzioni. Se la richiesta implica "
        "una scelta, includi pro e contro, rischi e passaggi operativi; se non implica "
        "una scelta, non forzare pro e contro.")


def estimate_timeout(profile, dossier=""):
    """Budget adattivo: profilo, complessita' e latenza EMA storica."""
    profile = _profile(profile)
    try:
        from verdict.verdict import estimate_verdict_timeout
        return int(estimate_verdict_timeout(profile, dossier))
    except Exception:
        return 600 if profile == "lite" else 480


def start_verdict(question, profile="standard", context="", dossier=None):
    """Scrive il dossier e lancia il runner detached. Non blocca mai.

    dossier: se fornito (gia' completo) viene usato cosi' com'e', saltando
    build_dossier. Serve ai chiamanti che hanno gia' composto il contesto.
    """
    question = " ".join(str(question or "").split()).strip()
    if not question:
        return {"error": "Serve una domanda per il consiglio."}
    profile = _profile(profile)
    if not os.path.isfile(RUNNER_FILE):
        return {"error": "Runner verdetto assente: %s" % RUNNER_FILE}

    state = _read_state()
    if state and _runner_alive(state):
        return {"error": "Un verdetto e' gia' in corso.", "status": "running",
                "job_id": state.get("job_id"), "profile": state.get("profile"),
                "hint": "Usa verdict_wait per l'esito o verdict_cancel per annullare."}
    _remove(LOCK_FILE)

    dossier = str(dossier or "").strip() or build_dossier(question, context)
    try:
        with open(QUESITO_FILE, "w", encoding="utf-8") as stream:
            stream.write(dossier)
    except OSError as exc:
        log_error("verdict/quesito", exc)
        return {"error": "Impossibile scrivere il quesito: %s" % exc}

    _remove(OUT_FILE, ERR_FILE)
    timeout = estimate_timeout(profile, dossier)
    try:
        proc = subprocess.Popen(
            [sys.executable, RUNNER_FILE, profile],
            cwd=BASE_DIR,
            creationflags=getattr(subprocess, "CREATE_NO_WINDOW", 0),
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )
    except OSError as exc:
        log_error("verdict/spawn", exc)
        return {"error": "Avvio del runner fallito: %s" % exc}

    job = {
        "job_id": uuid.uuid4().hex[:12],
        "profile": profile,
        "pid": proc.pid,
        "started_at": datetime.now().isoformat(timespec="seconds"),
        "started_ts": time.time(),
        "timeout": timeout,
        "deadline": time.time() + timeout,
        "polls": 0,
        "question": question[:400],
    }
    _write_state(job)
    return {"status": "running", "job_id": job["job_id"], "profile": profile,
            "pid": proc.pid, "timeout": timeout,
            "hint": "Richiama verdict_wait (25s per chiamata) fino a 'done'."}


def _finalize(state, content):
    """Salva il verdetto su file, ripulisce i temporanei e ritorna l'esito."""
    result = content.replace("VERDICT_DONE", "").strip()
    path = ""
    try:
        os.makedirs(VERDICTS_DIR, exist_ok=True)
        stamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        path = os.path.join(VERDICTS_DIR, "verdict_%s_%s.md" % (stamp, state.get("profile", "standard")))
        with open(path, "w", encoding="utf-8") as stream:
            stream.write(result)
    except OSError as exc:
        log_error("verdict/save", exc)
        path = ""
    _remove(JOB_FILE, OUT_FILE, ERR_FILE, QUESITO_FILE)
    return {"status": "done", "job_id": state.get("job_id"),
            "profile": state.get("profile"), "file": path,
            "chars": len(result), "truncated": len(result) > MAX_RESULT_CHARS,
            "result": result[:MAX_RESULT_CHARS]}


def wait_verdict(max_wait=DEFAULT_WAIT, job_id=None):
    """Attende l'esito con budget bounded, senza mai bloccare oltre max_wait."""
    state = _read_state()
    if not state:
        return {"error": "Nessun verdetto in corso. Avvialo con run_verdict."}
    if job_id and str(job_id) != str(state.get("job_id")):
        return {"error": "job_id non corrispondente.", "current": state.get("job_id")}
    try:
        budget = float(max_wait)
    except (TypeError, ValueError):
        budget = DEFAULT_WAIT
    budget = max(3.0, min(budget, MAX_WAIT))

    end_of_call = time.time() + budget
    while True:
        content = _read(OUT_FILE)
        if "VERDICT_DONE" in content:
            return _finalize(state, content)

        if not _runner_alive(state):
            # Il runner non e' piu' vivo e il marker DONE non c'e': e' un
            # fallimento reale (crash, lock stantio, errore interno).
            _remove(JOB_FILE, OUT_FILE, ERR_FILE, QUESITO_FILE, LOCK_FILE)
            detail = _read(ERR_FILE).strip()
            return {"status": "failed", "job_id": state.get("job_id"),
                    "error": detail[-1200:] or "runner terminato senza VERDICT_DONE"}

        if time.time() >= float(state.get("deadline") or 0):
            cancel_verdict()
            return {"status": "timeout", "job_id": state.get("job_id"),
                    "error": "Timeout adattivo (%ss) superato; runner chiuso."
                             % state.get("timeout")}

        if time.time() >= end_of_call:
            state["polls"] = int(state.get("polls") or 0) + 1
            _write_state(state)
            elapsed = round(time.time() - float(state.get("started_ts") or time.time()), 1)
            return {"status": "running", "job_id": state.get("job_id"),
                    "profile": state.get("profile"), "polls": state["polls"],
                    "elapsed_s": elapsed, "timeout": state.get("timeout"),
                    "hint": "Richiama verdict_wait per continuare ad attendere."}

        time.sleep(POLL_INTERVAL)


def verdict_status():
    """Stato sintetico del job corrente, senza attendere."""
    state = _read_state()
    if not state:
        return {"status": "idle", "hint": "Nessun verdetto in corso."}
    content = _read(OUT_FILE)
    if "VERDICT_DONE" in content:
        return _finalize(state, content)
    alive = _runner_alive(state)
    elapsed = round(time.time() - float(state.get("started_ts") or time.time()), 1)
    return {"status": "running" if alive else "stalled",
            "job_id": state.get("job_id"), "profile": state.get("profile"),
            "pid": state.get("pid"), "elapsed_s": elapsed,
            "timeout": state.get("timeout"), "polls": state.get("polls") or 0}


def cancel_verdict():
    """Chiude il runner attivo e ripulisce i file di protocollo."""
    state = _read_state()
    pid = state.get("pid")
    killed = False
    if pid and _runner_alive(state):
        try:
            subprocess.run(["taskkill", "/PID", str(pid), "/T", "/F"],
                           capture_output=True, text=True, timeout=15)
            killed = True
        except Exception as exc:
            log_error("verdict/cancel", exc)
    _remove(JOB_FILE, OUT_FILE, ERR_FILE, QUESITO_FILE, LOCK_FILE)
    return {"status": "cancelled" if killed else "idle", "job_id": state.get("job_id"),
            "killed": killed}
