# -*- coding: utf-8 -*-
# verdict/verdict.py - Orchestratore: broadcast parallelo -> sintesi anonimizzata -> output
import asyncio
import json
import os
import random
import time
from datetime import datetime, timedelta

from openai import OpenAI
from rich.markdown import Markdown
from rich.markup import escape
from rich.panel import Panel
from rich.rule import Rule

from core_io import console, log_error
from verdict.schemas import Parere, VerdettoFinale
from llm_core import client as _shared_client, print_telemetry

# --- Config consiglio -------------------------------------------------------
SIMBOLI = ["Delta", "Omega", "Sigma", "Lambda", "Theta", "Iota", "Kappa", "Rho"]

# (modello_openrouter, temperatura) - temperatura FORZATA <= 0.5 per tutti
PROFILI_GIUDICI = {
    "standard": [
        ("deepseek/deepseek-v4-pro", 0.3),
        ("z-ai/glm-5.3", 0.4),
        ("openai/gpt-5.6-sol", 0.5),
    ],
    "lite": [
        ("deepseek/deepseek-v4-flash-0731", 0.3),
        ("z-ai/glm-5.3-flash", 0.4),
        ("openai/gpt-5.6-luna", 0.5),
    ],
}
GIUDICI = PROFILI_GIUDICI["standard"]  # attivo, riassegnato da run_verdict(profilo=...)
MAX_TEMP = 0.5  # hard cap: nessun giudice puo' superarlo

# Ponderazione dei giudici (stessa scala del benchmark di routing, valori fittizi
# iniziali 0-10 per dimensione). La maggioranza pesata SOSTITUISCE la maggioranza
# semplice: conta il punteggio, non il numero di teste. Fonte: "benchmark_url" in
# config o override in .verdict_benchmark_override.json; refresh/caching locale.
# Peso giudice = qualita' pura (analisi + affidabilita'). Il costo NON pesa qui:
# e' un criterio del routing (dove si paga per chiamata), non del consiglio.
PESI_GIUDICI_DEFAULT = {
    "deepseek/deepseek-v4-pro": {"analisi": 8.5, "affidabilita": 8.0},
    "z-ai/glm-5.3": {"analisi": 8.0, "affidabilita": 8.5},
    "openai/gpt-5.6-sol": {"analisi": 9.0, "affidabilita": 9.0},
    "deepseek/deepseek-v4-flash-0731": {"analisi": 7.0, "affidabilita": 7.5},
    "z-ai/glm-5.3-flash": {"analisi": 7.5, "affidabilita": 7.5},
    "openai/gpt-5.6-luna": {"analisi": 8.0, "affidabilita": 8.5},
}
BENCHMARK_FILE = os.path.join("verdict", ".verdict_benchmark_cache.json")
BENCHMARK_TTL = 24 * 3600


def _pesi_giudici() -> dict:
    """Pesi con cache 24h: override file -> config benchmark_url -> default.
    Mai eccezioni verso l'utente: fallback sempre sui default."""
    try:
        if os.path.exists(BENCHMARK_FILE):
            with open(BENCHMARK_FILE, "r", encoding="utf-8") as f:
                data = json.load(f)
            if data and time.time() - data.get("ts", 0) < BENCHMARK_TTL:
                return data.get("pesi", PESI_GIUDICI_DEFAULT)
        from memory_store import get_config
        url = (get_config("benchmark_url", "") or "").strip()
        if url:
            import urllib.request
            with urllib.request.urlopen(url, timeout=10) as r:
                payload = json.loads(r.read().decode("utf-8"))
            pesi = payload.get("giudici", payload) if isinstance(payload, dict) else {}
            if isinstance(pesi, dict) and pesi:
                with open(BENCHMARK_FILE, "w", encoding="utf-8") as f:
                    json.dump({"ts": time.time(), "source": url, "pesi": pesi}, f, ensure_ascii=False)
                return pesi
    except Exception as e:
        log_error("verdict/pesi_giudici", e)
    return dict(PESI_GIUDICI_DEFAULT)

PROMPT_GIUDICE = (
    "Sei un giudice tecnico del consiglio Astral. Rispondi al quesito in modo "
    "conciso, operativo e fondato: analisi (max 5 punti), raccomandazione chiara, "
    "rischi/limiti. Massimo 350 parole(non per forza tutte). Non fare domande all'utente."
)

PROMPT_SINTESI = (
    "Ricevi i pareri ANONIMI di {n} giudici (identificati da codici, NON da nomi di "
    "modelli: non puoi sapere - e non devi inferire o dichiarare - quale modello abbia "
    "scritto cosa). Il tuo compito: produrre il VERDETTO del consiglio in markdown con "
    "esattamente queste sezioni:\n"
    "## Punti di accordo\n## Punti di disaccordo (citando i codici es. Giudice Delta)\n"
    "## Verdetto\n## Livello di confidenza (Alto/Medio/Basso + motivo)\n"
    "Regole: zero riferimento a modelli o vendor; massima sintesi; se i pareri "
    "contraddicono, dichiara esplicitamente il disaccordo invece di appiattirlo.\n\n"
    "PARERI:\n{pareri}"
)

PROMPT_RIVALUTAZIONE = (
    "Questa e' la tua precedente deliberazione anonima (codice {codice}):\n{precedente}\n\n"
    "Il consiglio, valutando i pareri con maggioranza pesata, ha adottato questa "
    "posizione prevalente:\n{sintesi}\n\n"
    "In massimo 120 parole: rivedi la tua posizione o confermala esplicitamente, "
    "indicando se aderisci alla posizione prevalente o mantieni un dissenso fondato "
    "(citi gli argomenti, mai altri modelli). Nessuna domanda all'utente."
)


def _giudici_assegnati():
    """Ruota l'ordine dei giudici e mescola i simboli: l'utente non puo' dedurre
    l'identita' dalla posizione. Mantiene temp<=MAX_TEMP."""
    pool = [(m, min(t, MAX_TEMP)) for m, t in GIUDICI]
    random.shuffle(pool)
    simboli = SIMBOLI[:]
    random.shuffle(simboli)
    return [(simboli[i], m, t) for i, (m, t) in enumerate(pool)]


def _peso_giudice(pesi: dict, modello: str) -> float:
    """Peso 0-10 di un giudice = qualita' pura: analisi 0.625 + affidabilita 0.375
    (rinormalizzazione di 0.5/0.3 dopo rimozione del costo, criterio solo del
    routing). Default neutro 7.5."""
    d = pesi.get(modello, {})
    return round(0.625 * float(d.get("analisi", 7.5))
                 + 0.375 * float(d.get("affidabilita", 7.5)), 3)


def _maggioranza_pesata(pareri, pesi: dict):
    """Majority weighted: ogni parere vale il peso del suo modello. Ritorna
    (posizione vincente [None se parita], dettagli per audit). NIENTE LLM: la
    ponderazione e' euristica e trasparente (verdetto: confidenza come gate,
    no giudice LLM)."""
    scores, dettaglio = {}, []
    for p in pareri:
        if not p.ok:
            continue
        w = _peso_giudice(pesi, p.modello)
        scores[p.codice] = w
        dettaglio.append({"codice": p.codice, "modello": p.modello, "peso": w})
    if not scores:
        return None, dettaglio
    top = max(scores.values())
    vincitori = [c for c, w in scores.items() if w == top]
    return (vincitori[0] if len(vincitori) == 1 else None), dettaglio


def _chiama_giudice(client, modello, temp, quesito):
    """Chiamata sincrona per un giudice (eseguita in thread separato)."""
    start = time.time()
    try:
        resp = client.chat.completions.create(
            model=modello,
            temperature=temp,
            messages=[
                {"role": "system", "content": PROMPT_GIUDICE},
                {"role": "user", "content": quesito},
            ],
        )
        testo = (resp.choices[0].message.content or "").strip()
        print_telemetry(resp, model_name=modello)
        return testo
    except Exception as e:
        log_error(f"verdict/giudice[{modello}]", e)
        return ""


async def _broadcast(client, assegnazioni, quesito):
    """Esegue tutti i giudici in parallelo (thread pool) e ritorna i Pareri."""
    loop = asyncio.get_running_loop()
    tasks = [
        loop.run_in_executor(
            None,
            lambda m=modello, t=temp: _chiama_giudice(client, m, t, quesito),
        )
        for _, modello, temp in assegnazioni
    ]
    risultati = await asyncio.gather(*tasks, return_exceptions=True)
    pareri = []
    for (codice, modello, temp), res in zip(assegnazioni, risultati):
        if isinstance(res, Exception):
            pareri.append(Parere(codice, modello, temp, errore=str(res)))
        elif not res:
            pareri.append(Parere(codice, modello, temp, errore="risposta vuota/errore API"))
        else:
            pareri.append(Parere(codice, modello, temp, risposta=res))
    return pareri


async def _rivaluta_dissenso(client, pareri, sintesi):
    """Fase ponderata: i giudici MINORANZA (peso < max) rivedono/ confermano la
    posizione prevalente; poi il primo disponibile del pool rivaluta la sintesi
    integrando gli eventuali dissensi rimasti. Failsafe: errore -> sintesi invariata."""
    pesi = _pesi_giudici()
    try:
        vincitore, dettaglio = _maggioranza_pesata(pareri, pesi)
        if not vincitore or not sintesi:
            return sintesi, None
        peso_max = _peso_giudice(pesi, vincitore_modello := next(
            p.modello for p in pareri if p.codice == vincitore))
        loop = asyncio.get_running_loop()
        tasks = []
        minori = [p for p in pareri if p.ok and p.codice != vincitore
                  and _peso_giudice(pesi, p.modello) < peso_max]
        for p in minori:
            prompt = PROMPT_RIVALUTAZIONE.format(codice=p.codice,
                                                 precedente=p.risposta, sintesi=sintesi)
            tasks.append(loop.run_in_executor(
                None, lambda mm=p.modello, tt=min(p.temp, MAX_TEMP), pp=prompt:
                _chiama_giudice(client, mm, tt, pp)))
        revisioni = [r for r in (await asyncio.gather(*tasks)) if r] if tasks else []
        if not revisioni:
            return sintesi, dettaglio
        blocco = "\n\n".join(f"[revisione {i + 1}]\n{r}" for i, r in enumerate(revisioni))
        prompt_finale = (
            "Questa e' la posizione adottata dal consiglio:\n" + sintesi
            + "\n\nQueste sono le revisioni/conferme dei giudici minoranza:\n" + blocco
            + "\n\nRiscrivi il verdetto (stesse sezioni, massima sintesi) integrando "
              "gli eventuali dissensi fondati rimasti. Zero riferimenti a modelli/vendor.")
        for modello, _t in GIUDICI:
            try:
                resp = client.chat.completions.create(
                    model=modello, temperature=MAX_TEMP,
                    messages=[{"role": "user", "content": prompt_finale}])
                testo = (resp.choices[0].message.content or "").strip()
                if testo:
                    print_telemetry(resp, model_name=modello)
                    return testo, dettaglio
            except Exception as e:
                log_error(f"verdict/rivaluta[{modello}]", e)
        return sintesi, dettaglio
    except Exception as e:
        log_error("verdict/rivaluta", e)
        return sintesi, None


def _sintetizza(client, pareri, quesito):
    """Un modello (il primo del pool) sintetizza i pareri anonimizzati."""
    blocco = "\n\n".join(
        f"[{p.codice}]\n{p.risposta if p.ok else '(non disponibile: ' + p.errore + ')'}"
        for p in pareri
    )
    prompt = PROMPT_SINTESI.format(n=len(pareri), pareri=blocco)
    modelli = [m for m, _ in GIUDICI]
    for modello in modelli:
        try:
            resp = client.chat.completions.create(
                model=modello,
                temperature=MAX_TEMP,
                messages=[
                    {"role": "system", "content": PROMPT_SINTESI.split("\n")[0]},
                    {"role": "user", "content": prompt},
                ],
            )
            testo = (resp.choices[0].message.content or "").strip()
            if testo:
                print_telemetry(resp, model_name=modello)
                return testo, modello
        except Exception as e:
            log_error(f"verdict/sintesi[{modello}]", e)
    return "", ""


def _stampa_verdetto(v: "VerdettoFinale"):
    console.print(Rule(style="dark_orange"))
    console.print(Panel(
        Markdown(v.sintesi or "*(sintesi non disponibile)*"),
        title="[bold dark_orange]VERDETTO DEL CONSIGLIO[/]",
        subtitle=f"[dim]{len([p for p in v.pareri if p.ok])}/{len(v.pareri)} giudici attivi[/dim]",
    ))
    # Mapping identita': SOLO ORA l'utente scopre chi e' chi (mai prima, mai nel testo)
    if v.mapping:
        righe = "\n".join(
            f"[bold]{escape(c)}[/] -> [dim]{escape(m)}[/] (temp {t})" for c, m, t in v.mapping
        )
        console.print(Panel(righe, title="[dim]Identita' rivelata (solo alla fine)[/]", border_style="dim"))
    # Ponderazione: peso di ciascun giudice e maggioranza pesata adottata
    if v.ponderazione:
        det = v.ponderazione
        vincente = max(det, key=lambda d: d["peso"]) if det else None
        righe_p = "\n".join(
            f"[bold]{escape(d['codice'])}[/] peso {d['peso']} [dim]({escape(str(d['modello']).split('/')[-1])})[/]"
            for d in det
        )
        if vincente:
            righe_p += f"\n[bold dark_orange]Maggioranza pesata: Giudice {escape(vincente['codice'])}[/]"
        console.print(Panel(righe_p, title="[dim]Ponderazione giudici (benchmark analisi/affidabilita')[/]", border_style="dim"))


async def run_verdict_async(client, quesito: str) -> "VerdettoFinale":
    assegnazioni = _giudici_assegnati()
    pareri = await _broadcast(client, assegnazioni, quesito)
    sintesi, modello_sint = _sintetizza(client, pareri, quesito)
    # Fase ponderata: minoranza rivede/ conferma -> verdetto finale rivalutato
    sintesi, ponderazione = await _rivaluta_dissenso(client, pareri, sintesi)
    return VerdettoFinale(
        quesito=quesito,
        pareri=pareri,
        sintesi=sintesi,
        modello_sintetizzatore=modello_sint,
        mapping=[(c, m, t) for c, m, t in assegnazioni],
        ponderazione=ponderazione,
    )


def _salva_verdetto(v, profilo: str, completo: bool):
    """Archivio storicizzato: NESSUN verdetto viene piu' perso (JSON con data/ora,
    profilo, quesito, pareri integrali e mapping)."""
    dt = datetime.now()
    os.makedirs(os.path.join("verdict", "archive"), exist_ok=True)
    slug = "".join(ch if ch.isalnum() else "_" for ch in v.quesito.strip().lower())[:60].strip("_") or "quesito"
    record = {
        "timestamp": dt.isoformat(timespec="seconds"),
        "profilo": profilo,
        "completo": completo,
        "quesito": v.quesito,
        "sintesi": v.sintesi,
        "modello_sintetizzatore": v.modello_sintetizzatore,
        "mapping": [{"codice": c, "modello": m, "temp": t} for c, m, t in v.mapping],
        "ponderazione": getattr(v, "ponderazione", None),
        "pareri": [
            {"codice": p.codice, "modello": p.modello, "temp": p.temp, "ok": p.ok,
             "risposta": p.risposta if p.ok else "", "errore": p.errore}
            for p in v.pareri
        ],
    }
    fname = os.path.join("verdict", "archive", dt.strftime("%Y%m%d_%H%M%S") + f"_{profilo}_{slug}.json")
    try:
        with open(fname, "w", encoding="utf-8") as f:
            json.dump(record, f, ensure_ascii=False, indent=2)
        console.print(f"[dim][OK] Verdetto archiviato: {fname}[/]")
    except Exception as e:
        log_error("verdict/salvataggio", e)
        console.print("[bold orange_red1][!] Salvataggio archivio fallito: dettagli in error_log.txt.[/]")


def run_verdict(quesito: str, profilo: str = "standard"):
    """Entry point sincrono per astral.py. Non aggiunge nulla alla cronologia
    conversazione: il verdetto e' un canale separato."""
    global GIUDICI
    profilo = (profilo or "standard").strip().lower()
    if profilo not in PROFILI_GIUDICI:
        console.print(f"[bold orange_red1][!] Profilo '{profilo}' sconosciuto: uso 'standard'.[/]")
        profilo = "standard"
    GIUDICI = PROFILI_GIUDICI[profilo]
    if not quesito.strip():
        console.print("[bold orange_red1][!] Specifica un quesito: /verdict <domanda>[/]")
        return
    client = _shared_client
    console.print(f"[dim][*] Consiglio convocato ({profilo}): {len(GIUDICI)} giudici, temp max {MAX_TEMP}. Quesito:[/] {escape(quesito)}")
    with console.status("[bold dodger_blue1]I giudici deliberano in parallelo...[/]", spinner="dots"):
        v = asyncio.run(run_verdict_async(client, quesito))
    _stampa_verdetto(v)
    attivi = [p for p in v.pareri if p.ok]
    completo = bool(attivi) and bool(v.sintesi)
    if not completo:
        console.print("[bold orange_red1][!] Consiglio incompleto: dettagli in error_log.txt.[/]")
    _salva_verdetto(v, profilo, completo)