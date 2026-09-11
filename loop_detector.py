# -*- coding: utf-8 -*-
# Modulo estratto da astral.py - modularizzazione fase 1 (12/09/2026)

from memory_store import _jaccard, _word_set

LOOP_SIMILARITY_THRESHOLD = 0.75
LOOP_MIN_REPEATS = 3
class LoopDetector:
    """Rileva loop: messaggi utente molto simili ripetuti senza progresso (Jaccard)."""
    def __init__(self, threshold=LOOP_SIMILARITY_THRESHOLD, min_repeats=LOOP_MIN_REPEATS, maxlen=12):
        self.threshold = threshold
        self.min_repeats = min_repeats
        self.maxlen = maxlen
        self.history = []
        self.strike = 0

    def check(self, text):
        """Aggiorna lo stato e ritorna True quando rileva un loop."""
        ws = _word_set(text)
        if not ws:
            return False
        sim_prev = _jaccard(ws, self.history[-1]) if self.history else 0.0
        self.history.append(ws)
        if len(self.history) > self.maxlen:
            self.history.pop(0)
        self.strike = self.strike + 1 if sim_prev >= self.threshold else 0
        return self.strike >= self.min_repeats - 1
