# -*- coding: utf-8 -*-
# verdict/schemas.py - Modelli di dominio del verdetto (dataclass, zero dipendenze esterne)
from dataclasses import dataclass, field


@dataclass
class Quesito:
    """Domanda sottoposta al consiglio dei giudici."""
    testo: str


@dataclass
class Parere:
    """Risposta di un singolo giudice. `codice` e' l'identita' ANONIMA
    esposta all'utente; `modello` resta riservato al mapping finale."""
    codice: str          # es. "Giudice Delta"
    modello: str         # id reale OpenRouter (mai mostrato prima della fine)
    temp: float          # temperatura usata (<= 0.5)
    risposta: str = ""
    errore: str = ""     # se non vuoto, il giudice ha fallito

    @property
    def ok(self) -> bool:
        return not self.errore and bool(self.risposta.strip())


@dataclass
class VerdettoFinale:
    """Struttura del verdetto: sintesi anonima + mapping identita' a fine output."""
    quesito: str
    pareri: list = field(default_factory=list)   # list[Parere]
    sintesi: str = ""                            # markdown del verdetto sintetizzato
    modello_sintetizzatore: str = ""
    mapping: list = field(default_factory=list)  # list[(codice, modello, temp)]
    ponderazione: list = field(default_factory=list)  # list[{codice, modello, peso}]