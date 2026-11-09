"""rea_progress.py - Finestra console con barra di avanzamento per i job REA.

REA non emette output durante l'analisi: Ghidra resta muto fino alla fine
(verificato con probe in streaming, anche con REA_LOG_LEVEL=debug). La
percentuale e' quindi STIMATA, non misurata: le fasi sono rilevate dai
processi figli (node -> java) e l'ETA viene dalla mediana delle durate
storiche per lo stesso target (nome file + dimensione).
"""
from __future__ import annotations

import json
import os
import subprocess
import sys
import time

JOBS = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "_rea_jobs")
HIST = os.path.join(JOBS, "history.json")
W = 38

# [ANTI-FORK-BOMB] Sentinella condivisa con tools_rea._INTERNAL_ENV: il figlio
# lanciato qui e' GIA' dentro un job REA e non deve riaprire una console.
_INTERNAL_ENV = "ASTRAL_REA_INTERNAL"
# Rete di sicurezza: nessuna analisi REA legittima supera le 6 ore. Se il figlio
# resta vivo oltre, la finestra smette di sorvegliare invece di restare appesa.
MAX_WATCH_S = 6 * 3600

def _hist_load():
    try:
        with open(HIST, encoding="utf-8") as f:
            return json.load(f)
    except Exception:
        return {}


def _hist_save(h):
    try:
        with open(HIST, "w", encoding="utf-8") as f:
            json.dump(h, f)
    except Exception:
        pass


def _eta(key, default=600.0):
    """Mediana delle ultime durate per lo stesso target."""
    h = _hist_load().get(key) or []
    if not h:
        return default
    s = sorted(h)[-5:]
    return s[len(s) // 2]


def _java_running():
    try:
        out = subprocess.run(
            ["tasklist", "/FI", "IMAGENAME eq java.exe", "/NH"],
            capture_output=True, text=True, timeout=5,
        ).stdout
        return "java.exe" in out.lower()
    except Exception:
        return False


def _bar(pct, phase, elapsed, eta):
    n = int(W * pct / 100.0)
    bar = "#" * n + "-" * (W - n)
    el = "%d:%02d" % (elapsed // 60, elapsed % 60)
    if pct >= 100:
        tail = "tempo %s" % el
    else:
        rem = max(0, int(eta - elapsed))
        tail = "%s  ETA %d:%02d" % (el, rem // 60, rem % 60)
    return "  [%s] %3d%%  %-20s %s" % (bar, pct, phase[:20], tail)


def main(job):
    os.system("title REA job %s" % job)
    payload_p = os.path.join(JOBS, job + ".payload.json")
    with open(payload_p, encoding="utf-8") as f:
        payload = json.load(f)
    args = payload.get("args") or []
    target = str(args[-1]) if args else ""
    try:
        size = os.path.getsize(target)
    except Exception:
        size = 0
    key = "%s|%d" % (os.path.basename(target), size)
    eta = _eta(key)
    out_p = os.path.join(JOBS, job + ".json")
    log_p = os.path.join(JOBS, job + ".log")
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    code = (
        "import json,sys;sys.path.insert(0,r'%s');"
        "from tools.tools_rea import rea_run;"
        "r=rea_run(json.loads(sys.argv[1]));"
        "open(sys.argv[2],'w',encoding='utf-8').write(json.dumps(r,ensure_ascii=False))"
        % root.replace("\\", "\\\\")
    )
    print("  target: %s" % target)
    print("  ETA stimata: %d:%02d (mediana storica)\n" % (eta // 60, eta % 60))
    t0 = time.time()
    child_env = dict(os.environ)
    child_env[_INTERNAL_ENV] = "1"
    with open(log_p, "w", encoding="utf-8") as lg:
        lg.write("job %s start\n" % job)
        lg.flush()
        proc = subprocess.Popen(
            [sys.executable, "-c", code, json.dumps(payload), out_p],
            stdout=lg, stderr=subprocess.STDOUT, cwd=root, env=child_env,
        )
    phase = "avvio motore"
    seen_java = False
    last = ""
    while proc.poll() is None:
        el = time.time() - t0
        if el > MAX_WATCH_S:
            sys.stdout.write("\r" + _bar(95, "timeout watchdog", el, eta) + "\n")
            print("  il job supera %ds: smetto di sorvegliare (il processo resta vivo)." % MAX_WATCH_S)
            break
        if not seen_java and _java_running():
            seen_java = True
        if seen_java:
            phase = "Ghidra analizza"
        elif el > 8:
            phase = "preparazione"
        base = 12 if seen_java else 4
        pct = min(95, base + int(83 * el / max(eta, 1)))
        line = _bar(pct, phase, el, eta)
        if line != last:
            sys.stdout.write("\r" + line)
            sys.stdout.flush()
            last = line
        time.sleep(1)
    el = time.time() - t0
    ok = False
    res = {}
    try:
        with open(out_p, encoding="utf-8") as f:
            res = json.load(f)
        ok = bool(res.get("ok"))
    except Exception:
        pass
    sys.stdout.write("\r" + _bar(100, "fatto" if ok else "errore", el, eta) + "\n")
    h = _hist_load()
    h.setdefault(key, []).append(round(el, 1))
    h[key] = h[key][-10:]
    _hist_save(h)
    if ok:
        print("  risultato: %s" % out_p)
    else:
        print("  errore: %s" % str(res.get("error"))[:300])
    print("  log: %s" % log_p)
    print("\n  (finestra si chiude tra 8s)")
    time.sleep(8)


if __name__ == "__main__":
    main(sys.argv[1])
