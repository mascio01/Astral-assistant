# -*- coding: utf-8 -*-
import json, os, math
from datetime import date, timedelta
from collections import defaultdict

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
d = json.load(open(os.path.join(BASE,"scratch","_cal_events.json"), encoding="utf-8"))

def cat(e):
    s = (e["summary"] + " " + e["cal"]).lower()
    if any(k in s for k in ["pizzeria","nost","no nico","no ste","no pit","focaccia","turno","spazza","lav. sala","sala"]):
        return "Lavoro"
    if any(k in s for k in ["psico","visita","esami","sangue","urolog","malpighi","orsoka","hiv","medic","dott"]):
        return "Salute"
    if any(k in s for k in ["accademia","spaziolabo","lezione","esame","corso","universit","studio","lab"]):
        return "Studio"
    return "Personale"

def hours(e):
    if e["all_day"] or not e["time"] or not e["end_time"]:
        return 0.0
    h1,m1 = map(int,e["time"].split(":")); h2,m2 = map(int,e["end_time"].split(":"))
    a = h1*60+m1; b = h2*60+m2
    if b <= a: b += 24*60
    return (b-a)/60.0

CATS = ["Lavoro","Salute","Studio","Personale"]
tot = {c:0.0 for c in CATS}
cnt = {c:0 for c in CATS}
per_day = defaultdict(lambda: defaultdict(float))
allday = []

for e in d:
    c = cat(e); h = hours(e)
    tot[c] += h; cnt[c] += 1
    per_day[e["date"]][c] += h
    if e["all_day"]:
        allday.append((e["date"], e["summary"], c))

START = date(2026,10,29); END = date(2026,11,8)
days = []
x = START
while x <= END:
    days.append(x.isoformat()); x += timedelta(days=1)

print("PERIODO:", START, "->", END, "(%d giorni)" % len(days))
print()
for c in CATS:
    print("%-10s %5.1f h   (%d eventi)" % (c, tot[c], cnt[c]))
print()
print("DETTAGLIO GIORNALIERO")
for dd in days:
    row = per_day.get(dd, {})
    tot_d = sum(row.values())
    det = ", ".join("%s %.1fh" % (k,v) for k,v in row.items() if v>0)
    print("  %s  tot %4.1fh  %s" % (dd[5:], tot_d, det))
print()
print("ALL-DAY:", allday)

# tempo libero residuo su base sveglia 8-24 (16h/giorno)
awake = len(days)*16.0
busy = sum(tot.values())
print()
print("Ore totali periodo (24h): %.0f" % (len(days)*24))
print("Ore impegnate: %.1f" % busy)
print("Ore veglia disponibili (16h/gg): %.0f -> libere %.1f" % (awake, awake-busy))

# SVG torta
colors = {"Lavoro":"#E8743B","Salute":"#4C9F70","Studio":"#8E6CBF","Personale":"#F2C14E"}
labels = CATS + ["Tempo libero"]
vals   = [tot[c] for c in CATS] + [max(awake-busy,0)]
cols   = [colors[c] for c in CATS] + ["#D9D9D9"]

def svg_pie(path):
    totalv = sum(vals) or 1
    cx,cy,r = 330,320,210
    ang = -math.pi/2; parts=[]
    for lab,v,col in zip(labels,vals,cols):
        frac=v/totalv
        if frac<=0: continue
        a2=ang+frac*2*math.pi
        x1,y1=cx+r*math.cos(ang),cy+r*math.sin(ang)
        x2,y2=cx+r*math.cos(a2),cy+r*math.sin(a2)
        large=1 if frac>0.5 else 0
        parts.append('<path d="M%d %d L%.2f %.2f A%d %d 0 %d 1 %.2f %.2f Z" fill="%s" stroke="#fff" stroke-width="2"/>'%(cx,cy,x1,y1,r,r,large,x2,y2,col))
        mid=ang+frac*math.pi
        lx,ly=cx+(r*0.65)*math.cos(mid),cy+(r*0.65)*math.sin(mid)
        parts.append('<text x="%.1f" y="%.1f" font-family="Segoe UI" font-size="15" fill="#222" text-anchor="middle">%d%%</text>'%(lx,ly,round(frac*100)))
        ang=a2
    leg=[]
    for i,(lab,v,col) in enumerate(zip(labels,vals,cols)):
        y=140+i*36
        leg.append('<rect x="600" y="%d" width="20" height="20" fill="%s"/>'%(y,col))
        leg.append('<text x="630" y="%d" font-family="Segoe UI" font-size="16" fill="#222">%s — %.1f h</text>'%(y+16,lab,v))
    s=('<svg xmlns="http://www.w3.org/2000/svg" width="960" height="660">'
       '<rect width="960" height="660" fill="#fff"/>'
       '<text x="480" y="52" font-family="Segoe UI" font-size="23" font-weight="bold" fill="#222" text-anchor="middle">Impegni 29/10 – 08/11/2026 · tempo libero vs occupato</text>'
       '<text x="480" y="80" font-family="Segoe UI" font-size="14" fill="#666" text-anchor="middle">base: ore di veglia 8:00–24:00 · 11 giorni · 3 account Google</text>'
       + "".join(parts)+"".join(leg)+'</svg>')
    open(path,"w",encoding="utf-8").write(s)

out_svg=os.path.join(os.path.expanduser("~"),"Desktop","impegni_29ott_08nov.svg")
svg_pie(out_svg); print("SVG:",out_svg)

try:
    import matplotlib; matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    fig,ax=plt.subplots(figsize=(9.5,6.5))
    ax.pie(vals,labels=labels,colors=cols,autopct=lambda p:"%.0f%%"%p,
           startangle=90,pctdistance=0.72,
           wedgeprops={"edgecolor":"white","linewidth":2},textprops={"fontsize":12})
    ax.set_title("Impegni 29/10 – 08/11/2026\n(tempo libero vs occupato)",fontsize=15,fontweight="bold")
    ax.axis("equal"); fig.tight_layout()
    png=os.path.join(os.path.expanduser("~"),"Desktop","impegni_29ott_08nov.png")
    fig.savefig(png,dpi=140); print("PNG:",png)
except Exception as ex:
    print("matplotlib KO:",ex)
