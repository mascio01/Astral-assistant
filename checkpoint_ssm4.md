# CHECKPOINT — Astral x The Sims 4 (dump/relazioni/grafo)
Ultimo aggiornamento: sessione notte 15/11/2026 (~03:00), prima dello spegnimento PC.

## OBIETTIVO
Estrarre dal save TS4 tutti i sim (anche non caricati/morti), households, quartieri e RELAZIONI,
e visualizzarli in un viewer locale (tabella dati + grafo relazioni).

## STATO VERIFICATO (fatti, non ipotesi)
- TS4_x64 NON in esecuzione al momento del checkpoint; `saves\Slot_00000003.save` NON lockato.
- Mod installata: `Documents\Electronic Arts\The Sims 4\Mods\astral_relations.ts4script`
  - size 20407 B, mtime 15/11/2026 02:51:29
  - backup presente: `astral_relations.ts4script.bak` (19932 B, 02:17)
  - contenuto zip: SOLO `astral_relations.py` (20271 B, 02:51:16) -> nessun .pyc nel pacchetto
- Sorgente di build: `C:\Users\masci\Astral\tools\build\astral_relations.py`
- Copia di lavoro/analisi: `C:\Users\masci\Astral\scratch\installed_rel.py`
- Probe separata installata: `Mods\astral_probe_c.ts4script` (1923 B, 00:25)
- `astral_dump_log.txt` riporta: "autostart: proto pronto al tentativo 1" e
  savegame_fields con conteggi reali -> sims 763, households 433, zones 415,
  neighborhoods 39, streets 43 (+ guid/save_slot/account/gameplay_data/custom_colors).
- Python 3.7 NON trovato sul sistema via `py -3.7` / `python3.7` (output vuoto):
  il pacchetto va quindi distribuito come SORGENTE .py dentro lo zip (il gioco usa il suo runtime).

## DIAGNOSI PRECEDENTE (confermata)
- Il troncamento `"<deep>"` nelle relazioni viene dal SERIALIZER dello script in-game,
  non dal file .save.
- `relgraph` esiste nello schema del save (campo 16) ma risulta VUOTO nel dump:
  il grafo nel viewer era vuoto perche' le relazioni venivano scartate in serializzazione.
- Bug noto nel decompressore RefPack usato offline: manca l'estensione di lunghezza
  (quando i bit di lunghezza sono al massimo va letto un byte extra).

## APERTI / PROSSIMI PASSI
1. Correggere il serializer delle relazioni in `tools/build/astral_relations.py`
   (evitare il troncamento `<deep>`, emettere struttura piatta o id-based).
2. Ripacchettizzare lo zip ts4script con il solo .py aggiornato e reinstallare in Mods.
3. Riavviare TS4, caricare la partita e verificare che il dump produca relgraph popolato.
4. Disambiguare la mappa eta' (enum TS4) con test empirico.
5. Fixare l'estensione di lunghezza del decompressore RefPack offline.
6. Aggiornare il viewer per leggere il nuovo formato relazioni e mostrare il grafo.

## NOTE OPERATIVE
- Il `.pyc` compilato con Python 3.13 NON e' utilizzabile dal gioco (runtime 3.7):
  caricare sempre il sorgente `.py`.
- Comando console in-game per il dump: `astral.dump`.
