# -*- coding: utf-8 -*-
# task_hook.py - Dispatcher per gli hook git (policy Astral).
#
# Uso:
#   python task_hook.py commit-msg <file_messaggio>   -> exit 0 ok, 1 rifiuto
#   python task_hook.py post-merge <squash_flag>      -> solo log, exit 0
#
# Gli hook sono attivati via core.hooksPath=.githooks (setup_githooks.py).
# Nessun hook scrive sul DB: la riconciliazione e' compito di sync_from_git().

import os
import sys

from core_io import BASE_DIR


def _log_bypass(msg):
    """Registra i commit che bypassano la policy (--no-verify)."""
    try:
        p = os.path.join(BASE_DIR, ".git", "task_bypass.log")
        with open(p, "a", encoding="utf-8") as f:
            f.write(msg + "\n")
    except Exception:
        pass


def cmd_commit_msg(msg_file):
    """Rifiuta il commit se il messaggio non contiene un ID task."""
    try:
        with open(msg_file, "r", encoding="utf-8", errors="replace") as f:
            msg = f.read()
    except Exception:
        return 0  # non bloccare se non riusciamo a leggere
    # Consenti merge commit e revert automatici (non hanno ID task).
    first = (msg.strip().splitlines() or [""])[0]
    if first.startswith("Merge ") or first.startswith("Revert "):
        return 0
    try:
        from task_tracker import validate_commit_msg, extract_task_id
    except Exception:
        return 0  # tracker non disponibile: non bloccare il lavoro
    if validate_commit_msg(msg):
        return 0
    _log_bypass(f"RIFIUTATO: {first[:120]}")
    return 1


def cmd_post_merge(squash_flag):
    """Solo notifica: nessuna scrittura su DB o file versionati."""
    try:
        from task_tracker import extract_task_id
        import subprocess
        r = subprocess.run(
            ["git", "log", "-1", "--pretty=%s"],
            capture_output=True, text=True, encoding="utf-8", errors="replace",
            cwd=BASE_DIR, timeout=10,
        )
        tid = extract_task_id(r.stdout or "")
        if tid:
            print(f"[task] merge rilevato per task #{tid}: "
                  f"esegui /task-sync in Astral per riconciliare.")
    except Exception:
        pass
    return 0


def main(argv):
    if len(argv) < 2:
        return 0
    action = argv[1]
    if action == "commit-msg":
        return cmd_commit_msg(argv[2] if len(argv) > 2 else "")
    if action == "post-merge":
        return cmd_post_merge(argv[2] if len(argv) > 2 else "")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))

