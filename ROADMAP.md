# ROADMAP - Piano fatturato (origine: verdetto 2026-09-13 21:35, verdict/archive/*ratifica*)

- **P0 - Integrita' meta DB**: WAL, busy_timeout, watermark/tombstone, audit drift con reimport. -> FATTA (audit_meta, 26 righe riparate)
- **P1 - Prune automatico**: job a scrittore singolo (mai startup/shutdown), ordine backfill -> snapshot versionato -> prune, retention N snapshot. -> DA VERIFICARE/COMPLETARE
- **P2 - Comandi REPL**: /pin, /tag, /stats con validazione. -> FATTE
- **P3 - Report costi**: price_map.py (tariffe/1M, override JSON) + usage_rows(days) + comando /usage, CSV con costi etichettati "stime". -> FATTA (2026-09-13)
- **P4 - Modularizzazione**: POLICY per responsabilita', non per conta righe. -> SODDISFATTA PER COSTRUZIONE (architettura modulare: core_io, memory_meta, llm_core, tools_exec, repair_loop, price_map, ...). astral.py = orchestrator, 595 righe.
- **P5 - Bootstrap log**: conteggi sintetici scanned/imported/skipped/error. -> FATTA
- **P6 - Breathing trim**: gia' operativa, ratificata dal verdetto. -> FATTA

Ordine esecutivo ratificato: P0 -> P2,P5 -> P1 -> P3 -> P4.
