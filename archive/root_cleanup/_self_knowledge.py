import sqlite3, json, os

conn = sqlite3.connect('telemetry.db')
c = conn.cursor()

# 1. LEGGO I GIUDICI DAL SORGENTE
verdict_path = 'verdict/verdict.py'
with open(verdict_path, 'r', encoding='utf-8') as f:
    src = f.read()

# Estrai GIUDICI
import re
m = re.search(r'GIUDICI\s*=\s*\[([^\]]+)\]', src, re.DOTALL)
if m:
    giudici_block = m.group(1)
    giudici = re.findall(r'"([^"]+)"', giudici_block)
    # prendi solo i modelli (primo di ogni coppia)
    giudici_modelli = [giudici[i] for i in range(0, len(giudici), 2)]
else:
    giudici_modelli = []

print(f"[SORGENTE] GIUDICI dichiarati ({len(giudici_modelli)}):")
for g in giudici_modelli:
    print(f"  - {g}")

# 2. CONFRONTO CON TELEMETRIA: finestra verdetto
v_min, v_max = 1789259144, 1789268610

print(f"\n[TELEMETRIA] Chiamate nella finestra verdetto:")
c.execute("SELECT model_name, COUNT(*), SUM(total_tokens) FROM telemetry WHERE ts >= ? AND ts <= ? GROUP BY model_name ORDER BY COUNT(*) DESC", (v_min, v_max))
finestra = {}
for r in c.fetchall():
    finestra[r[0]] = {'n': r[1], 'tok': r[2]}
    tag = " <<< GIUDICE" if r[0] in giudici_modelli else " <<< ??? ignoto"
    print(f"  {r[0]}: {r[1]} chiamate, {r[2]:,} tok{tag}")

# 3. IDENTIFICA I MODELLI NON GIUDICI
print(f"\n[MODELLI NON GIUDICI nella finestra]")
for model, data in finestra.items():
    if model not in giudici_modelli:
        print(f"  {model}: {data['n']} chiamate, {data['tok']:,} tok")

# 4. COSTO REALE DEL VERDETTO (SOLO giudici + sintesi)
prices = {
    "deepseek/deepseek-v4-pro": 2.00,
    "z-ai/glm-5.3": 1.50,
    "openai/gpt-5.6-sol": 5.00,
    "gml": 2.50
}
print(f"\n[COSTO REALE VERDETTO] (solo modelli GIUDICI dichiarati):")
totale_giudici = 0
for model in giudici_modelli:
    if model in finestra:
        d = finestra[model]
        cost = d['tok'] / 1_000_000 * prices.get(model, 1.0)
        totale_giudici += cost
        print(f"  {model}: {d['n']} chiamate, {d['tok']:,} tok = ${cost:.4f}")
    else:
        print(f"  {model}: 0 chiamate (non presente nella finestra)")
print(f"  TOTALE GIUDICI: ${totale_giudici:.4f}")

# 5. COSTO TOTALE FINESTRA vs REALE
totale_finestra = sum(d['tok']/1_000_000 * prices.get(m, 1.0) for m, d in finestra.items() if m in prices)
other_price = 0.45  # prezzo medio per modelli non listati
totale_finestra += sum(d['tok']/1_000_000 * other_price for m, d in finestra.items() if m not in prices)

print(f"\n[CONFRONTO]")
print(f"  Costo TOTALE della finestra: ~${totale_finestra:.4f}")
print(f"  Costo REALE del verdetto (soli giudici): ${totale_giudici:.4f}")
print(f"  Rapporto: {(totale_giudici/totale_finestra*100):.1f}% del costo finestra e' verdetto reale")
print(f"  Il resto ({100-totale_giudici/totale_finestra*100:.1f}%) sono chiamate NON del verdetto")

conn.close()
