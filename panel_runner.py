import sys, json
sys.path.insert(0, r"C:\Users\masci\Astral")
# Monkeypatch: nessuna console interattiva disponibile
import prompt_toolkit
class _FakeSession:
    def __init__(self, *a, **k): pass
    def prompt(self, *a, **k): return ""
prompt_toolkit.PromptSession = _FakeSession
import prompt_toolkit.shortcuts
prompt_toolkit.shortcuts.PromptSession = _FakeSession
import importlib
astral = importlib.import_module("astral")
model_key, question = sys.argv[1], sys.argv[2]
model = astral.MODELS.get(model_key, model_key)
resp, used = astral.call_with_dynamic_fallback([{"role": "user", "content": question}], primary_model=model)
print(json.dumps({"model": used, "answer": resp.choices[0].message.content}, ensure_ascii=False))
