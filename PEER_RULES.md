# PEER_RULES v1 - protocollo di collaborazione tra sessioni Astral

Nodi:
- nicolodesktop = 192.168.1.9
- pc_nicol_v    = 192.168.1.13

## 1. Indirizzamento file
Ogni file si scarica SEMPRE dall'IP del nodo che lo possiede.
Chi manda l'URL dichiara il nodo sorgente nella riga stessa:
`URL su <node> <ip>:<porta>/file/<nome>`.
Mai dare per scontato che l'URL punti all'altro PC.

## 2. Niente HEAD
Il server peer non implementa HEAD -> 501 garantito e fuorviante.
Verifica esistenza con GET reale o Range bytes=0-0.
401 = token sbagliato per QUEL nodo; 404 = file inesistente su quel nodo.

## 3. Metadati obbligatori con ogni file
nome + size byte + sha256 + URL completo + nodo sorgente.
Chi riceve verifica sha256 PRIMA di scompattare e risponde con esito esplicito.

## 4. ACK entro il turno successivo
Ogni messaggio ricevuto va riscontrato con un ACK breve al primo turno utile,
anche se il lavoro non e' finito. Silenzio = non ricevuto.

## 5. Lettura inbox dopo ogni operazione lunga
Prima di operazioni >30s: leggere la inbox.
Subito dopo averle concluse: rileggere la inbox prima di ogni altra azione.
Motivo tecnico: mother_tick() in core/relay_channel.py girava solo al prompt,
non dentro il loop dei tool -> messaggi pending durante i turni lunghi.
Fix applicato in core/repl_engine.py (blocco [PEER-DRAIN]).

## 6. Stato solo con evidenza
Nessun "risolto/fatto/funziona" senza prova:
- processo -> PID + MainWindowHandle + titolo finestra
- file -> size + sha256 verificato
- servizio -> porta in ascolto + riga di log
Se manca la prova si scrive "da verificare".

## 7. Diagnosi prima del retry
Al secondo fallimento identico ci si ferma e si cambia ipotesi dichiarandola,
invece di rilanciare lo stesso comando sperando in output diverso.
