import sqlite3, json

conn = sqlite3.connect('telemetry.db')
c = conn.cursor()

# LEGGO I GIUDICI DAL SORGENTE CORRETTAMENTE
with open('verdict/verdict.py', 'r', encoding='utf-8') as f:
    src = f.read()

import re
m = re.search(r'GIUDICI\s*=\s*\[(.+?)\]', src, re.DOTALL)
if m:
    giudici_block = m.group(1).strip()
    # Estrai i modelli: la regex trova tutte le stringhe tra virgolette
    models_in_block = re.findall(r'"([^"]+)"', giudici_block)
else:
    models_in_block = []

print("=" * 60)
print("VERITA' DAL SORGENTE")
print("=" * 60)
print(f"GIUDICI dichiarati ({len(models_in_block)}):")
for g in models_in_block:
    print(f"  ✓ {g}")

# Finestra verdetto
v_min, v_max = 1789259144, 1789268610

print(f"\n{'=' * 60}")
print("CHIAMATE NELLA FINESTRA VERDETTO (436 totali)")
print("=" * 60)
c.execute("SELECT model_name, COUNT(*), SUM(total_tokens) FROM telemetry WHERE ts >= ? AND ts <= ? GROUP BY model_name ORDER BY COUNT(*) DESC", (v_min, v_max))
for r in c.fetchall():
    is_giudice = "★★ GIUDICE" if r[0] in models_in_block else "  (ALTRO)"
    print(f"  {is_giudice}: {r[0]:35s} {r[1]:3d} chiamate, {r[2]:>8,} tok")

# SEPARAZIONE
print(f"\n{'=' * 60}")
print("VERO COSTO DEL VERDETTO (solo giudici)")
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

print(f"\n  TOTALE REALE (soli giudici): ${totale:.4f}")

# COSTO FINESTRA TOTALE meno giudici
print(f"\n{'=' * 60}")
print("COSTO NON-VERDETTO (stessa finestra, stesse conversazioni)")
print("=" * 60)
c.execute("SELECT model_name, COUNT(*), SUM(total_tokens) FROM telemetry WHERE ts >= ? AND ts <= ? AND model_name NOT IN (?,?,?) GROUP BY model_name ORDER BY COUNT(*) DESC", (v_min, v_max, *models_in_block))
non_tok = 0
for r in c.fetchall():
    pri = 0.45 if "flash" in r[0] else 2.50
    cost = r[2] / 1_000_000 * pri
    non_tok += r[2]
    print(f"  {r[0]:35s} {r[1]:3d} chiamate, {r[2]:>8,} tok, ~${cost:.4f}")

print(f"\n  SUBTOTALE NON-VERDETTO: {non_tok:,} tok")
print(f"  RAPPORTO: {non_tok/(non_tok+sum(r[2] for _,_,r[2] in [c.execute('SELECT model_name, COUNT(*), SUM(total_tokens) FROM telemetry WHERE ts >= ? AND ts <= ? AND model_name IN (?,?,?)', (v_min, v_max, *models_in_block)).fetchone()]))*100:.1f}% non-verdetto")

conn.close()
