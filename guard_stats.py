# -*- coding: utf-8 -*-
"""guard_stats.py - Aggrega gli scatti della guardia anti-degenerazione per modello.

Legge logs/loop_diagnostics.jsonl (event=guard_triggered) e mostra:
- quante volte la guardia e' scattata per ciascun modello
- quali cause sono piu' frequenti (loop ripetitivo / reasoning inline)
Uso: py guard_stats.py
"""
import json
import os
from collections import Counter, defaultdict

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
LOG = os.path.join(BASE_DIR, "logs", "loop_diagnostics.jsonl")


def main():
    if not os.path.exists(LOG):
        print("Nessun log trovato:", LOG)
        return
    per_model = Counter()
    reasons_by_model = defaultdict(Counter)
    fields_by_model = defaultdict(Counter)
    total = 0
    with open(LOG, encoding="utf-8") as fh:
        for line in fh:
            line = line.strip()
            if not line:
                continue
            try:
                e = json.loads(line)
            except Exception:
                continue
            if e.get("event") != "guard_triggered":
                continue
            total += 1
            m = e.get("model", "?")
            per_model[m] += 1
            for r in e.get("reasons", []):
                reasons_by_model[m][r] += 1
            fields_by_model[m][e.get("field", "?")] += 1

    if total == 0:
        print("Nessuno scatto della guardia registrato finora.")
        return

    print(f"Totale scatti guardia: {total}\n")
    print(f"{'modello':<40} {'scatti':>7}  cause")
    print("-" * 80)
    for m, n in per_model.most_common():
        causes = ", ".join(f"{r}={c}" for r, c in reasons_by_model[m].most_common())
        print(f"{m:<40} {n:>7}  {causes}")


if __name__ == "__main__":
    main()
