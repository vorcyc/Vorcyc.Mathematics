#!/usr/bin/env python3
"""Fix sp_exact_translations.json by merging full-line mappings and safe re-translation."""
from __future__ import annotations

import importlib.util
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXACT_JSON = ROOT / "tools" / "sp_exact_translations.json"
UNIQUE_JSON = ROOT / "tools" / "sp_unique_cn_lines.json"
OVERRIDES_JSON = ROOT / "tools" / "sp_manual_overrides.json"
GEN_PATH = ROOT / "tools" / "generate_sp_exact.py"
TWP_PATH = ROOT / "tools" / "translate_wiki_signal_processing.py"

_spec = importlib.util.spec_from_file_location("generate_sp_exact", GEN_PATH)
_gen = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_gen)

_twp_spec = importlib.util.spec_from_file_location("twp", TWP_PATH)
_twp = importlib.util.module_from_spec(_twp_spec)
_twp_spec.loader.exec_module(_twp)

# Start with generator EXACT + full-line PHRASES from twp
EXACT: dict[str, str] = dict(_gen.EXACT)
for zh, en in _twp.PHRASES:
    if re.search(r"[\u4e00-\u9fff]", zh) and not re.search(r"[\u4e00-\u9fff]", en):
        EXACT[zh] = en

# Load manual overrides if present
if OVERRIDES_JSON.exists():
    EXACT.update(json.loads(OVERRIDES_JSON.read_text(encoding="utf-8")))

# Safe phrases: min 3 Chinese chars, sorted longest first
SAFE: list[tuple[str, str]] = []
seen = set()
for zh, en in _twp.PHRASES:
    if len(zh) >= 3 and re.search(r"[\u4e00-\u9fff]", zh):
        if zh not in seen:
            SAFE.append((zh, en))
            seen.add(zh)
SAFE.sort(key=lambda x: len(x[0]), reverse=True)


def safe_translate(text: str) -> str:
    for zh, en in SAFE:
        if zh in text:
            text = text.replace(zh, en)
    return text


def translate_line(line: str) -> str:
    if line in EXACT:
        return EXACT[line]
    out = _gen.translate_line(line)
    if re.search(r"[\u4e00-\u9fff]", out):
        out2 = safe_translate(out)
        if out2 != out:
            out = out2
        # second pass from Chinese key directly for bullets
        if re.search(r"[\u4e00-\u9fff]", out) and line != out:
            out3 = _gen.translate_line(line)
            if not re.search(r"[\u4e00-\u9fff]", out3):
                out = out3
    return out


def cn_score(s: str) -> int:
    return len(re.findall(r"[\u4e00-\u9fff]", s))


def main() -> int:
    data = json.loads(UNIQUE_JSON.read_text(encoding="utf-8"))
    all_lines: list[str] = []
    for v in data.values():
        all_lines.extend(v)
    seen: set[str] = set()
    unique: list[str] = []
    for line in all_lines:
        if line not in seen:
            seen.add(line)
            unique.append(line)

    mapping = {line: translate_line(line) for line in unique}
    EXACT_JSON.write_text(json.dumps(mapping, ensure_ascii=False, indent=2), encoding="utf-8")

    remaining = [k for k, v in mapping.items() if cn_score(v) > 0]
    (ROOT / "tools" / "sp_still_chinese.json").write_text(
        json.dumps(remaining, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print(f"Generated {len(mapping)} translations, {len(remaining)} still have Chinese")
    return 0


if __name__ == "__main__":
    sys.exit(main())
