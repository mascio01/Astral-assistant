# Chat Hub

Chat di rete locale senza dipendenze esterne (solo Python 3 standard library).

## Requisiti

- Python 3.8 o superiore
- Tutti i computer sulla stessa rete locale (Wi-Fi/LAN)

Nessun pacchetto da installare: niente pip, niente requirements.txt.

## Avvio

### Windows
    py chat_hub.py

### macOS / Linux
    python3 chat_hub.py

Il primo avvio apre il browser su http://127.0.0.1:8765/ e stampa in console
l'indirizzo di rete (es. http://192.168.1.42:8765/) da dare agli altri.

## Come si entra

1. Chi avvia il programma diventa host della stanza.
2. Gli altri aprono l'indirizzo di rete nel browser e inseriscono un nickname.
3. La stanza si chiude automaticamente 10 secondi dopo l'uscita dell'ultimo utente.

## Opzioni

    python3 chat_hub.py --port 9000      # porta diversa
    python3 chat_hub.py --no-browser     # non aprire il browser (utile su server/SSH)

Variabili d'ambiente: CHAT_HUB_PORT (porta di default).

## Problemi comuni su macOS

- "python3: command not found": installa Python da https://www.python.org/downloads/
  oppure con Homebrew: brew install python
- Al primo avvio macOS chiede se consentire le connessioni in entrata:
  clicca "Consenti". Se hai negato, vai in Impostazioni di Sistema >
  Rete > Firewall e riabilita Python.
- Gli altri non riescono a connettersi: verifica di essere sulla stessa rete
  e che l'indirizzo usato sia quello stampato in console (non 127.0.0.1).
- Il browser non si apre da terminale remoto: usa --no-browser e apri
  l'indirizzo manualmente.

## File

    chat_hub.py        server HTTP + logica stanza
    static/index.html  interfaccia web
    static/app.js      client JavaScript (polling)
