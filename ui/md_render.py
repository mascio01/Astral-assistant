"""Rendering Markdown con tabelle auto-dimensionate per la console Rich.

Problema risolto: il renderer Markdown nativo di Rich NON calcola le
larghezze ottimali delle colonne di una tabella. Con celle lunghe la
riga "sborda" e il terminale la spezza a capriccio, rendendo il testo
illeggibile.

Soluzione: si isolano i blocchi tabella Markdown (righe che iniziano e
finiscono con '|', con la riga separatrice '---') e si renderizzano con
rich.Table, che distribuisce la larghezza in modo intelligente e manda
a capo il contenuto DENTRO la cella. Il resto del testo resta al
renderer Markdown standard.

Uso:
    from md_render import print_markdown
    print_markdown(console, final_text)
"""

import re

from rich.markdown import Markdown
from rich.table import Table
from rich import box


_SEP_CELL = re.compile(r"^:?-{2,}:?$")


def _split_row(line):
    """Divide una riga di tabella Markdown nelle sue celle."""
    s = line.strip()
    if s.startswith("|"):
        s = s[1:]
    if s.endswith("|"):
        s = s[:-1]
    return [c.strip() for c in s.split("|")]


def _is_separator(line):
    """True se la riga e' la separatrice '|---|---|' di una tabella."""
    s = line.strip()
    if "|" not in s:
        return False
    cells = _split_row(s)
    return bool(cells) and all(_SEP_CELL.match(c) for c in cells)


def _is_table_row(line):
    s = line.strip()
    return s.startswith("|") and s.endswith("|") and s.count("|") >= 2


def _extract_tables(text):
    """Ritorna (segmenti) dove ogni segmento e' ('md', str) o ('table', righe).

    Una tabella valida = riga header + separatrice + >=1 riga di dati.
    """
    lines = text.split("\n")
    segments = []
    buf = []
    i = 0
    n = len(lines)
    while i < n:
        if (
            _is_table_row(lines[i])
            and i + 1 < n
            and _is_separator(lines[i + 1])
        ):
            header = _split_row(lines[i])
            j = i + 2
            rows = []
            while j < n and _is_table_row(lines[j]):
                rows.append(_split_row(lines[j]))
                j += 1
            if rows:
                if buf:
                    segments.append(("md", "\n".join(buf)))
                    buf = []
                segments.append(("table", (header, rows)))
                i = j
                continue
        buf.append(lines[i])
        i += 1
    if buf:
        segments.append(("md", "\n".join(buf)))
    return segments


def _build_table(header, rows):
    """Costruisce una rich.Table auto-dimensionata e leggibile."""
    table = Table(
        box=box.SIMPLE_HEAVY,
        show_header=True,
        header_style="bold cyan",
        pad_edge=False,
        expand=False,
    )
    ncols = len(header)
    for col in header:
        table.add_column(col or " ", overflow="fold", no_wrap=False)
    for row in rows:
        cells = list(row[:ncols])
        while len(cells) < ncols:
            cells.append("")
        table.add_row(*cells)
    return table


def print_markdown(console, text):
    """Stampa Markdown con le tabelle renderizzate da rich.Table.

    Mai eccezioni: in caso di problemi ricade sul Markdown standard.
    """
    if not text:
        return
    try:
        segments = _extract_tables(text)
        if not any(kind == "table" for kind, _ in segments):
            console.print(Markdown(text))
            return
        for kind, payload in segments:
            if kind == "md":
                if payload.strip():
                    console.print(Markdown(payload))
            else:
                header, rows = payload
                console.print(_build_table(header, rows))
    except Exception:
        try:
            console.print(Markdown(text))
        except Exception:
            console.print(text)

