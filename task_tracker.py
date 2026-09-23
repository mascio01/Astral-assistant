# -*- coding: utf-8 -*-
# task_tracker.py - Registro task SQLite (verdetto consiglio: ibrido minimale)
# Git = fonte unica per "cosa e' integrato"; questa tabella tiene SOLO i campi
# NON derivabili da git: titolo, stato funzionale, esito test verificato, timestamp.
#
# Fix test_passed: il flag non e' una dichiarazione, ma un fatto verificato.
# mark_test_passed() esegue la suite reale (test_*.py) e registra l'hash dei
# file di test al momento del passaggio. mark_integrated() verifica che la
# suite corrente sia identica a quella registrata, altrimenti avvisa.

import hashlib
import os
import re
import sqlite3
import subprocess
import threading
from datetime import datetime, timezone

from core_io import BASE_DIR, log_error

# Stesso DB di memory_meta: un solo file, zero dipendenze nuove.
TASKS_DB = os.path.join(BASE_DIR, "memory_meta.db")
_LOCK = threading.RLock()

# Convenzioni git (policy --no-ff, vedi verdetto):
#   branch: task/<id>-slug
#   commit: feat(#<id>): ...
_BRANCH_RE = re.compile(r"^task/(\d+)(?:-|$)")
# NB: senza '^' per poter usare .search() sulle righe di 'git log --oneline'
# (che iniziano con l'hash). La validazione del messaggio usa .match().
_COMMIT_MSG_RE = re.compile(r"(?:feat|fix|chore|docs|refactor|test|build|ci)\(#(\d+)\)", re.IGNORECASE)

_DDL = """CREATE TABLE IF NOT EXISTS tasks (
    id INTEGER PRIMARY KEY,
    title TEXT NOT NULL,
    status TEXT CHECK(status IN ('pending','in_progress','integrated')) DEFAULT 'pending',
    test_passed INTEGER DEFAULT 0,
    test_suite_hash TEXT,
    test_commit TEXT,
    ts_test_passed TEXT,
    ts_created TEXT DEFAULT (datetime('now')),
    ts_integrated TEXT
)"""


def _conn():
    c = sqlite3.connect(TASKS_DB, timeout=10)
    c.execute("PRAGMA foreign_keys=ON")
    c.execute("PRAGMA journal_mode=WAL")
    c.execute("PRAGMA busy_timeout=10000")
    return c


def _now_utc():
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def _git(*args, cwd=None):
    """Esegue git; ritorna (ok, stdout). Non lancia mai eccezioni."""
    try:
        r = subprocess.run(
            ["git"] + list(args),
            capture_output=True, text=True, encoding="utf-8", errors="replace",
            cwd=cwd or BASE_DIR, timeout=20,
        )
        return r.returncode == 0, r.stdout.strip()
    except Exception as e:
        return False, str(e)


def init_tasks():
    """Crea la tabella tasks e applica migrazioni idempotenti."""
    with _LOCK:
        c = _conn()
        try:
            c.execute("BEGIN IMMEDIATE")
            c.execute(_DDL)
            # Migrazione: aggiungi colonne mancanti su tabelle preesistenti.
            cols = {r[1] for r in c.execute("PRAGMA table_info(tasks)").fetchall()}
            if "test_commit" not in cols:
                c.execute("ALTER TABLE tasks ADD COLUMN test_commit TEXT")
            c.execute("CREATE INDEX IF NOT EXISTS idx_tasks_status ON tasks(status)")
            c.commit()
        finally:
            c.close()


def _is_test_source(fn):
    """True per ogni file che fa parte del CODICE di test.

    Non basta 'test_*.py': il supporto condiviso (_test_support.py) contiene
    fixture e assert helper, quindi una sua modifica puo' cambiare l'esito
    della suite. Se non entrasse nell'hash, mark_integrated() non se ne
    accorgerebbe e test_passed resterebbe verde su una suite diversa.
    """
    return (fn.endswith(".py") and (fn.startswith("test_") or fn.startswith("_test_")))


def _suite_hash():
    """Hash di TUTTO il codice di test nella cartella progetto.

    Se cambia un test (o il suo supporto condiviso) dopo il passaggio, l'hash
    non coincide piu' e mark_integrated() segnala che il flag va rivalidato.
    """
    h = hashlib.sha256()
    found = 0
    try:
        for fn in sorted(os.listdir(BASE_DIR)):
            if _is_test_source(fn):
                p = os.path.join(BASE_DIR, fn)
                with open(p, "rb") as f:
                    h.update(fn.encode("utf-8"))
                    h.update(b"\0")
                    h.update(f.read())
                found += 1
    except Exception:
        pass
    h.update(str(found).encode())
    return h.hexdigest()[:16]


def _test_files():
    """Elenco dei file test_*.py da ESEGUIRE nella cartella progetto.

    Il supporto condiviso non e' eseguibile: entra nell'hash (_suite_hash) ma
    non in questo elenco.
    """
    try:
        return sorted(fn for fn in os.listdir(BASE_DIR)
                      if fn.startswith("test_") and fn.endswith(".py"))
    except Exception:
        return []


def _run_tests():
    """Esegue la suite reale e ritorna (ok, dettaglio).

    Strategia a cascata, perche' il progetto puo' avere test pytest,
    unittest o script standalone:
      1) pytest, se installato;
      2) unittest discover, se trova test;
      3) esecuzione diretta di ogni test_*.py (exit code 0 = verde).
    Il fix test_passed funziona quindi anche senza dipendenze extra.
    """
    try:
        import importlib.util
        has_pytest = importlib.util.find_spec("pytest") is not None
    except Exception:
        has_pytest = False
    if has_pytest:
        try:
            r = subprocess.run(
                ["python", "-m", "pytest", "-q", "--tb=short"],
                capture_output=True, text=True, encoding="utf-8",
                errors="replace", cwd=BASE_DIR, timeout=180,
            )
            tail = (r.stdout + r.stderr).strip().splitlines()
            if r.returncode == 0:
                return True, tail[-1:] or ["ok"]
            # pytest presente ma nessun test raccolto -> prova gli script
            if "no tests ran" not in (r.stdout or "").lower():
                return False, tail[-3:]
        except Exception:
            pass
    # Fallback: esegui direttamente ogni test_*.py come script.
    files = _test_files()
    if not files:
        return False, ["nessun file test_*.py trovato"]
    failed = []
    for fn in files:
        try:
            r = subprocess.run(
                ["python", fn], capture_output=True, text=True,
                encoding="utf-8", errors="replace", cwd=BASE_DIR, timeout=120,
            )
            if r.returncode != 0:
                failed.append(f"{fn}: exit {r.returncode}")
        except Exception as e:
            failed.append(f"{fn}: {e}")
    if failed:
        return False, failed
    return True, [f"{len(files)} file test ok"]


def create_task(task_id, title):
    """Crea la task e il branch task/<id>-slug."""
    task_id = int(task_id)
    title = (title or "").strip()
    if not title:
        return {"error": "Serve un titolo."}
    with _LOCK:
        c = _conn()
        try:
            c.execute("BEGIN IMMEDIATE")
            c.execute(
                "INSERT OR IGNORE INTO tasks(id, title) VALUES(?,?)",
                (task_id, title),
            )
            c.commit()
        finally:
            c.close()
    # Slug dal titolo (ASCII-safe, decorativo: l'ID e' la chiave).
    slug = re.sub(r"[^a-z0-9]+", "-", title.lower()).strip("-")[:24] or "task"
    branch = f"task/{task_id}-{slug}"
    ok, out = _git("checkout", "-b", branch)
    if not ok:
        # Branch gia' esistente: non e' un errore.
        ok2, _ = _git("checkout", branch)
        if not ok2:
            return {"error": f"Branch non creato: {out}"}
    return {"ok": True, "id": task_id, "branch": branch}


def mark_in_progress(task_id, commit_hash=None):
    """Passa la task a in_progress (commit su branch non integrato)."""
    with _LOCK:
        c = _conn()
        try:
            c.execute("BEGIN IMMEDIATE")
            cur = c.execute(
                "UPDATE tasks SET status='in_progress' WHERE id=? AND status!='integrated'",
                (int(task_id),),
            )
            c.commit()
            return {"ok": True, "updated": cur.rowcount > 0}
        finally:
            c.close()


def mark_test_passed(task_id, run_tests=True):
    """Esegue la suite reale; se verde, registra test_passed=1.

    Fix rafforzamento: il flag non e' dichiarato ma VERIFICATO. Registra:
      - test_suite_hash: hash dei file test_*.py al momento del passaggio;
      - test_commit: HEAD del codice testato (ancoraggio a uno stato preciso).
    Se run_tests e' False (es. test manuali) registra comunque l'hash ma
    marca verified=False, cosi' il chiamante sa che non e' stato eseguito.
    """
    ok = True
    detail = []
    if run_tests:
        ok, detail = _run_tests()
    if not ok:
        return {"ok": False, "error": "Test falliti", "detail": detail}
    suite_hash = _suite_hash()
    okc, commit = _git("rev-parse", "HEAD")
    commit = commit if okc else None
    with _LOCK:
        c = _conn()
        try:
            c.execute("BEGIN IMMEDIATE")
            c.execute(
                "UPDATE tasks SET test_passed=1, test_suite_hash=?, test_commit=?, "
                "ts_test_passed=? WHERE id=?",
                (suite_hash, commit, _now_utc(), int(task_id)),
            )
            c.commit()
        finally:
            c.close()
    return {"ok": True, "id": int(task_id), "suite_hash": suite_hash,
            "commit": commit, "detail": detail, "verified": run_tests}


def mark_integrated(task_id):
    """Segna la task integrata. Verifica che test_passed sia valido:
    se la suite corrente non coincide con quella registrata, avvisa."""
    task_id = int(task_id)
    with _LOCK:
        c = _conn()
        try:
            row = c.execute(
                "SELECT test_passed, test_suite_hash, test_commit FROM tasks WHERE id=?",
                (task_id,),
            ).fetchone()
            if not row:
                return {"error": f"Task {task_id} inesistente."}
            test_passed, suite_hash, test_commit = row
            warnings = []
            if not test_passed:
                warnings.append("test_passed=0: integrazione senza test verdi.")
            else:
                cur_hash = _suite_hash()
                if suite_hash and suite_hash != cur_hash:
                    warnings.append(
                        "La suite di test e' cambiata dopo il passaggio: "
                        "rivalida con /task-test."
                    )
                # Ancoraggio al commit: se il codice e' cambiato dopo il test,
                # il flag non copre piu' lo stato corrente.
                okc, head = _git("rev-parse", "HEAD")
                if test_commit and okc and head != test_commit:
                    warnings.append(
                        f"Il codice e' cambiato dopo il test "
                        f"({test_commit[:7]} -> {head[:7]}): rivalida con /task-test."
                    )
            c.execute("BEGIN IMMEDIATE")
            c.execute(
                "UPDATE tasks SET status='integrated', ts_integrated=? WHERE id=?",
                (_now_utc(), task_id),
            )
            c.commit()
            return {"ok": True, "id": task_id, "warnings": warnings}
        finally:
            c.close()


def list_active():
    """Elenco task non integrate (o tutte con flag)."""
    with _LOCK:
        c = _conn()
        try:
            rows = c.execute(
                "SELECT id, title, status, test_passed, ts_created, ts_integrated "
                "FROM tasks ORDER BY id"
            ).fetchall()
            return [{
                "id": r[0], "title": r[1], "status": r[2],
                "test_passed": bool(r[3]), "ts_created": r[4],
                "ts_integrated": r[5],
            } for r in rows]
        finally:
            c.close()


def get_task(task_id):
    """Dettaglio di una singola task."""
    with _LOCK:
        c = _conn()
        try:
            row = c.execute(
                "SELECT id, title, status, test_passed, test_suite_hash, "
                "test_commit, ts_test_passed, ts_created, ts_integrated "
                "FROM tasks WHERE id=?",
                (int(task_id),),
            ).fetchone()
            if not row:
                return {"error": f"Task {task_id} inesistente."}
            return {
                "id": row[0], "title": row[1], "status": row[2],
                "test_passed": bool(row[3]), "test_suite_hash": row[4],
                "test_commit": row[5], "ts_test_passed": row[6],
                "ts_created": row[7], "ts_integrated": row[8],
            }
        finally:
            c.close()


def _branch_integrato():
    """Branch di integrazione: main se esiste, altrimenti master.

    Hardcodare "main" faceva fallire in silenzio la riconciliazione su un repo
    col branch di default "master": git log usciva con errore, ok=False, e il
    report non segnalava nulla (nessuna task veniva marcata integrata).
    """
    for nome in ("main", "master"):
        ok, _ = _git("rev-parse", "--verify", "--quiet", nome)
        if ok:
            return nome
    return None


def sync_from_git():
    """Riconcilia i branch task/<id> reali con il registro.

    - Branch task/<id> esistente senza riga  -> crea riga pending
    - Commit con #id raggiungibile dal branch integrato -> status integrated
    - Riga pending/in_progress senza branch  -> segnala (branch cancellato)
    Ritorna un report delle divergenze.
    """
    init_tasks()
    report = {"created": [], "integrated": [], "warnings": []}
    # 1) Branch locali task/<id>
    ok, out = _git("branch", "--list", "task/*")
    if ok:
        for line in out.splitlines():
            m = _BRANCH_RE.match(line.strip().lstrip("* "))
            if not m:
                continue
            tid = int(m.group(1))
            with _LOCK:
                c = _conn()
                try:
                    row = c.execute("SELECT id FROM tasks WHERE id=?", (tid,)).fetchone()
                    if not row:
                        c.execute(
                            "INSERT OR IGNORE INTO tasks(id, title) VALUES(?,?)",
                            (tid, f"Task {tid} (da branch)"),
                        )
                        report["created"].append(tid)
                    c.commit()
                finally:
                    c.close()
    # 2) Commit con #id raggiungibili dal branch integrato (integrazione reale)
    integrato = _branch_integrato()
    if integrato:
        ok, out = _git("log", "--oneline", integrato, "--grep=#")
    else:
        ok, out = False, ""
        report["warnings"].append(
            "branch di integrazione non trovato (ne' main ne' master): "
            "integrazioni non verificate")
    if ok:
        for line in out.splitlines():
            m = _COMMIT_MSG_RE.search(line)
            if not m:
                continue
            tid = int(m.group(1))
            with _LOCK:
                c = _conn()
                try:
                    c.execute("BEGIN IMMEDIATE")
                    cur = c.execute(
                        "UPDATE tasks SET status='integrated', ts_integrated=? "
                        "WHERE id=? AND status!='integrated'",
                        (_now_utc(), tid),
                    )
                    c.commit()
                    if cur.rowcount:
                        report["integrated"].append(tid)
                finally:
                    c.close()
    # 3) Righe pending/in_progress senza branch corrispondente
    #    Il branch e' task/<id>-slug: confronto per prefisso, non per nome esatto.
    branch_ids = set()
    ok, out = _git("branch", "--list", "task/*")
    if ok:
        for line in out.splitlines():
            m = _BRANCH_RE.match(line.strip().lstrip("* "))
            if m:
                branch_ids.add(int(m.group(1)))
    for t in list_active():
        if t["status"] == "integrated":
            continue
        if t["id"] not in branch_ids:
            report["warnings"].append(
                f"task {t['id']} '{t['title']}' senza branch task/{t['id']}*"
            )
    return report


def validate_commit_msg(msg):
    """Hook commit-msg: True se il messaggio INIZIA con un ID task valido."""
    return bool(_COMMIT_MSG_RE.match((msg or "").lstrip()))


def extract_task_id(msg):
    """Estrae l'ID task da un messaggio di commit, o None."""
    m = _COMMIT_MSG_RE.search(msg or "")
    return int(m.group(1)) if m else None

