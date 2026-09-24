"""Launcher detached del consiglio dei giudici di Astral.
Uso: python verdict_runner.py [standard|lite]
Legge il quesito da .verdict_quesito.tmp, esegue esclusivamente
verdict.verdict.run_verdict() e scrive .verdict_out.txt.
I subagent scout/reviewer hanno un percorso separato e non passano da questo
runner.
"""
import sys, os, traceback, time

BASE = os.path.dirname(os.path.abspath(__file__))
# Il runner e' un file pubblico e stabile: il processo madre lo risolve sempre
# tramite questo percorso assoluto, evitando il precedente '.verdict_runner.py'.
profilo = sys.argv[1] if len(sys.argv) > 1 else "standard"
if profilo not in ("standard", "lite"):
    profilo = "standard"
quesito_path = os.path.join(BASE, ".verdict_quesito.tmp")
out_path = os.path.join(BASE, ".verdict_out.txt")
err_path = os.path.join(BASE, ".verdict_err.txt")

quesito = ""
try:
    with open(quesito_path, "r", encoding="utf-8-sig") as f:
        quesito = f.read().strip()
except Exception:
    pass

# Il file sentinella viene scritto solo a processo terminato; il lock evita
# avvii concorrenti che potrebbero sovrascrivere il risultato.
_lock_path = os.path.join(BASE, ".verdict_runner.lock")
_lock = None
try:
    _lock = open(_lock_path, "x", encoding="ascii")
except FileExistsError:
    sys.exit(2)

log = open(out_path, "w", encoding="utf-8")
_old_stdout, _old_stderr = sys.stdout, sys.stderr
failed = False
try:
    sys.stdout = log
    sys.stderr = log
    from verdict.verdict import run_verdict
    # Il runner e' deliberatamente limitato al consiglio dei giudici.
    risultato = run_verdict(quesito, profilo, quiet=True)
    if risultato:
        print(risultato)
except Exception:
    # [FIX G18] Un crash NON deve essere presentato come successo: scriviamo il
    # traceback su stderr e marchiamo l'esito come FALLITO, cosi' il launcher
    # non trova VERDICT_DONE e riporta 'failed'
    # (prima il finally appendeva VERDICT_DONE anche dopo un crash).
    sys.stdout = _old_stdout
    try:
        with open(err_path, "w", encoding="utf-8") as ef:
            ef.write(traceback.format_exc())
    except Exception:
        pass
    failed = True
finally:
    sys.stdout = _old_stdout
    sys.stderr = _old_stderr
    log.close()
    if not failed:
        with open(out_path, "a", encoding="utf-8") as f:
            f.write("\nVERDICT_DONE\n")
    try:
        _lock.close()
        os.remove(_lock_path)
    except OSError:
        pass
