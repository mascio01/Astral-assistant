# -*- coding: utf-8 -*-
"""Input riga utente con supporto al paste multi-riga (bracketed paste).

Problema risolto
----------------
``rich.console.Console.input()`` legge UNA riga per volta tramite ``input()``:
quando si incolla un testo multi-riga il terminale lo consegna riga per riga,
ogni INVIO diventa un comando separato e Windows Terminal mostra il popup di
conferma "stai incollando piu' righe".

Soluzione
---------
Si usa una ``PromptSession`` di prompt_toolkit con **bracketed paste** attivo:
il testo incollato entra nel buffer come UN unico input (i newline non fanno
scattare l'invio) e viene poi ridotto a una sola riga stile LingoJam:
``\\r\\n`` / ``\\r`` / ``\\n`` -> spazio singolo, mentre spazi multipli e tab
restano intatti.

Fallback: se prompt_toolkit non e' disponibile o non c'e' una TTY si torna a
``console.input()`` (comportamento storico).
"""

from __future__ import annotations

import re

from rich.markup import escape

from ui.components import console

# Sessione riusata tra un turno e l'altro (evita re-init a ogni prompt).
_session = None
_session_ok = None  # None = non ancora provata, True/False = esito

_NEWLINE_RE = re.compile(r"\r\n|\r|\n")

# Byte che il terminale invia quando si preme Ctrl+Backspace.
# - Console host nativo Windows: Backspace = "\x08", Ctrl+Backspace = "\x7f"
# - Input VT100 (Windows Terminal): convenzione invertita
#   (Backspace = "\x7f", Ctrl+Backspace = "\x08").
# Va deciso a runtime, altrimenti il Backspace semplice cancellerebbe
# una parola intera.
_WORD_DELETE_BYTE = "\x7f"


def _detect_word_delete_byte():
    """Ritorna il byte che identifica Ctrl+Backspace su questo terminale."""
    try:
        from prompt_toolkit.input.win32 import _is_win_vt100_input_enabled

        return "\x08" if _is_win_vt100_input_enabled() else "\x7f"
    except Exception:
        return "\x7f"


def to_one_line(text):
    """Riduce un testo multi-riga a una sola riga stile LingoJam.

    Ogni sequenza di a capo diventa UNO spazio; spazi multipli e tab sono
    preservati cosi' come sono.
    """
    if not text:
        return ""
    return _NEWLINE_RE.sub(" ", text)


def _markup_to_ansi(markup):
    """Converte markup Rich in sequenza ANSI (per colorare il prompt)."""
    try:
        from io import StringIO

        from rich.console import Console as _RichConsole

        buf = StringIO()
        tmp = _RichConsole(
            file=buf,
            force_terminal=True,
            color_system="truecolor",
            width=400,
            legacy_windows=False,
        )
        tmp.print(markup, end="")
        return buf.getvalue()
    except Exception:
        return re.sub(r"\[/?[^\]]*\]", "", markup or "")


def _build_key_bindings():
    """KeyBindings con Ctrl+Backspace = cancella parola.

    Nota: i nomi chiave vanno presi da ``prompt_toolkit.keys.Keys``.
    Stringhe come ``"c-backspace"`` NON esistono e sollevano ValueError:
    un binding invalido qui faceva fallire l'intera sessione e disattivava
    in cascata il bracketed paste (regressione paste multi-riga).
    """
    from prompt_toolkit.key_binding import KeyBindings
    from prompt_toolkit.keys import Keys

    kb = KeyBindings()
    word_byte = _detect_word_delete_byte()

    @kb.add(Keys.ControlH)
    def _backspace(event):
        """Backspace = 1 carattere; Ctrl+Backspace = parola intera."""
        buffer = event.current_buffer
        if event.data == word_byte:
            start = buffer.document.find_start_of_previous_word()
            if start:
                buffer.delete_before_cursor(count=-start)
            return
        buffer.delete_before_cursor(count=1)

    @kb.add(Keys.ControlW)
    def _delete_word_before(event):
        """Ctrl+W: cancella la parola prima del cursore (stile shell)."""
        buffer = event.current_buffer
        start = buffer.document.find_start_of_previous_word()
        if start:
            buffer.delete_before_cursor(count=-start)

    return kb


def _get_session():
    """Crea lazy la PromptSession con bracketed paste attivo."""
    global _session, _session_ok
    if _session_ok is False:
        return None
    if _session is None:
        try:
            from prompt_toolkit import PromptSession

            # I key bindings sono opzionali: se qualcosa va storto si crea
            # comunque la sessione, cosi' il bracketed paste resta attivo.
            try:
                kb = _build_key_bindings()
            except Exception:
                kb = None

            # NB: il bracketed paste e' attivo di default nel renderer di
            # prompt_toolkit (Vt100_Output.enable_bracketed_paste): cosi' il
            # terminale NON mostra il popup "stai incollando piu' righe" e il
            # testo incollato entra nel buffer come input unico.
            _session = PromptSession(multiline=False, key_bindings=kb)
            _session_ok = True
        except Exception:
            _session_ok = False
            return None
    return _session


def read_user_input(prompt_markup=""):
    """Legge una riga di input gestendo correttamente il paste multi-riga.

    Ritorna il testo gia' convertito in una sola riga (senza strip finale dei
    soli a capo). Solleva ``KeyboardInterrupt`` / ``EOFError`` come ``input()``.
    """
    session = _get_session()
    if session is None:
        raw = console.input(prompt_markup + " " if prompt_markup else "")
        return to_one_line(raw).strip()

    try:
        from prompt_toolkit.formatted_text import ANSI

        message = ANSI(_markup_to_ansi(prompt_markup)) if prompt_markup else ""
        raw = session.prompt(message=message)
    except (KeyboardInterrupt, EOFError):
        raise
    except Exception:
        # Terminale non interattivo o sessione rotta: fallback classico.
        raw = console.input(prompt_markup + " " if prompt_markup else "")
    return to_one_line(raw).strip()


__all__ = ["read_user_input", "to_one_line"]
