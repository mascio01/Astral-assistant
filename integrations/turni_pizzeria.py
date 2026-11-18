# -*- coding: utf-8 -*-
"""Genera i turni pizzeria di Nico dal PDF turni e li scrive su Google Calendar.

Regole orarie:
  - pranzo (cella "Nico P" / "Nico Pranz" nella riga pranzo): 11:30 - 15:00
  - sera (cella "Nico" nelle righe serali): 17:00 - 23:00
  - se lo stesso giorno Nico fa ANCHE il pranzo, la sera slitta a 19:00 - 23:00

Uso:
  py turni_pizzeria.py --pdf "...Turni Pizzeria nov inizio dicembre.pdf" --dry-run
  py turni_pizzeria.py --pdf ... --apply
"""
import os, re, sys, json, argparse, datetime, collections

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

COLS = [("LUN", 104.4), ("MAR", 156.0), ("MER", 207.7), ("GIO", 259.2),
        ("VEN", 310.9), ("SAB", 362.5), ("DOM", 414.1)]
MESI = {"gen": 1, "feb": 2, "mar": 3, "apr": 4, "mag": 5, "giu": 6,
        "lug": 7, "ago": 8, "set": 9, "ott": 10, "nov": 11, "dic": 12}
NICO_RE = re.compile(r"\bnico\b", re.I)
DESC = "Turno pizzeria generato da Astral Assistant (fonte: PDF turni)"
COLOR = "6"  # arancione


def _col(x):
    best, bd = None, 99.0
    for name, cx in COLS:
        d = abs(x - cx)
        if d < bd:
            bd, best = d, name
    return best if bd < 26 else None


def _parse_date(tok, year):
    d, m = tok.split("-")
    return datetime.date(year, MESI[m.lower()], int(d))


def parse_pdf(path, year=None):
    """Ritorna {date: {'lunch': [...], 'eve': [...]}} leggendo la griglia del PDF."""
    import pdfplumber
    with pdfplumber.open(path) as pdf:
        words = []
        for pg in pdf.pages:
            words += pg.extract_words()
    rows = collections.defaultdict(list)
    for w in words:
        rows[round(w["top"], 1)].append((w["x0"], w["text"]))
    tops = sorted(rows)

    blocks = []
    for t in tops:
        toks = [x[1] for x in rows[t]]
        dates = [x for x in toks if re.match(r"^\d{2}-[a-z]{3}$", x)]
        if len(dates) >= 2:
            blocks.append((t, dates[0]))
    if not blocks:
        raise SystemExit("[!] Nessuna intestazione settimana trovata nel PDF.")

    if year is None:
        year = datetime.date.today().year

    byday = collections.defaultdict(lambda: {"lunch": [], "eve": []})
    for i, (t, start) in enumerate(blocks):
        tnext = blocks[i + 1][0] if i + 1 < len(blocks) else float("inf")
        d0 = _parse_date(start, year)
        data_rows = [tt for tt in tops if t < tt < tnext]
        for ri, tt in enumerate(data_rows):
            cells = collections.defaultdict(list)
            for x, txt in sorted(rows[tt]):
                c = _col(x)
                if c:
                    cells[c].append(txt)
            for idx, (cname, _) in enumerate(COLS):
                val = " ".join(cells.get(cname, [])).strip()
                if not val or val == "/":
                    continue
                day = d0 + datetime.timedelta(days=idx)
                byday[day]["lunch" if ri == 0 else "eve"].append(val)
    return byday


def build_shifts(byday):
    """Applica le regole orarie e ritorna la lista di eventi."""
    out = []
    for day in sorted(byday):
        lunch = byday[day]["lunch"]
        eve = byday[day]["eve"]
        nico_lunch = any(NICO_RE.search(v) for v in lunch)
        nico_eve = any(NICO_RE.search(v) for v in eve)
        if nico_lunch:
            out.append({
                "date": str(day), "tipo": "pranzo",
                "start": f"{day}T11:30:00", "end": f"{day}T15:00:00",
                "summary": "Pizzeria - Pranzo",
            })
        if nico_eve:
            start_h = 19 if nico_lunch else 17
            out.append({
                "date": str(day), "tipo": "sera",
                "start": f"{day}T{start_h:02d}:00:00", "end": f"{day}T23:00:00",
                "summary": "Pizzeria - Sera",
            })
    return out


def _service(account):
    """Costruisce il client Calendar da un token gia autorizzato (no OAuth flow)."""
    from google.oauth2.credentials import Credentials
    from googleapiclient.discovery import build
    tok = os.path.join(BASE, ".gcal_tokens", account + ".json")
    if not os.path.exists(tok):
        raise SystemExit(f"[!] Token non trovato: {tok}")
    creds = Credentials.from_authorized_user_file(
        tok, ["https://www.googleapis.com/auth/calendar"])
    return build("calendar", "v3", credentials=creds, cache_discovery=False)


def to_gcal(shift):
    return {
        "summary": shift["summary"],
        "start": {"dateTime": shift["start"], "timeZone": "Europe/Rome"},
        "end": {"dateTime": shift["end"], "timeZone": "Europe/Rome"},
        "description": DESC,
        "colorId": COLOR,
    }


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--pdf", required=True)
    ap.add_argument("--year", type=int, default=None)
    ap.add_argument("--account", default="mascio666",
                    help="etichetta token in .gcal_tokens (default mascio666 = nicolomascella)")
    ap.add_argument("--calendar", default="nicolomascella@gmail.com")
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument("--apply", action="store_true")
    ap.add_argument("--json-out", default=None)
    args = ap.parse_args()

    byday = parse_pdf(args.pdf, args.year)
    shifts = build_shifts(byday)
    print(f"Giorni con Nico: {len({s['date'] for s in shifts})}  Eventi: {len(shifts)}")
    for s in shifts:
        print(f"  {s['date']}  {s['summary']:<18} {s['start'][11:16]}-{s['end'][11:16]}")

    if args.json_out:
        json.dump(shifts, open(args.json_out, "w", encoding="utf-8"),
                  ensure_ascii=False, indent=1)
        print(f"Scritto {args.json_out}")

    if not args.apply:
        print("\n[dry-run] nessuna scrittura su Google Calendar.")
        return

    svc = _service(args.account)
    cal = args.calendar
    existing = set()
    page = svc.events().list(calendarId=cal, timeMin=shifts[0]["start"] + "Z",
                             timeMax=shifts[-1]["date"] + "T23:59:59Z",
                             singleEvents=True, maxResults=2500).execute()
    for ev in page.get("items", []):
        existing.add((ev["start"].get("dateTime", "")[:16], ev.get("summary")))
    ok = skip = 0
    for s in shifts:
        key = (s["start"][:16], s["summary"])
        if key in existing:
            print(f"  [=] gia presente {s['start'][:16]} {s['summary']}")
            skip += 1
            continue
        try:
            svc.events().insert(calendarId=cal, body=to_gcal(s)).execute()
            ok += 1
            print(f"  [+] {s['start'][:16]} {s['summary']}")
        except Exception as e:
            print(f"  [!] {s['start'][:16]} ERRORE {e}")
    print(f"\nInseriti {ok}, saltati {skip}, totale {len(shifts)}")


if __name__ == "__main__":
    main()