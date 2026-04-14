#!/usr/bin/env python3
"""Final zh->en translator for Signal Processing wiki docs."""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
HANS = ROOT / "wiki_hans"
EN = ROOT / "wiki_en"
EXACT_JSON = ROOT / "tools" / "sp_exact_translations.json"

MAPPINGS = [
    ("Module_SignalProcessing_Signals_zh.md", "Module_SignalProcessing_Signals.md"),
    ("Module_SignalProcessing_Operations_zh.md", "Module_SignalProcessing_Operations.md"),
    ("Module_SignalProcessing_Filters_zh.md", "Module_SignalProcessing_Filters.md"),
]

LINKS = [
    ("HOME_zh.md", "HOME.md"),
    ("Module_SignalProcessing_zh.md", "Module_SignalProcessing.md"),
    ("Module_SignalProcessing_Signals_zh.md", "Module_SignalProcessing_Signals.md"),
    ("Module_SignalProcessing_Operations_zh.md", "Module_SignalProcessing_Operations.md"),
    ("Module_SignalProcessing_Filters_zh.md", "Module_SignalProcessing_Filters.md"),
    ("Module_ComputingContext_zh.md", "Module_ComputingContext.md"),
    ("Module_GPU_Policy_zh.md", "Module_GPU_Policy.md"),
    ("Module_Extensions_FFTW_zh.md", "Module_Extensions_FFTW.md"),
]


def fix_links(text: str) -> str:
    for a, b in LINKS:
        text = text.replace(a, b)
    return text


def fix_anchors(line: str) -> str:
    line = re.sub(r"\(#([^)]+)-类\)", r"(#\1-class)", line)
    line = re.sub(r"\(#([^)]+)-结构\)", r"(#\1-struct)", line)
    line = re.sub(r"\(#([^)]+)-接口\)", r"(#\1-interface)", line)
    line = re.sub(r"\(#([^)]+)-枚举\)", r"(#\1-enum)", line)
    line = re.sub(r"\[([^\]]+?) 类\]", r"[\1 class]", line)
    line = re.sub(r"\[([^\]]+?) 结构\]", r"[\1 struct]", line)
    line = re.sub(r"\[([^\]]+?) 枚举\]", r"[\1 enum]", line)
    line = re.sub(r"\[([^\]]+?) 接口\]", r"[\1 interface]", line)
    line = re.sub(r"^## (.+?) 类\s*$", r"## \1 class", line)
    line = re.sub(r"^## (.+?) 结构\s*$", r"## \1 struct", line)
    line = re.sub(r"^## (.+?) 接口\s*$", r"## \1 interface", line)
    line = re.sub(r"^## (.+?) 枚举\s*$", r"## \1 enum", line)
    line = re.sub(r"^#### (\d+)\. (.+?) 构造函数\s*$", r"#### \1. \2 constructor", line)
    line = re.sub(r"^#### (\d+)\. (.+?) 构造器\s*$", r"#### \1. \2 constructor", line)
    line = re.sub(r"^### 重载运算符\s*$", "### Operator overloads", line)
    return line


def cn_tail(s: str) -> str:
    if s.endswith("。"):
        return s[:-1] + "."
    if s.endswith("："):
        return s[:-1] + ":"
    return s


# Rule-based translation for lines still containing Chinese after exact lookup
RULES: list[tuple[str, str]] = []


def load_exact() -> dict[str, str]:
    if EXACT_JSON.exists():
        return json.loads(EXACT_JSON.read_text(encoding="utf-8"))
    return {}


def apply_rules(line: str) -> str:
    for pat, repl in RULES:
        line = re.sub(pat, repl, line)
    return line


def translate_line(line: str, exact: dict[str, str]) -> str:
    if not re.search(r"[\u4e00-\u9fff]", line):
        return fix_anchors(line)
    if line in exact:
        return exact[line]
    # fallback: import generator logic
    try:
        from generate_sp_exact import translate_line as gen_translate

        out = gen_translate(line)
        if re.search(r"[\u4e00-\u9fff]", out):
            import importlib.util
            spec = importlib.util.spec_from_file_location(
                "cn_to_en_sp", ROOT / "tools" / "cn_to_en_sp.py"
            )
            cn = importlib.util.module_from_spec(spec)
            spec.loader.exec_module(cn)
            alt = cn.translate_line(line)
            if len(re.findall(r"[\u4e00-\u9fff]", alt)) < len(re.findall(r"[\u4e00-\u9fff]", out)):
                return alt
        return out
    except Exception:
        out = apply_rules(line)
        return fix_anchors(out)


def translate_file(src: Path, dst: Path, exact: dict[str, str]) -> tuple[int, int]:
    text = fix_links(src.read_text(encoding="utf-8"))
    lines = [translate_line(l, exact) for l in text.splitlines()]
    out = "\n".join(lines) + ("\n" if text.endswith("\n") else "")
    dst.write_text(out, encoding="utf-8", newline="\n")
    cn = len(re.findall(r"[\u4e00-\u9fff]", out))
    return len(lines), cn


def collect_untranslated(src: Path, exact: dict[str, str]) -> list[str]:
    text = fix_links(src.read_text(encoding="utf-8"))
    missing = []
    seen = set()
    for line in text.splitlines():
        if re.search(r"[\u4e00-\u9fff]", line) and line not in exact and line not in seen:
            seen.add(line)
            missing.append(line)
    return missing


def main() -> int:
    if len(sys.argv) > 1 and sys.argv[1] == "--collect":
        exact = load_exact()
        all_missing = []
        for src_name, _ in MAPPINGS:
            all_missing.extend(collect_untranslated(HANS / src_name, exact))
        seen = set()
        uniq = []
        for l in all_missing:
            if l not in seen:
                seen.add(l)
                uniq.append(l)
        out = ROOT / "tools" / "sp_need_translation.json"
        out.write_text(json.dumps(uniq, ensure_ascii=False, indent=2), encoding="utf-8")
        print(f"Need translation: {len(uniq)} unique lines -> {out}")
        return 0

    exact = load_exact()
    for src_name, dst_name in MAPPINGS:
        lines, cn = translate_file(HANS / src_name, EN / dst_name, exact)
        print(f"{dst_name}: {lines} lines, {cn} Chinese chars remaining")
    return 0


if __name__ == "__main__":
    sys.exit(main())
