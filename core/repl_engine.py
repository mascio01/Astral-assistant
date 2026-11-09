# -*- coding: utf-8 -*-
"""REPL di Astral: ciclo conversazione, comandi slash, tool loop.

[P2 audit azione ciclo1] Estratto da ``astral.py``: qui vive il corpo di
quello che era ``main()`` (991 righe), mentre ``astral.py`` resta il solo
bootstrap (import, config, avvio).

Il corpo usa simboli definiti nel bootstrap (helper di notifica, limiti di
round, stato modelli): vengono importati in modo lazy all'ingresso di
``run_repl()`` per non creare un ciclo di import a livello di modulo.
"""
import json  # noqa: F401
import os  # noqa: F401
import time  # noqa: F401

from core.core_io import BASE_DIR
from ui import (banner as ui_banner, prompt_label as ui_prompt_label,
                confirm as ui_confirm, table as ui_table, error as ui_error)
from ui.input_line import read_user_input

def run_repl():
    """Esegue il REPL interattivo (ex ``astral.main``)."""
    # Import lazy: astral.py e' gia' inizializzato quando questa funzione parte.
    from astral import (  # noqa: F401
        LoopDetector,
        MAX_SESSIONS,
        MODELS,
        MODEL_CODE,
        MODEL_CONVERSATION,
        MODEL_REASONING,
        Markdown,
        Panel,
        Table,
        _MAX_CONSEC_ERRORS,
        _SESSION_COLORS,
        _TOOL_ROUND_DEFAULT,
        _TOOL_ROUND_HARD_DEFAULT,
        _TOOL_ROUND_WARN1_DEFAULT,
        _TOOL_ROUND_WARN2_DEFAULT,
        _build_verdict_input,
        _claim_session_slot,
        _estimate_context_tokens,
        _get_context_warn_pct,
        _get_context_warn_tokens,
        _get_tool_round_limit,
        _get_tool_token_budget,
        _handle_task_command,
        _log_loop_diagnostic,
        _notify_turn_done,
        _parse_verdict_request,
        _sys_notify,
        _usage_total_tokens,
        all_prices,
        auto_repair,
        auto_repair_graph,
        call_with_dynamic_fallback,
        checkpoint_pull,
        checkpoint_save,
        clear_persistent_history,
        console,
        cost_usd,
        escape,
        gateway,
        gateway_call_tool,
        get_current_model,
        get_history,
        get_telemetry_enabled,
        get_telemetry_stats,
        get_value,
        last_meta,
        launch_self_repair,
        log_error,
        maybe_offload_tool_result,
        print_telemetry,
        record_usage,
        repair_watchdog,
        route_model,
        run_meta_maintenance,
        safe_trim_context,
        save_persistent_history,
        set_current_model,
        set_session_color,
        set_telemetry_enabled,
        set_value,
    )

    # Watchdog self-repair: gli errori nuovi vengono gestiti senza fermare il loop
    try:
        repair_watchdog()
    except Exception:
        pass

    # [P0-2 audit azione ciclo1] Lettura via adapter unico (config.db SSOT,
    # migrazione lazy dal config.json legacy). Nessun accesso diretto al file.
    try:
        _cfg_models = get_value("models", None)
        if isinstance(_cfg_models, dict) and _cfg_models:
            MODELS.update(_cfg_models)
        _cfg_model = get_value("current_model", None)
        if _cfg_model:
            set_current_model(MODELS.get(_cfg_model, _cfg_model))
    except Exception as _cfg_e:
        log_error("load_config_startup", _cfg_e)

    console.print(ui_banner(
        body=(
            f"[bold white]Il tuo assistente operativo per Windows 11[/]\n"
            f"[dim]Conversazione, sviluppo e automazione in un unico spazio.[/]\n\n"
            f"[bold]ROUTING[/]     [bold green]AUTO[/]  [dim]pool dinamico[/]\n"
            f"[bold]CHAT[/]        [dim]{MODEL_CONVERSATION}[/]\n"
            f"[bold]SVILUPPO[/]    [dim]{MODEL_CODE}[/]\n"
            f"[bold]RAGIONAMENTO[/] [dim]{MODEL_REASONING}[/]\n"
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
    _consec_errors = 0
    _busy = False          # True durante l'elaborazione di un turno (per distinguere Ctrl+C)
    _turn_t0 = time.time()  # inizio turno: serve al toast "tocca a te"
    _turn_start_idx = 0    # indice dei messaggi all'inizio del turno corrente
    while True:
        try:
            _busy = False  # al prompt Ctrl+C = esci; viene alzato solo durante l'elaborazione
            if not _mt_run:
                _mt_run = True
                run_meta_maintenance()
                # Budget tier: check credito residuo OpenRouter UNA volta per
                # processo; modula il peso del costo nel routing dinamico.
                try:
                    from core.budget_guard import check_credits
                    from routing_engine import set_budget_tier
                    set_budget_tier(*check_credits())
                except Exception:
                    pass  # il routing resta sui pesi base in caso di errore
            cm = get_current_model()
            prompt_label = "Astral (Auto)" if cm == "auto" else cm
            _prompt_color = _SESSION_COLORS[_session_slot % len(_SESSION_COLORS)]
            set_session_color(_prompt_color)  # aggancia anche i messaggi "Elaborazione" allo stesso colore
            # Input con bracketed paste: un testo multi-riga incollato entra
            # come UN unico messaggio (niente popup del terminale) e viene
            # ridotto a una sola riga stile LingoJam prima dell'invio.
            user_input = read_user_input(
                f"\n{ui_prompt_label(prompt_label, _prompt_color)}"
            )
            if not user_input:
                continue
            _busy = True
            _turn_t0 = time.time()
            _turn_start_idx = len(messages)  # punto di rollback se l'utente annulla

            # Feedback immediato: non lasciare la console apparentemente bloccata
            # mentre selfmap/routing preparano la richiesta.
            console.print(f"[dim][bold {_prompt_color}]Elaborazione in corso...[/][/dim]")

            # La selfmap viene mantenuta dal watcher in background all'avvio;
            # non deve mai inserirsi tra INVIO e la chiamata OpenRouter.

            if user_input.lower() in ['exit', 'quit']:
                checkpoint_save(messages)
                save_persistent_history(messages)
                console.print("[dim][*] Sessione salvata. Arrivederci![/dim]")
                break

            lower_input = user_input.lower()
            if lower_input == '/clear':
                checkpoint_save(messages)
                clear_persistent_history()
                # Azzera anche lo stato di sessione del trimmer: senza questo
                # l'obiettivo globale restava in memoria e ricompariva nel
                # primo checkpoint dopo la pulizia.
                from core import astral_trim as _at  # modulo canonico (evita doppio oggetto modulo)
                _at.reset_session_state()
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
                    f"  [bold cyan]/graphify[/] [dim]<domanda>|update|hubs|explain[/]   knowledge graph\n"
                    f"  [bold cyan]/repair[/]      auto-riparazione  |  [bold cyan]/repair-graph[/]\n"
                    f"  [bold cyan]/telemetry[/]   toggle telemetria  |  [bold cyan]/stats[/]\n\n"
                    f"[bold]STRUMENTI[/]\n"
                    f"  [bold cyan]/scrape[/] <url>  [dim]--refresh --json --no-cache --no-robots[/]\n"
                    f"  [bold cyan]/execlog[/] [n]   log esecuzioni tool\n"
                    f"  [bold cyan]/usage[/] [g]     costi [dim]--csv | prices[/]  |  [bold cyan]/model[/] <nome>\n\n"
                    f"[bold]AGENTI[/]\n"
                    f"  [bold cyan]/peer[/]        finestra chat peer-to-peer (parallela)\n"
                    f"  [bold cyan]/verdict[/] [lite] <quesito>   giudici anonimi\n"
                    f"  [bold cyan]/subagent[/] scout|review [file]   subagent detached\n\n"
                    f"[dim]Scrivi una richiesta libera oppure usa i comandi sopra.[/]",
                    title="[bold bright_cyan] ASTRAL [/bold bright_cyan] [dim]· help[/dim]",
                    border_style="bright_cyan", padding=(1, 2)
                ))
                continue

            if lower_input == '/breath':
                from core import astral_trim as _at  # modulo canonico (evita doppio oggetto modulo)
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
                    from utils import selfmap as _sm
                    if _sm.is_stale():
                        _sm.generate()
                    _idx = json.load(open(os.path.join(BASE_DIR, '.selfmap.json'), encoding='utf-8'))
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

            if lower_input.startswith('/graphify'):
                # Knowledge graph del codebase: query, aggiornamento, hub, explain.
                try:
                    from utils import graphify_bridge as _gb
                    _gp = lower_input.split(maxsplit=2)
                    if not _gb.available():
                        console.print(ui_error("[!] graphify non installato (pip install graphifyy)."))
                    elif len(_gp) == 1:
                        _gi = _gb.info()
                        console.print(f"[bold cyan]Graphify[/] v{_gi.get('installed_version')} \u00b7 "
                                      f"{_gi.get('nodes', 0)} nodi \u00b7 {_gi.get('links', 0)} edge \u00b7 "
                                      f"gen {_gi.get('mtime_iso')}")
                        console.print("[dim]uso: /graphify <domanda> | update | hubs | explain <nodo>[/dim]")
                    elif _gp[1] == 'update':
                        console.print("[dim][*] Aggiorno il knowledge graph...[/dim]")
                        _gu = _gb.update(force=True)
                        console.print(f"[dim]   ok={_gu.get('ok')} nodi {_gu.get('nodes_before')}->"
                                      f"{_gu.get('nodes_after')} in {_gu.get('duration_s')}s[/dim]")
                    elif _gp[1] == 'hubs':
                        _gh = _gb.god_nodes(top=15)
                        _ht = Table(title="Hub architetturali (graphify)")
                        for _c in ("Nodo", "Grado"):
                            _ht.add_column(_c, justify="left" if _c == "Nodo" else "right")
                        for _n in _gh.get("nodes", [])[:15]:
                            _ht.add_row(str(_n.get("label")), str(_n.get("degree")))
                        console.print(_ht)
                    elif _gp[1] == 'explain':
                        _ge = _gb.explain(_gp[2] if len(_gp) > 2 else '')
                        console.print(_ge.get("text") or _ge.get("error", ""))
                    else:
                        _gq = _gb.query(_gp[1] + ((" " + _gp[2]) if len(_gp) > 2 else ''), budget=2000)
                        console.print(_gq.get("answer") or _gq.get("error", ""))
                except Exception as _e_gr:
                    console.print(ui_error(f"[!] /graphify error: {_e_gr}"))
                continue

            if lower_input == '/repair':
                console.print("[dim][*] Avvio auto-riparazione (classica)...[/dim]")
                auto_repair()
                continue

            if lower_input == '/repair-graph':
                console.print("[dim][*] Avvio graph-based self-repair...[/dim]")
                # Auto-repair su tutti i file .py del progetto
                import glob
                py_files = glob.glob(os.path.join(BASE_DIR, '*.py'))
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

            # --- /peer: finestra figlia di chat peer-to-peer (detached) ---
            if lower_input in ("/peer", "/peer-window"):
                try:
                    from ui import peer_window as _pw
                    _info = _pw.open_window()
                    if _info.get("error"):
                        console.print(ui_error(f"[!] finestra peer: {_info['error']}"))
                    elif _info.get("already_open"):
                        console.print(f"[dim][*] finestra peer gia' aperta (pid {_info['pid']})[/dim]")
                    else:
                        console.print(f"[dim][*] finestra peer avviata (pid {_info['pid']})[/dim]")
                except Exception as _e_pw:
                    console.print(ui_error(f"[!] /peer errore: {type(_e_pw).__name__}: {_e_pw}"))
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
                        from tools.tools_scrape import scrape, _fmt
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
                    from utils.exec_logger import read_executions
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
                    from core.memory_meta import stats as meta_stats
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
                    from core.memory_meta import pin as meta_pin, unpin as meta_unpin
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
                    from core.memory_meta import tag as meta_tag
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
                    from core.memory_meta import list_events
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
                    from core.memory_meta import prune
                    _pr = prune()
                    console.print(f"[bold cyan]Prune eseguito[/]: soft_deleted={_pr['soft_deleted']} purged={_pr['purged']} pinned_immuni={_pr['pinned']}")
                    if _pr.get('snapshot'):
                        console.print(f"[dim]Snapshot: {_pr['snapshot']}[/dim]")
                except Exception as _e_pr:
                    console.print(ui_error(f"[!] /prune error: {_e_pr}"))
                continue

            if lower_input == '/audit':
                try:
                    from core.memory_meta import audit_meta
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
                        from verdict import verdict_launcher as _vl
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
                        from agents.roles import run_role
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
                        from core.memory_meta import usage_rows, usage_stats
                        _rows = usage_rows(_u_days)
                        if not _rows:
                            console.print(f"[yellow]Nessun utilizzo registrato negli ultimi {_u_days} giorni.[/]")
                        elif _u_mode == "csv":
                            import csv
                            _csv_path = os.path.join(BASE_DIR, "usage_report.csv")
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
                    # [P0-2 audit azione ciclo1] Scrittura su config.db via adapter.
                    if not set_value("current_model", get_current_model()):
                        raise RuntimeError("set_value('current_model') ha restituito False")
                except Exception as _cfg_e:
                    # [FIX G09] Errore NON silenzioso: se il salvataggio fallisce
                    # l'utente crederebbe il modello persistito quando non lo e'.
                    console.print(ui_error(f"[!] Impossibile salvare la config: {_cfg_e}"))
                    try:
                        log_error("save_config_model", _cfg_e)
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

            # [FIX P1] Reset dello stato anti-loop del gateway all'inizio di ogni
            # turno utente: la guardia deve valutare solo le chiamate del turno
            # corrente, non trascinare lo storico tra richieste indipendenti.
            try:
                gateway.reset_loop_state()
            except Exception:
                pass

            # Live Sliding Window respiratorio: finestra sinusoidale + ledger (v2)
            messages = safe_trim_context(messages)

            # [CONTESTO] Avviso di soglia: se il contesto stimato supera
            # 'context_warn_tokens' ci si ferma e si chiede se continuare,
            # cosi' l'utente puo' salvare/riepilogare prima che il trim
            # respiratorio scarti i turni piu' vecchi (0 = disattivato).
            # [CONTESTO] Soglia espressa come % della finestra del modello
            # ATTIVO in questo turno: si adatta da sola quando il routing
            # cambia modello (nessun avviso al cambio, solo se si supera la
            # percentuale della finestra corrente). Fallback: soglia fissa.
            _ctx_warn_pct = _get_context_warn_pct()
            _ctx_warn = 0
            _ctx_window = 0
            if _ctx_warn_pct:
                try:
                    from core.benchmark_data import get_models
                    _ctx_window = int(
                        (get_models().get(active_model) or {}).get("contesto") or 0
                    )
                except Exception:
                    _ctx_window = 0
                if _ctx_window > 0:
                    _ctx_warn = int(_ctx_window * _ctx_warn_pct / 100)
            if not _ctx_warn:
                _ctx_warn = _get_context_warn_tokens()
            if _ctx_warn:
                _ctx_tokens = _estimate_context_tokens(messages)
                if _ctx_tokens > _ctx_warn:
                    _ctx_pct = (
                        f"{_ctx_tokens * 100 // _ctx_window}% di {_ctx_window:,}"
                        if _ctx_window else f"soglia {_ctx_warn:,}"
                    )
                    console.print(ui_error(
                        f"[!] Contesto conversazione ampio: ~{_ctx_tokens:,} token "
                        f"stimati ({_ctx_pct}). Per non perdere il filo "
                        "posso fermarmi qui: salva un checkpoint o riepiloga prima "
                        "di proseguire."
                    ))
                    _sys_notify(
                        "Astral - Contesto ampio",
                        f"~{_ctx_tokens:,} token stimati ({_ctx_pct}): proseguo?",
                        kind="context_warn",
                    )
                    try:
                        _ctx_ans = ui_confirm("Continuo con questo contesto?")
                    except Exception:
                        _ctx_ans = "s"
                    if (_ctx_ans or "").strip().lower() != "s":
                        # Rimuove il messaggio utente appena accodato: il turno
                        # non e' stato eseguito, quindi non deve restare in
                        # cronologia (evita duplicati al prossimo invio).
                        if messages and messages[-1].get("role") == "user":
                            messages.pop()
                        console.print("[dim]Turno sospeso: nessuna chiamata al modello. "
                                      "Usa /checkpoint per salvare, poi riprendi.[/dim]")
                        continue

            if last_meta.get("event") == "consolidate":
                console.print(f"[dim][*] Respirazione: consolidamento (ciclo #{last_meta['cycle']}, "
                              f"{last_meta['facts']} fatti attivi, finestra {last_meta['window']} turni).[/dim]")

            try:
                response, used_model = call_with_dynamic_fallback(
                    messages,
                    tools_schema=gateway.select_schemas(user_input, context=messages),
                    primary_model=active_model)
            except Exception as e:
                console.print(f"[orange_red1]{escape(str(e))}[/orange_red1]")
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
            # [ANTI-LOOP] Limiti su round tool: soft (conferma utente) + hard + budget token.
            _tool_round_warn1 = _get_tool_round_limit("tool_round_warn1", _TOOL_ROUND_WARN1_DEFAULT)
            _tool_round_warn2 = _get_tool_round_limit("tool_round_warn2", _TOOL_ROUND_WARN2_DEFAULT)
            _tool_round_soft = _get_tool_round_limit("tool_round_soft_limit", _TOOL_ROUND_DEFAULT,
                                                     aliases=("max_tool_rounds",))
            _tool_round_hard = _get_tool_round_limit("tool_round_hard_limit", _TOOL_ROUND_HARD_DEFAULT)
            _tool_token_budget = _get_tool_token_budget()
            _tool_round = 0
            _tool_round_hit = False
            _tool_stop_reason = None
            _tool_tokens = _usage_total_tokens(usage)  # token del primo turno
            _soft_asked = False
            _warned_warn1 = False
            _warned_warn2 = False
            _ctx_warned_in_loop = False
            while msg.tool_calls:
                _tool_round += 1
                # Budget token cumulati nel turno (0 = illimitato).
                # [BUDGET-ASK] Al superamento della soglia NON interrompe:
                # chiede conferma all'utente; se confermato, prosegue e
                # richiede di nuovo ogni 1.2M token aggiuntivi (step).
                _TOKEN_BUDGET_STEP = 1_200_000
                if _tool_token_budget and _tool_tokens > _tool_token_budget:
                    console.print(ui_error(
                        f"[!] Budget token del turno superato ({_tool_tokens:,} > "
                        f"{_tool_token_budget:,})."
                    ))
                    _sys_notify(
                        "Astral - Serve una conferma",
                        f"Budget token superato ({_tool_tokens:,} > {_tool_token_budget:,}): "
                        "interrompo il ciclo?",
                        kind="token_budget_ask",
                    )
                    try:
                        _ans = ui_confirm("Fermo il ciclo di tool?")
                    except Exception:
                        _ans = ""
                    # [FIX] La domanda e' "Fermo il ciclo?": si interrompe SOLO su
                    # risposta affermativa esplicita (s/si/y/yes). Qualsiasi altra
                    # risposta (incluso "n") significa "continua".
                    _ans_norm = (_ans or "").strip().lower()
                    _stop_cycle = _ans_norm in ("s", "si", "sì", "y", "yes", "1")
                    _log_loop_diagnostic(user_input, used_model, _tool_round - 1,
                                         _tool_token_budget,
                                         reason="token_budget_stopped" if _stop_cycle else "token_budget_confirmed",
                                         tokens=_tool_tokens)
                    if _stop_cycle:
                        _tool_round_hit = True
                        _tool_stop_reason = "token_budget"
                        console.print(ui_error(
                            "[!] Ciclo interrotto su conferma utente. Riformula la "
                            "richiesta o suddividila in passi piu' piccoli."
                        ))
                        break
                    # Confermato: alza la soglia e ripetera' alla prossima
                    # iterazione ogni _TOKEN_BUDGET_STEP token aggiuntivi.
                    _tool_token_budget = max(_tool_token_budget, _tool_tokens) + _TOKEN_BUDGET_STEP
                    console.print(ui_error(
                        f"[!] Proseguo su conferma: prossimo controllo a "
                        f"{_tool_token_budget:,} token."
                    ))
                # Limite assoluto di sicurezza: stop senza chiedere.
                if _tool_round > _tool_round_hard:
                    _tool_round_hit = True
                    _tool_stop_reason = "hard_stop"
                    _log_loop_diagnostic(user_input, used_model, _tool_round - 1,
                                         _tool_round_hard, reason="hard_stop", tokens=_tool_tokens)
                    console.print(ui_error(
                        f"[!] Limite assoluto raggiunto ({_tool_round_hard} round di tool in un "
                        "singolo turno): interrompo il ciclo per evitare un loop. "
                        "Riformula la richiesta o suddividila in passi piu' piccoli."
                    ))
                    _sys_notify(
                        "Astral - Limite raggiunto",
                        f"Stop di sicurezza: {_tool_round_hard} round di tool in un singolo turno.",
                        kind="hard_stop",
                    )
                    break
                # Avvisi non bloccanti: il ragionamento continua.
                if _tool_round >= _tool_round_warn1 and not _warned_warn1:
                    _warned_warn1 = True
                    console.print(ui_error(
                        f"[!] Attenzione: {_tool_round} round di tool in questo turno "
                        f"({_tool_tokens:,} token finora). Il turno sta diventando lungo."
                    ))
                    _sys_notify(
                        "Astral - Avviso",
                        f"Turno lungo: {_tool_round} round di tool ({_tool_tokens:,} token).",
                        kind="warn_rounds",
                    )
                if _tool_round >= _tool_round_warn2 and not _warned_warn2:
                    _warned_warn2 = True
                    console.print(ui_error(
                        f"[!] Attenzione: {_tool_round} round di tool in questo turno "
                        f"({_tool_tokens:,} token finora). Ci stiamo avvicinando al limite "
                        "di sicurezza."
                    ))
                    _sys_notify(
                        "Astral - Avviso importante",
                        f"Prossimo al limite: {_tool_round} round di tool ({_tool_tokens:,} token).",
                        kind="warn_rounds2",
                    )
                # Soft-limit: avvisa e chiedi conferma una sola volta.
                if _tool_round > _tool_round_soft and not _soft_asked:
                    _soft_asked = True
                    console.print(ui_error(
                        f"[!] Superati {_tool_round_soft} round di tool in questo turno "
                        f"({_tool_tokens:,} token finora). Un turno cosi' lungo puo' essere "
                        "costoso o indicare un loop."
                    ))
                    _sys_notify(
                        "Astral - Serve una conferma",
                        f"Superati {_tool_round_soft} round di tool: proseguo?",
                        kind="soft_ask",
                    )
                    try:
                        _ans = ui_confirm("Proseguo con altri round di tool?")
                    except Exception:
                        _ans = ""
                    # [FIX] Accetta le varianti affermative comuni, non solo "s".
                    if (_ans or "").strip().lower() not in ("s", "si", "sì", "y", "yes", "1"):
                        _tool_round_hit = True
                        _tool_stop_reason = "soft_declined"
                        _log_loop_diagnostic(user_input, used_model, _tool_round - 1,
                                             _tool_round_soft, reason="soft_declined", tokens=_tool_tokens)
                        break
                if msg.content:
                    console.print(msg.content.strip(), style="dim", markup=False)
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
                            _sys_notify(
                                "Astral - Conferma richiesta",
                                f"Comando su percorso di sistema: {cmd[:120]}",
                                kind="confirm_sys_cmd",
                            )
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
                            _sys_notify(
                                "Astral - Conferma richiesta",
                                f"Eliminazione protetta di: {path[:120]}",
                                kind="confirm_sys_trash",
                            )
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
                    _tool_tokens += _usage_total_tokens(getattr(response, "usage", None))
                    print_telemetry(response, followup_model)
                    msg = response.choices[0].message

                    # [CONTESTO] Il contesto cresce anche DENTRO il turno (ogni
                    # round aggiunge tool call + risultati): ricontrollo la %
                    # della finestra del modello attualmente in uso. Avviso
                    # non bloccante, una sola volta per turno.
                    if not _ctx_warned_in_loop:
                        _ctx_warned_in_loop = True
                        _lw_pct = _get_context_warn_pct()
                        if _lw_pct:
                            try:
                                from core.benchmark_data import get_models
                                _lw_win = int(
                                    (get_models().get(followup_model) or {}).get("contesto") or 0
                                )
                            except Exception:
                                _lw_win = 0
                            if _lw_win > 0:
                                _lw_tokens = _estimate_context_tokens(messages)
                                if _lw_tokens > _lw_win * _lw_pct / 100:
                                    console.print(ui_error(
                                        f"[!] Contesto al {_lw_tokens * 100 // _lw_win}% "
                                        f"della finestra di {followup_model} "
                                        f"(~{_lw_tokens:,}/{_lw_win:,} token). "
                                        "Se il turno si allunga, valuta di fermarti "
                                        "e riepilogare."
                                    ))
                                    _sys_notify(
                                        "Astral - Contesto quasi pieno",
                                        f"{_lw_tokens * 100 // _lw_win}% della finestra "
                                        f"di {followup_model}: valuta di fermarti.",
                                        kind="context_warn",
                                    )
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


            if tool_cancelled:
                final_text = "Operazione annullata dall'utente."
            elif _tool_round_hit:
                if _tool_stop_reason == "token_budget":
                    final_text = (
                        f"Budget token del turno superato ({_tool_tokens:,} token): ho interrotto "
                        "il ciclo per contenere i costi. Riformula o suddividi la richiesta."
                    )
                elif _tool_stop_reason == "soft_declined":
                    final_text = (
                        f"Turno interrotto dopo {_tool_round_soft} round di tool (limite "
                        "morbido) su tua scelta: riformula la richiesta o suddividila in passi "
                        "piu' piccoli."
                    )
                else:
                    final_text = (
                        f"Limite assoluto raggiunto ({_tool_round_hard} round di tool in un "
                        "singolo turno): ho interrotto il ciclo per evitare un loop infinito. "
                        "Riformula la richiesta o suddividila in passi piu' piccoli."
                    )
            else:
                final_text = (msg.content or "").strip()
            if not final_text:
                # P0 audit: il ragionamento interno NON va mai mostrato come
                # risposta. Il vecchio fallback (<600 char) esponeva frammenti
                # di catena-di-pensieri all'utente. Ora si ignora del tutto e
                # si lascia che il messaggio neutro sotto gestisca il caso.
                pass
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
            _consec_errors = 0  # turno completato: azzera il contatore errori
            # Turno concluso: la palla torna all'utente. Se sta guardando la
            # console non viene disturbato (gate di focus in sys_notify).
            _notify_turn_done(messages, time.time() - _turn_t0)
            _busy = False

        except KeyboardInterrupt:
            if _busy:
                # Ctrl+C durante l'elaborazione: annulla SOLO il turno corrente,
                # senza uscire dal processo. Scarta i messaggi orfani del turno.
                del messages[_turn_start_idx:]
                save_persistent_history(messages)
                console.print("\n[dim][*] Turno annullato (Ctrl+C). Richiesta scartata, Astral resta attivo.[/dim]")
                _busy = False
                _consec_errors = 0
                continue
            checkpoint_save(messages)
            save_persistent_history(messages)
            console.print("\n[dim]Uscita in corso... Storico salvato.[/dim]")
            break
        except Exception as e:
            log_error("main_loop", e)
            _consec_errors += 1
            console.print(ui_error(f"[!] Errore: {e}"))
            # Notifica di sistema: l'utente puo' essere altrove. Per i timeout
            # del provider il testo generico non aiuta, quindi lo si qualifica.
            try:
                _code = getattr(e, "code", "")
            except Exception:
                _code = ""
            if _code == "TIMEOUT":
                _sys_notify(
                    "Astral - Timeout del provider",
                    f"Nessuna risposta dal modello entro il tempo massimo. {e}",
                    kind="err_timeout",
                )
            else:
                _sys_notify(
                    "Astral - Errore",
                    f"{_code or type(e).__name__}: {e}",
                    kind="err_%s" % (_code or "generic"),
                )
            if _consec_errors >= _MAX_CONSEC_ERRORS:
                # Circuit breaker: errore sistemico ripetuto. Interrompo invece di
                # riprovare all'infinito (era la causa di loop a CPU piena nel log).
                console.print(ui_error(
                    f"[!] {_consec_errors} errori consecutivi: interrompo la sessione "
                    "per evitare un loop. Controlla error_log.txt e riavvia."
                ))
                _sys_notify(
                    "Astral - Sessione interrotta",
                    f"{_consec_errors} errori consecutivi: sessione fermata per evitare un loop.",
                    kind="circuit_breaker",
                )
                break
            started = launch_self_repair()
            if started:
                console.print("[dim]Autoriparazione avviata in background; la sessione resta attiva.[/dim]")
