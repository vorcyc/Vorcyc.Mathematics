#!/usr/bin/env python3
"""Pick best translation per line and build sp_manual_overrides.json."""
from __future__ import annotations

import importlib.util
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXACT_JSON = ROOT / "tools" / "sp_exact_translations.json"
OUT_JSON = ROOT / "tools" / "sp_manual_overrides.json"

def load_module(name: str, path: Path):
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod

gen = load_module("gen", ROOT / "tools" / "generate_sp_exact.py")
twp = load_module("twp", ROOT / "tools" / "translate_wiki_signal_processing.py")

def cn_count(s: str) -> int:
    return len(re.findall(r"[\u4e00-\u9fff]", s))

def candidates(line: str) -> list[str]:
    opts = []
    if line in gen.EXACT:
        opts.append(gen.fix_anchors(gen.fix_links(gen.EXACT[line])))
    opts.append(gen.fix_anchors(gen.translate_line(line)))
    opts.append(twp.translate_headers(twp.translate_text(line)))
    # dedupe
    seen = set()
    out = []
    for o in opts:
        if o not in seen:
            seen.add(o)
            out.append(o)
    return out

def main() -> int:
    exact = json.loads(EXACT_JSON.read_text(encoding="utf-8"))
    overrides: dict[str, str] = {}
    if OUT_JSON.exists():
        overrides = json.loads(OUT_JSON.read_text(encoding="utf-8"))

    for zh, cur in exact.items():
        if cn_count(cur) == 0:
            continue
        if zh in overrides and cn_count(overrides[zh]) == 0:
            continue
        best = cur
        best_n = cn_count(cur)
        for opt in candidates(zh):
            n = cn_count(opt)
            if n < best_n:
                best, best_n = opt, n
        if best_n < cn_count(cur):
            overrides[zh] = best

    OUT_JSON.write_text(json.dumps(overrides, ensure_ascii=False, indent=2), encoding="utf-8")
    still = [k for k, v in exact.items() if cn_count(overrides.get(k, v)) > 0]
    print(f"Overrides: {len(overrides)}, still Chinese: {len(still)}")
    (ROOT / "tools" / "sp_still_chinese.json").write_text(
        json.dumps(still, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    return 0

if __name__ == "__main__":
    sys.exit(main())
