# -*- coding: utf-8 -*-
# graphify_bridge.py - Bridge tra Astral e il pacchetto graphifyy (knowledge graph
# queryable del codebase). Incapsula subprocess, staleness e auto-rigenerazione.
#
# Il pacchetto PyPI `graphifyy` installa il modulo importabile `graphify` e il CLI
# `graphify` / `graphify-mcp` (in Scripts, spesso non in PATH). Qui invochiamo il
# CLI in modo robusto via `python -m graphify`, cosi' non dipendiamo dal PATH.
#
# API pubblica:
#   available()          -> bool: graphify e' importabile/eseguibile
#   info()               -> metadati del grafo (nodi, edge, built, versione)
#   needs_update()       -> bool: esistono file codice piu' recenti del grafo
#   update(force=False)  -> re-extract AST (no LLM), aggiorna graph.json
#   query(q, budget)     -> BFS traversal del grafo (risparmia token vs leggere file)
#   explain(name) / god_nodes() / path(a,b) / affected(name)
#   ensure_fresh()       -> aggiorna solo se stale (chiamata all'avvio)
#   start_watcher()      -> thread in background che mantiene il grafo fresco

import json
import os
import subprocess
import sys
import threading
import time

ROOT = os.path.dirname(os.path.abspath(__file__))
GRAPH_DIR = os.path.join(ROOT, "graphify-out")
GRAPH_JSON = os.path.join(GRAPH_DIR, "graph.json")

# Timeout (secondi) per le varie operazioni CLI.
T_QUERY = 30.0
T_READ = 30.0
T_UPDATE = 300.0

_WATCH_STARTED = False
_UPDATE_LOCK = threading.Lock()


def _python():
    """Interprete Python corrente (quello che ha il modulo graphify installato)."""
    return sys.executable or "python"


def available():
    """True se il modulo `graphify` e' importabile nell'interprete corrente."""
    try:
        import importlib.util
        return importlib.util.find_spec("graphify") is not None
    except Exception:
        return False


def version():
    try:
        import importlib.metadata as _m
        return _m.version("graphifyy")
    except Exception:
        return None


def _run(args, timeout):
    """Esegue `python -m graphify <args>`. Ritorna (rc, stdout, stderr)."""
    cmd = [_python(), "-m", "graphify"] + list(args)
    try:
        proc = subprocess.run(
            cmd, cwd=ROOT, capture_output=True, text=True,
            encoding="utf-8", errors="replace", timeout=timeout,
        )
        return proc.returncode, proc.stdout or "", proc.stderr or ""
    except subprocess.TimeoutExpired:
        return -1, "", f"timeout dopo {timeout}s"
    except FileNotFoundError as exc:
        return -1, "", f"graphify non eseguibile: {exc}"
    except Exception as exc:
        return -1, "", f"{type(exc).__name__}: {exc}"


def _load_graph():
    try:
        with open(GRAPH_JSON, encoding="utf-8") as fh:
            return json.load(fh)
    except Exception:
        return None


def _node_count():
    data = _load_graph()
    if not data:
        return 0
    return len(data.get("nodes") or [])


def info():
    """Metadati del grafo per diagnostica e UI."""
    data = _load_graph()
    nodes = len(data.get("nodes") or []) if data else 0
    links = len(data.get("links") or []) if data else 0
    mtime = os.path.getmtime(GRAPH_JSON) if os.path.exists(GRAPH_JSON) else None
    return {
        "available": available(),
        "installed_version": version(),
        "graph_json": GRAPH_JSON,
        "exists": os.path.exists(GRAPH_JSON),
        "nodes": nodes,
        "links": links,
        "built_at": (data or {}).get("built_at_commit"),
        "mtime": mtime,
        "mtime_iso": (time.strftime("%Y-%m-%dT%H:%M:%S", time.localtime(mtime))
                      if mtime else None),
    }


def _newest_code_mtime():
    """mtime del file codice piu' recente del progetto (esclusi output e cache)."""
    newest = 0.0
    skip = {"graphify-out", "__pycache__", ".git", ".verdict", "venv", ".venv",
            "node_modules", "scrape_cache", "logs", "ecc_skills"}
    exts = (".py", ".js", ".ts", ".tsx", ".jsx", ".java", ".go", ".rs",
            ".c", ".cpp", ".cs")
    for dirpath, dirnames, filenames in os.walk(ROOT):
        dirnames[:] = [d for d in dirnames if d not in skip and not d.startswith(".")]
        for name in filenames:
            if name.endswith(exts):
                try:
                    m = os.path.getmtime(os.path.join(dirpath, name))
                    if m > newest:
                        newest = m
                except OSError:
                    pass
    return newest


def needs_update(max_age=None):
    """True se il grafo e' assente, o se esiste codice piu' recente di graph.json.

    max_age (secondi): se indicato, considera stale anche un grafo piu' vecchio.
    """
    if not os.path.exists(GRAPH_JSON):
        return True
    g_mtime = os.path.getmtime(GRAPH_JSON)
    if max_age is not None and (time.time() - g_mtime) > max_age:
        return True
    return _newest_code_mtime() > g_mtime


def update(force=False, no_cluster=False, timeout=T_UPDATE):
    """Re-estrae i file codice (AST, nessuna LLM) e aggiorna graph.json.

    Non concorrente: serializzato da un lock, per non far collidere il watcher
    con una chiamata manuale. Ritorna dict con esito e conteggi.
    """
    if not available():
        return {"ok": False, "error": "graphify non installato"}
    acquired = _UPDATE_LOCK.acquire(timeout=1.0)
    if not acquired:
        return {"ok": False, "error": "aggiornamento gia' in corso"}
    try:
        before = _node_count()
        args = ["update", ROOT]
        if force:
            args.append("--force")
        if no_cluster:
            args.append("--no-cluster")
        t0 = time.perf_counter()
        rc, out, err = _run(args, timeout)
        dur = time.perf_counter() - t0
        after = _node_count()
        ok = rc == 0
        return {
            "ok": ok,
            "rc": rc,
            "duration_s": round(dur, 2),
            "nodes_before": before,
            "nodes_after": after,
            "tail": (out or err or "").strip().splitlines()[-6:],
            "error": None if ok else (err or out or "errore sconosciuto").strip()[:400],
        }
    finally:
        _UPDATE_LOCK.release()


def ensure_fresh(max_age=None, force=False):
    """Aggiorna il grafo solo se stale. Non solleva mai: adatta all'avvio."""
    try:
        if force or needs_update(max_age=max_age):
            return update(force=force)
        return {"ok": True, "skipped": True, "reason": "grafo fresco"}
    except Exception as exc:
        return {"ok": False, "error": f"{type(exc).__name__}: {exc}"}


def query(question, budget=2000, dfs=False):
    """BFS/DFS traversal del grafo per una domanda. Ritorna dict con testo."""
    if not question:
        return {"error": "Serve una domanda"}
    if not os.path.exists(GRAPH_JSON):
        return {"error": "graph.json assente: esegui prima un update"}
    args = ["query", question, "--budget", str(int(budget))]
    if dfs:
        args.append("--dfs")
    rc, out, err = _run(args, T_QUERY)
    text = (out or err).strip()
    if rc not in (0, 3, 4):
        return {"error": text[:400] or "graphify query fallito", "rc": rc}
    return {"ok": rc == 0, "rc": rc, "answer": text}


def explain(name, all_connections=False):
    if not name:
        return {"error": "Serve un nome nodo"}
    args = ["explain", name]
    if all_connections:
        args.append("--all")
    rc, out, err = _run(args, T_READ)
    return {"ok": rc == 0, "rc": rc, "text": (out or err).strip()}


def path(source, target):
    args = ["path", source, target]
    rc, out, err = _run(args, T_READ)
    return {"ok": rc == 0, "rc": rc, "text": (out or err).strip()}


def affected(name, depth=2):
    args = ["affected", name, "--depth", str(int(depth))]
    rc, out, err = _run(args, T_READ)
    return {"ok": rc == 0, "rc": rc, "text": (out or err).strip()}


def god_nodes(top=10):
    rc, out, err = _run(["god-nodes", "--top", str(int(top)), "--json"], T_READ)
    if rc != 0:
        return {"ok": False, "rc": rc, "error": (err or out).strip()[:400]}
    try:
        return {"ok": True, "nodes": json.loads(out)}
    except Exception:
        return {"ok": True, "raw": out.strip()}


def start_watcher(interval=600, max_age=1800):
    """Thread daemon che rigenera il grafo se stale mentre Astral e' in esecuzione."""
    global _WATCH_STARTED
    if _WATCH_STARTED:
        return False
    _WATCH_STARTED = True

    def _watch():
        while True:
            try:
                if needs_update(max_age=max_age):
                    update()
            except Exception:
                pass
            time.sleep(interval)

    threading.Thread(target=_watch, name="graphify-watcher", daemon=True).start()
    return True

