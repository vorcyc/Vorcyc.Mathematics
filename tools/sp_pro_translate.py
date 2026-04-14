#!/usr/bin/env python3
"""Professional translator for remaining SP wiki lines -> manual overrides."""
from __future__ import annotations

import importlib.util
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
STILL = ROOT / "tools" / "sp_still_chinese.json"
OVERRIDES = ROOT / "tools" / "sp_manual_overrides.json"

_spec = importlib.util.spec_from_file_location("gen", ROOT / "tools" / "generate_sp_exact.py")
gen = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(gen)

_spec2 = importlib.util.spec_from_file_location("cn", ROOT / "tools" / "cn_to_en_sp.py")
cn = importlib.util.module_from_spec(_spec2)
_spec2.loader.exec_module(cn)

# Extra phrases for gaps not covered by cn_to_en_sp / generate_sp_exact
EXTRA: list[tuple[str, str]] = [
    ("余弦信号", "cosine signals"),
    ("周期性脉冲波", "periodic pulse waves"),
    ("直线信号", "ramp (linear) signals"),
    ("信号的形式为", "Signal form:"),
    ("基于波表的信号", "wavetable-based signals"),
    ("脉冲持续时间", "pulse duration"),
    ("脉冲波周期", "pulse wave period"),
    (" 秒。", " seconds."),
    ("秒。", "seconds."),
    ("使用实部和虚部数组构造", "Constructs from real and imaginary arrays"),
    ("使用实部和虚部集合构造", "Constructs from real and imaginary sets"),
    ("使用复数值样本集合构造", "Constructs from complex sample set"),
    ("实部和虚部数组", "real and imaginary arrays"),
    ("实部和虚部", "real and imaginary parts"),
    ("并输出", "and print"),
    ("获取并输出", "Get and print"),
    ("信号的幅度", "signal magnitude"),
    ("信号的相位", "signal phase"),
    ("信号的样本", "signal samples"),
    ("信号的能量", "signal energy"),
    ("信号的均方根值", "signal RMS"),
    ("信号的过零率", "signal zero-crossing rate"),
    ("信号的香农熵", "signal Shannon entropy"),
    ("创建 `signal` 的延迟副本，通过将其向右（正 `delay`）或向左（负 `delay`）移动。",
     "Creates a delayed copy of `signal` by shifting right (positive `delay`) or left (negative `delay`)."),
    ("延迟（正数或负数的延迟样本数）", "delay in samples (positive or negative)"),
    ("延迟后的", "delayed "),
    ("叠加 `signal1` 和 `signal2`。如果大小不同，则较小的信号将广播以适应较大的信号大小。",
     "Superimposes `signal1` and `signal2`; if sizes differ, the smaller signal is broadcast to match the larger."),
    ("叠加后的", "superimposed "),
    ("连接 `signal1` 和 `signal2`。", "Concatenates `signal1` and `signal2`."),
    ("连接后的", "concatenated "),
    ("创建 `signal` 的副本，并重复 `n` 次。", "Creates a copy of `signal` repeated `n` times."),
    ("重复次数", "repeat count"),
    ("重复后的", "repeated "),
    ("按 `coeff` 放大 `signal`。", "Amplifies `signal` by `coeff`."),
    ("按 `coeff` 衰减 `signal`。", "Attenuates `signal` by `coeff`."),
    ("放大系数", "amplification coefficient"),
    ("衰减系数", "attenuation coefficient"),
    ("从 `signal` 的前 `n` 个样本创建新信号。", "Creates a new signal from the first `n` samples of `signal`."),
    ("从 `signal` 的后 `n` 个样本创建新信号。", "Creates a new signal from the last `n` samples of `signal`."),
    ("从 `signal` 创建新的零填充复数离散信号。", "Creates a new zero-padded complex discrete signal from `signal`."),
    ("零填充信号的长度", "length of zero-padded signal"),
    ("零填充后的", "zero-padded "),
    ("执行 `signal1` 和 `signal2` 的复数乘法（按长度归一化）。",
     "Performs complex multiplication of `signal1` and `signal2` (normalized by length)."),
    ("执行 `signal1` 和 `signal2` 的复数除法（按长度归一化）。",
     "Performs complex division of `signal1` and `signal2` (normalized by length)."),
    ("乘法后的", "multiplied "),
    ("除法后的", "divided "),
    ("展开复数值样本的相位。", "Unwraps phase of complex samples."),
    ("展开后的相位数组", "unwrapped phase array"),
    ("从 `signal` 样本中生成复数。", "Generates complex numbers from `signal` samples."),
    ("复数序列", "sequence of complex numbers"),
    ("获取前 n 个样本", "first n samples"),
    ("获取后 n 个样本", "last n samples"),
    ("零填充信号", "zero-padded signal"),
    ("复数乘法", "complex multiplication"),
    ("复数除法", "complex division"),
    ("获取样本量，分配后不变。", "Gets sample count (fixed after allocation)."),
    ("获取有效样本量。", "Gets effective sample count."),
    ("获取信号的持续时间。", "Gets signal duration."),
    ("获取信号的样本数组。", "Gets signal sample array."),
    ("获取信号的内存策略。", "Gets signal memory strategy."),
    ("使用采样率和持续时间构造 DiscreteSignal 实例。", "Constructs `DiscreteSignal` from sample rate and duration."),
    ("信号的持续时间", "signal duration"),
    ("内存策略，默认为 `MemoryStrategy.Immediate`", "memory strategy; default `MemoryStrategy.Immediate`"),
    ("使用采样率和样本数量构造 DiscreteSignal 实例。", "Constructs `DiscreteSignal` from sample rate and sample count."),
    ("样本数量", "sample count"),
    ("使用采样率和样本数组构造 DiscreteSignal 实例。", "Constructs `DiscreteSignal` from sample rate and sample array."),
    ("使用采样率、样本数组、偏移量和样本数量构造 DiscreteSignal 实例。",
     "Constructs `DiscreteSignal` from sample rate, sample array, offset, and count."),
    ("样本数组的偏移量", "offset into sample array"),
    ("使用采样率和样本集合构造 DiscreteSignal 实例。", "Constructs `DiscreteSignal` from sample rate and sample set."),
    ("使用采样率和样本段构造 DiscreteSignal 实例。", "Constructs `DiscreteSignal` from sample rate and sample segment."),
    ("样本段", "sample segment"),
    ("使用采样率和样本跨度构造 DiscreteSignal 实例。", "Constructs `DiscreteSignal` from sample rate and sample span."),
    ("样本跨度", "sample span"),
    ("样本索引器。", "Sample indexer."),
    ("样本范围", "sample range"),
    ("计算信号片段的能量。", "Computes energy of signal segment."),
    ("信号片段的能量", "energy of signal segment"),
    ("计算整个信号的能量。", "Computes energy of entire signal."),
    ("整个信号的能量", "energy of entire signal"),
    ("计算信号片段的均方根值。", "Computes RMS of signal segment."),
    ("信号片段的均方根值", "RMS of signal segment"),
    ("计算整个信号的均方根值。", "Computes RMS of entire signal."),
    ("整个信号的均方根值", "RMS of entire signal"),
    ("计算信号片段的过零率。", "Computes zero-crossing rate of signal segment."),
    ("信号片段的过零率", "zero-crossing rate of signal segment"),
    ("计算整个信号的过零率。", "Computes zero-crossing rate of entire signal."),
    ("整个信号的过零率", "zero-crossing rate of entire signal"),
    ("计算信号片段的香农熵。", "Computes Shannon entropy of signal segment."),
    ("直方图的箱数，默认为 32", "histogram bin count; default 32"),
    ("信号片段的香农熵", "Shannon entropy of signal segment"),
    ("计算整个信号的香农熵。", "Computes Shannon entropy of entire signal."),
    ("整个信号的香农熵", "Shannon entropy of entire signal"),
    ("创建信号 s 的取反副本。", "Creates an inverted copy of signal s."),
    ("取反后的新信号", "new inverted signal"),
    ("通过从信号 s1 中减去信号 s2 创建新信号。如果大小不同，则较小的信号将广播以适应较大的信号大小。",
     "Creates new signal by subtracting s2 from s1; if sizes differ, smaller signal is broadcast to match larger."),
    ("减去后的新信号", "new signal after subtraction"),
    ("提供了用于处理离散信号的扩展方法。", "provides extension methods for discrete signals."),
    ("在给定的 `positions` 位置处多次叠加 `signal2` 和 `signal1`。",
     "Superimposes `signal2` onto `signal1` at given `positions` multiple times."),
    ("插入 `signal2` 的位置（索引）", "positions (indices) to insert `signal2`"),
    ("在 `signal1` 和 `signal2` 之间进行线性交叉淡化，并返回交叉淡化后的信号，长度等于信号长度之和减去交叉淡化部分的长度。",
     "Linearly crossfades between `signal1` and `signal2`; result length equals sum of lengths minus crossfade region."),
    ("全波整流信号", "full-wave rectified signal"),
    ("半波整流信号", "half-wave rectified signal"),
    ("使用 FFT 对 `input` 和 `kernel` 进行快速卷积，并将结果存储在 `output` 数组中。",
     "Uses FFT for fast convolution of `input` and `kernel`; stores result in `output` array."),
    ("输入信号数组", "input signal array"),
    ("卷积核数组", "kernel array"),
    ("输出结果数组", "output result array"),
    ("使用 FFT 对 `signal1` 和 `signal2` 进行快速互相关。",
     "Uses FFT for fast cross-correlation of `signal1` and `signal2`."),
    ("使用 FFT 对 `input1` 和 `input2` 进行快速互相关，并将结果存储在 `output` 数组中。",
     "Uses FFT for fast cross-correlation of `input1` and `input2`; stores result in `output` array."),
    ("输入信号数组1", "input signal array 1"),
    ("输入信号数组2", "input signal array 2"),
    ("获取跳跃长度：FFT 大小 - 核大小 + 1。", "Gets hop length: FFT size - kernel size + 1."),
    ("构造 OlaBlockConvolver 实例，指定卷积核和 FFT 大小。",
     "Constructs `OlaBlockConvolver` with kernel and FFT size."),
    ("使用 FIR 滤波器核和 FFT 大小构造 OlaBlockConvolver 实例。",
     "Constructs `OlaBlockConvolver` from FIR kernel and FFT size."),
    ("在线更改卷积核系数。", "Changes kernel coefficients online."),
    ("新的卷积核", "new kernel"),
    ("处理一个样本。", "Processes one sample."),
    ("处理后的样本", "processed sample"),
    ("处理整个信号并返回新的滤波信号。", "Processes entire signal and returns new filtered signal."),
    ("处理后的 `DiscreteSignal` 对象", "processed `DiscreteSignal`"),
    ("重置重叠-相加卷积器。", "Resets overlap-add convolver."),
    ("定义卷积核和 FFT 大小", "kernel and FFT size"),
    ("处理整个信号", "entire signal"),
    ("在线更改卷积核", "kernel online"),
    ("处理单个样本", "single sample"),
    ("重置卷积器", "convolver"),
    ("构造 OlsBlockConvolver 实例，指定卷积核和 FFT 大小。",
     "Constructs `OlsBlockConvolver` with kernel and FFT size."),
    ("使用 FIR 滤波器核和 FFT 大小构造 OlsBlockConvolver 实例。",
     "Constructs `OlsBlockConvolver` from FIR kernel and FFT size."),
    ("重置重叠-保存卷积器。", "Resets overlap-save convolver."),
    ("枚举定义了动态处理器的类型（模式）。", "enum defines dynamics processor types (modes)."),
    ("较小的压缩比，例如 1:1, 2:1。", "Lower compression ratio, e.g. 1:1, 2:1."),
    ("较大的压缩比，例如 5:1, 10:1。", "Higher compression ratio, e.g. 5:1, 10:1."),
    ("较小的扩展比，例如 1:1, 2:1。", "Lower expansion ratio, e.g. 1:1, 2:1."),
    ("非常高的压缩比，例如 5:1。", "Very high compression ratio, e.g. 5:1."),
    ("获取或设置压缩/扩展阈值。", "Gets or sets compression/expansion threshold."),
    ("获取或设置压缩/扩展比率。", "Gets or sets compression/expansion ratio."),
    ("构造 DynamicsProcessor 实例，指定动态处理器模式、采样率、阈值、比率、补偿增益、攻击时间、释放时间和最小振幅阈值。",
     "Constructs `DynamicsProcessor` with mode, sample rate, threshold, ratio, makeup gain, attack, release, and minimum amplitude threshold."),
    ("动态处理器模式", "dynamics processor mode"),
    ("压缩/扩展阈值", "compression/expansion threshold"),
    ("压缩/扩展比率", "compression/expansion ratio"),
    ("重置动态处理器。", "Resets dynamics processor."),
    ("处理整个信号并返回新的信号（动态处理）。", "Processes entire signal and returns dynamics-processed signal."),
    ("定义动态处理器模式", "dynamics processor mode"),
    ("重置动态处理器", "dynamics processor"),
    ("获取或设置攻击时间（以秒为单位）。", "Gets or sets attack time (seconds)."),
    ("获取或设置释放时间（以秒为单位）。", "Gets or sets release time (seconds)."),
    ("构造 EnvelopeFollower 实例，指定采样率、攻击时间和释放时间。",
     "Constructs `EnvelopeFollower` with sample rate, attack time, and release time."),
    ("重置包络跟随器。", "Resets envelope follower."),
    ("处理整个信号并返回新的信号（包络）。", "Processes entire signal and returns envelope signal."),
    ("重置包络跟随器", "envelope follower"),
    ("获取或设置幅度增益因子。", "Gets or sets magnitude gain factor."),
    ("构造 GriffinLimReconstructor 实例，指定谱图、窗口大小、跳跃大小、窗口类型和功率。",
     "Constructs `GriffinLimReconstructor` with spectrogram, window size, hop size, window type, and power."),
    ("谱图（谱列表）", "spectrogram (list of spectra)"),
    ("窗口大小，默认为 1024", "window size; default 1024"),
    ("跳跃大小，默认为 256", "hop size; default 256"),
    ("窗口类型，默认为 `WindowType.Hann`", "window type; default `WindowType.Hann`"),
    ("功率（2 - 功率谱，否则 - 幅度谱），默认为 2", "power (2 = power spectrum, else magnitude spectrum); default 2"),
    ("构造 GriffinLimReconstructor 实例，指定谱图、STFT 变换器和功率。",
     "Constructs `GriffinLimReconstructor` with spectrogram, STFT transformer, and power."),
    ("STFT 变换器", "STFT transformer"),
    ("执行一次重建迭代并返回当前步骤的重建信号。",
     "Performs one reconstruction iteration and returns signal for current step."),
    ("上一次迭代重建的信号，默认为 null", "signal from previous iteration; default null"),
    ("当前步骤的重建信号", "reconstructed signal for current step"),
    ("迭代地从谱图中重建信号。", "Iteratively reconstructs signal from spectrogram."),
    ("Griffin-Lim 算法的迭代次数，默认为 20", "Griffin–Lim iteration count; default 20"),
    ("重建的信号", "reconstructed signal"),
    ("定义谱图", "spectrogram"),
    ("执行一次迭代", "one iteration"),
    ("构造 HarmonicPercussiveSeparator 实例，指定 FFT 大小、跳跃长度、谐波中值滤波器大小、打击乐中值滤波器大小和掩蔽模式。",
     "Constructs `HarmonicPercussiveSeparator` with FFT size, hop length, harmonic/percussive median filter sizes, and masking mode."),
    ("跳跃长度（样本数），默认为 512", "hop length (samples); default 512"),
    ("沿时间轴的中值滤波器大小，默认为 17", "median filter size along time axis; default 17"),
    ("沿频率轴的中值滤波器大小，默认为 17", "median filter size along frequency axis; default 17"),
    ("掩蔽模式，默认为 `HpsMasking.WienerOrder2`", "masking mode; default `HpsMasking.WienerOrder2`"),
    ("从给定的信号中评估谐波和打击乐的幅度-相位谱图。两个谱图对象共享相同的相位数组。",
     "Evaluates harmonic and percussive magnitude-phase spectrograms from signal; both share the same phase array."),
    ("包含谐波和打击乐谱图的元组", "tuple of harmonic and percussive spectrograms"),
    ("从给定的信号中提取谐波和打击乐信号。", "Extracts harmonic and percussive signals from given signal."),
    ("包含谐波和打击乐信号的元组", "tuple of harmonic and percussive signals"),
    ("评估谱图", "spectrograms"),
    ("提取谐波和打击乐信号", "harmonic and percussive signals"),
    ("执行环形调制（RM）并返回 RM 信号。", "Performs ring modulation (RM) and returns RM signal."),
    ("调制后的", "modulated "),
    ("执行幅度调制（AM）并返回 AM 信号。", "Performs amplitude modulation (AM) and returns AM signal."),
    ("调制指数（深度），默认为 0.5", "modulation index (depth); default 0.5"),
    ("执行频率调制（FM）并返回 FM 信号。", "Performs frequency modulation (FM) and returns FM signal."),
    ("基带信号", "baseband signal"),
    ("载波幅度", "carrier amplitude"),
    ("载波频率", "carrier frequency"),
    ("频率偏移，默认为 0.1 Hz", "frequency deviation; default 0.1 Hz"),
    ("执行正弦频率调制（FM）并返回正弦 FM 信号。", "Performs sinusoidal FM and returns FM signal."),
    ("FM 信号长度", "FM signal length"),
    ("执行线性频率调制（FM）并返回 FM 信号。", "Performs linear FM and returns FM signal."),
    ("执行相位调制（PM）并返回 PM 信号。", "Performs phase modulation (PM) and returns PM signal."),
    ("频率偏移，默认为 0.8", "frequency deviation; default 0.8"),
    ("基于 Hilbert 变换对信号进行简单的幅度解调。", "Simple amplitude demodulation via Hilbert transform."),
    ("解调后的", "demodulated "),
    ("基于 Hilbert 变换对信号进行简单的频率解调。", "Simple frequency demodulation via Hilbert transform."),
    ("定义载波信号和调制信号", "carrier and modulator signals"),
    ("执行环形调制", "ring modulation"),
    ("提供了多种 DSP/音频操作方法：", "provides DSP/audio operation methods:"),
    ("卷积结果的数组", "convolution result array"),
    ("对 `signal` 和 `kernel` 进行块卷积（使用重叠-相加或重叠-保存算法）。",
     "Block-convolves `signal` and `kernel` (overlap-add or overlap-save)."),
    ("块卷积方法（OverlapAdd / OverlapSave），默认为 `FilteringMethod.OverlapSave`",
     "block convolution method (OverlapAdd / OverlapSave); default `FilteringMethod.OverlapSave`"),
    ("块卷积结果的", "block convolution result "),
    ("对 `signal` 和 `kernel` 进行反卷积。", "Deconvolves `signal` and `kernel`."),
    ("对 `signal` 进行插值并进行低通滤波。", "Interpolates `signal` with low-pass filtering."),
    ("插值因子（例如 factor=2 表示从 8000 Hz -> 16000 Hz）",
     "interpolation factor (e.g. factor=2: 8000 Hz -> 16000 Hz)"),
    ("低通抗混叠滤波器", "low-pass anti-aliasing filter"),
    ("插值结果的", "interpolation result "),
    ("对 `signal` 进行抽取并进行低通滤波。", "Decimates `signal` with low-pass filtering."),
    ("抽取因子（例如 factor=2 表示从 16000 Hz -> 8000 Hz）",
     "decimation factor (e.g. factor=2: 16000 Hz -> 8000 Hz)"),
    ("抽取结果的", "decimation result "),
    ("对 `signal` 进行带限重采样。", "Band-limited resampling of `signal`."),
    ("目标采样率", "target sample rate"),
    ("重采样结果的", "resampling result "),
    ("对 `signal` 进行简单重采样（插值和抽取的组合）。", "Simple resampling of `signal` (interpolation + decimation)."),
    ("插值因子", "interpolation factor"),
    ("抽取因子", "decimation factor"),
    ("对 `signal` 进行时间拉伸，参数由用户设置。", "Time-stretches `signal` with user-specified parameters."),
    ("拉伸因子（比率）", "stretch factor (ratio)"),
    ("窗口大小（对于声码器 - FFT 大小）", "window size (vocoder FFT size)"),
    ("时间拉伸算法，默认为 `TsmAlgorithm.PhaseVocoderPhaseLocking`",
     "time-stretch algorithm; default `TsmAlgorithm.PhaseVocoderPhaseLocking`"),
    ("时间拉伸结果的", "time-stretch result "),
    ("对 `signal` 进行时间拉伸，参数自动推导。", "Time-stretches `signal` with auto-derived parameters."),
    ("提取 `signal` 的包络。", "Extracts envelope of `signal`."),
    ("攻击时间（以秒为单位），默认为 0.01 秒", "attack time (seconds); default 0.01 s"),
    ("释放时间（以秒为单位），默认为 0.05 秒", "release time (seconds); default 0.05 s"),
    ("包络结果的", "envelope result "),
    ("对 `signal` 进行全波整流。", "Full-wave rectifies `signal`."),
    ("全波整流结果的", "full-wave rectification result "),
    ("对 `signal` 进行半波整流。", "Half-wave rectifies `signal`."),
    ("半波整流结果的", "half-wave rectification result "),
    ("使用谱减法对 `signal` 进行去噪。将 `noise` 从 `signal` 中减去。",
     "Denoises `signal` via spectral subtraction; subtracts `noise` from `signal`."),
    ("噪声信号", "noise signal"),
    ("去噪结果的", "denoised result "),
    ("归一化峰值电平。", "Normalizes peak level."),
    ("峰值电平（以分贝为单位），例如 -1dB, -3dB 等", "peak level (dB), e.g. -1 dB, -3 dB"),
    ("归一化结果的", "normalization result "),
    ("相对于输入 `samples` 改变峰值电平（就地）。", "Changes peak level relative to input `samples` (in-place)."),
    ("峰值变化（以分贝为单位），例如 -6dB 表示峰值电平减半",
     "peak change (dB), e.g. -6 dB halves peak level"),
    ("相对于输入 `signal` 改变峰值电平。", "Changes peak level relative to input `signal`."),
    ("改变峰值后的", "peak-adjusted "),
    ("RMS 电平（以分贝为单位），例如 -6dB, -18dB, -26dB 等", "RMS level (dB), e.g. -6 dB, -18 dB, -26 dB"),
    ("相对于输入 `samples` 改变 RMS。", "Changes RMS relative to input `samples`."),
    ("RMS 变化（以分贝为单位），例如 -6dB 表示 RMS 减半", "RMS change (dB), e.g. -6 dB halves RMS"),
    ("相对于输入 `signal` 改变 RMS。", "Changes RMS relative to input `signal`."),
    ("改变 RMS 后的", "RMS-adjusted "),
    ("使用 Welch 方法计算周期图。如果 `samplingRate`=0，则评估功率谱，否则评估功率谱密度。",
     "Computes periodogram via Welch method; if `samplingRate`=0 evaluates power spectrum, else PSD."),
    ("窗口大小（样本数），默认为 1024", "window size (samples); default 1024"),
    ("窗口函数，默认为 `WindowType.Hann`", "window function; default `WindowType.Hann`"),
    ("如果采样率=0，则评估功率谱，否则评估功率谱密度",
     "if sample rate=0 evaluates power spectrum, else power spectral density"),
    ("周期图数组", "periodogram array"),
    ("计算 Lomb-Scargle 周期图。", "Computes Lomb–Scargle periodogram."),
    ("样本时间", "sample times"),
    ("样本时间对应的信号值", "signal values at sample times"),
    ("输出周期图的角频率", "angular frequencies for output periodogram"),
    ("在评估周期图之前从值中减去均值", "subtract mean from values before evaluating periodogram"),
    ("通过数据围绕常数参考模型（零）的残差来归一化周期图",
     "normalize periodogram by residuals around constant reference model (zero)"),
    ("执行全波整流", "full-wave rectification"),
    ("执行谱减法去噪", "spectral subtraction denoising"),
    ("归一化峰值电平", "peak level normalization"),
    ("计算 Welch 周期图", "Welch periodogram"),
    ("执行插值", "interpolation"),
    ("执行抽取", "decimation"),
    ("执行带限重采样", "band-limited resampling"),
    ("执行简单重采样（插值和抽取的组合）", "simple resampling (interpolation + decimation)"),
    ("获取或设置谱底（beta 系数）。默认值为 0.009f。", "Gets or sets spectral floor (beta). Default 0.009f."),
    ("获取或设置减法因子的最小阈值（alpha）。默认值为 2f。", "Gets or sets minimum subtraction factor (alpha). Default 2f."),
    ("获取或设置减法因子的最大阈值（alpha）。默认值为 5f。", "Gets or sets maximum subtraction factor (alpha). Default 5f."),
    ("构造 SpectralSubtractor 实例，指定噪声样本数组、FFT 大小和跳跃长度。",
     "Constructs `SpectralSubtractor` with noise sample array, FFT size, and hop length."),
    ("噪声样本数组", "noise sample array"),
    ("跳跃长度（样本数），默认为 128", "hop length (samples); default 128"),
    ("构造 SpectralSubtractor 实例，指定噪声信号、FFT 大小和跳跃长度。",
     "Constructs `SpectralSubtractor` with noise signal, FFT size, and hop length."),
    ("在每个 STFT 步骤中处理一个谱。", "Processes one spectrum per STFT step."),
    ("输入谱的实部", "real part of input spectrum"),
    ("输入谱的虚部", "imaginary part of input spectrum"),
    ("输出谱的实部", "real part of output spectrum"),
    ("输出谱的虚部", "imaginary part of output spectrum"),
    ("估计噪声的功率谱。", "Estimates noise power spectrum."),
    ("数组中处理的第一个样本的索引", "index of first sample processed in array"),
    ("数组中处理的最后一个样本的索引", "index of last sample processed in array"),
    ("估计噪声信号的功率谱。", "Estimates noise signal power spectrum."),
    ("信号中处理的第一个样本的索引", "index of first sample processed in signal"),
    ("信号中处理的最后一个样本的索引", "index of last sample processed in signal"),
    ("定义噪声样本数组", "noise sample array"),
    ("构造 WaveShaper 实例，指定波形整形函数。", "Constructs `WaveShaper` with waveshaping function."),
    ("波形整形函数", "waveshaping function"),
    ("重置波形整形器。", "Resets waveshaper."),
    ("处理整个信号并返回新的波形整形信号。", "Processes entire signal and returns waveshaped signal."),
    ("定义波形整形函数", "waveshaping function"),
    ("重置波形整形器", "waveshaper"),
    ("输入信号的样本", "input signal samples"),
    ("期望信号的样本", "desired signal samples"),
    ("是一个用于实现最小均方（Least-Mean-Squares）自适应滤波算法的类。",
     "implements the least-mean-squares (LMS) adaptive filtering algorithm."),
    ("构造一个给定阶数的 LmsFilter 实例。", "Constructs `LmsFilter` with given order."),
    ("是一个用于实现归一化最小均方四次（Normalized Least-Mean-Fourth）自适应滤波算法的类。",
     "implements the normalized least-mean-fourth (NLMF) adaptive filtering algorithm."),
    ("构造一个给定阶数的 NlmfFilter 实例。", "Constructs `NlmfFilter` with given order."),
    ("是一个用于实现归一化最小均方（Normalized Least-Mean-Squares）自适应滤波算法的类。",
     "implements the normalized least-mean-squares (NLMS) adaptive filtering algorithm."),
    ("构造一个给定阶数的 NlmsFilter 实例。", "Constructs `NlmsFilter` with given order."),
    ("是一个用于实现递归最小二乘（Recursive-Least-Squares）自适应滤波算法的类。",
     "implements the recursive least-squares (RLS) adaptive filtering algorithm."),
    ("构造一个给定阶数的 RlsFilter 实例。", "Constructs `RlsFilter` with given order."),
    ("是一个用于实现符号最小均方（Sign Least-Mean-Squares）自适应滤波算法的类。",
     "implements the sign least-mean-squares adaptive filtering algorithm."),
    ("构造一个给定阶数的 SignLmsFilter 实例。", "Constructs `SignLmsFilter` with given order."),
    ("是一个用于实现具有可变步长的最小均方（Least-Mean-Squares）自适应滤波算法的类。",
     "implements LMS adaptive filtering with variable step size."),
    ("构造一个给定阶数的 VariableStepLmsFilter 实例。", "Constructs `VariableStepLmsFilter` with given order."),
    ("定义滤波器阶数、步长因子、归一化因子和泄漏因子", "filter order, step size, normalization factor, and leakage factor"),
    ("定义滤波器阶数、遗忘因子和初始化逆相关矩阵的值", "filter order, forgetting factor, and initial inverse correlation matrix value"),
    ("定义滤波器阶数、步长因子数组和泄漏因子", "filter order, step-size array, and leakage factor"),
    ("从在线滤波器集合构造 FilterChain 实例", "Constructs `FilterChain` from online filter collection"),
    ("从传递函数集合（例如，SOS 段）构造 FilterChain 实例。此构造函数在内部创建 IirFilter 对象。",
     "Constructs `FilterChain` from transfer function set (e.g. SOS sections); creates `IirFilter` objects internally."),
    ("将滤波器添加到链中。", "Adds filter to chain."),
    ("在链中指定索引处插入滤波器。", "Inserts filter at specified index in chain."),
    ("从链中移除指定索引处的滤波器。", "Removes filter at specified index from chain."),
    ("通过滤波器链处理一个样本。", "Processes one sample through filter chain."),
    ("重置链中的所有滤波器。", "Resets all filters in chain."),
    ("将滤波器应用于整个信号，并返回新的滤波信号。", "Applies filter to entire signal and returns new filtered signal."),
    ("过滤方法，默认为 `FilteringMethod.Auto`", "filtering method; default `FilteringMethod.Auto`"),
    ("创建 FilterChain 实例并添加滤波器", "create `FilterChain` and add filters"),
    ("重置滤波器链", "filter chain"),
    ("获取或设置在自动模式下切换到重叠保存算法的最小核长度。",
     "Gets or sets minimum kernel length to switch to overlap-save in auto mode."),
]

# Merge all phrase sources
ALL_PHRASES: list[tuple[str, str]] = []
seen_p = set()
for src in (cn.PHRASES, gen.CN_PHRASES, EXTRA):
    for zh, en in src:
        if zh not in seen_p:
            seen_p.add(zh)
            ALL_PHRASES.append((zh, en))
ALL_PHRASES.sort(key=lambda x: len(x[0]), reverse=True)


def apply_phrases(text: str) -> str:
    for zh, en in ALL_PHRASES:
        if zh in text:
            text = text.replace(zh, en)
    return text


def translate_groups(groups: tuple[str, ...]) -> tuple[str, ...]:
    return tuple(apply_phrases(gen.cn_to_en(g)) for g in groups)


def apply_class_patterns(line: str) -> str:
    for pat, repl in gen.CLASS_PATTERNS:
        m = re.match(pat, line)
        if m:
            groups = translate_groups(m.groups())
            out = repl
            for i, g in enumerate(groups, 1):
                out = out.replace(f"\\{i}", g)
            return out
    return line


def apply_comment_patterns(line: str) -> str:
    for pat, repl in gen.COMMENT_RE:
        m = re.match(pat, line)
        if m:
            groups = m.groups()
            out = repl
            for i, g in enumerate(groups, 1):
                val = apply_phrases(gen.cn_to_en(g)) if i > 1 else g
                out = out.replace(f"\\{i}", val)
            return out
    return line


def pro_translate(line: str) -> str:
    line = gen.fix_links(line)
    if line in gen.EXACT:
        return gen.fix_anchors(gen.EXACT[line])
    out = apply_class_patterns(line)
    if out == line:
        out = apply_comment_patterns(line)
    if out == line:
        out = apply_phrases(line)
        out = gen.cn_to_en(out)
    # prefix rules from cn_to_en_sp
    for pat, tmpl in cn.PREFIX_RULES:
        m = re.match(pat, out)
        if m:
            groups = [apply_phrases(gen.cn_to_en(g)) for g in m.groups()]
            try:
                out = tmpl.format(*groups)
            except (IndexError, KeyError):
                out2 = tmpl
                for i, g in enumerate(groups, 1):
                    out2 = out2.replace(f"{{{i-1}}}", g).replace(f"\\{i}", g)
                out = out2
            break
    out = apply_phrases(out)
    out = gen.cn_to_en(out)
    # example intro
    m = gen.EXAMPLE_INTRO_RE.match(out)
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
    # section headers
    out = out.replace("### 私有方法", "### Private methods")
    out = out.replace("### 示例", "### Example")
    out = re.sub(r"^以下是一个使用 (.+?) 类中(.+?)，并在示例中加入了注释：$",
                  lambda m: f"The following example uses `{m.group(1)}` ({m.group(2)}) with comments:", out)
    out = apply_phrases(out)
    out = gen.cn_to_en(out)
    return gen.fix_anchors(out)


# Explicit overrides for lines that still fail phrase translation
EXPLICIT: dict[str, str] = {}


def load_explicit_part2() -> None:
    """Load remaining explicit translations from companion file if present."""
    p = ROOT / "tools" / "sp_explicit_part2.json"
    if p.exists():
        EXPLICIT.update(json.loads(p.read_text(encoding="utf-8")))


def main() -> int:
    load_explicit_part2()
    still = json.loads(STILL.read_text(encoding="utf-8"))
    existing: dict[str, str] = {}
    if OVERRIDES.exists():
        existing = json.loads(OVERRIDES.read_text(encoding="utf-8"))

    added = 0
    for zh in still:
        if zh in EXPLICIT:
            en = EXPLICIT[zh]
        else:
            en = pro_translate(zh)
        if not re.search(r"[\u4e00-\u9fff]", en):
            existing[zh] = en
            added += 1

    OVERRIDES.write_text(json.dumps(existing, ensure_ascii=False, indent=2), encoding="utf-8")
    remaining = [zh for zh in still if zh not in existing or re.search(r"[\u4e00-\u9fff]", existing.get(zh, ""))]
    print(f"Merged {added} new overrides, total {len(existing)}, still need {len(remaining)}")
    if remaining:
        bad = []
        for zh in remaining:
            tr = pro_translate(zh)
            bad.append({"zh": zh, "en": tr, "cn": len(re.findall(r"[\u4e00-\u9fff]", tr))})
        (ROOT / "tools" / "sp_remaining_after_pro.json").write_text(
            json.dumps(bad, ensure_ascii=False, indent=2), encoding="utf-8"
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
