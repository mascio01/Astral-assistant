# -*- coding: utf-8 -*-
"""Agenda unificata multi-account: legge tutti i calendari dei 3 account Google
e produce un riepilogo + grafico a torta dei prossimi mesi."""
import os, sys, math
from datetime import date, timedelta
from collections import defaultdict

BASE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, BASE)
import gcal_import as g

START = date(2026, 10, 13)
END = date(2026, 11, 8)
DESK = os.path.join(os.path.expanduser("~"), "Desktop")

ACCOUNTS = [
    ("nicolomascella@gmail.com", "default"),
    ("mascio666@gmail.com", "mascio666"),
    ("mascella.student@spaziolabo.it", "mascella.student"),
]

SKIP_CAL = ("holiday", "festivit", "holidays", "compleanni", "birthday")

def categorize(summary, cal):
    s = (summary + " " + cal).lower()
    if any(k in s for k in ["pizzeria", "noste", "no nico", "no ste", "no pit",
                            "focaccia", "turno", "spazza"]):
        return "Lavoro"
    if any(k in s for k in ["psico", "visita", "esami", "sangue", "urolog",
                            "malpighi", "orsoka", "hiv", "medic", "dott"]):
        return "Salute"
    if any(k in s for k in ["accademia", "spaziolabo", "lezione", "esame",
                            "corso", "universit", "studio", "lab"]):
        return "Studio"
    return "Personale"

events = []
per_account = {}
for email, label in ACCOUNTS:
    try:
        svc = g.get_service(label)
        cals = svc.calendarList().list().execute().get("items", [])
    except Exception as ex:
        print("[!] %s: %s" % (email, ex))
        continue
    n_acc = 0
    for c in cals:
        cid = c.get("id", "")
        cname = c.get("summary", "")
        if any(k in cid.lower() or k in cname.lower() for k in SKIP_CAL):
            continue
        try:
            items = svc.events().list(
                calendarId=cid,
                timeMin=START.isoformat() + "T00:00:00Z",
                timeMax=END.isoformat() + "T23:59:59Z",
                maxResults=2500, singleEvents=True, orderBy="startTime"
            ).execute().get("items", [])
        except Exception as ex:
            print("[!] %s / %s: %s" % (email, cname, ex))
            continue
        for e in items:
            st = e.get("start", {})
            d = st.get("date") or st.get("dateTime", "")[:10]
            if not d:
                continue
            tm = st.get("dateTime", "")[11:16]
            events.append({"date": d, "time": tm, "summary": e.get("summary", ""),
                           "cal": cname, "account": email})
            n_acc += 1
    per_account[email] = n_acc

day_events = defaultdict(list)
for ev in events:
    day_events[ev["date"]].append(ev)

all_days = []
d = START
while d <= END:
    all_days.append(d.isoformat())
    d += timedelta(days=1)

print("=" * 70)
print("AGENDA UNIFICATA  %s -> %s  (%d giorni)" % (START, END, len(all_days)))
print("=" * 70)
for email, n in per_account.items():
    print("  %-38s %3d eventi" % (email, n))
print("  TOTALE: %d eventi" % len(events))

CATS = ["Lavoro", "Salute", "Studio", "Personale"]
for cat in CATS:
    days = sorted(x for x in all_days
                  if any(categorize(e["summary"], e["cal"]) == cat for e in day_events.get(x, [])))
    print("\n--- %s (%d giorni) ---" % (cat.upper(), len(days)))
    for x in days:
        det = "; ".join("%s%s [%s]" % (e["summary"], (" " + e["time"]) if e["time"] else "",
                                       e["account"].split("@")[0])
                        for e in day_events[x] if categorize(e["summary"], e["cal"]) == cat)
        print("   %s  %s" % (x, det))

free_days = [x for x in all_days if not day_events.get(x)]
print("\n--- GIORNI LIBERI (%d) ---" % len(free_days))
print("   " + ", ".join(free_days))

counts = [0, 0, 0, 0]
for x in all_days:
    cats = set(categorize(e["summary"], e["cal"]) for e in day_events.get(x, []))
    if "Lavoro" in cats:
        counts[0] += 1
    elif "Salute" in cats:
        counts[1] += 1
    elif "Studio" in cats:
        counts[2] += 1
    elif "Personale" in cats:
        counts[3] += 1
    else:
        counts[3] += 1
labels = ["Lavoro", "Salute", "Studio", "Tempo libero"]
print("\nTORTA:", dict(zip(labels, counts)))

colors = ["#E8743B", "#4C9F70", "#8E6CBF", "#D9D9D9"]
out_png = os.path.join(DESK, "impegni_torta_ott_nov_2026.png")
out_svg = os.path.join(DESK, "impegni_torta_ott_nov_2026.svg")

def make_svg(path):
    total = sum(counts) or 1
    cx, cy, r = 320, 300, 200
    ang = -math.pi / 2
    parts = []
    for lab, val, col in zip(labels, counts, colors):
        frac = val / total
        if frac <= 0:
            continue
        a2 = ang + frac * 2 * math.pi
        x1, y1 = cx + r * math.cos(ang), cy + r * math.sin(ang)
        x2, y2 = cx + r * math.cos(a2), cy + r * math.sin(a2)
        large = 1 if frac > 0.5 else 0
        if frac >= 0.9999:
            parts.append('<circle cx="%d" cy="%d" r="%d" fill="%s"/>' % (cx, cy, r, col))
        else:
            parts.append('<path d="M%d %d L%.2f %.2f A%d %d 0 %d 1 %.2f %.2f Z" fill="%s" stroke="#fff" stroke-width="2"/>'
                         % (cx, cy, x1, y1, r, r, large, x2, y2, col))
        mid = ang + frac * math.pi
        lx, ly = cx + (r * 0.62) * math.cos(mid), cy + (r * 0.62) * math.sin(mid)
        parts.append('<text x="%.1f" y="%.1f" font-family="Segoe UI" font-size="16" fill="#222" text-anchor="middle">%d%%</text>'
                     % (lx, ly, round(frac * 100)))
        ang = a2
    legend = []
    for i, (lab, val, col) in enumerate(zip(labels, counts, colors)):
        y = 120 + i * 34
        legend.append('<rect x="580" y="%d" width="20" height="20" fill="%s"/>' % (y, col))
        legend.append('<text x="610" y="%d" font-family="Segoe UI" font-size="17" fill="#222">%s: %d gg</text>' % (y + 16, lab, val))
    svg = ('<svg xmlns="http://www.w3.org/2000/svg" width="900" height="620">'
           '<rect width="900" height="620" fill="#ffffff"/>'
           '<text x="450" y="50" font-family="Segoe UI" font-size="24" font-weight="bold" fill="#222" text-anchor="middle">Impegni 13/10 - 08/11/2026 (3 account)</text>'
           + "".join(parts) + "".join(legend) + '</svg>')
    with open(path, "w", encoding="utf-8") as f:
        f.write(svg)

try:
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    fig, ax = plt.subplots(figsize=(9, 6))
    ax.pie(counts, labels=labels, colors=colors, autopct="%1.0f%%",
           startangle=90, textprops={"fontsize": 12})
    ax.set_title("Impegni 13/10 - 08/11/2026 (3 account)", fontsize=15, fontweight="bold")
    ax.axis("equal")
    fig.tight_layout()
    fig.savefig(out_png, dpi=130)
    print("CHART_PNG:", out_png)
except Exception as ex:
    print("matplotlib non disponibile (%s), genero SVG" % ex)
    make_svg(out_svg)
    print("CHART_SVG:", out_svg)

# --- Grafico a barre: eventi per giorno (impilati per categoria) ---
bar_png = os.path.join(DESK, "impegni_bar_ott_nov_2026.png")
bar_svg = os.path.join(DESK, "impegni_bar_ott_nov_2026.svg")

day_cat = {x: {c: 0 for c in CATS} for x in all_days}
for x in all_days:
    for e in day_events.get(x, []):
        day_cat[x][categorize(e["summary"], e["cal"])] += 1

def make_bar_svg(path):
    W, H = 1200, 620
    left, right, top, bottom = 70, 30, 70, 110
    plot_w = W - left - right
    plot_h = H - top - bottom
    n = len(all_days)
    maxv = max((sum(day_cat[x].values()) for x in all_days), default=1) or 1
    bw = plot_w / n * 0.7
    step = plot_w / n
    parts = []
    parts.append('<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="#888"/>' % (left, top + plot_h, left + plot_w, top + plot_h))
    parts.append('<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="#888"/>' % (left, top, left, top + plot_h))
    for gv in range(0, maxv + 1):
        y = top + plot_h - (gv / maxv) * plot_h
        parts.append('<line x1="%d" y1="%.1f" x2="%d" y2="%.1f" stroke="#eee"/>' % (left, y, left + plot_w, y))
        parts.append('<text x="%d" y="%.1f" font-family="Segoe UI" font-size="12" fill="#666" text-anchor="end">%d</text>' % (left - 8, y + 4, gv))
    for i, x in enumerate(all_days):
        cx = left + step * i + step / 2
        yb = top + plot_h
        for c, col in zip(CATS, colors):
            v = day_cat[x][c]
            if v <= 0:
                continue
            h = (v / maxv) * plot_h
            yb -= h
            parts.append('<rect x="%.1f" y="%.1f" width="%.1f" height="%.1f" fill="%s"/>' % (cx - bw / 2, yb, bw, h, col))
        parts.append('<text x="%.1f" y="%d" font-family="Segoe UI" font-size="11" fill="#333" text-anchor="end" transform="rotate(-60 %.1f %d)">%s</text>' % (cx, top + plot_h + 18, cx, top + plot_h + 18, x[5:]))
    for i, (lab, col) in enumerate(zip(CATS, colors)):
        y = top + i * 24
        parts.append('<rect x="%d" y="%d" width="16" height="16" fill="%s"/>' % (left + plot_w - 140, y, col))
        parts.append('<text x="%d" y="%d" font-family="Segoe UI" font-size="14" fill="#222">%s</text>' % (left + plot_w - 118, y + 13, lab))
    svg = ('<svg xmlns="http://www.w3.org/2000/svg" width="%d" height="%d">' % (W, H)
           + '<rect width="%d" height="%d" fill="#fff"/>' % (W, H)
           + '<text x="%d" y="40" font-family="Segoe UI" font-size="22" font-weight="bold" fill="#222" text-anchor="middle">Eventi per giorno 13/10 - 08/11/2026 (3 account)</text>' % (W // 2)
           + "".join(parts) + '</svg>')
    with open(path, "w", encoding="utf-8") as f:
        f.write(svg)

try:
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    xs = list(range(len(all_days)))
    bottoms = [0] * len(all_days)
    fig, ax = plt.subplots(figsize=(14, 6))
    for c, col in zip(CATS, colors):
        vals = [day_cat[x][c] for x in all_days]
        ax.bar(xs, vals, bottom=bottoms, color=col, label=c, width=0.7)
        bottoms = [b + v for b, v in zip(bottoms, vals)]
    ax.set_xticks(xs)
    ax.set_xticklabels([x[5:] for x in all_days], rotation=60, ha="right", fontsize=9)
    ax.set_ylabel("Numero eventi")
    ax.set_title("Eventi per giorno 13/10 - 08/11/2026 (3 account)", fontsize=15, fontweight="bold")
    ax.legend()
    fig.tight_layout()
    fig.savefig(bar_png, dpi=130)
    print("BAR_PNG:", bar_png)
except Exception as ex:
    print("matplotlib non disponibile (%s), genero SVG bar" % ex)
    make_bar_svg(bar_svg)
    print("BAR_SVG:", bar_svg)