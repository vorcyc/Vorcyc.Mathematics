#!/usr/bin/env python3
"""Translate Signal Processing wiki pages from Chinese to English."""
from __future__ import annotations

import json
import re
import sys
import time
from pathlib import Path

from deep_translator import MyMemoryTranslator
from deep_translator.exceptions import TooManyRequests

ROOT = Path(__file__).resolve().parents[1]
HANS = ROOT / "wiki_hans"
EN = ROOT / "wiki_en"
CACHE_FILE = ROOT / "tools" / ".wiki_translate_cache.json"

FILES = [
    (
        "Module_SignalProcessing_Signals_zh.md",
        "Module_SignalProcessing_Signals.md",
        "Signals",
        "信号定义和相关操作",
    ),
    (
        "Module_SignalProcessing_Operations_zh.md",
        "Module_SignalProcessing_Operations.md",
        "Operations",
        "常用操作",
    ),
    (
        "Module_SignalProcessing_Filters_zh.md",
        "Module_SignalProcessing_Filters.md",
        "Filters",
        "一维滤波器",
    ),
]

CHINESE_RE = re.compile(r"[\u4e00-\u9fff]")
BACKTICK_RE = re.compile(r"`[^`]*`")
SPLIT_MARKER = "<<<WIKI_LINE_SPLIT>>>"
MAX_BATCH_CHARS = 450

STATIC_PHRASES: list[tuple[str, str]] = [
    (
        "`Vorcyc.Mathematics.SignalProcessing.Signals` 命名空间包含时域/频域信号定义与编辑工具。",
        "The `Vorcyc.Mathematics.SignalProcessing.Signals` namespace provides time-domain and frequency-domain signal definitions and editing utilities.",
    ),
    (
        "`Vorcyc.Mathematics.SignalProcessing.Operations` 命名空间包含多种常用的信号处理操作类，包括卷积操作类（如 `ComplexConvolver`、`Convolver`、`OlaBlockConvolver`、`OlsBlockConvolver`）、动态处理类（如 `DynamicsProcessor`）、包络跟随器类（如 `EnvelopeFollower`）、信号重建类（如 `GriffinLimReconstructor`）、谐波/打击乐分离器类（如 `HarmonicPercussiveSeparator`）、调制类（如 `Modulator`）、通用操作类（如 `Operation`）、重采样类（如 `Resampler`）、谱减法滤波类（如 `SpectralSubtractor`）和波形整形类（如 `WaveShaper`）。这些类提供了丰富的信号处理功能，适用于各种音频和信号处理需求。",
        "The `Vorcyc.Mathematics.SignalProcessing.Operations` namespace includes common signal-processing operations: convolution (`ComplexConvolver`, `Convolver`, `OlaBlockConvolver`, `OlsBlockConvolver`), dynamics (`DynamicsProcessor`), envelope following (`EnvelopeFollower`), reconstruction (`GriffinLimReconstructor`), harmonic/percussive separation (`HarmonicPercussiveSeparator`), modulation (`Modulator`), general utilities (`Operation`), resampling (`Resampler`), spectral subtraction (`SpectralSubtractor`), and waveshaping (`WaveShaper`). These types support a wide range of audio and signal-processing tasks.",
    ),
    (
        "`Vorcyc.Mathematics.SignalProcessing.Filters` 命名空间包含多种一维滤波器类，包括自适应滤波器（如 `LmfFilter`、`LmsFilter`、`NlmfFilter`、`NlmsFilter`、`RlsFilter` 等）、基础滤波器（如 `FilterChain`、`FirFilter`、`IirFilter`、`StereoFilter` 等）、贝塞尔滤波器、双二阶滤波器、巴特沃斯滤波器、切比雪夫滤波器、椭圆滤波器、FIR 滤波器设计器、单极滤波器、多相滤波器、梳状滤波器、直流去除滤波器、去加重滤波器、Hilbert 滤波器、中值滤波器、移动平均滤波器、预加重滤波器、RASTA 滤波器、Savitzky-Golay 滤波器、Thiran 滤波器和维纳滤波器。这些类提供了丰富的滤波功能，适用于各种信号处理需求。",
        "The `Vorcyc.Mathematics.SignalProcessing.Filters` namespace includes 1D filters: adaptive (`LmfFilter`, `LmsFilter`, `NlmfFilter`, `NlmsFilter`, `RlsFilter`, etc.), base types (`FilterChain`, `FirFilter`, `IirFilter`, `StereoFilter`, etc.), Bessel, biquad, Butterworth, Chebyshev, elliptic, FIR design (`Fda`), one-pole, polyphase, comb, DC removal, de-emphasis, Hilbert, median, moving average, pre-emphasis, RASTA, Savitzky–Golay, Thiran, and Wiener filters.",
    ),
    (
        "> **0.9 API 说明**：下列各类的 **首选重载均接受 `Signal`**（返回 `Signal` 或写入 `Signal.Samples`）。`Operation.Convolve`、`CrossCorrelate`、`BlockConvolve`、`Resample`、`TimeStretch`、`Envelope`、`FullRectify`、`HalfRectify`、`SpectralSubtract` 等均已提供 `Signal` 版本；`Convolver` / `Modulator` / `SpectralSubtractor` 内部使用 `ReadOnlySpan<float>` 零拷贝路径。文档中仍出现的 `DiscreteSignal` 签名表示旧 API，等价迁移为 `Signal` 即可（采样率改为 `float`）。",
        "> **0.9 API note**: Preferred overloads for the types below **accept `Signal`** (return `Signal` or write to `Signal.Samples`). `Operation.Convolve`, `CrossCorrelate`, `BlockConvolve`, `Resample`, `TimeStretch`, `Envelope`, `FullRectify`, `HalfRectify`, `SpectralSubtract`, and others have `Signal` versions; `Convolver` / `Modulator` / `SpectralSubtractor` use zero-copy `ReadOnlySpan<float>` internally. Remaining `DiscreteSignal` signatures are legacy; migrate to `Signal` (sample rate as `float`).",
    ),
    ("### 构造与工厂", "### Construction and factories"),
    ("### 主要属性", "### Main properties"),
    ("### 切片与拷贝", "### Slicing and copying"),
    ("### 频域与重采样", "### Frequency domain and resampling"),
    ("### 运算符", "### Operators"),
    ("### 方法", "### Methods"),
    ("### 属性", "### Properties"),
    ("### 枚举", "### Enums"),
    ("### 参数设置", "### Parameter settings"),
    ("### 代码示例", "### Code example"),
    ("| 类型 | 说明 |", "| Type | Description |"),
    ("| 成员 | 说明 |", "| Member | Description |"),
    ("| 方法 | 说明 |", "| Method | Description |"),
    ("| 字段 | 说明 |", "| Field | Description |"),
    ("| 属性 | 说明 |", "| Property | Description |"),
    ("| 参数 | 说明 |", "| Parameter | Description |"),
    ("| 枚举值 | 说明 |", "| Enum value | Description |"),
    ("**0.9 推荐 API**", "**0.9 recommended API**"),
    ("**兼容 / 过时**", "**Compatibility / obsolete**"),
    ("无", "None"),
    ("  - 参数:", "  - Parameters:"),
    ("  - 返回值:", "  - Returns:"),
    ("  - 枚举成员:", "  - Enum members:"),
]
STATIC_PHRASES.sort(key=lambda x: len(x[0]), reverse=True)

POST_FIXES: list[tuple[str, str]] = [
    ("HOME_zh.md", "HOME.md"),
    ("_zh.md", ".md"),
    (" class class", " class"),
    ("enum class", "enum"),
    ("struct class", "struct"),
    ("constructor class", "constructor"),
    ("Signal Processing Module - Signal Processing Module", "Signal Processing Module"),
    ("目录", "Contents"),
    ("首页", "Home"),
    ("信号处理模块", "Signal processing"),
    ("当前位置", "Location"),
]


def load_cache() -> dict[str, str]:
    if CACHE_FILE.exists():
        return json.loads(CACHE_FILE.read_text(encoding="utf-8"))
    return {}


def save_cache(cache: dict[str, str]) -> None:
    CACHE_FILE.write_text(json.dumps(cache, ensure_ascii=False, indent=2), encoding="utf-8")


def fix_links_and_anchors(text: str) -> str:
    text = text.replace("HOME_zh.md", "HOME.md")
    text = re.sub(r"_zh\.md", ".md", text)
    text = re.sub(r"#([^)\s]+)-类\)", r"#\1-class)", text)
    text = re.sub(r"#([^)\s]+)-结构\)", r"#\1-struct)", text)
    text = re.sub(r"#([^)\s]+)-枚举\)", r"#\1-enum)", text)
    text = re.sub(r"## ([^\n]+) 类\s*$", r"## \1 class", text, flags=re.M)
    text = re.sub(r"## ([^\n]+) 结构\s*$", r"## \1 struct", text, flags=re.M)
    text = re.sub(r"## ([^\n]+) 枚举\s*$", r"## \1 enum", text, flags=re.M)
    text = re.sub(r"#### \d+\. ([^\n]+) 构造函数\s*$", r"#### \1 constructor", text, flags=re.M)
    text = re.sub(r"- :bookmark: \[([^\]]+) 类\]", r"- :bookmark: [\1 class]", text)
    text = re.sub(r"- :bookmark: \[([^\]]+) 结构\]", r"- :bookmark: [\1 struct]", text)
    text = re.sub(r"- :bookmark: \[([^\]]+) 枚举\]", r"- :bookmark: [\1 enum]", text)
    text = re.sub(r"（\*\*已过时\*\*）", "(**obsolete**)", text)
    text = re.sub(r"（\*\*已过时\*\*,", "(**obsolete**,", text)
    return text


def fix_header(text: str, section_en: str, section_zh: str) -> str:
    module = f"Module_SignalProcessing_{section_en}.md"
    text = re.sub(
        r"^当前位置\s*:.*$",
        f"Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [{section_en}]({module})",
        text,
        count=1,
        flags=re.M,
    )
    text = re.sub(
        r"^# 信号处理模块 - Signal Processing Module\s*$",
        "# Signal Processing Module",
        text,
        count=1,
        flags=re.M,
    )
    text = re.sub(
        rf"^## {re.escape(section_zh)} - {re.escape(section_en)}\s*$",
        f"## {section_en}",
        text,
        count=1,
        flags=re.M,
    )
    text = re.sub(r":ledger:目录", ":ledger: Contents", text)
    text = re.sub(r"> 以下类型均位于命名空间 ：", "> All types below are in namespace: ", text)
    text = re.sub(r"> 以下类型均位于命名空间:", "> All types below are in namespace: ", text)
    text = re.sub(
        r"> 以下类型均位于 ([^\n]+) 命名空间。",
        r"> All types below are in \1 namespace.",
        text,
    )
    return text


def protect_backticks(text: str) -> tuple[str, list[str]]:
    parts: list[str] = []

    def repl(match: re.Match[str]) -> str:
        parts.append(match.group(0))
        return f"__BT{len(parts) - 1}__"

    return BACKTICK_RE.sub(repl, text), parts


def restore_backticks(text: str, parts: list[str]) -> str:
    for i, part in enumerate(parts):
        text = text.replace(f"__BT{i}__", part)
    return text


def apply_static_phrases(text: str) -> str:
    for old, new in STATIC_PHRASES:
        if old in text:
            text = text.replace(old, new)
    return text


def post_fix(text: str) -> str:
    text = apply_static_phrases(text)
    for old, new in POST_FIXES:
        text = text.replace(old, new)
    return text


def translate_batch(lines: list[str], translator: MyMemoryTranslator, cache: dict[str, str]) -> list[str]:
    pending: list[str] = []
    pending_idx: list[int] = []
    results = list(lines)

    for i, line in enumerate(lines):
        if line in cache:
            results[i] = cache[line]
            continue
        pre = apply_static_phrases(line)
        if pre != line:
            cache[line] = pre
            results[i] = pre
        elif CHINESE_RE.search(line):
            pending.append(line)
            pending_idx.append(i)

    for n, (line, idx) in enumerate(zip(pending, pending_idx), start=1):
        protected, parts = protect_backticks(line)
        translated = protected
        for attempt in range(10):
            try:
                translated = translator.translate(protected)
                break
            except TooManyRequests:
                wait = 20 * (attempt + 1)
                print(f"Rate limited; sleeping {wait}s...", file=sys.stderr)
                time.sleep(wait)
            except Exception as exc:  # noqa: BLE001
                if attempt == 9:
                    raise
                time.sleep(3 * (attempt + 1))
                print(f"Retry translate after error: {exc}", file=sys.stderr)

        translated = restore_backticks(translated.strip(), parts)
        translated = post_fix(translated)
        cache[line] = translated
        results[idx] = translated
        if n % 25 == 0 or n == len(pending):
            save_cache(cache)
            print(f"  translated {n}/{len(pending)} lines", file=sys.stderr)
        time.sleep(1.5)

    return results


def translate_file(src_name: str, dst_name: str, section_en: str, section_zh: str, translator: MyMemoryTranslator, cache: dict[str, str]) -> tuple[int, int]:
    content = (HANS / src_name).read_text(encoding="utf-8")
    content = fix_links_and_anchors(content)
    content = fix_header(content, section_en, section_zh)

    lines = content.splitlines()
    out: list[str] = []
    in_fence = False
    prose_block: list[str] = []
    code_block: list[str] = []

    def flush_prose() -> None:
        nonlocal prose_block
        if prose_block:
            out.extend(translate_batch(prose_block, translator, cache))
            prose_block = []

    def flush_code() -> None:
        nonlocal code_block
        if not code_block:
            return
        translated: list[str] = []
        for line in code_block:
            if line.strip().startswith("//") and CHINESE_RE.search(line):
                if line in cache:
                    translated.append(cache[line])
                else:
                    protected, parts = protect_backticks(line)
                    try:
                        for attempt in range(8):
                            try:
                                t = translator.translate(protected)
                                break
                            except TooManyRequests:
                                time.sleep(15 * (attempt + 1))
                        time.sleep(1.2)
                    except Exception:
                        t = line
                    t = restore_backticks(post_fix(t), parts)
                    cache[line] = t
                    translated.append(t)
            else:
                translated.append(line)
        out.extend(translated)
        code_block = []

    for line in lines:
        if line.strip().startswith("```"):
            if in_fence:
                flush_code()
            else:
                flush_prose()
            in_fence = not in_fence
            out.append(line)
            continue
        if in_fence:
            code_block.append(line)
        else:
            prose_block.append(line)

    flush_prose()
    flush_code()
    result = "\n".join(out) + "\n"
    result = fix_links_and_anchors(result)
    (EN / dst_name).write_text(result, encoding="utf-8", newline="\n")

    chinese_lines = sum(1 for line in out if CHINESE_RE.search(line))
    return len(out), chinese_lines


def main() -> int:
    cache = load_cache()
    translator = MyMemoryTranslator(source="zh-CN", target="en-US")
    stats: list[tuple[str, int, int]] = []

    for src_name, dst_name, section_en, section_zh in FILES:
        print(f"Translating {src_name} -> {dst_name}...", file=sys.stderr)
        line_count, chinese_lines = translate_file(
            src_name, dst_name, section_en, section_zh, translator, cache
        )
        stats.append((dst_name, line_count, chinese_lines))
        print(f"{dst_name}: {line_count} lines, {chinese_lines} lines with Chinese remaining")

    save_cache(cache)
    return 0 if all(c == 0 for _, _, c in stats) else 1


if __name__ == "__main__":
    sys.exit(main())
