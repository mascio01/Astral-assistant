# -*- coding: utf-8 -*-
"""Gestione del ciclo di vita delle sessioni Astral (P1 audit azione ciclo1).

Estratto da ``astral.py`` per ridurre il monolite. Contiene:
  - lo slot/colore stabile per processo (marker ``.astral_sessions.lock``);
  - il lock inter-processo sul marker;
  - il check di vita del PID;
  - il ciclo di vita del driver CUA legato alle sessioni.

Nessuna dipendenza da ``astral``: il modulo e' foglia e importabile dai test.
"""
import os
import subprocess

from core.core_io import _early_log, log_error

MAX_SESSIONS = 5  # cap sessioni simultanee
# Palette stabile per distinguere le sessioni concorrenti nel terminale.
SESSION_COLORS = (
    "dodger_blue1",
    "medium_purple1",
    "spring_green1",
    "dark_orange",
    "bright_cyan",
)

# Il driver CUA non deve partire al logon: viene gestito insieme al ciclo di vita
# delle sessioni Astral. La task "cua-driver-serve" resta quindi disabilitata.
CUA_DRIVER_EXE = os.path.join(
    os.environ.get("LOCALAPPDATA", os.path.expanduser("~\\AppData\\Local")),
    "Programs", "Cua", "cua-driver", "bin", "cua-driver.exe",
)


def cua_driver_running():
    """Ritorna True se il processo CUA driver e' gia' attivo."""
    try:
        flags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
        result = subprocess.run(
            ["tasklist", "/FI", "IMAGENAME eq cua-driver.exe", "/FO", "CSV", "/NH"],
            capture_output=True, text=True, timeout=3,
            creationflags=flags,
        )
        return "cua-driver.exe" in (result.stdout or "").lower()
    except Exception:
        return False


def start_cua_driver():
    """Avvia il driver solo quando esiste almeno una sessione Astral."""
    if cua_driver_running() or not os.path.isfile(CUA_DRIVER_EXE):
        return
    try:
        flags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
        subprocess.Popen(
            [CUA_DRIVER_EXE, "serve"],
            cwd=os.path.expanduser("~"),
            stdin=subprocess.DEVNULL,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            creationflags=flags,
        )
    except Exception as e:
        _early_log("start cua-driver", e)


def stop_cua_driver_if_idle():
    """Chiude il driver quando l'ultima sessione Astral e' terminata."""
    if not cua_driver_running():
        return
    try:
        flags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
        # [FIX G19] Il returncode di taskkill va controllato: se fallisce (es.
        # accesso negato) il driver resta vivo e NON va dichiarato chiuso.
        res = subprocess.run(
            ["taskkill", "/IM", "cua-driver.exe", "/T", "/F"],
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
            timeout=5, creationflags=flags,
        )
        if res.returncode != 0 and cua_driver_running():
            _early_log("stop cua-driver", RuntimeError(
                "taskkill fallito (codice %s): il driver risulta ancora attivo" % res.returncode))
    except Exception as e:
        _early_log("stop cua-driver", e)


def pid_alive(pid):
    """Check safe su Windows: OpenProcess (QUERY_LIMITED) invece di os.kill
    (che con sig!=CTRL_* chiama TerminateProcess e ucciderebbe il target!)."""
    try:
        import ctypes
        h = ctypes.windll.kernel32.OpenProcess(0x1000, False, int(pid))  # PROCESS_QUERY_LIMITED_INFORMATION
        if not h:
            return False
        ctypes.windll.kernel32.CloseHandle(h)
        return True
    except Exception:
        return False


def claim_session_slot(base_dir=None):
    """Assegna uno slot stabile (e quindi un colore) a questo processo.

    Il marker conserva ``slot pid`` invece della sola lista dei PID: cosi', se
    la sessione blu termina mentre quella magenta resta aperta, la nuova
    sessione rioccupa il blu e non ricolora erroneamente quella magenta.

    Ritorna lo slot assegnato oppure -1 se il cap e' raggiunto o in caso di
    errore (fail-closed: mai ripiegare sullo slot 0).
    """
    import atexit

    base_dir = base_dir or os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    marker = os.path.join(base_dir, ".astral_sessions.lock")
    mutex = marker + ".mutex"
    my = os.getpid()

    try:
        import msvcrt
    except ImportError:  # pragma: no cover - Astral gira normalmente su Windows
        msvcrt = None

    def _locked_update(callback):
        """Serializza lettura/scrittura del marker tra processi Windows."""
        if msvcrt is None:
            return callback()
        with open(mutex, "a+b") as lock:
            lock.seek(0, os.SEEK_END)
            if lock.tell() == 0:
                lock.write(b"0")
                lock.flush()
            lock.seek(0)
            msvcrt.locking(lock.fileno(), msvcrt.LK_LOCK, 1)
            try:
                return callback()
            finally:
                lock.seek(0)
                msvcrt.locking(lock.fileno(), msvcrt.LK_UNLCK, 1)

    def _read_records():
        """Legge anche il vecchio formato, migrandolo al primo avvio."""
        records = []
        if not os.path.exists(marker):
            return records
        with open(marker, "r", encoding="utf-8") as f:
            for line in f:
                parts = line.split()
                if len(parts) >= 2 and parts[0].isdigit() and parts[1].isdigit():
                    records.append((int(parts[0]), int(parts[1])))
                elif len(parts) == 1 and parts[0].isdigit():
                    # Compatibilita' con il marker precedente: slot ancora da assegnare.
                    records.append((None, int(parts[0])))
        return records

    def _write_records(records):
        with open(marker, "w", encoding="utf-8") as f:
            f.write("\n".join(f"{slot} {pid}" for slot, pid in records))

    def _active_records():
        records = _read_records()
        active = []
        used = set()
        # Prima preserva gli slot gia' espliciti.
        for slot, pid in records:
            if (slot is not None and 0 <= slot < MAX_SESSIONS
                    and slot not in used and pid_alive(pid)):
                active.append((slot, pid))
                used.add(slot)
        # Poi assegna i vecchi record senza slot ai primi slot liberi.
        for slot, pid in records:
            if slot is None and pid_alive(pid):
                free = next((i for i in range(MAX_SESSIONS) if i not in used), None)
                if free is None:
                    break
                active.append((free, pid))
                used.add(free)
        return sorted(active)

    try:
        def _claim():
            active = _active_records()  # ripulisce anche i PID gia' terminati
            used = {slot for slot, _pid in active}
            free = next((i for i in range(MAX_SESSIONS) if i not in used), None)
            if free is None:
                _write_records(active)
                return -1
            active.append((free, my))
            _write_records(sorted(active))
            return free

        slot = _locked_update(_claim)
        if slot < 0:
            return -1

        def _cleanup():
            try:
                def _release():
                    active = _active_records()
                    _write_records([
                        record for record in active
                        if record != (slot, my)
                    ])
                _locked_update(_release)
            except Exception:
                pass

        def _cleanup_with_driver():
            _cleanup()
            # Il marker e' gia' stato aggiornato: se non resta alcuna sessione,
            # il driver puo' essere chiuso senza interferire con altre sessioni.
            try:
                if not _active_records():
                    stop_cua_driver_if_idle()
            except Exception:
                pass

        atexit.register(_cleanup_with_driver)
        start_cua_driver()
        return slot
    except Exception as e:
        # [FIX A-07] Fail-closed: in caso di errore NON ripieghiamo sullo slot 0
        # (piu' processi si comporterebbero come la stessa sessione).
        # Ritorniamo -1: il chiamante blocca l'avvio e l'errore viene loggato.
        try:
            log_error("claim_session_slot", e)
        except Exception:
            pass
        return -1


def session_color(slot):
    """Colore stabile associato allo slot; grigio se lo slot non e' valido."""
    try:
        idx = int(slot) % len(SESSION_COLORS)
    except Exception:
        idx = 0
    return SESSION_COLORS[idx]