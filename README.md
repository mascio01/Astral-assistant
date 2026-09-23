# Astral Assistant

**Astral** is an autonomous, self-aware AI assistant for Windows 11, written in Python. It runs as a local console application that combines an LLM conversation core with a rich set of OS-level tools (PowerShell, file system, web scraping), persistent memory, a model-routing engine, self-repair capabilities, a "council of judges" verdict system, detached subagents, and local voice input/output.

> Astral is designed to **operate on its own machine**: it can read and modify its own source code, patch itself atomically, test itself, and keep a live semantic map of its codebase (`.selfmap.json` / `.selfmap.md`).

---

## Table of Contents

- [What Astral Does](#what-astral-does)
- [Architecture Overview](#architecture-overview)
- [Codebase Structure](#codebase-structure)
  - [Core Modules](#core-modules)
  - [Memory & Persistence](#memory--persistence)
  - [Tools Layer](#tools-layer)
  - [Routing Engine](#routing-engine)
  - [Self-Repair & Self-Awareness](#self-repair--self-awareness)
  - [Verdict (Council of Judges)](#verdict-council-of-judges)
  - [Subagents](#subagents)
  - [UI & Voice](#ui--voice)
  - [Utilities & One-off Scripts](#utilities--one-off-scripts)
- [Runtime Data Files](#runtime-data-files)
- [How to Run Astral on Your Machine](#how-to-run-astral-on-your-machine)
- [Safety Notes](#safety-notes)

---

## What Astral Does

1. **Conversational LLM assistant** — talks to you via a rich terminal UI (prompt_toolkit), keeping multi-session chat history.
2. **Windows operator** — executes PowerShell commands, scans storage, moves files to trash, applies atomic code patches, scrapes the web.
3. **Smart model routing** — classifies every turn (`codice`, `conversazione`, `informativo`, ...) and routes it to the best LLM on OpenRouter, with hysteresis, tool-phase awareness, cost tracking and a personal preference score that learns from your usage.
4. **Persistent memory** — stores long-term memories, checkpoints, execution logs, telemetry and a full routing audit in local SQLite databases.
5. **Self-improvement** — can refactor and patch its own code, run syntax/dry-run tests on itself, and regenerate its own code map after every change.
6. **Quality control** — a "verdict" pipeline where a council of judge models reviews answers/code before it is accepted.
7. **Voice** — local speech-to-text via whisper.cpp (no cloud APIs) and console voice-state feedback.

---

## Architecture Overview

```
                        ┌────────────────────────────┐
   User input ────────► │  astral.py  (entry point)  │
   (text / voice)       └─────────────┬──────────────┘
                                       │
                    ┌──────────────────┼──────────────────────┐
                    ▼                  ▼                      ▼
             routing_engine.py    llm_core.py           tools_gateway.py
             (model selection)    (LLM calls, loop      (tool dispatch:
                    │             detection, repair)     exec / scrape / patch)
                    │                  │                      │
                    ▼                  ▼                      ▼
             price_map.py        memory_store.py        tools_exec.py
             budget_guard.py     history_store.py       tools_scrape.py
                                 memory_meta.py         tools_patch.py
                                        │
                                        ▼
                          SQLite: recall.db, memory_meta.db,
                          checkpoints.db, telemetry.db, config.db
```

Supporting subsystems: `repair_loop.py` (self-repair), `verdict/` (council of judges), `subagents/` (detached workers), `ui/` (design system), `stt_integration.py` + `whisper_bin/` (local voice), `selfmap.py` (code self-awareness).

---

## Codebase Structure

### Core Modules

| File | Role |
|---|---|
| **`astral.py`** (~1300 lines) | Entry point and main loop. Handles the conversation cycle: builds the system prompt (identity + state + selfmap context), routes the request, calls the LLM, executes tool calls, runs follow-ups after tools, manages voice prompts, verdict requests, meta-maintenance, and session lifecycle. |
| **`llm_core.py`** | LLM abstraction layer: talks to OpenRouter, manages streaming, retries, loop detection integration, repair hooks, and model fallbacks. |
| **`routing_engine.py`** (~880 lines) | The router. Classifies each user turn into categories (`codice`, `conversazione`, `informativo`, ...), selects the best model per category using benchmark data, applies **hysteresis** (avoids flapping between models), tracks the tool phase, and appends every decision to `.routing_audit.jsonl`. Also maintains personal preference scores (`.routing_personal_scores.json`) learned from actual usage. |
| **`astral_trim.py`** | Context-window trimming: intelligently compresses/trims conversation history to fit model context limits while preserving system prompt, recent turns and pinned content. Ships with fixtures in `test_trim_fixtures.py`. |
| **`benchmark_data.py`** | Static benchmark data (model quality per category) used by the router to rank models. |
| **`price_map.py`** | LLM pricing table (USD per 1M tokens) powering the `/usage` cost report. |
| **`budget_guard.py`** | Spending guard: blocks or warns when API cost exceeds configured budgets. |
| **`loop_detector.py`** | Detects repetitive/looping LLM behavior using memory store signals. |

### Memory & Persistence

| File | Role |
|---|---|
| **`core_io.py`** | Safe, shared I/O primitives (atomic writes, JSON helpers, path handling) used by almost every module. |
| **`memory_store.py`** | Long-term memory: save/search/recall of facts and notes (backed by `recall.db`). |
| **`memory_meta.py`** | Meta-memory layer: indexes and ranks memories, tracks usage metadata (`memory_meta.db`). |
| **`history_store.py`** | Multi-session chat history: one `chat_history_<id>.json` per session, plus `checkpoints.db` for conversation checkpoints. |
| **`exec_logger.py`** | Execution logging of tool runs and commands into `telemetry.db`. |
| **`config.db` / `config.json`** | Local configuration storage. |

### Tools Layer

| File | Role |
|---|---|
| **`tools_gateway.py`** | Tool dispatcher: receives tool calls from the LLM, validates them, routes to the right implementation, and enforces logging via `exec_logger`. |
| **`tools_exec.py`** | Executes PowerShell commands locally, runs Python files, and applies code patches. |
| **`tools_patch.py`** | Atomic search/replace code patching: a patch either applies fully or not at all (no partial writes). |
| **`tools_scrape.py`** | Self-contained web scraper: fetches URLs, respects `robots.txt`, caches results (`scrape_cache/`), extracts main content as markdown, links and JSON-LD. |

### Self-Repair & Self-Awareness

| File | Role |
|---|---|
| **`repair_loop.py`** | Graph-based self-repair agent: when a tool or code change fails, it builds a repair plan, applies patches via `tools_patch`, and re-tests. Last repair state is kept in `.repair_last.json`. |
| **`selfmap.py`** | Generates `.selfmap.json` and `.selfmap.md`: a full semantic index of the codebase (files, responsibilities, line counts, symbols, dependencies, hashes). Regenerated at every startup and after every code patch — this is Astral's "self-awareness" layer. |
| **`learn_code_repair/`** | Collected examples/data used to improve the repair loop. |

### Verdict (Council of Judges)

| File | Role |
|---|---|
| **`verdict/verdict.py`** | The council: sends a question plus a dossier of context to multiple judge models, collects independent verdicts, and synthesizes a final judgment. |
| **`verdict/schemas.py`** | Data schemas for verdict requests/results. |
| **`verdict_runner.py`** | Detached launcher so the council can run in a separate process without blocking the main loop. Output goes to `.verdict_out.txt`. |

### Subagents

| File | Role |
|---|---|
| **`subagents/roles.py`** | Defines read-only subagent roles (e.g. **scout**: analyzes project structure and integration points; **reviewer**: reviews files/diffs for bugs and risks). |
| **`subagents/jobspec.py`** | Job specifications passed to subagents. |
| **`subagents/watchdog.py`** | Watches detached subagent processes (locks, timeouts, `.watchdog.lock`). |

Subagents are budgeted, read-only helpers used for independent analysis — they never replace the main tools.

### UI & Voice

| File | Role |
|---|---|
| **`ui/tokens.py`** | Centralized design tokens (colors, styles) — design-system first. |
| **`ui/components.py`** | Reusable console UI components built on the tokens. |
| **`stt_integration.py`** | Local speech-to-text via **whisper.cpp** (binaries in `whisper_bin/`). No cloud APIs. |
| **`voice_feedback.py`** | Console voice-state indicator (listening / transcribing / idle). |
| **`astral.ico` / `Astral.spec`** | App icon and PyInstaller build spec for packaging a standalone `.exe`. |

### Utilities & One-off Scripts

| File | Role |
|---|---|
| `bench_perf.py` | Quick performance benchmark of tool execution. |
| `disco_dancer.py` | Fun/visual console animation module. |
| `_*.py` (e.g. `_inspect_db.py`, `_query_db.py`, `_search_recall.py`) | Small one-off inspection/debug scripts used during development. |
| `archive/`, `backups/`, `logs/`, `progetti/` | Archived old code, backups, runtime logs, and unrelated side projects. |

---

## Runtime Data Files

These are generated at runtime and should **not** be committed or edited by hand:

| File | Purpose |
|---|---|
| `.selfmap.json` / `.selfmap.md` | Auto-generated code map (self-awareness). |
| `.routing_audit.jsonl` | Full audit log of every routing decision. |
| `.routing_personal_scores.json` | Learned personal model preferences. |
| `bootstrap.json` / `bootstrap.md` | Auto-generated identity/state views injected into the system prompt. |
| `identity.md` / `state.md` | Stable identity and current operational state (hand-editable). |
| `chat_history_*.json` | Per-session conversation history. |
| `recall.db`, `memory_meta.db`, `checkpoints.db`, `telemetry.db`, `config.db` | SQLite stores for memory, checkpoints, telemetry and config. |
| `.env` | Your API key (never commit it — see `.env.example`). |

---

## How to Run Astral on Your Machine

### Prerequisites

- **Windows 10/11** (Astral uses Windows-specific APIs and PowerShell).
- **Git** — <https://git-scm.com/download/win>
- **Python 3.11+** (developed and tested on Python 3.13) — <https://www.python.org/downloads/>. During installation, tick **"Add python.exe to PATH"**.
- An **OpenRouter API key** — get one at <https://openrouter.ai/keys>.
- *(Optional, for voice input)* a working microphone. Note: whisper.cpp model binaries are **not** committed (too large for GitHub) — see [Voice input](#optional-voice-input) below.

### Fresh machine setup (step by step)

1. **Install Python and Git** if not already present (see prerequisites above). Verify from a new terminal:

   ```powershell
   python --version
   git --version
   ```

2. **Clone the repository**

   ```powershell
   git clone https://github.com/mascio01/Astral-assistant.git
   cd Astral-assistant
   ```

3. **(Recommended) Create and activate a virtual environment**

   ```powershell
   python -m venv .venv
   .\.venv\Scripts\Activate.ps1
   ```

   > Your prompt should now show `(.venv)`.

4. **Install dependencies**

   ```powershell
   pip install -r requirements.txt
   ```

   The only hard dependency is `prompt_toolkit` (rich terminal input). Everything else uses the Python standard library.

5. **Configure your API key**

   ```powershell
   Copy-Item .env.example .env
   notepad .env
   ```

   Then set:

   ```
   OPENROUTER_API_KEY=your_real_key_here
   ```

   Save and close. The `.env` file is git-ignored and never committed.

6. **Run Astral**

   ```powershell
   python astral.py
   ```

   On first start Astral will:
   - generate its code self-map (`.selfmap.json` / `.selfmap.md`),
   - build the bootstrap context (identity + state),
   - open the interactive console session.

7. **Talk to it.** Type requests in natural language; Astral will route them to the best model, use tools when needed (PowerShell, file operations, scraping), and keep memory across sessions.

### Verify the installation

```powershell
python test_trim_fixtures.py   # trimming unit tests
python test_routing_engine.py  # routing engine tests
python test_price_map.py       # pricing table tests
python bench_perf.py           # quick performance check
```

### Optional: voice input

Astral uses local speech-to-text via whisper.cpp (no cloud APIs). The model binaries are too large for GitHub and are **not** committed. To enable voice:

1. Download a whisper model (e.g. `ggml-base.bin`) from <https://huggingface.co/ggerganov/whisper.cpp/tree/main>.
2. Place it in `whisper_bin\Release\` (create the folder if missing).
3. Restart Astral.

### Optional: build a standalone executable

```powershell
pip install pyinstaller
pyinstaller Astral.spec
```

The executable will be created in the `dist/` folder.

### Optional: enable the git hooks

The repository ships with hooks in `.githooks/` (commit-msg, post-merge). To use them:

```powershell
git config core.hooksPath .githooks
```

### Troubleshooting

| Problem | Fix |
|---|---|
| `python` is not recognized | Reinstall Python and tick **"Add python.exe to PATH"**, or use `py` instead of `python`. |
| `pip` fails installing `prompt_toolkit` | Upgrade pip first: `python -m pip install --upgrade pip`, then retry step 4. |
| Blank screen / no output on start | Make sure `.env` exists and `OPENROUTER_API_KEY` is set; check that outbound HTTPS to `openrouter.ai` is allowed. |
| Voice input does nothing | The whisper model binary is missing — see [Voice input](#optional-voice-input). |

---

## Safety Notes

- Astral **executes real commands on your machine**. It is designed with guardrails (atomic patches, confirmation for system paths, execution logging), but you are responsible for what you let it do.
- Never commit your `.env` file or API keys. `.gitignore` already excludes sensitive/runtime files.
- The `progetti/` folder contains unrelated side projects and is not part of Astral's core.
- Runtime databases (`*.db`) and audit logs grow over time; you can safely archive or delete them when Astral is not running (it will rebuild what it needs).

---

## License & Status

Astral is a personal, actively-developed project. The `main` branch on GitHub is the published mirror of the working local version — development happens locally and is pushed when stable.
