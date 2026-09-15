# -*- coding: utf-8 -*-
# tools_gateway.py - Gateway interno dei tool (pattern pi-mcp-adapter)
# Registro + allowlist + namespace + descrizioni on-demand + timeout +
# circuit breaker + kill switch. I tool esistenti (tools_exec, tools_scrape)
# restano i backend; il gateway e' l'unico punto di ingresso per il modello.
import threading
import time

from exec_logger import log_execution

# --- Kill switch globale ---
_KILLED = False
_kill_lock = threading.Lock()

def kill_switch():
    """Attiva il kill switch: blocca TUTTE le chiamate tool successive."""
    global _KILLED
    with _kill_lock:
        _KILLED = True

def is_killed():
    with _kill_lock:
        return _KILLED

# --- Circuit breaker per tool ---
class CircuitBreaker:
    def __init__(self, threshold=5, reset_after=60.0):
        self.threshold = threshold
        self.reset_after = reset_after
        self.failures = 0
        self.open_until = 0.0
        self.lock = threading.Lock()

    def allow(self):
        with self.lock:
            now = time.time()
            if self.open_until and now < self.open_until:
                return False
            if self.open_until and now >= self.open_until:
                self.open_until = 0.0
                self.failures = 0
            return True

    def record_success(self):
        with self.lock:
            self.failures = 0

    def record_failure(self):
        with self.lock:
            self.failures += 1
            if self.failures >= self.threshold:
                self.open_until = time.time() + self.reset_after

_CIRCUITS = {}

def _breaker(name):
    if name not in _CIRCUITS:
        _CIRCUITS[name] = CircuitBreaker()
    return _CIRCUITS[name]

# --- Timeout wrapper (thread + join) ---
def _with_timeout(fn, timeout, name):
    box = {}

    def runner():
        try:
            box["res"] = fn()
            box["ok"] = True
        except Exception as e:
            box["err"] = e
            box["ok"] = False

    t = threading.Thread(target=runner, daemon=True)
    t.start()
    t.join(timeout)
    if t.is_alive():
        return False, {"error": f"Timeout tool '{name}' dopo {timeout}s"}
    if box.get("ok"):
        return True, box["res"]
    return False, {"error": f"{type(box['err']).__name__}: {box['err']}"}

# --- Registro tool ---
_TOOL_SPECS = {}

def _register(name, namespace, description, schema, handler, timeout=30.0, read_only=True):
    _TOOL_SPECS[name] = {
        "name": name, "namespace": namespace, "description": description,
        "schema": schema, "handler": handler, "timeout": timeout,
        "read_only": read_only,
    }

# --- Handler: delegano ai backend esistenti (niente doppio logging) ---
def _h_exec(name):
    def _h(args):
        from tools_exec import _execute_tool_impl
        return _execute_tool_impl(name, args)
    return _h

def _h_scrape(args):
    from tools_scrape import scrape, _fmt
    url = args.get("url", "")
    if not url:
        return {"error": "Serve 'url'"}
    r = scrape(url, refresh=bool(args.get("refresh", False)),
               use_cache=not args.get("no_cache", False),
               respect_robots=not args.get("no_robots", False),
               max_chars=args.get("max_chars") or None)
    if args.get("links") and r.html:
        from tools_scrape import estrai_links
        r.links = estrai_links(r.html, r.url)
    if args.get("jsonld") and r.html:
        from tools_scrape import estrai_jsonld
        r.jsonld = estrai_jsonld(r.html)
    r.html = ""
    return r.to_dict() if args.get("json") else _fmt(r)

_READ_ONLY = {"scan_storage", "recall", "test_python_file"}

def _build_schemas():
    from tools_exec import tools
    for t in tools:
        fn = t["function"]
        name = fn["name"]
        _register(name, "core", fn["description"], t, _h_exec(name),
                  timeout=30.0, read_only=(name in _READ_ONLY))
    # Tool scraper (namespace web, read-only)
    _register("scrape", "web",
              "Scrapa una URL: estrae il contenuto principale in markdown, con cache e rispetto robots.txt.",
              {"type": "function", "function": {
                  "name": "scrape",
                  "description": "Scrapa una URL: estrae il contenuto principale in markdown, con cache e rispetto robots.txt.",
                  "parameters": {"type": "object", "properties": {
                      "url": {"type": "string", "description": "URL da scrapare"},
                      "refresh": {"type": "boolean", "description": "Ignora la cache"},
                      "no_cache": {"type": "boolean", "description": "Non usare la cache"},
                      "no_robots": {"type": "boolean", "description": "Ignora robots.txt"},
                      "max_chars": {"type": "number", "description": "Tronca il markdown a N caratteri"},
                      "links": {"type": "boolean", "description": "Estrai anche i link interni/esterni"},
                      "jsonld": {"type": "boolean", "description": "Estrai anche i blocchi JSON-LD"},
                      "json": {"type": "boolean", "description": "Output come JSON"}
                  }, "required": ["url"]}}},
              _h_scrape, timeout=45.0, read_only=True)

# --- API pubblica ---
def search_tools(query=""):
    """Cerca tool per nome/descrizione (solo metadati leggeri, niente schemi)."""
    q = query.lower().strip()
    out = []
    for name, spec in _TOOL_SPECS.items():
        if not q or q in name.lower() or q in spec["description"].lower():
            out.append({"name": name, "namespace": spec["namespace"],
                        "description": spec["description"],
                        "read_only": spec["read_only"]})
    return {"tools": out, "count": len(out)}

def describe_tool(name):
    """Descrizione completa (schema OpenAI) di un tool, caricata on-demand."""
    spec = _TOOL_SPECS.get(name)
    if not spec:
        return {"error": f"Tool sconosciuto: {name}"}
    return spec["schema"]

def list_schemas():
    """Schema completo di TUTTI i tool (per la chiamata LLM iniziale)."""
    return [spec["schema"] for spec in _TOOL_SPECS.values()]

def call_tool(name, arguments=None):
    """Esegue un tool: allowlist + circuit breaker + timeout + kill switch."""
    args = arguments or {}
    if is_killed():
        return {"error": "Kill switch attivo: chiamate tool bloccate."}
    spec = _TOOL_SPECS.get(name)
    if not spec:
        return {"error": f"Tool sconosciuto o non in allowlist: {name}"}
    br = _breaker(name)
    if not br.allow():
        return {"error": f"Circuit breaker aperto per '{name}': troppi errori recenti."}
    t0 = time.perf_counter()
    ok, res = _with_timeout(lambda: spec["handler"](args), spec["timeout"], name)
    dur = time.perf_counter() - t0
    err = res.get("error") if isinstance(res, dict) else None
    if ok and not err:
        br.record_success()
    else:
        br.record_failure()
    log_execution(name, args, error=err, duration_ms=dur * 1000)
    return res

_build_schemas()
