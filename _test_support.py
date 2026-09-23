# -*- coding: utf-8 -*-
"""Supporto condiviso della suite Astral (standalone + compatibile pytest).

Il nome non inizia con 'test_' apposta: pytest non lo raccoglie come test e
task_tracker non lo esegue come script, ma lo include nell'hash della suite
(vedi task_tracker._hash_files), cosi' un cambio del supporto invalida il
test registrato.
"""
import os
import shutil
import sys
import tempfile
import traceback


def run_all(namespace, title=None):
    """Esegue tutte le funzioni test_* del modulo; exit 1 se una fallisce."""
    tests = [(name, obj) for name, obj in namespace.items()
             if name.startswith("test_") and callable(obj)]
    tests.sort(key=lambda item: item[1].__code__.co_firstlineno)
    failed = []
    for name, fn in tests:
        try:
            fn()
            print("  ok   %s" % name)
        except Exception:
            failed.append(name)
            print("  FAIL %s" % name)
            traceback.print_exc()
    label = title or os.path.basename(sys.argv[0])
    if failed:
        print("FAIL %s: %d/%d test falliti: %s"
              % (label, len(failed), len(tests), ", ".join(failed)))
        sys.exit(1)
    print("OK %s: %d test" % (label, len(tests)))


def temp_dir(prefix="astral_test_"):
    """Cartella temporanea isolata per i test."""
    return tempfile.mkdtemp(prefix=prefix)


def cleanup(path):
    """Rimozione best-effort di una cartella temporanea."""
    shutil.rmtree(path, ignore_errors=True)


class Patcher(object):
    """Monkeypatch esplicito con ripristino garantito (context manager)."""

    def __init__(self):
        self._saved = []

    def set(self, obj, name, value):
        self._saved.append((obj, name, getattr(obj, name, None)))
        setattr(obj, name, value)
        return value

    def restore(self):
        while self._saved:
            obj, name, old = self._saved.pop()
            setattr(obj, name, old)

    def __enter__(self):
        return self

    def __exit__(self, *exc):
        self.restore()
        return False