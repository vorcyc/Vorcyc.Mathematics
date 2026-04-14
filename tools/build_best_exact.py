#!/usr/bin/env python3
"""Build best sp_exact_translations.json from multiple translators."""
from __future__ import annotations

import importlib.util
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
UNIQUE = ROOT / "tools" / "sp_unique_cn_lines.json"
OUT = ROOT / "tools" / "sp_exact_translations.json"
OVERRIDES = ROOT / "tools" / "sp_manual_overrides.json"


def load(name: str, path: Path):
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


gen = load("gen", ROOT / "tools" / "generate_sp_exact.py")
twp = load("twp", ROOT / "tools" / "translate_wiki_signal_processing.py")
cn = load("cn", ROOT / "tools" / "cn_to_en_sp.py")


def cn_count(s: str) -> int:
    return len(re.findall(r"[\u4e00-\u9fff]", s))


def candidates(line: str) -> list[str]:
    opts: list[str] = []
    if line in gen.EXACT:
        opts.append(gen.fix_anchors(gen.fix_links(gen.EXACT[line])))
    for zh, en in twp.PHRASES:
        if line == zh:
            opts.append(gen.fix_anchors(gen.fix_links(en)))
    for zh, en in cn.PHRASES:
        if line == zh:
            opts.append(gen.fix_anchors(gen.fix_links(en)))
    opts.append(gen.fix_anchors(gen.translate_line(line)))
    opts.append(gen.fix_anchors(twp.translate_text(line)))
    opts.append(gen.fix_anchors(cn.translate_line(line)))
    seen = set()
    out = []
    for o in opts:
        if o not in seen:
            seen.add(o)
            out.append(o)
    return out


def main() -> int:
    data = json.loads(UNIQUE.read_text(encoding="utf-8"))
    lines = []
    for v in data.values():
        lines.extend(v)
    seen = set()
    unique = []
    for l in lines:
        if l not in seen:
            seen.add(l)
            unique.append(l)

    manual: dict[str, str] = {}
    if OVERRIDES.exists():
        manual = json.loads(OVERRIDES.read_text(encoding="utf-8"))

    mapping = {}
    overrides = {}
    for line in unique:
        if line in manual and cn_count(manual[line]) == 0:
            best = manual[line]
            best_n = 0
        else:
            best = line
            best_n = cn_count(line)
            for opt in candidates(line):
                n = cn_count(opt)
                if n < best_n:
                    best, best_n = opt, n
        mapping[line] = best
        if best_n == 0:
            overrides[line] = best

    # Preserve all clean manual overrides (may include lines outside unique set)
    for k, v in manual.items():
        if cn_count(v) == 0:
            overrides[k] = v
            if k in mapping:
                mapping[k] = v

    OUT.write_text(json.dumps(mapping, ensure_ascii=False, indent=2), encoding="utf-8")
    OVERRIDES.write_text(json.dumps(overrides, ensure_ascii=False, indent=2), encoding="utf-8")
    still = [k for k, v in mapping.items() if cn_count(v) > 0]
    (ROOT / "tools" / "sp_still_chinese.json").write_text(
        json.dumps(still, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print(f"Mapped {len(mapping)}, clean {len(overrides)}, still Chinese {len(still)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
