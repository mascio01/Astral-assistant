# -*- coding: utf-8 -*-
# selfmap.py - Auto-mappa del progetto (self-awareness persistente).
import ast, datetime, hashlib, json, os, threading, time
ROOT = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(ROOT, ".selfmap.md")
JSON_OUT = os.path.join(ROOT, ".selfmap.json")
_WATCH_STARTED = False
EXCLUDE_DIRS = {"__pycache__", ".verdict", "venv", ".git", "node_modules"}
EXCLUDE_FILES = {"_explore_temp.py"}


def is_stale(max_age=1800):
    """True se la mappa manca, e' vecchia (>max_age sec) o qualche .py e' piu' recente di lei."""
    if not os.path.exists(OUT):
        return True
    map_mtime = os.path.getmtime(OUT)
    if time.time() - map_mtime > max_age:
        return True
    for dirpath, dirnames, filenames in os.walk(ROOT):
        dirnames[:] = [d for d in dirnames if d not in EXCLUDE_DIRS]
        for fn in filenames:
            if fn.endswith(".py"):
                try:
                    if os.path.getmtime(os.path.join(dirpath, fn)) > map_mtime:
                        return True
                except OSError:
                    pass
    return False

def _signature(node):
    args = [a.arg for a in node.args.posonlyargs + node.args.args]
    if node.args.vararg:
        args.append("*" + node.args.vararg.arg)
    args.extend(a.arg for a in node.args.kwonlyargs)
    if node.args.kwarg:
        args.append("**" + node.args.kwarg.arg)
    return "%s(%s)" % (node.name, ", ".join(args))


def _analyze(path):
    with open(path, encoding="utf-8-sig", errors="replace") as f:
        src = f.read()
    info = {"lines": src.count("\n") + 1, "summary": "", "classes": [],
            "functions": [], "imports": [], "defines": [],
            "sha256": hashlib.sha256(src.encode("utf-8")).hexdigest()[:16]}
    try:
        tree = ast.parse(src)
    except SyntaxError as e:
        info["syntax_error"] = str(e)
        return info
    info["summary"] = (ast.get_docstring(tree) or "").split("\n", 1)[0][:240]
    for node in tree.body:
        if isinstance(node, ast.ClassDef):
            info["classes"].append({"name": node.name, "line": node.lineno,
                                    "end_line": node.end_lineno,
                                    "summary": (ast.get_docstring(node) or "").split("\n", 1)[0][:180]})
        elif isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)):
            info["functions"].append({"name": node.name, "signature": _signature(node),
                                      "line": node.lineno, "end_line": node.end_lineno,
                                      "summary": (ast.get_docstring(node) or "").split("\n", 1)[0][:180]})
        elif isinstance(node, (ast.Assign, ast.AnnAssign)):
            targets = node.targets if isinstance(node, ast.Assign) else [node.target]
            info["defines"].extend(t.id for t in targets if isinstance(t, ast.Name) and t.id.isupper())
    for node in ast.walk(tree):
        if isinstance(node, ast.ImportFrom) and node.module and node.level == 0:
            m0 = node.module.split(".")[0]
            if os.path.exists(os.path.join(ROOT, m0 + ".py")) or os.path.isdir(os.path.join(ROOT, m0)):
                info["imports"].append(node.module)
        elif isinstance(node, ast.Import):
            for alias in node.names:
                m0 = alias.name.split(".")[0]
                if os.path.exists(os.path.join(ROOT, m0 + ".py")):
                    info["imports"].append(alias.name)
    info["imports"] = sorted(set(info["imports"]))
    return info

def generate():
    files = {}
    for dirpath, dirnames, filenames in os.walk(ROOT):
        dirnames[:] = [d for d in dirnames if d not in EXCLUDE_DIRS]
        for fn in sorted(filenames):
            if not fn.endswith(".py") or fn in EXCLUDE_FILES:
                continue
            path = os.path.join(dirpath, fn)
            files[os.path.relpath(path, ROOT)] = _analyze(path)
    reverse = {name: [] for name in files}
    module_to_file = {os.path.splitext(name)[0].replace(os.sep, "."): name for name in files}
    for source, info in files.items():
        for module in info["imports"]:
            target = module_to_file.get(module) or module_to_file.get(module.split(".")[0])
            if target and source not in reverse[target]:
                reverse[target].append(source)
    generated = datetime.datetime.now().isoformat(timespec="seconds")
    index = {"generated": generated, "root": ROOT, "file_count": len(files),
             "total_lines": sum(i["lines"] for i in files.values()),
             "files": files, "imported_by": reverse}
    md = ["# SELFMAP - Indice semantico di Astral",
          "_Generata: %s | File: %d | Righe: %d | JSON: `.selfmap.json`_" %
          (generated, index["file_count"], index["total_lines"]), "",
          "## Inventario", "| File | Responsabilita | Righe | Simboli | Dipendenze |",
          "|---|---|---:|---:|---|"]
    for rel, info in sorted(files.items(), key=lambda row: -row[1]["lines"]):
        summary = info["summary"].replace("|", "\\|") or "(docstring modulo assente)"
        md.append("| %s | %s | %d | %d | %s |" %
                  (rel, summary, info["lines"], len(info["classes"]) + len(info["functions"]),
                   ", ".join(info["imports"]) or "-"))
    md += ["", "## Dettaglio: posizione e contenuto"]
    for rel, info in sorted(files.items()):
        md += ["", "### %s", "- **Responsabilita:** %s" % (info["summary"] or "non documentata"),
               "- **Hash:** `%s`" % info["sha256"]]
        md[-3] = md[-3] % rel
        if info["imports"]: md.append("- **Usa:** " + ", ".join(info["imports"]))
        if reverse[rel]: md.append("- **Usato da:** " + ", ".join(sorted(reverse[rel])))
        for cls in info["classes"]:
            md.append("- **class %s** righe %d-%d — %s" %
                      (cls["name"], cls["line"], cls["end_line"], cls["summary"] or "non documentata"))
        for fn in info["functions"]:
            md.append("- `%s` righe %d-%d — %s" %
                      (fn["signature"], fn["line"], fn["end_line"], fn["summary"] or "non documentata"))
        if info["defines"]: md.append("- **Costanti:** " + ", ".join(info["defines"]))
    md += ["", "## Protocollo", "La mappa viene aggiornata all'avvio, dopo le patch e dal watcher runtime."]
    for path, content in ((JSON_OUT, json.dumps(index, ensure_ascii=False, indent=2)),
                          (OUT, "\n".join(md))):
        tmp = path + ".tmp"
        with open(tmp, "w", encoding="utf-8") as f:
            f.write(content)
        os.replace(tmp, path)
    return OUT


def start_watcher(interval=10):
    """Mantiene l'indice sincronizzato mentre Astral e' in esecuzione."""
    global _WATCH_STARTED
    if _WATCH_STARTED:
        return
    _WATCH_STARTED = True
    def _watch():
        while True:
            try:
                if is_stale(max_age=3600):
                    generate()
            except Exception:
                pass
            time.sleep(interval)
    threading.Thread(target=_watch, name="selfmap-watcher", daemon=True).start()

IDENTITY_FILE = os.path.join(ROOT, "identity.md")
STATE_FILE = os.path.join(ROOT, "state.md")
BOOTSTRAP_JSON = os.path.join(ROOT, "bootstrap.json")
BOOTSTRAP_MD = os.path.join(ROOT, "bootstrap.md")


def _read_context_file(path, limit=1200):
    try:
        with open(path, encoding="utf-8-sig", errors="replace") as stream:
            text = stream.read()
    except OSError:
        return ""
    text = "\n".join(line.rstrip() for line in text.splitlines())
    return text.strip()[:limit]


def _context_hash(path):
    try:
        with open(path, "rb") as stream:
            return hashlib.sha256(stream.read()).hexdigest()
    except OSError:
        return "missing"


def _bootstrap_data():
    """Costruisce il contesto minimo senza caricare la mappa tecnica completa."""
    return {
        "version": 1,
        "generated": datetime.datetime.now().isoformat(timespec="seconds"),
        "identity": _read_context_file(IDENTITY_FILE),
        "state": _read_context_file(STATE_FILE),
        "selfmap": {
            "generated": "",
            "file_count": 0,
            "total_lines": 0,
        },
        "hashes": {
            "identity": _context_hash(IDENTITY_FILE),
            "state": _context_hash(STATE_FILE),
            "selfmap": _context_hash(JSON_OUT),
        },
    }


def refresh_bootstrap():
    """Genera bootstrap.json e bootstrap.md come viste derivate."""
    data = _bootstrap_data()
    try:
        with open(JSON_OUT, encoding="utf-8") as stream:
            index = json.load(stream)
        data["selfmap"] = {
            "generated": index.get("generated", ""),
            "file_count": index.get("file_count", 0),
            "total_lines": index.get("total_lines", 0),
        }
    except (OSError, ValueError, TypeError):
        pass
    json_text = json.dumps(data, ensure_ascii=False, indent=2)
    md_text = "\n".join([
        "# Astral Bootstrap",
        "Contesto minimo derivato automaticamente; i dettagli tecnici sono on-demand.",
        "",
        "## Identita'",
        data["identity"] or "(identity.md assente)",
        "",
        "## Stato",
        data["state"] or "(state.md assente)",
        "",
        "## Mappa tecnica",
        "%s file, %s righe; generata: %s" % (
            data["selfmap"]["file_count"], data["selfmap"]["total_lines"],
            data["selfmap"]["generated"] or "n/d"),
        "",
        "## Integrita'",
        "Hash di identity.md, state.md e .selfmap.json in bootstrap.json.",
    ]) + "\n"
    for path, content in ((BOOTSTRAP_JSON, json_text), (BOOTSTRAP_MD, md_text)):
        tmp = path + ".tmp"
        with open(tmp, "w", encoding="utf-8", newline="\n") as stream:
            stream.write(content)
        os.replace(tmp, path)
    return data


def load_bootstrap_context(max_chars=2200):
    """Carica solo identita', stato e contatori della mappa."""
    try:
        data = refresh_bootstrap()
    except Exception:
        data = _bootstrap_data()
    text = "\n\n".join([
        "[ASTRAL BOOTSTRAP]",
        "IDENTITA':\n" + data.get("identity", ""),
        "STATO:\n" + data.get("state", ""),
        "MAPPA: %s file, %s righe; dettagli on-demand." % (
            data.get("selfmap", {}).get("file_count", 0),
            data.get("selfmap", {}).get("total_lines", 0)),
    ])
    return text[:max_chars].rstrip()


if __name__ == "__main__":
    print("Selfmap generata:", generate())
    refresh_bootstrap()
