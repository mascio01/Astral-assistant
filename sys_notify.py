# -*- coding: utf-8 -*-
"""sys_notify.py - Notifiche di sistema (toast Windows) per avvisi importanti.

Regole di progetto:
  * Si notifica SOLO se la console/finestra di Astral NON e' in primo piano:
    se stai gia' guardando Astral non arriva nessun disturbo.
  * Best-effort assoluto: nessuna eccezione puo' risalire al chiamante e il
    REPL non viene mai bloccato (invio in thread daemon).
  * Debounce per testo (lo stesso avviso non ripete un toast a raffica) e
    cooldown globale minimo fra due toast qualsiasi.
  * Fuori da Windows: no-op silenzioso.

Uso:
    import sys_notify
    sys_notify.notify("Attenzione", "Budget token del turno superato")
"""

import base64
import os
import subprocess
import threading
import time

IS_WINDOWS = os.name == "nt"

# --- configurazione -------------------------------------------------------
APP_ID = "Astral"                 # AppUserModelId dedicato (registrato in HKCU)
DEBOUNCE_SAME_S = 20.0            # stesso (titolo,messaggio) entro N secondi: ignorato
COOLDOWN_ANY_S = 2.0              # distanza minima fra due toast qualsiasi
PS_TIMEOUT_S = 10                 # timeout del processo PowerShell di appoggio
_MSG_CAP = 220                    # lunghezza massima del corpo notifica

_lock = threading.Lock()
_last_same = {}                   # chiave -> timestamp ultimo invio
_last_any = [0.0]

# --- rilevamento focus ----------------------------------------------------
_SELF_PID = os.getpid()
_STOP_AT_SHELL = {"explorer.exe", "winlogon.exe", "services.exe", "svchost.exe",
                  "csrss.exe", "wininit.exe"}


def _wintypes():
    import ctypes
    import ctypes.wintypes as w
    return ctypes, w


def _fg_info():
    """Ritorna (hwnd, pid, titolo) della finestra in primo piano, o (0,0,'').

    Non solleva mai: in caso di problemi ritorna valori vuoti.
    """
    if not IS_WINDOWS:
        return 0, 0, ""
    try:
        ctypes, w = _wintypes()
        u = ctypes.WinDLL("user32", use_last_error=True)
        u.GetForegroundWindow.restype = w.HWND
        u.GetWindowThreadProcessId.restype = w.DWORD
        hwnd = u.GetForegroundWindow()
        if not hwnd:
            return 0, 0, ""
        pid = w.DWORD(0)
        u.GetWindowThreadProcessId(hwnd, ctypes.byref(pid))
        buf = ctypes.create_unicode_buffer(512)
        u.GetWindowTextW(hwnd, buf, 512)
        return int(hwnd), int(pid.value), buf.value
    except Exception:
        return 0, 0, ""


def _console_title():
    """Titolo della console corrente (funziona anche sotto Windows Terminal)."""
    if not IS_WINDOWS:
        return ""
    try:
        ctypes, w = _wintypes()
        k = ctypes.WinDLL("kernel32", use_last_error=True)
        buf = ctypes.create_unicode_buffer(512)
        k.GetConsoleTitleW(buf, 512)
        return buf.value
    except Exception:
        return ""


def _ancestor_pids():
    """Catena dei pid antenati del processo corrente, fino alla shell esclusa."""
    if not IS_WINDOWS:
        return set()
    try:
        ctypes, w = _wintypes()
        k = ctypes.WinDLL("kernel32", use_last_error=True)

        class PE32(ctypes.Structure):
            _fields_ = [
                ("dwSize", w.DWORD), ("cntUsage", w.DWORD), ("th32ProcessID", w.DWORD),
                ("th32DefaultHeapID", ctypes.POINTER(ctypes.c_ulong)), ("th32ModuleID", w.DWORD),
                ("cntThreads", w.DWORD), ("th32ParentProcessID", w.DWORD),
                ("pcPriClassBase", ctypes.c_long), ("dwFlags", w.DWORD),
                ("szExeFile", ctypes.c_char * 260),
            ]

        TH32CS_SNAPPROCESS = 0x2
        snap = k.CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0)
        if not snap:
            return set()
        pe = PE32()
        pe.dwSize = ctypes.sizeof(PE32)
        table = {}
        if k.Process32First(snap, ctypes.byref(pe)):
            while True:
                table[pe.th32ProcessID] = (pe.th32ParentProcessID,
                                           pe.szExeFile.decode("mbcs", "replace"))
                if not k.Process32Next(snap, ctypes.byref(pe)):
                    break
        k.CloseHandle(snap)

        chain = set()
        pid = _SELF_PID
        for _ in range(25):
            if pid not in table or pid in chain:
                break
            parent, name = table[pid]
            if name.lower() in _STOP_AT_SHELL:
                break
            chain.add(pid)
            if parent == 0 or parent == pid:
                break
            pid = parent
        return chain
    except Exception:
        return set()


def has_focus():
    """True se Astral (o il suo terminale/console) e' in primo piano.

    Tre prove indipendenti, in ordine di affidabilita':
      1. il processo in primo piano e' Astral stesso;
      2. il processo in primo piano e' un suo antenato (cmd/powershell/py/...);
      3. il titolo della finestra in primo piano coincide con quello della
         console (necessario sotto Windows Terminal, dove GetConsoleWindow
         restituisce una pseudo-console con pid diverso).
    """
    fg_hwnd, fg_pid, fg_title = _fg_info()
    if not fg_hwnd:
        # Impossibile determinare: prudenza -> NON notificare.
        return True
    if fg_pid == _SELF_PID:
        return True
    if fg_pid in _ancestor_pids():
        return True
    ctitle = _console_title().strip().lower()
    if ctitle and fg_title.strip().lower() == ctitle:
        return True
    return False


# --- invio toast ----------------------------------------------------------
def _toast_command(title, message, app_id=APP_ID):
    """Costruisce il comando PowerShell che mostra il toast.

    Titolo e corpo viaggiano in base64: nessuna interpolazione/escaping
    possibile, quindi nessun rischio di injection da testo arbitrario.
    """
    t = base64.b64encode(title.encode("utf-8")).decode("ascii")
    m = base64.b64encode(message.encode("utf-8")).decode("ascii")
    return (
        "[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType=WindowsRuntime]|Out-Null;"
        "$t=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('%s'));"
        "$m=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('%s'));"
        "$x=New-Object Windows.Data.Xml.Dom.XmlDocument;"
        "$x.LoadXml('<toast><visual><binding template=\"ToastGeneric\"><text></text><text></text></binding></visual></toast>');"
        "$n=$x.GetElementsByTagName('text');"
        "$n.Item(0).AppendChild($x.CreateTextNode($t))|Out-Null;"
        "$n.Item(1).AppendChild($x.CreateTextNode($m))|Out-Null;"
        "$toast=[Windows.UI.Notifications.ToastNotification]::new($x);"
        "[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('%s').Show($toast)"
        % (t, m, app_id)
    )


def _show_toast(title, message):
    """Lancia il toast. Ritorna True/False. Mai eccezioni."""
    if not IS_WINDOWS:
        return False
    try:
        flags = 0x08000000  # CREATE_NO_WINDOW
        subprocess.run(
            ["powershell", "-NoProfile", "-NonInteractive", "-Command",
             _toast_command(title, message)],
            capture_output=True, timeout=PS_TIMEOUT_S, creationflags=flags,
        )
        return True
    except Exception:
        return False


def _debounced(key):
    """True se la notifica va SCARTATA per debounce/cooldown."""
    now = time.time()
    with _lock:
        if now - _last_any[0] < COOLDOWN_ANY_S:
            return True
        if key in _last_same and now - _last_same[key] < DEBOUNCE_SAME_S:
            return True
        _last_same[key] = now
        _last_any[0] = now
        if len(_last_same) > 64:
            for k in [k for k, v in _last_same.items() if now - v > DEBOUNCE_SAME_S]:
                _last_same.pop(k, None)
        return False


def notify(title, message, kind="info", force=False):
    """Mostra una notifica di sistema se Astral non ha il focus.

    Args:
        title:   titolo breve (es. "Astral - Attenzione").
        message: corpo, troncato a _MSG_CAP caratteri.
        kind:    etichetta libera, usata solo per il debounce interno.
        force:   se True salta il controllo di focus (uso diagnostico/test).

    Ritorna True se il toast e' stato effettivamente inviato.
    """
    try:
        title = (str(title) or "Astral").strip()[:80]
        message = " ".join(str(message or "").split())[:_MSG_CAP]
        if not message:
            return False
        if not force and has_focus():
            return False
        if _debounced((kind, title, message)):
            return False
        th = threading.Thread(target=_show_toast, args=(title, message), daemon=True)
        th.start()
        return True
    except Exception:
        return False


if __name__ == "__main__":
    print("focus su Astral:", has_focus())
    ok = notify("Astral", "Test notifica di sistema (sys_notify)", force=True)
    print("toast inviato:", ok)
    time.sleep(2)
