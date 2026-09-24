# -*- coding: utf-8 -*-
# ui/__init__.py - [FIX G08] Esporta l'interfaccia pubblica del package UI.
#
# Senza questo file il package non era importabile come 'ui': astral.py
# cadeva SEMPRE nel fallback rich, rendendo di fatto inutilizzati i componenti
# del design-system (ui/components.py, ui/tokens.py).
from .components import (
    banner,
    confirm,
    error,
    prompt_label,
    status,
    table,
    voice_confirm,
)
from . import tokens

__all__ = [
    "banner",
    "confirm",
    "error",
    "prompt_label",
    "status",
    "table",
    "voice_confirm",
    "tokens",
]

