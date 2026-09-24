# Piano di implementazione — Rilievi analisi (G01–G20, KM01–KM10)

Stato: 30 rilievi (20 G + 10 KM) verificati presenti nel codice attuale.
Ordine: per dipendenze reali, non per numero di rilievo.

## Fase 0 — Fondamenta e riproducibilità (P1/P2)
- G01 Dipendenze e avvio non riproducibili → requirements.txt completo (base + opzionali), separare creazione client LLM dall'import in llm_core.py.
- G06 Scansione selfmap include il virtualenv → escludere .venv, build, __pycache__, .git, dist in selfmap.py.
- G07 Runner test dipende dal nome `python` → usare sys.executable in task_tracker.py.
- G10 Documentazione operativa da riallineare → README: budget guard, core_io, knowledge map.
- Verifica: test_python_file compile su file toccati + suite pytest mirata.

## Fase 1 — Knowledge map (P1/P2)
- KM01 refresh perde personalizzazioni; KM02 fonti mancanti senza stato degradato; KM03 freschezza timestamp; KM04 corruzione JSON; KM05 ricerca tool; KM06 limite record/payload; KM07 confinamento fonti; KM08 scritture concorrenti; KM09 topic non allineati; KM10 diagnosi e test.
- G03 Knowledge map: conservazione e stato dati incompleti (sovrapposto a KM01–KM04).
- Verifica: test dedicati knowledge map + migrazione dati manuali senza perdita.

## Fase 2 — Prezzi e benchmark (P1)
- G02 Benchmark scaricati con nomi incompatibili con il lettore → normalizzazione nomi in benchmark_data.py.
- G04 Precedenza dei prezzi diversa da documentata → allineare price_map.py alla documentazione.
- Verifica: test_price_map.py + test benchmark.

## Fase 3 — Esecuzione e tool (P1)
- G11 riassunto PowerShell richiede seconda esecuzione; G12 fallback LLM ignora esito incerto; G13 gateway evita anti-loop; G15 esiti falliti trattati come successi; G18 crash runner verdetti appare come successo; G19 taskkill fallito dichiarato riuscito.
- Verifica: test_astral_trim.py + test dedicati esecuzione.

## Fase 4 — Memoria e meta-memoria (P1)
- G14 logger perde aggiornamenti meta-memoria; G16 retention elimina e reimporta; G17 stato test passati sopravvive a fallimento; G20 manutenzione memoria inattiva.
- Verifica: test memoria + retention.

## Fase 5 — Architettura e UI (P2)
- G05 atomicità patch limitata alla validazione; G08 UI package non esportata; G09 errori silenziosi e responsabilità concentrate.
- Verifica: test_audit_fixes.py + suite completa.

## Fase 6 — Verifica finale
- Suite pytest completa (atteso 145/146 + 20/20 fixture), selfmap rigenerata, README allineato, commit e push su origin/main.
