# -*- coding: utf-8 -*-
"""Aggiunge un singolo evento su Google Calendar via OAuth (riusa gcal_import)."""
import os, sys, argparse, datetime

BASE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, BASE)
import gcal_import as g


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--account", default=None)
    ap.add_argument("--summary", required=True)
    ap.add_argument("--description", default="")
    ap.add_argument("--date", required=True, help="YYYY-MM-DD")
    ap.add_argument("--time", default="09:00", help="HH:MM")
    ap.add_argument("--duration", type=int, default=30)
    ap.add_argument("--calendar", default=None)
    args = ap.parse_args()

    service = g.get_service(args.account)
    cals = g.list_calendars(service)
    if args.calendar:
        cal_id = args.calendar
    else:
        cal = g.choose("Quale calendario?", cals,
                       lambda c: f"{c.get('summary')} [{c.get('id')}]")
        cal_id = cal["id"]

    start = datetime.datetime.strptime(f"{args.date} {args.time}", "%Y-%m-%d %H:%M")
    end = start + datetime.timedelta(minutes=args.duration)
    ev = {
        "summary": args.summary,
        "description": args.description,
        "start": {"dateTime": start.strftime("%Y-%m-%dT%H:%M:%S"),
                  "timeZone": "Europe/Rome"},
        "end": {"dateTime": end.strftime("%Y-%m-%dT%H:%M:%S"),
                "timeZone": "Europe/Rome"},
        "colorId": "6",
    }
    created = service.events().insert(calendarId=cal_id, body=ev).execute()
    print(f"OK evento creato: {created.get('htmlLink')}")


if __name__ == "__main__":
    main()
