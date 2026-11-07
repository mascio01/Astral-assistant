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
