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
from memory_store import TELEMETRY_DB, get_config
from exec_logger import log_execution
import random
import collections


# --- Rate Limiter: sliding window (anti-boom) ---------------------------------
_CALL_TIMESTAMPS = collections.deque()


def _get_rate_config():
    """Legge i parametri del rate limiter dalla config persistente (SQLite)."""
    max_calls = int(get_config("rate_limit_max_calls", "35"))
    window = int(get_config("rate_limit_window_seconds", "60"))
    return max_calls, window


def _check_rate_limit():
    """Sliding window rate limiter. Parametri N/W letti dinamicamente dalla config store."""
    max_calls, window = _get_rate_config()
    now = time.time()
    while _CALL_TIMESTAMPS and _CALL_TIMESTAMPS[0] < now - window:
        _CALL_TIMESTAMPS.popleft()

    if len(_CALL_TIMESTAMPS) >= max_calls:
        sleep_time = _CALL_TIMESTAMPS[0] + window - now
        if sleep_time > 0:
            console.print(f"[dim][rate-limit] Sforato limite {max_calls} chiamate/{window}s. Attendo {sleep_time:.1f}s...[/dim]")
            time.sleep(sleep_time)
        _CALL_TIMESTAMPS.popleft()

    _CALL_TIMESTAMPS.append(time.time())


def _rate_limited_call(**kwargs):
    """W.per il client.chat.completions.create con rate limiter."""
    _check_rate_limit()
    return client.chat.completions.create(**kwargs)


# --- n8n Pattern 1: errori tipizzati ---------------------------------------
class ErroreAstral(RuntimeError):
    """Errore tipizzato layer LLM (sottoclasse RuntimeError: compatibile con i catch esistenti)."""
    CODES = {"RATE_LIMIT", "TIMEOUT", "AUTH", "QUOTA", "SERVER", "SCONOSCIUTO"}

    def __init__(self, code, message="", cause=None):
        if code not in self.CODES:
            code = "SCONOSCIUTO"
        self.code = code
        self.cause = cause
        super().__init__(f"[{code}] {message}" if message else f"[{code}]")


def _classifica_errore(exc) -> str:
    """Mappa un'eccezione generica a un codice ErroreAstral."""
    s = str(exc).lower()
    if "429" in s or "rate limit" in s or "too many requests" in s:
        return "RATE_LIMIT"
    if "401" in s or "403" in s or "unauthorized" in s or "invalid api key" in s:
        return "AUTH"
    if "402" in s or "quota" in s or "insufficient" in s or "billing" in s:
        return "QUOTA"
    if "timeout" in s or "timed out" in s or "connection" in s:
        return "TIMEOUT"
    if "500" in s or "502" in s or "503" in s or "server error" in s:
        return "SERVER"
    return "SCONOSCIUTO"


# --- n8n Pattern 2: retry con backoff esponenziale (1s/2s + jitter) ---------
RETRYABLE_CODES = {"RATE_LIMIT", "TIMEOUT", "SERVER"}
RETRY_MAX = 2            # tentativi extra oltre il primo
RETRY_BASE_DELAY = 1.0   # secondi


def _with_retry(fn, op="llm_call", log=None):
    """Esegue fn() con retry esponenziale sui codici retryable; al termine rilancia ErroreAstral tipizzato."""
    last_exc = None
    for attempt in range(RETRY_MAX + 1):
        try:
            return fn()
        except Exception as e:
            code = _classifica_errore(e)
            last_exc = ErroreAstral(code, str(e), cause=e)
            if code not in RETRYABLE_CODES or attempt >= RETRY_MAX:
                raise last_exc from e
            delay = RETRY_BASE_DELAY * (2 ** attempt) + random.uniform(0, 0.3)
            if log:
                log(f"[dim][retry {attempt + 1}/{RETRY_MAX}] {op}: {code} -> riprovo tra {delay:.1f}s[/dim]")
            time.sleep(delay)
    raise last_exc  # unreachable

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
MODEL_CONVERSATION = "deepseek/deepseek-v4-flash-0731"
MODEL_CODE = "z-ai/glm-5.3-flash"

MODELS = {
    "deepseek": "deepseek/deepseek-v4-flash-0731",
    "1": "deepseek/deepseek-v4-flash-0731",
    "deepseek-flash": "deepseek/deepseek-v4-flash-0731",
    "z-ai": "z-ai/glm-5.3-flash",
    "glm": "z-ai/glm-5.3-flash",
    "sol": "openai/gpt-5.6-sol",
    "5.6": "openai/gpt-5.6-sol",
    "gpt-5.6": "openai/gpt-5.6-sol",
    "luna": "openai/gpt-5.6-luna",
    "5.6-luna": "openai/gpt-5.6-luna",
    "gpt-5.6-luna": "openai/gpt-5.6-luna",
    "auto": "auto"
}

# Full IDs verificati su OpenRouter (ammessi oltre ai valori di MODELS)
EXTRA_VALID_MODELS = {"deepseek/deepseek-v4-flash-0731", "z-ai/glm-5.3-flash", "openai/gpt-5.6-sol", "openai/gpt-5.6-luna"}

DYNAMIC_MODELS_POOL = [
    "deepseek/deepseek-v4-flash-0731",
    "z-ai/glm-5.3-flash",
    "openai/gpt-5.6-luna"
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
    "- MODULARIZZAZIONE: Se astral.py supera ~1500 righe, separa la logica dei tools in moduli dedicati (es. tools_patch.py, tools_test.py) importandoli dal file madre.\n"
    "- SELF-AWARENESS: La mappa completa del codice (file, classi, funzioni, righe, import) e' in C:\\Users\\masci\\Astral\\.selfmap.md, rigenerata automaticamente ad ogni avvio di astral.py. A inizio sessione, prima di modifiche strutturali o su dubbio: leggila PRIMA di agire. Dopo ogni patch a file di codice: rigenerala con 'py selfmap.py'.\n"
    "- VERDICT PROTOCOL: Quando l'utente dice 'verdetto' o 'verdict' (anche senza '/verdict'): (1) salva il quesito in C:\\Users\\masci\\Astral\\.verdict_quesito.tmp (Set-Content, encoding UTF8); (2) lancia detached: Start-Process python.exe -ArgumentList 'verdict_runner.py' -WorkingDirectory C:\\Users\\masci\\Astral -RedirectStandardOutput .verdict_out.txt -RedirectStandardError .verdict_err.txt -WindowStyle Hidden (ATTENZIONE: il runner si chiama verdict_runner.py SENZA punto iniziale: il vecchio '.verdict_runner.py' NON esiste piu', non riprovarlo MAI); (3) polling EFFICIENTE con POCHISSIME chiamate: UNA sola chiamata PowerShell per step, con loop di attesa INTERNO alla chiamata (max ~25s totali, sotto il timeout tool di 30s): $d=(Get-Date).AddSeconds(25); while((Get-Date) -lt $d){ if(Test-Path .verdict_out.txt){ if((Get-Content .verdict_out.txt -Raw) -match 'VERDICT_DONE'){'DONE';break} }; Start-Sleep -Seconds 3 }; se non DONE ripeti la STESSA chiamata (max 10 min complessivi, poi kill orfano e report fallimento). VIETATO Start-Sleep 30 in chiamate separate: va in timeout del tool e raddoppia le chiamate. (4) al termine mostra il verdetto ed elimina i 3 file .verdict_*. NON invocare run_verdict() inline nel processo madre (blocca il REPL, rischio crash). Giudici gia' configurati in verdict/verdict.py (versioni standard, non flash).\n"
    "- SUBAGENTS: comando `/subagent scout|review [file1,file2,...]` lancia subagent READ-ONLY detached (budget 8 job/h, depth 1, snapshot pre-job, budget via subagents/jobspec.check_budget). scout=ricerca, review=revisione codice. Output in subagents/out/. NON invocare i job inline: vanno in detached."
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

def route_model(user_input: str, context=None) -> str:
    """Seleziona dinamicamente il modello ottimale in base al tipo di richiesta.
    v2: routing pesato (benchmark 0.5 / affidabilita 0.3 / costo 0.2) con gate di
    confidenza, margine dinamico e isteresi (verdetto consiglio 2026-09-14).
    Fallback: euristica legacy is_code_query se il nuovo motore fallisce."""
    if current_model != "auto":
        return current_model
    try:
        from routing_engine import route as _weighted_route
        return _weighted_route(user_input, context=context)
    except Exception as e:
        log_error("route_model/routing_engine", e)
        if is_code_query(user_input):
            return MODEL_CODE
        return MODEL_CONVERSATION

def save_telemetry(model_name, prompt_t, resp_t, total_t):
    """Compatibilita': la persistenza e' proprieta' del routing service."""
    try:
        from routing_engine import save_telemetry as _save
        return _save(model_name, prompt_t, resp_t, total_t)
    except Exception as e:
        log_error("save_telemetry", e)
        return False

def print_telemetry(response, model_name=None):
    try:
        from routing_engine import get_telemetry_enabled as _enabled
        if not _enabled() or not response:
            return
    except Exception:
        if not telemetry_enabled or not response:
            return
    usage = getattr(response, "usage", None)
    if usage:
        prompt_t = getattr(usage, "prompt_tokens", 0) or 0
        resp_t = getattr(usage, "completion_tokens", 0) or 0
        console.print(f"[dim italic]in [gold1]{prompt_t}[/] out [dark_orange]{resp_t}[/] tot [bold grey100]{prompt_t + resp_t}[/][/dim italic]")
        if model_name:
            save_telemetry(model_name, prompt_t, resp_t, prompt_t + resp_t)


def get_telemetry_stats(model_filter=None, days=30):
    """Compatibilita': le statistiche sono aggregate dal routing service."""
    try:
        from routing_engine import telemetry_stats
        return telemetry_stats(model_filter=model_filter, days=days)
    except Exception as e:
        log_error("get_telemetry_stats", e)
        return "Errore nel recupero statistiche: %s" % e
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
    """Valida l'ID/alias del modello: alias -> ID reale; sconosciuto ->
    fallback 'auto' con warning (previene 400 per model ID invalidi)."""
    global current_model
    v = (value or "auto").strip()
    if v in MODELS:
        v = MODELS[v]  # alias risolto all'ID pieno
    valid = {m for m in MODELS.values()} | EXTRA_VALID_MODELS | {"auto"}
    # Un ID pieno esplicito (es. `vendor/modello-esatto`) viene accettato cosi' com'e',
    # anche se non e' tra quelli noti: /model <id> deve cambiare su QUEL modello.
    is_full_id = ("/" in v) and (v.count("/") == 1) and (v != "/")
    if v not in valid and not is_full_id:
        console.print(f"[bold orange_red1][!] Modello '{v}' non valido: "
                      f"fallback ad 'auto'.[/]")
        v = "auto"
    current_model = v

def get_telemetry_enabled():
    try:
        from routing_engine import get_telemetry_enabled as _enabled
        return _enabled()
    except Exception:
        return telemetry_enabled


def set_telemetry_enabled(value):
    global telemetry_enabled
    telemetry_enabled = bool(value)
    try:
        from routing_engine import set_telemetry_enabled as _set_enabled
        _set_enabled(value)
    except Exception as e:
        log_error("set_telemetry_enabled", e)
def call_with_dynamic_fallback(messages, tools_schema=None, primary_model=None):
    """
    Esegue la chiamata partendo dal modello target con reasoning sempre HIGH.
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
                # Reasoning esplicito e sempre HIGH, come configurato in precedenza.
                extra_params = {
                    "temperature": 0.3,
                    "reasoning_effort": "high",
                }
                kwargs.update(extra_params)
                if tools_schema:
                    kwargs["tools"] = tools_schema
                try:
                    response = _with_retry(
                        lambda: _rate_limited_call(**kwargs),
                        op=f"chat:{attempt_model}", log=console.print)
                except Exception as e_:
                    err_str = str(e_).lower()
                    if extra_params and any(w in err_str for w in ["reasoning_effort","temperature","top_p","not supported","unknown parameter","invalid parameter","bad request"]):
                        console.print(f"[dim][*] {attempt_model} non supporta parametri avanzati. Riprovo senza...[/dim]")
                        # Mantieni la richiesta esplicita dei parametri; alcuni
                        # provider possono rifiutarla e il retry serve solo come
                        # compatibilità per quei modelli.
                        kwargs.pop("reasoning_effort", None)
                        kwargs.pop("temperature", None)
                        kwargs.pop("top_p", None)
                        response = _rate_limited_call(**kwargs)
                    else:
                        raise

            if attempt_model != primary_model:
                console.print(f"[dim][*] Fallback dinamico attivato: risposta completata con {attempt_model}[/dim]")
            return response, attempt_model
        except ErroreAstral as e:
            last_error = f"{e.code}: {e}"
            console.print(f"[dim][!] {escape(str(attempt_model))} fallito [{escape(e.code)}]: fallback dinamico...[/dim]")
            continue
        except Exception as e:
            last_error = str(e)
            console.print(f"[dim][!] {escape(str(attempt_model))} fallito: {escape(str(e))}. Fallback dinamico in corso...[/dim]")
            continue

    log_execution("llm_call", {"model": primary_model}, error=RuntimeError(f"Tutti i modelli falliti: {last_error}"))
    raise ErroreAstral("SCONOSCIUTO", f"Tutti i modelli nel pool dinamico sono falliti. Ultimo errore: {last_error}")

def auto_repair():
    """Compatibilita CLI: esegue il repair graph reale sull'ultimo crash."""
    from repair_loop import repair_from_log

    state = repair_from_log()
    color = "green" if state.status == "success" else "orange1"
    console.print(
        f"[{color}][Repair] stato={state.status} file={state.file_path} "
        f"tentativi={state.attempts}[/{color}]"
    )
    return state

def repair_watchdog():
    """Thread demone: monitora error_log.txt e i file del protocollo verdetto.
    - Single-flight globale: solo UNA istanza astral fa da leader (lock file
      O_EXCL con heartbeat rinnovato); le altre restano in ascolto passivo.
      Se il leader muore, il lock stantio (>60s) viene rilasciato.
    - Anti-storm: cooldown 180s tra processi --repair consecutivi.
    - Watchdog stalla: se .verdict_out.txt resta senza VERDICT_DONE per oltre
      600s, kill del runner orfano + pulizia dei file .verdict_*.
    Non ferma mai il loop principale (self-healing)."""
    def _watch():
        log_file = os.path.join(BASE_DIR, "error_log.txt")
        lock_file = os.path.join(BASE_DIR, ".watchdog.lock")
        out_file = os.path.join(BASE_DIR, ".verdict_out.txt")
        COOLDOWN = 180
        STALL_TIMEOUT = 600
        LOCK_STALE = 60
        is_leader = False
        last_repair = 0.0
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
            # --- leadership single-flight (heartbeat rinnovato a ogni ciclo) ---
            try:
                if not is_leader:
                    if os.path.exists(lock_file):
                        try:
                            if time.time() - os.path.getmtime(lock_file) > LOCK_STALE:
                                os.remove(lock_file)  # lock stantio: leader morto
                        except Exception:
                            pass
                    try:
                        fd = os.open(lock_file, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
                        os.write(fd, str(os.getpid()).encode())
                        os.close(fd)
                        is_leader = True
                    except OSError:
                        pass  # un'altra istanza e' leader
                else:
                    try:
                        os.utime(lock_file, None)  # heartbeat
                    except Exception:
                        pass
            except Exception:
                pass

            # --- watchdog stalla: protocollo verdetto appeso ---
            try:
                if os.path.exists(out_file):
                    age = time.time() - os.path.getmtime(out_file)
                    content = ""
                    try:
                        with open(out_file, "r", encoding="utf-8", errors="replace") as f:
                            content = f.read()
                    except Exception:
                        pass
                    if ("VERDICT_DONE" not in content) and (age > STALL_TIMEOUT):
                        safe_print("[watchdog] Verdetto in stalla (>600s senza VERDICT_DONE): kill orfano + pulizia...")
                        subprocess.run(
                            ["powershell", "-NoProfile", "-Command",
                             "Get-CimInstance Win32_Process -Filter \"Name='python.exe'\" | "
                             "Where-Object { $_.CommandLine -match 'verdict_runner' } | "
                             "ForEach-Object { Stop-Process -Id $_.ProcessId -Force }"],
                            capture_output=True, timeout=30)
                        for _f in (".verdict_out.txt", ".verdict_err.txt", ".verdict_quesito.tmp"):
                            try:
                                os.remove(os.path.join(BASE_DIR, _f))
                            except Exception:
                                pass
            except Exception:
                pass

            # --- monitor errori + repair (solo leader, con cooldown) ---
            try:
                if is_leader and (time.time() - last_repair >= COOLDOWN) and os.path.exists(log_file):
                    with open(log_file, "r", encoding="utf-8", errors="replace") as f:
                        tail = f.read()[-4000:]
                    cand = ""
                    for line in tail.splitlines():
                        if "] ERROR" in line:
                            cand = line[:30]
                            break
                    if cand and cand != last_seen:
                        last_seen = cand
                        last_repair = time.time()
                        safe_print("[watchdog] Nuovo errore nel log: avvio self-repair in console separata...")
                        subprocess.Popen(
                            [sys.executable, os.path.join(BASE_DIR, "repair_loop.py"), "--from-log"],
                            cwd=BASE_DIR,
                            creationflags=getattr(subprocess, "CREATE_NEW_CONSOLE", 0),
                        )
            except Exception:
                pass
            time.sleep(5)
    try:
        threading.Thread(target=_watch, daemon=True).start()
    except Exception:
        pass
