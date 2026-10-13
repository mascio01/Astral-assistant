# -*- coding: utf-8 -*-
"""Import turni Nico su Google Calendar via OAuth.

Uso:
  py gcal_import.py            -> importa nico_shifts.json sul calendario scelto
  py gcal_import.py --list     -> elenca account e calendari disponibili

Richiede credentials.json (OAuth client Desktop) nella cartella dello script.
Ad ogni esecuzione chiede quale account e quale calendario usare.
"""
import os, sys, json, argparse, datetime

BASE = os.path.dirname(os.path.abspath(__file__))
CREDS = os.path.join(BASE, "credentials.json")
TOKEN_DIR = os.path.join(BASE, ".gcal_tokens")
SCOPES = ["https://www.googleapis.com/auth/calendar"]

def _load_libs():
    from google.oauth2.credentials import Credentials
    from google_auth_oauthlib.flow import InstalledAppFlow
    from google.auth.transport.requests import Request
    from googleapiclient.discovery import build
    return Credentials, InstalledAppFlow, Request, build

def get_service(account_hint=None):
    Credentials, InstalledAppFlow, Request, build = _load_libs()
    if not os.path.exists(CREDS):
        print(f"[!] Manca {CREDS}")
        print("    Scarica un OAuth client (Desktop app) da Google Cloud Console")
        print("    e salvalo come credentials.json in questa cartella.")
        sys.exit(1)
    os.makedirs(TOKEN_DIR, exist_ok=True)
    token_path = os.path.join(TOKEN_DIR, (account_hint or "default") + ".json")
    creds = None
    if os.path.exists(token_path):
        creds = Credentials.from_authorized_user_file(token_path, SCOPES)
    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            creds.refresh(Request())
        else:
            flow = InstalledAppFlow.from_client_secrets_file(CREDS, SCOPES)
            creds = flow.run_local_server(port=0)
        with open(token_path, "w", encoding="utf-8") as f:
            f.write(creds.to_json())
    return build("calendar", "v3", credentials=creds)

def choose(prompt, items, label):
    print(f"\n{prompt}")
    for i, it in enumerate(items, 1):
        print(f"  {i}. {label(it)}")
    while True:
        raw = input("Scelta (numero): ").strip()
        if raw.isdigit() and 1 <= int(raw) <= len(items):
            return items[int(raw) - 1]
        print("  Inserisci un numero valido.")

def list_calendars(service):
    cals = service.calendarList().list().execute().get("items", [])
    return cals

def build_events(shifts):
    events = []
    seen = set()
    for s in shifts:
        d = datetime.date.fromisoformat(s["date"])
        key = (s["date"], s["tipo"])
        if key in seen:
            continue
        seen.add(key)
        if s["tipo"] == "pranzo":
            start_dt = datetime.datetime.combine(d, datetime.time(11, 30))
            end_dt = datetime.datetime.combine(d, datetime.time(15, 30))
            summary = "Pizzeria - Pranzo"
        else:
            start_dt = datetime.datetime.combine(d, datetime.time(17, 0))
            # 24:00 non e' accettato dall'API: uso la mezzanotte del giorno dopo
            end_dt = datetime.datetime.combine(d + datetime.timedelta(days=1),
                                               datetime.time(0, 0))
            summary = "Pizzeria - Sera"
        events.append({
            "summary": summary,
            "start": {"dateTime": start_dt.strftime("%Y-%m-%dT%H:%M:%S"),
                      "timeZone": "Europe/Rome"},
            "end": {"dateTime": end_dt.strftime("%Y-%m-%dT%H:%M:%S"),
                    "timeZone": "Europe/Rome"},
            "description": "Turno generato da Astral Assistant",
            "colorId": "6",
        })
    return events

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--list", action="store_true", help="elenca calendari e esci")
    ap.add_argument("--account", default=None, help="etichetta account per il token")
    ap.add_argument("--calendar", default=None, help="id calendario (salta la scelta)")
    ap.add_argument("--file", default=os.path.join(BASE, "nico_shifts.json"))
    ap.add_argument("--clean", action="store_true",
                    help="cancella eventi importati in precedenza prima di importare")
    args = ap.parse_args()

    service = get_service(args.account)
    cals = list_calendars(service)
    if not cals:
        print("[!] Nessun calendario accessibile.")
        return
    if args.list:
        for c in cals:
            print(f"  {c.get('summary')}  ->  {c.get('id')}  (primary={c.get('primary', False)})")
        return

    if args.calendar:
        cal_id = args.calendar
    else:
        cal = choose("Quale calendario?", cals,
                     lambda c: f"{c.get('summary')} [{c.get('id')}]")
        cal_id = cal["id"]

    if args.clean:
        print(f"\nPulizia eventi precedenti su: {cal_id}")
        page = service.events().list(calendarId=cal_id, maxResults=2500,
                                     q="Turno generato da Astral Assistant").execute()
        for ev in page.get("items", []):
            if ev.get("description") == "Turno generato da Astral Assistant":
                service.events().delete(calendarId=cal_id, eventId=ev["id"]).execute()
                print(f"  [-] rimosso {ev.get('start', {}).get('dateTime', '?')}")
        print("Pulizia completata.")

    shifts = json.load(open(args.file, encoding="utf-8"))
    events = build_events(shifts)
    print(f"\nImporto {len(events)} eventi su: {cal_id}")
    ok = 0
    for ev in events:
        try:
            service.events().insert(calendarId=cal_id, body=ev).execute()
            ok += 1
            print(f"  [+] {ev['start']['dateTime']}  {ev['summary']}")
        except Exception as e:
            print(f"  [!] {ev['start']['dateTime']}  ERRORE: {e}")
    print(f"\nCompletato: {ok}/{len(events)} eventi inseriti.")

if __name__ == "__main__":
    main()
