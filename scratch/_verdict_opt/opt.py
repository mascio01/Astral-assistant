import itertools, json, benchmark_data as b
ms = b.get_models()

def comp(m):
    d = ms.get(m); lb = (d or {}).get("lb") or {}
    v = [lb.get(k) for k in ("reasoning","coding","research") if lb.get(k) is not None]
    return round(sum(v)/len(v), 2) if v else None

def cost(m):
    d = ms.get(m)
    return round((d.get("prezzo_in",0)*20 + d.get("prezzo_out",0)*2)/22, 3) if d else None

def fam(m):
    return m.split("/")[0]

CUR = {
    "standard": ["google/gemini-3.7-flash", "moonshotai/kimi-k2.5", "qwen/qwen3.6-flash"],
    "lite": ["deepseek/deepseek-v4.1-flash", "qwen/qwen3.7-flash", "openai/gpt-6-luna"],
}

pool = []
for m, d in ms.items():
    if ":batch" in m or m.startswith("~"):
        continue
    c, k = comp(m), cost(m)
    if c is None or k is None or k <= 0:
        continue
    pool.append({"m": m, "fam": fam(m), "comp": c, "cost": k})

print("=== BASELINE ===")
for p, lst in CUR.items():
    tot_c = sum(comp(x) for x in lst)
    tot_k = sum(cost(x) for x in lst)
    print(f"{p:9} comp_tot={tot_c:.2f} cost_tot={tot_k:.3f}  {lst}")

for prof in ("standard", "lite"):
    base_comp = sum(comp(x) for x in CUR[prof])
    base_cost = sum(cost(x) for x in CUR[prof])
    best = []
    for combo in itertools.combinations(pool, 3):
        fams = {c["fam"] for c in combo}
        if len(fams) < 3:
            continue
        cc = sum(c["comp"] for c in combo)
        kk = sum(c["cost"] for c in combo)
        if kk > base_cost + 1e-9:
            continue
        best.append((cc, -kk, [c["m"] for c in combo], kk))
    best.sort(reverse=True)
    print(f"\n=== {prof.upper()} : vincolo cost_tot <= {base_cost:.3f}, 3 famiglie diverse ===")
    print(f"baseline comp_tot={base_comp:.2f}")
    for cc, nk, names, kk in best[:8]:
        print(f"  comp_tot={cc:.2f} (+{cc-base_comp:+.2f}) cost_tot={kk:.3f}  {names}")
