# -*- coding: utf-8 -*-
# ps_async.py - Esecuzione PowerShell asincrona con redirect su file + polling.
# Pattern B: i comandi lunghi vengono lanciati in background; il tool ritorna
# subito un job_id e l'output viene letto su richiesta (polling), evitando il
# timeout fisso del gateway (30s) e i thread orfani non terminabili.
import os
import subprocess
import time
import uuid

from core.core_io import BASE_DIR, cap_output
from core.memory_store import recall_save

JOBS_DIR = os.path.join(BASE_DIR, "ps_jobs")
_JOBS = {}          # job_id -> dict(proc, out, err, started, cmd, fout, ferr)
_JOB_TTL = 3600.0   # secondi: oltre, i job conclusi vengono potati


def _ensure_dir():
    try:
        os.makedirs(JOBS_DIR, exist_ok=True)
    except Exception:
        pass


def _purge(now):
    for jid in [j for j, v in _JOBS.items()
                if v.get("proc") is not None and v["proc"].poll() is not None
                and now - v.get("started", now) > _JOB_TTL]:
        _JOBS.pop(jid, None)


def start_job(command):
    """Lancia il comando in background. Ritorna {job_id, status:'running'}."""
    if not command or not str(command).strip():
        return {"error": "Comando vuoto."}
    _ensure_dir()
    now = time.time()
    _purge(now)
    jid = uuid.uuid4().hex[:12]
    out_path = os.path.join(JOBS_DIR, jid + ".out")
    err_path = os.path.join(JOBS_DIR, jid + ".err")
    try:
        fout = open(out_path, "w", encoding="utf-8", errors="replace")
        ferr = open(err_path, "w", encoding="utf-8", errors="replace")
        proc = subprocess.Popen(
            ["powershell", "-NoProfile", "-NonInteractive", "-Command",
             "[Console]::OutputEncoding=[System.Text.Encoding]::UTF8; " + command],
            stdout=fout, stderr=ferr, text=True, encoding="utf-8",
            errors="replace",
            creationflags=getattr(subprocess, "CREATE_NEW_PROCESS_GROUP", 0),
        )
        _JOBS[jid] = {"proc": proc, "out": out_path, "err": err_path,
                      "started": now, "cmd": command, "fout": fout, "ferr": ferr}
        return {"job_id": jid, "status": "running",
                "hint": f"Usa ps_job_status(job_id='{jid}') per leggere l'output."}
    except Exception as e:
        return {"error": f"{type(e).__name__}: {e}"}


def _read(path, limit=200000):
    try:
        with open(path, "r", encoding="utf-8", errors="replace") as f:
            return f.read(limit)
    except Exception:
        return ""


def _kill_tree(pid):
    try:
        subprocess.run(["taskkill", "/T", "/F", "/PID", str(pid)],
                       capture_output=True, text=True, timeout=10)
    except Exception:
        pass


def job_status(job_id, kill=False):
    """Polling: ritorna stato + output. Con kill=True termina l'albero di processi."""
    rec = _JOBS.get(job_id)
    if rec is None:
        return {"error": f"Job '{job_id}' non trovato (scaduto o inesistente)."}
    proc = rec["proc"]
    if kill:
        _kill_tree(proc.pid)
        return {"job_id": job_id, "status": "killed"}
    rc = proc.poll()
    if rc is None:
        return {"job_id": job_id, "status": "running",
                "elapsed_s": round(time.time() - rec["started"], 1)}
    for h in ("fout", "ferr"):
        try:
            rec[h].close()
        except Exception:
            pass
    raw_out = _read(rec["out"]).strip()
    raw_err = _read(rec["err"]).strip()
    out, _ = cap_output(raw_out)
    err, _ = cap_output(raw_err)
    payload = {"job_id": job_id, "status": "done", "exit_code": rc,
               "stdout": out, "stderr": err}
    if len(raw_out) > 4000:
        h = recall_save(raw_out)
        if h:
            payload["full_output_hint"] = f"[full output: recall {h}]"
    return payload

