# astral_relations.py - probe API relazioni The Sims 4 (Python 3.7)
# v2: sentinella scritta PRIMA di ogni import, log errori in chiaro (niente except: pass)
import os
import sys

HOME = os.path.expanduser('~')
DOCS = os.path.join(HOME, 'Documents', 'Electronic Arts', 'The Sims 4')
OUT = os.path.join(DOCS, 'astral_probe.json')
ERR = os.path.join(DOCS, 'astral_probe_err.txt')
CANDIDATES = [
    OUT,
    os.path.join(HOME, 'Desktop', 'astral_probe.json'),
    os.path.join(HOME, 'astral_probe.json'),
]


def _touch_all(text):
    written = []
    for path in CANDIDATES:
        try:
            with open(path, 'w') as f:
                f.write(text)
            written.append(path)
        except Exception:
            pass
    return written


_touch_all('START carica si, python=' + sys.version)

import json
import traceback


def _names(obj, limit=200):
    try:
        return sorted([n for n in dir(obj) if not n.startswith('__')])[:limit]
    except Exception:
        return []


def _collect():
    data = {'python': sys.version, 'ok': True}
    try:
        import services
        data['services'] = _names(services)
    except Exception:
        data['services_error'] = traceback.format_exc()
    try:
        import sims4.resources as resources
        data['resources'] = _names(resources)
    except Exception:
        data['resources_error'] = traceback.format_exc()
    for modname in ('relationships.relationship_tracker',
                    'relationships.relationship',
                    'sims.sim_info'):
        try:
            mod = __import__(modname, fromlist=['*'])
            data[modname] = _names(mod)
        except Exception:
            data[modname + '_error'] = traceback.format_exc()
    return data


def _write(data):
    text = json.dumps(data, indent=2, default=str)
    return _touch_all(text)


def _log_err(exc_text):
    try:
        with open(ERR, 'a') as f:
            f.write(exc_text + '\n' + '-' * 60 + '\n')
    except Exception:
        pass


def _run():
    try:
        data = _collect()
        data['written_to'] = _write(data)
        return data
    except Exception:
        _log_err(traceback.format_exc())
        return None


def _on_load(*args, **kwargs):
    _run()


_run()
