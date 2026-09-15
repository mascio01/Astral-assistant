# -*- coding: utf-8 -*-
# tools_exec.py - Schema tools OpenAI + dispatcher di esecuzione
import os
import subprocess
import tempfile

from core_io import BASE_DIR, _scan_dir_fast, cap_output
from memory_store import recall_get, recall_list, recall_save
from tools_patch import apply_code_patch, test_python_file
from exec_logger import log_execution
import time

tools = [
    {
        "type": "function",
        "function": {
            "name": "scan_storage",
            "description": "Scansiona una cartella specifica o una lista di cartelle per individuare file pesanti o file temporanei.",
            "parameters": {
                "type": "object",
                "properties": {
                    "folder_path": {"type": "string", "description": "Il percorso della cartella da scansionare"},
                    "min_size_mb": {"type": "number", "description": "Dimensione minima in MB"}
                }
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "move_to_trash",
            "description": "Sposta un file o una cartella specifica nel cestino previa conferma.",
            "parameters": {
                "type": "object",
                "properties": {
                    "path": {"type": "string", "description": "Il percorso completo del file o cartella"}
                },
                "required": ["path"]
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "run_powershell_cmd",
            "description": "Esegue un comando PowerShell in locale sul sistema.",
            "parameters": {
                "type": "object",
                "properties": {
                    "command": {"type": "string", "description": "Il comando PowerShell da eseguire"}
                },
                "required": ["command"]
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "recall",
            "description": "Recupera l'output completo di un comando precedentemente troncato (hint: [full output: recall <hash>]). Con mode='list' elenca le entry salvate.",
            "parameters": {
                "type": "object",
                "properties": {
                    "hash": {"type": "string", "description": "Hash dell'output da recuperare"},
                    "full": {"type": "boolean", "description": "Se true restituisce l'output completo senza troncamento"},
                    "mode": {"type": "string", "description": "'list' per elencare le entry salvate"}
                }
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "apply_code_patch",
            "description": "Applica modifiche chirurgiche a un file di codice tramite blocchi search/replace esatti (non regex). Accetta una lista di blocchi. Se un blocco fallisce, NESSUN blocco viene scritto (atomicita'). Usare INVECE di PowerShell per modificare codice.",
            "parameters": {
                "type": "object",
                "properties": {
                    "path": {"type": "string", "description": "Percorso completo del file da modificare"},
                    "patches": {
                        "type": "array",
                        "description": "Lista di blocchi {search: codice esatto da cercare, replace: nuovo codice}",
                        "items": {
                            "type": "object",
                            "properties": {
                                "search": {"type": "string"},
                                "replace": {"type": "string"}
                            },
                            "required": ["search", "replace"]
                        }
                    }
                },
                "required": ["path", "patches"]
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "test_python_file",
            "description": "Auto-testing silenzioso di un file Python: check sintattico (mode='compile', default) o dry-run (mode='run'). Ritorna SOLO stato + traceback troncato a 1000 caratteri. Usare INVECE di PowerShell per testare codice.",
            "parameters": {
                "type": "object",
                "properties": {
                    "path": {"type": "string", "description": "Percorso completo del file Python"},
                    "mode": {"type": "string", "description": "'compile' (default) o 'run'"},
                    "timeout": {"type": "number", "description": "Timeout in secondi (default 30)"}
                },
                "required": ["path"]
            }
        }
    }
]

def execute_tool(name, args):
    """Wrapper n8n Pattern 3: timing + log JSONL; Pattern 2: 1 retry su eccezione imprevista."""
    t0 = time.perf_counter()
    try:
        res = _execute_tool_impl(name, args)
    except Exception as e1:
        log_execution(name, args, error=e1, duration_ms=time.perf_counter() - t0)
        try:
            time.sleep(1.0)  # backoff pre-retry
            res = _execute_tool_impl(name, args)
            log_execution(name, args, duration_ms=time.perf_counter() - t0,
                          extra={"note": f"ok_dopo_retry: {e1}"})
        except Exception as e2:
            log_execution(name, args, error=e2, duration_ms=time.perf_counter() - t0)
            raise
        return res
    err = res.get("error") if isinstance(res, dict) else None
    log_execution(name, args, error=err, duration_ms=time.perf_counter() - t0)
    return res


def _execute_tool_impl(name, args):
    if not isinstance(args, dict):
        args = {}
    if name == "scan_storage":
        folder_path = args.get("folder_path")
        min_size = args.get("min_size_mb", 50)
        paths_to_check = [folder_path] if folder_path else [
            os.path.expanduser("~/Downloads"),
            os.path.expanduser("~/AppData/Local/Temp")
        ]
        results = []
        for p in paths_to_check:
            if not os.path.exists(p):
                continue
            min_bytes = min_size * 1024 * 1024
            results.extend(_scan_dir_fast(p, min_bytes))
        results.sort(key=lambda x: x[1], reverse=True)
        formatted_results = [f"{fp} ({sz:.2f} MB)" for fp, sz in results[:10]]
        payload = {"found_items": formatted_results, "count": len(results)}
        if len(results) > 10:
            full_list = "\n".join(f"{fp} ({sz:.2f} MB)" for fp, sz in results)
            rh = recall_save(full_list)
            if rh:
                payload["full_output_hint"] = f"recall {rh}"
        return payload

    elif name == "move_to_trash":
        path = args.get("path")
        if not path or not os.path.exists(path):
            return {"error": "Percorso non valido o inesistente."}
        if os.path.isdir(path):
            ps_cmd = f"Add-Type -AssemblyName Microsoft.VisualBasic; [Microsoft.VisualBasic.FileIO.FileSystem]::DeleteDirectory('{path}', 'OnlyErrorDialogs', 'SendToRecycleBin')"
        else:
            ps_cmd = f"Add-Type -AssemblyName Microsoft.VisualBasic; [Microsoft.VisualBasic.FileIO.FileSystem]::DeleteFile('{path}', 'OnlyErrorDialogs', 'SendToRecycleBin')"
        try:
            res = subprocess.run(["powershell", "-NoProfile", "-NonInteractive", "-Command", "[Console]::OutputEncoding=[System.Text.Encoding]::UTF8; " + ps_cmd], capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=15)
            if res.returncode == 0:
                return {"status": "success", "message": f"Elemento {path} spostato nel cestino."}
            else:
                return {"error": res.stderr.strip()}
        except Exception as e:
            return {"error": str(e)}

    elif name == "run_powershell_cmd":
        cmd = args.get("command")
        try:
            result = subprocess.run(["powershell", "-NoProfile", "-NonInteractive", "-Command", "[Console]::OutputEncoding=[System.Text.Encoding]::UTF8; " + cmd], capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=30)
            raw_out = result.stdout.strip()
            raw_err = result.stderr.strip()
            out, _omitted = cap_output(raw_out)
            err, _ = cap_output(raw_err)
            max_chars = 4000
            if len(out) > max_chars:
                rtk_exe = os.path.join(BASE_DIR, "rtk", "rtk.exe")
                if os.path.exists(rtk_exe):
                    try:
                        # rtk passa il comando a cmd.exe: per evitare problemi di quoting/pipe
                        # scriviamo il comando PS in un file temporaneo ed eseguiamo con -File
                        fd, tmp_ps1 = tempfile.mkstemp(suffix=".ps1")
                        with os.fdopen(fd, "w", encoding="utf-8-sig") as f:
                            f.write(cmd)
                        rtk_res = subprocess.run([rtk_exe, "summary", "powershell", "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", tmp_ps1], capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=35)
                        os.unlink(tmp_ps1)
                        if rtk_res.returncode == 0 and rtk_res.stdout.strip():
                            # Ibrido: testa dell'output (dati reali) + riassunto RTK (contestuale)
                            orig_len = len(out)
                            head = out[:2000]
                            out = head + f"\n... [Output completo di {orig_len} caratteri troncato. RTK summary:]\n" + rtk_res.stdout.strip()[:1500]
                    except Exception:
                        pass
                if len(out) > max_chars:
                    out = out[:max_chars] + f"\n... [Output PowerShell troncato a {max_chars} caratteri per risparmio token]"
            if len(err) > max_chars:
                err = err[:max_chars] + f"\n... [Errore PowerShell troncato a {max_chars} caratteri]"
            def never_worse(grezzo, troncato):
                """Mai peggio del grezzo: restituisce la versione troncata solo se non e' piu' lunga dell'output completo."""
                try:
                    if grezzo is None:
                        return troncato or ""
                    if troncato is None or len(troncato) > len(grezzo):
                        return grezzo
                    return troncato
                except Exception:
                    return (troncato or "")
            # Guard never_worse: non emettere mai piu' dell'output grezzo
            out = never_worse(raw_out, out)
            err = never_worse(raw_err, err)
            # Recall store: salva l'output completo se troncato o se il comando e' fallito
            try:
                if len(result.stdout) > max_chars or result.returncode != 0:
                    rh = recall_save(result.stdout + "\n[stderr]\n" + result.stderr, force=(result.returncode != 0))
                    if rh:
                        hint = f"\n[full output: recall {rh}]"
                        if out.strip():
                            out += hint
                        else:
                            err += hint  # stdout vuoto: l'hint deve restare visibile al modello
            except Exception:
                pass
            return {"stdout": out, "stderr": err, "exit_code": result.returncode}
        except subprocess.TimeoutExpired:
            return {"error": "Timeout del comando dopo 30 secondi."}
        except Exception as e:
            return {"error": str(e)}
    elif name == "recall":
        if args.get("mode") == "list":
            return recall_list()
        h = args.get("hash")
        if not h:
            return {"error": "Specificare hash oppure mode='list'."}
        return recall_get(h, bool(args.get("full")))
    elif name == "apply_code_patch":
        path = args.get("path")
        patches = args.get("patches", [])
        if not path or not isinstance(patches, list) or not patches:
            return {"error": "Servono 'path' e 'patches' (lista non vuota)."}
        return apply_code_patch(path, patches)
    elif name == "test_python_file":
        path = args.get("path")
        if not path:
            return {"error": "Serve 'path'."}
        return test_python_file(path, args.get("mode", "compile"), args.get("timeout", 30))
    return {"error": "Tool sconosciuto"}




# --- Nanobot-inspired: tool result offload + summary checkpoint ---

