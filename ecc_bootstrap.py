# -*- coding: utf-8 -*-
# ecc_bootstrap.py - Installazione on-demand delle skill/agenti ECC.
#
# Le skill ECC NON sono versionate nel repo (680+ file): sporcherebbero
# ogni clone. Al primo avvio, se la cartella ecc_skills/ manca o e' vuota,
# questo modulo chiede all'utente se scaricarle dalla repo upstream pinnata.
#
# Sorgente: https://github.com/affaan-m/ECC (MIT).
# Il download e' un archivio zip del commit pinnato: nessuna dipendenza esterna
# (solo stdlib), nessuna esecuzione di codice remoto (solo estrazione di .md).

import os
import shutil
import tempfile
import urllib.request
import zipfile

_BASE = os.path.dirname(os.path.abspath(__file__))
_SKILLS_ROOT = os.path.join(_BASE, "ecc_skills")
_SKILLS_DIR = os.path.join(_SKILLS_ROOT, "skills")
_AGENTS_DIR = os.path.join(_SKILLS_ROOT, "agents")

# Commit pinnato: aggiornare qui per cambiare versione delle skill.
ECC_REPO = "affaan-m/ECC"
ECC_COMMIT = "b92f181b3b748d1abbc2afd94af6aa7d8c8fca29"
ECC_URL = "https://codeload.github.com/%s/zip/%s" % (ECC_REPO, ECC_COMMIT)
ECC_ATTRIBUTION = (
    "ECC upstream: https://github.com/%s\n"
    "commit: %s\n"
    "license: MIT\n"
) % (ECC_REPO, ECC_COMMIT)


def is_installed():
    """True se ecc_skills/ contiene almeno una skill o un agente."""
    for d in (_SKILLS_DIR, _AGENTS_DIR):
        try:
            if os.path.isdir(d) and any(os.scandir(d)):
                return True
        except OSError:
            continue
    return False


def _download(url, dest, timeout=120):
    """Scarica url in dest. Ritorna il numero di byte scritti."""
    req = urllib.request.Request(url, headers={"User-Agent": "astral-ecc-bootstrap"})
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        data = resp.read()
    with open(dest, "wb") as fh:
        fh.write(data)
    return len(data)


def _extract(zip_path):
    """Estrae skills/ e agents/ dall'archivio ECC in ecc_skills/.

    L'archivio ha una cartella radice tipo 'ECC-<sha>/': la si individua e si
    copiano solo le due sottocartelle utili. Ritorna (n_skills, n_agents).
    """
    n_skills = n_agents = 0
    with zipfile.ZipFile(zip_path) as zf:
        names = zf.namelist()
        if not names:
            raise ValueError("archivio ECC vuoto")
        root = names[0].split("/", 1)[0] + "/"
        for member in zf.infolist():
            if member.is_dir():
                continue
            fn = member.filename
            rel = fn[len(root):] if fn.startswith(root) else fn
            low = rel.lower()
            if low.startswith("skills/") and low.endswith(".md"):
                target = os.path.join(_SKILLS_DIR, rel[len("skills/"):])
                n_skills += 1
            elif low.startswith("agents/") and low.endswith(".md"):
                target = os.path.join(_AGENTS_DIR, rel[len("agents/"):])
                n_agents += 1
            else:
                continue
            os.makedirs(os.path.dirname(target), exist_ok=True)
            with zf.open(member) as src, open(target, "wb") as out:
                shutil.copyfileobj(src, out)
    return n_skills, n_agents


def install(verbose=True):
    """Scarica ed estrae le skill ECC. Ritorna un dict di esito."""
    tmpdir = tempfile.mkdtemp(prefix="ecc_boot_")
    zip_path = os.path.join(tmpdir, "ecc.zip")
    try:
        if verbose:
            print("[ecc] download skill da %s @ %s ..." % (ECC_REPO, ECC_COMMIT[:8]))
        size = _download(ECC_URL, zip_path)
        if verbose:
            print("[ecc] scaricati %.1f MB, estrazione ..." % (size / 1048576.0))
        os.makedirs(_SKILLS_ROOT, exist_ok=True)
        n_skills, n_agents = _extract(zip_path)
        with open(os.path.join(_SKILLS_ROOT, "ATTRIBUTION.txt"), "w", encoding="utf-8") as fh:
            fh.write(ECC_ATTRIBUTION)
        if verbose:
            print("[ecc] installate %d skill e %d agenti in ecc_skills/" % (n_skills, n_agents))
        return {"ok": True, "skills": n_skills, "agents": n_agents, "bytes": size}
    except Exception as exc:
        return {"ok": False, "error": "%s: %s" % (type(exc).__name__, exc)}
    finally:
        shutil.rmtree(tmpdir, ignore_errors=True)


def ensure_skills(ask=None, verbose=True):
    """Al primo avvio: se mancano le skill, chiede se installarle.

    ask: callable(text)->str che ritorna 's' per confermare. Se None, non
    chiede nulla (modalita' non interattiva) e ritorna 'missing'.
    Non solleva mai eccezioni: l'avvio di Astral non deve dipendere da questo.
    """
    try:
        if is_installed():
            return {"status": "present"}
        if ask is None:
            return {"status": "missing"}
        answer = (ask("Skill ECC non installate. Scaricarle ora dalla repo "
                      "upstream (~37 MB)?") or "").strip().lower()
        if answer not in ("s", "si", "y", "yes"):
            return {"status": "declined"}
        res = install(verbose=verbose)
        res["status"] = "installed" if res.get("ok") else "error"
        return res
    except Exception as exc:
        return {"status": "error", "error": "%s: %s" % (type(exc).__name__, exc)}


if __name__ == "__main__":
    import sys

    if "--install" in sys.argv:
        print(install())
    else:
        print("installed:", is_installed())

