import re
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")
ROOT = Path(__file__).resolve().parents[1]
for fname in [
    "Module_SignalProcessing_Signals.md",
    "Module_SignalProcessing_Operations.md",
    "Module_SignalProcessing_Filters.md",
]:
    text = (ROOT / "wiki_en" / fname).read_text(encoding="utf-8")
    lines = text.splitlines()
    cn_lines = [l for l in lines if re.search(r"[\u4e00-\u9fff]", l)]
    chars = len(re.findall(r"[\u4e00-\u9fff]", text))
    print(f"=== {fname}: {len(cn_lines)} lines, {chars} chars")
    for l in cn_lines[:10]:
        print(l[:140])
    print()
