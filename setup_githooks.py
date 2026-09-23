# -*- coding: utf-8 -*-
# setup_githooks.py - Attiva gli hook git versionati in .githooks/.
#
# Gli hook in .git/hooks non viaggiano col clone; core.hooksPath punta a una
# cartella versionata, cosi' la policy e' riproducibile. Idempotente.

import os
import subprocess
import sys

from core_io import BASE_DIR


def _git(*args):
    r = subprocess.run(
        ["git"] + list(args), capture_output=True, text=True,
        encoding="utf-8", errors="replace", cwd=BASE_DIR, timeout=20,
    )
    return r.returncode == 0, (r.stdout + r.stderr).strip()


def setup():
    hooks_dir = os.path.join(BASE_DIR, ".githooks")
    if not os.path.isdir(hooks_dir):
        return {"error": f"Cartella hook assente: {hooks_dir}"}
    ok, out = _git("config", "core.hooksPath", ".githooks")
    if not ok:
        return {"error": f"config fallita: {out}"}
    # Rendi eseguibili gli hook (rilevante su sistemi POSIX; su Windows no-op).
    for fn in os.listdir(hooks_dir):
        p = os.path.join(hooks_dir, fn)
        try:
            os.chmod(p, 0o755)
        except Exception:
            pass
    ok2, cur = _git("config", "core.hooksPath")
    return {"ok": True, "hooksPath": cur}


if __name__ == "__main__":
    res = setup()
    print(res)
    sys.exit(0 if res.get("ok") else 1)

