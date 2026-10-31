# -*- coding: utf-8 -*-
import os, sys, json
from datetime import datetime
BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, BASE)
from google.oauth2.credentials import Credentials
from google.auth.transport.requests import Request
from googleapiclient.discovery import build

TOK = os.path.join(BASE, ".gcal_tokens")
START = "2026-10-29T00:00:00Z"
END   = "2026-11-09T00:00:00Z"
SKIP = ("holiday","festivit","holidays","compleanni","birthday")

out = []
for fn in sorted(os.listdir(TOK)):
    if not fn.endswith(".json"): continue
    label = fn[:-5]
    path = os.path.join(TOK, fn)
    try:
        creds = Credentials.from_authorized_user_file(path)
        if not creds.valid:
            creds.refresh(Request())
            with open(path,"w",encoding="utf-8") as f:
                f.write(creds.to_json())
        svc = build("calendar","v3",credentials=creds,cache_discovery=False)
        cals = svc.calendarList().list().execute().get("items",[])
    except Exception as ex:
        print("[ERR] %s: %s" % (label, ex)); continue
    n=0
    for c in cals:
        cid=c.get("id",""); cname=c.get("summary","")
        low=(cid+" "+cname).lower()
        if any(k in low for k in SKIP): continue
        try:
            items = svc.events().list(calendarId=cid,timeMin=START,timeMax=END,
                    maxResults=2500,singleEvents=True,orderBy="startTime").execute().get("items",[])
        except Exception as ex:
            print("[ERR] %s/%s: %s" % (label,cname,ex)); continue
        for e in items:
            st=e.get("start",{}); en=e.get("end",{})
            d=st.get("date") or st.get("dateTime","")[:10]
            if not d: continue
            out.append({"account":label,"cal":cname,
                        "date":d,
                        "time":st.get("dateTime","")[11:16],
                        "end_time":en.get("dateTime","")[11:16],
                        "all_day":"date" in st,
                        "summary":e.get("summary",""),
                        "loc":e.get("location","")})
            n+=1
    print("[OK] %s -> %d eventi" % (label,n))

with open(os.path.join(BASE,"scratch","_cal_events.json"),"w",encoding="utf-8") as f:
    json.dump(out,f,ensure_ascii=False,indent=1)
print("TOTALE:",len(out))
