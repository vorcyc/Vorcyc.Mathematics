#!/usr/bin/env python3
"""Scan wiki_en for Chinese lines and build overrides from wiki_hans."""
from __future__ import annotations

import importlib.util
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
HANS = ROOT / "wiki_hans"
EN = ROOT / "wiki_en"
OVERRIDES = ROOT / "tools" / "sp_manual_overrides.json"

MAPPINGS = [
    ("Module_SignalProcessing_Signals_zh.md", "Module_SignalProcessing_Signals.md"),
    ("Module_SignalProcessing_Operations_zh.md", "Module_SignalProcessing_Operations.md"),
    ("Module_SignalProcessing_Filters_zh.md", "Module_SignalProcessing_Filters.md"),
]

_spec = importlib.util.spec_from_file_location("cn", ROOT / "tools" / "cn_to_en_sp.py")
cn = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(cn)

_gen_spec = importlib.util.spec_from_file_location("gen", ROOT / "tools" / "generate_sp_exact.py")
gen = importlib.util.module_from_spec(_gen_spec)
_gen_spec.loader.exec_module(gen)


def cn_count(s: str) -> int:
    return len(re.findall(r"[\u4e00-\u9fff]", s))


def best_translation(zh_line: str) -> str:
    opts = []
    if zh_line in gen.EXACT:
        opts.append(gen.fix_anchors(gen.fix_links(gen.EXACT[zh_line])))
    opts.append(cn.translate_line(zh_line))
    opts.append(gen.fix_anchors(gen.translate_line(zh_line)))
    best = zh_line
    best_n = cn_count(zh_line)
    for o in opts:
        n = cn_count(o)
        if n < best_n:
            best, best_n = o, n
    return best


def main() -> int:
    overrides: dict[str, str] = {}
    if OVERRIDES.exists():
        overrides = json.loads(OVERRIDES.read_text(encoding="utf-8"))

    for src, dst in MAPPINGS:
        hans_lines = (HANS / src).read_text(encoding="utf-8").splitlines()
        en_lines = (EN / dst).read_text(encoding="utf-8").splitlines()
        for zh, en in zip(hans_lines, en_lines):
            if cn_count(en) > 0 and re.search(r"[\u4e00-\u9fff]", zh):
                tr = best_translation(zh)
                if cn_count(tr) < cn_count(en):
                    overrides[zh] = tr

    OVERRIDES.write_text(json.dumps(overrides, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Overrides: {len(overrides)}")
    for src, dst in MAPPINGS:
        text = (EN / dst).read_text(encoding="utf-8")
        print(f"{dst}: {cn_count(text)} Chinese chars")
    return 0


if __name__ == "__main__":
    sys.exit(main())
