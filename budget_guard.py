# -*- coding: utf-8 -*-
# budget_guard.py - Check credito residuo OpenRouter (UNA volta per processo).
# Fonte unica del consumo rimanente: endpoint /api/v1/key, che NON consuma
# crediti. Gli usage_log stimano solo il passato: il saldo vero sta qui.
# Fail-silent: offline/key assente -> (None, None) e il routing resta sui
# pesi base. Nessun retry nel processo (done=True anche in errore).

import json
import os
import urllib.request

_CACHE = {"done": False, "remaining": None, "total": None}


def _api_key():
    key = os.getenv("OPENROUTER_API_KEY", "").strip()
    if key:
        return key
    try:
        import winreg
        env_key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, "Environment")
        return str(winreg.QueryValueEx(env_key, "OPENROUTER_API_KEY")[0]).strip()
    except Exception:
        return ""


def check_credits(force=False):
    """Ritorna (remaining, total) in USD, oppure (None, None) se non noti.

    total=None significa limite illimitato/pagamento a consumo: in quel caso
    il routing non applica alcun tier budget.
    """
    if _CACHE["done"] and not force:
        return _CACHE["remaining"], _CACHE["total"]
    _CACHE["done"] = True
    key = _api_key()
    if not key:
        return None, None
    try:
        req = urllib.request.Request(
            "https://openrouter.ai/api/v1/key",
            headers={"Authorization": "Bearer " + key})
        with urllib.request.urlopen(req, timeout=8) as r:
            data = json.load(r).get("data", {}) or {}
        rem = data.get("limit_remaining")
        tot = data.get("limit")
        _CACHE["remaining"] = float(rem) if rem is not None else None
        _CACHE["total"] = float(tot) if tot is not None else None
    except Exception:
        _CACHE["remaining"], _CACHE["total"] = None, None
    return _CACHE["remaining"], _CACHE["total"]
