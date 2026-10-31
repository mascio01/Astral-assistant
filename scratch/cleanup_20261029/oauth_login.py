# -*- coding: utf-8 -*-
"""Login OAuth: genera URL, lo scrive su file e attende il code su localhost."""
import os, sys, json, time, threading
from http.server import BaseHTTPRequestHandler, HTTPServer
from urllib.parse import urlparse, parse_qs

BASE = os.path.dirname(os.path.abspath(__file__))
CREDS = os.path.join(BASE, "credentials.json")
TOKEN_DIR = os.path.join(BASE, ".gcal_tokens")
SCRATCH = os.path.join(BASE, "scratch")
SCOPES = ["https://www.googleapis.com/auth/calendar"]
PORTS = {"mascella.student": 8765, "mascio666": 8766, "nicolomascella": 8767}
PORT = 8765
REDIRECT = f"http://localhost:{PORT}/"
TIMEOUT = int(os.environ.get("OAUTH_TIMEOUT", "600"))

ACCOUNTS = ["mascella.student", "mascio666", "nicolomascella"]


class H(BaseHTTPRequestHandler):
    """Handler che cattura ?code=... dal redirect di Google."""

    def do_GET(self):
        qs = parse_qs(urlparse(self.path).query)
        code = (qs.get("code") or [None])[0]
        err = (qs.get("error") or [None])[0]
        if code:
            self.server.auth_code = code
            body = b"<h2>OK - autorizzazione ricevuta.</h2><p>Puoi chiudere questa finestra.</p>"
        elif err:
            self.server.auth_error = err
            body = f"<h2>Errore: {err}</h2>".encode()
        else:
            body = b"<h2>In attesa del codice...</h2>"
        self.send_response(200)
        self.send_header("Content-Type", "text/html; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def log_message(self, *a):
        pass


def login_account(account):
    from google_auth_oauthlib.flow import InstalledAppFlow
    from google.oauth2.credentials import Credentials

    if not os.path.exists(CREDS):
        print(f"[!] Manca {CREDS}")
        return False

    os.makedirs(TOKEN_DIR, exist_ok=True)
    os.makedirs(SCRATCH, exist_ok=True)
    token_path = os.path.join(TOKEN_DIR, account + ".json")

    if os.path.exists(token_path):
        try:
            c = Credentials.from_authorized_user_file(token_path, SCOPES)
            if c.valid:
                print(f"[OK] {account}: token gia' valido")
                return True
        except Exception:
            pass

    port = PORTS.get(account, PORT)
    redirect = f"http://localhost:{port}/"
    flow = InstalledAppFlow.from_client_secrets_file(CREDS, SCOPES,
                                                     redirect_uri=redirect)
    url, _ = flow.authorization_url(access_type="offline", prompt="consent",
                                    include_granted_scopes="true")

    url_file = os.path.join(SCRATCH, f"_oauth_url_{account}.txt")
    with open(url_file, "w", encoding="utf-8") as f:
        f.write(url)

    srv = HTTPServer(("localhost", port), H)
    srv.auth_code = None
    srv.auth_error = None
    t = threading.Thread(target=srv.serve_forever, daemon=True)
    t.start()

    print(f"[URL] {account}: {url}")
    print(f"[ATTESA] In ascolto su {redirect} (max {TIMEOUT}s)")

    deadline = time.time() + TIMEOUT
    while time.time() < deadline and not srv.auth_code and not srv.auth_error:
        time.sleep(0.5)

    srv.shutdown()

    if srv.auth_error:
        print(f"[!] {account}: errore OAuth -> {srv.auth_error}")
        return False
    if not srv.auth_code:
        print(f"[!] {account}: TIMEOUT_NO_CODE")
        return False

    try:
        flow.fetch_token(code=srv.auth_code)
    except Exception as e:
        print(f"[!] {account}: scambio code fallito -> {e}")
        return False

    with open(token_path, "w", encoding="utf-8") as f:
        f.write(flow.credentials.to_json())
    print(f"[OK] {account}: token salvato in {token_path}")
    return True


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("-")]
    accounts = args if args else ACCOUNTS
    ok = 0
    for acc in accounts:
        try:
            if login_account(acc):
                ok += 1
        except Exception as e:
            print(f"[!] {acc}: eccezione -> {e}")
    print(f"\nCompletato: {ok}/{len(accounts)}")


if __name__ == "__main__":
    main()
