"""Runner detached del consiglio dei giudici (protocollo verdetto Astral).
Uso: python verdict_runner.py [standard|lite]
Legge il quesito da .verdict_quesito.tmp, esegue run_verdict(),
scrive l'output in .verdict_out.txt terminando con VERDICT_DONE.
"""
import sys, os, traceback

BASE = os.path.dirname(os.path.abspath(__file__))
# Il runner e' un file pubblico e stabile: il processo madre lo risolve sempre
# tramite questo percorso assoluto, evitando il precedente '.verdict_runner.py'.
profilo = sys.argv[1] if len(sys.argv) > 1 else "standard"
quesito_path = os.path.join(BASE, ".verdict_quesito.tmp")
out_path = os.path.join(BASE, ".verdict_out.txt")
err_path = os.path.join(BASE, ".verdict_err.txt")

quesito = ""
try:
    with open(quesito_path, "r", encoding="utf-8") as f:
        quesito = f.read().strip()
except Exception:
    pass

log = open(out_path, "w", encoding="utf-8")
_old_stdout, _old_stderr = sys.stdout, sys.stderr
try:
    sys.stdout = log
    sys.stderr = log
    from verdict.verdict import run_verdict
    run_verdict(quesito, profilo)
except Exception:
    sys.stdout = _old_stdout
    with open(err_path, "w", encoding="utf-8") as ef:
        ef.write(traceback.format_exc())
finally:
    sys.stdout = _old_stdout
    sys.stderr = _old_stderr
    log.close()
    with open(out_path, "a", encoding="utf-8") as f:
        f.write("\nVERDICT_DONE\n")
