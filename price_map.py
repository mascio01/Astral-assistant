"""
P3 - price_map.py: tariffe LLM (USD per 1M token) per il report /usage.
- Match per sottostringa case-insensitive (vince la chiave piu' lunga/specifica).
- Override utente: file prices_override.json nella root Astral
  {"modello-parziale": [prompt_usd_mtok, completion_usd_mtok]}
Prezzi indicativi di listino: EDITARE qui o via override JSON senza toccare altro codice.
"""

import json
import os

_CACHE_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), ".or_models_cache.json")

_DIR = os.path.dirname(os.path.abspath(__file__))
_OVERRIDE_FILE = os.path.join(_DIR, "prices_override.json")

# family/model-parziale: (prompt USD/1M, completion USD/1M)
# NB: get_price fa match per ID ESATTO (il catalogo non fa piu' sottostringa),
# quindi le voci utili sono gli ID pieni; gli alias brevi servono a /model.
PRICES = {
    "deepseek/deepseek-v4.1-flash": (0.10, 0.50),
    "deepseek/deepseek-v4-flash-vision-exp": (0.22, 0.66),
    "openai/gpt-6-luna": (0.10, 0.50),
    "deepseek/deepseek-v4-flash-0731": (0.04, 0.64),
    "deepseek-v4-flash-0731": (0.04, 0.64),
    "deepseek-v4-flash-latest": (0.0352, 0.1056),   # alias '~...-latest' SCONTATO (OpenRouter)
    "deepseek": (0.27, 1.10),
    "glm-5.3-flash": (0.10, 0.30),
    "deepseek-v4.1-flash": (0.10, 0.50),
    "deepseek-v4-flash-vision-exp": (0.22, 0.66),
    "gpt-6-luna": (0.10, 0.50),
    "glm": (0.60, 2.20),
    "qwen": (0.15, 0.60),
    "llama": (0.10, 0.40),
    "mistral": (0.15, 0.60),
    "gpt-4o-mini": (0.15, 0.60),
    "gpt-4o": (2.50, 10.00),
    "gpt-4.1-mini": (0.40, 1.60),
    "gpt-4.1": (2.00, 8.00),
    "o4-mini": (1.10, 4.40),
    "claude-haiku": (0.80, 4.00),
    "claude-sonnet": (3.00, 15.00),
    "claude-opus": (15.00, 75.00),
    "gemini-2.5-flash": (0.30, 2.50),
    "gemini-2.0-flash": (0.10, 0.40),
    "gemini": (1.25, 10.00),
    "grok": (3.00, 15.00),
}

# Override utente (se presente, caricato all'avvio)
try:
    if os.path.exists(_OVERRIDE_FILE):
        with open(_OVERRIDE_FILE, "r", encoding="utf-8") as _f:
            for _k, _v in json.load(_f).items():
                PRICES[str(_k)] = (float(_v[0]), float(_v[1]))
except Exception:
    pass


def _catalog_prices():
    """Prezzi OpenRouter per ID ESATTO, inclusi alias '~' e varianti.

    Il catalogo e' la fonte autorevole: non facciamo piu' matching per
    sottostringa, perche' ``deepseek/...-flash`` non equivale a ``deepseek``.
    """
    try:
        with open(_CACHE_FILE, "r", encoding="utf-8") as f:
            data = json.load(f)
        return {
            str(item["id"]).lower(): (
                float(item["prompt"]) * 1_000_000,
                float(item["completion"]) * 1_000_000,
            )
            for item in data.get("models", [])
            if item.get("id") and item.get("prompt") is not None
            and item.get("completion") is not None
        }
    except (OSError, ValueError, TypeError, KeyError):
        return {}


def get_price(model):
    """Ritorna (prompt, completion) USD/1M per l'ID esatto, o None.

    Ordine: override locale esatto, catalogo OpenRouter esatto, tabella
    statica esatta. Il prefisso '~' e il suffisso ':batch' NON vengono
    rimossi: identificano prezzi diversi e devono restare distinguibili.
    """
    if not model:
        return None
    m = str(model).strip().lower()
    if m in PRICES:
        return PRICES[m]
    return _catalog_prices().get(m)


def is_discounted_alias(model):
    """True se l'ID usa il prefisso '~' di OpenRouter (alias 'latest',
    pubblicato in genere a prezzo SCONTATO rispetto alla versione stabile)."""
    return bool(model) and str(model).startswith("~")


def cost_usd(model, prompt_tokens, completion_tokens):
    """Costo totale USD della chiamata; None se prezzo sconosciuto."""
    p = get_price(model)
    if p is None:
        return None
    return (int(prompt_tokens or 0) / 1000000.0) * p[0] + (int(completion_tokens or 0) / 1000000.0) * p[1]


def all_prices():
    """Lista [(match, prompt, completion)] ordinata alfabeticamente."""
    return sorted((k, v[0], v[1]) for k, v in PRICES.items())
