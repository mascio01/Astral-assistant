# MAESTRO-FLOW — Analisi Completa

## Repository: catlog22/maestro-flow
## Dimensione: 69 MB (sotto limite 300 MB - SCARICATO)
## Linguaggi: TypeScript (61%), JavaScript (20%), HTML (16%), Rust (1%), Python (1%)
## Stato: Evoluzione attiva di Claude-Code-Workflow (archiviato)

---

## Architettura directory

```
maestro/
├── src/                    # Core CLI (Commander.js + MCP SDK)
│   ├── commands/           # 35+ CLI comandi
│   ├── mcp/                # MCP server (stdio)
│   ├── graph/              # Knowledge Graph (SQLite + tree-sitter)
│   ├── core/               # Tool registry, extension loading
│   ├── agents/             # Definizioni agent
│   ├── coordinator/        # Orchestrazione multi-agente
│   ├── knowledge/          # Knowledge base
│   ├── hooks/              # Sistema di hook
│   ├── db/                 # Database
│   └── cli.ts              # Entry point
├── bin/                    # Entry point CLI (maestro.js + monitor)
├── dashboard/              # Web dashboard (React 19)
├── .claude/                # Configurazioni per Claude Code
│   ├── commands/           # 64 comandi slash
│   ├── agents/             # 23 definizioni agent
│   └── skills/             # 45 skill package
├── workflows/              # 115 definizioni workflow
└── templates/              # 92 template JSON
```

## Concept chiave

| Componente | Ruolo |
|---|---|
| **Ralph v2** | Engine adattivo: classifica intento naturale in 40+ catene |
| **Knowledge Graph** | SQLite + tree-sitter per chunking codice e memoria persistente |
| **MCP Server** | Stdio-based, con connect/disconnect/run contemporanei |
| **Coordinator** | Orchestrazione multi-agente con routing consapevole |
| **CLI** | 64 comandi slash via Commander.js |
| **Dashboard** | React 19 con Vite, architettura plugin |

## Flusso di esecuzione

1. Utente esprime intento naturale
2. Ralph v2 classifica l'intento (top-3 con confidence)
3. Coordinator sceglie agente primario + agenti di supporto
4. Esecuzione pipeline con knowledge graph enrichment
5. Validazione risultati con hook post-esecuzione

## Meta-analisi

- Framework TypeScript multi-agent intent-driven
- Alternativa a sistemi orchestration tradizionali (LangChain, CrewAI)
- Punto di forza: integrazione nativa MCP + conoscenza persistente via graph
- Debolezza percepita: dipendenza da Claude Code come runtime (.claude/)
