# -*- coding: utf-8 -*-
# astral_trim.py - Context trimming con adjacency closure (fase 1, piano ratificato)
# Ispirato ai principi del progetto Raven (Apache 2.0), riscritto in stile Astral:
# sincrono, stdlib-only, nessuna dipendenza dal runtime madre.
#
# Principio (adjacency closure): un assistant con tool_calls + TUTTI i suoi
# tool results formano un BLOCCO ATOMICO. In uscita esistono solo blocchi
# completi: zero coppie orfane, zero errori API 400.
#
# Criteri ratificati dal consiglio (verdetto 3/3):
#   - zero orfani tool_call/result
#   - riduzione token >= 30% a budget fisso
#   - fallback al trimmer storico su qualunque errore (safe_trim_context)

import math
import os
import sys

MAX_CONTENT_CHARS = 1500    # cap storico di get_history
CHECKPOINT_MAX_CHARS = 200  # cap storico del checkpoint
CHECKPOINT_MAX_ITEMS = 8
DEFAULT_CHAR_BUDGET = 6000  # cap duro sulla finestra selezionata

CHECKPOINT_HEADER = "[CHECKPOINT CONTESTO - turni precedenti compressi]"

# ---------------------------------------------------------------- respirazione v2
# Curva di consumo sinusoidale: finestra e budget oscillano col ciclo; il
# checkpoint e' un LEDGER di fatti atomici deduplicati (mai ri-riassunti,
# solo sostituiti per FIFO al cap) -> crescita media minima e limitata.
BREATH_PERIOD = 12            # turni per ciclo completo (2*pi)
WAVE_MIN_TURNS = 4            # finestra al ventre
WAVE_MAX_TURNS = 12           # finestra alla cresta
BUDGET_MIN_CHARS = 3500       # budget caratteri al ventre
BUDGET_MAX_CHARS = 8000       # budget caratteri alla cresta
HARD_CAP_CHARS = 12000        # valvola di isteresi (cond. 2 verdetto)
CP_LEDGER_MAX_LINES = 30      # cap globale del ledger fattuale
CP_LINE_CAP = 160             # cap per singolo fatto
CP_SECTIONS = ("OBIETTIVO", "DECISIONI", "ARTEFATTI", "APERTI", "CONTESTO")
CP_SECTION_CAPS = {"OBIETTIVO": 2, "DECISIONI": 6, "ARTEFATTI": 6,
                   "APERTI": 6, "CONTESTO": 10}
_CP_LEDGER_MARK = "[CHECKPOINT CONTESTO - ledger fattuale"
_CP_MARKERS = (CHECKPOINT_HEADER, _CP_LEDGER_MARK)

_TURNS_SEEN = 0
_SESSION_GOAL = None
last_meta = {"event": "boot", "turn": 0, "cycle": 0, "facts": 0,
             "window": 0, "chars": 0}

_ART_HINTS = (".py", ".md", ".json", ".tmp", ".txt", "file", "path",
              "\\", "/", "tool", "patch", "tool")
_DEC_HINTS = ("verdetto", "decid", "approv", "conferm", "ratif",
              "scegli", "scelt", "condizione", "decisione")
_OPEN_HINTS = ("prossim", "todo", "manca", "futur", "devo", "riprov",
               "in sospeso", "da fare", "next")


def _is_checkpoint_msg(m):
    if not isinstance(m, dict) or _role(m) != "user":
        return False
    c = m.get("content") or ""
    return any(c.startswith(k) for k in _CP_MARKERS)


def _norm_key(text):
    return tuple("".join(ch.lower() if ch.isalnum() else " "
                         for ch in text).split())


def _ledger_add(ledger, section, fact):
    fact = " ".join(fact.split())
    if not fact:
        return
    key = _norm_key(fact)
    sec = ledger[section]
    if any(_norm_key(f) == key for f in sec):
        return
    sec.append(fact)
    while len(sec) > CP_SECTION_CAPS[section]:
        sec.pop(0)  # FIFO: i fatti vecchi escono, non vengono ricompressi


def _ledger_parse(text):
    ledger = {k: [] for k in CP_SECTIONS}
    cycle = 0
    cur = None
    for line in (text or "").splitlines():
        ls = line.strip()
        if not ls:
            continue
        if ls.startswith(_CP_LEDGER_MARK):
            try:
                cycle = int(ls.rsplit("#", 1)[1].rstrip(" ]"))
            except Exception:
                cycle = 0
            continue
        if ls.startswith(CHECKPOINT_HEADER):
            continue
        head = ls[:-1].strip().upper() if ls.endswith(":") else None
        if head in CP_SECTIONS:
            cur = head
            continue
        if ls.startswith("- ") and cur:
            ledger[cur].append(ls[2:].strip())
        elif cur and ledger[cur]:
            ledger[cur][-1] += " " + ls  # continuazione della riga precedente
    return ledger, cycle


def _classify_fact(text):
    tl = text.lower()
    if any(h in tl for h in _OPEN_HINTS):
        return "APERTI"
    if any(h in tl for h in _ART_HINTS):
        return "ARTEFATTI"
    if any(h in tl for h in _DEC_HINTS):
        return "DECISIONI"
    return "CONTESTO"


def _harvest_facts(ledger, blocks_msgs):
    global _SESSION_GOAL
    for blk in blocks_msgs:
        for m in blk:
            if not isinstance(m, dict):
                continue
            txt = (m.get("content") or "").strip()
            if not txt or _is_checkpoint_msg(m):
                continue
            r = _role(m)
            first = txt.splitlines()[0][:CP_LINE_CAP]
            if r == "user":
                if _SESSION_GOAL is None:
                    _SESSION_GOAL = first
                _ledger_add(ledger, "CONTESTO", "U: " + first)
            elif r == "assistant":
                bullets = [l.strip() for l in txt.splitlines()
                           if l.strip().startswith(("- ", "* ", "\u2022 "))][:3]
                if bullets:
                    for b in bullets:
                        _ledger_add(ledger, _classify_fact(b),
                                    b.lstrip("-*\u2022 ").strip()[:CP_LINE_CAP])
                else:
                    _ledger_add(ledger, _classify_fact(first), "A: " + first)


def _ledger_total(ledger):
    return sum(len(v) for v in ledger.values())


def _ledger_dump(ledger, cycle):
    global _SESSION_GOAL
    if _SESSION_GOAL and not ledger["OBIETTIVO"]:
        ledger["OBIETTIVO"] = [_SESSION_GOAL[:CP_LINE_CAP]]
    # cap globale: evict FIFO dalla sezione piu' grossa finche' non rientra
    while _ledger_total(ledger) > CP_LEDGER_MAX_LINES:
        sec = max(CP_SECTIONS, key=lambda s: len(ledger[s]))
        if not ledger[sec]:
            break
        ledger[sec].pop(0)
    lines = [f"{_CP_LEDGER_MARK} | ciclo #{cycle}"]
    for sec in CP_SECTIONS:
        if ledger[sec]:
            lines.append(sec + ":")
            lines.extend("- " + f for f in ledger[sec])
    return "\n".join(lines)


def breathing_trim(messages, turn=None):
    """Trimmer respiratorio: finestra e budget sinusoidali, checkpoint a
    ledger di fatti. Deterministico, stdlib-only, zero orfani."""
    global _TURNS_SEEN
    if turn is None:
        turn = _TURNS_SEEN
    _TURNS_SEEN = turn + 1
    phase = (turn % BREATH_PERIOD) / BREATH_PERIOD * 2 * math.pi
    wave = (1 + math.sin(phase)) / 2
    window = int(round(WAVE_MIN_TURNS + (WAVE_MAX_TURNS - WAVE_MIN_TURNS) * wave))
    budget = int(BUDGET_MIN_CHARS + (BUDGET_MAX_CHARS - BUDGET_MIN_CHARS) * wave)
    cycle = turn // BREATH_PERIOD

    blocks = build_blocks(messages)
    sys_blocks = [b for b in blocks if b[2] == "system"]
    conv_blocks = [b for b in blocks if b[2] != "system"]

    cp_block, conv2 = None, []
    for b in conv_blocks:
        if cp_block is None and _is_checkpoint_msg(messages[b[0]]):
            cp_block = b
        else:
            conv2.append(b)

    tail = select_tail(conv2, window)
    kept = set((b[0], b[1]) for b in tail)
    excl = [messages[s:e] for s, e, _ in conv_blocks
            if (s, e) not in kept and (s, e) != (cp_block or (None, None)[:0])]
    # (escludi anche il blocco checkpoint da 'excl': e' gia' nel ledger)
    excl = [blk for blk in excl
            if not (_is_checkpoint_msg(blk[0]) if blk else False)]

    ledger, _ = _ledger_parse(
        (messages[cp_block[0]].get("content") or "") if cp_block else "")
    _harvest_facts(ledger, excl)
    cp_text = _ledger_dump(ledger, cycle)

    result = []
    for s, e, _ in sys_blocks:
        result.extend(messages[s:e])
    if _ledger_total(ledger) or _SESSION_GOAL:
        result.append({"role": "user", "content": cp_text})

    def assemble(window_, budget_):
        t2 = select_tail(conv2, window_)
        sel = []
        for s, e, _ in t2:
            blk = []
            for m in messages[s:e]:
                if isinstance(m, dict) and m.get("content"):
                    m = dict(m)
                    m["content"] = _truncate(m["content"], MAX_CONTENT_CHARS)
                blk.append(m)
            sel.append(blk)
        eff = max(500, budget_ - len(cp_text))
        out = list(result)
        for blk in _apply_char_budget(sel, eff):
            out.extend(blk)
        return out

    out = assemble(window, budget)
    total = sum(len(m.get("content") or "")
                for m in out if isinstance(m, dict))
    if total > HARD_CAP_CHARS:  # isteresi: consolidamento d'emergenza
        out = assemble(WAVE_MIN_TURNS, BUDGET_MIN_CHARS)
        total = sum(len(m.get("content") or "")
                    for m in out if isinstance(m, dict))

    last_meta.update({"event": "consolidate" if (turn % BREATH_PERIOD)
                      == int(BREATH_PERIOD * 0.75) else "turn",
                      "turn": turn, "cycle": cycle,
                      "facts": _ledger_total(ledger),
                      "window": window, "chars": total})
    return out


def _role(m):
    if isinstance(m, dict):
        return m.get("role")
    return getattr(m, "role", None)


def _tool_call_ids(m):
    if not isinstance(m, dict):
        return []
    return [tc.get("id") for tc in (m.get("tool_calls") or [])
            if isinstance(tc, dict) and tc.get("id")]


def _truncate(text, cap):
    if text and len(text) > cap:
        return text[:cap] + " ...[troncato per risparmio token]"
    return text


def build_blocks(messages):
    """Partiziona in blocchi atomici [(start, end, kind)].
    kind: system | user | assistant | toolblock | opaque.
    Tool orfani e blocchi incompleti (assistant senza tutti i results)
    vengono DROPPATI: non possono esistere in uscita."""
    blocks = []
    i, n = 0, len(messages)
    while i < n:
        m = messages[i]
        r = _role(m)
        if r == "system":
            blocks.append((i, i + 1, "system"))
            i += 1
        elif r == "tool":
            i += 1  # orfano: nessun assistant lo apre -> droppato
        elif r == "assistant" and _tool_call_ids(m):
            ids = set(_tool_call_ids(m))
            j, found = i + 1, set()
            while j < n:
                mj = messages[j]
                if _role(mj) == "tool" and isinstance(mj, dict):
                    tid = mj.get("tool_call_id")
                    if tid in ids and tid not in found:
                        found.add(tid)
                        j += 1
                        continue
                break
            if found == ids:
                blocks.append((i, j, "toolblock"))
            i = j  # se incompleto: assistant + results parziali scartati
        elif r == "assistant":
            blocks.append((i, i + 1, "assistant"))
            i += 1
        elif r == "user":
            blocks.append((i, i + 1, "user"))
            i += 1
        else:
            blocks.append((i, i + 1, "opaque"))
            i += 1
    return blocks


def select_tail(blocks, budget):
    """Selezione pull-mode dal fondo: i blocchi conversazionali consumano
    1 turno ciascuno; system/opaque non consumano e non vengono selezionati."""
    kept, used = [], 0
    for b in reversed(blocks):
        if used >= budget:
            break
        if b[2] in ("user", "assistant", "toolblock"):
            kept.append(b)
            used += 1
    kept.reverse()
    return kept


def _apply_char_budget(blocks_msgs, budget_chars):
    """Budget: prima tronca i contenuti dei blocchi NON ultimi (tool 750/400,
    user/assistant 400); se ancora sopra budget, scarta i blocchi piu' vecchi
    della coda finche' non rientra (minimo 1 blocco: l'ultimo e' sacro e puo'
    eccedere il budget da solo)."""
    blocks_msgs = [list(b) for b in blocks_msgs]

    def total():
        return sum(len(m.get("content") or "")
                   for b in blocks_msgs for m in b if isinstance(m, dict))

    for cap_tool, cap_text in ((750, 400), (400, 400)):
        if total() <= budget_chars or len(blocks_msgs) <= 1:
            break
        for bi in range(len(blocks_msgs) - 1):
            if total() <= budget_chars:
                break
            for mi, m in enumerate(blocks_msgs[bi]):
                if not isinstance(m, dict):
                    continue
                c = m.get("content")
                if not c:
                    continue
                cap = cap_tool if _role(m) == "tool" else cap_text
                if len(c) > cap:
                    nm = dict(m)
                    nm["content"] = c[:cap] + " ...[troncato]"
                    blocks_msgs[bi][mi] = nm
    while len(blocks_msgs) > 1 and total() > budget_chars:
        blocks_msgs.pop(0)
    return blocks_msgs


def trim_context(messages, max_turns=6, with_summary=True,
                 budget_chars=DEFAULT_CHAR_BUDGET):
    """Sostituto 1:1 di compact_history_with_summary con adjacency closure."""
    blocks = build_blocks(messages)
    sys_blocks = [b for b in blocks if b[2] == "system"]
    conv_blocks = [b for b in blocks if b[2] != "system"]
    tail = select_tail(conv_blocks, max_turns)
    kept = set((b[0], b[1]) for b in tail)
    excl = [b for b in conv_blocks if (b[0], b[1]) not in kept]

    result = []
    for s, e, _ in sys_blocks:
        result.extend(messages[s:e])

    cp_text = None
    if with_summary and excl:
        parts = []
        for s, e, kind in excl:
            if kind not in ("user", "assistant"):
                continue
            m = messages[s]
            txt = (m.get("content") or "").strip() if isinstance(m, dict) else ""
            if txt:
                parts.append(("U: " if kind == "user" else "A: ") +
                             txt[:CHECKPOINT_MAX_CHARS])
        if parts:
            cp_text = CHECKPOINT_HEADER + "\n" + "\n".join(parts[-CHECKPOINT_MAX_ITEMS:])
            result.append({"role": "user", "content": cp_text})

    sel = []
    for s, e, _ in tail:
        blk = []
        for m in messages[s:e]:
            if isinstance(m, dict) and m.get("content"):
                m = dict(m)
                m["content"] = _truncate(m["content"], MAX_CONTENT_CHARS)
            blk.append(m)
        sel.append(blk)
    # il budget vale per la coda: il checkpoint consuma il suo margine
    eff_budget = max(500, budget_chars - (len(cp_text) if cp_text else 0))
    sel = _apply_char_budget(sel, eff_budget)
    for blk in sel:
        result.extend(blk)
    return result


def safe_trim_context(messages, max_turns=6):
    """Entry point per astral.py: breathing (v2) -> wave trim v1 -> storico.
    Kill-switch: env ASTRAL_TRIM_LEGACY=1 forza il trimmer v1."""
    if os.environ.get("ASTRAL_TRIM_LEGACY") != "1":
        try:
            return breathing_trim(messages)
        except Exception:
            pass  # rollback trasparente alla v1
    try:
        return trim_context(messages, max_turns=max_turns)
    except Exception:
        try:
            from history_store import compact_history_with_summary
            return compact_history_with_summary(messages)
        except Exception:
            return messages


def count_orphans(messages):
    """Coppie rotte: tool results senza assistant aperto + assistant con
    tool_calls rimasti senza result (causa errore API 400)."""
    orphans, open_ids = 0, set()
    for m in messages:
        r = _role(m)
        if r == "assistant" and isinstance(m, dict):
            open_ids.update(_tool_call_ids(m))
        elif r == "tool" and isinstance(m, dict):
            tid = m.get("tool_call_id")
            if tid in open_ids:
                open_ids.discard(tid)
            else:
                orphans += 1
    return orphans + len(open_ids)


# ---------------------------------------------------------------- selftest

def _selftest():
    import json

    def fake_conv(n_calls, result_chars):
        msgs = [{"role": "system", "content": "SYS"}]
        for k in range(n_calls):
            msgs.append({"role": "user", "content": f"Domanda {k} " + "x" * 300})
            msgs.append({"role": "assistant", "content": "", "tool_calls": [
                {"id": f"call_{k}", "type": "function",
                 "function": {"name": "run_powershell_cmd",
                              "arguments": json.dumps({"command": "dir"})}}]})
            msgs.append({"role": "tool", "tool_call_id": f"call_{k}",
                         "content": json.dumps({"stdout": "y" * result_chars})})
            msgs.append({"role": "assistant",
                         "content": f"Risposta finale {k}. " + "z" * 100})
        return msgs

    fails = []

    def tot(msgs):
        return sum(len(m.get("content") or "") for m in msgs if isinstance(m, dict))

    # F1: riduzione + zero orfani su conversazione realistica (tool 1800 chars)
    conv = fake_conv(10, 1800)
    out = trim_context(conv, max_turns=6)
    red = 1 - tot(out) / tot(conv)
    if count_orphans(out) != 0:
        fails.append("F1 orfani != 0")
    if red < 0.30:
        fails.append(f"F1 riduzione {red:.0%} < 30%")

    # F1b: fixture snella (tool 300 chars)
    conv_s = fake_conv(10, 300)
    out_s = trim_context(conv_s, max_turns=6)
    red_s = 1 - tot(out_s) / tot(conv_s)
    if count_orphans(out_s) != 0 or red_s < 0.30:
        fails.append(f"F1b orfani={count_orphans(out_s)} riduzione={red_s:.0%}")

    # F2: tool orfano in testa -> droppato
    conv2 = [{"role": "tool", "tool_call_id": "ghost", "content": "orphan"}] + fake_conv(2, 100)
    out2 = trim_context(conv2, max_turns=6)
    if any(isinstance(m, dict) and m.get("tool_call_id") == "ghost" for m in out2):
        fails.append("F2 orfano sopravvissuto")

    # F3: assistant con tool_calls SENZA result -> blocco droppato (niente 400)
    conv3 = [{"role": "assistant", "content": "", "tool_calls": [
        {"id": "call_x", "type": "function",
         "function": {"name": "t", "arguments": "{}"}}]}] + fake_conv(2, 100)
    out3 = trim_context(conv3, max_turns=6)
    leaked = any(isinstance(m, dict) and (
        m.get("tool_call_id") == "call_x" or
        any(tc.get("id") == "call_x" for tc in (m.get("tool_calls") or [])))
        for m in out3)
    if leaked or count_orphans(out3) != 0:
        fails.append("F3 assistant con tool_calls senza result in uscita")

    # F4: system sempre in testa
    if not out or _role(out[0]) != "system":
        fails.append("F4 system non in testa")

    # F5: ultimo scambio integro (verifica a livello di BLOCCO, non di slice)
    last_tc_list = [m for m in out if _role(m) == "assistant" and _tool_call_ids(m)]
    ok5 = _role(out[-1]) == "assistant" and count_orphans(out) == 0 and bool(last_tc_list)
    if ok5:
        last_tc = last_tc_list[-1]
        i_tc = out.index(last_tc)
        res_ids = {m.get("tool_call_id") for m in out[i_tc + 1:] if _role(m) == "tool"}
        ok5 = set(_tool_call_ids(last_tc)) <= res_ids
    if not ok5:
        fails.append("F5 ultimo blocco incompleto")

    # F6: multi-tool (2 calls) con un solo result -> blocco incompleto -> drop
    blk = [{"role": "assistant", "content": "", "tool_calls": [
        {"id": "a", "type": "function", "function": {"name": "t", "arguments": "{}"}},
        {"id": "b", "type": "function", "function": {"name": "t", "arguments": "{}"}}]},
        {"role": "tool", "tool_call_id": "a", "content": "ok"}]
    out6 = trim_context(fake_conv(1, 50)[:-1] + blk + fake_conv(1, 50)[1:], max_turns=6)
    if any(isinstance(m, dict) and m.get("tool_call_id") == "b" for m in out6) \
            or any(isinstance(m, dict) and m.get("tool_call_id") == "a" for m in out6):
        fails.append("F6 blocco multi-tool incompleto sopravvissuto")

    # F7: budget (target: rientra, oppure converge al floor = checkpoint + ultimo blocco)
    out7 = trim_context(fake_conv(10, 1800), max_turns=6, budget_chars=6000)
    out7b = trim_context(fake_conv(10, 1800), max_turns=6, budget_chars=2500)
    if tot(out7) > 6100:
        fails.append(f"F7 budget 6000 non rispettato: {tot(out7)}")
    if not (tot(out7b) <= 4300 and tot(out7b) <= tot(out7)):
        fails.append(f"F7 floor: {tot(out7b)} (atteso <= 4300 e monotono)")

    # F8: confronto col trimmer storico (correttezza: orfani generati)
    old_info = "n/a"
    try:
        from history_store import compact_history_with_summary as old_compact
        old = old_compact(conv)
        old_info = f"chars={tot(old)} orphans={count_orphans(old)}"
    except Exception as e:
        old_info = f"skip ({e})"

    # B1: oscillazione della finestra (sinusoidale, non monotona)
    import astral_trim as _self
    _self._TURNS_SEEN, _self._SESSION_GOAL = 0, None
    wins, long_conv = [], fake_conv(40, 200)
    for t in range(24):
        o = _self.breathing_trim(long_conv, turn=t)
        wins.append(_self.last_meta["window"])
    if len(set(wins)) < 3 or wins[0] >= max(wins):
        fails.append(f"B1 finestra non oscillante: {wins[:12]}")

    # B2: zero orfani + integrita' ultimo blocco in breathing
    _self._TURNS_SEEN, _self._SESSION_GOAL = 0, None
    ob = _self.breathing_trim(fake_conv(15, 300), turn=0)
    if count_orphans(ob) != 0:
        fails.append("B2 orfani != 0")

    # B3: dedup ledger + FIFO per sezione
    led = {k: [] for k in _self.CP_SECTIONS}
    _self._ledger_add(led, "DECISIONI", "Verdetto approvato 3/3")
    _self._ledger_add(led, "DECISIONI", "verdetto  APPROVATO   3/3")
    for i in range(_self.CP_SECTION_CAPS["DECISIONI"] + 5):
        _self._ledger_add(led, "DECISIONI", f"decisione numero {i}")
    if len(led["DECISIONI"]) != _self.CP_SECTION_CAPS["DECISIONI"]:
        fails.append("B3 cap FIFO sezione errato")

    # B4: HARD_CAP con contenuti enormi (isteresi)
    _self._TURNS_SEEN, _self._SESSION_GOAL = 0, None
    huge = fake_conv(30, 3000)
    oh = _self.breathing_trim(huge, turn=3)
    th = tot(oh)
    if th > _self.HARD_CAP_CHARS + 1500:  # margine: ultimo blocco sacro
        fails.append(f"B4 hard cap violato: {th}")

    # B5: un solo checkpoint in uscita, con marca ledger, mai duplicati
    cps = [m for m in oh if _self._is_checkpoint_msg(m)]
    if len(cps) != 1 or _self._CP_LEDGER_MARK not in (cps[0].get("content") or ""):
        fails.append("B5 checkpoint ledger assente/duplicato")

    # B6: media piatta: su 2 cicli, chars medi ciclo2 <= ciclo1 * 1.10
    _self._TURNS_SEEN, _self._SESSION_GOAL = 0, None
    tots = [tot(_self.breathing_trim(fake_conv(30, 600), turn=t)) for t in range(24)]
    m1, m2 = sum(tots[:12]) / 12, sum(tots[12:]) / 12
    if m2 > m1 * 1.10:
        fails.append(f"B6 deriva media {m1:.0f}->{m2:.0f} > 10%")

    print(f"SELFTEST breathing: windows={wins[0]}..{max(wins)} "
          f"hardcap={th} media_cicli={m1:.0f}/{m2:.0f}")
    print(f"SELFTEST new: in={tot(conv)} out={tot(out)} riduzione={red:.0%} "
          f"orfani={count_orphans(out)} | snella rid={red_s:.0%}")
    print(f"SELFTEST old_compact: {old_info}")
    if fails:
        print("FAIL:")
        for f in fails:
            print(" -", f)
        sys.exit(1)
    print("SELFTEST OK")


if __name__ == "__main__":
    if "--selftest" in sys.argv:
        _selftest()
    else:
        print("uso: python astral_trim.py --selftest")
