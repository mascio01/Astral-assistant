"""Componenti base della UI di Astral (design-system first).

Ogni componente accetta uno stato (idle/running/ok/warn/error) e usa i token
centralizzati. Compatibile con la console Rich gia'' usata da astral.py.
"""

from rich.console import Console
from rich.panel import Panel
from rich.markup import escape
from rich.table import Table

from ui.tokens import COLORS, TYPO, SPACING, STATES

console = Console()


_voice_prompt_session = None
_voice_prompt_toolkit_ok = None


def _get_voice_prompt_session():
    """Crea lazy la sessione prompt_toolkit, senza imporre una TTY all'import."""
    global _voice_prompt_session, _voice_prompt_toolkit_ok
    if _voice_prompt_toolkit_ok is False:
        return None
    if _voice_prompt_session is None:
        try:
            from prompt_toolkit import PromptSession
            _voice_prompt_session = PromptSession()
            _voice_prompt_toolkit_ok = True
        except Exception:
            _voice_prompt_toolkit_ok = False
    return _voice_prompt_session


def voice_confirm(text, timeout=1.5):
    """Conferma della trascrizione vocale con INVIO/ESC/auto-invio.

    Restituisce il testo confermato, ``""`` se annullato e usa il prompt
    classico come fallback quando prompt_toolkit non e' disponibile.
    """
    session = _get_voice_prompt_session()
    if session is None:
        try:
            answer = console.input(
                "[bold orange_red1]Invio la trascrizione? "
                "(INVIO=s / n=modifica): [/]")
            return text if answer.strip().lower() != "n" else console.input(
                "Testo corretto: ").strip()
        except (EOFError, KeyboardInterrupt):
            return ""

    import asyncio
    from prompt_toolkit.key_binding import KeyBindings

    async def _run():
        bindings = KeyBindings()

        @bindings.add("escape")
        def _esc(event):
            event.app.exit(exception=KeyboardInterrupt)

        prompt_task = asyncio.ensure_future(session.prompt_async(
            "Astral (Voce) > ", default=text, key_bindings=bindings))
        if not timeout:
            return await prompt_task
        timer_task = asyncio.ensure_future(asyncio.sleep(timeout))
        done, _pending = await asyncio.wait(
            {prompt_task, timer_task}, return_when=asyncio.FIRST_COMPLETED)
        if timer_task in done and prompt_task not in done:
            if (session.default_buffer.text or "").strip() == text.strip():
                prompt_task.cancel()
                try:
                    await prompt_task
                except asyncio.CancelledError:
                    pass
                return "__AUTO__"
            timer_task.cancel()
        else:
            timer_task.cancel()
        try:
            return (await prompt_task or "").strip()
        except (KeyboardInterrupt, EOFError, asyncio.CancelledError):
            return ""

    try:
        result = asyncio.run(_run())
    except (EOFError, KeyboardInterrupt):
        return ""
    except Exception:
        # Un terminale non interattivo puo' fallire dopo l'inizializzazione.
        try:
            return console.input("Testo corretto (INVIO per confermare): ").strip() or text
        except (EOFError, KeyboardInterrupt):
            return ""
    return text if result == "__AUTO__" else result


def banner(title=None, subtitle=None, border_style=None, body=None):
    """Banner di avvio (sostituisce il Panel inline di main()).

    Se body e' None, costruisce il corpo da title/subtitle (testo semplice,
    markup escapato). Se body e' fornito, lo usa cosi' com'e' (markup rich
    gia' pronto, come il banner ricco di main()).
    """
    if body is None:
        body = f"[{TYPO['banner_title']}]{escape(title or '')}[/]"
        if subtitle:
            body += f"\n[dim]{escape(subtitle)}[/]"
    return Panel(
        body,
        title=title,
        subtitle=subtitle,
        border_style=border_style or COLORS["border"],
        padding=SPACING["banner_padding"],
    )


def status(text, state="idle"):
    """Messaggio di stato colorato in base allo stato del componente."""
    return f"[{STATES.get(state, 'dim')}]{escape(text)}[/]"


def error(text):
    """Messaggio di errore standard."""
    return f"[{TYPO['error']}]{escape(text)}[/]"


def prompt_label(label, color=None):
    """Etichetta prompt con caret (es. 'Astral (Auto) >')."""
    return f"[bold {color or COLORS['primary']}]{escape(label)}[/] [bold spring_green1]>[/]"


def confirm(text):
    """Conferma s/N (sostituisce i console.input di sicurezza)."""
    return console.input(f"[{TYPO['error']}]{escape(text)}[/] (s/N): ")


def table(headers, rows, justify=None, **kw):
    """Tabella dati (sostituisce i Table costruiti inline).

    justify: lista opzionale di allineamenti per colonna ("left"/"right"/"center").
    """
    t = Table(**kw)
    for i, h in enumerate(headers):
        t.add_column(h, justify=(justify[i] if justify and i < len(justify) else "left"))
    for r in rows:
        t.add_row(*[str(c) for c in r])
    return t
