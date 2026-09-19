"""Design token centralizzati per la UI di Astral (design-system first).

Fonte unica per colori, tipografia, spaziature e stati dei componenti.
La migrazione progressiva sostituira'' i markup inline di astral.py con questi token.
"""

# --- Palette colori ---
COLORS = {
    # Brand / primari
    "primary": "bright_cyan",
    "accent": "gold1",
    "success": "green",
    "warning": "yellow",
    "danger": "orange_red1",
    "info": "dim",
    # Neutri
    "text": "white",
    "muted": "dim",
    "border": "bright_cyan",
    # Slot sessione (ciclo colori prompt)
    "session": ["bright_blue", "spring_green1", "magenta", "deep_sky_blue1", "gold1", "bright_magenta"],
}

# --- Tipografia ---
TYPO = {
    "banner_title": "bold",
    "prompt_label": "bold",
    "prompt_caret": "bold spring_green1",
    "status": "dim",
    "error": "bold orange_red1",
    "success_msg": "green",
}

# --- Spaziatura / layout ---
SPACING = {
    "banner_padding": (1, 2),
    "section_gap": 1,
    "indent": 2,
}

# --- Stati dei componenti ---
STATES = {
    "idle": "dim",
    "running": "bold",
    "ok": "green",
    "warn": "yellow",
    "error": "orange_red1",
}
