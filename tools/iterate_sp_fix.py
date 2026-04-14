#!/usr/bin/env python3
"""Iteratively improve overrides until Chinese count stops decreasing."""
from __future__ import annotations

import importlib.util
import json
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OVERRIDES = ROOT / "tools" / "sp_manual_overrides.json"
EXACT = ROOT / "tools" / "sp_exact_translations.json"


def cn_count(path: Path) -> int:
    return len(re.findall(r"[\u4e00-\u9fff]", path.read_text(encoding="utf-8")))


def load_cn():
    spec = importlib.util.spec_from_file_location("cn", ROOT / "tools" / "cn_to_en_sp.py")
    cn = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(cn)
    return cn


def main() -> int:
    cn = load_cn()
    overrides: dict[str, str] = {}
    if OVERRIDES.exists():
        overrides = json.loads(OVERRIDES.read_text(encoding="utf-8"))

    exact = json.loads(EXACT.read_text(encoding="utf-8"))
    improved = 0
    for zh, cur in exact.items():
        if not re.search(r"[\u4e00-\u9fff]", cur):
            continue
        tr = cn.translate_line(zh)
        cur_n = len(re.findall(r"[\u4e00-\u9fff]", cur))
        tr_n = len(re.findall(r"[\u4e00-\u9fff]", tr))
        if tr_n < cur_n:
            overrides[zh] = tr
            improved += 1

    OVERRIDES.write_text(json.dumps(overrides, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Improved {improved} overrides, total {len(overrides)}")

    subprocess.run([sys.executable, str(ROOT / "tools" / "generate_sp_exact.py")], check=True)
    subprocess.run([sys.executable, str(ROOT / "tools" / "translate_sp_final.py")], check=True)

    for name in [
        "Module_SignalProcessing_Signals.md",
        "Module_SignalProcessing_Operations.md",
        "Module_SignalProcessing_Filters.md",
    ]:
        n = cn_count(ROOT / "wiki_en" / name)
        print(f"{name}: {n} Chinese chars")
    return 0


if __name__ == "__main__":
    sys.exit(main())
