# -*- coding: utf-8 -*-
"""Evidenze e limiti allegati ai risultati dei tool (pattern da REA).

Un risultato di tool non deve essere un'affermazione nuda: porta con se'
  * 'evidenze' -> prove concrete e verificabili su cui il risultato si basa;
  * 'limiti'   -> cosa il risultato NON dimostra o dove resta incompleto.

E' post-processing READ-ONLY del payload gia' prodotto dal tool: non esegue
nulla di nuovo e non altera gli effetti dell'operazione.

Fail-open: se l'arricchimento solleva un'eccezione il payload viene restituito
INTATTO e identico all'originale.
"""

MAX_EVIDENZE = 4      # tetto per risultato (token-saving)
MAX_LIMITI = 3        # tetto per risultato (token-saving)
MAX_VOCE_CHARS = 160  # lunghezza massima di una singola voce

_FAIL_STATUSES = ("fail", "failed", "error", "timeout")


def _clip(valore):
    """Rende una voce compatta, su una riga sola e di lunghezza limitata."""
    try:
        testo = valore if isinstance(valore, str) else repr(valore)
        testo = " ".join(testo.split())
    except Exception:
        return "<non serializzabile>"
    if len(testo) > MAX_VOCE_CHARS:
        testo = testo[:MAX_VOCE_CHARS - 1] + "\u2026"
    return testo


def _primo_esistente(dati, chiavi):
    """Ritorna la prima chiave presente in 'dati' fra quelle indicate."""
    for chiave in chiavi:
        if chiave in dati:
            return chiave, dati[chiave]
    return None, None


def _status_fallito(dati):
    """True se il payload segnala un esito negativo non ambiguo."""
    for chiave in ("status", "esito", "outcome", "state"):
        valore = dati.get(chiave)
        if isinstance(valore, str) and valore.strip().lower() in _FAIL_STATUSES:
            return True
    return False


def _evidenze_generiche(tool_name, dati):
    """Evidenza minima sempre disponibile: struttura e segnale di errore."""
    evidenze = ["tool=%s" % tool_name]
    if dati:
        chiavi = sorted(str(k) for k in dati.keys())
        evidenze.append("chiavi_payload=%s" % ",".join(chiavi[:12]))
    if "error" in dati:
        evidenze.append("campo_error=%s" % _clip(dati.get("error")))
    return evidenze[:MAX_EVIDENZE]


def _limiti_generici(tool_name, dati):
    """Limiti dichiarati in modo esplicito e prudente.

    Non si afferma nulla che non sia deducibile dal payload stesso.
    """
    limiti = []
    if _status_fallito(dati):
        limiti.append("esito negativo dichiarato dal tool: nessun effetto da considerare riuscito")
    if "truncated" in dati or "troncato" in dati:
        if dati.get("truncated") or dati.get("troncato"):
            limiti.append("output troncato: esistono dati oltre il limite mostrato")
    return limiti[:MAX_LIMITI]


def _arricchisci_powershell(dati, evidenze, limiti):
    chiave, valore = _primo_esistente(dati, ("exit_code", "exitcode", "returncode"))
    if chiave is not None:
        evidenze.append("%s=%s" % (chiave, _clip(valore)))
        if isinstance(valore, int) and valore != 0:
            limiti.append("comando terminato con codice diverso da zero")
    chiave, valore = _primo_esistente(dati, ("stdout", "output"))
    if isinstance(valore, str):
        evidenze.append("righe_stdout=%d" % len(valore.splitlines()))
        if not valore.strip():
            limiti.append("stdout vuoto: l'assenza di output non e' prova di assenza di effetto")
    return evidenze, limiti


def _arricchisci_filesystem(dati, evidenze, limiti):
    for chiave in ("path", "folder_path", "percorso"):
        if chiave in dati:
            evidenze.append("path=%s" % _clip(dati.get(chiave)))
            break
    for chiave in ("files", "results", "items", "elements", "matches"):
        valore = dati.get(chiave)
        if isinstance(valore, (list, tuple)):
            evidenze.append("%s=%d" % (chiave, len(valore)))
            break
    if dati.get("errors"):
        limiti.append("alcune voci non sono state processate (campo errors valorizzato)")
    return evidenze, limiti


def _arricchisci_recall(dati, evidenze, limiti):
    for chiave in ("hash", "hash_full", "id"):
        if chiave in dati:
            evidenze.append("hash=%s" % _clip(dati.get(chiave)))
            break
    contenuto = dati.get("content")
    if isinstance(contenuto, str):
        evidenze.append("chars_contenuto=%d" % len(contenuto))
    return evidenze, limiti


_ROUTER_SPECIFICI = (
    (("run_powershell_cmd", "powershell", "shell"), _arricchisci_powershell),
    (("scan_storage", "move_to_trash", "find_heavy"), _arricchisci_filesystem),
    (("recall",), _arricchisci_recall),
)


def _per_tool(tool_name, dati, evidenze, limiti):
    """Applica gli arricchitori specifici pertinenti al tool."""
    nome = (tool_name or "").lower()
    for prefissi, arricchitore in _ROUTER_SPECIFICI:
        if any(p in nome for p in prefissi):
            return arricchitore(dati, evidenze, limiti)
    return evidenze, limiti


def enrich_tool_result(result, tool_name=""):
    """Allega 'evidenze' e 'limiti' al payload del tool. Fail-open garantito.

    Ritorna sempre un nuovo dict quando l'input e' un dict; in ogni altro caso
    (o in caso di eccezione) ritorna l'oggetto originale senza modifiche.
    """
    try:
        if not isinstance(result, dict):
            return result
        if "evidenze" in result and "limiti" in result:
            return result
        dati = result
        evidenze = _evidenze_generiche(tool_name, dati)
        limiti = _limiti_generici(tool_name, dati)
        evidenze, limiti = _per_tool(tool_name, dati, evidenze, limiti)
        arricchito = dict(dati)
        arricchito["evidenze"] = evidenze[:MAX_EVIDENZE]
        arricchito["limiti"] = limiti[:MAX_LIMITI]
        return arricchito
    except Exception:
        return result

