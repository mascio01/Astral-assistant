# -*- coding: utf-8 -*-
# Modulo estratto da astral.py - modularizzazione fase 1 (12/09/2026)

from core.memory_store import _jaccard, _word_set

STOPWORDS = {'il', 'la', 'i', 'le', 'gli', 'un', 'una', 'di', 'a', 'da', 'in', 'con', 'su', 'per', 'tra', 'fra', 'ho', 'hai', 'ha', 'abbiamo', 'avete', 'hanno', 'sono', 'sei', 'è', 'siamo', 'siete', 'che', 'cui', 'non', 'più', 'anche', 'tutto', 'tutta', 'tutti', 'tutte', 'molto', 'poco', 'altro', 'stesso', 'così', 'come', 'dove', 'quando', 'perché', 'se', 'ma', 'o', 'e', 'nel', 'nello', 'nella', 'nei', 'nelle', 'sul', 'sullo', 'sulla', 'sui', 'sulle', 'al', 'allo', 'alla', 'ai', 'alle', 'dal', 'dallo', 'dalla', 'dai', 'dalle', 'col', 'coi', 'lo', 'mi', 'ti', 'ci', 'vi', 'si', 'ne', 'del', 'dei', 'degli', 'delle', 'questo', 'questa', 'questi', 'queste'}

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
        # Filtra stopwords per evitare falsi positivi su frasi generiche
        filtered_ws = ws - STOPWORDS
        # Fallback: se restano troppe poche parole chiave, usa l'originale
        if len(filtered_ws) < 2:
            filtered_ws = ws
        if not filtered_ws:
            return False
            
        # Confronta con TUTTA la finestra, non solo l'ultimo messaggio
        similarities = [_jaccard(filtered_ws, h) for h in self.history]
        max_sim = max(similarities) if similarities else 0.0
        
        self.history.append(filtered_ws)
        if len(self.history) > self.maxlen:
            self.history.pop(0)
            
        if max_sim >= self.threshold:
            self.strike += 1
        else:
            self.strike = 0
            
        return self.strike >= self.min_repeats - 1
