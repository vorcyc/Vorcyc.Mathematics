#!/usr/bin/env python3
"""Generate exact line translations for Signal Processing wiki."""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
UNIQUE_JSON = ROOT / "tools" / "sp_unique_cn_lines.json"
OUT_JSON = ROOT / "tools" / "sp_exact_translations.json"
OVERRIDES_JSON = ROOT / "tools" / "sp_manual_overrides.json"

LINKS = [
    ("HOME_zh.md", "HOME.md"),
    ("Module_SignalProcessing_zh.md", "Module_SignalProcessing.md"),
    ("Module_SignalProcessing_Signals_zh.md", "Module_SignalProcessing_Signals.md"),
    ("Module_SignalProcessing_Operations_zh.md", "Module_SignalProcessing_Operations.md"),
    ("Module_SignalProcessing_Filters_zh.md", "Module_SignalProcessing_Filters.md"),
    ("Module_ComputingContext_zh.md", "Module_ComputingContext.md"),
]

# Full-line exact translations (longest keys win when applied via exact lookup)
EXACT: dict[str, str] = {
    "当前位置 : [首页](HOME_zh.md)/[信号处理模块](Module_SignalProcessing_zh.md)/[信号定义和相关操作](Module_SignalProcessing_Signals_zh.md)":
    "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Signals](Module_SignalProcessing_Signals.md)",
    "当前位置 : [首页](HOME.md)/[信号处理模块](Module_SignalProcessing.md)/[信号定义和相关操作](Module_SignalProcessing_Signals.md)":
    "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Signals](Module_SignalProcessing_Signals.md)",
    "当前位置 : [首页](HOME_zh.md)/[信号处理模块](Module_SignalProcessing_zh.md)/[常用操作](Module_SignalProcessing_Operations_zh.md)":
    "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Operations](Module_SignalProcessing_Operations.md)",
    "当前位置 : [首页](HOME.md)/[信号处理模块](Module_SignalProcessing.md)/[常用操作](Module_SignalProcessing_Operations.md)":
    "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Operations](Module_SignalProcessing_Operations.md)",
    "当前位置 : [首页](HOME_zh.md)/[信号处理模块](Module_SignalProcessing_zh.md)/[一维滤波器](Module_SignalProcessing_Filters_zh.md)":
    "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Filters](Module_SignalProcessing_Filters.md)",
    "当前位置 : [首页](HOME.md)/[信号处理模块](Module_SignalProcessing.md)/[一维滤波器](Module_SignalProcessing_Filters.md)":
    "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Filters](Module_SignalProcessing_Filters.md)",
    "- `TransformToFrequencyDomain(WindowType?, FftVersion)` → `FrequencyDomain`（**已过时**，见 [ComputingContext](Module_ComputingContext_zh.md)）":
    "- `TransformToFrequencyDomain(WindowType?, FftVersion)` → `FrequencyDomain` (**obsolete**; see [ComputingContext](Module_ComputingContext.md))",
    "- `TransformToFrequencyDomain(WindowType?, FftVersion)` → `FrequencyDomain`（**已过时**，见 [ComputingContext](Module_ComputingContext.md)）":
    "- `TransformToFrequencyDomain(WindowType?, FftVersion)` → `FrequencyDomain` (**obsolete**; see [ComputingContext](Module_ComputingContext.md))",
    "# 信号处理模块 - Signal Processing Module": "# Signal Processing Module",
    "## 信号定义和相关操作 - Signals": "## Signals",
    "## 常用操作 - Operations": "## Operations",
    "## 一维滤波器 - Filters": "## Filters",
    "`Vorcyc.Mathematics.SignalProcessing.Signals` 命名空间包含时域/频域信号定义与编辑工具。":
    "The `Vorcyc.Mathematics.SignalProcessing.Signals` namespace provides time-domain and frequency-domain signal definitions and editing utilities.",
    "`Vorcyc.Mathematics.SignalProcessing.Operations` 命名空间包含多种常用的信号处理操作类，包括卷积操作类（如 `ComplexConvolver`、`Convolver`、`OlaBlockConvolver`、`OlsBlockConvolver`）、动态处理类（如 `DynamicsProcessor`）、包络跟随器类（如 `EnvelopeFollower`）、信号重建类（如 `GriffinLimReconstructor`）、谐波/打击乐分离器类（如 `HarmonicPercussiveSeparator`）、调制类（如 `Modulator`）、通用操作类（如 `Operation`）、重采样类（如 `Resampler`）、谱减法滤波类（如 `SpectralSubtractor`）和波形整形类（如 `WaveShaper`）。这些类提供了丰富的信号处理功能，适用于各种音频和信号处理需求。":
    "The `Vorcyc.Mathematics.SignalProcessing.Operations` namespace includes common signal-processing operations: convolution (`ComplexConvolver`, `Convolver`, `OlaBlockConvolver`, `OlsBlockConvolver`), dynamics (`DynamicsProcessor`), envelope following (`EnvelopeFollower`), reconstruction (`GriffinLimReconstructor`), harmonic/percussive separation (`HarmonicPercussiveSeparator`), modulation (`Modulator`), general utilities (`Operation`), resampling (`Resampler`), spectral subtraction (`SpectralSubtractor`), and waveshaping (`WaveShaper`). These types support a wide range of audio and signal-processing tasks.",
    "`Vorcyc.Mathematics.SignalProcessing.Filters` 命名空间包含多种一维滤波器类，包括自适应滤波器（如 `LmfFilter`、`LmsFilter`、`NlmfFilter`、`NlmsFilter`、`RlsFilter` 等）、基础滤波器（如 `FilterChain`、`FirFilter`、`IirFilter`、`StereoFilter` 等）、贝塞尔滤波器、双二阶滤波器、巴特沃斯滤波器、切比雪夫滤波器、椭圆滤波器、FIR 滤波器设计器、单极滤波器、多相滤波器、梳状滤波器、直流去除滤波器、去加重滤波器、Hilbert 滤波器、中值滤波器、移动平均滤波器、预加重滤波器、RASTA 滤波器、Savitzky-Golay 滤波器、Thiran 滤波器和维纳滤波器。这些类提供了丰富的滤波功能，适用于各种信号处理需求。":
    "The `Vorcyc.Mathematics.SignalProcessing.Filters` namespace includes 1D filters: adaptive (`LmfFilter`, `LmsFilter`, `NlmfFilter`, `NlmsFilter`, `RlsFilter`, etc.), base types (`FilterChain`, `FirFilter`, `IirFilter`, `StereoFilter`, etc.), Bessel, biquad, Butterworth, Chebyshev, elliptic, FIR design (`Fda`), one-pole, polyphase, comb, DC removal, de-emphasis, Hilbert, median, moving average, pre-emphasis, RASTA, Savitzky–Golay, Thiran, and Wiener filters.",
    "> **0.9 API 说明**：下列各类的 **首选重载均接受 `Signal`**（返回 `Signal` 或写入 `Signal.Samples`）。`Operation.Convolve`、`CrossCorrelate`、`BlockConvolve`、`Resample`、`TimeStretch`、`Envelope`、`FullRectify`、`HalfRectify`、`SpectralSubtract` 等均已提供 `Signal` 版本；`Convolver` / `Modulator` / `SpectralSubtractor` 内部使用 `ReadOnlySpan<float>` 零拷贝路径。文档中仍出现的 `DiscreteSignal` 签名表示旧 API，等价迁移为 `Signal` 即可（采样率改为 `float`）。":
    "> **0.9 API note**: Preferred overloads for the types below **accept `Signal`** (return `Signal` or write to `Signal.Samples`). `Operation.Convolve`, `CrossCorrelate`, `BlockConvolve`, `Resample`, `TimeStretch`, `Envelope`, `FullRectify`, `HalfRectify`, `SpectralSubtract`, and others have `Signal` versions; `Convolver` / `Modulator` / `SpectralSubtractor` use zero-copy `ReadOnlySpan<float>` internally. Remaining `DiscreteSignal` signatures are legacy; migrate to `Signal` (sample rate as `float`).",
    "**0.9 推荐 API**": "**0.9 recommended API**",
    "**兼容 / 过时**": "**Compatibility / obsolete**",
    "- `DiscreteSignal`、`DiscreteSignalExtensions`：仍可用，已 `[Obsolete]`；请改用 `Signal` / `SignalExtensions` 或 `DiscreteSignal.ToSignal()`。": "- `DiscreteSignal`, `DiscreteSignalExtensions`: still available, marked `[Obsolete]`; use `Signal` / `SignalExtensions` or `DiscreteSignal.ToSignal()`.",
    "- 下文手册中保留 `DiscreteSignal` 章节供对照迁移；新代码请以 [Signal 类](#vorcycmathematicssignalprocessingsignalssignal-类) 为准。": "- The `DiscreteSignal` sections below are kept for migration reference; new code should use the [Signal class](#vorcycmathematicssignalprocessingsignalssignal-class).",
    "> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Signals": "> All types below are in namespace: Vorcyc.Mathematics.SignalProcessing.Signals",
    "> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Operations": "> All types below are in namespace: Vorcyc.Mathematics.SignalProcessing.Operations",
    "> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Filters": "> All types below are in namespace: Vorcyc.Mathematics.SignalProcessing.Filters",
    ":ledger:目录  ": ":ledger: Contents  ",
    ":ledger:目录": ":ledger: Contents",
    "（**已过时**）": "(**obsolete**)",
    "无公开属性。": "No public properties.",
    "无": "None",
    "### 构造与工厂": "### Constructors and factories",
    "### 主要属性": "### Main properties",
    "### 切片与拷贝": "### Slicing and copying",
    "### 频域与重采样": "### Frequency domain and resampling",
    "### 参数设置": "### Parameters",
    "### 枚举": "### Enums",
    "### 属性": "### Properties",
    "### 构造器": "### Constructors",
    "### 方法": "### Methods",
    "### 索引器": "### Indexers",
    "### 运算符": "### Operators",
    "### 代码示例": "### Code example",
    "### 重载运算符": "### Operator overloads",
    "### 枚举成员": "### Enum members",
    "| 类型 | 说明 |": "| Type | Description |",
    "| 成员 | 说明 |": "| Member | Description |",
    "| 方法 | 说明 |": "| Method | Description |",
    "| 属性 | 类型 | 说明 |": "| Property | Type | Description |",
    "| 值 | 说明 |": "| Value | Description |",
}

# Chinese phrase -> English (min length 2, longest first)
CN_PHRASES = [
    ("获取或设置", "Gets or sets"),
    ("获取", "Gets"),
    ("返回", "Returns"),
    ("计算", "Computes"),
    ("使用", "Uses"),
    ("判断", "Determines whether"),
    ("初始化", "Initializes"),
    ("执行", "Performs"),
    ("创建", "Creates"),
    ("重置", "Resets"),
    ("设置", "Sets"),
    ("构造", "Constructs"),
    ("生成", "Generates"),
    ("表示", "Represents"),
    ("处理", "Processes"),
    ("在线", "Online"),
    ("迭代", "Iterates"),
    ("从给定的信号中", "From the given signal"),
    ("输入信号", "input signal"),
    ("输出信号", "output signal"),
    ("期望信号", "desired signal"),
    ("载波信号", "carrier signal"),
    ("调制信号", "modulator signal"),
    ("卷积核", "kernel"),
    ("采样率", "sample rate"),
    ("样本数", "sample count"),
    ("样本组", "sample array"),
    ("采样数据", "sample data"),
    ("输入样本", "input sample"),
    ("输出样本", "output sample"),
    ("滤波器", "filter"),
    ("步长因子", "step size"),
    ("泄漏因子", "leakage factor"),
    ("攻击阶段", "attack phase"),
    ("衰减阶段", "decay phase"),
    ("持续阶段", "sustain phase"),
    ("释放阶段", "release phase"),
    ("零拷贝", "zero-copy"),
    ("逐样本", "per-sample"),
    ("逐元素", "element-wise"),
    ("已过时", "obsolete"),
    ("推荐", "recommended"),
    ("默认为", "default"),
    ("默认值为", "default"),
    ("命名空间", "namespace"),
    ("目录", "Contents"),
    ("说明", "description"),
    ("参数", "parameters"),
    ("返回值", "return value"),
    ("实例", "instance"),
    ("枚举成员", "enum members"),
    ("构造函数", "constructor"),
    ("攻击", "attack"),
    ("衰减", "attenuation"),
    ("持续", "sustain"),
    ("释放", "release"),
    ("信号", "signal"),
    ("样本", "sample"),
    ("频率", "frequency"),
    ("幅度", "amplitude"),
    ("相位", "phase"),
    ("功率", "power"),
    ("能量", "energy"),
    ("窗函数", "window"),
    ("卷积", "convolution"),
    ("互相关", "cross-correlation"),
    ("反卷积", "deconvolution"),
    ("重采样", "resample"),
    ("整流", "rectification"),
    ("叠加", "superimpose"),
    ("连接", "concatenate"),
    ("反转", "reverse"),
    ("延迟", "delay"),
    ("调制", "modulation"),
    ("包络", "envelope"),
    ("谐波", "harmonic"),
    ("打击乐", "percussive"),
    ("分离", "separation"),
    ("重建", "reconstruction"),
    ("截止频率", "cutoff frequency"),
    ("中心频率", "center frequency"),
    ("通带", "passband"),
    ("阻带", "stopband"),
    ("增益", "gain"),
    ("阶数", "order"),
    ("带宽", "bandwidth"),
    ("纹波", "ripple"),
    ("原型", "prototype"),
    ("极点", "poles"),
    ("零点", "zeros"),
    ("实部", "real part"),
    ("虚部", "imaginary part"),
    ("复数", "complex"),
    ("离散", "discrete"),
    ("时域", "time-domain"),
    ("频域", "frequency-domain"),
    ("过零率", "zero-crossing rate"),
    ("熵", "entropy"),
    ("帧级", "frame-level"),
    ("特征", "features"),
    ("统计", "statistics"),
    ("区间", "range"),
    ("峰值", "peak"),
    ("常数", "constant"),
    ("单位脉冲", "unit impulse"),
    ("拷贝构造", "copy construction"),
    ("可读写视图", "read/write view"),
    ("只读结构体", "readonly struct"),
    ("继承", "inherits"),
    ("实现", "implements"),
    ("的类", " class"),
]
CN_PHRASES.sort(key=lambda x: len(x[0]), reverse=True)

CLASS_PATTERNS = [
    (r"^(.+?) 是一个用于实现复数值输入/输出的快速卷积（FFT）的类。$", r"\1 performs fast convolution (FFT) with complex-valued input/output."),
    (r"^(.+?) 是一个用于实现快速卷积（FFT）的类。$", r"\1 performs fast convolution (FFT)."),
    (r"^(.+?) 是一个用于实现最小均方四次（Least-Mean-Fourth）自适应滤波算法的类。$", r"\1 implements the least-mean-fourth (LMF) adaptive filtering algorithm."),
    (r"^(.+?) 是一个用于实现(.+?)自适应滤波算法的类。$", r"\1 implements the \2 adaptive filtering algorithm."),
    (r"^(.+?) 是一个用于实现(.+?)的类。$", r"\1 is a class that implements \2."),
    (r"^(.+?) 是一个用于构建 (.+?) 包络的类，继承了 SignalBuilder 类。$", r"\1 builds \2 envelopes; inherits `SignalBuilder`."),
    (r"^(.+?) 是一个用于生成(.+?)的类，继承了 SignalBuilder 类。使用 (.+?) 生成(.+?)。$", r"\1 generates \2; inherits `SignalBuilder`. Uses \3 to generate \4."),
    (r"^(.+?) 是一个用于生成(.+?)的类，继承了 SignalBuilder 类。$", r"\1 generates \2; inherits `SignalBuilder`."),
    (r"^(.+?) 是一个用于对信号生成器进行淡入淡出效果装饰的类，继承了 SignalBuilder 类。$", r"\1 decorates a signal generator with fade-in/fade-out; inherits `SignalBuilder`."),
    (r"^(.+?) 是一个使用 (.+?) 算法生成信号的类，继承了 (.+?) 类。$", r"\1 generates signals with the \2 algorithm; inherits \3."),
    (r"^(.+?) 是一个使用 (.+?) 算法的 \"Drum\" 变体生成信号的类，继承了 (.+?) 类。$", r"\1 generates signals using the \2 \"Drum\" variant; inherits \3."),
    (r"^(.+?) 是一个用于表示有限复数值离散时间信号的类。信号以一定的采样率存储为两个数据数组（实部和虚部）。$", r"\1 represents a finite complex discrete-time signal stored as real and imaginary arrays at a given sample rate."),
    (r"^(.+?) 是一个用于表示有限实数值离散时间信号的类。信号以一定的采样率存储为一个数据数组。$", r"\1 represents a finite real discrete-time signal stored as a single array at a given sample rate."),
    (r"^(.+?) 是一个用于实现基于重叠-相加算法的块卷积的信号处理器类。$", r"\1 is a block convolver using the overlap-add algorithm."),
    (r"^(.+?) 是一个用于实现基于重叠-保存算法的块卷积的信号处理器类。$", r"\1 is a block convolver using the overlap-save algorithm."),
    (r"^(.+?) 是一个用于实现动态处理（如限制器、压缩器、扩展器或噪声门）的类。$", r"\1 implements dynamics processing (limiter, compressor, expander, or noise gate)."),
    (r"^(.+?) 是一个用于实现包络跟随（包络检测）的类。$", r"\1 implements envelope following (envelope detection)."),
    (r"^(.+?) 是一个用于从功率（或幅度）谱图中使用 Griffin-Lim 迭代算法重建信号的类。$", r"\1 reconstructs signals from power (or magnitude) spectrograms using the Griffin–Lim algorithm."),
    (r"^(.+?) 是一个基于中值滤波的谐波/打击乐分离器类。$", r"\1 is a median-filtering-based harmonic/percussive separator."),
    (r"^(.+?) 是一个提供滤波器设计和分析方法的静态类。$", r"\1 is a static class for filter design and analysis."),
    (r"^(.+?) 是一个提供滤波器组设计方法的静态类。$", r"\1 is a static class for filter-bank design."),
    (r"^(.+?) 枚举定义了(.+?)。$", r"\1 enum defines \2."),
    (r"^(.+?) 提供了用于处理复数离散信号的扩展方法。$", r"\1 provides extension methods for complex discrete signals."),
    (r"^(.+?) 提供了多种调制方法：$", r"\1 provides several modulation methods:"),
    (r"^(.+?) 是一个用于实现双二阶(.+?)滤波器的类。$", r"\1 implements a biquad \2 filter."),
    (r"^(.+?) 是一个用于实现(.+?)滤波器的类。$", r"\1 implements a \2 filter."),
    (r"^(.+?) 是一个用于生成(.+?)滤波器原型的静态类。$", r"\1 is a static class that generates \2 filter prototypes."),
    (r"^(.+?) 是一个用于(.+?)的类，继承了 (.+?) 类。$", r"\1 is used for \2; inherits \3."),
    (r"^(.+?) 是一个用于(.+?)的类。$", r"\1 is used for \2."),
    (r"^(.+?) 是一个(.+?)类。$", r"\1 is a \2 class."),
]

COMMENT_RE = [
    (r"^(\s*// )创建 (.+?) 实例$", r"\1Create \2 instance"),
    (r"^(\s*// )定义(.+)$", r"\1Define \2"),
    (r"^(\s*// )执行(.+)$", r"\1Run \2"),
    (r"^(\s*// )使用(.+)$", r"\1Use \2"),
    (r"^(\s*// )设置(.+)$", r"\1Set \2"),
    (r"^(\s*// )生成(.+)$", r"\1Generate \2"),
    (r"^(\s*// )获取(.+)$", r"\1Get \2"),
    (r"^(\s*// )输出(.+)$", r"\1Print \2"),
    (r"^(\s*// )重置(.+)$", r"\1Reset \2"),
    (r"^(\s*// )处理(.+)$", r"\1Process \2"),
    (r"^(\s*// )评估(.+)$", r"\1Evaluate \2"),
    (r"^(\s*// )提取(.+)$", r"\1Extract \2"),
    (r"^(\s*// )连接(.+)$", r"\1Concatenate \2"),
    (r"^(\s*// )放大(.+)$", r"\1Amplify \2"),
    (r"^(\s*// )衰减(.+)$", r"\1Attenuate \2"),
    (r"^(\s*// )转换为(.+)$", r"\1Convert to \2"),
    (r"^(\s*// )展开(.+)$", r"\1Unwrap \2"),
    (r"^(\s*// )设计(.+)$", r"\1Design \2"),
    (r"^(\s*// )计算(.+)$", r"\1Compute \2"),
    (r"^(\s*// )更改(.+)$", r"\1Change \2"),
    (r"^(\s*// )在线(.+)$", r"\1Online \2"),
]

EXAMPLE_INTRO_RE = re.compile(
    r"^以下是一个使用 (.+?) 类中(多个方法的示例|Poles 方法的示例|Poles 和 Zeros 方法的示例|FirWinFdLp 方法的示例)，并在示例中加入了注释：$"
)


def fix_links(line: str) -> str:
    for a, b in LINKS:
        line = line.replace(a, b)
    return line


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
    return line


def cn_to_en(text: str) -> str:
    for zh, en in CN_PHRASES:
        text = text.replace(zh, en)
    text = text.replace("，", ", ")
    text = text.replace("。", ".")
    text = text.replace("：", ": ")
    text = text.replace("（", " (")
    text = text.replace("）", ")")
    text = text.replace("、", ", ")
    text = re.sub(r"\s+\.", ".", text)
    return text


def translate_cn_segment(seg: str) -> str:
    return cn_to_en(seg)


def translate_line(line: str) -> str:
    line = fix_links(line)
    if line in EXACT:
        return fix_anchors(EXACT[line])
    out = line
    for pat, repl in CLASS_PATTERNS:
        out2 = re.sub(pat, repl, out)
        if out2 != out:
            out = out2
            break
    for pat, repl in COMMENT_RE:
        out2 = re.sub(pat, repl, out)
        if out2 != out:
            out = out2
            break
    m = EXAMPLE_INTRO_RE.match(out)
    if m:
        cls, kind = m.group(1), m.group(2)
        if "Poles 和 Zeros" in kind:
            out = f"The following example uses `{cls}.Poles` and `Zeros` with comments:"
        elif "Poles" in kind:
            out = f"The following example uses `{cls}.Poles` with comments:"
        elif "FirWinFdLp" in kind:
            out = f"The following example uses `{cls}.FirWinFdLp` with comments:"
        else:
            out = f"The following example uses `{cls}` with comments:"
    if re.search(r"[\u4e00-\u9fff]", out):
        if out.startswith("  - 参数:"):
            out = "  - Parameters:"
        elif out.startswith("  - 返回值:"):
            rest = out.split(":", 1)[1].strip()
            out = f"  - Returns: {translate_cn_segment(rest)}"
        elif out.startswith("  - 异常:"):
            out = "  - Exceptions:"
        elif out.startswith("  - 枚举成员:"):
            out = "  - Enum members:"
        elif out.startswith("|") and "|" in out[1:]:
            parts = out.split("|")
            if len(parts) >= 3:
                parts[-2] = " " + translate_cn_segment(parts[-2].strip()) + " "
                out = "|".join(parts)
        elif out.startswith("  - "):
            body = out[4:]
            out = "  - " + translate_cn_segment(body)
        elif out.startswith("    - "):
            body = out[6:]
            if ": " in body:
                k, v = body.split(": ", 1)
                out = f"    - {k}: {translate_cn_segment(v)}"
            else:
                out = "    - " + translate_cn_segment(body)
        elif out.startswith("> 以下类型均位于 "):
            out = re.sub(r"> 以下类型均位于 (.+?) 命名空间。", r"> All types below are in namespace: \1", out)
        elif out.startswith("- :bookmark:"):
            out = fix_anchors(out)
        else:
            out = translate_cn_segment(out)
    out = fix_anchors(out)
    return out


def translate_line_cn(line: str) -> str:
    """Fallback high-quality translator."""
    try:
        import importlib.util
        spec = importlib.util.spec_from_file_location(
            "cn_to_en_sp", ROOT / "tools" / "cn_to_en_sp.py"
        )
        cn = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(cn)
        return cn.translate_line(line)
    except Exception:
        return translate_line(line)


def main():
    # Merge full-line phrases from translate_wiki_signal_processing
    twp_path = ROOT / "tools" / "translate_wiki_signal_processing.py"
    if twp_path.exists():
        import importlib.util
        spec = importlib.util.spec_from_file_location("twp", twp_path)
        twp = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(twp)
        for zh, en in twp.PHRASES:
            if re.search(r"[\u4e00-\u9fff]", zh) and not re.search(r"[\u4e00-\u9fff]", en):
                EXACT[zh] = en
    if OVERRIDES_JSON.exists():
        EXACT.update(json.loads(OVERRIDES_JSON.read_text(encoding="utf-8")))
    data = json.loads(UNIQUE_JSON.read_text(encoding="utf-8"))
    all_lines: list[str] = []
    for v in data.values():
        all_lines.extend(v)
    mapping = {}
    for line in all_lines:
        primary = translate_line(line)
        alt = translate_line_cn(line)
        p_n = len(re.findall(r"[\u4e00-\u9fff]", primary))
        a_n = len(re.findall(r"[\u4e00-\u9fff]", alt))
        mapping[line] = alt if a_n < p_n else primary
    OUT_JSON.write_text(json.dumps(mapping, ensure_ascii=False, indent=2), encoding="utf-8")
    remaining = [k for k, v in mapping.items() if re.search(r"[\u4e00-\u9fff]", v)]
    print(f"Generated {len(mapping)} translations, {len(remaining)} still have Chinese")
    if remaining:
        Path(ROOT / "tools" / "sp_still_chinese.json").write_text(
            json.dumps(remaining[:80], ensure_ascii=False, indent=2), encoding="utf-8"
        )


if __name__ == "__main__":
    main()
