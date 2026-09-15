# -*- coding: utf-8 -*-
"""repair_loop.py — Graph-based Self-Repair Agent for Astral.
Ispirato a snath-ai/code-repair-demo (Lár Engine pattern).

Nodi del grafo:
  1. LLMNode — diagnostica errore + genera patch
  2. ToolNode — applica patch con apply_code_patch
  3. TestNode — testa con test_python_file
  4. RouterNode — classifica errore, decide prossimo passo
  5. ClearErrorNode — resetta stato errore prima del retry

Loop: Test → Diagnosi → Patch → Clear Error → Retest (max max_attempts)
"""

from dataclasses import dataclass, field
from typing import Optional
import hashlib
import json
import os
import re
import subprocess
import sys
import time

from core_io import BASE_DIR, console, log_error


@dataclass
class SelfRepairState:
    """Stato del ciclo di auto-riparazione (GraphState)."""
    file_path: str
    mode: str = "compile"
    attempts: int = 0
    max_attempts: int = 3
    last_error: Optional[str] = None
    error_type: str = "unknown"
    fix_hint: str = ""
    patches_applied: list = field(default_factory=list)
    test_results: list = field(default_factory=list)
    audit_trail: list = field(default_factory=list)
    status: str = "pending"

    def next_attempt(self):
        self.attempts += 1

    def is_exhausted(self) -> bool:
        return self.attempts >= self.max_attempts

    def summary(self) -> str:
        return (
            f"[RepairState] file={self.file_path} mode={self.mode} "
            f"attempts={self.attempts}/{self.max_attempts} "
            f"status={self.status} error_type={self.error_type}"
        )


ERROR_PATTERNS = {
    "SyntaxError": ["SyntaxError", "invalid syntax", "unexpected EOF", "unmatched", "EOL while scanning"],
    "IndentationError": ["IndentationError", "unexpected indent", "unindent does not match"],
    "ImportError": ["ImportError", "ModuleNotFoundError", "No module named"],
    "NameError": ["NameError", "is not defined"],
    "TypeError": ["TypeError", "unsupported operand", "cannot unpack"],
    "AttributeError": ["AttributeError", "has no attribute"],
    "KeyError": ["KeyError"],
    "IndexError": ["IndexError", "list index out of range"],
    "ValueError": ["ValueError"],
    "RecursionError": ["RecursionError", "maximum recursion depth"],
    "TimeoutError": ["Timeout", "timed out"],
    "FileNotFoundError": ["FileNotFoundError", "No such file or directory"],
    "ProcessError": ["CalledProcessError", "subprocess.CalledProcessError"],
    "ZeroDivisionError": ["ZeroDivisionError", "division by zero"],
    "AssertionError": ["AssertionError"],
}


def classify_error(error_msg: str) -> str:
    """RouterNode: classifica errore per routing intelligente."""
    if not error_msg:
        return "unknown"
    for err_type, patterns in ERROR_PATTERNS.items():
        for pat in patterns:
            if pat.lower() in error_msg.lower():
                return err_type
    return "other"


FIX_HINTS = {
    "SyntaxError": "Mancano parentesi, virgole o caratteri. Controlla sintassi.",
    "IndentationError": "Indentazione errata. Usa 4 spazi per livello.",
    "ImportError": "Aggiungi import mancante o installa il pacchetto.",
    "NameError": "Variabile/funzione non definita. Controlla nome e scope.",
    "TypeError": "Tipo incompatibile. Aggiungi cast o type-check.",
    "AttributeError": "Oggetto non ha attributo. Controlla nome o API.",
    "KeyError": "Chiave mancante. Usa .get() o check presenza.",
    "IndexError": "Indice fuori range. Controlla lunghezza lista.",
    "ValueError": "Valore non valido. Controlla input.",
    "RecursionError": "Manca base case o recursione infinita.",
    "TimeoutError": "Troppo lento. Aumenta timeout o ottimizza.",
    "FileNotFoundError": "Percorso errato. Usa percorso assoluto.",
    "ProcessError": "Errore subprocess. Controlla comando e args.",
    "ZeroDivisionError": "Divisione per zero. Aggiungi b==0 check.",
    "AssertionError": "Test fallito. Controlla valori attesi.",
}


def route_fix_hint(error_type: str) -> str:
    """Restituisce hint fix per tipo errore."""
    return FIX_HINTS.get(error_type, "Errore sconosciuto. Analizza traceback.")


def _llm_diagnose(state: SelfRepairState) -> Optional[dict]:
    """LLMNode: usa LLM per diagnosticare errore e suggerire patch."""
    from llm_core import MODEL_CODE, call_with_dynamic_fallback

    file_content = ""
    if os.path.exists(state.file_path):
        try:
            with open(state.file_path, "r", encoding="utf-8") as f:
                lines = f.readlines()
                file_content = "".join(lines[-200:])
        except Exception:
            file_content = "(lettura fallita)"

    prompt = (
        f"Sei un assistente di auto-riparazione codice. Analizza errore e "
        f"proponi UNA patch search/replace.\n\n"
        f"FILE: {state.file_path}\n"
        f"ERRORE ({state.error_type}):\n{state.last_error[:2000]}\n\n"
        f"HINT: {state.fix_hint}\n\n"
        f"CONTENUTO (ultime 200 righe):\n```\n{file_content[:3000]}\n```\n\n"
        f"Formato risposta:\n"
        f"SEARCH=<codice da sostituire>\n"
        f"REPLACE=<nuovo codice>\n"
        f"Oppure: NO_FIX_KNOWN se non sai cosa correggere."
    )

    try:
        resp, used_model = call_with_dynamic_fallback(
            [{"role": "user", "content": prompt}],
            primary_model=MODEL_CODE
        )
        text = resp.choices[0].message.content.strip() if resp.choices else ""
        return {"diagnosis": text, "model": used_model}
    except Exception as e:
        log_error("repair_loop/llm_diagnose", e)
        return None


def _extract_patch_from_diagnosis(text: str) -> Optional[list]:
    """Estrae blocchi search/replace dal responso LLM."""
    if not text or "NO_FIX_KNOWN" in text:
        return None

    search_lines = []
    replace_lines = []
    mode = None  # "search" | "replace"

    for line in text.splitlines():
        stripped = line.strip()
        if stripped.upper().startswith("SEARCH"):
            mode = "search"
            val = stripped.split("=", 1)[-1].strip() if "=" in stripped else ""
            if val:
                search_lines.append(val)
            continue
        if stripped.upper().startswith("REPLACE"):
            mode = "replace"
            val = stripped.split("=", 1)[-1].strip() if "=" in stripped else ""
            if val:
                replace_lines.append(val)
            continue
        if mode == "search":
            search_lines.append(line)
        elif mode == "replace":
            replace_lines.append(line)

    if search_lines and replace_lines:
        return [{"search": "\n".join(search_lines), "replace": "\n".join(replace_lines)}]
    return None


def _is_allowed_target(path: str) -> bool:
    """Consente al riparatore di modificare soltanto codice Python del progetto."""
    try:
        root = os.path.normcase(os.path.abspath(BASE_DIR))
        target = os.path.normcase(os.path.abspath(path))
        return target.startswith(root + os.sep) and target.endswith(".py")
    except (TypeError, OSError):
        return False


def _file_snapshot(path: str):
    """Legge uno snapshot binario e il relativo hash prima della patch."""
    with open(path, "rb") as f:
        data = f.read()
    return data, hashlib.sha256(data).hexdigest()


def _restore_snapshot(path: str, data: bytes) -> bool:
    """Ripristina uno snapshot con sostituzione atomica e verifica hash."""
    tmp = path + ".repair-rollback.tmp"
    try:
        with open(tmp, "wb") as f:
            f.write(data)
            f.flush()
            os.fsync(f.fileno())
        os.replace(tmp, path)
        with open(path, "rb") as f:
            return hashlib.sha256(f.read()).hexdigest() == hashlib.sha256(data).hexdigest()
    except Exception:
        try:
            if os.path.exists(tmp):
                os.remove(tmp)
        except OSError:
            pass
        return False


def _record_repair_event(state: SelfRepairState, event: dict):
    """Registra ogni tentativo in memoria e in un audit JSONL persistente."""
    event = {"timestamp": time.time(), "file": state.file_path, **event}
    state.audit_trail.append(event)
    try:
        with open(os.path.join(BASE_DIR, ".repair_audit.jsonl"), "a", encoding="utf-8") as f:
            f.write(json.dumps(event, ensure_ascii=False, default=str) + "\n")
    except Exception:
        pass


def _result_ok(result: dict) -> bool:
    return bool(result.get("success") or result.get("status") == "ok")


def auto_repair_graph(file_path: str, mode: str = "compile", max_attempts: int = 3) -> SelfRepairState:
    """Graph-based self-repair loop completo.

    Nodi: TestNode -> RouterNode -> LLMNode -> ToolNode -> ClearErrorNode -> retry.
    """
    from tools_patch import apply_code_patch, test_python_file

    state = SelfRepairState(file_path=file_path, mode=mode, max_attempts=max_attempts)
    if not _is_allowed_target(file_path):
        state.status = "aborted"
        state.last_error = "Target fuori dal progetto Astral o non Python"
        _record_repair_event(state, {"attempt": 0, "phase": "guard", "status": "aborted"})
        return state

    while not state.is_exhausted() and state.status not in ("success", "aborted"):
        state.next_attempt()
        attempt = state.attempts

        # Nodo 1: TestNode
        console.print(f"[dim][Repair] Test #{attempt}/{max_attempts}: {file_path}[/dim]")
        result = test_python_file(file_path, mode=mode)
        state.test_results.append(result)
        _record_repair_event(state, {"attempt": attempt, "phase": "pre_patch_test",
                                     "status": "ok" if _result_ok(result) else "failed",
                                     "test": result})

        if _result_ok(result):
            state.status = "success"
            console.print(f"[green][Repair] Successo al tentativo #{attempt}![/green]")
            break

        # Nodo 2: RouterNode
        error_msg = result.get("error", "") or result.get("traceback", "")
        state.last_error = error_msg
        state.error_type = classify_error(error_msg)
        state.fix_hint = route_fix_hint(state.error_type)
        console.print(f"[dim][Repair] Errore: [orange1]{state.error_type}[/]: {error_msg[:120]}[/dim]")

        if state.is_exhausted():
            state.status = "failed"
            console.print(f"[red][Repair] Tentativi esauriti ({max_attempts}).[/red]")
            break

        # Nodo 3: LLMNode
        console.print("[dim][Repair] Diagnosi con LLM...[/dim]")
        llm_result = _llm_diagnose(state)
        if llm_result is None:
            state.status = "aborted"
            break

        diagnosis = llm_result.get("diagnosis", "")
        console.print(f"[dim][Repair] Analisi completata ({llm_result.get('model', '?')})[/dim]")

        # Nodo 4: ToolNode
        patches = _extract_patch_from_diagnosis(diagnosis)
        if not patches:
            console.print("[yellow][Repair] LLM non ha proposto patch. Abort.[/yellow]")
            state.status = "aborted"
            break

        console.print("[dim][Repair] Applicazione patch...[/dim]")
        try:
            snapshot, before_sha256 = _file_snapshot(file_path)
        except OSError as exc:
            state.status = "aborted"
            state.last_error = str(exc)
            _record_repair_event(state, {"attempt": attempt, "phase": "snapshot", "status": "aborted"})
            break
        patch_result = apply_code_patch(file_path, patches)
        state.patches_applied.append(patch_result)

        if patch_result.get("status") == "success":
            console.print("[dim][Repair] Patch applicata.[/dim]")
            test = test_python_file(file_path, mode=mode)
            state.test_results.append(test)
            if _result_ok(test):
                state.status = "success"
                _record_repair_event(state, {"attempt": attempt, "phase": "patch_test",
                                             "status": "success", "before_sha256": before_sha256,
                                             "patch": patch_result, "test": test})
                break
            rolled_back = _restore_snapshot(file_path, snapshot)
            state.last_error = (test.get("traceback", "") or test.get("error", "") or str(test))
            _record_repair_event(state, {"attempt": attempt, "phase": "patch_test",
                                         "status": "rolled_back" if rolled_back else "rollback_failed",
                                         "before_sha256": before_sha256, "patch": patch_result,
                                         "test": test, "rollback_verified": rolled_back})
            console.print("[yellow][Repair] Retest fallito: patch annullata.[/yellow]")
        else:
            _record_repair_event(state, {"attempt": attempt, "phase": "patch",
                                         "status": "failed", "before_sha256": before_sha256,
                                         "patch": patch_result})
            console.print(f"[yellow][Repair] Patch fallita: {patch_result}[/yellow]")

        # Nodo 5: ClearErrorNode — reset implicito al prossimo tentativo

    if state.status == "pending":
        state.status = "failed"
    return state


def _latest_error_block(log_path: str) -> str:
    """Recupera l'ultimo evento completo, non soltanto l'ultima riga del log."""
    with open(log_path, "r", encoding="utf-8", errors="replace") as f:
        content = f.read()[-20000:]
    starts = list(re.finditer(r"(?m)^\[\d{4}-\d{2}-\d{2} .*?\] (?:EARLY )?ERROR", content))
    return content[starts[-1].start():] if starts else content


def _target_from_traceback(error_text: str) -> str:
    """Sceglie l'ultimo frame Python appartenente al progetto Astral."""
    candidates = re.findall(r'File "([^"]+\.py)", line \d+', error_text)
    base = os.path.normcase(os.path.abspath(BASE_DIR)) + os.sep
    for candidate in reversed(candidates):
        path = os.path.abspath(candidate)
        if os.path.normcase(path).startswith(base) and os.path.isfile(path):
            return path
    return os.path.join(BASE_DIR, "astral.py")


def repair_from_log(log_path: Optional[str] = None, max_attempts: int = 3) -> SelfRepairState:
    """Ripara l'ultimo crash: diagnosi, patch atomica, test e rollback se fallisce."""
    from tools_patch import apply_code_patch, test_python_file

    log_path = log_path or os.path.join(BASE_DIR, "error_log.txt")
    state = SelfRepairState(file_path=os.path.join(BASE_DIR, "astral.py"), max_attempts=max_attempts)
    if not os.path.exists(log_path):
        state.status = "aborted"
        state.last_error = "Log errori assente"
        return state

    state.last_error = _latest_error_block(log_path)
    state.file_path = _target_from_traceback(state.last_error)
    if not _is_allowed_target(state.file_path):
        state.status = "aborted"
        state.last_error = "Target del traceback fuori dal progetto Astral o non Python"
        _record_repair_event(state, {"attempt": 0, "phase": "guard", "status": "aborted"})
        return state
    state.error_type = classify_error(state.last_error)
    state.fix_hint = route_fix_hint(state.error_type)

    while not state.is_exhausted():
        state.next_attempt()
        llm_result = _llm_diagnose(state)
        patches = _extract_patch_from_diagnosis((llm_result or {}).get("diagnosis", ""))
        if not patches:
            state.status = "aborted"
            break

        try:
            snapshot, before_sha256 = _file_snapshot(state.file_path)
        except OSError as exc:
            state.status = "aborted"
            state.last_error = str(exc)
            _record_repair_event(state, {"attempt": state.attempts, "phase": "snapshot", "status": "aborted"})
            break
        result = apply_code_patch(state.file_path, patches)
        state.patches_applied.append(result)
        if result.get("status") != "success":
            state.last_error += "\nPatch non applicabile: " + json.dumps(result, ensure_ascii=False)
            _record_repair_event(state, {"attempt": state.attempts, "phase": "patch",
                                         "status": "failed", "before_sha256": before_sha256,
                                         "patch": result})
            continue

        test = test_python_file(state.file_path, mode="compile")
        state.test_results.append(test)
        if _result_ok(test):
            state.status = "success"
            _record_repair_event(state, {"attempt": state.attempts, "phase": "patch_test",
                                         "status": "success", "before_sha256": before_sha256,
                                         "patch": result, "test": test})
            try:
                import selfmap
                selfmap.generate()
            except Exception as exc:
                log_error("repair_from_log/selfmap", exc)
            break

        rolled_back = _restore_snapshot(state.file_path, snapshot)
        _record_repair_event(state, {"attempt": state.attempts, "phase": "patch_test",
                                     "status": "rolled_back" if rolled_back else "rollback_failed",
                                     "before_sha256": before_sha256, "patch": result,
                                     "test": test, "rollback_verified": rolled_back})
        state.last_error += "\nLa patch non compila ed e' stata annullata:\n" + str(test)

    if state.status == "pending":
        state.status = "failed"
    return state


def _cli():
    if "--from-log" not in sys.argv:
        return
    lock = os.path.join(BASE_DIR, ".repair_active.lock")
    try:
        fd = os.open(lock, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
        os.write(fd, str(os.getpid()).encode("ascii"))
        os.close(fd)
    except FileExistsError:
        if time.time() - os.path.getmtime(lock) < 600:
            return
        try:
            os.remove(lock)
        except OSError:
            return
        return _cli()
    try:
        state = repair_from_log()
        with open(os.path.join(BASE_DIR, ".repair_last.json"), "w", encoding="utf-8") as f:
            json.dump({"status": state.status, "file": state.file_path,
                       "attempts": state.attempts, "error_type": state.error_type,
                       "last_error": state.last_error, "audit": state.audit_trail,
                       "patches": state.patches_applied, "tests": state.test_results,
                       "timestamp": time.time()}, f, ensure_ascii=False, indent=2)
        if state.status == "success" and "--restart" in sys.argv:
            subprocess.Popen([sys.executable, os.path.join(BASE_DIR, "astral.py")], cwd=BASE_DIR,
                             creationflags=getattr(subprocess, "CREATE_NEW_PROCESS_GROUP", 0))
    finally:
        try:
            os.remove(lock)
        except OSError:
            pass


if __name__ == "__main__":
    _cli()
