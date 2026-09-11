# -*- coding: utf-8 -*-
# llm_core.py - Config LLM, routing, fallback dinamico, telemetria, self-repair
import os
from dotenv import load_dotenv
import re
import sys
import json
import time
import sqlite3
import threading
import subprocess
from datetime import datetime
from openai import OpenAI

from rich.panel import Panel
from rich.markdown import Markdown
from rich.markup import escape

from core_io import BASE_DIR, console, log_error, safe_print
from memory_store import TELEMETRY_DB

def load_api_key():
    """Legge la chiave dall'ambiente, con fallback al registro utente Windows."""
    load_dotenv()
    key = os.getenv("OPENROUTER_API_KEY", "").strip()
    if key or os.name != "nt":
        return key
    try:
        import winreg
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, r"Environment") as env_key:
            return str(winreg.QueryValueEx(env_key, "OPENROUTER_API_KEY")[0]).strip()
    except (FileNotFoundError, OSError):
        return ""

API_KEY = load_api_key()
if not API_KEY:
    console.print("[bold orange_red1][!] Variabile OPENROUTER_API_KEY non configurata.[/]")
    raise SystemExit(1)
telemetry_enabled = True

# Modelli dedicati al routing dinamico intelligente (tutti verificati e operativi)
MODEL_CONVERSATION = "deepseek/deepseek-v4-flash"
MODEL_CODE = "z-ai/glm-5.3-flash"

MODELS = {
    "deepseek": "deepseek/deepseek-v4-flash",
    "1": "deepseek/deepseek-v4-flash",
    "deepseek-flash": "deepseek/deepseek-v4-flash",
    "z-ai": "z-ai/glm-5.3-flash",
    "glm": "z-ai/glm-5.3-flash",
    "sol": "openai/gpt-5.6-sol",
    "5.6": "openai/gpt-5.6-sol",
    "gpt-5.6": "openai/gpt-5.6-sol",
    "auto": "auto"
}

DYNAMIC_MODELS_POOL = [
    "deepseek/deepseek-v4-flash",
    "z-ai/glm-5.3-flash"
]

current_model = "auto"
client = OpenAI(base_url="https://openrouter.ai/api/v1", api_key=API_KEY)

SYSTEM_INSTRUCTION = (
    "HOME: La cartella principale del progetto e' C:\\Users\\masci\\Astral; il file madre attualmente in esecuzione e' astral.py (percorso completo: C:\\Users\\masci\\Astral\\astral.py).\n"
    "ROLE: Assistente IA integrato in Windows 11 con controllo file e PowerShell.\n"
    "SCOPO: Essere un assistente Windows 11 efficiente e migliorare attivamente se stesso e il proprio codice sorgente.\n"
    "DIRECTIVES:\n"
    "- AUTONOMY: Esegui azioni ordinarie SENZA chiedere conferma per velocizzare.\n"
    "- SELF-IMPROVEMENT: Ottimizza, rifattorizza e correggi autonomamente il codice sorgente dell'applicazione quando opportuno o richiesto.\n"
    "- ONE-SHOT: Accorpa piu azioni in un singolo script PowerShell per minimizzare le chiamate e risparmiare token.\n"
    "- SAFETY: Chiedi conferma ESPLICITA SOLO se l'operazione coinvolge file/cartelle di sistema (es. C:\\Windows, System32, Program Files).\n"
    "- TOKEN SAVING: L'output dei tool e' gia' troncato automaticamente con caps standard (20 errori, 10 warning, 20 righe lista, 50 inventario). NON serve usare Select-Object -First. Se ti serve il dato completo, usa il tool recall con l'hash dell'hint [full output: recall <hash>] invece di rieseguire il comando.\n"
    "- OUTPUT FORMAT: Risposte sintetiche, chiare e prive di ridondanze.\n"
    "- CODE PATCHING: Per modificare file di codice usa SEMPRE il tool apply_code_patch (blocchi search/replace esatti, atomici). NON usare PowerShell per sovrascrivere o stampare file interi: genera output chilometrici e spreca token.\n"
    "- AUTO-TESTING: Dopo ogni modifica al codice, verifica con test_python_file (mode='compile' per sintassi, mode='run' per dry-run). Durante ragionamenti multi-step, applica modifiche e test in modo SILENZIOSO: riporta all'utente solo l'esito finale.\n"
    "- MODULARIZZAZIONE: Se astral.py supera ~1500 righe, separa la logica dei tools in moduli dedicati (es. tools_patch.py, tools_test.py) importandoli dal file madre."
)

# --- Recall Store (ispirato a rtk retriever) ---

# --- Caps standardizzati (ispirati a rtk) ---







# --- Checkpoint store + Loop detection (ispirato a token-optimizer resume-checkpoint/looping) ---







def is_code_query(text: str) -> bool:
    """Rileva se il testo richiede elaborazione di codice, scripting o query tecnica."""
    if any(marker in text for marker in ["`", "def ", "class ", "import ", "function", "=>", "const ", "var ", "let ", "public static", "#include"]):
        return True

    code_keywords = [
        "codice", "script", "programma", "sviluppa", "funzione", "algoritmo",
        "python", "powershell", "bash", "cmd", "batch", ".bat", ".ps1", ".py", ".exe",
        "javascript", "typescript", "html", "css", "sql", "c#", "c++", "rust", "golang",
        "debug", "bug", "errore", "exception", "traceback", "syntax", "refactor",
        "compila", "build", "pyinstaller", "pip", "npm", "git", "regex", "json",
        "endpoint", "api rest", "query", "database", "terminale", "powershell", "classe"
    ]
    lower = text.lower()
    for kw in code_keywords:
        if re.search(r'\b' + re.escape(kw) + r'\b', lower):
            return True
    return False

def route_model(user_input: str) -> str:
    """Seleziona dinamicamente il modello ottimale in base al tipo di richiesta."""
    if current_model != "auto":
        return current_model
    if is_code_query(user_input):
        return MODEL_CODE
    return MODEL_CONVERSATION

def save_telemetry(model_name, prompt_t, resp_t, total_t):
    """Salva telemetria persistente divisa per IA (modello)."""
    try:
        conn = sqlite3.connect(TELEMETRY_DB)
        conn.execute("INSERT INTO telemetry VALUES (?,?,?,?,?)",
            (model_name, time.time(), prompt_t, resp_t, total_t))
        # Retention: cancella dati piu vecchi di 90 giorni
        conn.execute("DELETE FROM telemetry WHERE ts < ?", (time.time() - 90 * 86400,))
        conn.commit()
        conn.close()
    except Exception as e:
        log_error("save_telemetry", e)

def print_telemetry(response, model_name=None):
    global telemetry_enabled
    if not telemetry_enabled or not response:
        return
    usage = getattr(response, "usage", None)
    if usage:
        prompt_t = getattr(usage, "prompt_tokens", 0) or 0
        resp_t = getattr(usage, "completion_tokens", 0) or 0
        total_t = getattr(usage, "total_tokens", 0) or (prompt_t + resp_t)
        console.print(f"[dim italic]>> Telemetria Token | Input: [gold1]{prompt_t}[/] | Output: [dark_orange]{resp_t}[/] | Totale turno: [bold grey100]{total_t}[/][/dim italic]")
        if model_name:
            save_telemetry(model_name, prompt_t, resp_t, total_t)


def get_telemetry_stats(model_filter=None, days=30):
    """Restituisce statistiche telemetria aggregate per modello."""
    try:
        import sqlite3
        conn = sqlite3.connect(TELEMETRY_DB)
        cutoff = time.time() - days * 86400
        if model_filter:
            rows = conn.execute("SELECT model_name, COUNT(*), SUM(prompt_tokens), SUM(completion_tokens), SUM(total_tokens) FROM telemetry WHERE ts > ? AND model_name = ? GROUP BY model_name ORDER BY SUM(total_tokens) DESC", (cutoff, model_filter)).fetchall()
        else:
            rows = conn.execute("SELECT model_name, COUNT(*), SUM(prompt_tokens), SUM(completion_tokens), SUM(total_tokens) FROM telemetry WHERE ts > ? GROUP BY model_name ORDER BY SUM(total_tokens) DESC", (cutoff,)).fetchall()
        conn.close()
        if not rows:
            return "Nessun dato telemetria disponibile."
        lines = ["ðŸ“Š Statistiche telemetria (ultimi %d giorni):" % days]
        for r in rows:
            model, count, p_in, p_out, p_total = r
            avg_total = p_total // count if count else 0
            lines.append(f"  [{model}] {count} chiamate | Input: {p_in} | Output: {p_out} | Totale: {p_total} token | Media/turno: {avg_total}")
        return "\\n".join(lines)
    except Exception as e:
        log_error("get_telemetry_stats", e)
        return "Errore nel recupero statistiche: %s" % e



# --- Accessori stato (il REPL aggiorna i globali senza violare l'incapsulamento) ---

def get_current_model():
    return current_model

def set_current_model(value):
    global current_model
    current_model = value

def get_telemetry_enabled():
    return telemetry_enabled

def set_telemetry_enabled(value):
    global telemetry_enabled
    telemetry_enabled = value
def call_with_dynamic_fallback(messages, tools_schema=None, primary_model=None):
    """
    Esegue la chiamata partendo dal modello target (DeepSeek per chat).
    Se incontra errori o rate limit, itera dinamicamente attraverso gli altri modelli del pool.
    """
    if not primary_model:
        primary_model = MODEL_CONVERSATION
    
    models_to_try = [primary_model] + [m for m in DYNAMIC_MODELS_POOL if m != primary_model]
    last_error = ""

    # Include sempre il System Prompt per definire istruzioni, ruoli e limiti
    payload_messages = [{"role": "system", "content": SYSTEM_INSTRUCTION}] + messages

    for attempt_model in models_to_try:
        try:
            with console.status(f"[bold dodger_blue1]Elaborazione con {attempt_model}...[/]", spinner="dots"):
                kwargs = {
                    "model": attempt_model,
                    "messages": payload_messages,
                }
                extra_params = {}
                kwargs.update(extra_params)
                if tools_schema:
                    kwargs["tools"] = tools_schema
                try:
                    response = client.chat.completions.create(**kwargs)
                except Exception as e_:
                    err_str = str(e_).lower()
                    if extra_params and any(w in err_str for w in ["reasoning_effort","temperature","top_p","not supported","unknown parameter","invalid parameter","bad request"]):
                        console.print(f"[dim][*] {attempt_model} non supporta parametri avanzati. Riprovo senza...[/dim]")
                        kwargs.pop("reasoning_effort", None)
                        kwargs.pop("temperature", None)
                        kwargs.pop("top_p", None)
                        response = client.chat.completions.create(**kwargs)
                    else:
                        raise

            if attempt_model != primary_model:
                console.print(f"[dim][*] Fallback dinamico attivato: risposta completata con {attempt_model}[/dim]")
            return response, attempt_model
        except Exception as e:
            last_error = str(e)
            console.print(f"[dim][!] {escape(str(attempt_model))} fallito: {escape(str(e))}. Fallback dinamico in corso...[/dim]")
            continue

    raise RuntimeError(f"Tutti i modelli nel pool dinamico sono falliti. Ultimo errore: {last_error}")

def auto_repair():
    log_file = os.path.join(BASE_DIR, "error_log.txt")
    if not os.path.exists(log_file):
        console.print("[dim][*] Nessun log di errore trovato.[/dim]")
        return
    with open(log_file, "r", encoding="utf-8") as f:
        content = f.read()
    if not content.strip():
        return
    
    prompt = f"Sei un assistente diagnostico. Analizza questo log:\n\n{content[-2000:]}"
    try:
        resp, used_model = call_with_dynamic_fallback([{"role": "user", "content": prompt}], primary_model=MODEL_CODE)
        if resp.choices and resp.choices[0].message.content:
            console.print(Panel(Markdown(resp.choices[0].message.content.strip()), title=f"[bold spring_green1]Report Diagnosi ({used_model})[/]", border_style="green"))
    except Exception as e:
        console.print(f"[bold orange_red1]Errore auto-riparazione:[/] {escape(str(e))}")

def repair_watchdog():
    """Thread demone: monitora error_log.txt; a ogni nuova entry ERROR avvia
    un processo astral.py --repair (console separata) che analizza e ripara,
    senza mai fermare il loop principale (self-healing)."""
    def _watch():
        log_file = os.path.join(BASE_DIR, "error_log.txt")
        last_seen = ""
        try:
            if os.path.exists(log_file):
                with open(log_file, "r", encoding="utf-8", errors="replace") as f:
                    tail = f.read()[-4000:]
                for line in tail.splitlines():
                    if "] ERROR" in line:
                        last_seen = line[:30]
                        break
        except Exception:
            pass
        while True:
            time.sleep(5)
            try:
                if not os.path.exists(log_file):
                    continue
                with open(log_file, "r", encoding="utf-8", errors="replace") as f:
                    tail = f.read()[-4000:]
                cand = ""
                for line in tail.splitlines():
                    if "] ERROR" in line:
                        cand = line[:30]
                        break
                if cand and cand != last_seen:
                    last_seen = cand
                    safe_print("[watchdog] Nuovo errore nel log: avvio self-repair in console separata...")
                    subprocess.Popen(
                        [sys.executable, os.path.join(BASE_DIR, "astral.py"), "--repair"],
                        cwd=BASE_DIR,
                        creationflags=getattr(subprocess, "CREATE_NEW_CONSOLE", 0),
                    )
            except Exception:
                pass
    try:
        threading.Thread(target=_watch, daemon=True).start()
    except Exception:
        pass
