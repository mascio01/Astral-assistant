# -*- coding: utf-8 -*-
# astral.py - Entry point: REPL, comandi slash, ciclo conversazione
import json
import os
import subprocess
import sys
import time

from core_io import _early_log, console, install_exception_hooks, launch_self_repair, log_error

try:
    import stt_integration
except Exception as e:
    stt_integration = None
    _early_log("import stt_integration", e)

from rich.panel import Panel
from rich.markdown import Markdown
from rich.markup import escape
from rich.table import Table

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

def _voice_confirm_async(text, timeout):
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


def read_voice_input(text, auto_send_s=1.5):
    """Finestra di conferma trascrizione (1.5s): INVIO=invia subito,
    ESC=annulla, altro tasto=modifica (disattiva il timer). Auto-invio
    allo scadere. Fallback (no prompt_toolkit): conferma senza timer."""
    _lights_notify("ok")  # casella verde: trascrizione pronta
    console.print(f"[bold gold1]Trascrizione:[/] {escape(text)}")
    if _get_voice_prompt_session() is not None:
        console.print(f"[dim]INVIO=invia | ESC=annulla | auto-invio tra "
                      f"{auto_send_s}s (un tasto qualsiasi disattiva il timer)[/dim]")
        try:
            ev = _voice_confirm_async(text, auto_send_s)
        except (EOFError, KeyboardInterrupt):
            return ""
        except Exception:
            ev = None
        if ev is None:  # prompt async PT non disponibile: prompt classico
            try:
                return voice_prompt_session.prompt(
                    "Astral (Voce) > ", default=text).strip()
            except (EOFError, KeyboardInterrupt):
                return ""
        return text if ev == "__AUTO__" else ev
    try:
        ok = console.input("[bold orange_red1]Invio la trascrizione? (INVIO=s / n=modifica): [/]")
        return text if ok.strip().lower() != "n" else console.input("Testo corretto: ").strip()
    except (EOFError, KeyboardInterrupt):
        return ""


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

    console.print(Panel(
        f"[bold white]Il tuo assistente operativo per Windows 11[/]\n"
        f"[dim]Conversazione, sviluppo e automazione in un unico spazio.[/]\n\n"
        f"[bold]ROUTING[/]     [bold green]AUTO[/]  [dim]pool dinamico[/]\n"
        f"[bold]CHAT[/]        [dim]{MODEL_CONVERSATION}[/]\n"
        f"[bold]SVILUPPO[/]    [dim]{MODEL_CODE}[/]\n"
        f"[bold]ATTIVO[/]      [bold cyan]{get_current_model()}[/]\n\n"
        f"[dim]Scrivi una richiesta oppure usa [/][bold cyan]/help[/][dim] per i comandi.[/]",
        title="[bold bright_cyan] ASTRAL [/bold bright_cyan] [dim]· workspace[/dim]",
        subtitle="[dim]online · pronto a collaborare[/dim]",
        border_style="bright_cyan", padding=(1, 2)
    ))

    messages = get_history()
    loop_det = LoopDetector()
    session_first_msg = True

    _mt_run = False
    while True:
        try:
            if not _mt_run:
                _mt_run = True
                run_meta_maintenance()
            cm = get_current_model()
            prompt_label = "Astral (Auto)" if cm == "auto" else cm
            user_input = console.input(f"\n[bold dodger_blue1]{prompt_label}[/] [bold spring_green1]>[/] ").strip()
            if not user_input:
                continue

            # Feedback immediato: non lasciare la console apparentemente bloccata
            # mentre selfmap/routing preparano la richiesta.
            console.print("[dim]Elaborazione avviata...[/dim]")

            # La selfmap viene mantenuta dal watcher in background all'avvio;
            # non deve mai inserirsi tra INVIO e la chiamata OpenRouter.

            if user_input.lower() == '/voice':
                if stt_integration is None or not stt_integration.is_available():
                    console.print("[bold orange_red1]STT non disponibile. Dettagli in error_log.txt.[/]")
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
                console.print("[dim]Sessione salvata. Arrivederci![/dim]")
                break
            
            lower_input = user_input.lower()
            if lower_input == '/clear':
                checkpoint_save(messages)
                clear_persistent_history()
                messages = []
                session_first_msg = True
                continue

            if lower_input == '/breath':
                import astral_trim as _at
                _lm = _at.last_meta
                console.print(
                    f"[bold cyan]Respirazione contesto (v2)[/]\n"
                    f"  periodo: {_at.BREATH_PERIOD} turni | finestra: {_at.WAVE_MIN_TURNS}-"
                    f"{_at.WAVE_MAX_TURNS} turni\n"
                    f"  budget: {_at.BUDGET_MIN_CHARS}-{_at.BUDGET_MAX_CHARS} char | "
                    f"hard cap: {_at.HARD_CAP_CHARS}\n"
                    f"  turno {_lm.get('turn')}, ciclo #{_lm.get('cycle')}, "
                    f"fatti attivi: {_lm.get('facts')}\n"
                    f"  ultimo evento: {_lm.get('event')} | chars finestra: {_lm.get('chars')}\n"
                    f"  legacy forzato: {'SI' if os.environ.get('ASTRAL_TRIM_LEGACY') == '1' else 'no'}")
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
                        console.print(f"[bold cyan]Selfmap[/] gen: {_idx['generated']} | file: {_idx['file_count']} | righe: {_idx['total_lines']}")
                        _big = sorted(_idx['files'].items(), key=lambda kv: -kv[1]['lines'])[:8]
                        for _rel, _i in _big:
                            console.print(f"  {_rel} ({_i['lines']} righe, {len(_i['classes']) + len(_i['functions'])} simboli)")
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
                            console.print(f"[red]File '{_rel}' non in selfmap.[/red]")
                        else:
                            console.print(f"[bold cyan]{_rel}[/] ({_i['lines']} righe) — {_i['summary'] or 'non documentata'}")
                            console.print(f"  hash: {_i['sha256']} | import: {', '.join(_i['imports']) or '-'}")
                            console.print(f"  usato da: {', '.join(_idx['imported_by'].get(_rel, [])) or '-'}")
                            for _c in _i['classes']:
                                console.print(f"  class {_c['name']} righe {_c['line']}-{_c['end_line']}")
                            for _f in _i['functions']:
                                console.print(f"  {_f['signature']} righe {_f['line']}-{_f['end_line']}")
                except Exception as _e_sm:
                    console.print(f"[red]/self error: {_e_sm}[/red]")
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
                        console.print(f"[red]/scrape errore: {type(_sc_e).__name__}: {_sc_e}[/red]")
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
                        console.print(f"[bold]Ultime {len(_rows)} esecuzioni:[/bold]")
                        for _r in _rows:
                            _st = _r.get('status', '?')
                            _col = 'red' if _st == 'error' else ('green' if _st == 'ok' else 'yellow')
                            _dur = _r.get('duration_ms')
                            _dur_s = f"{_dur}ms" if _dur is not None else "-"
                            _err = f" | {str(_r.get('error'))[:60]}" if _r.get('error') else ""
                            console.print(f"[dim]{_r.get('ts', '?')}[/dim] [{_col}]{_st}[/] {_r.get('tool', '?')} ({_dur_s}){_err}")
                except Exception as _e_el:
                    console.print(f"[red]/execlog error: {_e_el}[/red]")
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
                    console.print(f"[red]/meta-stats error: {_e_ms}[/red]")
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
                    console.print(f"[red]/meta-pin error: {_e_mp}[/red]")
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
                    console.print(f"[red]/meta-tag error: {_e_mt}[/red]")
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
                    for _r in _ml_rows:
                        _ml_pin = " [yellow][pin][/yellow]" if _r["pinned"] else ""
                        _ml_when = time.strftime('%d/%m %H:%M', time.localtime(_r["ts_utc"] or 0))
                        console.print(f"[dim]{_r['src_ref']}[/] [dim]{_ml_when}[/] {escape(str(_r['tool']))} | {_r['event_type']} | {_r['status']}{_ml_pin}")
                except Exception as _e_ml:
                    console.print(f"[red]/meta-last error: {_e_ml}[/red]")
                continue

            if lower_input == '/prune':
                try:
                    from memory_meta import prune
                    _pr = prune()
                    console.print(f"[bold cyan]Prune eseguito[/]: soft_deleted={_pr['soft_deleted']} purged={_pr['purged']} pinned_immuni={_pr['pinned']}")
                    if _pr.get('snapshot'):
                        console.print(f"[dim]Snapshot: {_pr['snapshot']}[/dim]")
                except Exception as _e_pr:
                    console.print(f"[red]/prune error: {_e_pr}[/red]")
                continue

            if lower_input == '/audit':
                try:
                    from memory_meta import audit_meta
                    _au = audit_meta()
                    if _au.get('error'):
                        console.print(f"[red]/audit error: {_au['error']}[/red]")
                    else:
                        console.print(f"[bold cyan]Audit raw/meta[/]: raw={_au['raw_lines']} meta={_au['meta_rows']} missing={_au['missing_meta']} reimported={_au['reimported']} orphan={_au['orphan_meta']}")
                        console.print(f"[dim]watermark: {_au['watermark']}[/dim]")
                except Exception as _e_au:
                    console.print(f"[red]/audit error: {_e_au}[/red]")
                continue

            _v_cmd = lower_input.split(' ')[0]
            if lower_input.startswith('/verdict') or _v_cmd in ('verdict', 'verdetto'):
                if lower_input.startswith('/verdict'):
                    quesito = user_input[len('/verdict'):].strip()
                else:
                    quesito = user_input.split(' ', 1)[1].strip() if ' ' in lower_input else ''
                _v_profilo = "standard"
                _v_tok = quesito.split(" ", 1)
                if _v_tok and _v_tok[0].lower() in ("lite", "--lite"):
                    _v_profilo = "lite"
                    quesito = _v_tok[1].strip() if len(_v_tok) > 1 else ""
                if not quesito:
                    console.print("[dim]Uso: /verdict [lite] <quesito> (oppure 'verdict [lite] <quesito>') — consiglio di giudici anonimi con verdetto ('lite' = modelli veloci/economici)[/dim]")
                else:
                    # protocollo detached: salva quesito, lancia runner, polling non bloccante
                    try:
                        BASE_DIR = os.path.dirname(os.path.abspath(__file__))
                        tmp_file = os.path.join(BASE_DIR, ".verdict_quesito.tmp")
                        with open(tmp_file, "w", encoding="utf-8") as f:
                            f.write(quesito)
                        out_file = os.path.join(BASE_DIR, ".verdict_out.txt")
                        err_file = os.path.join(BASE_DIR, ".verdict_err.txt")
                        # rimuove eventuali residui di run precedenti
                        for _f in (out_file, err_file):
                            try:
                                os.remove(_f)
                            except Exception:
                                pass
                        # lancia sempre il runner versionato corretto, accanto ad astral.py.
                        # Non usare il vecchio nome nascosto '.verdict_runner.py': causava
                        # avvii falliti dopo i tentativi detached da PowerShell.
                        _runner_file = os.path.join(BASE_DIR, "verdict_runner.py")
                        if not os.path.isfile(_runner_file):
                            raise FileNotFoundError(
                                f"Runner verdetto assente: {_runner_file}. "
                                "Ripristinare verdict_runner.py prima di procedere."
                            )
                        _vp = subprocess.Popen(
                            [sys.executable, _runner_file, _v_profilo],
                            cwd=BASE_DIR,
                            creationflags=getattr(subprocess, "CREATE_NEW_CONSOLE", 0),
                            stdout=subprocess.DEVNULL,
                            stderr=subprocess.DEVNULL,
                        )
                        _verdict_pid = _vp.pid
                        console.print("[dim]Avvio verdetto... polling ogni 15s (max 10 min)[/dim]")
                        deadline = time.time() + 600
                        done = False
                        while time.time() < deadline:
                            time.sleep(15)
                            if os.path.exists(out_file):
                                with open(out_file, "r", encoding="utf-8", errors="replace") as f:
                                    content = f.read()
                                if "VERDICT_DONE" in content:
                                    console.print(content.replace("VERDICT_DONE", "").strip())
                                    done = True
                                    break
                                console.print("[dim]...attendiamo il verdetto...[/dim]")
                            else:
                                console.print("[dim]...attendiamo il verdetto...[/dim]")
                        if not done:
                            console.print("[bold orange_red1][!] Verdetto non completato entro 10 minuti, kill orfano[/]")
                            # Kill mirato: solo PID specifico + albero figli. MAI tutti i python.
                            subprocess.run(
                                ["powershell", "-NoProfile", "-Command",
                                 f"$p = Get-Process -Id {_verdict_pid} -ErrorAction SilentlyContinue; "
                                 f"if ($p) {{ Get-CimInstance Win32_Process -Filter 'ParentProcessId={_verdict_pid}' | "
                                 f"ForEach-Object {{ Stop-Process -Id $_.ProcessId -Force }}; "
                                 f"Stop-Process -Id {_verdict_pid} -Force }}"],
                                capture_output=True, timeout=30)
                        # pulizia file temporanei
                        for _f in (tmp_file, out_file, err_file):
                            try:
                                os.remove(_f)
                            except Exception:
                                pass
                    except Exception as v_e:
                        log_error("verdict/run", v_e)
                        console.print(f"[bold orange_red1][!] Verdetto fallito:[/] {escape(str(v_e))}")
                continue

            # --- Subagents (verdetto 2026-09-15): scout/reviewer detached, budget 8/h, depth 1 ---
            if lower_input.startswith('/subagent'):
                _s_parts = user_input.split(None, 2)
                _s_ruolo = _s_parts[1].lower() if len(_s_parts) > 1 else ""
                _s_ctx = _s_parts[2].strip() if len(_s_parts) > 2 else ""
                if _s_ruolo not in ("scout", "review"):
                    console.print("[dim]Uso: /subagent scout|review [file1,file2,...] — subagent read-only detached (budget 8 job/h, depth 1)[/dim]")
                else:
                    try:
                        from subagents.watchdog import snapshot
                        from subagents.jobspec import check_budget
                        _ok_b, _msg_b = check_budget()
                        if not _ok_b:
                            console.print(f"[bold orange_red1][!] {escape(_msg_b)}[/]")
                        else:
                            snapshot()
                            _s_out = os.path.join(BASE_DIR, ".subagent_out.txt")
                            _s_err = os.path.join(BASE_DIR, ".subagent_err.txt")
                            for _f in (_s_out, _s_err):
                                try:
                                    os.remove(_f)
                                except Exception:
                                    pass
                            _s_tmp = os.path.join(BASE_DIR, ".verdict_quesito.tmp")
                            with open(_s_tmp, "w", encoding="utf-8") as f:
                                f.write(_s_ctx)
                            _sp = subprocess.Popen(
                                [sys.executable, os.path.join(BASE_DIR, "verdict_runner.py"), _s_ruolo, _s_ctx],
                                cwd=BASE_DIR,
                                creationflags=getattr(subprocess, "CREATE_NEW_CONSOLE", 0),
                                stdout=subprocess.DEVNULL,
                                stderr=subprocess.DEVNULL,
                            )
                            console.print(f"[dim]Subagent {_s_ruolo} avviato (PID {_sp.pid}). Polling...[/dim]")
                            _deadline = time.time() + 300
                            _done = False
                            while time.time() < _deadline:
                                time.sleep(10)
                                if os.path.exists(_s_out):
                                    with open(_s_out, "r", encoding="utf-8", errors="replace") as f:
                                        _sc = f.read()
                                    if "VERDICT_DONE" in _sc:
                                        console.print(_sc.replace("VERDICT_DONE", "").strip())
                                        _done = True
                                        break
                            if not _done:
                                console.print("[bold orange_red1][!] Subagent non completato entro 5 min.[/]")
                            # watchdog: verifica hash file critici post-job
                            try:
                                from subagents.watchdog import verify
                                _ch = verify()
                                if _ch:
                                    console.print(f"[bold orange_red1][!] WATCHDOG: file critici modificati dal subagent: {', '.join(_ch)}[/]")
                                else:
                                    console.print("[dim][watchdog] File critici intatti.[/dim]")
                            except Exception:
                                pass
                            for _f in (_s_tmp, _s_out, _s_err):
                                try:
                                    os.remove(_f)
                                except Exception:
                                    pass
                    except Exception as s_e:
                        log_error("subagents/run", s_e)
                        console.print(f"[bold orange_red1][!] Subagent fallito:[/] {escape(str(s_e))}")
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
                            _table = Table(title=f"Usage ultimi {_u_days} giorni")
                            for _col in ("Modello", "Chiamate", "Prompt tok", "Compl. tok", "Costo USD"):
                                _table.add_column(_col, justify="left" if _col == "Modello" else "right")
                            for _m in _st["by_model"]:
                                _table.add_row(escape(_m["model"]), str(_m["calls"]),
                                               f"{_m['prompt_tokens']:,}", f"{_m['completion_tokens']:,}",
                                               f"{_cost_m.get(_m['model'], 0.0):.4f}" if _m["model"] not in _unk_m else "n/d")
                            _tt = _st["totals"]
                            _table.add_row("[bold]TOTALE[/]", str(sum(_m["calls"] for _m in _st["by_model"])),
                                           f"{_tt['prompt_tokens']:,}", f"{_tt['completion_tokens']:,}",
                                           f"[bold gold1]{_tot_cost:.4f}[/]")
                            console.print(_table)
                            if _unk_m:
                                console.print(f"[dim]n/d = prezzo sconosciuto per: {', '.join(sorted(_unk_m))} "
                                              f"(vedi /usage prices; override: prices_override.json)[/]")
                except Exception as _e_u:
                    log_error("astral/usage", _e_u)
                    console.print(f"[red]/usage error: {_e_u}[/red]")
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
                    messages.append({"role": "user", "content": cp})
                session_first_msg = False
            messages.append({"role": "user", "content": user_input})
            
            # Live Sliding Window respiratorio: finestra sinusoidale + ledger (v2)
            messages = safe_trim_context(messages)
            if last_meta.get("event") == "consolidate":
                console.print(f"[dim][*] Respirazione: consolidamento (ciclo #{last_meta['cycle']}, "
                              f"{last_meta['facts']} fatti attivi, finestra {last_meta['window']} turni).[/dim]")

            try:
                response, used_model = call_with_dynamic_fallback(messages, tools_schema=gateway.list_schemas(), primary_model=active_model)
            except Exception as e:
                console.print(f"[orange_red1]{escape(str(e))}[/red]")
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

                for tool_call in msg.tool_calls:
                    fn_name = tool_call.function.name
                    try:
                        fn_args = json.loads(tool_call.function.arguments)
                    except Exception:
                        fn_args = {}
                    
                    console.print(f" [dim]> Esecuzione Tool: [gold1]{fn_name}[/][/dim]")
                    
                    sys_dirs = ["c:\\windows", "system32", "program files"]
                    
                    if fn_name == "run_powershell_cmd":
                        cmd = fn_args.get("command", "")
                        if any(x in cmd.lower() for x in sys_dirs):
                            confirm = console.input(f"[bold orange_red1]Confermi l'esecuzione di '{cmd}' su sistema protetto? (s/N): [/]")
                            if confirm.lower() != 's':
                                result = {"error": "Annullato dall'utente."}
                            else:
                                result = gateway_call_tool(fn_name, fn_args)
                        else:
                            result = gateway_call_tool(fn_name, fn_args)
                    elif fn_name == "move_to_trash":
                        path = fn_args.get("path", "")
                        if any(x in path.lower() for x in sys_dirs):
                            confirm = console.input(f"[bold orange_red1]Confermi eliminazione protetta di '{path}'? (s/N): [/]")
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
                        messages, tools_schema=gateway.list_schemas(), primary_model=active_model)
                    record_usage(followup_model, getattr(response, "usage", None))
                    print_telemetry(response, followup_model)
                    msg = response.choices[0].message
                except Exception as e:
                    tool_followup_error = str(e)
                    console.print(f"[orange_red1]{tool_followup_error}[/]")
                    break


            final_text = (msg.content or "").strip()
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
                        {"tool_results": tool_results_for_learning},
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
            console.print(f"[bold orange_red1][!] Errore:[/] {escape(str(e))}")
            if started:
                console.print("[dim]Autoriparazione avviata in background; la sessione resta attiva.[/dim]")


# ------------------------------------------------------------------ Guardie anti-loop
# 1) Lock single-instance: impedisce che due astral.py girino insieme (causa di
#    retry/polling duplicati e spin-loop). 2) Watchdog CPU: rileva spin-loop e
#    scrive heartbeat per diagnosi esterna.
def _acquire_single_instance():
    """Lock single-instance: se un'altra istanza e' viva, esce subito."""
    import atexit
    lock_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), ".astral.lock")
    try:
        if os.path.exists(lock_file):
            try:
                with open(lock_file, "r", encoding="utf-8") as f:
                    old_pid = int(f.read().strip() or "0")
            except Exception:
                old_pid = 0
            if old_pid > 0:
                _alive = False
                try:
                    _out = subprocess.run(
                        ["tasklist", "/FI", f"PID eq {old_pid}"],
                        capture_output=True, text=True, timeout=5,
                    )
                    _alive = str(old_pid) in _out.stdout
                except Exception:
                    _alive = False
                if _alive:
                    console.print(
                        f"[bold orange_red1][!] Astral e' gia' in esecuzione (PID {old_pid}). "
                        f"Termina l'altra istanza prima di avviarne una nuova.[/]"
                    )
                    sys.exit(1)
        with open(lock_file, "w", encoding="utf-8") as f:
            f.write(str(os.getpid()))
        atexit.register(lambda: _release_lock(lock_file))
    except Exception as e:
        try:
            log_error("single_instance_lock", e)
        except Exception:
            pass


def _release_lock(lock_file):
    try:
        if os.path.exists(lock_file):
            os.remove(lock_file)
    except Exception:
        pass


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
    _acquire_single_instance()
    _start_loop_watchdog()
    init_db()
    _bs = bootstrap_meta()
    console.print(f"[dim][meta] bootstrap: scanned={_bs.get('scanned', 0)} imported={_bs.get('imported', 0)} skipped={_bs.get('skipped', 0)} error={_bs.get('error', 0)}[/dim]")
    try:
        import selfmap

        selfmap.generate()  # indice semantico completo ad ogni avvio
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

