#!/usr/bin/env python3
"""Generate high-quality exact translations using sentence templates."""
from __future__ import annotations

import importlib.util
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
UNIQUE_JSON = ROOT / "tools" / "sp_unique_cn_lines.json"
OUT_JSON = ROOT / "tools" / "sp_exact_translations.json"
GEN_PATH = ROOT / "tools" / "generate_sp_exact.py"
TWP_PATH = ROOT / "tools" / "translate_wiki_signal_processing.py"

LINKS = [
    ("HOME_zh.md", "HOME.md"),
    ("Module_SignalProcessing_zh.md", "Module_SignalProcessing.md"),
    ("Module_SignalProcessing_Signals_zh.md", "Module_SignalProcessing_Signals.md"),
    ("Module_SignalProcessing_Operations_zh.md", "Module_SignalProcessing_Operations.md"),
    ("Module_SignalProcessing_Filters_zh.md", "Module_SignalProcessing_Filters.md"),
    ("Module_ComputingContext_zh.md", "Module_ComputingContext.md"),
    ("Module_GPU_Policy_zh.md", "Module_ComputingContext.md"),
    ("Module_GPU_Policy_zh.md", "Module_GPU_Policy.md"),
    ("Module_Extensions_FFTW_zh.md", "Module_Extensions_FFTW.md"),
]

_spec = importlib.util.spec_from_file_location("generate_sp_exact", GEN_PATH)
_gen = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_gen)

EXACT: dict[str, str] = dict(_gen.EXACT)

_twp_spec = importlib.util.spec_from_file_location("twp", TWP_PATH)
_twp = importlib.util.module_from_spec(_twp_spec)
_twp_spec.loader.exec_module(_twp)
for zh, en in _twp.PHRASES:
    if re.search(r"[\u4e00-\u9fff]", zh) and not re.search(r"[\u4e00-\u9fff]", en) and len(zh) >= 10:
        EXACT[zh] = en

# Safe multi-char phrases only (no single-char particles)
SAFE_PHRASES: list[tuple[str, str]] = [
    ("当前位置 : [首页](HOME_zh.md)/[信号处理模块](Module_SignalProcessing_zh.md)/[信号定义和相关操作](Module_SignalProcessing_Signals_zh.md)",
     "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Signals](Module_SignalProcessing_Signals.md)"),
    ("当前位置 : [首页](HOME_zh.md)/[信号处理模块](Module_SignalProcessing_zh.md)/[常用操作](Module_SignalProcessing_Operations_zh.md)",
     "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Operations](Module_SignalProcessing_Operations.md)"),
    ("当前位置 : [首页](HOME_zh.md)/[信号处理模块](Module_SignalProcessing_zh.md)/[一维滤波器](Module_SignalProcessing_Filters_zh.md)",
     "Location: [Home](HOME.md) / [Signal processing](Module_SignalProcessing.md) / [Filters](Module_SignalProcessing_Filters.md)"),
    ("# 信号处理模块 - Signal Processing Module", "# Signal Processing Module"),
    ("## 信号定义和相关操作 - Signals", "## Signals"),
    ("## 常用操作 - Operations", "## Operations"),
    ("## 一维滤波器 - Filters", "## Filters"),
    ("`Vorcyc.Mathematics.SignalProcessing.Signals` 命名空间包含时域/频域信号定义与编辑工具。",
     "The `Vorcyc.Mathematics.SignalProcessing.Signals` namespace provides time-domain and frequency-domain signal definitions and editing utilities."),
    ("**0.9 推荐 API**", "**0.9 recommended API**"),
    ("**兼容 / 过时**", "**Compatibility / obsolete**"),
    ("| 类型 | 说明 |", "| Type | Description |"),
    ("| 成员 | 说明 |", "| Member | Description |"),
    ("| 方法 | 说明 |", "| Method | Description |"),
    ("| 属性 | 类型 | 说明 |", "| Property | Type | Description |"),
    ("| 值 | 说明 |", "| Value | Description |"),
    ("| `Signal` | 单线程时域实信号主类型：`float` 采样率、固定针脚缓冲、`Samples` 为 `Span<float>`，实现 `ISingleThreadTimeDomainSignal` |",
     "| `Signal` | Primary single-threaded real time-domain type: `float` sample rate, pinned buffer, `Samples` as `Span<float>`, implements `ISingleThreadTimeDomainSignal` |"),
    ("| `SignalSegment` | 对父 `Signal` 的零拷贝片段 `signal[start, length]`，用于帧级 SIMD 统计与特征 |",
     "| `SignalSegment` | Zero-copy slice `signal[start, length]` of parent `Signal`; frame-level SIMD statistics and features |"),
    ("| `SignalExtensions` | 信号编辑与帧统计扩展（替代已过时的 `DiscreteSignalExtensions`） |",
     "| `SignalExtensions` | Signal editing and frame statistics extensions (replaces obsolete `DiscreteSignalExtensions`) |"),
    ("| `ComplexDiscreteSignal` | 复数离散信号（卷积、频域运算等） |",
     "| `ComplexDiscreteSignal` | Complex discrete signal (convolution, frequency-domain ops, etc.) |"),
    ("- `DiscreteSignal`、`DiscreteSignalExtensions`：仍可用，已 `[Obsolete]`；请改用 `Signal` / `SignalExtensions` 或 `DiscreteSignal.ToSignal()`。",
     "- `DiscreteSignal`, `DiscreteSignalExtensions`: still available, marked `[Obsolete]`; use `Signal` / `SignalExtensions` or `DiscreteSignal.ToSignal()`."),
    ("- 下文手册中保留 `DiscreteSignal` 章节供对照迁移；新代码请以 [Signal 类](#vorcycmathematicssignalprocessingsignalssignal-类) 为准。",
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
    ("### 构造与工厂", "### Constructors and factories"),
    ("### 主要属性", "### Main properties"),
    ("### 切片与拷贝", "### Slicing and copying"),
    ("### 频域与重采样", "### Frequency domain and resampling"),
    ("### 参数设置", "### Parameters"),
    ("### 枚举", "### Enums"),
    ("### 属性", "### Properties"),
    ("### 构造器", "### Constructors"),
    ("### 方法", "### Methods"),
    ("### 索引器", "### Indexers"),
    ("### 运算符", "### Operators"),
    ("### 代码示例", "### Code example"),
    ("### 重载运算符", "### Operator overloads"),
    ("### 枚举成员", "### Enum members"),
    ("可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：",
     "Set the following via `SignalBuilder.SetParameter(string, double)`:"),
    ("  - 参数:", "  - Parameters:"),
    ("  - 返回值:", "  - Returns:"),
    ("  - 异常:", "  - Exceptions:"),
    ("  - 枚举成员:", "  - Enum members:"),
    ("以下是一个使用", "The following example uses "),
    (" 类中多个方法的示例，并在示例中加入了注释：", " with comments:"),
    (" 类中 Transform 方法的示例，并在示例中加入了注释：", " `Transform` with comments:"),
    (" 类中 Poles 和 Zeros 方法的示例，并在示例中加入了注释：", " `Poles` and `Zeros` with comments:"),
    (" 类中 Poles 方法的示例，并在示例中加入了注释：", " `Poles` with comments:"),
    (" 类中 FirWinFdLp 方法的示例，并在示例中加入了注释：", " `FirWinFdLp` with comments:"),
    ("构造函数", "constructor"),
    ("每秒的样本数", "samples per second"),
    ("信号的长度", "signal length"),
    ("复数值样本的功率（幅度的平方）", "power of complex sample (amplitude squared)"),
    ("复数值样本的展开相位", "unwrapped phase of complex sample"),
    ("如果应为数据分配新内存，则设置为 true，默认为 false", "if new memory should be allocated for data, set to true; default false"),
    ("具有指定长度并填充指定值的信号", "signal with specified length filled with specified value"),
    ("每个样本的实部值，默认为 0.0", "real part value per sample; default 0.0"),
    ("每个样本的虚部值，默认为 0.0", "imaginary part value per sample; default 0.0"),
    ("使用整数样本集合构造信号，并在给定采样率下进行归一化", "constructs signal from integer sample set and normalizes at given sample rate"),
    ("整数样本集合", "integer sample set"),
    ("每个样本将除以此值，默认为 1.0", "each sample is divided by this value; default 1.0"),
    ("创建信号的深拷贝", "creates a deep copy of the signal"),
    ("信号的深拷贝", "deep copy of the signal"),
    ("样本索引器。仅适用于样本实部数组。谨慎使用", "sample indexer; applies to real-part array only; use with care"),
    ("创建信号的切片", "creates a slice of the signal"),
    ("第一个样本的索引（包含）", "index of first sample (inclusive)"),
    ("最后一个样本的索引（不包含）", "index of last sample (exclusive)"),
    ("信号的切片", "slice of the signal"),
    ("通过叠加信号 s1 和 s2 创建新信号。如果大小不同，则较小的信号将广播以适应较大的信号大小",
     "creates new signal by superimposing s1 and s2; if sizes differ, the smaller signal is broadcast to match the larger"),
    ("第一个信号", "first signal"),
    ("第二个信号", "second signal"),
    ("叠加后的新信号", "new superimposed signal"),
    ("通过将常数添加到信号 s 创建新信号", "creates new signal by adding constant to signal s"),
    ("要添加到每个样本的常数", "constant to add to each sample"),
    ("添加常数后的新信号", "new signal after adding constant"),
    ("通过从信号 s 中减去常数创建新信号", "creates new signal by subtracting constant from signal s"),
    ("要从每个样本中减去的常数", "constant to subtract from each sample"),
    ("减去常数后的新信号", "new signal after subtracting constant"),
    ("通过将信号 s 乘以系数（放大/衰减）创建新信号", "creates new signal by multiplying signal s by coefficient (amplify/attenuate)"),
    ("放大/衰减系数", "amplify/attenuate coefficient"),
    ("放大/衰减", "amplify/attenuate"),
    ("攻击阶段", "attack phase"),
    ("衰减阶段", "decay phase"),
    ("持续阶段", "sustain phase"),
    ("释放阶段", "release phase"),
    ("粉红噪声", "pink noise"),
    ("红噪声（布朗噪声）", "red noise (Brownian noise)"),
    ("周期性脉冲波", "periodic pulse waves"),
    ("直线信号", "ramp (linear) signals"),
    ("反馈系数", "feedback coefficient"),
    ("谐波的振幅", "harmonic amplitudes"),
    ("振幅数组", "amplitude array"),
    ("带宽比例", "bandwidth scale"),
    ("缩放比例", "scale factor"),
    ("脉冲持续时间", "pulse duration"),
    ("脉冲波周期", "pulse wave period"),
    ("加性白高斯噪声（AWGN）", "additive white Gaussian noise (AWGN)"),
    ("啁啾信号", "chirp signals"),
    ("余弦信号", "cosine signals"),
    ("正弦波信号", "sine wave signals"),
    ("方波信号", "square wave signals"),
    ("锯齿波信号", "sawtooth wave signals"),
    ("三角波信号", "triangle wave signals"),
    ("白噪声信号", "white noise signals"),
    ("基于波表的信号", "wavetable-based signals"),
    ("单线程时域实信号主类型", "primary single-threaded real time-domain type"),
    ("固定针脚缓冲", "pinned buffer"),
    ("零拷贝片段", "zero-copy slice"),
    ("时域特征（带 SIMD 缓存）", "time-domain features (SIMD-cached)"),
    ("按样本数与采样率分配缓冲", "allocate buffer by sample count and rate"),
    ("按时长与采样率分配缓冲", "allocate buffer by duration and rate"),
    ("单位脉冲（首样本为 1）", "unit impulse (first sample is 1)"),
    ("从 span 拷贝构造", "construct from span copy"),
    ("从数组拷贝构造", "construct from array copy"),
    ("可读写视图", "read/write view"),
    ("只读结构体", "readonly struct"),
    ("标量与同形状 `Signal` 逐样本运算，返回新 `Signal`", "scalar and same-shape `Signal` element-wise ops; returns new `Signal`"),
    ("取前/后 n 个样本（新 `Signal`）", "first/last n samples (new `Signal`)"),
    ("原地增益/反转", "in-place gain / reverse"),
    ("按峰值归一化到 [-1, 1]", "peak-normalize to [-1, 1]"),
    ("延迟副本（正 delay 延长，负 delay 截断左移）", "delayed copy (positive delay pads; negative delay truncates/shifts left)"),
    ("连接 / 重复", "concatenate / repeat"),
    ("叠加与相减（支持不等长广播）", "superimpose and subtract (unequal-length broadcast)"),
    ("原地整流", "in-place rectification"),
    ("线性淡入淡出（`double` 秒或 `TimeSpan`）", "linear fade in/out (`double` seconds or `TimeSpan`)"),
    ("两信号交叉淡化", "crossfade two signals"),
    ("转为 `ComplexDiscreteSignal`（虚部为零）", "convert to `ComplexDiscreteSignal` (zero imaginary part)"),
    ("区间统计（`Signal` 或 `ReadOnlySpan<float>`）", "range statistics (`Signal` or `ReadOnlySpan<float>`)"),
    ("自 `DiscreteSignalExtensions` 迁移的静态扩展方法（**新代码请用本类**）",
     "static extensions migrated from `DiscreteSignalExtensions` (**use this class in new code**)"),
    ("FIR 滤波器核和 FFT 大小", "FIR filter kernel and FFT size"),
    ("卷积核和 FFT 大小", "kernel and FFT size"),
    ("处理整个信号并返回新的滤波信号", "processes entire signal and returns new filtered signal"),
    ("动态处理器的类型（模式）", "dynamics processor modes"),
    ("动态处理器模式、采样率、阈值、比率、补偿增益、攻击时间、释放时间和最小振幅阈值",
     "dynamics mode, sample rate, threshold, ratio, makeup gain, attack time, release time, and minimum amplitude threshold"),
    ("FFT 大小、跳跃长度、谐波中值滤波器大小、打击乐中值滤波器大小和掩蔽模式",
     "FFT size, hop length, harmonic median filter size, percussive median filter size, and masking mode"),
    ("沿时间轴的中值滤波器大小", "median filter size along time axis"),
    ("沿频率轴的中值滤波器大小", "median filter size along frequency axis"),
    ("对 `signal` 进行插值并进行低通滤波", "interpolates and low-pass filters `signal`"),
    ("低通抗混叠滤波器", "low-pass anti-aliasing filter"),
    ("块卷积方法（OverlapAdd / OverlapSave）", "block convolution method (OverlapAdd / OverlapSave)"),
    ("默认值为", "default"),
    ("默认为", "default"),
    ("采样率", "sample rate"),
    ("样本数", "sample count"),
    ("时域", "time-domain"),
    ("频域", "frequency-domain"),
    ("零拷贝", "zero-copy"),
    ("逐样本", "per-sample"),
    ("已过时", "obsolete"),
    ("命名空间", "namespace"),
    ("说明", "description"),
    ("返回值", "return value"),
    ("枚举成员", "enum members"),
    ("单位脉冲", "unit impulse"),
    ("过零率", "zero-crossing rate"),
    ("帧级", "frame-level"),
    ("卷积核", "kernel"),
    ("滤波器", "filter"),
    ("步长因子", "step size"),
    ("泄漏因子", "leakage factor"),
    ("输入信号", "input signal"),
    ("输出信号", "output signal"),
    ("期望信号", "desired signal"),
    ("载波信号", "carrier signal"),
    ("调制信号", "modulator signal"),
    ("截止频率", "cutoff frequency"),
    ("中心频率", "center frequency"),
    ("通带", "passband"),
    ("阻带", "stopband"),
    ("双二阶", "biquad"),
    ("巴特沃斯", "Butterworth"),
    ("切比雪夫", "Chebyshev"),
    ("椭圆", "elliptic"),
    ("贝塞尔", "Bessel"),
    ("最小均方", "least mean square"),
    ("最小均方四次", "least-mean-fourth"),
    ("归一化最小均方", "normalized least mean square"),
    ("递归最小二乘", "recursive least squares"),
    ("重叠相加", "overlap-add"),
    ("重叠保留", "overlap-save"),
    ("动态处理", "dynamics processing"),
    ("波形整形", "waveshaping"),
    ("谱减法", "spectral subtraction"),
    ("时间拉伸", "time stretch"),
    ("音高变换", "pitch shift"),
    ("实部", "real part"),
    ("虚部", "imaginary part"),
    ("复数", "complex"),
    ("离散信号", "discrete signal"),
    ("正弦波", "sine wave"),
    ("方波", "square wave"),
    ("锯齿波", "sawtooth wave"),
    ("三角波", "triangle wave"),
    ("白噪声", "white noise"),
    ("加性白高斯噪声", "additive white Gaussian noise"),
    ("Karplus-Strong", "Karplus–Strong"),
    ("Griffin-Lim", "Griffin–Lim"),
    ("窗函数", "window"),
    ("互相关", "cross-correlation"),
    ("反卷积", "deconvolution"),
    ("重采样", "resample"),
    ("淡入淡出", "fade in/out"),
    ("交叉淡化", "crossfade"),
    ("维纳滤波", "Wiener filtering"),
    ("移动平均", "moving average"),
    ("中值滤波", "median filtering"),
    ("梳状滤波", "comb filtering"),
    ("预加重", "pre-emphasis"),
    ("去加重", "de-emphasis"),
    ("直流去除", "DC removal"),
    ("传递函数", "transfer function"),
    ("群延迟", "group delay"),
    ("品质因数", "Q factor"),
    ("阻带衰减", "stopband attenuation"),
    ("通带增益", "passband gain"),
    ("概率", "probability"),
    ("振幅", "amplitude"),
    ("斜率", "slope"),
    ("截距", "intercept"),
    ("系数", "coefficient"),
    ("阈值", "threshold"),
    ("比率", "ratio"),
    ("补偿增益", "makeup gain"),
    ("攻击时间", "attack time"),
    ("释放时间", "release time"),
    ("最小振幅阈值", "minimum amplitude threshold"),
    ("噪声门", "noise gate"),
    ("限制器", "limiter"),
    ("压缩器", "compressor"),
    ("扩展器", "expander"),
    ("跳跃长度", "hop length"),
    ("跳跃大小", "hop size"),
    ("掩蔽模式", "masking mode"),
    ("滤波方法", "filtering method"),
    ("深拷贝", "deep copy"),
    ("整数", "integer"),
    ("切片", "slice"),
    ("展开", "unwrapped"),
    ("平方", "squared"),
    ("长度", "length"),
    ("填充", "filled"),
    ("广播", "broadcast"),
    ("适应", "match"),
    ("减去", "subtract"),
    ("乘以", "multiply"),
    ("放大", "amplify"),
    ("衰减", "attenuation"),
    ("谨慎", "care"),
    ("索引", "index"),
    ("包含", "inclusive"),
    ("不包含", "exclusive"),
    ("叠加", "superimpose"),
    ("常数", "constant"),
    ("新信号", "new signal"),
    ("新内存", "new memory"),
    ("内存", "memory"),
    ("数据", "data"),
    ("分配", "allocate"),
    ("设置为", "set to"),
    ("设置为 true", "set to true"),
    ("设置为 false", "set to false"),
    ("阶段", "phase"),
    ("因子", "factor"),
    ("大小", "size"),
    ("时间", "time"),
    ("数组", "array"),
    ("集合", "set"),
    ("权重", "weight"),
    ("响应", "response"),
    ("模式", "mode"),
    ("类型", "type"),
    ("形式", "form"),
    ("结果", "result"),
    ("方法", "method"),
    ("属性", "property"),
    ("参数", "parameter"),
    ("实例", "instance"),
    ("信号", "signal"),
    ("样本", "sample"),
    ("频率", "frequency"),
    ("幅度", "amplitude"),
    ("相位", "phase"),
    ("功率", "power"),
    ("能量", "energy"),
    ("卷积", "convolution"),
    ("调制", "modulation"),
    ("包络", "envelope"),
    ("谐波", "harmonic"),
    ("打击乐", "percussive"),
    ("分离", "separation"),
    ("重建", "reconstruction"),
    ("自适应", "adaptive"),
    ("算法", "algorithm"),
    ("增益", "gain"),
    ("阶数", "order"),
    ("带宽", "bandwidth"),
    ("纹波", "ripple"),
    ("极点", "poles"),
    ("零点", "zeros"),
    ("原型", "prototype"),
    ("峰值", "peak"),
    ("区间", "range"),
    ("特征", "features"),
    ("统计", "statistics"),
    ("熵", "entropy"),
    ("整流", "rectification"),
    ("连接", "concatenate"),
    ("反转", "reverse"),
    ("延迟", "delay"),
    ("噪声", "noise"),
    ("直流", "DC"),
    ("低通", "low-pass"),
    ("高通", "high-pass"),
    ("带通", "band-pass"),
    ("带阻", "band-stop"),
    ("插值", "interpolation"),
    ("归一化", "normalize"),
    ("归一化", "normalized"),
    ("推荐", "recommended"),
    ("实现", "implements"),
    ("继承", "inherits"),
    ("表示", "represents"),
    ("生成", "generates"),
    ("获取", "gets"),
    ("返回", "returns"),
    ("计算", "computes"),
    ("执行", "performs"),
    ("初始化", "initializes"),
    ("构造", "constructs"),
    ("创建", "creates"),
    ("设置", "sets"),
    ("重置", "resets"),
    ("使用", "uses"),
    ("判断", "determines whether"),
    ("处理", "processes"),
    ("应用", "applies"),
    ("设计", "designs"),
    ("训练", "trains"),
    ("预测", "predicts"),
    ("估计", "estimates"),
    ("更新", "updates"),
    ("添加", "adds"),
    ("保存", "saves"),
    ("加载", "loads"),
    ("滤波", "filters"),
    ("在线", "online"),
    ("离线", "offline"),
    ("静态", "static"),
    ("抽象", "abstract"),
    ("泛型", "generic"),
    ("只读", "readonly"),
    ("公开", "public"),
    ("可选", "optional"),
    ("当前", "current"),
    ("底层", "underlying"),
    ("输入", "input"),
    ("输出", "output"),
    ("期望", "desired"),
    ("载波", "carrier"),
    ("步长", "step"),
    ("泄漏", "leakage"),
    ("占空比", "duty cycle"),
    ("均值", "mean"),
    ("标准差", "standard deviation"),
    ("结构", "struct"),
    ("枚举", "enum"),
    ("接口", "interface"),
    ("类", "class"),
]
SAFE_PHRASES.sort(key=lambda x: len(x[0]), reverse=True)
EXACT.update(dict(SAFE_PHRASES))

CLASS_PATTERNS = list(_gen.CLASS_PATTERNS) + [
    (r"^(.+?) 是一个用于生成基于波表的信号的类，继承了 SignalBuilder 类。$", r"\1 generates wavetable-based signals; inherits `SignalBuilder`."),
    (r"^(.+?) 是一个用于生成(.+?)信号的类，继承了 SignalBuilder 类。$", r"\1 generates \2 signals; inherits `SignalBuilder`."),
    (r"^(.+?) 是一个用于生成(.+?)的类，继承了 SignalBuilder 类。使用 (.+?) 生成(.+?)。$", r"\1 generates \2; inherits `SignalBuilder`. Uses \3 to generate \4."),
    (r"^(.+?) 是一个用于生成(.+?)的类，继承了 SignalBuilder 类。$", r"\1 generates \2; inherits `SignalBuilder`."),
    (r"^(.+?) 是一个用于构建 (.+?) 包络的类，继承了 SignalBuilder 类。$", r"\1 builds \2 envelopes; inherits `SignalBuilder`."),
    (r"^(.+?) 是一个用于对信号生成器进行淡入淡出效果装饰的类，继承了 SignalBuilder 类。$", r"\1 decorates a signal generator with fade-in/fade-out; inherits `SignalBuilder`."),
    (r"^(.+?) 是一个使用 (.+?) 算法生成信号的类，继承了 (.+?) 类。$", r"\1 generates signals with the \2 algorithm; inherits \3."),
    (r"^(.+?) 是一个使用 (.+?) 算法的 \"Drum\" 变体生成信号的类，继承了 (.+?) 类。$", r"\1 generates signals using the \2 \"Drum\" variant; inherits \3."),
    (r"^(.+?) 是一个用于表示有限复数值离散时间信号的类。信号以一定的采样率存储为两个数据数组（实部和虚部）。$",
     r"\1 represents a finite complex discrete-time signal stored as real and imaginary arrays at a given sample rate."),
    (r"^(.+?) 是一个用于表示有限实数值离散时间信号的类。信号以一定的采样率存储为一个数据数组。$",
     r"\1 represents a finite real discrete-time signal stored as a single array at a given sample rate."),
    (r"^(.+?) 枚举定义了(.+?)。$", r"\1 enum defines \2."),
    (r"^(.+?) 提供了用于处理复数离散信号的扩展方法。$", r"\1 provides extension methods for complex discrete signals."),
    (r"^(.+?) 是一个提供滤波器设计和分析方法的静态类。$", r"\1 is a static class for filter design and analysis."),
    (r"^(.+?) 是一个提供滤波器组设计方法的静态类。$", r"\1 is a static class for filter-bank design."),
    (r"^(.+?) 是一个用于生成直线信号的类，继承了 SignalBuilder 类。信号的形式为 y\[n\] = slope \* n \+ intercept。$",
     r"\1 generates ramp signals; inherits `SignalBuilder`. Form: y[n] = slope * n + intercept."),
    (r"^(.+?) 是一个用于生成直线信号的类，继承了 SignalBuilder 类。信号的形式为 y\[n\] = slope \*",
     r"\1 generates ramp signals; inherits `SignalBuilder`. Form: y[n] = slope *"),
]

VERB_PREFIXES = [
    ("获取或设置", "Gets or sets"),
    ("获取当前的", "Gets the current"),
    ("获取采样率（每秒的样本数）", "Gets sample rate (samples per second)"),
    ("获取信号的长度", "Gets signal length"),
    ("获取复数值样本的功率（幅度的平方）", "Gets power of complex sample (amplitude squared)"),
    ("获取复数值样本的展开相位", "Gets unwrapped phase of complex sample"),
    ("获取", "Gets"),
    ("返回", "Returns"),
    ("计算", "Computes"),
    ("使用整数样本集合构造信号，并在给定采样率下进行归一化", "Constructs signal from integer sample set and normalizes at given sample rate"),
    ("使用 FIR 滤波器核和 FFT 大小构造", "Constructs using FIR filter kernel and FFT size"),
    ("使用", "Uses"),
    ("判断", "Determines whether"),
    ("初始化", "Initializes"),
    ("执行", "Performs"),
    ("训练", "Trains"),
    ("预测", "Predicts"),
    ("创建信号的深拷贝", "Creates a deep copy of the signal"),
    ("创建信号的切片", "Creates a slice of the signal"),
    ("创建", "Creates"),
    ("扩展", "Expands"),
    ("更新", "Updates"),
    ("估计", "Estimates"),
    ("添加", "Adds"),
    ("保存", "Saves"),
    ("加载", "Loads"),
    ("重置", "Resets"),
    ("设置谐波的振幅", "Sets harmonic amplitudes"),
    ("设置", "Sets"),
    ("构造具有指定长度并填充指定值的信号", "Constructs signal with specified length filled with specified value"),
    ("构造 OlaBlockConvolver 实例，指定卷积核和 FFT 大小", "Constructs `OlaBlockConvolver` specifying kernel and FFT size"),
    ("构造 OlsBlockConvolver 实例，指定卷积核和 FFT 大小", "Constructs `OlsBlockConvolver` specifying kernel and FFT size"),
    ("构造 HarmonicPercussiveSeparator 实例，指定 FFT 大小、跳跃长度、谐波中值滤波器大小、打击乐中值滤波器大小和掩蔽模式",
     "Constructs `HarmonicPercussiveSeparator` specifying FFT size, hop length, harmonic/percussive median filter sizes, and masking mode"),
    ("构造 DynamicsProcessor 实例，指定动态处理器模式、采样率、阈值、比率、补偿增益、攻击时间、释放时间和最小振幅阈值",
     "Constructs `DynamicsProcessor` specifying dynamics mode, sample rate, threshold, ratio, makeup gain, attack/release times, and minimum amplitude threshold"),
    ("构造", "Constructs"),
    ("生成新的样本", "Generates the next sample"),
    ("生成信号，通过逐个生成所有样本", "Generates signal sample by sample"),
    ("生成", "Generates"),
    ("滤波", "Filters"),
    ("应用", "Applies"),
    ("设计", "Designs"),
    ("表示", "Represents"),
    ("开始", "Starts"),
    ("处理整个信号并返回新的滤波信号", "Processes entire signal and returns new filtered signal"),
    ("处理", "Processes"),
    ("对 `signal` 进行插值并进行低通滤波", "Interpolates and low-pass filters `signal`"),
    ("通过叠加信号 s1 和 s2 创建新信号。如果大小不同，则较小的信号将广播以适应较大的信号大小",
     "Creates new signal by superimposing s1 and s2; if sizes differ, smaller signal is broadcast to match larger"),
    ("通过将常数添加到信号 s 创建新信号", "Creates new signal by adding constant to signal s"),
    ("通过从信号 s 中减去常数创建新信号", "Creates new signal by subtracting constant from signal s"),
    ("通过将信号 s 乘以系数（放大/衰减）创建新信号", "Creates new signal by multiplying signal s by coefficient (amplify/attenuate)"),
    ("通过", "By"),
    ("在线", "Online"),
    ("迭代", "Iterates"),
]

DOC_TEMPLATES = [
    (r"^#### (\d+)\. (.+?) 构造函数$", r"#### \1. \2 constructor"),
    (r"^  - 返回值: (.+)$", r"  - Returns: \1"),
    (r"^  - 构造 (.+?) 实例，指定(.+)。$", r"  - Constructs \1 instance, specifying \2."),
    (r"^  - 使用 (.+?) 构造 (.+?) 实例。$", r"  - Constructs \2 instance using \1."),
    (r"^  - 使用 (.+?) 和 (.+?) 构造 (.+?) 实例。$", r"  - Constructs \3 instance using \1 and \2."),
    (r"^    - `([^`]+)`: (.+)$", None),
    (r"^  - 样本索引器。仅适用于样本实部数组。谨慎使用。$", r"  - Sample indexer; applies to real-part array only; use with care."),
    (r"^        // 设置(.+)$", r"        // Set \1"),
    (r"^        // 创建(.+?) 实例$", r"        // Create \1 instance"),
    (r"^        // 创建(.+)$", r"        // Create \1"),
    (r"^        // 定义(.+)$", r"        // Define \1"),
    (r"^        // 生成(.+)$", r"        // Generate \1"),
    (r"^        // 执行(.+)$", r"        // Run \1"),
    (r"^        // 使用(.+)$", r"        // Use \1"),
    (r"^        // 处理(.+)$", r"        // Process \1"),
    (r"^        // 应用(.+)$", r"        // Apply \1"),
    (r"^        // 设计(.+)$", r"        // Design \1"),
    (r"^        // 计算(.+)$", r"        // Compute \1"),
    (r"^        // 重置(.+)$", r"        // Reset \1"),
    (r"^        // 输出(.+)$", r"        // Print \1"),
    (r"^        // 加载(.+)$", r"        // Load \1"),
    (r"^        // 保存(.+)$", r"        // Save \1"),
    (r"^        // 训练(.+)$", r"        // Train \1"),
    (r"^        // 预测(.+)$", r"        // Predict \1"),
    (r"^        // 更新(.+)$", r"        // Update \1"),
    (r"^        // 初始化(.+)$", r"        // Initialize \1"),
    (r"^        // 配置(.+)$", r"        // Configure \1"),
    (r"^        // 连接(.+)$", r"        // Chain \1"),
    (r"^        // 添加(.+)$", r"        // Add \1"),
    (r"^        // 移除(.+)$", r"        // Remove \1"),
    (r"^        // 复制(.+)$", r"        // Copy \1"),
    (r"^        // 比较(.+)$", r"        // Compare \1"),
    (r"^        // 验证(.+)$", r"        // Verify \1"),
    (r"^        // 测试(.+)$", r"        // Test \1"),
    (r"^        // 演示(.+)$", r"        // Demonstrate \1"),
    (r"^        // 打印(.+)$", r"        // Print \1"),
    (r"^        // 显示(.+)$", r"        // Display \1"),
    (r"^        // 读取(.+)$", r"        // Read \1"),
    (r"^        // 写入(.+)$", r"        // Write \1"),
    (r"^        // 卷积(.+)$", r"        // Convolve \1"),
    (r"^        // 重采样(.+)$", r"        // Resample \1"),
    (r"^        // 调制(.+)$", r"        // Modulate \1"),
    (r"^        // 解调(.+)$", r"        // Demodulate \1"),
    (r"^        // 分离(.+)$", r"        // Separate \1"),
    (r"^        // 重建(.+)$", r"        // Reconstruct \1"),
]

COMMENT_RE = list(_gen.COMMENT_RE)
EXAMPLE_INTRO_RE = _gen.EXAMPLE_INTRO_RE


def fix_links(line: str) -> str:
    for a, b in LINKS:
        line = line.replace(a, b)
    return line


def fix_anchors(line: str) -> str:
    return _gen.fix_anchors(line)


def apply_safe_phrases(text: str) -> str:
    for zh, en in SAFE_PHRASES:
        if zh in text:
            text = text.replace(zh, en)
    return text


def translate_tail(text: str) -> str:
    text = text.rstrip("。").rstrip("：").rstrip()
    text = apply_safe_phrases(text)
    text = text.replace("，", ", ")
    text = text.replace("。", ".")
    text = text.replace("：", ": ")
    text = text.replace("（", " (")
    text = text.replace("）", ")")
    text = text.replace("、", ", ")
    text = text.replace("；", "; ")
    text = re.sub(r"\s+", " ", text)
    text = re.sub(r" +([,.;:!?])", r"\1", text)
    return text.strip()


def translate_bullet_body(body: str) -> str:
    body = body.rstrip("。").rstrip()
    for verb, en in VERB_PREFIXES:
        if body.startswith(verb):
            rest = body[len(verb):].lstrip("了").lstrip("的").lstrip()
            if rest:
                return f"{en} {translate_tail(rest)}".strip()
            return en
    return translate_tail(body)


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

    for pat, repl in DOC_TEMPLATES:
        if repl is None:
            continue
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
        if out.startswith("|") and out.count("|") >= 2:
            parts = out.split("|")
            if len(parts) >= 3:
                parts[-2] = " " + translate_tail(parts[-2].strip()) + " "
                out = "|".join(parts)
        elif re.match(r"^    - `[^`]+`: ", out):
            m2 = re.match(r"^(    - `[^`]+`: )(.+)$", out)
            if m2:
                out = m2.group(1) + translate_tail(m2.group(2))
        elif re.match(r'^- `"', out):
            m2 = re.match(r'^(- `"[^`]+"[^:]*: )(.+)$', out)
            if m2:
                out = m2.group(1) + translate_tail(m2.group(2))
        elif out.startswith("    - "):
            out = f"    - {translate_bullet_body(out[6:])}"
        elif out.startswith("  - "):
            body = out[4:]
            out = f"  - {translate_bullet_body(body)}"
        elif out.startswith("- "):
            out = f"- {translate_bullet_body(out[2:])}"
        elif out.startswith("> 以下类型均位于"):
            out = re.sub(r"> 以下类型均位于(?:命名空间 ：|命名空间：|)(.+?)(?:命名空间)?[。.]?$",
                         r"> All types below are in namespace: \1", out)
        elif out.startswith("- :bookmark:"):
            out = fix_anchors(out)
        else:
            out = translate_tail(out)

        if re.search(r"[\u4e00-\u9fff]", out):
            out = apply_safe_phrases(out)

    out = fix_anchors(out)
    if out.endswith("。"):
        out = out[:-1] + "."
    if out.endswith("："):
        out = out[:-1] + ":"
    out = re.sub(r"\s+", " ", out)
    out = re.sub(r" +([,.;:!?])", r"\1", out)
    return out.strip()


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
    OUT_JSON.write_text(json.dumps(mapping, ensure_ascii=False, indent=2), encoding="utf-8")

    remaining = [k for k, v in mapping.items() if re.search(r"[\u4e00-\u9fff]", v)]
    still_path = ROOT / "tools" / "sp_still_chinese.json"
    still_path.write_text(json.dumps(remaining, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Generated {len(mapping)} translations, {len(remaining)} still have Chinese")
    return 0


if __name__ == "__main__":
    sys.exit(main())
