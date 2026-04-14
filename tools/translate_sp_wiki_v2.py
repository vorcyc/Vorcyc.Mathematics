#!/usr/bin/env python3
"""High-quality zh->en translation for Signal Processing wiki (Signals, Operations, Filters)."""
from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
HANS = ROOT / "wiki_hans"
EN = ROOT / "wiki_en"

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

BREADCRUMB_PAGES = {
    "信号定义和相关操作": ("Signals", "Module_SignalProcessing_Signals.md"),
    "常用操作": ("Operations", "Module_SignalProcessing_Operations.md"),
    "一维滤波器": ("Filters", "Module_SignalProcessing_Filters.md"),
}

SECTION = {
    "构造与工厂": "Constructors and factories",
    "主要属性": "Main properties",
    "切片与拷贝": "Slicing and copying",
    "频域与重采样": "Frequency domain and resampling",
    "参数设置": "Parameters",
    "枚举": "Enums",
    "属性": "Properties",
    "构造器": "Constructors",
    "方法": "Methods",
    "方法清单及说明": "Methods",
    "索引器": "Indexers",
    "隐式转换": "Implicit conversions",
    "运算符": "Operators",
    "代码示例": "Code example",
    "描述": "Description",
    "重写方法": "Overrides",
    "要点": "Notes",
    "快速示例": "Quick examples",
}

TABLE_HDR = {
    "| 类型 | 说明 |": "| Type | Description |",
    "| 成员 | 说明 |": "| Member | Description |",
    "| 方法 | 说明 |": "| Method | Description |",
    "| 属性 | 类型 | 说明 |": "| Property | Type | Description |",
    "| 值 | 说明 |": "| Value | Description |",
}

# Longest-first inline phrase replacements (avoid single-char)
INLINE = [
    ("当前位置 : [首页](HOME.md)/[信号处理模块](Module_SignalProcessing.md)/[信号定义和相关操作](Module_SignalProcessing_Signals.md)",
     "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Signals](Module_SignalProcessing_Signals.md)"),
    ("当前位置 : [首页](HOME.md)/[信号处理模块](Module_SignalProcessing.md)/[常用操作](Module_SignalProcessing_Operations.md)",
     "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Operations](Module_SignalProcessing_Operations.md)"),
    ("当前位置 : [首页](HOME.md)/[信号处理模块](Module_SignalProcessing.md)/[一维滤波器](Module_SignalProcessing_Filters.md)",
     "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Filters](Module_SignalProcessing_Filters.md)"),
    ("# 信号处理模块 - Signal Processing Module", "# Signal Processing Module"),
    ("## 信号定义和相关操作 - Signals", "## Signals"),
    ("## 常用操作 - Operations", "## Operations"),
    ("## 一维滤波器 - Filters", "## Filters"),
    ("`Vorcyc.Mathematics.SignalProcessing.Signals` 命名空间包含时域/频域信号定义与编辑工具。",
     "The `Vorcyc.Mathematics.SignalProcessing.Signals` namespace provides time-domain and frequency-domain signal definitions and editing utilities."),
    ("`Vorcyc.Mathematics.SignalProcessing.Operations` 命名空间包含多种常用的信号处理操作类，包括卷积操作类（如 `ComplexConvolver`、`Convolver`、`OlaBlockConvolver`、`OlsBlockConvolver`）、动态处理类（如 `DynamicsProcessor`）、包络跟随器类（如 `EnvelopeFollower`）、信号重建类（如 `GriffinLimReconstructor`）、谐波/打击乐分离器类（如 `HarmonicPercussiveSeparator`）、调制类（如 `Modulator`）、通用操作类（如 `Operation`）、重采样类（如 `Resampler`）、谱减法滤波类（如 `SpectralSubtractor`）和波形整形类（如 `WaveShaper`）。这些类提供了丰富的信号处理功能，适用于各种音频和信号处理需求。",
     "The `Vorcyc.Mathematics.SignalProcessing.Operations` namespace includes common signal-processing operations: convolution (`ComplexConvolver`, `Convolver`, `OlaBlockConvolver`, `OlsBlockConvolver`), dynamics (`DynamicsProcessor`), envelope following (`EnvelopeFollower`), reconstruction (`GriffinLimReconstructor`), harmonic/percussive separation (`HarmonicPercussiveSeparator`), modulation (`Modulator`), general utilities (`Operation`), resampling (`Resampler`), spectral subtraction (`SpectralSubtractor`), and waveshaping (`WaveShaper`). These types support a wide range of audio and signal-processing tasks."),
    ("`Vorcyc.Mathematics.SignalProcessing.Filters` 命名空间包含多种一维滤波器类，包括自适应滤波器（如 `LmfFilter`、`LmsFilter`、`NlmfFilter`、`NlmsFilter`、`RlsFilter` 等）、基础滤波器（如 `FilterChain`、`FirFilter`、`IirFilter`、`StereoFilter` 等）、贝塞尔滤波器、双二阶滤波器、巴特沃斯滤波器、切比雪夫滤波器、椭圆滤波器、FIR 滤波器设计器、单极滤波器、多相滤波器、梳状滤波器、直流去除滤波器、去加重滤波器、Hilbert 滤波器、中值滤波器、移动平均滤波器、预加重滤波器、RASTA 滤波器、Savitzky-Golay 滤波器、Thiran 滤波器和维纳滤波器。这些类提供了丰富的滤波功能，适用于各种信号处理需求。",
     "The `Vorcyc.Mathematics.SignalProcessing.Filters` namespace includes 1D filters: adaptive (`LmfFilter`, `LmsFilter`, `NlmfFilter`, `NlmsFilter`, `RlsFilter`, etc.), base types (`FilterChain`, `FirFilter`, `IirFilter`, `StereoFilter`, etc.), Bessel, biquad, Butterworth, Chebyshev, elliptic, FIR design (`Fda`), one-pole, polyphase, comb, DC removal, de-emphasis, Hilbert, median, moving average, pre-emphasis, RASTA, Savitzky–Golay, Thiran, and Wiener filters."),
    ("> **0.9 API 说明**：下列各类的 **首选重载均接受 `Signal`**（返回 `Signal` 或写入 `Signal.Samples`）。`Operation.Convolve`、`CrossCorrelate`、`BlockConvolve`、`Resample`、`TimeStretch`、`Envelope`、`FullRectify`、`HalfRectify`、`SpectralSubtract` 等均已提供 `Signal` 版本；`Convolver` / `Modulator` / `SpectralSubtractor` 内部使用 `ReadOnlySpan<float>` 零拷贝路径。文档中仍出现的 `DiscreteSignal` 签名表示旧 API，等价迁移为 `Signal` 即可（采样率改为 `float`）。",
     "> **0.9 API note**: Preferred overloads for the types below **accept `Signal`** (return `Signal` or write to `Signal.Samples`). `Operation.Convolve`, `CrossCorrelate`, `BlockConvolve`, `Resample`, `TimeStretch`, `Envelope`, `FullRectify`, `HalfRectify`, `SpectralSubtract`, and others have `Signal` versions; `Convolver` / `Modulator` / `SpectralSubtractor` use zero-copy `ReadOnlySpan<float>` internally. Remaining `DiscreteSignal` signatures are legacy; migrate to `Signal` (sample rate as `float`)."),
    ("**0.9 推荐 API**", "**0.9 recommended API**"),
    ("**兼容 / 过时**", "**Compatibility / obsolete**"),
    ("- `DiscreteSignal`、`DiscreteSignalExtensions`：仍可用，已 `[Obsolete]`；请改用 `Signal` / `SignalExtensions` 或 `DiscreteSignal.ToSignal()`。",
     "- `DiscreteSignal`, `DiscreteSignalExtensions`: still available, marked `[Obsolete]`; use `Signal` / `SignalExtensions` or `DiscreteSignal.ToSignal()`."),
    ("- 下文手册中保留 `DiscreteSignal` 章节供对照迁移；新代码请以 [Signal 类](#vorcycmathematicssignalprocessingsignalssignal-class) 为准。",
     "- The `DiscreteSignal` sections below are kept for migration reference; new code should use the [Signal class](#vorcycmathematicssignalprocessingsignalssignal-class)."),
    ("> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Signals",
     "> All types below are in namespace: Vorcyc.Mathematics.SignalProcessing.Signals"),
    ("> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Operations",
     "> All types below are in namespace: Vorcyc.Mathematics.SignalProcessing.Operations"),
    ("> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Filters",
     "> All types below are in namespace: Vorcyc.Mathematics.SignalProcessing.Filters"),
    (":ledger:目录  ", ":ledger: Contents  "),
    (":ledger:目录", ":ledger: Contents"),
    ("（**已过时**）", "(**obsolete**)"),
    ("无公开属性。", "No public properties."),
    ("无", "None"),
    ("以下是一个使用", "The following example uses "),
    (" 类中多个方法的示例，并在示例中加入了注释：", " with comments:"),
    ("可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：",
     "Set the following via `SignalBuilder.SetParameter(string, double)`:"),
    ("  - 参数:", "  - Parameters:"),
    ("  - 返回值:", "  - Returns:"),
    ("  - 异常:", "  - Exceptions:"),
    ("  - 枚举成员:", "  - Enum members:"),
    ("`Signal` 是 **0.9** 起推荐的单线程实值时域信号类型。样本存放在 `POHBuffer<float>` 中，通过 `Samples` 属性以 `Span<float>` 暴露，修改后需调用 `NotifySamplesModified()` 以失效缓存的时域特征。",
     "`Signal` is the recommended single-threaded real time-domain type from **0.9**. Samples live in `POHBuffer<float>` and are exposed as `Span<float>` via `Samples`; call `NotifySamplesModified()` after edits to invalidate cached time-domain features."),
    ("只读结构体，表示父 `Signal` 上连续样本的零拷贝视图。实现 `ITimeDomainSignal` / `ISingleThreadTimeDomainSignal`，帧级 `Rms`、`AverageEnergy`、`ZeroCrossingRate`、`Entropy` 等通过 SIMD 延迟计算。",
     "Readonly struct: zero-copy view of contiguous samples on a parent `Signal`. Implements `ITimeDomainSignal` / `ISingleThreadTimeDomainSignal`; frame-level `Rms`, `AverageEnergy`, `ZeroCrossingRate`, `Entropy`, etc. are lazily computed with SIMD."),
    ("自 `DiscreteSignalExtensions` 迁移的静态扩展方法（**新代码请用本类**）。",
     "Static extensions migrated from `DiscreteSignalExtensions` (**use this class in new code**)."),
    ("> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Signals.Builders 命名空间。",
     "> All types below are in namespace: Vorcyc.Mathematics.SignalProcessing.Signals.Builders"),
    ("> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Operations.Convolution 命名空间。",
     "> All types below are in namespace: Vorcyc.Mathematics.SignalProcessing.Operations.Convolution"),
    ("> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive 命名空间。",
     "> All types below are in namespace: Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive"),
]
INLINE.sort(key=lambda x: len(x[0]), reverse=True)

VERB_MAP = [
    ("获取", "Gets"),
    ("返回", "Returns"),
    ("计算", "Computes"),
    ("使用", "Uses"),
    ("判断", "Determines whether"),
    ("初始化", "Initializes"),
    ("执行", "Performs"),
    ("训练", "Trains"),
    ("预测", "Predicts"),
    ("创建", "Creates"),
    ("扩展", "Expands"),
    ("更新", "Updates"),
    ("估计", "Estimates"),
    ("添加", "Adds"),
    ("保存", "Saves"),
    ("加载", "Loads"),
    ("将", ""),
    ("对", ""),
    ("根据", ""),
    ("从", "Loads from"),
    ("在", ""),
    ("重置", "Resets"),
    ("设置", "Sets"),
    ("构造", "Constructs"),
    ("生成", "Generates"),
    ("滤波", "Filters"),
    ("应用", "Applies"),
    ("设计", "Designs"),
    ("表示", "Represents"),
    ("继承", "Inherits"),
    ("实现", "Implements"),
]

CLASS_DESC_PATTERNS = [
    (r"^(.+?) 是一个用于实现(.+?)的类。$", r"\1 is a class that implements \2."),
    (r"^(.+?) 是一个用于实现(.+?)自适应滤波算法的类。$", r"\1 is a class that implements the \2 adaptive filtering algorithm."),
    (r"^(.+?) 是一个用于实现(.+?)（(.+?)）自适应滤波算法的类。$", r"\1 implements the \2 (\3) adaptive filtering algorithm."),
    (r"^(.+?) 是一个用于实现复数值输入/输出的快速卷积（FFT）的类。$", r"\1 performs fast convolution (FFT) with complex-valued input/output."),
    (r"^(.+?) 是一个用于构建 (.+?) 包络的类，继承了 SignalBuilder 类。$", r"\1 builds \2 envelopes; inherits `SignalBuilder`."),
    (r"^(.+?) 是一个用于生成(.+?)的类，继承了 SignalBuilder 类。$", r"\1 generates \2; inherits `SignalBuilder`."),
    (r"^(.+?) 是一个用于生成(.+?)的类，继承了 SignalBuilder 类。使用 (.+?) 生成(.+?)。$", r"\1 generates \2; inherits `SignalBuilder`. Uses \3 to generate \4."),
    (r"^(.+?) 是一个用于(.+?)的类，继承了 (.+?) 类。$", r"\1 is used for \2; inherits \3."),
    (r"^(.+?) 是一个用于(.+?)的类。$", r"\1 is used for \2."),
    (r"^(.+?) 是一个(.+?)类。$", r"\1 is a \2 class."),
    (r"^(.+?) 是一个静态类，(.+?)。$", r"\1 is a static class that \2."),
    (r"^(.+?) 是一个抽象类，(.+?)。$", r"\1 is an abstract class that \2."),
]

PHRASE_SUBS = [
    ("最小均方四次（Least-Mean-Fourth）", "least-mean-fourth (Least-Mean-Fourth)"),
    ("加性白高斯噪声（AWGN）", "additive white Gaussian noise (AWGN)"),
    ("多项式除法和 FFT", "polynomial division and FFT"),
    ("快速卷积", "fast convolution"),
    ("快速互相关", "fast cross-correlation"),
    ("快速反卷积", "fast deconvolution"),
    ("攻击阶段", "attack phase"),
    ("衰减阶段", "decay phase"),
    ("持续阶段", "sustain phase"),
    ("释放阶段", "release phase"),
    ("攻击、衰减、持续、释放", "attack, decay, sustain, release"),
    ("样本数", "sample count"),
    ("采样率", "sample rate"),
    ("输入信号", "input signal"),
    ("输出信号", "output signal"),
    ("期望信号", "desired signal"),
    ("卷积核", "kernel"),
    ("滤波器", "filter"),
    ("步长因子", "step size"),
    ("泄漏因子", "leakage factor"),
    ("生成的样本", "generated sample"),
    ("生成的 `DiscreteSignal` 对象", "generated `DiscreteSignal`"),
    ("生成的 `Signal` 对象", "generated `Signal`"),
    ("设置采样率后的 SignalBuilder 实例", "`SignalBuilder` after setting sample rate"),
    ("重置样本生成器", "resets the sample generator"),
    ("设置信号的采样率", "sets the signal sample rate"),
    ("生成新的样本", "generates the next sample"),
    ("生成信号，通过逐个生成所有样本", "generates the signal sample by sample"),
    ("均值，默认值为", "mean; default"),
    ("标准差，默认值为", "standard deviation; default"),
    ("下限幅度，默认值为", "lower amplitude; default"),
    ("上限幅度，默认值为", "upper amplitude; default"),
    ("起始频率，默认值为", "start frequency; default"),
    ("结束频率，默认值为", "end frequency; default"),
    ("零拷贝", "zero-copy"),
    ("逐样本", "per-sample"),
    ("逐元素", "element-wise"),
    ("已过时", "obsolete"),
    ("构造函数", "constructor"),
    ("枚举成员", "enum members"),
    ("命名空间", "namespace"),
    ("目录", "Contents"),
    ("说明", "description"),
    ("返回值", "return value"),
    ("参数", "parameters"),
    ("实例", "instance"),
    ("默认值为", "default"),
    ("的类", " class"),
    ("类", "class"),
    ("结构", "struct"),
    ("枚举", "enum"),
    ("接口", "interface"),
]
PHRASE_SUBS.sort(key=lambda x: len(x[0]), reverse=True)


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
    line = re.sub(r"^#### \d+\. (.+?) 构造函数\s*$", lambda m: f"#### {m.group(0).split('.')[0].replace('####','####')} {m.group(1)} constructor".replace("####  ", "#### "), line)
    return line


def translate_doc_bullet(line: str) -> str:
    m = re.match(r"^(\s*- )(.+)$", line)
    if not m:
        return line
    prefix, rest = m.group(1), m.group(2)
    if rest.startswith("参数:"):
        return f"{prefix}Parameters:"
    if rest.startswith("返回值:"):
        return f"{prefix}Returns:"
    if rest.startswith("异常:"):
        return f"{prefix}Exceptions:"
    if rest.startswith("枚举成员:"):
        return f"{prefix}Enum members:"
    for zh, en in VERB_MAP:
        if rest.startswith(zh):
            tail = rest[len(zh):].lstrip("了").lstrip()
            if en:
                return f"{prefix}{en} {tail}".rstrip()
            return f"{prefix}{tail}"
    pm = re.match(r"^`([^`]+)`: (.+)$", rest)
    if pm:
        desc = pm.group(2)
        for zh, en in PHRASE_SUBS:
            desc = desc.replace(zh, en)
        if desc.endswith("。"):
            desc = desc[:-1] + "."
        return f"{prefix}`{pm.group(1)}`: {desc}"
    return line


def apply_phrases(line: str) -> str:
    for zh, en in INLINE:
        if zh in line:
            line = line.replace(zh, en)
    for zh, en in PHRASE_SUBS:
        line = line.replace(zh, en)
    return line


def translate_line(line: str) -> str:
    if not re.search(r"[\u4e00-\u9fff]", line):
        return line
    orig = line
    line = apply_phrases(line)
    line = fix_anchors(line)
    if line in TABLE_HDR:
        return TABLE_HDR[line]
    m = re.match(r"^### (.+)$", line)
    if m and m.group(1) in SECTION:
        return f"### {SECTION[m.group(1)]}"
    for pat, repl in CLASS_DESC_PATTERNS:
        if re.search(r"[\u4e00-\u9fff]", line):
            line2 = re.sub(pat, repl, line)
            if line2 != line:
                line = line2
                break
    line = translate_doc_bullet(line)
    if line.endswith("。"):
        line = line[:-1] + "."
    if line.endswith("："):
        line = line[:-1] + ":"
    # second pass phrases for leftovers in bullets
    if re.search(r"[\u4e00-\u9fff]", line):
        for zh, en in PHRASE_SUBS:
            line = line.replace(zh, en)
    if re.search(r"[\u4e00-\u9fff]", line) and line == orig:
        # generic verb prefixes on indented doc lines
        for zh, en in VERB_MAP:
            ind = "  - " + zh
            if line.startswith(ind):
                tail = line[len(ind):].lstrip()
                line = f"  - {en + ' ' if en else ''}{tail}".rstrip()
                break
    return line


def translate_content(text: str) -> str:
    text = fix_links(text)
    lines = [translate_line(l) for l in text.splitlines()]
    return "\n".join(lines) + ("\n" if text.endswith("\n") else "")


def main() -> int:
    for src_name, dst_name in MAPPINGS:
        src = HANS / src_name
        dst = EN / dst_name
        out = translate_content(src.read_text(encoding="utf-8"))
        dst.write_text(out, encoding="utf-8", newline="\n")
        cn = len(re.findall(r"[\u4e00-\u9fff]", out))
        cn_lines = sum(1 for l in out.splitlines() if re.search(r"[\u4e00-\u9fff]", l))
        print(f"{dst_name}: {len(out.splitlines())} lines, {cn} cn chars, {cn_lines} cn lines")
    return 0


if __name__ == "__main__":
    sys.exit(main())
