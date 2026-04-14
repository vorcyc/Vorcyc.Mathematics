#!/usr/bin/env python3
"""Generate manual overrides for remaining Chinese wiki lines."""
from __future__ import annotations

import importlib.util
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXACT_JSON = ROOT / "tools" / "sp_exact_translations.json"
OUT_JSON = ROOT / "tools" / "sp_manual_overrides.json"

GEN_PATH = ROOT / "tools" / "generate_sp_exact.py"
_spec = importlib.util.spec_from_file_location("generate_sp_exact", GEN_PATH)
_gen = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_gen)

# Sentence-level templates on Chinese source (key), longest first
CN_TEMPLATES: list[tuple[str, str]] = [
    # ADSR / builders
    (r"^  - 表示 ADSR 包络的状态。$", "  - Represents ADSR envelope state."),
    (r"^  - 获取当前的 ADSR 状态（攻击、衰减、持续、释放）。$", "  - Gets the current ADSR state (attack, decay, sustain, release)."),
    (r"^  - 使用 ADSR 参数（以样本数表示）构造 AdsrBuilder 实例。$", "  - Constructs an `AdsrBuilder` instance with ADSR parameters (in sample count)."),
    (r"^    - `attack`: 攻击阶段的样本数。$", "    - `attack`: Sample count for attack phase."),
    (r"^    - `decay`: 衰减阶段的样本数。$", "    - `decay`: Sample count for decay phase."),
    (r"^    - `sustain`: 持续阶段的样本数。$", "    - `sustain`: Sample count for sustain phase."),
    (r"^    - `release`: 释放阶段的样本数。$", "    - `release`: Sample count for release phase."),
    (r"^  - 使用 ADSR 参数（以秒为单位）构造 AdsrBuilder 实例。$", "  - Constructs an `AdsrBuilder` instance with ADSR parameters (in seconds)."),
    (r"^    - `attack`: 攻击阶段的持续时间（秒）。$", "    - `attack`: Attack phase duration (seconds)."),
    (r"^    - `decay`: 衰减阶段的持续时间（秒）。$", "    - `decay`: Decay phase duration (seconds)."),
    (r"^    - `sustain`: 持续阶段的持续时间（秒）。$", "    - `sustain`: Sustain phase duration (seconds)."),
    (r"^    - `release`: 释放阶段的持续时间（秒）。$", "    - `release`: Release phase duration (seconds)."),
    (r"^  - 生成新的样本。$", "  - Generates the next sample."),
    (r"^  - 返回值: 生成的样本。$", "  - Returns: generated sample."),
    (r"^  - 重置样本生成器。$", "  - Resets the sample generator."),
    (r"^  - 设置信号的采样率。$", "  - Sets the signal sample rate."),
    (r"^  - 返回值: 设置采样率后的 SignalBuilder 实例。$", "  - Returns: `SignalBuilder` after setting sample rate."),
    (r"^        // 生成样本并输出$", "        // Generate samples and print"),
    (r"^        // 重置样本生成器$", "        // Reset sample generator"),
    (r"^        // 使用秒数构造 AdsrBuilder 实例$", "        // Construct AdsrBuilder with seconds"),
    (r"^    - `Attack`: 攻击阶段。$", "    - `Attack`: Attack phase."),
    (r"^    - `Decay`: 衰减阶段。$", "    - `Decay`: Decay phase."),
    (r"^    - `Sustain`: 持续阶段。$", "    - `Sustain`: Sustain phase."),
    (r"^    - `Release`: 释放阶段。$", "    - `Release`: Release phase."),
    (r"^#### (\d+)\. (.+?) 构造函数$", r"#### \1. \2 constructor"),
    # ComplexDiscreteSignal
    (r"^  - 获取采样率（每秒的样本数）。$", "  - Gets sample rate (samples per second)."),
    (r"^  - 获取信号的长度。$", "  - Gets signal length."),
    (r"^  - 获取复数值样本的功率（幅度的平方）。$", "  - Gets power of complex sample (amplitude squared)."),
    (r"^  - 获取复数值样本的展开相位。$", "  - Gets unwrapped phase of complex sample."),
    (r"^    - `allocateNew`: 如果应为数据分配新内存，则设置为 true，默认为 false。$",
     "    - `allocateNew`: If new memory should be allocated for data, set to true; default false."),
    (r"^  - 构造具有指定长度并填充指定值的信号。$", "  - Constructs signal with specified length filled with specified value."),
    (r"^    - `real`: 每个样本的实部值，默认为 0.0。$", "    - `real`: Real part value per sample; default 0.0."),
    (r"^    - `imag`: 每个样本的虚部值，默认为 0.0。$", "    - `imag`: Imaginary part value per sample; default 0.0."),
    (r"^  - 使用整数样本集合构造信号，并在给定采样率下进行归一化。$",
     "  - Constructs signal from integer sample set and normalizes at given sample rate."),
    (r"^    - `samples`: 整数样本集合。$", "    - `samples`: Integer sample set."),
    (r"^    - `normalizeFactor`: 每个样本将除以此值，默认为 1.0。$",
     "    - `normalizeFactor`: Each sample is divided by this value; default 1.0."),
    (r"^  - 创建信号的深拷贝。$", "  - Creates a deep copy of the signal."),
    (r"^  - 返回值: 信号的深拷贝。$", "  - Returns: deep copy of the signal."),
    (r"^  - 样本索引器。仅适用于样本实部数组。谨慎使用。$", "  - Sample indexer; applies to real-part array only; use with care."),
    (r"^  - 创建信号的切片。$", "  - Creates a slice of the signal."),
    (r"^    - `startPos`: 第一个样本的索引（包含）。$", "    - `startPos`: Index of first sample (inclusive)."),
    (r"^    - `endPos`: 最后一个样本的索引（不包含）。$", "    - `endPos`: Index of last sample (exclusive)."),
    (r"^  - 返回值: 信号的切片。$", "  - Returns: slice of the signal."),
    (r"^  - 通过叠加信号 s1 和 s2 创建新信号。如果大小不同，则较小的信号将广播以适应较大的信号大小。$",
     "  - Creates new signal by superimposing s1 and s2; if sizes differ, smaller signal is broadcast to match larger."),
    (r"^    - `s1`: 第一个信号。$", "    - `s1`: First signal."),
    (r"^    - `s2`: 第二个信号。$", "    - `s2`: Second signal."),
    (r"^  - 返回值: 叠加后的新信号。$", "  - Returns: new superimposed signal."),
    (r"^  - 通过将常数添加到信号 s 创建新信号。$", "  - Creates new signal by adding constant to signal s."),
    (r"^    - `constant`: 要添加到每个样本的常数。$", "    - `constant`: Constant to add to each sample."),
    (r"^  - 返回值: 添加常数后的新信号。$", "  - Returns: new signal after adding constant."),
    (r"^  - 通过从信号 s 中减去常数创建新信号。$", "  - Creates new signal by subtracting constant from signal s."),
    (r"^    - `constant`: 要从每个样本中减去的常数。$", "    - `constant`: Constant to subtract from each sample."),
    (r"^  - 返回值: 减去常数后的新信号。$", "  - Returns: new signal after subtracting constant."),
    (r"^  - 通过将信号 s 乘以系数（放大/衰减）创建新信号。$",
     "  - Creates new signal by multiplying signal s by coefficient (amplify/attenuate)."),
    (r"^    - `coeff`: 放大/衰减系数。$", "    - `coeff`: Amplify/attenuate coefficient."),
    # Generic doc patterns
    (r"^  - 获取(.+)。$", r"  - Gets \1."),
    (r"^  - 返回(.+)。$", r"  - Returns \1."),
    (r"^  - 计算(.+)。$", r"  - Computes \1."),
    (r"^  - 执行(.+)。$", r"  - Performs \1."),
    (r"^  - 使用(.+)。$", r"  - Uses \1."),
    (r"^  - 创建(.+)。$", r"  - Creates \1."),
    (r"^  - 构造(.+)。$", r"  - Constructs \1."),
    (r"^  - 设置(.+)。$", r"  - Sets \1."),
    (r"^  - 重置(.+)。$", r"  - Resets \1."),
    (r"^  - 生成(.+)。$", r"  - Generates \1."),
    (r"^  - 应用(.+)。$", r"  - Applies \1."),
    (r"^  - 设计(.+)。$", r"  - Designs \1."),
    (r"^  - 训练(.+)。$", r"  - Trains \1."),
    (r"^  - 预测(.+)。$", r"  - Predicts \1."),
    (r"^  - 更新(.+)。$", r"  - Updates \1."),
    (r"^  - 初始化(.+)。$", r"  - Initializes \1."),
    (r"^  - 处理(.+)。$", r"  - Processes \1."),
    (r"^  - 滤波(.+)。$", r"  - Filters \1."),
    (r"^  - 判断(.+)。$", r"  - Determines whether \1."),
    (r"^  - 表示(.+)。$", r"  - Represents \1."),
    (r"^  - 返回值: (.+)$", r"  - Returns: \1"),
    (r"^  - 参数:$", "  - Parameters:"),
    (r"^  - 异常:$", "  - Exceptions:"),
    (r"^  - 枚举成员:$", "  - Enum members:"),
    (r"^        // 创建(.+?) 实例$", r"        // Create \1 instance"),
    (r"^        // 创建(.+)$", r"        // Create \1"),
    (r"^        // 设置(.+)$", r"        // Set \1"),
    (r"^        // 定义(.+)$", r"        // Define \1"),
    (r"^        // 生成(.+)$", r"        // Generate \1"),
    (r"^        // 使用(.+)$", r"        // Use \1"),
    (r"^        // 执行(.+)$", r"        // Run \1"),
    (r"^        // 处理(.+)$", r"        // Process \1"),
    (r"^        // 应用(.+)$", r"        // Apply \1"),
    (r"^        // 设计(.+)$", r"        // Design \1"),
    (r"^        // 计算(.+)$", r"        // Compute \1"),
    (r"^        // 重置(.+)$", r"        // Reset \1"),
    (r"^        // 输出(.+)$", r"        // Print \1"),
    (r"^        // 加载(.+)$", r"        // Load \1"),
    (r"^        // 保存(.+)$", r"        // Save \1"),
]

# Chinese fragment -> English for captured groups
FRAG: dict[str, str] = {
    "采样率（每秒的样本数）": "sample rate (samples per second)",
    "信号的长度": "signal length",
    "复数值样本的功率（幅度的平方）": "power of complex sample (amplitude squared)",
    "复数值样本的展开相位": "unwrapped phase of complex sample",
    "信号的深拷贝": "deep copy of the signal",
    "信号的切片": "slice of the signal",
    "叠加后的新信号": "new superimposed signal",
    "添加常数后的新信号": "new signal after adding constant",
    "减去常数后的新信号": "new signal after subtracting constant",
    "生成的样本": "generated sample",
    "设置采样率后的 SignalBuilder 实例": "`SignalBuilder` after setting sample rate",
    "样本生成器": "the sample generator",
    "信号的采样率": "the signal sample rate",
    "谐波的振幅": "harmonic amplitudes",
    "当前的 PadSynthBuilder 实例": "current `PadSynthBuilder` instance",
    "当前的 FadeInOutBuilder 实例": "current `FadeInOutBuilder` instance",
    "信号是否开始淡出": "whether fade-out has started",
    "信号是否完成淡出": "whether fade-out has completed",
    "底层信号生成器": "underlying signal generator",
    "淡入部分的持续时间（秒）": "fade-in duration (seconds)",
    "淡出部分的持续时间（秒）": "fade-out duration (seconds)",
    "持续时间（秒）": "duration (seconds)",
    "样本数组": "sample array",
    "波表样本数组": "wavetable sample array",
    "整数样本集合": "integer sample set",
    "第一个信号": "first signal",
    "第二个信号": "second signal",
    "攻击阶段": "attack phase",
    "衰减阶段": "decay phase",
    "持续阶段": "sustain phase",
    "释放阶段": "release phase",
    "反馈系数": "feedback coefficient",
    "概率": "probability",
    "振幅数组": "amplitude array",
    "斜率": "slope",
    "截距": "intercept",
    "步长": "step",
    "拉伸因子": "stretch factor",
    "均值": "mean",
    "标准差": "standard deviation",
    "下限幅度": "lower amplitude",
    "上限幅度": "upper amplitude",
    "起始频率": "start frequency",
    "结束频率": "end frequency",
    "初始相位": "initial phase",
    "脉冲持续时间": "pulse duration",
    "脉冲波周期": "pulse wave period",
    "带宽比例": "bandwidth scale",
    "缩放比例": "scale factor",
    "FFT 大小": "FFT size",
    "放大/衰减系数": "amplify/attenuate coefficient",
    "要添加到每个样本的常数": "constant to add to each sample",
    "要从每个样本中减去的常数": "constant to subtract from each sample",
    "第一个样本的索引（包含）": "index of first sample (inclusive)",
    "最后一个样本的索引（不包含）": "index of last sample (exclusive)",
    "每个样本的实部值，默认为 0.0": "real part value per sample; default 0.0",
    "每个样本的虚部值，默认为 0.0": "imaginary part value per sample; default 0.0",
    "每个样本将除以此值，默认为 1.0": "each sample is divided by this value; default 1.0",
    "如果应为数据分配新内存，则设置为 true，默认为 false":
    "if new memory should be allocated for data, set to true; default false",
    "具有指定长度并填充指定值的信号": "signal with specified length filled with specified value",
    "通过逐个生成所有样本": "sample by sample",
    "开始淡出": "fade-out",
    "生成信号，通过逐个生成所有样本": "Generates signal sample by sample",
    "生成新的样本": "the next sample",
    "设置信号的采样率": "the signal sample rate",
    "设置谐波的振幅": "harmonic amplitudes",
    "使用样本数组构造 KarplusStrongBuilder 实例": "`KarplusStrongBuilder` from sample array",
    "使用秒数构造 AdsrBuilder 实例": "AdsrBuilder with seconds",
    "使用样本数构造 AdsrBuilder 实例": "AdsrBuilder with sample count",
    "创建一个 SineBuilder 实例": "a SineBuilder instance",
    "创建一个 FadeInOutBuilder 实例，装饰 SineBuilder": "FadeInOutBuilder decorating SineBuilder",
    "定义波表样本数组": "wavetable sample array",
    "设置信号的采样率": "signal sample rate",
    "设置谐波的振幅": "harmonic amplitudes",
    "生成样本并输出": "samples and print",
    "重置样本生成器": "sample generator",
    "创建 AwgnBuilder 实例": "AwgnBuilder instance",
    "创建 ChirpBuilder 实例": "ChirpBuilder instance",
    "创建 CosineBuilder 实例": "CosineBuilder instance",
    "创建 SineBuilder 实例": "SineBuilder instance",
    "创建 SawtoothBuilder 实例": "SawtoothBuilder instance",
    "创建 SquareWaveBuilder 实例": "SquareWaveBuilder instance",
    "创建 TriangleWaveBuilder 实例": "TriangleWaveBuilder instance",
    "创建 WhiteNoiseBuilder 实例": "WhiteNoiseBuilder instance",
    "创建 PinkNoiseBuilder 实例": "PinkNoiseBuilder instance",
    "创建 RedNoiseBuilder 实例": "RedNoiseBuilder instance",
    "创建 PulseWaveBuilder 实例": "PulseWaveBuilder instance",
    "创建 RampBuilder 实例": "RampBuilder instance",
    "创建 SincBuilder 实例": "SincBuilder instance",
    "创建 WaveTableBuilder 实例": "WaveTableBuilder instance",
    "创建 PadSynthBuilder 实例": "PadSynthBuilder instance",
    "创建 PerlinNoiseBuilder 实例": "PerlinNoiseBuilder instance",
    "创建 KarplusStrongBuilder 实例": "KarplusStrongBuilder instance",
    "创建 KarplusStrongDrumBuilder 实例": "KarplusStrongDrumBuilder instance",
    "创建 FadeInOutBuilder 实例": "FadeInOutBuilder instance",
    "创建 AdsrBuilder 实例": "AdsrBuilder instance",
}
FRAG_LIST = sorted(FRAG.items(), key=lambda x: len(x[0]), reverse=True)


def translate_frag(s: str) -> str:
    for zh, en in FRAG_LIST:
        s = s.replace(zh, en)
    # apply generator phrase translation on remainder
    s = _gen.cn_to_en(s)
    return s


def translate_cn_line(line: str) -> str | None:
    for pat, repl in CN_TEMPLATES:
        m = re.match(pat, line)
        if m:
            if "\\1" in repl or r"\1" in repl:
                groups = m.groups()
                parts = []
                for g in groups:
                    parts.append(translate_frag(g))
                try:
                    return repl.format(*parts) if "{" in repl else re.sub(pat, repl, line)
                except Exception:
                    out = repl
                    for i, g in enumerate(groups, 1):
                        out = out.replace(f"\\{i}", translate_frag(g))
                    return out
            return repl
    return None


def main() -> int:
    exact = json.loads(EXACT_JSON.read_text(encoding="utf-8"))
    overrides: dict[str, str] = {}
    if OUT_JSON.exists():
        overrides = json.loads(OUT_JSON.read_text(encoding="utf-8"))

    for zh, en in exact.items():
        if re.search(r"[\u4e00-\u9fff]", en):
            tr = translate_cn_line(zh)
            if tr and not re.search(r"[\u4e00-\u9fff]", tr):
                overrides[zh] = _gen.fix_anchors(_gen.fix_links(tr))
            else:
                tr2 = _gen.translate_line(zh)
                if not re.search(r"[\u4e00-\u9fff]", tr2):
                    overrides[zh] = tr2

    OUT_JSON.write_text(json.dumps(overrides, ensure_ascii=False, indent=2), encoding="utf-8")
    remaining = [k for k, v in exact.items() if k not in overrides and re.search(r"[\u4e00-\u9fff]", v)]
    print(f"Overrides: {len(overrides)}, still need: {len(remaining)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
