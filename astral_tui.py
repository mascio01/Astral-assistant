"""Astral TUI - mockup standalone dell'interfaccia in stile Hermes.

Demo ISOLATA: non tocca astral.py ne' il resto del progetto.
Mostra tre elementi: transcript a pannelli bordati, status bar fissa in basso,
input box bordato con autocomplete dei comandi slash.

Avvio:    py astral_tui.py
Selftest: py astral_tui.py --selftest
Uscita:   Ctrl+C oppure /exit
"""
from __future__ import annotations

import io
import shutil
import sys
import threading
import time

from prompt_toolkit.application import Application
from prompt_toolkit.completion import Completer, Completion
from prompt_toolkit.formatted_text import ANSI, FormattedText
from prompt_toolkit.key_binding import KeyBindings
from prompt_toolkit.layout import (
    Float,
    FloatContainer,
    FormattedTextControl,
    HSplit,
    Layout,
    Window,
)
from prompt_toolkit.layout.dimension import Dimension
from prompt_toolkit.layout.menus import CompletionsMenu
from prompt_toolkit.styles import Style
from prompt_toolkit.widgets import Frame, TextArea
from rich import box
from rich.console import Console
from rich.markdown import Markdown
from rich.panel import Panel
from rich.table import Table
from rich.text import Text

ACCENT = "#7aa2f7"
_SPIN = "\u280b\u2819\u2839\u2838\u283c\u2834\u2826\u2827\u2807\u280f"

STYLE = Style.from_dict({
    "status": "bg:#1c1c28 #c8c8d0",
    "status.key": "bg:#1c1c28 #7aa2f7 bold",
    "status.ok": "bg:#1c1c28 #9ece6a bold",
    "status.warn": "bg:#1c1c28 #e0af68 bold",
    "frame.border": "#7aa2f7",
    "frame.label": "#7aa2f7 bold",
    "prompt": "#7aa2f7 bold",
    "completion-menu.completion": "bg:#26263a #c8c8d0",
    "completion-menu.completion.current": "bg:#7aa2f7 #1c1c28",
    "completion-menu.meta.completion": "bg:#26263a #565f89",
    "completion-menu.meta.completion.current": "bg:#7aa2f7 #1c1c28",
})

SLASH_COMMANDS = {
    "/help": "mostra i comandi disponibili",
    "/tools": "elenca i tool registrati",
    "/status": "stato di Astral: modello, token, sessione",
    "/clear": "pulisci il transcript",
    "/repair": "avvia l'auto-riparazione del codice",
    "/exit": "esci dal mockup",
}


class SlashCompleter(Completer):
    """Autocomplete dei comandi slash, stile Hermes."""

    def get_completions(self, document, complete_event):
        text = document.text_before_cursor
        if not text.startswith("/") or " " in text:
            return
        for cmd, desc in SLASH_COMMANDS.items():
            if cmd.startswith(text):
                yield Completion(cmd, start_position=-len(text),
                                 display=cmd, display_meta=desc)


class AstralTUI:
    def __init__(self, build_ui=True):
        cols = shutil.get_terminal_size((100, 30)).columns
        self.console = Console(record=True, force_terminal=True, legacy_windows=False,
                               width=max(60, cols - 2), soft_wrap=True)
        self._blocks: list[str] = []
        self._scroll = 0
        self._busy = False
        self._spin = 0
        self._phase = "idle"
        self._running = False
        self._spinner_thread = None
        self.model = "gpt-4o-mini"
        self.tokens = 0
        self.session = "mockup"
        if build_ui:
            self._build_ui()

    # ---------------------------------------------------------- costruzione UI
    def _build_ui(self):
        self.output = Window(
            FormattedTextControl(self._render_output),
            wrap_lines=True,
            height=Dimension(weight=1),
        )
        self.status = Window(FormattedTextControl(self._render_status), height=1)
        self.input = TextArea(
            prompt="\u276f ",
            multiline=False,
            completer=SlashCompleter(),
            complete_while_typing=True,
            accept_handler=self._on_accept,
        )
        self.input_frame = Frame(
            self.input,
            title=" messaggio ",
            width=Dimension(weight=1),
            height=Dimension(min=3, max=3),
        )
        root = HSplit([self.output, self.status, self.input_frame])
        body = FloatContainer(
            root,
            floats=[Float(xcursor=True, ycursor=True,
                           content=CompletionsMenu(max_height=8, scroll_offset=1))],
        )
        self.app = Application(
            layout=Layout(body, focused_element=self.input),
            key_bindings=self._keybindings(),
            style=STYLE,
            full_screen=True,
            mouse_support=True,
        )

    def _keybindings(self):
        kb = KeyBindings()

        @kb.add("c-c")
        def _(event):
            event.app.exit()

        @kb.add("c-l")
        def _(event):
            self._blocks.clear()
            self._invalidate()

        @kb.add("pageup")
        def _(event):
            self._scroll += max(1, self._rows() - 2)
            self._invalidate()

        @kb.add("pagedown")
        def _(event):
            self._scroll = max(0, self._scroll - max(1, self._rows() - 2))
            self._invalidate()

        return kb

    # ------------------------------------------------------------- rendering
    def _rows(self) -> int:
        total = shutil.get_terminal_size((100, 30)).lines
        return max(3, total - 5)

    def _all_lines(self):
        if not self._blocks:
            return [""]
        return "".join(self._blocks).split("\n")

    def _render_output(self):
        lines = self._all_lines()
        rows = self._rows()
        end = max(0, len(lines) - self._scroll)
        start = max(0, end - rows)
        return ANSI("\n".join(lines[start:end]))

    def _render_status(self):
        if self._busy:
            ch = _SPIN[self._spin % len(_SPIN)]
            phase_txt, phase_style = f"{ch} {self._phase}", "class:status.warn"
        else:
            phase_txt, phase_style = f"\u25cf {self._phase}", "class:status.ok"
        return FormattedText([
            ("class:status.key", " astral "),
            ("class:status.key", " model "), ("class:status", self.model + "  "),
            ("class:status.key", " tokens "), ("class:status", str(self.tokens) + "  "),
            ("class:status.key", " sess "), ("class:status", self.session + "  "),
            (phase_style, phase_txt + " "),
        ])

    def _invalidate(self):
        if self._running:
            try:
                self.app.invalidate()
            except Exception:
                pass

    def _emit(self, renderable):
        self.console.print(renderable)
        try:
            chunk = self.console.export_text(styles=True, clear=True)
        except Exception:
            chunk = self.console.export_text(clear=True)
        self._blocks.append(chunk)
        self._scroll = 0
        self._invalidate()

    # --------------------------------------------------------------- pannelli
    def emit_banner(self):
        art = Text()
        art.append("ASTRAL", style=f"bold {ACCENT}")
        art.append("  \u00b7  TUI mockup in stile Hermes\n", style="dim")
        art.append("status bar + input bordato + pannelli", style="dim italic")
        self._emit(Panel(art, border_style=ACCENT, box=box.ROUNDED, padding=(0, 1)))

    def emit_user(self, text):
        self._emit(Panel(Text(text, style="bold"), title="[bold]tu[/bold]",
                         border_style="bright_black", box=box.ROUNDED, padding=(0, 1)))

    def emit_assistant(self, md):
        self._emit(Panel(Markdown(md), title=f"[bold {ACCENT}]astral[/bold {ACCENT}]",
                         border_style=ACCENT, box=box.ROUNDED, padding=(0, 1)))

    def emit_tool(self, name, args, result="", ok=True):
        body = Text()
        body.append("$ ", style="bold green" if ok else "bold red")
        body.append(f"{name}", style="bold")
        if args:
            body.append(f" {args}", style="dim")
        if result:
            body.append("\n\u21b3 ", style="dim")
            body.append(result, style="dim")
        self._emit(Panel(body, title="[bold]tool[/bold]",
                         border_style="green" if ok else "red",
                         box=box.ROUNDED, padding=(0, 1)))

    def emit_help(self):
        table = Table(box=box.SIMPLE, show_header=False, padding=(0, 2))
        table.add_column(style=f"bold {ACCENT}", no_wrap=True)
        table.add_column(style="")
        for cmd, desc in SLASH_COMMANDS.items():
            table.add_row(cmd, desc)
        self._emit(Panel(table, title=f"[bold {ACCENT}]comandi[/bold {ACCENT}]",
                         border_style=ACCENT, box=box.ROUNDED, padding=(0, 1)))

    # ------------------------------------------------------------ demo turno
    def _on_accept(self, buffer):
        text = buffer.text.strip()
        buffer.text = ""
        if not text:
            return False
        if text in ("/exit", "/quit"):
            self.app.exit()
            return False
        if text == "/clear":
            self._blocks.clear()
            self._invalidate()
            return False
        if text == "/help":
            self.emit_help()
            return False
        if text == "/status":
            self.emit_assistant(f"modello `{self.model}`, {self.tokens} token, "
                                f"sessione `{self.session}`.")
            return False
        threading.Thread(target=self._demo_turn, args=(text,), daemon=True).start()
        return False

    def _set_busy(self, flag, phase):
        self._busy = flag
        self._phase = phase
        if flag:
            self._start_spinner()
        self._invalidate()

    def _start_spinner(self):
        if self._spinner_thread and self._spinner_thread.is_alive():
            return

        def run():
            while self._busy:
                self._spin += 1
                self._invalidate()
                time.sleep(0.1)

        self._spinner_thread = threading.Thread(target=run, daemon=True)
        self._spinner_thread.start()

    def _demo_turn(self, text):
        self.emit_user(text)
        self._set_busy(True, "pensa")
        time.sleep(0.9)
        self._set_busy(True, "tool")
        self.emit_tool("scan_storage", 'folder="C:\\\\Users" min_size_mb=100',
                       "3 file sopra i 100 MB trovati", True)
        time.sleep(0.9)
        self._set_busy(True, "risponde")
        self.emit_assistant(
            "Ecco cosa ho trovato:\n\n"
            "- **3 file** sopra i 100 MB\n"
            "- il piu' pesante e' `cache.db` (\u2248 1.2 GB)\n\n"
            "Vuoi che li sposti nel cestino?")
        self.tokens += 420
        self._set_busy(False, "idle")

    def run(self):
        self._running = True
        self.emit_banner()
        self.emit_assistant(
            "Ciao! Sono il **mockup** dell'interfaccia Astral in stile Hermes.\n\n"
            "Scrivi qualcosa e premi Invio, oppure digita `/` per i comandi.")
        self.app.run()


def _selftest():
    tui = AstralTUI(build_ui=False)
    # Console su buffer UTF-8: il selftest non deve dipendere dal terminale reale.
    tui.console = Console(file=io.StringIO(), record=True, force_terminal=False,
                          legacy_windows=False, width=100, soft_wrap=True)
    tui.emit_banner()
    tui.emit_user("test")
    tui.emit_tool("scan_storage", 'folder="C:\\\\"', "ok", True)
    tui.emit_assistant("smoke **test**")
    print("SELFTEST OK | blocchi=%d righe=%d righe_viewport=%d"
          % (len(tui._blocks), len(tui._all_lines()), tui._rows()))


def main():
    try:
        AstralTUI().run()
    except KeyboardInterrupt:
        pass


if __name__ == "__main__":
    if "--selftest" in sys.argv:
        _selftest()
    else:
        main()
