# -*- coding: utf-8 -*-
# astral.py - Entry point: REPL, comandi slash, ciclo conversazione
import json
import os
import subprocess
import sys
import time

from core.core_io import (BASE_DIR, _early_log, console, install_exception_hooks,
                          launch_self_repair, log_error, set_session_color)

from rich.panel import Panel
from rich.markdown import Markdown
from rich.markup import escape
from rich.table import Table

try:
    from ui import (banner as ui_banner, prompt_label as ui_prompt_label,
                    confirm as ui_confirm, table as ui_table, error as ui_error)
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

    def ui_table(headers, rows, justify=None, **kw):
        _t = _ui_Table(**kw)
        for _i, _h in enumerate(headers):
            _t.add_column(_h, justify=(justify[_i] if justify and _i < len(justify) else "left"))
        for _r in rows:
            _t.add_row(*[str(_c) for _c in _r])
        return _t

    def ui_error(text):
        return f"[bold orange_red1]{_ui_escape(text)}[/]"

from ui.loop_detector import LoopDetector


# --- [ANTI-LOOP] Modello soft-limit + budget token sul ciclo agentico ----------
# Il ciclo `while msg.tool_calls` in main() prosegue finche' il modello emette
# tool_calls. Le guardie di tools_gateway intercettano solo la ripetizione
# esatta/near-duplicate consecutiva: una catena lunga di chiamate diverse puo'
# comunque girare a lungo bruciando token.
#
# Analisi sullo storico (logs/executions.jsonl, ~3.8k chiamate) mostra una
# distribuzione bimodale: ~86% dei turni usa <=2 round, ma il vero lavoro
# agentico multi-step arriva a decine di round (p90=48). Quindi:
#   - WARN1 (config 'tool_round_warn1', default 50): avviso rosso non bloccante.
#   - WARN2 (config 'tool_round_warn2', default 100): secondo avviso non bloccante.
#   - SOFT  (config 'tool_round_soft_limit', default 130): avviso + conferma utente.
#   - HARD  (config 'tool_round_hard_limit', default 150): stop assoluto.
#   - TOKEN (config 'tool_token_budget', default 3000000; 0=illimitato):
#     al superamento NON interrompe: chiede conferma all'utente se fermare
#     il ciclo. Se si prosegue, richiede di nuovo ogni 1.2M token extra
#     (step incrementale, vedi _TOKEN_BUDGET_STEP in core/repl_engine.py).
_TOOL_ROUND_WARN1_DEFAULT = 50
_TOOL_ROUND_WARN2_DEFAULT = 100
_TOOL_ROUND_DEFAULT = 130
_TOOL_ROUND_HARD_DEFAULT = 150
_TOOL_TOKEN_BUDGET_DEFAULT = 3_000_000
# [CONTESTO] Soglia di avviso sul contesto totale della conversazione (token
# stimati). Se superata, Astral si ferma e chiede se continuare, per non
# perdere il contesto con un trim improvviso. 0 = disattivato.
_CONTEXT_WARN_TOKENS_DEFAULT = 150_000

# [ANTI-LOOP] Circuit breaker sul loop principale: se piu' turni consecutivi
# sollevano eccezioni (errore sistemico, es. NameError prima del prompt di
# input) il loop non deve ritentare all'infinito bruciando CPU/log. Oltre
# questa soglia la sessione si chiude con un messaggio invece di girare a vuoto.
_MAX_CONSEC_ERRORS = 5


def _get_tool_round_limit(key, default, aliases=()):
    """Legge un limite di round tool dalla config (int > 0), altrimenti 'default'.
    'aliases' permette chiavi legacy (es. 'max_tool_rounds')."""
    for k in (key,) + tuple(aliases):
        try:
            raw = get_config(k, None)
            if raw is not None:
                val = int(str(raw).strip())
                if val > 0:
                    return val
        except Exception:
            pass
    return default


def _get_tool_token_budget():
    """Budget token cumulati per turno (config 'tool_token_budget', default 3M; 0=illimitato).

    [P0-2 audit azione ciclo1] Nessuna lettura diretta di config.json: la
    migrazione lazy dal file legacy e' responsabilita' di core.config_store.
    """
    return get_int("tool_token_budget", _TOOL_TOKEN_BUDGET_DEFAULT)


def _get_context_warn_tokens():
    """Soglia di avviso sul contesto totale della conversazione
    (config 'context_warn_tokens', default 150k; 0 = disattivato).

    Quando il contesto stimato supera questa soglia Astral si ferma e chiede
    se continuare, cosi' l'utente puo' salvare/riepilogare prima che il trim
    respiratorio scarti i turni piu' vecchi.
    """
    return get_int("context_warn_tokens", _CONTEXT_WARN_TOKENS_DEFAULT)


def _get_context_warn_pct():
    """Soglia di avviso sul contesto come PERCENTUALE della finestra di
    contesto del modello attivo (config 'context_warn_pct', default 85;
    0 = disattivato). Se >0 ha precedenza sulla soglia fissa in token:
    la finestra si adatta automaticamente al modello scelto dal routing.
    """
    return get_int("context_warn_pct", 85)


def _get_tool_round_cap():
    """[compat] Alias storico: ritorna il soft-limit."""
    return _get_tool_round_soft()


def _usage_total_tokens(usage):
    """Estrae i token totali da un oggetto usage (o None). Best-effort: mai solleva."""
    if usage is None:
        return 0
    try:
        total = getattr(usage, "total_tokens", None)
        if total is None and isinstance(usage, dict):
            total = usage.get("total_tokens")
        if total is None:
            pt = getattr(usage, "prompt_tokens", None)
            ct = getattr(usage, "completion_tokens", None)
            if isinstance(usage, dict):
                pt = pt if pt is not None else usage.get("prompt_tokens")
                ct = ct if ct is not None else usage.get("completion_tokens")
            total = (pt or 0) + (ct or 0)
        return int(total or 0)
    except Exception:
        return 0


def _estimate_context_tokens(messages):
    """Stima grezza (chars/4) dei token occupati dal contesto inviato al modello.

    Serve solo per l'avviso di soglia: best-effort, mai solleva. Conta i campi
    'content' testuali e gli argomenti delle tool_calls.
    """
    try:
        chars = 0
        for m in messages or []:
            if not isinstance(m, dict):
                continue
            c = m.get("content")
            if isinstance(c, str):
                chars += len(c)
            elif isinstance(c, list):
                for part in c:
                    if isinstance(part, dict):
                        chars += len(str(part.get("text", "")))
            for tc in (m.get("tool_calls") or []):
                try:
                    fn = tc.get("function") or {}
                    chars += len(str(fn.get("name", ""))) + len(str(fn.get("arguments", "")))
                except Exception:
                    pass
        return chars // 4
    except Exception:
        return 0


def _log_loop_diagnostic(user_input, used_model, rounds, cap, reason="hard_stop", tokens=0):
    """[ANTI-LOOP] Report diagnostico quando scatta soft/hard stop sul ciclo tool."""
    try:
        import datetime as _dt
        base_dir = os.path.dirname(os.path.abspath(__file__))
        log_dir = os.path.join(base_dir, "logs")
        os.makedirs(log_dir, exist_ok=True)
        entry = {
            "ts": _dt.datetime.now().isoformat(timespec="seconds"),
            "event": "tool_round_cap_hit",
            "reason": reason,
            "rounds": rounds,
            "cap": cap,
            "tokens": tokens,
            "model": used_model,
            "input": (user_input or "")[:500],
        }
        with open(os.path.join(log_dir, "loop_diagnostics.jsonl"), "a", encoding="utf-8") as fh:
            fh.write(json.dumps(entry, ensure_ascii=False) + "\n")
    except Exception:
        pass


# --- notifiche di sistema -------------------------------------------------
# [RIMOSSO] voice_feedback: dipendenza voce disattivata. No-op compatibile.
def _lights_notify(kind):
    """No-op: la voce e' stata rimossa, il hook resta per compatibilita'."""
    pass


def _sys_notify(title, message, kind="info", force=False):
    """Notifica di sistema best-effort su avvisi importanti.

    Delega a sys_notify, che decide se notificare in base al focus: se la
    console di Astral e' in primo piano non arriva nessun disturbo.
    Mai eccezioni: l'assenza di notifiche non deve rompere il REPL.

    Nota: un tempo qui veniva passato play_audio=..., parametro che NON esiste
    nella firma di sys_notify.notify(): ogni chiamata sollevava TypeError,
    catturato dal try/except, e nessun toast partiva mai da astral.py.
    """
    try:
        from ui import sys_notify
        return sys_notify.notify(title, message, kind=kind, force=force)
    except Exception:
        return False


def _plain_summary(text, cap=140):
    """Riduce una risposta a una riga leggibile dal toast.

    Toglie blocchi di codice, markup rich e markdown: nel toast serve testo
    scorrevole, non sintassi. Tronca all'ultimo spazio utile per non spezzare
    le parole. Mai eccezioni: ritorna "" in caso di problemi.
    """
    try:
        import re as _re
        t = str(text or "")
        t = _re.sub(r"```.*?```", " ", t, flags=_re.S)          # blocchi di codice
        t = _re.sub(r"\[/?[A-Za-z0-9# _.,=()\-]+\]", " ", t)   # markup rich
        t = _re.sub(r"^\s{0,3}#{1,6}\s*", "", t, flags=_re.M)   # heading markdown
        t = t.replace("**", "").replace("`", "").replace("|", " ")
        t = " ".join(t.split())
        if len(t) > cap:
            cut = t[:cap]
            sp = cut.rfind(" ")
            if sp > cap // 2:
                cut = cut[:sp]
            t = cut.rstrip(" .,;:") + "..."
        return t
    except Exception:
        return ""


def _notify_turn_done(messages, elapsed_s):
    """Toast "tocca a te" quando il turno e' concluso.

    Il gate di focus vive dentro sys_notify: se la console di Astral e' in
    primo piano non arriva nessun disturbo, quindi il toast compare solo
    quando l'utente e' davvero altrove. Il corpo riassume l'ultima risposta,
    cosi' il toast dice qualcosa di utile senza tornare sulla finestra.

    Toggle: ASTRAL_NOTIFY_TURN=0 disattiva; ASTRAL_NOTIFY_TURN_MIN_S
    (default 5) e' la durata minima del turno per cui vale la pena avvisare.
    """
    try:
        if os.environ.get("ASTRAL_NOTIFY_TURN", "1").strip().lower() in (
                "0", "false", "no", "off"):
            return False
        try:
            min_s = float(os.environ.get("ASTRAL_NOTIFY_TURN_MIN_S", "5") or 5)
        except ValueError:
            min_s = 5.0
        if elapsed_s < min_s:
            return False

        text = ""
        for msg in reversed(messages):
            if str(msg.get("role") or "") == "assistant":
                text = str(msg.get("content") or "")
                if text.strip():
                    break
        # Un turno puo' durare minuti: il toast deve variare, altrimenti il
        # debounce interno lo scarta come se fosse una ripetizione.
        summary = _plain_summary(text)
        if summary:
            summary = f"{int(elapsed_s)}s - {summary}"
        else:
            summary = f"Ragionamento completato in {int(elapsed_s)}s."
        return _sys_notify("Astral - tocca a te", summary, kind="turn_done")
    except Exception:
        return False

from core.memory_store import checkpoint_pull, checkpoint_save, get_config, init_db
from core.config_store import get_int, get_value, set_value
from core.memory_meta import bootstrap_meta, record_usage
from utils.price_map import all_prices, cost_usd
from core.history_store import (
    clear_persistent_history,
    compact_history_with_summary,
    get_history,
    maybe_offload_tool_result,
    save_persistent_history,
)
from core.astral_trim import (  # v2 breathing: finestra sinusoidale + ledger fattuale
    last_meta,
    safe_trim_context,
)
from repair_loop import auto_repair_graph  # graph-based self-repair (fase 2)
from llm_core import (
    MODEL_CODE,
    MODEL_CONVERSATION,
    MODEL_REASONING,
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
install_exception_hooks()


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
    if tokens and tokens[0].lower().strip(",;:.!") in ("lite", "--lite"):
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
    """[RIMOSSO] La voce e' stata disattivata. Ritorna il testo invariato."""
    return text


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
        from core.memory_meta import audit_meta, prune, prune_usage
        rep = {}
        rep["audit"] = audit_meta() or {}
        rep["prune"] = prune() or {}
        # [FIX G21] usage_log non era coperta da nessuna retention: la telemetria
        # cresceva indefinitamente. Potata nella stessa finestra di manutenzione.
        rep["usage"] = prune_usage() or {}
        # [FIX G20] Manutenzione esplicita anche di recall.db/checkpoints.db:
        # la loro retention operava solo sul save, restando di fatto inattiva
        # senza nuovi salvataggi.
        try:
            from core.memory_store import maintain_memory
            rep["store"] = maintain_memory() or {}
        except Exception:
            rep["store"] = {}
        os.makedirs(os.path.dirname(wm_file), exist_ok=True)
        with open(wm_file, "w", encoding="utf-8") as f:
            f.write(str(int(time.time())))
        _st = rep.get("store") or {}
        _us = rep.get("usage") or {}
        if (rep["prune"].get("purged") or rep["prune"].get("snapshot")
                or _st.get("recall") or _st.get("checkpoints")
                or _us.get("deleted_by_age") or _us.get("deleted_by_cap")):
            console.print(
                f"[dim]Manutenzione memoria: purge={rep['prune'].get('purged', 0)} "
                f"snapshot={rep['prune'].get('snapshot')} "
                f"recall={_st.get('recall', 0)} checkpoints={_st.get('checkpoints', 0)} "
                f"usage_age={_us.get('deleted_by_age', 0)} usage_cap={_us.get('deleted_by_cap', 0)}[/dim]")
    except Exception:
        pass  # la manutenzione non deve MAI bloccare il loop utente

# [P1 audit azione ciclo1] Ciclo di vita delle sessioni estratto in
# core/session_manager.py: qui restano solo gli alias usati dal loop.
from core.session_manager import (
    MAX_SESSIONS,
    SESSION_COLORS as _SESSION_COLORS,
    claim_session_slot as _claim_session_slot,
)


def main():
    # [P2 audit azione ciclo1] Il REPL vive in core/repl_engine.py.
    from core.repl_engine import run_repl
    return run_repl()


def _handle_task_command(user_input):
    """Gestisce i comandi /task* (registro task git+SQLite).

    Sottocomandi:
      /task                 elenco task
      /task-new [<id>] <titolo> crea task + branch (ID auto se omesso)
      /task-test <id>       esegue la suite e marca test_passed (verificato)
      /task-done <id>       marca integrata (verifica hash suite)
      /task-sync            riconcilia registro <-> git
    """
    try:
        from tools import task_tracker as tt
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
            # /task-new <titolo>          -> ID allocato automaticamente
            # /task-new <id> <titolo>     -> ID esplicito
            if len(parts) < 2:
                console.print(ui_error("[!] Uso: /task-new [<id>] <titolo>"))
                return
            if parts[1].isdigit() and len(parts) >= 3:
                task_id, title = int(parts[1]), parts[2]
            else:
                task_id, title = None, " ".join(parts[1:])
            res = tt.create_task(task_id, title)
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
    """Monitora CPU del processo e salva stack diagnostici, senza invocare il self-repair."""
    import threading
    import sys
    import traceback

    base_dir = os.path.dirname(os.path.abspath(__file__))
    hb_file = os.path.join(base_dir, ".astral_heartbeat.tmp")
    diagnostic_file = os.path.join(base_dir, ".astral_watchdog.log")
    last_cpu = time.process_time()
    last_t = time.time()
    spin_streak = 0
    last_report = 0.0

    def _tick():
        nonlocal last_cpu, last_t, spin_streak, last_report
        while True:
            time.sleep(5)
            now = time.time()
            cpu = time.process_time()
            delta_cpu = cpu - last_cpu
            delta_t = now - last_t
            last_cpu, last_t = cpu, now
            # Percentuale rispetto a un singolo core: puo' superare 100% se il
            # processo usa piu' thread/core; non equivale alla CPU totale Windows.
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
                last_report = 0.0
            # Alto uso CPU non dimostra da solo un loop. Acquisisci gli stack
            # dei thread e non scrivere ERROR: il repair_watchdog monitora error_log.
            if spin_streak >= 6 and now - last_report >= 60:
                last_report = now
                try:
                    frames = sys._current_frames()
                    lines = [
                        f"[{time.strftime('%Y-%m-%d %H:%M:%S')}] pid={os.getpid()} "
                        f"process_cpu={pct:.0f}% (per core; non CPU totale) "
                        f"sostenuta_per={spin_streak * 5}s; possibile carico intenso o loop."
                    ]
                    for thread in threading.enumerate():
                        lines.append(
                            f"\\n--- thread {thread.name!r} ident={thread.ident} "
                            f"daemon={thread.daemon} ---\\n"
                        )
                        frame = frames.get(thread.ident)
                        if frame is not None:
                            lines.extend(traceback.format_stack(frame))
                    lines.append("\\n")
                    with open(diagnostic_file, "a", encoding="utf-8") as f:
                        f.writelines(lines)
                except Exception:
                    pass
                console.print(
                    f"[bold orange_red1][!] WATCHDOG: CPU del processo {pct:.0f}% "
                    "per almeno 30s (misura per-core, non prova di spin-loop). "
                    "Stack salvati in .astral_watchdog.log.[/]"
                )
                _sys_notify(
                    "Astral - WATCHDOG",
                    f"CPU del processo {pct:.0f}% per almeno 30s; stack diagnostici salvati.",
                    kind="watchdog",
                )

    t = threading.Thread(target=_tick, daemon=True)
    t.start()
    return t


if __name__ == "__main__":
    _start_loop_watchdog()
    init_db()
    try:
        from tools import task_tracker as _tt
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
        from utils import selfmap

        if selfmap.is_stale():  # rigenera solo se i sorgenti sono piu' recenti della mappa
            selfmap.generate()  # indice semantico completo
        selfmap.refresh_bootstrap()  # contesto minimo derivato e verificabile
        # KM03: hash catalogo + stat fonti, rigenera solo se qualcosa e' cambiato
        selfmap.refresh_knowledge_map_if_needed()
        selfmap.start_watcher()  # sincronizzazione continua durante il runtime
    except Exception:
        pass  # mai bloccare l'avvio per la mappa
    # Knowledge graph del codebase (graphifyy): auto-rigenerazione se stale +
    # watcher in background. Mai bloccante: qualunque errore e' ignorato.
    # NB: il rebuild gira in un thread daemon, cosi' l'avvio non aspetta i
    # ~30s di re-estrazione AST. Se il grafo e' gia' fresco la stampa arriva
    # comunque subito (skipped).
    try:
        from utils import graphify_bridge as _gb

        if _gb.available():
            _gb.refresh_async(report=True)
    except Exception:
        pass  # mai bloccare l'avvio per il grafo
    # Skill ECC: vendorizzate on-demand, non versionate. Al primo avvio (cartella
    # assente) si chiede se scaricarle. Mai bloccante: qualunque errore e' ignorato.
    try:
        from skills import ecc_bootstrap as _ecc_boot

        if not _ecc_boot.is_installed():
            def _ask_ecc(text):
                try:
                    return console.input(f"[bold orange_red1]{text}[/] (s/N): ")
                except (EOFError, KeyboardInterrupt):
                    return ""

            _ecc_res = _ecc_boot.ensure_skills(ask=_ask_ecc)
            _st = _ecc_res.get("status")
            if _st == "installed":
                console.print(f"[dim][ecc] installate {_ecc_res.get('skills', 0)} skill "
                              f"e {_ecc_res.get('agents', 0)} agenti[/dim]")
            elif _st == "error":
                console.print(f"[dim][ecc] installazione non riuscita: "
                              f"{_ecc_res.get('error', '')}[/dim]")
    except Exception:
        pass  # mai bloccare l'avvio per le skill ECC
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

