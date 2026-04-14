#!/usr/bin/env python3
"""Generate complete explicit line translations for all still-Chinese SP wiki lines."""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
STILL = ROOT / "tools" / "sp_still_chinese.json"
OUT = ROOT / "tools" / "sp_explicit_part2.json"

# Signal-type fragments for builder class descriptions
SIGNAL_TYPES: dict[str, str] = {
    "余弦信号": "cosine signals",
    "周期性脉冲波": "periodic pulse waves",
    "直线信号": "ramp (linear) signals",
    "红噪声（布朗噪声）": "red noise (Brownian noise)",
    "基于波表的信号": "wavetable-based signals",
    "加性白高斯噪声（AWGN）": "additive white Gaussian noise (AWGN)",
}

FILTER_IMPL: dict[str, str] = {
    "带通贝塞尔": "band-pass Bessel",
    "带阻贝塞尔": "band-stop Bessel",
    "高通贝塞尔": "high-pass Bessel",
    "低通贝塞尔": "low-pass Bessel",
    "双二阶全通": "biquad all-pass",
    "双二阶带通": "biquad band-pass",
    "双二阶 IIR": "biquad IIR",
    "双二阶高通": "biquad high-pass",
    "双二阶高架": "biquad high-shelf",
    "双二阶低通": "biquad low-pass",
    "双二阶低架": "biquad low-shelf",
    "双二阶陷波": "biquad notch",
    "双二阶峰值均衡": "biquad peak EQ",
    "带通巴特沃斯": "band-pass Butterworth",
    "带阻巴特沃斯": "band-stop Butterworth",
    "高通巴特沃斯": "high-pass Butterworth",
    "低通巴特沃斯": "low-pass Butterworth",
    "带通切比雪夫-I型": "band-pass Chebyshev type I",
    "带阻切比雪夫-I型": "band-stop Chebyshev type I",
    "高通切比雪夫-I型": "high-pass Chebyshev type I",
    "低通切比雪夫-I型": "low-pass Chebyshev type I",
    "切比雪夫-I型": "Chebyshev type I",
    "带通切比雪夫-II型": "band-pass Chebyshev type II",
    "带阻切比雪夫-II型": "band-stop Chebyshev type II",
    "高通切比雪夫-II型": "high-pass Chebyshev type II",
    "低通切比雪夫-II型": "low-pass Chebyshev type II",
    "切比雪夫-II型": "Chebyshev type II",
    "带通椭圆": "band-pass elliptic",
    "带阻椭圆": "band-stop elliptic",
    "高通椭圆": "high-pass elliptic",
    "低通椭圆": "low-pass elliptic",
    "椭圆": "elliptic",
    "有限脉冲响应（Finite Impulse Response, FIR）": "finite impulse response (FIR)",
    "无限脉冲响应（Infinite Impulse Response, IIR）": "infinite impulse response (IIR)",
    "顺序连接滤波器链": "serial filter chain",
    "反馈梳状": "feedback comb",
    "前馈梳状": "feedforward comb",
    "直流偏移去除": "DC offset removal",
    "去加重": "de-emphasis",
    "预加重 FIR": "pre-emphasis FIR",
    "非递归移动平均": "non-recursive moving average",
    "快速递归移动平均": "fast recursive moving average",
    "N 阶 Thiran 全通插值": "N-th order Thiran all-pass interpolation",
    "维纳": "Wiener",
    "单极高通": "single-pole high-pass",
    "单极低通": "single-pole low-pass",
}


def t_comment(body: str) -> str:
    m = {
        "使用秒数构造 AdsrBuilder 实例": "Construct AdsrBuilder with seconds",
        "生成样本并输出": "Generate samples and print",
        "创建一个 SineBuilder 实例": "Create a SineBuilder instance",
        "创建一个 FadeInOutBuilder 实例，装饰 SineBuilder": "Create a FadeInOutBuilder instance decorating SineBuilder",
        "设置谐波的振幅": "Set harmonic amplitudes",
        "定义波表样本数组": "Define wavetable sample array",
        "定义实部和虚部数组": "Define real and imaginary arrays",
        "输出信号的实部和虚部": "Print signal real and imaginary parts",
        "获取并输出信号的幅度": "Get and print signal magnitude",
        "获取并输出信号的相位": "Get and print signal phase",
        "获取前 n 个样本": "Get first n samples",
        "获取后 n 个样本": "Get last n samples",
        "定义样本数组": "Define sample array",
        "输出信号的样本": "Print signal samples",
        "获取并输出信号的能量": "Get and print signal energy",
        "获取并输出信号的均方根值": "Get and print signal RMS",
        "获取并输出信号的过零率": "Get and print signal zero-crossing rate",
        "获取并输出信号的香农熵": "Get and print signal Shannon entropy",
        "创建信号的切片": "Create signal slice",
        "创建信号的深拷贝": "Create deep copy of signal",
        "定义动态处理器模式": "Define dynamics processor mode",
        "重置动态处理器": "Reset dynamics processor",
        "重置包络跟随器": "Reset envelope follower",
        "定义谱图": "Define spectrogram",
        "执行一次迭代": "Run one iteration",
        "评估谱图": "Evaluate spectrograms",
        "提取谐波和打击乐信号": "Extract harmonic and percussive signals",
        "定义载波信号和调制信号": "Define carrier and modulator signals",
        "执行环形调制": "Perform ring modulation",
        "执行全波整流": "Perform full-wave rectification",
        "执行谱减法去噪": "Perform spectral subtraction denoising",
        "归一化峰值电平": "Normalize peak level",
        "计算 Welch 周期图": "Compute Welch periodogram",
        "执行插值": "Perform interpolation",
        "执行抽取": "Perform decimation",
        "执行带限重采样": "Perform band-limited resampling",
        "执行简单重采样（插值和抽取的组合）": "Perform simple resampling (interpolation + decimation)",
        "定义噪声样本数组": "Define noise sample array",
        "定义波形整形函数": "Define waveshaping function",
        "重置波形整形器": "Reset waveshaper",
        "定义卷积核和 FFT 大小": "Define kernel and FFT size",
        "处理整个信号": "Process entire signal",
        "在线更改卷积核": "Change kernel online",
        "处理单个样本": "Process single sample",
        "重置卷积器": "Reset convolver",
        "全波整流信号": "Full-wave rectify signal",
        "半波整流信号": "Half-wave rectify signal",
        "零填充信号": "Zero-pad signal",
        "复数乘法": "Complex multiplication",
        "复数除法": "Complex division",
        "定义滤波器核": "Define filter kernel",
        "定义滤波器的分子和分母系数": "Define filter numerator and denominator coefficients",
        "定义传递函数的分子和分母": "Define transfer function numerator and denominator",
        "计算脉冲响应": "Compute impulse response",
        "计算频率响应": "Compute frequency response",
        "执行零相位滤波": "Perform zero-phase filtering",
        "定义低截止频率、高截止频率和滤波器阶数": "Define low/high cutoff frequencies and filter order",
        "更改滤波器的截止频率": "Change filter cutoff frequency",
        "定义截止频率和滤波器阶数": "Define cutoff frequency and filter order",
        "定义贝塞尔多项式的阶数": "Define Bessel polynomial order",
        "获取贝塞尔多项式的系数": "Get Bessel polynomial coefficients",
        "计算贝塞尔滤波器的极点": "Compute Bessel filter poles",
        "定义中心频率和 Q 因子": "Define center frequency and Q factor",
        "更改滤波器的中心频率和 Q 因子": "Change filter center frequency and Q factor",
        "更改滤波器的系数": "Change filter coefficients",
        "定义截止频率和 Q 因子": "Define cutoff frequency and Q factor",
        "更改滤波器的截止频率和 Q 因子": "Change filter cutoff frequency and Q factor",
        "定义架频率、Q 因子和增益": "Define shelf frequency, Q factor, and gain",
        "更改滤波器的架频率、Q 因子和增益": "Change filter shelf frequency, Q factor, and gain",
        "定义中心频率、Q 因子和增益": "Define center frequency, Q factor, and gain",
        "更改滤波器的中心频率、Q 因子和增益": "Change filter center frequency, Q factor, and gain",
        "更改滤波器的低截止频率和高截止频率": "Change filter low and high cutoff frequencies",
        "计算巴特沃斯滤波器的极点": "Compute Butterworth filter poles",
        "定义低截止频率、高截止频率、滤波器阶数和纹波": "Define low/high cutoff, order, and ripple",
        "更改滤波器的低截止频率、高截止频率和纹波": "Change filter low/high cutoff and ripple",
        "定义截止频率、滤波器阶数和纹波": "Define cutoff frequency, order, and ripple",
        "更改滤波器的截止频率和纹波": "Change filter cutoff frequency and ripple",
        "定义滤波器阶数和纹波": "Define filter order and ripple",
        "计算切比雪夫-I型滤波器的极点": "Compute Chebyshev type I filter poles",
        "计算切比雪夫-II型滤波器的极点": "Compute Chebyshev type II filter poles",
        "计算切比雪夫-II型滤波器的零点": "Compute Chebyshev type II filter zeros",
        "定义低截止频率、高截止频率、滤波器阶数、通带纹波和阻带纹波": "Define low/high cutoff, order, passband and stopband ripple",
        "更改滤波器的低截止频率、高截止频率、通带纹波和阻带纹波": "Change filter low/high cutoff, passband and stopband ripple",
        "定义截止频率、滤波器阶数、通带纹波和阻带纹波": "Define cutoff frequency, order, passband and stopband ripple",
        "更改滤波器的截止频率、通带纹波和阻带纹波": "Change filter cutoff, passband and stopband ripple",
        "定义滤波器阶数、通带纹波和阻带纹波": "Define filter order, passband and stopband ripple",
        "计算椭圆滤波器的极点": "Compute elliptic filter poles",
        "计算椭圆滤波器的零点": "Compute elliptic filter zeros",
        "定义滤波器阶数、截止频率和分数延迟": "Define filter order, cutoff frequency, and fractional delay",
        "设计低通分数延迟 FIR 滤波器": "Design low-pass fractional-delay FIR filter",
        "输出滤波器系数": "Print filter coefficients",
        "定义 FFT 大小、采样率和频率元组": "Define FFT size, sample rate, and frequency tuples",
        "生成三角形滤波器组权重": "Generate triangular filter-bank weights",
        "输出滤波器组权重": "Print filter-bank weights",
        "定义滤波器阶数、频率、期望响应和权重": "Define filter order, frequencies, desired response, and weights",
        "设计滤波器并获取滤波器系数": "Design filter and get coefficients",
        "输出其他属性": "Print other properties",
        "定义扭曲因子和频率范围": "Define warp factor and frequency range",
        "扭曲频率并输出结果": "Warp frequency and print result",
        "输出当前截止频率": "Print current cutoff frequency",
        "进行多相抽取": "Perform polyphase decimation",
        "输出抽取后的信号样本": "Print decimated signal samples",
        "进行多相插值": "Perform polyphase interpolation",
        "输出插值后的信号样本": "Print interpolated signal samples",
        "更改滤波器系数": "Change filter coefficients",
        "定义滤波器阶数和分数延迟": "Define filter order and fractional delay",
        "创建一些示例滤波器": "Create example filters",
        "创建 FilterChain 实例并添加滤波器": "Create FilterChain and add filters",
        "重置滤波器链": "Reset filter chain",
        "定义滤波器阶数、步长因子、归一化因子和泄漏因子": "Define filter order, step size, normalization factor, and leakage factor",
        "定义滤波器阶数、遗忘因子和初始化逆相关矩阵的值": "Define filter order, forgetting factor, and initial inverse correlation matrix value",
        "定义滤波器阶数、步长因子数组和泄漏因子": "Define filter order, step-size array, and leakage factor",
    }
    return m.get(body, body)


def translate_line(line: str) -> str:
  # Comments
    m = re.match(r"^(\s*// )(.+)$", line)
    if m:
        return m.group(1) + t_comment(m.group(2))

    # Builder class with extra sentence
    m = re.match(
        r"^(.+?) 是一个用于生成(.+?)的类，继承了 SignalBuilder 类。信号的形式为 (.+)$", line
    )
    if m:
        sig = SIGNAL_TYPES.get(m.group(2), m.group(2))
        return f"{m.group(1)} generates {sig}; inherits `SignalBuilder`. Signal form: {m.group(3)}"

    m = re.match(r"^(.+?) 是一个用于生成(.+?)的类，继承了 SignalBuilder 类。使用 (.+?) 生成(.+?)。$", line)
    if m:
        sig = SIGNAL_TYPES.get(m.group(2), m.group(2))
        gen4 = SIGNAL_TYPES.get(m.group(4), m.group(4))
        return f"{m.group(1)} generates {sig}; inherits `SignalBuilder`. Uses {m.group(3)} to generate {gen4}."

    m = re.match(r"^(.+?) 是一个用于生成(.+?)的类，继承了 SignalBuilder 类。$", line)
    if m:
        sig = SIGNAL_TYPES.get(m.group(2), m.group(2))
        return f"{m.group(1)} generates {sig}; inherits `SignalBuilder`."

    m = re.match(r"^(.+?) 是一个用于实现(.+?)滤波器的类。$", line)
    if m:
        kind = FILTER_IMPL.get(m.group(2), m.group(2))
        return f"{m.group(1)} implements a {kind} filter."

    m = re.match(r"^(.+?) 是一个用于实现(.+?)的类。$", line)
    if m:
        impl = {
            "最小均方（Least-Mean-Squares）自适应滤波": "least-mean-squares (LMS) adaptive filtering",
            "归一化最小均方四次（Normalized Least-Mean-Fourth）自适应滤波": "normalized least-mean-fourth (NLMF) adaptive filtering",
            "归一化最小均方（Normalized Least-Mean-Squares）自适应滤波": "normalized least-mean-squares (NLMS) adaptive filtering",
            "递归最小二乘（Recursive-Least-Squares）自适应滤波": "recursive least-squares (RLS) adaptive filtering",
            "符号最小均方（Sign Least-Mean-Squares）自适应滤波": "sign least-mean-squares adaptive filtering",
            "具有可变步长的最小均方（Least-Mean-Squares）自适应滤波": "LMS adaptive filtering with variable step size",
            "有限脉冲响应（Finite Impulse Response, FIR）": "finite impulse response (FIR) filtering",
            "无限脉冲响应（Infinite Impulse Response, IIR）": "infinite impulse response (IIR) filtering",
            "顺序连接滤波器链": "serial filter chain",
            "反馈梳状": "feedback comb filtering",
            "前馈梳状": "feedforward comb filtering",
            "直流偏移去除": "DC offset removal",
            "去加重": "de-emphasis",
            "预加重 FIR": "pre-emphasis FIR filtering",
            "非递归移动平均": "non-recursive moving average filtering",
            "快速递归移动平均": "fast recursive moving average filtering",
            "N 阶 Thiran 全通插值": "N-th order Thiran all-pass interpolation",
            "维纳": "Wiener filtering",
            "中值": "median filtering",
            "Savitzky-Golay": "Savitzky–Golay filtering",
            "RASTA 滤波器（用于鲁棒语音处理）": "RASTA filtering (robust speech processing)",
            "单极高通": "single-pole high-pass filtering",
            "单极低通": "single-pole low-pass filtering",
        }.get(m.group(2), m.group(2))
        return f"{m.group(1)} implements {impl}."

    m = re.match(r"^(.+?) 是一个用于生成(.+?)滤波器原型的静态类。$", line)
    if m:
        kind = FILTER_IMPL.get(m.group(2), m.group(2))
        return f"{m.group(1)} is a static class that generates {kind} filter prototypes."

    m = re.match(r"^(.+?) 是一个用于(.+?)的类。$", line)
    if m:
        use = {
            "信号重采样（采样率转换）": "signal resampling (sample-rate conversion)",
            "谱减法滤波": "spectral subtraction filtering",
            "波形整形": "waveshaping",
            "处理交错立体声缓冲区数据": "processing interleaved stereo buffer data",
            "表示线性时不变（LTI）滤波器传递函数": "representing an LTI filter transfer function",
            "实现类似于 Kaldi 实现的声道长度归一化 (VTLN)": "vocal-tract length normalization (VTLN) similar to Kaldi",
        }.get(m.group(2), m.group(2))
        return f"{m.group(1)} is used for {use}."

    m = re.match(r"^(.+?) 是一个(.+?)类。$", line)
    if m:
        return f"{m.group(1)} is a {m.group(2)} class."

    m = re.match(r"^(.+?) 枚举定义了(.+?)。$", line)
    if m:
        return f"{m.group(1)} enum defines {m.group(2)}."

    m = re.match(r"^(.+?) 提供了(.+?)。$", line)
    if m:
        return f"{m.group(1)} provides {m.group(2)}."

    m = re.match(r"^(.+?) 是一个基于状态向量实现的 LTI 滤波器的特殊实现。(.+)$", line)
    if m:
        return f"{m.group(1)} is a state-vector-based LTI filter implementation. {m.group(2)}"

    m = re.match(r"^(.+?) 是一个用于实现维纳滤波器的类。其实现与 `scipy.signal.wiener\(\)` 相同。$", line)
    if m:
        return f"{m.group(1)} implements a Wiener filter; same as `scipy.signal.wiener()`."

    m = re.match(r"^(.+?) 是一个用于实现中值滤波器的类。它的实现比 `MedianFilter` 类稍快，但仅适用于较小的滤波器尺寸（不超过 5 左右）。$", line)
    if m:
        return (
            f"{m.group(1)} implements median filtering; faster than `MedianFilter` "
            "but only for small sizes (up to about 5)."
        )

    m = re.match(r"^(.+?) 是一个基于 Remez \(Parks-McClellan\) 算法的最优等波纹滤波器设计器。$", line)
    if m:
        return f"{m.group(1)} is an equiripple filter designer based on the Remez (Parks–McClellan) algorithm."

    m = re.match(r"^`(.+?)` 是一个单极 IIR 滤波器类。$", line)
    if m:
        return f"`{m.group(1)}` is a single-pole IIR filter class."

    m = re.match(r"^(.+?) 是一个多相滤波器系统类。$", line)
    if m:
        return f"{m.group(1)} is a polyphase filter system."

    m = re.match(r"^以下是一个使用 (.+?) 类中(.+?)，并在示例中加入了注释：$", line)
    if m:
        return f"The following example uses `{m.group(1)}` ({m.group(2)}) with comments:"

    # Param alias lines
    m = re.match(r'^- `"([^"]+)"[^:]*: (.+)$', line)
    if m:
        desc = {
            "脉冲持续时间，默认值为 0.05 秒": "pulse duration; default 0.05 s",
            "脉冲波周期，默认值为 0.1 秒": "pulse wave period; default 0.1 s",
            "初始相位，默认值为 0.0": "initial phase; default 0.0",
            "拉伸因子，默认值为 1.0": "stretch factor; default 1.0",
            "反馈系数，默认值为 1.0": "feedback coefficient; default 1.0",
            "概率，默认值为 0.5": "probability; default 0.5",
            "带宽比例，默认值为 1.25": "bandwidth scale; default 1.25",
            "缩放比例，默认值为 0.02": "scale factor; default 0.02",
            "步长，默认值为 1": "step; default 1",
        }.get(m.group(2).rstrip("。"), m.group(2))
        return f'- `"{m.group(1)}"`: {desc}.'

    # Bullet returns with 新的
    if line == "  - 返回值: 新的 `ComplexDiscreteSignal`。":
        return "  - Returns: new `ComplexDiscreteSignal`."
    if line == "  - 返回值: 新的 `ComplexDiscreteSignal<T>`。":
        return "  - Returns: new `ComplexDiscreteSignal<T>`."

    if line == "    - `samples`: 样本集合。":
        return "    - `samples`: Sample set."

    if line == "### 私有方法":
        return "### Private methods"
    if line == "### 示例":
        return "### Example"

    # Load from existing good overrides in manual file
    return ""


def main() -> int:
    still = json.loads(STILL.read_text(encoding="utf-8"))
    existing = {}
    if (ROOT / "tools" / "sp_manual_overrides.json").exists():
        existing = json.loads((ROOT / "tools" / "sp_manual_overrides.json").read_text(encoding="utf-8"))

    # Also load pro translate results for lines already clean
    explicit: dict[str, str] = {}
    for zh in still:
        if zh in existing and not re.search(r"[\u4e00-\u9fff]", existing[zh]):
            continue
        tr = translate_line(zh)
        if tr and not re.search(r"[\u4e00-\u9fff]", tr):
            explicit[zh] = tr

    # Load any previously saved part2 and merge
    if OUT.exists():
        explicit.update(json.loads(OUT.read_text(encoding="utf-8")))

    OUT.write_text(json.dumps(explicit, ensure_ascii=False, indent=2), encoding="utf-8")
    missing = [z for z in still if z not in existing and z not in explicit]
    missing2 = [z for z in still if z in explicit and re.search(r"[\u4e00-\u9fff]", explicit[z])]
    print(f"Explicit: {len(explicit)}, still missing: {len(missing)}, bad explicit: {len(missing2)}")
    if missing:
        (ROOT / "tools" / "sp_still_missing.json").write_text(
            json.dumps(missing, ensure_ascii=False, indent=2), encoding="utf-8"
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
