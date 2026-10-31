# Chat Hub LAN — Documento di Progetto

> Stato: **progetto approvabile, nessun codice scritto**
> Cartella di lavoro: `C:\Users\Nicolò\Astral Assistant\scratch\`
> File previsto: `scratch\hub.py` (singolo file autocontenuto)

---

## 1. Obiettivo

Una chat di classe utilizzabile nei momenti liberi, senza installazioni sui PC dei compagni e senza server esterni.

Requisiti funzionali confermati:

| # | Requisito | Decisione |
|---|---|---|
| R1 | Avvio da cmd, chiede nickname | Launcher Python: chiede nick, avvia server, apre browser |
| R2 | Prima persona crea la stanza | Il server crea la stanza al primo join; gli altri si agganciano |
| R3 | La stanza si chiude quando non ci sono piu persone attive | Reap heartbeat + grace period |
| R4 | Due palline in alto a destra: colore testo + colore sfondo | `<input type="color">` mascherati da pallini, persistiti in localStorage |
| R5 | `/duck` -> i prossimi 3 messaggi **globali** diventano papere | Contatore globale lato server |
| R6 | `/rainbow [testo]` -> ogni lettera un colore dei 7 dell'arcobaleno | Span generati lato server |
| R7 | Tasto allegati con mini-finestra drag&drop / selezione | Modale HTML nativo |
| R8 | In chat si vede solo la card del file; il download parte **solo al click** | Metadati nel broadcast, byte solo su `GET /download` |
| R9 | Limite allegato 50 MB | Check su `Content-Length` prima del body |

Requisiti impliciti che aggiungo (senza i quali il prodotto si rompe nell'uso reale):

- **R10** Cronologia visibile a chi entra dopo (ultimi N messaggi)
- **R11** Nick univoci nella stanza
- **R12** Nessuna dipendenza pip installabile (rete scolastica spesso bloccata)
- **R13** Un solo processo, una sola porta, un solo file da copiare

---

## 2. Architettura

```
   PC host (il tuo)                          PC compagni
+--------------------------+          +-----------------------+
|  hub.py                  |          |  Browser              |
|  |- ThreadingHTTPServer  |<-------->|  http://IP:8765       |
|  |   |- GET /           |   HTTP   |  |- EventSource (SSE) |
|  |   |- GET /events     |   +SSE   |  +- fetch POST        |
|  |   |- POST /send      |          +-----------------------+
|  |   |- POST /upload    |
|  |   |- GET /download/..|          +-----------------------+
|  |   +- POST /heartbeat |<-------->|  Browser              |
|  |- RoomState (RAM)      |          +-----------------------+
|  +- attachments/ (disco) |
+--------------------------+
```

### Perche web e non CLI/Tkinter

La tua spec contiene tre elementi che un CLI testuale non puo dare: **palline colore cliccabili**, **drag&drop allegati**, **testo arcobaleno**. Il browser li fornisce nativamente:

- drag&drop -> eventi `dragover`/`drop` su un div (~10 righe)
- color picker -> `<input type="color">` mascherato con `border-radius:50%`
- rainbow -> span colorati generati da stringa gia escapata

Tkinter funzionerebbe ma richiederebbe ~3x il codice e drag&drop fragile su Windows.

### Perche SSE e non WebSocket

WebSocket in Python stdlib non esiste: servirebbe `websockets` o `aiohttp`, cioe una dipendenza esterna da installare su ogni PC o da bundlare. SSE invece:

- supportato da tutti i browser moderni via `EventSource`
- **auto-reconnect nativo** (gestisce da solo cadute di rete e F5)
- implementabile in ~30 righe sopra `ThreadingHTTPServer`

Il traffico e asimmetrico (molti messaggi in ricezione, pochi in invio): SSE + POST e la forma naturale per questo caso.

### Perche ThreadingHTTPServer

Ogni client SSE tiene aperta una connessione lunga. Con un server single-thread il primo client bloccherebbe tutti gli altri. `ThreadingHTTPServer` della stdlib risolve con zero dipendenze.

---

## 3. Ciclo di vita della stanza

```
        join #1                     join #2..N
IDLE ----------> ACTIVE <----------------------+
                  |                            |
                  | tutti i client spariscono   | rientro entro grace
                  v                            |
              GRACE (90 s) --------------------+
                  | scaduto senza rientri
                  v
              CLOSED -> stato azzerato, allegati cancellati,
                        prossimo join ricrea la stanza da zero
```

Dettagli:

1. **Presenza**: ogni client manda `POST /heartbeat` ogni **15 s**. Il server aggiorna `last_seen`.
2. **Reap**: thread demone ogni 5 s marca inattivo chi ha `last_seen > 45 s`. Copre chiusura brutale del browser o crash.
3. **Grace period**: quando i partecipanti attivi scendono a **0**, parte un timer di **90 s**. Serve perche F5 o una disconnessione Wi-Fi di dieci secondi non devono distruggere la cronologia della classe.
4. **Chiusura reale**: scaduto il grace senza rientri -> messaggi svuotati, contatore duck azzerato, cartella `attachments/` svuotata, stato torna IDLE.
5. Il primo join dopo la chiusura ricrea la stanza: e esattamente il comportamento "la prima persona che entra crea la stanza".

---

## 4. Protocollo HTTP

| Metodo | Path | Body / Query | Risposta |
|---|---|---|---|
| GET | `/` | - | HTML+CSS+JS inline (unica pagina) |
| GET | `/events?cid=..&nick=..` | - | stream `text/event-stream` |
| POST | `/send` | JSON `{cid, text}` oppure `{cid, type:"rainbow", text}` oppure `{cid, type:"duck"}` | `{ok}` |
| POST | `/upload?name=..&size=..&cid=..` | body binario raw streammato su disco | `{ok, id}` |
| GET | `/download/<id>` | - | file con `Content-Disposition: attachment` |
| POST | `/heartbeat` | JSON `{cid}` | `{ok}` |

### Formato evento SSE

```
data: {"seq":42,"type":"msg","nick":"Luca","text":"ciao","fg":"#ffffff","bg":"#111111","ts":1712345678}

data: {"seq":43,"type":"rainbow","nick":"Luca","text":"ciao","letters":[["c","#ff0000"],["i","#ff7f00"]]}

data: {"seq":44,"type":"duck","nick":"Luca","text":"papera papera papera"}

data: {"seq":45,"type":"file","nick":"Luca","file_id":"a1b2..","name":"compiti.pdf","size":2411724}

data: {"type":"presence","count":7,"nicks":["Luca","Sara"]}
```

Punti chiave:

- **`seq` assegnato dal server**, mai timestamp del client -> ordine corretto anche con orologi sfasati tra PC.
- Il rendering rainbow e calcolato **una volta sola lato server**: tutti i client mostrano lo stesso risultato senza duplicare logica.
- I colori utente viaggiano nel messaggio (`fg`,`bg`) cosi chi legge vede i colori scelti dall'autore.

---

## 5. Comandi slash

### /duck

Semantica confermata: *il testo diventa papere* sui prossimi **3 messaggi globali**, cioe contati su tutti gli utenti insieme.

```
stato_room = { duck_remaining: int }

/send con type=="duck":
    duck_remaining += 3
    broadcast evento {type:"duck", text:"papera"*len(testo)}

/send normale:
    if duck_remaining > 0:
        duck_remaining -= 1
        broadcast {type:"duck", text:"papera"*len(testo), nick}
    else:
        broadcast normale
```

Nota di design da decidere in build: se due persone digitano `/duck`, i contatori si sommano (`+3`) o si resettano a `3`? Consiglio **somma**, piu intuitivo.

Il messaggio originale viene sostituito visivamente da papere; opzionalmente conservarlo come tooltip (`title="testo originale"`) per non perdere informazione.

### /rainbow [testo]

```
lettere = [(ch, PALETTE[i % 7]) for i, ch in enumerate(testo)]
PALETTE = ["#ff0000","#ff7f00","#ffff00","#00c000","#0000ff","#4b0082","#ee82ee"]
```

Il server invia l'array gia risolto; il client fa solo:

```js
for (const [ch, color] of evt.letters) {
    const s = document.createElement("span");
    s.textContent = ch;            // textContent, MAI innerHTML
    s.style.color = color;
    row.appendChild(s);
}
```

Gli spazi vanno preservati (`white-space: pre-wrap`) altrimenti le parole arcobaleno collassano.

---

## 6. Allegati

### Flusso upload

1. Click sul tasto allegato -> modale con area drop + `<input type="file">`.
2. Il client valida subito dimensione <= **50 MB** e mostra errore prima di sprecare banda.
3. Upload via `fetch POST /upload?name=<urlencoded>&size=<n>` con body binario raw.
4. Server:
   - rifiuta se `Content-Length > 50 MB` (**prima** di leggere il body)
   - genera `file_id = uuid4().hex`
   - salva come `attachments/<file_id>__<nome_sanitizzato>` leggendo a chunk da 64 KB (**mai tutto in RAM**)
   - verifica che i byte scritti corrispondano al dichiarato; se no cancella e risponde errore
5. Broadcast ai client dei soli metadati (~200 byte): nessun byte del file passa nello stream SSE.

### Flusso download

Chi vuole il file clicca la card -> link a `/download/<file_id>` -> il browser scarica normalmente con nome originale preservato via header:

```
Content-Disposition: attachment; filename*=UTF-8''compiti.pdf
Content-Type: application/octet-stream
```

Questo soddisfa esattamente R8: *solo chi clicca lo scarica*.

### Sicurezza allegati

| Rischio | Mitigazione |
|---|---|
| Path traversal (`../../windows/system32`) | Il nome su disco e `<uuid>__<nome>`, il nome originale serve solo per l'header di download ed e sanitizzato (`[^A-Za-z0-9._ -]` -> `_`) |
| File enorme / disco pieno host | Limite hard 50 MB + quota totale cartella (es. 500 MB) oltre cui rifiutare nuovi upload |
| Flood di upload piccoli (spam) | Rate limit semplice per cid: max N upload/minuto |
| Esecuzione automatica del file scaricato | Sempre `attachment`, mai inline; nessuna anteprima HTML/SVG |

---

## 7. Interfaccia

```
+--------------------------------------------------------------+
| Chat Hub - Stanza aperta - 7 online            (o) (o)       | <- pallini colore dx
+--------------------------------------------------------------+
|                                                              |
|  14:02 Luca                                                  |
|        ciao raga                                             |
|                                                              |
|  14:03 Sara                                                  |
|        papera papera papera                                  | <- effetto /duck attivo
|                                                              |
|  14:04 Marco                                                 |
|        [allegato] compiti.pdf - 2.3 MB - [Scarica]           | <- card cliccabile,
|                                                              |    nessun download automatico
+--------------------------------------------------------------+
| [clip] [ scrivi un messaggio... ]                    [Invia] | <- Enter invia,
+--------------------------------------------------------------+    Shift+Enter va a capo
```

Elementi UI:

- **Pallini colore**: due `<input type="color">` con CSS che li rende cerchi da ~22 px (`appearance:none`, bordo bianco). Cambio immediato sul proprio testo + salvataggio in localStorage.
- **Contatore online**: aggiornato dall'evento presence.
- **Auto-scroll intelligente**: scrolla solo se l'utente era gia in fondo; se sta leggendo sopra compare un badge "nuovi messaggi".
- **Modale allegati**: overlay centrato, area tratteggiata "trascina qui o clicca", barra di progresso upload via XHR (`fetch` non espone progress).
- **Nick prompt**: al primo accesso overlay che chiede nickname; salvato in localStorage ma riproposto se gia occupato nella stanza.

---

## 8. Problemi individuati e mitigazioni

| # | Problema reale nell'uso scolastico | Gravita | Mitigazione adottata |
|---|---|---|---|
| P1 | F5 o crash browser distrugge la stanza e la cronologia sparisce mentre tutti stanno scrivendo | Alta | Grace period 90 s + heartbeat/reap |
| P2 | Nessuna autenticazione: chiunque sulla LAN entra e puo impersonare un compagno cambiando nick nel localStorage | Media/Alta per uso scolastico reale (spam anonimo) | Vedi D1/D2 in sezione 9 |
| P3 | XSS garantito se i messaggi finiscono in HTML grezzo (`innerHTML`): basta `<img onerror=...>` per rubare localStorage o spammare alert a tutta la classe | Alta sicurezza, banale da evitare | Solo `textContent`, vedi D3 |
| P4 | Firewall Windows blocca la porta al primo avvio e i compagni vedono "sito irraggiungibile" senza capire perche | Media, molto frequente | Vedi D4 |
| P5 | I compagni devono conoscere l'IP dell'host | Media, friction all'avvio | Vedi D5 |
| P6 | Upload letto tutto in RAM o base64 nello stream SSE -> crash del processo host con video da telefono | Alta | Streaming su disco a chunk + check size pre-body |
| P7 | Path traversal nei nomi file | Alta sicurezza, banale da evitare | Nome su disco = uuid, nome originale solo cosmetico sanitizzato |
| P8 | Client zombie dopo crash browser restano "online" per sempre e la stanza non si chiude mai | Media, rompe R3 | Heartbeat 15 s + reap 45 s |
| P9 | Orologi diversi tra PC producono ordine messaggi casuale | Media, confonde la chat | `seq` progressivo assegnato dal server |
| P10 | Spam/flood allegati riempie il disco dell'host e blocca tutto per la classe intera | Media/Alta in uso reale tra ragazzi | Rate limit upload + quota totale cartella |

---

## 9. Decisioni aperte (da confermare prima del codice)

Le elenco perche cambiano davvero l'implementazione; le altre le considero deducibili e procederei senza chiedere.

**D1 - Codice stanza.** Vuoi una password/codice d'ingresso opzionale? Con ~25 persone sulla LAN scolastica probabilmente basta il nick univoco; ma se la rete e aperta anche ad altre classi serve un filtro.

**D2 - Nick duplicati.** Se qualcuno inserisce "Luca" gia presente: rifiuto con errore ("nick occupato") oppure suffisso automatico ("Luca2")?

**D3 - Rendering.** Confermo che uso esclusivamente `textContent`: niente markdown, niente link cliccabili automaticamente nei messaggi (un link malevolo cliccato dalla classe e peggio di un link non cliccabile). Se vuoi i link attivi li aggiungo solo per URL espliciti con schema http/https validato.

**D4 - Firewall.** Al primo avvio propongo di tentare automaticamente una regola netsh per la porta TCP scelta; se richiede privilegi admin fallisce silenziosamente e stampa istruzioni manuali nel terminale invece di bloccarsi.

**D5 - Scoperta host.** Stampo tutti gli IP locali all'avvio (`192.168.x.x`, ecc.) cosi scegli quello giusto; se hai piu schede di rete ti mostro quale usare come primaria.

**D6 - Allegati.** Streaming su disco a chunk fissi + check size pre-body come descritto sopra.

**D7-D10.** Mitigazioni tecniche gia definite sopra (sezione 6), nessuna scelta tua necessaria.

---

## 10. Piano di implementazione a fasi

Ogni fase e testabile isolatamente; se qualcosa si rompe sai dov'e.

1. **Fase A - Scheletro server**: HTTP server + pagina statica "Chat Hub online". Test: apri dal telefono sulla stessa Wi-Fi.
2. **Fase B - Join/nick/presence**: overlay nick, heartbeat, contatore online via SSE presence.
3. **Fase C - Messaggi base**: POST send + broadcast SSE + append DOM sicuro (`textContent`) + cronologia ultimi N al join.
4. **Fase D - Colori**: pallini fg/bg + persistenza localStorage + applicazione ai propri messaggi.
5. **Fase E - Comandi slash**: parser client-side minimale (`/duck`, `/rainbow ...`) che produce payload strutturati; logica contatori lato server.
6. **Fase F - Allegati**: modale drag&drop + upload streammato + card download-on-click.
7. **Fase G - Robustezza**: reap heartbeat, grace period/chiusura stanza, rate limit upload, quota disco.
8. **Fase H - Rifiniture UX**: auto-scroll intelligente, badge nuovi messaggi, Enter/Shift+Enter, gestione errori visibile all'utente invece che silenziosa.
9. **Fase I - Test multi-client**: due finestre normali + una incognito sull'host stesso simulano tre utenti reali senza bisogno dei compagni.

Stima complessiva ragionevole per MVP completo delle fasi A-H: circa **700-900 righe** in un singolo file Python con HTML/CSS/JS inline come stringhe costanti separate per leggibilita interna.

---

## 11. Cosa NON faro nell'MVP (per evitare scope creep)

Esplicitamente fuori scope finche non lo chiedi tu:

- persistenza su database tra sessioni diverse della stanza (la cronologia muore con la stanza)
- cifratura del traffico TLS locale (certificato self-signed = warning rosso su ogni browser della classe)
- notifiche desktop push native (`Notification API` opzionale dopo)
- moderazione/cancellazione messaggi altrui da parte dell'host (valutabile come fase J)
- emoji picker completo oltre alle papere richieste

---

## Prossimo passo

Conferma le decisioni aperte D1-D5 (sezione 9). Le altre le considero deducibili e procederei senza chiedere.
Confermate quelle posso passare alla Fase A-H direttamente nel file `scratch\hub.py`.
