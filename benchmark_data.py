# -*- coding: utf-8 -*-
# benchmark_data.py - Sorgenti REALI dei punteggi per il routing dinamico.
#
# Fonti (verificate 2026-09-14):
#  1) LiveBench: file statici alla root di https://livebench.ai/
#     table_{YYYY_MM_DD}.csv + categories_{YYYY_MM_DD}.json
#     La release corrente e' l'ULTIMO elemento dell'array di date (variabile
#     minified "pe") dentro static/js/main.<hash>.js (oggi: 2026-06-25).
#     Salviamo la release nota in .lb_sync_state.json -> path dei file noti.
#  2) OpenRouter: https://openrouter.ai/api/v1/models (API pubblica, no key).
#     FONTE UNICA e REALE per il PREZZO (pricing prompt/completion in $/token)
#     + contesto. (Punto 5 del brief: il prezzo si considera valido solo qui.)
#
# Intreccio (punto 1): modelli LiveBench -> ID OpenRouter via normalizzazione
# nome + fuzzy match (difflib). Categorie in comune usate (punto 2):
#   coding, reasoning, math, language, data_analysis, agentic_coding, if
# Parafrasi per il routing (punto 3): SOLO 3 gruppi (i restanti restano dati grezzi):
#   codice        -> coding  (media di Coding + Agentic Coding)
#   conversazione -> language
#   ragionamento  -> reasoning  (disponibile per estensioni future)
# Prezzo: SOLO OpenRouter (scala 0-10, 10 = economico). Affidabilita: IF (LB).
# Sync (punto 4): thread non bloccante con TTL 24h; file di stato e cache su disco.

import csv
import difflib
import glob
import json
import math
import os
import re
import subprocess
import sys
import threading
import time
import urllib.request

from core_io import log_error

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
STATE_FILE = os.path.join(BASE_DIR, ".lb_sync_state.json")
OR_CACHE = os.path.join(BASE_DIR, ".or_models_cache.json")
LB_BASE = "https://livebench.ai"
LB_RELEASES_URL = LB_BASE + "/static/js/main.ac6b12ef.js"
OR_MODELS_URL = "https://openrouter.ai/api/v1/models"
SYNC_TTL = 24 * 3600
_UA = ("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
       "(KHTML, like Gecko) Chrome/126.0 Safari/537.36")

# --- Parafrasi categorie: tutte le macro LiveBench -> 3 gruppi di routing ---
GRUPPI_ROUTING = ["coding", "language", "reasoning"]
GROUP_ROUTING = {
    "codice": "coding",
    "conversazione": "language",
    "misto": "reasoning",
    "ragionamento": "reasoning",
    "matematica": "math",
    "analisi_dati": "data_analysis",
    "istruzioni": "if",
}

_state = {}
_lock = threading.RLock()


def _read_state() -> dict:
    try:
        with open(STATE_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    except Exception:
        return {}


def _write_state():
    """Salva lo stato su disco. Chiamare SOLO tenendo _lock."""
    try:
        keys = ("synced", "lb_release", "release_checked", "lb_release_known")
        with open(STATE_FILE, "w", encoding="utf-8") as f:
            json.dump({k: _state.get(k) for k in keys}, f)
    except Exception:
        pass


_state = {"synced": 0.0, "lb_release": None, "release_checked": 0.0,
          "lb_release_known": None, "models": None, "thread": None}
_state.update(_read_state())


def _http_get(url: str, timeout: int = 20, binary: bool = False):
    """GET con urllib e fallback curl.exe (utile dietro Cloudflare).
    Ritorna (payload, status) oppure (None, 0)."""
    raw = None
    status = 0
    req = urllib.request.Request(url, headers={"User-Agent": _UA, "Accept": "*/*"})
    try:
        with urllib.request.urlopen(req, timeout=timeout) as r:
            raw = r.read()
            status = r.status
    except Exception:
        raw = None
    if raw is None:
        try:
            p = subprocess.run(["curl", "-sS", "-L", "--max-time", str(timeout),
                                "-A", _UA, url], capture_output=True,
                               timeout=timeout + 5)
            if p.returncode == 0 and p.stdout:
                raw = p.stdout
                status = 200
        except Exception:
            return None, 0
    if raw is None:
        return None, 0
    return (raw if binary else raw.decode("utf-8", "replace")), status


# ---------------------------------------------------------------- LiveBench --
def _macro(cat: str) -> str:
    return re.sub(r"\s+", "_", (cat or "").strip().lower())


def _lb_files():
    """Ultima coppia (table, categories) gia' scaricata, ordinata per release."""
    tabs = sorted(glob.glob(os.path.join(BASE_DIR, ".lb_table_*.csv")))
    cats = sorted(glob.glob(os.path.join(BASE_DIR, ".lb_categories_*.json")))
    if not tabs or not cats:
        return None, None
    return tabs[-1], cats[-1]


def _last_release():
    """Release LiveBence corrente: ultimo elemento dell'array di release nel
    bundle JS. Cache 24h; la release nota resta anche offline."""
    with _lock:
        last = _state.get("release_checked", 0.0)
    if time.time() - last < SYNC_TTL:
        return _state.get("lb_release_known")
    body, status = _http_get(LB_RELEASES_URL)
    rel = None
    if status == 200 and body:
        arrays = re.findall(r"\[(\"20\d{2}-\d{2}-\d{2}\"(?:,\"20\d{2}-\d{2}-\d{2}\")+)\]", body)
        best = None
        for a in arrays:
            dates = [d.strip('"') for d in a.split(",")]
            if len(dates) >= 5 and (best is None or dates[-1] > best[-1]):
                best = dates
        if best:
            rel = best[-1]
    with _lock:
        _state["release_checked"] = time.time()
        if rel:
            _state["lb_release_known"] = rel
            _write_state()
    return rel or _state.get("lb_release_known")


def _fetch_release(rel: str) -> bool:
    """Scarica table+categories della release; scrive SOLO se entrambi ok."""
    d = rel.replace("-", "_")
    got = {}
    for suffix, fname in (("table", "table_%s.csv" % d),
                          ("categories", "categories_%s.json" % d)):
        body, status = _http_get(LB_BASE + "/" + fname)
        if status == 200 and body:
            got[suffix] = body
    if "table" not in got or "categories" not in got:
        return False
    for suffix, body in got.items():
        try:
            with open(os.path.join(BASE_DIR, ".lb_%s_%s" % (suffix, d)), "w",
                      encoding="utf-8", newline="") as f:
                f.write(body)
        except Exception as e:
            log_error("benchmark_data/save_release", e)
            return False
    with _lock:
        _state["lb_release"] = d
        _write_state()
    return True


def _load_lb():
    """{modello_livebench: {macro_categoria: score 0-10}}, release."""
    table_path, cat_path = _lb_files()
    if not table_path or not cat_path:
        return {}, None
    try:
        with open(cat_path, "r", encoding="utf-8-sig") as f:
            cats = json.load(f)
    except Exception:
        cats = {}
    raw = {}
    try:
        with open(table_path, "r", encoding="utf-8-sig", newline="") as f:
            for row in csv.DictReader(f):
                name = (row.get("model") or "").strip()
                if name:
                    raw[name] = row
    except Exception as e:
        log_error("benchmark_data/lb_table", e)
        return {}, None
    m = re.search(r"(\d{4}_\d{2}_\d{2})", os.path.basename(table_path))
    release = m.group(1) if m else None
    out = {}
    for name, row in raw.items():
        agg = {}
        for cat, subs in (cats or {}).items():
            vals = []
            for s in subs:
                v = row.get(s)
                if v in (None, ""):
                    continue
                try:
                    vals.append(float(v))
                except (TypeError, ValueError):
                    pass
            if vals:
                agg[_macro(cat)] = sum(vals) / len(vals)
        out[name] = agg
    for agg in out.values():  # scala 0-100 -> 0-10 se necessario
        if max(agg.values(), default=0) > 15:
            for k in agg:
                agg[k] = agg[k] / 10.0
    return out, release


# -------------------------------------------------------------- OpenRouter --
def _sync_openrouter(timeout: int = 8) -> bool:
    body, status = _http_get(OR_MODELS_URL, timeout=timeout)
    models = None
    if status == 200 and body:
        try:
            models = (json.loads(body) or {}).get("data") or []
        except Exception:
            models = None
    if not models:
        return False
    slim = []
    for m in models:
        pr = m.get("pricing") or {}
        if m.get("id") and pr.get("prompt") is not None:
            slim.append({"id": m["id"], "name": m.get("name") or m["id"],
                         "context_length": m.get("context_length") or 0,
                         "prompt": pr.get("prompt"), "completion": pr.get("completion")})
    if not slim:
        return False
    try:
        with open(OR_CACHE, "w", encoding="utf-8") as f:
            json.dump({"ts": time.time(), "models": slim}, f)
        return True
    except Exception as e:
        log_error("benchmark_data/or_cache", e)
        return False


def _load_openrouter() -> list:
    data = None
    try:
        with open(OR_CACHE, "r", encoding="utf-8") as f:
            data = json.load(f)
    except Exception:
        data = None
    if not (data and time.time() - data.get("ts", 0) < SYNC_TTL):
        if _sync_openrouter():
            try:
                with open(OR_CACHE, "r", encoding="utf-8") as f:
                    data = json.load(f)
            except Exception:
                data = None
    if not data:
        return []
    out = []
    for m in data.get("models", []):
        try:
            p = float(m.get("prompt") or 0)
            c = float(m.get("completion") or 0)
        except (TypeError, ValueError):
            continue
        mm = dict(m)
        mm["prompt"], mm["completion"] = p, c
        mm["_usd"] = 0.75 * p + 0.25 * c  # blend 3:1 input/output
        out.append(mm)
    return out


def _cost_score(usd_per_token: float) -> float:
    """Scala 0-10 (10 = economico), log su prezzo blended $/token.
    Calibrazione: gratis=10, $0.10/M=8.5, $1/M=6.5, $10/M=4.5, $50/M~3.
    (10 = 6.5 - 2*log10(prezzo blended in $/Mtoken))"""
    if not usd_per_token or usd_per_token <= 0:
        return 10.0
    return round(max(0.0, min(10.0, -5.5 - 2.0 * math.log10(usd_per_token))), 2)


# ---------------------------------------------------------------- Intreccio --
def normalize_name(name: str) -> str:
    """Normalizza un nome modello per l'intreccio LB<->OR: toglie vendor,
    varianti, date, suffissi di reasoning-effort e unifica '.'/'-'.
    NB: LiveBench scrive '-max' quando usa il modello al massimo della
    potenza di calcolo: NON e' una variante di modello, va rimosso."""
    s = (name or "").lower().strip()
    s = re.sub(r":.*$", "", s)                       # varianti :free/:batch/...
    s = re.sub(r"^[~a-z0-9_-]+/", "", s)             # prefisso vendor (x-ai/, anthropic/, ...)
    s = re.sub(r"-20\d{2}-\d{2}-\d{2}", "", s)        # date ovunque (-2025-12-11-...)
    s = re.sub(r"-20\d{6,}", "", s)                   # date compatte ovunque (-20251101...)
    for _ in range(3):                               # suffissi effort/limiter (ripetuti)
        s = re.sub(r"-(thinking|auto|high|medium|low|xhigh|effort|max|64k|128k|256k|latest)$", "", s)
    s = s.replace(".", "-")                           # claude-opus-4.5 == claude-opus-4-5
    s = re.sub(r"-+", "-", s).strip("-")
    return s


def match_lb_to_openrouter(lb_names: list, or_ids: list) -> dict:
    """lb_name -> or_id: diretto, normalizzato, fuzzy (cutoff 0.8, no ':variant')."""
    res = {}
    norm_or = {}
    for m in or_ids:
        if ":" not in m:  # evita varianti :batch/:free nel matching
            k = normalize_name(m)
            cur = norm_or.get(k)
            if cur is None or not m.startswith("~"):
                norm_or[k] = m  # preferisce id puliti ai dagger/alias "~vendor/..."
    for lb in lb_names:
        if lb in or_ids:
            res[lb] = lb
            continue
        nl = normalize_name(lb)
        if nl in norm_or:
            res[lb] = norm_or[nl]
            continue
        close = difflib.get_close_matches(nl, list(norm_or.keys()), n=1, cutoff=0.8)
        if close:
            res[lb] = norm_or[close[0]]
    return res


def _rank_scores(or_models: list) -> dict:
    """Score stimato 0-10 stile LiveBench per chi NON ha punteggi LB:
    posizione nel catalogo OpenRouter su prezzo (60%) e contesto (40%)."""
    n = len(or_models)
    if n == 0:
        return {}
    ord_p = sorted(or_models, key=lambda m: m["_usd"])
    ord_c = sorted(or_models, key=lambda m: -(m.get("context_length") or 0))
    rp = {m["id"]: 10.0 * (1 - i / max(1, n - 1)) for i, m in enumerate(ord_p)}
    rc = {m["id"]: 10.0 * (1 - i / max(1, n - 1)) for i, m in enumerate(ord_c)}
    return {m["id"]: round(0.6 * rp[m["id"]] + 0.4 * rc[m["id"]], 2) for m in or_models}


def _build_models() -> dict:
    """Intreccio completo: {or_id: metriche 0-10 + prezzi + provenienza}."""
    lb, rel = _load_lb()
    or_models = _load_openrouter()
    if not or_models:
        return {}
    or2lb = {v: k for k, v in
             match_lb_to_openrouter(list(lb.keys()),
                                    [m["id"] for m in or_models]).items()}
    ranks = _rank_scores(or_models)
    out = {}
    for m in or_models:
        lb_scores = lb.get(or2lb.get(m["id"], ""), {})
        entry = {
            "costo": _cost_score(m["_usd"]),
            "prezzo_in": round(float(m.get("prompt") or 0) * 1e6, 4),   # $/Mtoken
            "prezzo_out": round(float(m.get("completion") or 0) * 1e6, 4),
            "contesto": int(m.get("context_length") or 0),
            "sconto_alias": m["id"].startswith("~"),  # '~' = alias latest a prezzo scontato
            "lb": {k: round(v, 2) for k, v in lb_scores.items()},
            "lb_release": rel, "fonte": "openrouter(stimato)",
        }
        if lb_scores:
            # categorie LiveBench grezze (tutte quelle in comune) + gruppi routing
            entry.update({k: round(v, 2) for k, v in lb_scores.items()})
            entry["conversazione"] = round(lb_scores.get("language", 7.5), 2)
            entry["codice"] = round(lb_scores.get("coding", 7.5), 2)
            entry["affidabilita"] = round(lb_scores.get("if", 7.5), 2)
            entry["fonte"] = "livebench+openrouter"
        else:
            r = ranks.get(m["id"], 5.0)
            entry["conversazione"] = round(r * 0.7 + 5.0 * 0.3, 2)
            entry["codice"] = round(r * 0.7 + 5.0 * 0.3, 2)
            entry["affidabilita"] = round(r * 0.5 + 5.0 * 0.5, 2)
        out[m["id"]] = entry
    # Seconda passata: ereditarieta' di famiglia (es. ".../deepseek-v4-flash-0731"
    # eredita i punteggi LB di ".../deepseek-v4-flash").
    for mid, e in list(out.items()):
        if e.get("lb"):
            continue
        base = re.sub(r"-\d{4,8}$", "", mid)
        if base != mid and base in out and out[base].get("lb"):
            b = out[base]
            e.update({"lb": b["lb"], "lb_release": b.get("lb_release"),
                      "conversazione": b["conversazione"], "codice": b["codice"],
                      "affidabilita": b["affidabilita"],
                      "fonte": "livebench+openrouter(famiglia)"})
    return out


# ------------------------------------------------------------------ Public --
# Pool CANONICO: le STESSE IA usate prima della rifattorizzazione
# (specchio di llm_core.DYNAMIC_MODELS_POOL). Il routing NON si allarga mai
# oltre questi 3 modelli: qualunque selezione per gruppo cade solo qui dentro.
POOL_CANONICO = [
    "deepseek/deepseek-v4-flash-0731",
    "deepseek/deepseek-v4.1-flash",
    "openai/gpt-5.6-luna",
]


def pool_suggerito(min_costo: float = 5.5, top: int = 2, ctx_min: int = 32768) -> dict:
    """Pool di routing consigliato, PARAFRASATO su 2 gruppi (punto 3 del brief):
    {"codice": [or_id, ...], "conversazione": [or_id, ...]}.
    Selezione: SOLO tra i modelli di POOL_CANONICO (stesse IA di prima) con
    punteggi LiveBench reali, prezzo sostenibile e contesto sufficiente.
    """
    models = get_models()
    cand = [(mid, v) for mid, v in models.items()
            if mid in POOL_CANONICO
            and v.get("fonte") == "livebench+openrouter"
            and v.get("costo", 0) >= min_costo and v.get("contesto", 0) >= ctx_min]
    out = {}
    for gruppo, chiave in (("codice", "codice"), ("conversazione", "conversazione")):
        ordi = sorted(cand, key=lambda kv: -kv[1].get(chiave, 0))
        out[gruppo] = [mid for mid, _ in ordi[:top]]
    return out


def _maybe_sync(force: bool = False):
    """Sync 24h NON bloccante (thread daemon): release LB + catalogo OR."""

    def job():
        try:
            rel = _last_release()
            _, disco = _load_lb()
            if rel and rel.replace("-", "_") != (disco or ""):
                _fetch_release(rel)   # nuova release LB: aggiorna i file su disco
            _sync_openrouter()        # prezzi/contesti sempre freschi
        except Exception as e:
            log_error("benchmark_data/sync", e)
        finally:
            # interprete in shutdown: non toccare stato/lock
            if not sys.is_finalizing():
                try:
                    with _lock:
                        _state["synced"] = time.time()
                        _state["thread"] = None
                        _state["models"] = None  # ricostruzione al prossimo get_models
                        _write_state()
                except Exception:
                    pass

    with _lock:
        if _state.get("thread") is not None:
            return
        if not force and _state.get("models") is not None \
                and time.time() - _state.get("synced", 0) < SYNC_TTL:
            return
        _state["thread"] = threading.Thread(target=job, daemon=True)
        _state["thread"].start()


def get_models(force_sync: bool = False) -> dict:
    """{or_id: {conversazione, codice, affidabilita, costo, prezzo_in,
    prezzo_out, contesto, lb{...}, lb_release, fonte}}"""
    with _lock:
        models = _state.get("models")
    if models is None:
        models = _build_models()
        with _lock:
            _state["models"] = models
    _maybe_sync(force=force_sync)
    return models


def info() -> dict:
    with _lock:
        synced = _state.get("synced", 0)
    models = get_models()
    fonti = {}
    for v in models.values():
        fonti[v.get("fonte", "?")] = fonti.get(v.get("fonte", "?"), 0) + 1
    rel = next((v.get("lb_release") for v in models.values() if v.get("lb_release")), None)
    return {"modelli": len(models), "release_lb": rel,
            "ultimo_sync": datetime_str(synced) if synced else "mai",
            "prossimo_sync": "24h dal sync", "fonti": fonti}


def datetime_str(ts: float) -> str:
    import time as _t
    return _t.strftime("%Y-%m-%d %H:%M", _t.localtime(ts))


if __name__ == "__main__":
    m = get_models(force_sync=True)
    t0 = time.time()  # demo CLI: attende il sync per persistere lo stato
    while _state.get("thread") is not None and time.time() - t0 < 30:
        time.sleep(0.5)
    print("Modelli totali:", len(m))
    with_lb = [k for k, v in m.items() if v.get("fonte", "").startswith("livebench")]
    print("Con punteggi LiveBench:", len(with_lb))
    for grp in ("codice", "conversazione", "reasoning"):
        top = sorted(m.items(), key=lambda kv: -kv[1].get(grp, 0))[:5]
        print(f"Top {grp}: " + ", ".join(f"{k.split('/')[-1]}={v.get(grp, 0):.1f}" for k, v in top))
    top_cost = sorted(m.items(), key=lambda kv: -kv[1]["costo"])[:5]
    print("Top economici: " + ", ".join(f"{k}={v['costo']:.1f}" for k, v in top_cost))
