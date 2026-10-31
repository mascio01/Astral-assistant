# -*- coding: utf-8 -*-
# Modulo estratto da astral.py - modularizzazione fase 1 (12/09/2026)

import io
import os
import subprocess
import sys

def apply_code_patch(path, patches):
    """Applica blocchi search/replace esatti a un file. patches = lista di dict {search, replace}.
    Ritorna dict con esito per blocco. Non usa regex: match letterale, un solo blocco per chiamata interna."""
    try:
        if not os.path.exists(path):
            return {"error": f"File non trovato: {path}"}
        with io.open(path, "r", encoding="utf-8") as f:
            src = f.read()
        results = []
        for i, p in enumerate(patches):
            search = p.get("search", "")
            replace = p.get("replace", "")
            if not search:
                results.append({"index": i, "status": "error", "message": "search vuoto"})
                continue
            n = src.count(search)
            if n == 0:
                results.append({"index": i, "status": "error", "message": "search non trovato"})
                continue
            if n > 1:
                results.append({"index": i, "status": "error", "message": f"search ambiguo ({n} occorrenze): aggiungi contesto"})
                continue
            src = src.replace(search, replace, 1)
            results.append({"index": i, "status": "ok"})
        if any(r["status"] == "error" for r in results):
            # Atomicita': se anche un solo blocco fallisce, non scrivere nulla
            return {"status": "failed", "results": results, "written": False}
        # [FIX G05] Scrittura ATOMICA: prima si scriveva direttamente sul file
        # di destinazione, quindi un errore a meta' write lasciava il file
        # corrotto. Ora si scrive su un temporaneo nella stessa cartella e si
        # sostituisce con os.replace (atomico sullo stesso filesystem).
        import tempfile
        d = os.path.dirname(os.path.abspath(path)) or "."
        fd, tmp = tempfile.mkstemp(dir=d, suffix=".tmp")
        try:
            with io.open(fd, "w", encoding="utf-8", closefd=True) as f:
                f.write(src)
            os.replace(tmp, path)
        except Exception:
            try:
                os.unlink(tmp)
            except Exception:
                pass
            raise
        return {"status": "success", "results": results, "written": True, "blocks": len(patches)}
    except Exception as e:
        return {"error": str(e)}


def test_python_file(path, mode="compile", timeout=30):
    """Auto-testing silenzioso: py_compile (default) o dry-run con timeout.
    Ritorna SOLO stato + traceback troncato (max 1000 caratteri). Nessun echo PowerShell."""
    try:
        if not os.path.exists(path):
            return {"error": f"File non trovato: {path}"}
        if mode == "compile":
            r = subprocess.run([sys.executable, "-m", "py_compile", path], capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=timeout)
            if r.returncode == 0:
                return {"status": "ok", "mode": "compile"}
            tb = (r.stderr or "").strip()[:1000]
            return {"status": "fail", "mode": "compile", "traceback": tb}
        elif mode == "run":
            r = subprocess.run([sys.executable, path], capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=timeout)
            out = (r.stdout or "").strip()[:1000]
            err = (r.stderr or "").strip()[:1000]
            return {"status": "ok" if r.returncode == 0 else "fail", "mode": "run", "returncode": r.returncode, "stdout": out, "stderr": err}
        else:
            return {"error": f"mode sconosciuto: {mode} (usa 'compile' o 'run')"}
    except subprocess.TimeoutExpired:
        return {"status": "timeout", "mode": mode, "message": f"Timeout dopo {timeout}s"}
    except Exception as e:
        return {"error": str(e)}
