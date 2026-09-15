# -*- coding: utf-8 -*-
"""voice_feedback.py - Indicatore di stato vocale in console.

Casella di testo colorata vicino ai messaggi vocali che cambia colore
secondo lo stato corrente:
  listen -> ciano   (microfono attivo, "IN ASCOLTO")
  ok     -> verde   (trascrizione pronta)
  error  -> rosso   (nessun discorso rilevato)
  lavoro -> giallo  (elaborazione in corso)

Solo output a schermo: nessuna lampadina, nessuna rete,
zero dipendenze dure (rich opzionale con fallback ANSI).
"""
import os

try:
    from rich.console import Console
    from rich.panel import Panel
    from rich.text import Text
    _RICH = True
except Exception:
    _RICH = False

console = Console() if _RICH else None

# stato -> (etichetta, stile rich, codice ANSI, glifo)
STATES = {
    "listen": ("IN ASCOLTO ...", "bold cyan", "96", "o"),
    "ok": ("TRASCRIZIONE PRONTA", "bold green", "92", "+"),
    "error": ("NESSUN DISCORSO", "bold red", "91", "x"),
    "lavoro": ("AL LAVORO ...", "bold yellow", "93", "~"),
}
_BOX_W = 28


def notify(kind, label=None):
    """Stampa la casella di stato colorata. Best-effort: mai eccezioni."""
    try:
        s_label, style, ansi, glyph = STATES.get(kind, STATES["lavoro"])
        title = "%s %s" % (glyph, label or s_label)
        if _RICH:
            console.print(Panel(
                Text(title, justify="center", style=style),
                border_style=style, width=_BOX_W, padding=(0, 0)))
        else:
            inner = _BOX_W - 2
            pad = max(0, inner - len(title))
            left = pad // 2
            print("+" + "-" * inner + "+")
            print("|" + " " * left + "\033[%sm%s\033[0m" % (ansi, title) + " " * (pad - left) + "|")
            print("+" + "-" * inner + "+")
    except Exception:
        pass


if __name__ == "__main__":
    import time
    for k in ("listen", "lavoro", "ok", "error"):
        notify(k)
        time.sleep(0.6)
