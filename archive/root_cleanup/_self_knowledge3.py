import sqlite3, json

conn = sqlite3.connect('telemetry.db')
c = conn.cursor()

with open('verdict/verdict.py', 'r', encoding='utf-8') as f:
    src = f.read()

import re
m = re.search(r'GIUDICI\s*=\s*\[(.+?)\]', src, re.DOTALL)
if m:
    giudici_block = m.group(1).strip()
    models_in_block = re.findall(r'"([^"]+)"', giudici_block)
else:
    models_in_block = []

print("=" * 60)
print("VERITA' DAL SORGENTE")
print("=" * 60)
print(f"GIUDICI dichiarati ({len(models_in_block)}):")
for g in models_in_block:
    print(f"  -> {g}")

v_min, v_max = 1789259144, 1789268610

print(f"\n{'=' * 60}")
print("CHIAMATE NELLA FINESTRA VERDETTO (436 totali)")
print("=" * 60)
c.execute("SELECT model_name, COUNT(*), SUM(total_tokens) FROM telemetry WHERE ts >= ? AND ts <= ? GROUP BY model_name ORDER BY COUNT(*) DESC", (v_min, v_max))
for r in c.fetchall():
    tag = "GIUDICE" if r[0] in models_in_block else "ALTRO"
    print(f"  [{tag:7s}] {r[0]:35s} {r[1]:3d} chiamate, {r[2]:>8,} tok")

print(f"\n{'=' * 60}")
print("COSTO REALE DEL VERDETTO (solo 3 giudici)")
print("=" * 60)
prices = {
    "deepseek/deepseek-v4-pro": 2.00,
    "z-ai/glm-5.3": 1.50,
    "openai/gpt-5.6-sol": 5.00,
}
totale = 0
for model in models_in_block:
    c.execute("SELECT COUNT(*), SUM(total_tokens) FROM telemetry WHERE ts >= ? AND ts <= ? AND model_name = ?", (v_min, v_max, model))
    cnt, tok = c.fetchone()
    cnt = cnt or 0
    tok = tok or 0
    cost = tok / 1_000_000 * prices.get(model, 1.0)
    totale += cost
    print(f"  {model}: {cnt} chiamate, {tok:,} tok, ${cost:.4f}")

print(f"\n  TOTALE REALE (soli 3 giudici): ${totale:.4f}")

# Costo non-verdetto nella stessa finestra
print(f"\n{'=' * 60}")
print("COSTO NON-VERDETTO (stessa finestra)")
print("=" * 60)
c.execute("SELECT model_name, COUNT(*), SUM(total_tokens) FROM telemetry WHERE ts >= ? AND ts <= ? AND model_name NOT IN (" + ",".join("?" for _ in models_in_block) + ") GROUP BY model_name ORDER BY COUNT(*) DESC", (v_min, v_max, *models_in_block))
non_tok = 0
for r in c.fetchall():
    pri = 0.45 if "flash" in r[0] else 2.50
    cost = r[2] / 1_000_000 * pri
    non_tok += r[2]
    print(f"  {r[0]:35s} {r[1]:3d} chiamate, {r[2]:>8,} tok")

# Totale giudici tok
c.execute("SELECT SUM(total_tokens) FROM telemetry WHERE ts >= ? AND ts <= ? AND model_name IN (" + ",".join("?" for _ in models_in_block) + ")", (v_min, v_max, *models_in_block))
giudici_tok = c.fetchone()[0] or 0

print(f"\n  RIEPILOGO FINESTRA VERDETTO:")
print(f"    Token giudici:     {giudici_tok:>8,} ({(giudici_tok/(giudici_tok+non_tok)*100):.1f}%)")
print(f"    Token non-giudici: {non_tok:>8,} ({(non_tok/(giudici_tok+non_tok)*100):.1f}%)")
print(f"    TOTALE:            {giudici_tok+non_tok:>8,}")

# Globali
c.execute("SELECT SUM(total_tokens) FROM telemetry")
globale = c.fetchone()[0] or 0

c.execute("SELECT SUM(total_tokens) FROM telemetry WHERE ts >= ? AND ts <= ?", (v_min, v_max))
finestra = c.fetchone()[0] or 0

print(f"\n{'=' * 60}")
print("SPROPORZIONE REALE (solo 3 giudici vs resto)")
print("=" * 60)
print(f"  Globale token: {globale:,}")
print(f"  Finestra verdetto: {finestra:,} ({finestra/globale*100:.1f}%)")
print(f"  Di cui giudici: {giudici_tok:,} ({giudici_tok/globale*100:.1f}% del globale)")
print(f"  Di cui non-giudici: {non_tok:,} ({non_tok/globale*100:.1f}% del globale)")
print(f"\n  -> Il verdetto REALE (3 giudici) consuma {giudici_tok/globale*100:.1f}% di tutti i token in {5.5}% del tempo")
print(f"  -> Il non-verdetto nella finestra e' {non_tok/globale*100:.1f}% del globale (traffico normale che capita nel mentre)")

conn.close()
