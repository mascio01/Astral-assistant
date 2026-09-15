"""
tools_scrape.py - Scraper interno eccellente di Astral (self-contained).

Architettura (verdetto consiglio, confidenza alta):
- Backend statico: httpx (fetch) + trafilatura (estrazione main-content -> markdown)
- Fallback estrazione: HTMLParser stdlib se trafilatura rende poco
- Cache a disco con TTL (scrape_cache/), rigenerabile con --refresh
- robots.txt rispettato di default (--no-robots per disattivare)
- Rilevazione pagine JS-rendered (contenuto scarso): warning, niente headless
- Interfaccia ScrapeBackend aperta a backend futuri (es. render JS opzionale)
- Estrazione strutturata LLM opzionale (lazy import, nessuna dipendenza dura)
- Retry con backoff su errori transitori + fallback TLS Chrome (curl_cffi)
- Extra zero-LLM: estrai_links, estrai_jsonld (schema.org), scrape_many concorrente
- max_chars opzionale (troncamento markdown), keep_html per post-processing

CLI:
    python tools_scrape.py <url> [--refresh] [--json] [--no-cache] [--no-robots]
                               [--links] [--jsonld] [--max-chars N]
    python tools_scrape.py --batch urls.txt [--json]
"""

from __future__ import annotations

import hashlib
import json
import re
import time
import urllib.robotparser
from dataclasses import dataclass, field, asdict
from html.parser import HTMLParser
from pathlib import Path
from typing import Optional
from urllib.parse import urlsplit

import httpx
import trafilatura

BASE_DIR = Path(__file__).resolve().parent
CACHE_DIR = BASE_DIR / "scrape_cache"
CACHE_TTL_H = 24          # ore di validita' cache
FETCH_TIMEOUT = 20.0      # secondi
MIN_TEXT_LEN = 500        # sotto questa soglia: probabile pagina JS-rendered
UA = ("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
      "(KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36")


# ----------------------------------------------------------------- risultati

@dataclass
class ScrapeResult:
    url: str
    ok: bool = False
    status: int = 0
    backend: str = "httpx+trafilatura"
    title: str = ""
    markdown: str = ""
    meta: dict = field(default_factory=dict)
    cached: bool = False
    elapsed_s: float = 0.0
    warning: str = ""
    errore: str = ""
    html: str = ""                             # popolato solo con keep_html=True
    links: dict = field(default_factory=dict)  # se estrai_links richiesto
    jsonld: list = field(default_factory=list) # dati strutturati schema.org

    def to_dict(self) -> dict:
        return asdict(self)


@dataclass
class FetchResponse:
    status: int
    html: str
    content_type: str


# ----------------------------------------------------------------- backend

class ScrapeBackend:
    """Interfaccia minima per backend di fetch (estendibile)."""
    name = "abstract"

    def fetch(self, url: str, headers: dict | None = None) -> FetchResponse:
        raise NotImplementedError


class HttpxBackend(ScrapeBackend):
    """Fetch statico: veloce, leggero, zero browser."""
    name = "httpx-static"

    def fetch(self, url: str, headers: dict | None = None) -> FetchResponse:
        h = {"User-Agent": UA, "Accept-Language": "it-IT,it;q=0.9,en;q=0.8"}
        if headers:
            h.update(headers)
        with httpx.Client(headers=h, follow_redirects=True,
                          timeout=FETCH_TIMEOUT) as client:
            r = client.get(url)
            return FetchResponse(r.status_code, r.text,
                                 r.headers.get("content-type", ""))


class CurlCffiBackend(ScrapeBackend):
    """Impersona il fingerprint TLS di Chrome (curl_cffi).
    Bypassa i blocchi anti-bot su fingerprint TLS (es. 403 Cloudflare)
    senza avviare alcun browser. Usato come fallback automatico."""
    name = "curl_cffi-chrome"

    def fetch(self, url: str, headers: dict | None = None) -> FetchResponse:
        from curl_cffi import requests as cr
        h = {"Accept-Language": "it-IT,it;q=0.9,en;q=0.8"}
        if headers:
            h.update(headers)
        r = cr.get(url, headers=h, impersonate="chrome",
                   timeout=FETCH_TIMEOUT, allow_redirects=True)
        return FetchResponse(r.status_code, r.text,
                             r.headers.get("content-type", ""))


# ----------------------------------------------------------------- estrazione

class _StdlibText(HTMLParser):
    """Fallback povero ma robusto: testo pulito senza dipendenze."""
    _SKIP = {"script", "style", "noscript", "template", "svg", "head"}
    _BREAK = {"p", "div", "br", "li", "h1", "h2", "h3", "h4", "h5", "tr"}

    def __init__(self):
        super().__init__(convert_charrefs=True)
        self.parts: list[str] = []
        self._skip = 0

    def handle_starttag(self, tag, attrs):
        if tag in self._SKIP:
            self._skip += 1
        elif tag in self._BREAK:
            self.parts.append("\n")

    def handle_endtag(self, tag):
        if tag in self._SKIP and self._skip:
            self._skip -= 1

    def handle_data(self, data):
        if not self._skip and data.strip():
            self.parts.append(data.strip() + " ")

    def text(self) -> str:
        t = "".join(self.parts)
        t = re.sub(r"\n{3,}", "\n\n", t)
        return re.sub(r"[ \t]{2,}", " ", t).strip()


def _stdlib_meta(html: str) -> tuple[str, dict]:
    title, meta = "", {}
    m = re.search(r"<title[^>]*>(.*?)</title>", html, re.I | re.S)
    if m:
        title = re.sub(r"\s+", " ", m.group(1)).strip()
    for prop in ("description", "og:title", "og:description", "og:image"):
        m = re.search(
            r'<meta[^>]+(?:property|name)=["\']' + re.escape(prop)
            + r'["\'][^>]+content=["\'](.*?)["\']', html, re.I | re.S)
        if not m:
            m = re.search(
                r'<meta[^>]+content=["\'](.*?)["\'][^>]+(?:property|name)=["\']'
                + re.escape(prop) + r'["\']', html, re.I | re.S)
        if m:
            meta[prop] = re.sub(r"\s+", " ", m.group(1)).strip()
    return title, meta


def _extract(html: str, url: str) -> tuple[str, str, dict, str]:
    """Ritorna (title, markdown, meta, backend_usato)."""
    title, meta = _stdlib_meta(html)
    txt = trafilatura.extract(
        html, url=url, include_links=True, include_images=True,
        include_tables=True, output_format="markdown", favor_recall=True)
    if txt and len(txt.strip()) >= MIN_TEXT_LEN:
        return title, txt.strip(), meta, "httpx+trafilatura"
    txt = trafilatura.extract(
        html, url=url, output_format="markdown", favor_precision=False)
    if txt and len(txt.strip()) >= MIN_TEXT_LEN:
        return title, txt.strip(), meta, "httpx+trafilatura-recall"
    # fallback stdlib
    p = _StdlibText()
    try:
        p.feed(html)
        txt = p.text()
    except Exception:
        txt = ""
    return title, txt, meta, "httpx+fallback-stdlib"


# ----------------------------------------------------------------- robots

def _robots_allows(url: str) -> bool:
    try:
        parts = urlsplit(url)
        robots_url = f"{parts.scheme}://{parts.netloc}/robots.txt"
        with httpx.Client(timeout=6.0, follow_redirects=True) as c:
            r = c.get(robots_url, headers={"User-Agent": UA})
        if r.status_code != 200:
            return True
        rp = urllib.robotparser.RobotFileParser()
        rp.parse(r.text.splitlines())
        return rp.can_fetch("*", url)
    except Exception:
        return True


# ----------------------------------------------------------------- cache

def _cache_path(url: str) -> Path:
    h = hashlib.sha256(url.encode("utf-8")).hexdigest()[:24]
    return CACHE_DIR / f"{h}.json"


def _cache_load(url: str, ttl_h: int = CACHE_TTL_H) -> Optional[ScrapeResult]:
    p = _cache_path(url)
    if not p.exists():
        return None
    try:
        raw = json.loads(p.read_text(encoding="utf-8"))
        if (time.time() - raw.get("ts", 0)) / 3600 > ttl_h:
            return None
        res = ScrapeResult(**raw["result"])
        res.cached = True
        return res
    except Exception:
        return None


def _cache_save(res: ScrapeResult) -> None:
    try:
        CACHE_DIR.mkdir(exist_ok=True)
        d = res.to_dict()
        d.pop("html", None)  # la cache non deve contenere l'HTML grezzo
        payload = {"ts": time.time(), "result": d}
        _cache_path(res.url).write_text(
            json.dumps(payload, ensure_ascii=False), encoding="utf-8")
    except Exception:
        pass


# ----------------------------------------------------------------- API

def _fetch_with_retry(backend: ScrapeBackend, url: str,
                      attempts: int = 2) -> Optional[FetchResponse]:
    """Fetch con retry su errori transitori (timeout/rete/429/5xx)."""
    last_exc: Exception | None = None
    for i in range(max(1, attempts)):
        try:
            fr = backend.fetch(url)
            if fr.status in (408, 429, 500, 502, 503, 504) and i < attempts - 1:
                time.sleep(0.6 * (i + 1))
                continue
            return fr
        except Exception as e:
            last_exc = e
            if i < attempts - 1:
                time.sleep(0.6 * (i + 1))
    if last_exc:
        raise last_exc
    return None


def scrape(url: str, refresh: bool = False, use_cache: bool = True,
           respect_robots: bool = True,
           backend: ScrapeBackend | None = None,
           max_chars: int | None = None, keep_html: bool = False) -> ScrapeResult:
    """Scrapa una URL: estrae main-content in markdown, con cache e robots.
    keep_html=True bypassa la cache (serve HTML fresco per post-processing)."""
    t0 = time.time()
    res = ScrapeResult(url=url,
                       backend=backend.name if backend else "httpx+trafilatura")
    if use_cache and not refresh and not keep_html:
        hit = _cache_load(url)
        if hit:
            hit.elapsed_s = round(time.time() - t0, 3)
            if max_chars and hit.markdown:
                hit.markdown = hit.markdown[:max_chars]
            return hit
    if respect_robots and not _robots_allows(url):
        res.errore = "bloccato da robots.txt (usa --no-robots per forzare)"
        return res
    primary = backend or HttpxBackend()
    fr = None
    try:
        fr = _fetch_with_retry(primary, url)
    except Exception as e:
        res.warning = f"backend {primary.name}: {type(e).__name__}"
    # Fallback automatico su blocchi anti-bot (403/429/503): fingerprint TLS Chrome
    if fr is None or fr.status in (403, 429, 503):
        _first = f"HTTP {fr.status}" if fr is not None else res.warning
        try:
            fb = CurlCffiBackend()
            fr = _fetch_with_retry(fb, url, attempts=1)
            res.backend = fb.name
            res.warning = f"fallback TLS-chrome (primo tentativo: {_first})"
        except Exception as e:
            if fr is None:
                res.errore = f"fetch fallito: {type(e).__name__}: {e}"
                return res
    if fr is None:
        res.errore = "fetch fallito: nessuna risposta"
        return res
    res.status = fr.status
    if fr.status >= 400:
        res.errore = f"HTTP {fr.status}"
        return res
    ct = fr.content_type
    if ct and "html" not in ct and "xml" not in ct:
        res.backend = "raw"
        res.ok = True
        res.markdown = fr.html[:20000] if ("text" in ct or "json" in ct) else ""
        res.warning = f"content-type non HTML ({ct}): restituito raw (max 20k)"
    else:
        res.title, res.markdown, res.meta, be = _extract(fr.html, url)
        res.backend = be
        res.ok = bool(res.markdown)
        if res.ok and len(res.markdown) < MIN_TEXT_LEN:
            res.warning = ("contenuto scarso: probabile pagina JS-rendered "
                           "(nessun rendering headless attivo)")
        if not res.ok:
            res.errore = "estrazione senza risultato"
    if keep_html:
        res.html = fr.html
    res.elapsed_s = round(time.time() - t0, 3)
    if res.ok:
        _cache_save(res)
    if max_chars and res.markdown:
        res.markdown = res.markdown[:max_chars]
    return res


def estrai_strutturato(testo: str, schema: str, max_chars: int = 15000,
                       llm_call=None) -> dict:
    """Estrazione guidata da schema in linguaggio naturale.

    schema es: 'Restituisci SOLO JSON con chiavi: titolo, autore, punti_chiave'
    llm_call: callable(prompt) -> str; default = llm_core (lazy, fallback chain).
    """
    if llm_call is None:
        try:
            import llm_core
            def llm_call(prompt: str) -> str:
                out = llm_core.call_with_dynamic_fallback(
                    [{"role": "user", "content": prompt}])
                if isinstance(out, str):
                    return out
                if isinstance(out, dict):
                    return (out.get("content") or out.get("text")
                            or out.get("risposta") or json.dumps(out,
                                                                 ensure_ascii=False))
                c = getattr(out, "choices", None)
                if c:
                    return c[0].message.content
                return str(out)
        except Exception as e:
            return {"ok": False, "errore": f"llm_core non disponibile: {e}"}
    prompt = (
        "Estrai dati dal testo seguendo ESATTAMENTE questo schema. "
        "Rispondi SOLO con JSON valido.\n\nSCHEMA:\n" + schema +
        "\n\nTESTO:\n" + testo[:max_chars])
    try:
        raw = llm_call(prompt)
        m = re.search(r"\{.*\}", raw, re.S)
        return {"ok": bool(m),
                "dati": json.loads(m.group(0)) if m else None,
                "errore": "" if m else "nessun JSON nella risposta"}
    except Exception as e:
        return {"ok": False, "errore": f"{type(e).__name__}: {e}"}


# ----------------------------------------------------------------- extra

def estrai_links(html: str, base_url: str) -> dict:
    """Estrae link interni/esterni assoluti, dedup, max 500 ciascuno."""
    from urllib.parse import urljoin
    host = urlsplit(base_url).netloc.lower()
    interni, esterni = [], []
    for m in re.finditer(r'<a[^>]+href=["\']([^"\'#]+)["\']', html, re.I):
        href = m.group(1).strip()
        if href.lower().startswith(("mailto:", "javascript:", "tel:")):
            continue
        p = urlsplit(urljoin(base_url, href))
        if p.scheme not in ("http", "https"):
            continue
        absu = f"{p.scheme}://{p.netloc}{p.path}" + (f"?{p.query}" if p.query else "")
        (interni if p.netloc.lower() == host else esterni).append(absu)
    return {"interni": list(dict.fromkeys(interni))[:500],
            "esterni": list(dict.fromkeys(esterni))[:500]}


def estrai_jsonld(html: str) -> list:
    """Estrae i blocchi JSON-LD (schema.org) senza alcun LLM."""
    out = []
    for m in re.finditer(
            r'<script[^>]+type=["\']application/ld\+json["\'][^>]*>(.*?)</script>',
            html, re.I | re.S):
        try:
            data = json.loads(m.group(1).strip())
        except Exception:
            continue
        out.extend(data if isinstance(data, list) else [data])
    return out


def scrape_many(urls: list, workers: int = 4, **kw) -> list:
    """Scrape concorrente di piu' URL (ThreadPool, default 4 worker)."""
    from concurrent.futures import ThreadPoolExecutor
    with ThreadPoolExecutor(max_workers=max(1, workers)) as ex:
        return list(ex.map(lambda u: scrape(u, **kw), urls))


# ----------------------------------------------------------------- CLI

def _fmt(res: ScrapeResult) -> str:
    tag = "OK" if res.ok else "ERRORE"
    if res.cached:
        tag += "+cache"
    out = [f"[{tag}] {res.url}",
           f"  backend={res.backend}  status={res.status}  "
           f"elapsed={res.elapsed_s}s  chars={len(res.markdown)}"]
    if res.title:
        out.append(f"  titolo: {res.title}")
    if res.meta:
        out.append(f"  meta: {json.dumps(res.meta, ensure_ascii=False)[:300]}")
    if res.links:
        out.append(f"  links: {len(res.links.get('interni', []))} interni, "
                   f"{len(res.links.get('esterni', []))} esterni")
    if res.jsonld:
        out.append(f"  json-ld: {len(res.jsonld)} blocchi")
    if res.warning:
        out.append(f"  !! {res.warning}")
    if res.errore:
        out.append(f"  ERR: {res.errore}")
    if res.ok and res.markdown:
        out.append("")
        out.append(res.markdown)
    return "\n".join(out)


if __name__ == "__main__":
    import argparse
    import sys
    try:  # console Windows: evita UnicodeEncodeError (cp1252)
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    except Exception:
        pass
    ap = argparse.ArgumentParser(description="Scraper interno Astral")
    ap.add_argument("url", nargs="?", default=None)
    ap.add_argument("--refresh", action="store_true")
    ap.add_argument("--json", action="store_true")
    ap.add_argument("--no-cache", action="store_true")
    ap.add_argument("--no-robots", action="store_true")
    ap.add_argument("--links", action="store_true",
                    help="estrae anche i link interni/esterni")
    ap.add_argument("--jsonld", action="store_true",
                    help="estrae anche i blocchi JSON-LD (schema.org)")
    ap.add_argument("--batch", default=None, metavar="FILE",
                    help="file con una URL per riga: scrape concorrente")
    ap.add_argument("--max-chars", type=int, default=0,
                    help="tronca il markdown a N caratteri")
    a = ap.parse_args()
    mc = a.max_chars or None
    if a.batch:
        try:
            with open(a.batch, encoding="utf-8") as bf:
                urls = [l.strip() for l in bf
                        if l.strip() and not l.strip().startswith("#")]
        except Exception as e:
            print(f"ERR batch: {e}")
            sys.exit(1)
        res = scrape_many(urls, use_cache=not a.no_cache,
                          respect_robots=not a.no_robots, max_chars=mc)
        print(json.dumps([x.to_dict() for x in res], ensure_ascii=False))
        sys.exit(0)
    if not a.url:
        print(__doc__)
        sys.exit(0)
    r = scrape(a.url, refresh=a.refresh, use_cache=not a.no_cache,
               respect_robots=not a.no_robots, max_chars=mc,
               keep_html=a.links or a.jsonld)
    if r.html:
        if a.links:
            r.links = estrai_links(r.html, r.url)
        if a.jsonld:
            r.jsonld = estrai_jsonld(r.html)
    r.html = ""  # l'HTML grezzo non va stampato
    print(json.dumps(r.to_dict(), ensure_ascii=False) if a.json else _fmt(r))