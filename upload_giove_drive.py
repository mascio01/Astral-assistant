import os, sys, json, pickle, time
from pathlib import Path
from google.auth.transport.requests import Request
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.http import MediaFileUpload

BASE = Path(r"C:\Users\Nicolò\Astral Assistant")
SRC  = Path(r"C:\Users\Nicolò\Desktop\GioveCalibrator\windows")
CRED = BASE / "credentials.json"
TOKEN= BASE / "gdrive_token_giove.pickle"
MANIFEST = BASE / "gdrive_manifest_giove.json"
ROOT_NAME = "GioveCalibrator_windows"
SCOPES = ["https://www.googleapis.com/auth/drive.file"]

def log(m):
    print(time.strftime("%H:%M:%S"), m, flush=True)

def get_service():
    creds = None
    if TOKEN.exists():
        creds = pickle.loads(TOKEN.read_bytes())
    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            try:
                creds.refresh(Request())
            except Exception:
                creds = None
        if not creds:
            flow = InstalledAppFlow.from_client_secrets_file(str(CRED), SCOPES)
            print("APRO IL BROWSER: accedi come mascio666@gmail.com e autorizza.", flush=True)
            creds = flow.run_local_server(port=0, open_browser=True,
                                          authorization_prompt_message="Autorizza l'accesso a Drive: {url}")
        TOKEN.write_bytes(pickle.dumps(creds))
    return build("drive", "v3", credentials=creds, cache_discovery=False)

def find_or_create(svc, name, parent=None):
    q = "name='%s' and mimeType='application/vnd.google-apps.folder' and trashed=false" % name.replace("'", r"\'")
    if parent: q += " and '%s' in parents" % parent
    r = svc.files().list(q=q, spaces="drive", fields="files(id,name)").execute()
    f = r.get("files", [])
    if f: return f[0]["id"]
    meta = {"name": name, "mimeType": "application/vnd.google-apps.folder"}
    if parent: meta["parents"] = [parent]
    return svc.files().create(body=meta, fields="id").execute()["id"]

def main():
    svc = get_service()
    about = svc.get(fileId=None) if False else None
    root_id = find_or_create(svc, ROOT_NAME)
    log(f"cartella Drive '{ROOT_NAME}' id={root_id}")
    manifest = json.loads(MANIFEST.read_text()) if MANIFEST.exists() else {}
    folders = {"." : root_id}
    files = [p for p in SRC.rglob("*") if p.is_file()]
    total = len(files); done=0; uploaded=0; skipped=0; failed=[]
    total_bytes = sum(p.stat().st_size for p in files)
    log(f"{total} file da processare ({total_bytes/1024**2:.1f} MB)")
    for p in files:
        done += 1
        rel = str(p.relative_to(SRC))
        # cartelle padre
        parent_id = root_id
        cur_rel = ""
        for part in Path(rel).parts[:-1]:
            key = str(Path(cur_rel)/part) if cur_rel else part
            if key not in folders:
                folders[key] = find_or_create(svc, part, folders.get(cur_rel or ".", root_id))
            parent_id = folders[key]; cur_rel = key if cur_rel else part
        st = p.stat()
        mkey = rel.replace("\\","/")
        ent = manifest.get(mkey)
        if ent and ent.get("size")==st.st_size:
            skipped += 1; continue
        try:
            media = MediaFileUpload(str(p), chunksize=20*1024*1024, resumable=True)
            body={"name": p.name}
            if parent_id and parent_id!=root_id: body["parents"]=[parent_id]
            elif parent_id==root_id: body["parents"]=[root_id]
            req = svc.files().create(body=body, media_body=media, fields="id")
            resp=None
            while resp is None:
                _status, resp = req.next_chunk()
            fid = resp.get("id") if hasattr(resp, "get") else getattr(resp, "id", None)
            if not fid:
                raise RuntimeError("upload senza id restituito: %r" % (resp,))
            manifest[mkey]={"id":fid,"size":st.st_size}
            uploaded+=1
            if uploaded % 25 == 0:
                MANIFEST.write_text(json.dumps(manifest)); log(f"[{done}/{total}] {mkey}")
        except Exception as e:
            failed.append((mkey,str(e)[:200])); log(f"ERRORE {mkey}: {str(e)[:150]}")
            MANIFEST.write_text(json.dumps(manifest))
    MANIFEST.write_text(json.dumps(manifest))
    log(f"FINE caricate={uploaded} saltate={skipped} errori={len(failed)}")
    for f in failed[:20]: log(f"KO {f[0]}: {f[1]}")

if __name__=="__main__":
    main()
