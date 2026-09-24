# -*- coding: utf-8 -*-
# astral.py - Entry point: REPL, comandi slash, ciclo conversazione
import json
import os
import subprocess
import sys
import time

from core_io import _early_log, console, install_exception_hooks, launch_self_repair, log_error, set_session_color

try:
    import stt_integration
except Exception as e:
    stt_integration = None
    _early_log("import stt_integration", e)

from rich.panel import Panel
from rich.markdown import Markdown
from rich.markup import escape
from rich.table import Table

try:
    from ui import (banner as ui_banner, prompt_label as ui_prompt_label,
                    confirm as ui_confirm, table as ui_table, error as ui_error,
                    voice_confirm as ui_voice_confirm)
except Exception as _ui_e:
    _early_log("import ui", _ui_e)
    from rich.panel import Panel as _ui_Panel
    from rich.table import Table as _ui_Table
    from rich.markup import escape as _ui_escape

    def ui_banner(title=None, subtitle=None, border_style=None, body=None):
        if body is None:
            body = f"[bold]{_ui_escape(title or '')}[/]"
            if subtitle:
                body += f"\n[dim]{_ui_escape(subtitle)}[/]"
        return _ui_Panel(body, title=title, subtitle=subtitle,
                         border_style=border_style or "bright_cyan", padding=(1, 2))

    def ui_prompt_label(label, color=None):
        return f"[bold {color or 'bright_cyan'}]{_ui_escape(label)}[/] [bold spring_green1]>[/]"

    def ui_confirm(text):
        return console.input(f"[bold orange_red1]{_ui_escape(text)}[/] (s/N): ")

    def ui_voice_confirm(text, timeout=1.5):
        try:
            answer = console.input(
                "[bold orange_red1]Invio la trascrizione? "
                "(INVIO=s / n=modifica): [/]")
            return text if answer.strip().lower() != "n" else console.input(
                "Testo corretto: ").strip()
        except (EOFError, KeyboardInterrupt):
            return ""

    def ui_table(headers, rows, justify=None, **kw):
        _t = _ui_Table(**kw)
        for _i, _h in enumerate(headers):
            _t.add_column(_h, justify=(justify[_i] if justify and _i < len(justify) else "left"))
        for _r in rows:
            _t.add_row(*[str(_c) for _c in _r])
        return _t

    def ui_error(text):
        return f"[bold orange_red1]{_ui_escape(text)}[/]"

from loop_detector import LoopDetector
try:
    import voice_feedback
except Exception:
    voice_feedback = None  # indicatore visivo opzionale: mai bloccare il REPL


def _lights_notify(kind):
    """Feedback visivo best-effort: casella colorata in console, mai eccezioni."""
    if voice_feedback is not None:
        try:
            voice_feedback.notify(kind)
        except Exception:
            pass
from memory_store import checkpoint_pull, checkpoint_save, init_db
from memory_meta import bootstrap_meta, record_usage
from price_map import all_prices, cost_usd
from history_store import (
    CONFIG_FILE,
    clear_persistent_history,
    compact_history_with_summary,
    get_history,
    maybe_offload_tool_result,
    save_persistent_history,
)
from astral_trim import (  # v2 breathing: finestra sinusoidale + ledger fattuale
    last_meta,
    safe_trim_context,
)
from repair_loop import auto_repair_graph  # graph-based self-repair (fase 2)
from llm_core import (
    MODEL_CODE,
    MODEL_CONVERSATION,
    MODELS,
    auto_repair,
    call_with_dynamic_fallback,
    get_current_model,
    get_telemetry_enabled,
    get_telemetry_stats,
    print_telemetry,
    repair_watchdog,
    route_model,
    set_current_model,
    set_telemetry_enabled,
)
import tools_gateway as gateway
from tools_gateway import call_tool as gateway_call_tool
try:
    from prompt_toolkit import PromptSession
    PROMPT_TOOLKIT_OK = True
except Exception as e:
    PromptSession = None
    PROMPT_TOOLKIT_OK = False
    _early_log("import prompt_toolkit", e)




install_exception_hooks()



voice_prompt_session = None  # lazy init: vedi _get_voice_prompt_session()

def _get_voice_prompt_session():
    """Crea il PromptSession al primo uso (lazy init: evita crash import senza TTY)."""
    global voice_prompt_session, PROMPT_TOOLKIT_OK
    if voice_prompt_session is None and PROMPT_TOOLKIT_OK:
        try:
            voice_prompt_session = PromptSession()
        except Exception as e:
            PROMPT_TOOLKIT_OK = False
            _early_log("init PromptSession", e)
    return voice_prompt_session

def _legacy_voice_confirm_async(text, timeout):
    """Prompt conferma con timer (prompt_toolkit async): INVIO=invia,
    ESC=annulla, scadenza timer=auto-invio se il buffer non e' stato modificato.
    Ritorna: testo (eventualmente modificato), "" (annulla) o __AUTO__.
    """
    import asyncio
    from prompt_toolkit.key_binding import KeyBindings

    async def _run():
        kb = KeyBindings()

        @kb.add("escape")
        def _esc(event):
            event.app.exit(exception=KeyboardInterrupt)

        session = _get_voice_prompt_session()
        prompt_t = asyncio.ensure_future(session.prompt_async(
            "Astral (Voce) > ", default=text, key_bindings=kb))
        if not timeout:
            return await prompt_t
        timer_t = asyncio.ensure_future(asyncio.sleep(timeout))
        done, _pend = await asyncio.wait({prompt_t, timer_t},
                                         return_when=asyncio.FIRST_COMPLETED)
        if timer_t in done and prompt_t not in done:
            if (session.default_buffer.text or "").strip() == text.strip():
                prompt_t.cancel()
                try:
                    await prompt_t
                except asyncio.CancelledError:  # Py3.13: BaseException, non Exception
                    pass
                return "__AUTO__"  # auto-invio della trascrizione
            timer_t.cancel()  # l'utente sta modificando: timer disattivato
        else:
            timer_t.cancel()
        try:
            val = await prompt_t
            return (val or "").strip()
        except (KeyboardInterrupt, EOFError, asyncio.CancelledError):
            return ""  # ESC / Ctrl-C / task cancellata: annulla
    try:
        return asyncio.run(_run())
    except (EOFError, KeyboardInterrupt):
        return ""


def _parse_verdict_request(text):
    """Riconosce sia il comando breve sia richieste naturali esplicite.

    Non basta la semplice presenza della parola ``verdetto``: serve una forma
    che chieda davvero di eseguirlo, così una discussione sul protocollo resta
    una normale conversazione.
    """
    import re
    raw = (text or "").strip()
    low = raw.lower()
    if low.startswith("/verdict"):
        rest = raw[len("/verdict"):].strip()
    else:
        parts = raw.split(None, 1)
        if parts and parts[0].lower() in ("verdict", "verdetto"):
            rest = parts[1].strip() if len(parts) > 1 else ""
        else:
            match = re.match(
                r"^(?:fammi|fai|esegui|dammi|voglio|richiedo|puoi\\s+farmi|puoi\\s+fare|"
                r"esamina|valuta)\\s+(?:un\\s+)?verdetto\\b\\s*(.*)$",
                raw, re.IGNORECASE,
            )
            if not match:
                return None
            rest = match.group(1).strip()
    tokens = rest.split(None, 1)
    profilo = "standard"
    if tokens and tokens[0].lower() in ("lite", "--lite"):
        profilo = "lite"
        rest = tokens[1].strip() if len(tokens) > 1 else ""
    # Forme naturali comuni: "verdetto su ...", "verdetto: ...".
    rest = re.sub(r"^(?:su|di|per)\\s+", "", rest, flags=re.IGNORECASE)
    rest = rest.lstrip(":- ").strip()
    return profilo, " ".join(rest.split()).strip()


def _build_verdict_input(question, messages, max_chars=18000):
    """Prepara un dossier esplicito per il consiglio, senza inventare contesto.

    Il comando verdetto interrompe il normale flusso della chat: per questo il
    runner detached non riceverebbe automaticamente la conversazione corrente.
    Qui raccogliamo solo i messaggi testuali recenti, li etichettiamo e lasciamo
    sempre la domanda originale chiaramente separata dal contesto.
    """
    question = " ".join((question or "").split()).strip()
    blocks = []
    used = 0
    for message in reversed(messages or []):
        if not isinstance(message, dict):
            continue
        role = message.get("role")
        if role not in ("user", "assistant"):
            continue
        content = message.get("content") or ""
        if not isinstance(content, str) or not content.strip():
            continue
        content = content.strip()
        # I pareri precedenti/risposte molto lunghe non devono soffocare il quesito.
        content = content[-2800:]
        block = f"[{role.upper()}]\n{content}"
        if used + len(block) > max_chars:
            break
        blocks.append(block)
        used += len(block)
    blocks.reverse()
    context = "\n\n".join(blocks) or "(nessun contesto conversazionale disponibile)"
    return (
        "DOMANDA ORIGINALE DELL'UTENTE:\n"
        + question
        + "\n\nCONTESTO CONVERSAZIONALE DISPONIBILE (puo' essere incompleto):\n"
        + context
        + "\n\nISTRUZIONI PER IL CONSIGLIO:\n"
        "Rispondi alla domanda originale usando il contesto solo quando e' pertinente. "
        "Non inventare dati mancanti e segnala le assunzioni. Se la richiesta implica "
        "una scelta, includi pro e contro, rischi e passaggi operativi; se non implica "
        "una scelta, non forzare pro e contro."
    )


def read_voice_input(text, auto_send_s=1.5):
    """Finestra di conferma trascrizione (1.5s): INVIO=invia subito,
    ESC=annulla, altro tasto=modifica (disattiva il timer). Auto-invio
    allo scadere. Fallback (no prompt_toolkit): conferma senza timer."""
    _lights_notify("ok")  # casella verde: trascrizione pronta
    console.print(f"[bold gold1]Trascrizione:[/] {escape(text)}")
    console.print(f"[dim]INVIO=invia | ESC=annulla | auto-invio tra "
                  f"{auto_send_s}s (un tasto qualsiasi disattiva il timer)[/dim]")
    return ui_voice_confirm(text, auto_send_s)


def run_meta_maintenance():
    """P1 (verdetto 21:35): manutenzione meta programmata. NO startup/shutdown.
    Eseguita al primo prompt di ogni sessione, solo se >24h dall'ultima run.
    Ordine prescritto: audit_meta (backfill/anti-gap) -> prune (snapshot versionato -> purge).
    """
    try:
        wm_file = os.path.join(BASE_DIR, "logs", ".meta_maint_last")
        last = 0
        if os.path.exists(wm_file):
            with open(wm_file, "r", encoding="utf-8") as f:
                last = int(f.read().strip() or 0)
        if time.time() - last < 86400:  # min 24h
            return
        from memory_meta import audit_meta, prune
        rep = {}
        rep["audit"] = audit_meta() or {}
        rep["prune"] = prune() or {}
        os.makedirs(os.path.dirname(wm_file), exist_ok=True)
        with open(wm_file, "w", encoding="utf-8") as f:
            f.write(str(int(time.time())))
        if rep["prune"].get("purged") or rep["prune"].get("snapshot"):
            console.print(f"[dim]Manutenzione memoria: {rep['prune'].get('purged', 0)} righe purge, snapshot: {rep['prune'].get('snapshot')}[/dim]")
    except Exception:
        pass  # la manutenzione non deve MAI bloccare il loop utente


MAX_SESSIONS = 5  # cap sessioni simultanee
# Palette stabile per distinguere le sessioni concorrenti nel terminale.
_SESSION_COLORS = (
    "dodger_blue1",
    "medium_purple1",
    "spring_green1",
    "dark_orange",
    "bright_cyan",
)

# Il driver CUA non deve partire al logon: viene gestito insieme al ciclo di vita
# delle sessioni Astral. La task "cua-driver-serve" resta quindi disabilitata.
_CUA_DRIVER_EXE = os.path.join(
    os.environ.get("LOCALAPPDATA", os.path.expanduser("~\\AppData\\Local")),
    "Programs", "Cua", "cua-driver", "bin", "cua-driver.exe",
)


def _cua_driver_running():
    """Ritorna True se il processo CUA driver e' gia' attivo."""
    try:
        flags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
        result = subprocess.run(
            ["tasklist", "/FI", "IMAGENAME eq cua-driver.exe", "/FO", "CSV", "/NH"],
            capture_output=True, text=True, timeout=3,
            creationflags=flags,
        )
        return "cua-driver.exe" in (result.stdout or "").lower()
    except Exception:
        return False


def _start_cua_driver():
    """Avvia il driver solo quando esiste almeno una sessione Astral."""
    if _cua_driver_running() or not os.path.isfile(_CUA_DRIVER_EXE):
        return
    try:
        flags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
        subprocess.Popen(
            [_CUA_DRIVER_EXE, "serve"],
            cwd=os.path.expanduser("~"),
            stdin=subprocess.DEVNULL,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            creationflags=flags,
        )
    except Exception as e:
        _early_log("start cua-driver", e)


def _stop_cua_driver_if_idle():
    """Chiude il driver quando l'ultima sessione Astral e' terminata."""
    if not _cua_driver_running():
        return
    try:
        flags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
        subprocess.run(
            ["taskkill", "/IM", "cua-driver.exe", "/T", "/F"],
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
            timeout=5, creationflags=flags,
        )
    except Exception as e:
        _early_log("stop cua-driver", e)


def _pid_alive(pid):
    """Check safe su Windows: OpenProcess (QUERY_LIMITED) invece di os.kill
    (che con sig!=CTRL_* chiama TerminateProcess e ucciderebbe il target!)."""
    try:
        import ctypes
        h = ctypes.windll.kernel32.OpenProcess(0x1000, False, int(pid))  # PROCESS_QUERY_LIMITED_INFORMATION
        if not h:
            return False
        ctypes.windll.kernel32.CloseHandle(h)
        return True
    except Exception:
        return False


def _claim_session_slot():
    """Assegna uno slot stabile (e quindi un colore) a questo processo.

    Il marker conserva ``slot pid`` invece della sola lista dei PID: cosi', se
    la sessione blu termina mentre quella magenta resta aperta, la nuova
    sessione rioccupa il blu e non ricolora erroneamente quella magenta.
    """
    import atexit

    base_dir = os.path.dirname(os.path.abspath(__file__))
    marker = os.path.join(base_dir, ".astral_sessions.lock")
    mutex = marker + ".mutex"
    my = os.getpid()

    try:
        import msvcrt
    except ImportError:  # pragma: no cover - Astral gira normalmente su Windows
        msvcrt = None

    def _locked_update(callback):
        """Serializza lettura/scrittura del marker tra processi Windows."""
        if msvcrt is None:
            return callback()
        with open(mutex, "a+b") as lock:
            lock.seek(0, os.SEEK_END)
            if lock.tell() == 0:
                lock.write(b"0")
                lock.flush()
            lock.seek(0)
            msvcrt.locking(lock.fileno(), msvcrt.LK_LOCK, 1)
            try:
                return callback()
            finally:
                lock.seek(0)
                msvcrt.locking(lock.fileno(), msvcrt.LK_UNLCK, 1)

    def _read_records():
        """Legge anche il vecchio formato, migrandolo al primo avvio."""
        records = []
        if not os.path.exists(marker):
            return records
        with open(marker, "r", encoding="utf-8") as f:
            for line in f:
                parts = line.split()
                if len(parts) >= 2 and parts[0].isdigit() and parts[1].isdigit():
                    records.append((int(parts[0]), int(parts[1])))
                elif len(parts) == 1 and parts[0].isdigit():
                    # Compatibilita' con il marker precedente: slot ancora da assegnare.
                    records.append((None, int(parts[0])))
        return records

    def _write_records(records):
        with open(marker, "w", encoding="utf-8") as f:
            f.write("\n".join(f"{slot} {pid}" for slot, pid in records))

    def _active_records():
        records = _read_records()
        active = []
        used = set()
        # Prima preserva gli slot gia' espliciti.
        for slot, pid in records:
            if (slot is not None and 0 <= slot < MAX_SESSIONS
                    and slot not in used and _pid_alive(pid)):
                active.append((slot, pid))
                used.add(slot)
        # Poi assegna i vecchi record senza slot ai primi slot liberi.
        for slot, pid in records:
            if slot is None and _pid_alive(pid):
                free = next((i for i in range(MAX_SESSIONS) if i not in used), None)
                if free is None:
                    break
                active.append((free, pid))
                used.add(free)
        return sorted(active)

    try:
        def _claim():
            active = _active_records()  # ripulisce anche i PID gia' terminati
            used = {slot for slot, _pid in active}
            free = next((i for i in range(MAX_SESSIONS) if i not in used), None)
            if free is None:
                _write_records(active)
                return -1
            active.append((free, my))
            _write_records(sorted(active))
            return free

        slot = _locked_update(_claim)
        if slot < 0:
            return -1

        def _cleanup():
            try:
                def _release():
                    active = _active_records()
                    _write_records([
                        record for record in active
                        if record != (slot, my)
                    ])
                _locked_update(_release)
            except Exception:
                pass

        def _cleanup_with_driver():
            _cleanup()
            # Il marker e' gia' stato aggiornato: se non resta alcuna sessione,
            # il driver puo' essere chiuso senza interferire con altre sessioni.
            try:
                if not _active_records():
                    _stop_cua_driver_if_idle()
            except Exception:
                pass

        atexit.register(_cleanup_with_driver)
        _start_cua_driver()
        return slot
    except Exception as e:
        # [FIX A-07] Fail-closed: in caso di errore NON ripieghiamo sullo slot 0
        # (piu' processi si comporterebbero come la stessa sessione).
        # Ritorniamo -1: il chiamante blocca l'avvio e l'errore viene loggato.
        try:
            from core_io import log_error
            log_error("claim_session_slot", e)
        except Exception:
            pass
        return -1


def main():
    # Watchdog self-repair: gli errori nuovi vengono gestiti senza fermare il loop
    try:
        repair_watchdog()
    except Exception:
        pass
    
    if os.path.exists(CONFIG_FILE):
        try:
            with open(CONFIG_FILE, "r", encoding="utf-8") as f:
                cfg = json.load(f)
                if "models" in cfg:
                    MODELS.update(cfg["models"])
                if "current_model" in cfg and cfg["current_model"]:
                    val = cfg["current_model"]
                    set_current_model(MODELS.get(val, val))
        except Exception:
            pass

    console.print(ui_banner(
        body=(
            f"[bold white]Il tuo assistente operativo per Windows 11[/]\n"
            f"[dim]Conversazione, sviluppo e automazione in un unico spazio.[/]\n\n"
            f"[bold]ROUTING[/]     [bold green]AUTO[/]  [dim]pool dinamico[/]\n"
            f"[bold]CHAT[/]        [dim]{MODEL_CONVERSATION}[/]\n"
            f"[bold]SVILUPPO[/]    [dim]{MODEL_CODE}[/]\n"
            f"[bold]ATTIVO[/]      [bold cyan]{get_current_model()}[/]\n\n"
            f"[dim]Scrivi una richiesta oppure usa [/][bold cyan]/help[/][dim] per i comandi.[/]"
        ),
        title="[bold bright_cyan] ASTRAL [/bold bright_cyan] [dim]· workspace[/dim]",
        subtitle="[dim]online · pronto a collaborare[/dim]",
        border_style="bright_cyan",
    ))

    messages = get_history()
    loop_det = LoopDetector()
    session_first_msg = True
    _session_slot = _claim_session_slot()
    if _session_slot < 0:
        console.print(ui_error(f"Cap sessioni raggiunto ({MAX_SESSIONS}). Chiudi una sessione prima di aprirne un'altra."))
        return

    _mt_run = False
    while True:
        try:
            if not _mt_run:
                _mt_run = True
                run_meta_maintenance()
                # Budget tier: check credito residuo OpenRouter UNA volta per
                # processo; modula il peso del costo nel routing dinamico.
                try:
                    from budget_guard import check_credits
                    from routing_engine import set_budget_tier
                    set_budget_tier(*check_credits())
                except Exception:
                    pass  # il routing resta sui pesi base in caso di errore
            cm = get_current_model()
            prompt_label = "Astral (Auto)" if cm == "auto" else cm
            _prompt_color = _SESSION_COLORS[_session_slot % len(_SESSION_COLORS)]
            set_session_color(_prompt_color)  # aggancia anche i messaggi "Elaborazione" allo stesso colore
            user_input = console.input(f"\n{ui_prompt_label(prompt_label, _prompt_color)} ").strip()
            if not user_input:
                continue

            # Feedback immediato: non lasciare la console apparentemente bloccata
            # mentre selfmap/routing preparano la richiesta.
            console.print(f"[dim][bold {_prompt_color}]Elaborazione in corso...[/][/dim]")

            # La selfmap viene mantenuta dal watcher in background all'avvio;
            # non deve mai inserirsi tra INVIO e la chiamata OpenRouter.

            if user_input.lower() == '/voice':
                if stt_integration is None or not stt_integration.is_available():
                    console.print(ui_error("STT non disponibile. Dettagli in error_log.txt."))
                    continue
                console.print("[dim]Ascolto... parla ora (max 60s, si ferma dopo 2.5s di silenzio)[/dim]")
                _lights_notify("listen")  # casella ciano: microfono attivo
                stt_text = stt_integration.transcribe(max_seconds=60)
                if not stt_text:
                    _lights_notify("error")  # casella rossa: nessun discorso rilevato
                    log_error("voice/transcribe", "Trascrizione vuota: nessun parlato riconosciuto")
                    console.print("[dim]Nessun discorso rilevato. Riprova.[/dim]")
                    continue
                # Finestra di conferma 1.5s: INVIO=invia, ESC=annulla, altrimenti modifica.
                # Lo scadere del timer invia automaticamente la trascrizione.
                user_input = read_voice_input(stt_text)
                if not user_input:
                    continue
            if user_input.lower() in ['exit', 'quit']:
                checkpoint_save(messages)
                save_persistent_history(messages)
                console.print("[dim][*] Sessione salvata. Arrivederci![/dim]")
                break
            
            lower_input = user_input.lower()
            if lower_input == '/clear':
                checkpoint_save(messages)
                clear_persistent_history()
                messages = []
                session_first_msg = True
                console.print("[dim][*] Sessione ripulita.[/dim]")
                continue

            if lower_input in ('/help', '/?', 'help'):
                console.print(Panel(
                    f"[bold]SESSIONE[/]\n"
                    f"  [bold cyan]/clear[/]       pulisci la cronologia della sessione\n"
                    f"  [bold cyan]/voice[/]       input vocale (STT)\n"
                    f"  [bold cyan]exit[/] | [bold cyan]quit[/]   salva ed esci\n\n"
                    f"[bold]MEMORIA[/]\n"
                    f"  [bold cyan]/breath[/]      stato respirazione contesto\n"
                    f"  [bold cyan]/meta-last[/]   ultimi eventi meta [n] [tag]\n"
                    f"  [bold cyan]/meta-stats[/]  statistiche meta [giorni]\n"
                    f"  [bold cyan]/meta-pin[/]    pin/unpin ref  |  [bold cyan]/meta-tag[/] <ref> <tags>\n"
                    f"  [bold cyan]/prune[/]       pulizia memoria  |  [bold cyan]/audit[/] raw/meta\n\n"
                    f"[bold]TASK[/]\n"
                    f"  [bold cyan]/task[/]        elenco task  |  [bold cyan]/task-sync[/] riconcilia git\n"
                    f"  [bold cyan]/task-new[/] <id> <titolo>  |  [bold cyan]/task-test[/] <id>  |  [bold cyan]/task-done[/] <id>\n\n"
                    f"[bold]SISTEMA[/]\n"
                    f"  [bold cyan]/self[/]        selfmap: [dim]fn|cls|file[/]\n"
                    f"  [bold cyan]/repair[/]      auto-riparazione  |  [bold cyan]/repair-graph[/]\n"
                    f"  [bold cyan]/telemetry[/]   toggle telemetria  |  [bold cyan]/stats[/]\n\n"
                    f"[bold]STRUMENTI[/]\n"
                    f"  [bold cyan]/scrape[/] <url>  [dim]--refresh --json --no-cache --no-robots[/]\n"
                    f"  [bold cyan]/execlog[/] [n]   log esecuzioni tool\n"
                    f"  [bold cyan]/usage[/] [g]     costi [dim]--csv | prices[/]  |  [bold cyan]/model[/] <nome>\n\n"
                    f"[bold]AGENTI[/]\n"
                    f"  [bold cyan]/verdict[/] [lite] <quesito>   giudici anonimi\n"
                    f"  [bold cyan]/subagent[/] scout|review [file]   subagent detached\n\n"
                    f"[dim]Scrivi una richiesta libera oppure usa i comandi sopra.[/]",
                    title="[bold bright_cyan] ASTRAL [/bold bright_cyan] [dim]· help[/dim]",
                    border_style="bright_cyan", padding=(1, 2)
                ))
                continue

            if lower_input == '/breath':
                import astral_trim as _at
                _lm = _at.last_meta
                console.print(Panel(
                    f"[bold]PERIODO[/]     {_at.BREATH_PERIOD} turni   [bold]FINESTRA[/]   {_at.WAVE_MIN_TURNS}-"
                    f"{_at.WAVE_MAX_TURNS} turni\n"
                    f"[bold]BUDGET[/]      {_at.BUDGET_MIN_CHARS}-{_at.BUDGET_MAX_CHARS} char   "
                    f"[bold]HARD CAP[/]    {_at.HARD_CAP_CHARS}\n"
                    f"[bold]TURNO[/]       {_lm.get('turn')}   [bold]CICLO[/] #{_lm.get('cycle')}   "
                    f"[bold]FATTI[/] {_lm.get('facts')}\n"
                    f"[bold]EVENTO[/]     {_lm.get('event')}   [bold]CHARS[/] {_lm.get('chars')}\n"
                    f"[bold]LEGACY[/]     {'[bold orange_red1]SI[/]' if os.environ.get('ASTRAL_TRIM_LEGACY') == '1' else '[dim]no[/]'}",
                    title="[bold cyan]Respirazione contesto (v2)[/]",
                    border_style="cyan", padding=(1, 2)
                ))
                continue

            if lower_input.startswith('/self'):
                # Self-awareness rapida: interroga .selfmap.json (indice strutturato)
                try:
                    import selfmap as _sm
                    if _sm.is_stale():
                        _sm.generate()
                    _idx = json.load(open(os.path.join(os.path.dirname(os.path.abspath(__file__)), '.selfmap.json'), encoding='utf-8'))
                    _parts = lower_input.split()
                    if len(_parts) == 1:
                        _sm_t = Table(title=f"Selfmap · {_idx['file_count']} file · {_idx['total_lines']} righe")
                        for _col in ("File", "Righe", "Simboli"):
                            _sm_t.add_column(_col, justify="left" if _col == "File" else "right")
                        _big = sorted(_idx['files'].items(), key=lambda kv: -kv[1]['lines'])[:8]
                        for _rel, _i in _big:
                            _sm_t.add_row(_rel, str(_i['lines']),
                                          str(len(_i['classes']) + len(_i['functions'])))
                        console.print(_sm_t)
                        console.print(f"[dim]gen: {_idx['generated']}[/dim]")
                    elif _parts[1] in ('fn', 'cls'):
                        _kind = 'functions' if _parts[1] == 'fn' else 'classes'
                        _q = _parts[2] if len(_parts) > 2 else ''
                        _hits = 0
                        for _rel, _i in sorted(_idx['files'].items()):
                            for _s in _i[_kind]:
                                if _q in _s['name']:
                                    console.print(f"  {_rel}:{_s['line']} {_s['name']} — {_s['summary'] or 'non documentata'}")
                                    _hits += 1
                        if not _hits:
                            console.print(f"[dim]Nessun {_kind[:-1]} '{_q}' trovato.[/dim]")
                    else:
                        _rel = _parts[1]
                        if not _rel.endswith('.py'):
                            _rel += '.py'
                        _i = _idx['files'].get(_rel)
                        if not _i:
                            console.print(f"[bold orange_red1][!] File '{_rel}' non in selfmap.")
                        else:
                            console.print(f"[bold cyan]{_rel}[/] ({_i['lines']} righe) — {_i['summary'] or 'non documentata'}")
                            console.print(f"  hash: {_i['sha256']} | import: {', '.join(_i['imports']) or '-'}")
                            console.print(f"  usato da: {', '.join(_idx['imported_by'].get(_rel, [])) or '-'}")
                            for _c in _i['classes']:
                                console.print(f"  class {_c['name']} righe {_c['line']}-{_c['end_line']}")
                            for _f in _i['functions']:
                                console.print(f"  {_f['signature']} righe {_f['line']}-{_f['end_line']}")
                except Exception as _e_sm:
                    console.print(ui_error(f"[!] /self error: {_e_sm}"))
                continue

            if lower_input == '/repair':
                console.print("[dim][*] Avvio auto-riparazione (classica)...[/dim]")
                auto_repair()
                continue

            if lower_input == '/repair-graph':
                console.print("[dim][*] Avvio graph-based self-repair...[/dim]")
                # Auto-repair su tutti i file .py del progetto
                import glob
                py_files = glob.glob(os.path.join(os.path.dirname(os.path.abspath(__file__)), '*.py'))
                py_files = [f for f in py_files if os.path.basename(f) not in ('astral.py',)]
                for fpath in py_files:
                    console.print(f"[dim]  -> {os.path.basename(fpath)}[/dim]")
                    state = auto_repair_graph(fpath, mode='compile', max_attempts=2)
                    console.print(f"[dim]     {state.summary()}[/dim]")
                console.print("[green][*] Graph repair completato.[/green]")
                continue

            if lower_input == '/telemetry':
                new_tel = not get_telemetry_enabled()
                set_telemetry_enabled(new_tel)
                state = "[bold spring_green1]ATTIVA[/]" if new_tel else "[bold orange_red1]DISATTIVA[/]"
                console.print(f"[dim][*] Telemetria {state}[/dim]")
                continue

            if lower_input == "/stats":
                stats = get_telemetry_stats()
                console.print(stats)
                continue

            # --- /scrape: scraper interno eccellente (tools_scrape) ---
            if lower_input.startswith('/scrape '):
                _sc = lower_input.split()
                if len(_sc) < 2:
                    console.print("[yellow]Uso: /scrape <url> [--refresh] [--json] [--no-cache] [--no-robots][/yellow]")
                else:
                    _sc_url = _sc[1]
                    _sc_refresh = "--refresh" in _sc
                    _sc_json = "--json" in _sc
                    try:
                        from tools_scrape import scrape, _fmt
                        _sc_res = scrape(_sc_url, refresh=_sc_refresh,
                                         use_cache=not ("--no-cache" in _sc),
                                         respect_robots=not ("--no-robots" in _sc))
                        if _sc_json:
                            import json as _json
                            console.print(_json.dumps(_sc_res.to_dict(), ensure_ascii=False))
                        else:
                            console.print(_fmt(_sc_res))
                    except Exception as _sc_e:
                        console.print(ui_error(f"[!] /scrape errore: {type(_sc_e).__name__}: {_sc_e}"))
                continue

            if lower_input.startswith('/execlog'):
                try:
                    from exec_logger import read_executions
                    _el_n = 15
                    _el_parts = lower_input.split()
                    if len(_el_parts) > 1:
                        _el_n = max(1, min(200, int(_el_parts[1])))
                    _rows = read_executions(limit=_el_n)
                    if not _rows:
                        console.print("[dim]Nessuna esecuzione loggata.[/dim]")
                    else:
                        _el_t = Table(title=f"Ultime {len(_rows)} esecuzioni")
                        for _col in ("Timestamp", "Stato", "Tool", "Durata", "Errore"):
                            _el_t.add_column(_col, justify="left" if _col in ("Timestamp", "Tool", "Errore") else "center")
                        for _r in _rows:
                            _st = _r.get('status', '?')
                            _col = 'red' if _st == 'error' else ('green' if _st == 'ok' else 'yellow')
                            _dur = _r.get('duration_ms')
                            _dur_s = f"{_dur}ms" if _dur is not None else "-"
                            _err = str(_r.get('error'))[:60] if _r.get('error') else ""
                            _el_t.add_row(_r.get('ts', '?'), f"[{_col}]{_st}[/]", _r.get('tool', '?'), _dur_s, _err)
                        console.print(_el_t)
                except Exception as _e_el:
                    console.print(ui_error(f"[!] /execlog error: {_e_el}"))
                continue

            if lower_input.startswith('/meta-stats'):
                try:
                    from memory_meta import stats as meta_stats
                    _ms_days = 7
                    _ms_p = lower_input.split()
                    if len(_ms_p) > 1:
                        _ms_days = max(1, min(365, int(_ms_p[1])))
                    _ms = meta_stats(days=_ms_days)
                    console.print(f"[bold cyan]Meta stats {_ms['days']}g[/] total={_ms['total']} avg_ms={_ms['avg_duration_ms']}")
                    console.print(f"  by_tool : {_ms['by_tool']}")
                    console.print(f"  by_type : {_ms['by_type']}")
                    console.print(f"  by_status: {_ms['by_status']}")
                except Exception as _e_ms:
                    console.print(ui_error(f"[!] /meta-stats error: {_e_ms}"))
                continue

            if lower_input.startswith('/meta-pin') or lower_input.startswith('/meta-unpin'):
                try:
                    from memory_meta import pin as meta_pin, unpin as meta_unpin
                    _mp = lower_input.split()
                    if len(_mp) < 2:
                        console.print("[dim]Uso: /meta-pin <src_ref|ln:N>  |  /meta-unpin <src_ref|ln:N>[/dim]")
                    elif _mp[0] == '/meta-unpin':
                        console.print("[dim]unpinned[/dim]" if meta_unpin(_mp[1]) else "[yellow]ref non trovato o gia' libero[/yellow]")
                    else:
                        console.print("[green]pinned[/green]" if meta_pin(_mp[1]) else "[yellow]ref non trovato (vedi /meta-last)[/yellow]")
                except Exception as _e_mp:
                    console.print(ui_error(f"[!] /meta-pin error: {_e_mp}"))
                continue

            if lower_input.startswith('/meta-tag'):
                try:
                    from memory_meta import tag as meta_tag
                    _mt = user_input.split(maxsplit=2)
                    if len(_mt) < 3:
                        console.print("[dim]Uso: /meta-tag <src_ref|ln:N> <tag1,tag2,...>[/dim]")
                    else:
                        _tags = [t.strip().lower() for t in _mt[2].split(',') if t.strip()]
                        console.print(f"[green]taggato: {meta_tag(_mt[1], _tags)}[/green]")
                except Exception as _e_mt:
                    console.print(ui_error(f"[!] /meta-tag error: {_e_mt}"))
                continue

            if lower_input.startswith('/meta-last'):
                try:
                    from memory_meta import list_events
                    _ml_n, _ml_tag = 10, None
                    _ml_p = lower_input.split()
                    if len(_ml_p) > 1:
                        if _ml_p[1].isdigit():
                            _ml_n = max(1, min(100, int(_ml_p[1])))
                            if len(_ml_p) > 2:
                                _ml_tag = _ml_p[2]
                        else:
                            _ml_tag = _ml_p[1]
                    _ml_rows = list_events(tag_filter=_ml_tag, limit=_ml_n)
                    if not _ml_rows:
                        console.print("[dim]Nessun evento meta.[/dim]")
                    else:
                        _ml_t = Table(title=f"Eventi meta (ultimi {len(_ml_rows)})")
                        for _col in ("Ref", "Quando", "Tool", "Tipo", "Stato", "Pin"):
                            _ml_t.add_column(_col, justify="left" if _col in ("Ref", "Tool", "Tipo") else "center")
                        for _r in _ml_rows:
                            _ml_pin = "[yellow]pin[/]" if _r["pinned"] else ""
                            _ml_when = time.strftime('%d/%m %H:%M', time.localtime(_r["ts_utc"] or 0))
                            _ml_t.add_row(_r['src_ref'], _ml_when, escape(str(_r['tool'])),
                                          _r['event_type'], _r['status'], _ml_pin)
                        console.print(_ml_t)
                except Exception as _e_ml:
                    console.print(ui_error(f"[!] /meta-last error: {_e_ml}"))
                continue

            if lower_input == '/prune':
                try:
                    from memory_meta import prune
                    _pr = prune()
                    console.print(f"[bold cyan]Prune eseguito[/]: soft_deleted={_pr['soft_deleted']} purged={_pr['purged']} pinned_immuni={_pr['pinned']}")
                    if _pr.get('snapshot'):
                        console.print(f"[dim]Snapshot: {_pr['snapshot']}[/dim]")
                except Exception as _e_pr:
                    console.print(ui_error(f"[!] /prune error: {_e_pr}"))
                continue

            if lower_input == '/audit':
                try:
                    from memory_meta import audit_meta
                    _au = audit_meta()
                    if _au.get('error'):
                        console.print(ui_error(f"[!] /audit error: {_au['error']}"))
                    else:
                        console.print(f"[bold cyan]Audit raw/meta[/]: raw={_au['raw_lines']} meta={_au['meta_rows']} missing={_au['missing_meta']} reimported={_au['reimported']} orphan={_au['orphan_meta']}")
                        console.print(f"[dim]watermark: {_au['watermark']}[/dim]")
                except Exception as _e_au:
                    console.print(ui_error(f"[!] /audit error: {_e_au}"))
                continue

            if lower_input.startswith('/task'):
                _handle_task_command(user_input)
                continue

            _v_request = _parse_verdict_request(user_input)
            if _v_request is not None:
                _v_profilo, quesito = _v_request
                if not quesito:
                    console.print("[dim]Uso: /verdict [lite] <quesito> (oppure 'verdict [lite] <quesito>') — consiglio di giudici anonimi con verdetto ('lite' = modelli veloci/economici)[/dim]")
                else:
                    # Servizio unico: avvio detached + attesa bounded (verdict_launcher).
                    try:
                        import verdict_launcher as _vl
                        _dossier = _build_verdict_input(quesito, messages)
                        _started = _vl.start_verdict(quesito, _v_profilo, dossier=_dossier)
                        if _started.get('error'):
                            console.print(ui_error(f"[!] Verdetto non avviato: {_started.get('error')}"))
                        else:
                            _job = _started.get('job_id')
                            console.print(f"[dim]Consiglio {_v_profilo} avviato (job {_job}); attendo l'esito...[/dim]")
                            while True:
                                _res = _vl.wait_verdict(25.0, _job)
                                _st = _res.get('status')
                                if _st == 'done':
                                    console.print(_res.get('result') or '(verdetto vuoto)')
                                    if _res.get('file'):
                                        console.print(f"[dim]Salvato in {_res.get('file')}[/dim]")
                                    break
                                if _st == 'running':
                                    console.print(f"[dim]Ancora in elaborazione ({_res.get('elapsed_s')}s); continuo ad attendere.[/dim]")
                                    continue
                                console.print(ui_error(f"[!] Verdetto {_st}: {_res.get('error')}"))
                                break
                    except Exception as v_e:
                        log_error("verdict/run", v_e)
                        console.print(ui_error(f"[!] Verdetto fallito: {v_e}"))
                continue

            # --- Subagent on-demand: esecuzione sincrona, read-only, budgetata ---
            if lower_input.startswith('/subagent'):
                _s_parts = user_input.split(None, 2)
                _s_ruolo = _s_parts[1].lower() if len(_s_parts) > 1 else ""
                _s_ctx = _s_parts[2].strip() if len(_s_parts) > 2 else ""
                if _s_ruolo == "review":
                    _s_ruolo = "reviewer"
                if _s_ruolo not in ("scout", "reviewer"):
                    console.print("[dim]Uso: /subagent scout|review [file1,file2,...] — analisi read-only budgetata[/dim]")
                else:
                    try:
                        from subagents.roles import run_role
                        console.print(f"[dim]Subagent {_s_ruolo} in esecuzione...[/dim]")
                        _s_res = run_role(_s_ruolo, contesto=_s_ctx)
                        _s_report = _s_res.get("report", _s_res.get("errore", "(nessun risultato)"))
                        console.print(Markdown(_s_report))
                    except Exception as s_e:
                        log_error("subagents/run", s_e)
                        console.print(ui_error(f"[!] Subagent fallito: {s_e}"))
                continue

            if lower_input.startswith('/usage'):
                # P3 - Report costi da usage_log: /usage [giorni] [--csv|prices]
                _u_days, _u_mode = 7, ""
                for _u_p in user_input.split()[1:]:
                    if _u_p == "--csv":
                        _u_mode = "csv"
                    elif _u_p == "prices":
                        _u_mode = "prices"
                    elif _u_p.lstrip('-').isdigit():
                        _u_days = max(1, int(_u_p.lstrip('-')))
                try:
                    if _u_mode == "prices":
                        console.print("[bold]Tariffe LLM (USD / 1M token: prompt | completion):[/]")
                        for _k, _p, _c in all_prices():
                            _tag = " [green]~ scontato[/]" if _k.startswith("deepseek-v4-flash-latest") else ""
                            console.print(f"  - {_k:<24}[gold1]{_p:>7.2f}[/] | [gold1]{_c:>7.2f}[/]{_tag}")
                    else:
                        from memory_meta import usage_rows, usage_stats
                        _rows = usage_rows(_u_days)
                        if not _rows:
                            console.print(f"[yellow]Nessun utilizzo registrato negli ultimi {_u_days} giorni.[/]")
                        elif _u_mode == "csv":
                            import csv
                            _csv_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "usage_report.csv")
                            with open(_csv_path, "w", newline="", encoding="utf-8") as _f:
                                _w = csv.writer(_f)
                                _w.writerow(["ts_utc", "ts_local", "session_id", "model",
                                             "prompt_tokens", "completion_tokens", "total_tokens", "cost_usd"])
                                for _r in _rows:
                                    _cu = cost_usd(_r["model"], _r["prompt_tokens"], _r["completion_tokens"])
                                    _w.writerow([_r["ts_utc"],
                                                 time.strftime("%Y-%m-%d %H:%M:%S", time.localtime(_r["ts_utc"])),
                                                 _r["session_id"], _r["model"],
                                                 _r["prompt_tokens"], _r["completion_tokens"], _r["total_tokens"],
                                                 f"{_cu:.4f}" if _cu is not None else ""])
                            console.print(f"[green][*] Export CSV:[/] {_csv_path} ({len(_rows)} chiamate)")
                        else:
                            _st = usage_stats(_u_days)
                            _tot_cost, _cost_m, _unk_m = 0.0, {}, set()
                            for _r in _rows:
                                _cu = cost_usd(_r["model"], _r["prompt_tokens"], _r["completion_tokens"])
                                _tot_cost += _cu or 0.0
                                _cost_m[_r["model"]] = _cost_m.get(_r["model"], 0.0) + (_cu or 0.0)
                                if _cu is None:
                                    _unk_m.add(_r["model"])
                            _usage_rows = []
                            for _m in _st["by_model"]:
                                _usage_rows.append((escape(_m["model"]), str(_m["calls"]),
                                                   f"{_m['prompt_tokens']:,}", f"{_m['completion_tokens']:,}",
                                                   f"{_cost_m.get(_m['model'], 0.0):.4f}" if _m["model"] not in _unk_m else "n/d"))
                            _tt = _st["totals"]
                            _usage_rows.append(("[bold]TOTALE[/]", str(sum(_m["calls"] for _m in _st["by_model"])),
                                                f"{_tt['prompt_tokens']:,}", f"{_tt['completion_tokens']:,}",
                                                f"[bold gold1]{_tot_cost:.4f}[/]"))
                            console.print(ui_table(
                                ("Modello", "Chiamate", "Prompt tok", "Compl. tok", "Costo USD"),
                                _usage_rows,
                                justify=("left", "right", "right", "right", "right"),
                                title=f"Usage ultimi {_u_days} giorni",
                            ))
                            if _unk_m:
                                console.print(f"[dim]n/d = prezzo sconosciuto per: {', '.join(sorted(_unk_m))} "
                                              f"(vedi /usage prices; override: prices_override.json)[/]")
                except Exception as _e_u:
                    log_error("astral/usage", _e_u)
                    console.print(ui_error(f"[!] /usage error: {_e_u}"))
                continue

            if lower_input.startswith('/model '):
                target = lower_input.split(maxsplit=1)[1].strip()
                if target in MODELS:
                    set_current_model(MODELS[target])
                else:
                    set_current_model(target)
                
                try:
                    cfg_data = {}
                    if os.path.exists(CONFIG_FILE):
                        with open(CONFIG_FILE, "r", encoding="utf-8") as f:
                            cfg_data = json.load(f)
                    cfg_data["current_model"] = get_current_model()
                    with open(CONFIG_FILE, "w", encoding="utf-8") as f:
                        json.dump(cfg_data, f, indent=4)
                except Exception:
                    pass
                console.print(f"[dim][*] Modello impostato a [bold dark_orange]{get_current_model()}[/][/dim]")
                continue

            # Routing dinamico pesato (qualita' 0.7 [LiveBench] + costo 0.3 [OpenRouter],
            # gate confidenza + isteresi) con fallback euristico legacy
            try:
                from routing_engine import learn_from_user_feedback
                learn_from_user_feedback(user_input, context=messages)
            except Exception:
                pass
            active_model = route_model(user_input, context=messages)
            if get_current_model() == "auto":
                try:
                    from routing_engine import descrivi_ultima_decisione
                    console.print(f"[dim]{descrivi_ultima_decisione(active_model)}[/dim]")
                except Exception:
                    if active_model == MODEL_CODE:
                        console.print(f"[dim][*] Riconosciuta richiesta Codice -> Routing: [dark_orange]{MODEL_CODE}[/][/dim]")
                    else:
                        console.print(f"[dim][*] Riconosciuta richiesta Conversazione -> Routing: [gold1]{MODEL_CONVERSATION}[/][/dim]")

            # Loop detection (token-optimizer): avvisa se richieste molto simili si ripetono
            if loop_det.check(user_input):
                console.print("[bold orange_red1][!] Possibile loop rilevato: richieste molto simili ripetute. Considera /clear o riformula.[/]")
                loop_det.strike = 0
            # Resume checkpoint: al primo messaggio della sessione, inietta il checkpoint precedente piu' rilevante
            if session_first_msg and not messages:
                cp = checkpoint_pull(user_input)
                if cp:
                    console.print("[dim][*] Checkpoint sessione precedente recuperato (contesto iniettato).[/dim]")
                    messages.append({
                        "role": "user",
                        "content": (
                            "[MEMORIA STORICA NON ISTRUZIONE - verifica prima di usarla]\n"
                            + cp
                        ),
                    })
                session_first_msg = False
            messages.append({"role": "user", "content": user_input})
            
            # Live Sliding Window respiratorio: finestra sinusoidale + ledger (v2)
            messages = safe_trim_context(messages)
            if last_meta.get("event") == "consolidate":
                console.print(f"[dim][*] Respirazione: consolidamento (ciclo #{last_meta['cycle']}, "
                              f"{last_meta['facts']} fatti attivi, finestra {last_meta['window']} turni).[/dim]")

            try:
                response, used_model = call_with_dynamic_fallback(
                    messages,
                    tools_schema=gateway.select_schemas(user_input, context=messages),
                    primary_model=active_model)
            except Exception as e:
                console.print(f"[orange_red1]{escape(str(e))}[/red]")
                try:
                    from routing_engine import record_model_penalty, classify_input
                    record_model_penalty(
                        used_model if "used_model" in locals() else active_model,
                        classify_input(user_input)[0],
                        reason=f"llm_request_failed: {e}",
                        weight=0.75,
                    )
                except Exception:
                    pass
                if messages and messages[-1].get("role") == "user":
                    messages.pop()
                continue

            usage = getattr(response, "usage", None)
            record_usage(used_model, usage)  # Fase 3 UsageTracker (silenzioso)
            # Il routing usa lo usage reale anche per il primo tentativo: il
            # log storico viene aggiornato prima della prossima decisione.
            print_telemetry(response, used_model)
            msg = response.choices[0].message

            # Gestione tool calls senza limite rigido: continua fino alla risposta finale del modello
            tool_followup_error = None
            tool_cancelled = False
            tool_results_for_learning = []
            while msg.tool_calls:
                if msg.content:
                    console.print(f"[dim]{msg.content.strip()}[/dim]")
                # Strutturiamo il messaggio assistant con i tool calls
                asst_tool_msg = {
                    "role": "assistant",
                    "content": msg.content or "",
                    "tool_calls": [
                        {
                            "id": tc.id,
                            "type": tc.type,
                            "function": {
                                "name": tc.function.name,
                                "arguments": tc.function.arguments
                            }
                        } for tc in msg.tool_calls
                    ]
                }
                messages.append(asst_tool_msg)

                # Se il questionario viene annullato, deve avere precedenza sugli
                # eventuali tool accodati nello stesso messaggio: nessuna azione
                # deve partire prima che l'utente abbia scelto di proseguire.
                tool_calls = list(msg.tool_calls)
                tool_calls.sort(key=lambda tc: 0 if tc.function.name == "ask_user_question" else 1)
                for tool_call in tool_calls:
                    fn_name = tool_call.function.name
                    try:
                        fn_args = json.loads(tool_call.function.arguments)
                    except Exception:
                        fn_args = {}
                    
                    console.print(f" [dim]> Tool: [gold1]{fn_name}[/][/dim]")
                    
                    sys_dirs = ["c:\\windows", "system32", "program files"]
                    
                    if fn_name == "run_powershell_cmd":
                        cmd = fn_args.get("command", "")
                        if any(x in cmd.lower() for x in sys_dirs):
                            confirm = ui_confirm(f"Confermi l'esecuzione di '{cmd}' su sistema protetto?")
                            if confirm.lower() != 's':
                                result = {"error": "Annullato dall'utente."}
                            else:
                                result = gateway_call_tool(fn_name, fn_args)
                        else:
                            result = gateway_call_tool(fn_name, fn_args)
                    elif fn_name == "move_to_trash":
                        path = fn_args.get("path", "")
                        if any(x in path.lower() for x in sys_dirs):
                            confirm = ui_confirm(f"Confermi eliminazione protetta di '{path}'?")
                            if confirm.lower() != 's':
                                result = {"error": "Annullato dall'utente."}
                            else:
                                result = gateway_call_tool(fn_name, fn_args)
                        else:
                            result = gateway_call_tool(fn_name, fn_args)
                    else:
                        result = gateway_call_tool(fn_name, fn_args)
                        
                    tool_results_for_learning.append(result)
                    messages.append({
                        "role": "tool",
                        "tool_call_id": tool_call.id,
                        "content": maybe_offload_tool_result(json.dumps(result), fn_name)
                    })
                    if (fn_name == "ask_user_question"
                            and isinstance(result, dict)
                            and isinstance(result.get("details"), dict)
                            and result["details"].get("cancelled")):
                        tool_cancelled = True
                        break

                if tool_cancelled:
                    # L'annullamento e' definitivo: non chiamare il modello di
                    # follow-up e non consentire una nuova catena di tool.
                    break

                try:
                    # Il risultato del tool puo' cambiare il profilo del lavoro:
                    # rivaluta il modello invece di fissare quello del primo prompt.
                    active_model = route_model(
                        user_input + " [follow-up dopo esecuzione tool]",
                        context=messages,
                    )
                    if get_current_model() == "auto":
                        try:
                            from routing_engine import descrivi_ultima_decisione
                            console.print(f"[dim]{descrivi_ultima_decisione(active_model)}[/dim]")
                        except Exception:
                            pass
                    response, followup_model = call_with_dynamic_fallback(
                        messages,
                        tools_schema=gateway.select_schemas(user_input, context=messages),
                        primary_model=active_model)
                    record_usage(followup_model, getattr(response, "usage", None))
                    print_telemetry(response, followup_model)
                    msg = response.choices[0].message
                except Exception as e:
                    tool_followup_error = str(e)
                    console.print(f"[orange_red1]{tool_followup_error}[/]")
                    try:
                        from routing_engine import record_model_penalty, classify_input
                        record_model_penalty(
                            followup_model if "followup_model" in locals() else active_model,
                            classify_input(user_input)[0],
                            reason=f"tool_followup_failed: {e}",
                            weight=0.75,
                        )
                    except Exception:
                        pass
                    break


            final_text = "Operazione annullata dall'utente." if tool_cancelled else (msg.content or "").strip()
            if not final_text:
                # Fallback: alcuni modelli mettono il testo in reasoning_content o tornano vuoti
                final_text = (getattr(msg, "reasoning_content", None) or getattr(msg, "reasoning", None) or "").strip()
            if not final_text and tool_followup_error:
                final_text = (
                    "La fase strumenti e' terminata, ma non e' stato possibile generare "
                    f"il riepilogo finale: {tool_followup_error}"
                )
            if final_text:
                console.print(Markdown(final_text))
                messages.append({"role": "assistant", "content": final_text})
            else:
                console.print("[dim](Nessuna risposta testuale dal modello - riformula o riprova.)[/dim]")

            if tool_results_for_learning:
                try:
                    from routing_engine import record_personal_outcome
                    from routing_engine import classify_input
                    categoria_esito = classify_input(user_input)[0]
                    record_personal_outcome(
                        used_model, categoria_esito,
                        {"tool_results": tool_results_for_learning,
                         "input": user_input, "context": messages},
                        context=messages,
                    )
                except Exception:
                    pass

            # Salva periodicamente lo storico per non perderlo in caso di chiusura imprevista
            save_persistent_history(messages)

        except KeyboardInterrupt:
            checkpoint_save(messages)
            save_persistent_history(messages)
            console.print("\n[dim]Uscita in corso... Storico salvato.[/dim]")
            break
        except Exception as e:
            log_error("main_loop", e)
            started = launch_self_repair()
            console.print(ui_error(f"[!] Errore: {e}"))
            if started:
                console.print("[dim]Autoriparazione avviata in background; la sessione resta attiva.[/dim]")


def _handle_task_command(user_input):
    """Gestisce i comandi /task* (registro task git+SQLite).

    Sottocomandi:
      /task                 elenco task
      /task-new <id> <titolo>   crea task + branch
      /task-test <id>       esegue la suite e marca test_passed (verificato)
      /task-done <id>       marca integrata (verifica hash suite)
      /task-sync            riconcilia registro <-> git
    """
    try:
        import task_tracker as tt
    except Exception as e:
        console.print(ui_error(f"[!] task_tracker non disponibile: {e}"))
        return
    parts = user_input.split(None, 2)
    sub = parts[0].lower()
    try:
        if sub == '/task':
            rows = tt.list_active()
            if not rows:
                console.print("[dim]Nessuna task registrata. Usa /task-new <id> <titolo>.[/dim]")
                return
            t = ui_table(["ID", "Titolo", "Stato", "Test", "Integrata"],
                         [[str(r['id']), r['title'], r['status'],
                           "si" if r['test_passed'] else "no",
                           r['ts_integrated'] or "-"] for r in rows])
            console.print(t)
        elif sub == '/task-new':
            if len(parts) < 3 or not parts[1].isdigit():
                console.print(ui_error("[!] Uso: /task-new <id> <titolo>"))
                return
            res = tt.create_task(int(parts[1]), parts[2])
            if res.get('error'):
                console.print(ui_error(f"[!] {res['error']}"))
            else:
                console.print(f"[green][*] Task #{res['id']} creata, branch {res['branch']}.[/green]")
        elif sub == '/task-test':
            if len(parts) < 2 or not parts[1].isdigit():
                console.print(ui_error("[!] Uso: /task-test <id>"))
                return
            console.print("[dim]Esecuzione suite di test...[/dim]")
            res = tt.mark_test_passed(int(parts[1]))
            if res.get('ok'):
                console.print(f"[green][*] Test verdi per task #{res['id']} "
                              f"(hash suite {res['suite_hash']}).[/green]")
            else:
                console.print(ui_error(f"[!] Test falliti: {res.get('detail')}"))
        elif sub == '/task-done':
            if len(parts) < 2 or not parts[1].isdigit():
                console.print(ui_error("[!] Uso: /task-done <id>"))
                return
            res = tt.mark_integrated(int(parts[1]))
            if res.get('error'):
                console.print(ui_error(f"[!] {res['error']}"))
            else:
                console.print(f"[green][*] Task #{res['id']} integrata.[/green]")
                for w in res.get('warnings', []):
                    console.print(f"[yellow][!] {w}[/yellow]")
        elif sub == '/task-sync':
            rep = tt.sync_from_git()
            console.print(f"[bold cyan]Task sync[/]: creati={rep['created']} "
                          f"integrati={rep['integrated']}")
            for w in rep['warnings']:
                console.print(f"[yellow][!] {w}[/yellow]")
        else:
            console.print(ui_error("[!] Sottocomandi: /task /task-new /task-test /task-done /task-sync"))
    except Exception as e:
        console.print(ui_error(f"[!] /task error: {e}"))


# ------------------------------------------------------------------ Guardie anti-loop
# 1) Lock single-instance: impedisce che due astral.py girino insieme (causa di
#    retry/polling duplicati e spin-loop). 2) Watchdog CPU: rileva spin-loop e
#    scrive heartbeat per diagnosi esterna.
# Multi-instance intenzionale: piu' processi Astral possono collaborare.
# I dati condivisi usano SQLite; la cronologia conversazionale e' per-sessione.

def _start_loop_watchdog():
    """Thread daemon: rileva spin-loop (CPU costantemente alta) e scrive heartbeat."""
    import threading
    hb_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), ".astral_heartbeat.tmp")
    last_cpu = time.process_time()
    last_t = time.time()
    spin_streak = 0

    def _tick():
        nonlocal last_cpu, last_t, spin_streak
        while True:
            time.sleep(5)
            now = time.time()
            cpu = time.process_time()
            delta_cpu = cpu - last_cpu
            delta_t = now - last_t
            last_cpu, last_t = cpu, now
            pct = (delta_cpu / delta_t * 100) if delta_t > 0 else 0
            try:
                with open(hb_file, "w", encoding="utf-8") as f:
                    f.write(f"{now:.0f} cpu={pct:.0f}%")
            except Exception:
                pass
            if pct > 80:
                spin_streak += 1
            else:
                spin_streak = 0
            if spin_streak >= 6:  # 30s consecutivi >80% CPU
                console.print(
                    "[bold orange_red1][!] WATCHDOG: possibile spin-loop (CPU >80% per 30s). "
                    "Se non stai eseguendo un calcolo, premi Ctrl+C o /exit.[/]"
                )
                try:
                    log_error("watchdog_spin_loop", RuntimeError(f"CPU {pct:.0f}% per {spin_streak * 5}s"))
                except Exception:
                    pass
                spin_streak = 0

    t = threading.Thread(target=_tick, daemon=True)
    t.start()
    return t


if __name__ == "__main__":
    _start_loop_watchdog()
    init_db()
    try:
        import task_tracker as _tt
        _tt.init_tasks()
        _rep = _tt.sync_from_git()  # riconciliazione obbligatoria all'avvio
        if _rep.get('created') or _rep.get('integrated') or _rep.get('warnings'):
            console.print(f"[dim][task] sync: creati={_rep['created']} "
                          f"integrati={_rep['integrated']} "
                          f"avvisi={len(_rep['warnings'])}[/dim]")
    except Exception:
        pass  # mai bloccare l'avvio per il tracker
    _bs = bootstrap_meta()
    console.print(f"[dim][meta] bootstrap: scanned={_bs.get('scanned', 0)} imported={_bs.get('imported', 0)} skipped={_bs.get('skipped', 0)} error={_bs.get('error', 0)}[/dim]")
    try:
        import selfmap

        selfmap.generate()  # indice semantico completo ad ogni avvio
        selfmap.refresh_bootstrap()  # contesto minimo derivato e verificabile
        selfmap.refresh_knowledge_map()  # indice separato della conoscenza distribuita
        selfmap.start_watcher()  # sincronizzazione continua durante il runtime
    except Exception:
        pass  # mai bloccare l'avvio per la mappa
    if "--repair" in sys.argv:
        try:
            auto_repair()
        except Exception as _rep_e:
            try:
                log_error("auto_repair_cli", _rep_e)
            except Exception:
                pass
    else:
        main()

