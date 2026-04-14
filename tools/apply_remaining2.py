#!/usr/bin/env python3
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EN = ROOT / "wiki_en"
TRANS = json.loads((ROOT / "tools" / "remaining2_translations.json").read_text(encoding="utf-8"))

FILES = ["Module_MachineLearning.md", "Module_LinearAlgebra.md", "Module_Experimental.md"]


def process(path: Path) -> int:
    text = path.read_text(encoding="utf-8")
    lines = text.splitlines()
    out = []
    for line in lines:
        if line in TRANS:
            out.append(TRANS[line])
        else:
            out.append(line)
    new_text = "\n".join(out) + ("\n" if text.endswith("\n") else "")
    path.write_text(new_text, encoding="utf-8", newline="\n")
    return len(re.findall(r"[\u4e00-\u9fff]", new_text))


def main():
    for name in FILES:
        n = process(EN / name)
        lines = len((EN / name).read_text(encoding="utf-8").splitlines())
        print(f"{name}: {lines} lines, {n} Chinese chars left")
    return 0


if __name__ == "__main__":
    sys.exit(main())
