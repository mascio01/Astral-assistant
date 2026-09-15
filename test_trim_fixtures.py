# -*- coding: utf-8 -*-
# test_trim_fixtures.py - criterio ratificato: >=20 conversazioni, zero orfani
# Fixture 1 = dati reali (chat_history.json); 2-20 = mutazioni realistico-avversarie.
import json, os, sys
from astral_trim import trim_context, count_orphans

BASE = os.path.dirname(os.path.abspath(__file__))

def load_real():
    p = os.path.join(BASE, "chat_history.json")
    if os.path.exists(p):
        with open(p, encoding="utf-8") as f:
            data = json.load(f)
        return [{"role": e.get("role", "user"), "content": e.get("text", "")} for e in data]
    return [{"role": "system", "content": "SYS"}] + \
           [{"role": "user", "content": f"Domanda {k}"} for k in range(8)]

def tool_turn(idx, n_calls=1, rchars=1800, drop_last=False):
    msgs = [{"role": "assistant", "content": "", "tool_calls": [
        {"id": f"c{idx}_{j}", "type": "function",
         "function": {"name": "run_powershell_cmd", "arguments": "{\"command\":\"dir\"}"}}
        for j in range(n_calls)]}]
    for j in range(n_calls - (1 if drop_last else 0)):
        msgs.append({"role": "tool", "tool_call_id": f"c{idx}_{j}",
                     "content": json.dumps({"stdout": "y" * rchars})})
    return msgs

def tot(ms):
    return sum(len(m.get("content") or "") for m in ms if isinstance(m, dict))

real = load_real()
fixtures = {
    1:  ("reale raw", list(real)),
    2:  ("reale + tool block", real + tool_turn(99)),
    3:  ("tool block in mezzo", real[:6] + tool_turn(98) + real[6:]),
    4:  ("orfano tool in testa", [{"role": "tool", "tool_call_id": "ghost", "content": "orphan"}] + real),
    5:  ("assistant tool_calls senza result", real + [{"role": "assistant", "content": "", "tool_calls": [
         {"id": "loose", "type": "function", "function": {"name": "t", "arguments": "{}"}}]}]),
    6:  ("multi-tool con result mancante", real[:4] + tool_turn(97, n_calls=3, drop_last=True) + real[4:]),
    7:  ("tool-heavy 6 turni", sum([tool_turn(k, rchars=2000) + [{"role": "user", "content": f"q{k}"},
         {"role": "assistant", "content": "ris " * 80}] for k in range(6)], [])),
    8:  ("tool-heavy 12 turni", sum([tool_turn(k, rchars=2500) + [{"role": "user", "content": f"q{k}"},
         {"role": "assistant", "content": "ris " * 80}] for k in range(12)], [])),
    9:  ("contenuti giganti", real + [{"role": "user", "content": "A" * 8000},
         {"role": "assistant", "content": "B" * 8000}]),
    10: ("solo system", [{"role": "system", "content": "SYS"}]),
    11: ("vuota", []),
    12: ("un solo scambio", real[:2]),
    13: ("tool-first conversation", sum([tool_turn(k, rchars=600) for k in range(4)], [])),
    14: ("oggetti non-dict", real[:3] + ["stranezza", None] + real[3:]),
    15: ("reale x2 con summary", real + real),
    16: ("results vuoti", real[:4] + [{"role": "assistant", "content": "", "tool_calls": [
         {"id": "e1", "type": "function", "function": {"name": "t", "arguments": "{}"}}]},
         {"role": "tool", "tool_call_id": "e1", "content": ""}] + real[4:]),
    17: ("id duplicati", [{"role": "assistant", "content": "", "tool_calls": [
         {"id": "dup", "type": "function", "function": {"name": "t", "arguments": "{}"}}]},
         {"role": "tool", "tool_call_id": "dup", "content": "a"},
         {"role": "tool", "tool_call_id": "dup", "content": "b"}] + real[:4]),
    18: ("nested tool chains", tool_turn(1) + tool_turn(2, n_calls=2) + tool_turn(3) +
         [{"role": "user", "content": "q"}] + real[:2]),
    19: ("reale con tool e orfani misti", [{"role": "tool", "tool_call_id": "g1", "content": "o"}] +
         real[:5] + tool_turn(96, n_calls=2) + real[5:] +
         [{"role": "assistant", "content": "", "tool_calls": [
          {"id": "g2", "type": "function", "function": {"name": "t", "arguments": "{}"}}]}]),
    20: ("budget stress", sum([tool_turn(k, rchars=3000) + [{"role": "user", "content": f"q{k} " + "x" * 500}]
         for k in range(8)], [])),
}

fails, reductions = [], {}
for fid, (name, msgs) in sorted(fixtures.items()):
    try:
        out = trim_context(msgs, max_turns=6)
        orph = count_orphans(out)
        if orph != 0:
            fails.append(f"F{fid} '{name}': {orph} orfani")
        if fid in (7, 8, 20):
            red = 1 - tot(out) / tot(msgs)
            reductions[fid] = red
            if red < 0.30:
                fails.append(f"F{fid} '{name}': riduzione {red:.0%} < 30%")
    except Exception as e:
        fails.append(f"F{fid} '{name}': EXC {type(e).__name__}: {e}")

for fid, red in sorted(reductions.items()):
    print(f"F{fid}: riduzione {red:.0%}")
# F20 budget
out20 = trim_context(fixtures[20][1], max_turns=6)
print(f"F20 chars: {tot(out20)} (budget target ~6000)")
if fails:
    print("FAIL:")
    [print(" -", f) for f in fails]
    sys.exit(1)
print(f"FIXTURES OK: 20/20 zero orfani")
