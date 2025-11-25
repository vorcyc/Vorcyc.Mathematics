using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Numerics;
using System.Numerics;

namespace Vorcyc.Mathematics.Extensions.FFTW;

/// <summary>
/// 针对 <see cref="PinnableArray{T}"/> 的信号/频谱相关扩展方法集合。
/// </summary>
/// <remarks>
/// 注意：下面的 <c>extension(PinnableArray{T} array)</c> 语法为自定义扩展形式（例如源码生成器），
/// 其内部定义的方法被视为针对特定类型的“扩展”。实现未做线程安全保证；调用者需自行确保并发安全。
/// </remarks>
public static class PinnableArraySignalExtension
{
    extension(PinnableArray<float> array)
    {
        /// <summary>
        /// 对数组中每个元素执行就地缩放：<c>x[i] = x[i] / N</c>，其中 <c>N = Length</c>。
        /// </summary>
        /// <remarks>
        /// 典型用途：对未归一化的逆 FFT/逆 DFT 输出进行幅度补偿（很多库的 Forward 与 Inverse 都不做归一化）。
        /// 行为说明：
        /// <list type="bullet">
        /// <item>不会保证元素之和等于 1。缩放后数组元素之和为原始和除以 <c>N</c>。</item>
        /// <item>若 <c>N == 0</c> 会产生除零错误（当前实现未做防护），调用前需确保长度 &gt; 0。</item>
        /// <item>若前向/逆向变换之一已做 <c>1/N</c> 归一化，则不应再次调用。</item>
        /// </list>
        /// </remarks>
        public void ScaleInPlace()
        {
            var factor = 1.0f / array.Length;
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= factor;
            }
        }
    }

    extension(PinnableArray<double> array)
    {
        /// <summary>
        /// 对数组中每个元素执行就地缩放：<c>x[i] = x[i] / N</c>，其中 <c>N = Length</c>。
        /// </summary>
        /// <remarks>
        /// 用途与 <see cref="PinnableArraySignalExtension.ScaleInPlace()"/>（float 版本）一致，适用于双精度。
        /// 注意事项同上：不保证元素和为 1；长度为 0 会导致除零；已归一化的数据不要重复缩放。
        /// </remarks>
        public void ScaleInPlace()
        {
            var factor = 1.0 / array.Length;
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= factor;
            }
        }
    }

    extension(PinnableArray<ComplexFp32> array)
    {
        /// <summary>
        /// 对复数数组进行幅度补偿：<c>X[i] = X[i] / N</c>（常用于逆 FFT/IDFT 未归一化输出）。
        /// </summary>
        /// <remarks>
        /// 不保证能量或元素和做特殊归一化，仅线性缩小到原始的 <c>1/N</c>。
        /// 若 <c>N == 0</c> 需在外层避免调用。
        /// </remarks>
        public void ScaleInPlace()
        {
            var factor = 1.0f / array.Length;
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= factor;
            }
        }

        /// <summary>
        /// 生成单边幅度谱（跳过 DC 分量 <c>k = 0</c>），返回频率与幅度的元组数组。
        /// </summary>
        /// <param name="sampleRate">采样率（Hz）。</param>
        /// <param name="inDb">是否返回 dB 幅度（使用 <c>20·log10</c>）。</param>
        /// <returns>
        /// 长度为 <c>N/2</c>（偶数）或 <c>(N+1)/2</c>（奇数）的数组。
        /// 频率：<c>f[k] = k · sampleRate / N</c>（<c>k = 1..len</c>）。
        /// 幅度：<c>A[k] = (2/N) · |X[k]|</c>；若 <paramref name="inDb"/> 为 <c>true</c>，返回 <c>20·log10(A[k])</c>。
        /// </returns>
        /// <remarks>
        /// 适用于由实信号产生的共轭对称谱。
        /// 偶数长度时 Nyquist 点（<c>k = N/2</c>）也被加倍；严格单边谱需自行对该点再除以 2。
        /// 为防止 <c>log(0)</c> 使用极小偏移 <c>1e-20</c>。
        /// </remarks>
        /// <exception cref="ArgumentException">当长度 &lt; 2 时抛出。</exception>
        public (float Frequency, float Amplitude)[] GetSingleSidedSpectrum(
            float sampleRate,
            bool inDb = false)
        {
            int N = array.Length;
            if (N < 2) throw new ArgumentException("FFT length too small");

            int len = N / 2 + (N % 2);
            var spectrum = new (float Frequency, float Amplitude)[len];

            float df = sampleRate / N;
            float scale = 2.0f / N;

            for (int k = 1; k <= len; k++)
            {
                float mag = array[k].Magnitude * scale;
                float amplitude = inDb ? 20.0f * MathF.Log10(mag + 1e-20f) : mag;
                spectrum[k - 1] = (k * df, amplitude);
            }
            return spectrum;
        }

        /// <summary>
        /// 获取单边复谱的只读视图（跳过 DC，保留相位与复数结构）。
        /// </summary>
        /// <returns>
        /// 指向原始频谱的 <see cref="ReadOnlySpan{T}"/>，长度 <c>N/2</c>（偶数）或 <c>(N+1)/2</c>（奇数），对应索引 <c>1..len</c>。
        /// </returns>
        /// <remarks>
        /// 不复制数据；修改底层数组会反映到该视图。包含 Nyquist（若存在），适用于滤波、频域卷积、相位分析。
        /// </remarks>
        /// <exception cref="ArgumentException">当长度 &lt; 2 时抛出。</exception>
        public ReadOnlySpan<ComplexFp32> GetSingleSided()
        {
            int N = array.Length;
            if (N < 2) throw new ArgumentException("FFT length too small");
            int len = N / 2 + (N % 2);
            return array.AsSpan(1, len);
        }

        /// <summary>
        /// 生成一个新的单边复谱数组，幅度按单边规范（除 DC 外乘以 <c>2/N</c>）。
        /// </summary>
        /// <returns>
        /// 新分配的 <see cref="PinnableArray{T}"/>，长度 <c>N/2</c> 或 <c>(N+1)/2</c>。
        /// 元素：<c>Y[k-1] = X[k] · (2/N)</c>，<c>k = 1..len</c>。
        /// </returns>
        /// <remarks>
        /// 用于绘制幅度谱、能量/功率分析、峰值检测等。偶数长度 Nyquist 同样加倍，严格需求需自行调整。
        /// </remarks>
        public PinnableArray<ComplexFp32> GetSingleSidedScaled()
        {
            int N = array.Length;
            int len = N / 2 + (N % 2);

            var singleSided = new PinnableArray<ComplexFp32>(len);
            float scale = 2.0f / N;
            for (int k = 1; k <= len; k++)
            {
                singleSided[k - 1] = array[k] * scale;
            }
            return singleSided;
        }
    }

    extension(PinnableArray<Complex> array)
    {
        /// <summary>
        /// 对双精度复数数组执行就地缩放：<c>X[i] = X[i] / N</c>（逆变换幅度补偿）。
        /// </summary>
        /// <remarks>
        /// 行为与单精度版本一致：不保证元素和为 1；长度为 0 时会触发除零（需外部保护）。
        /// </remarks>
        public void ScaleInPlace()
        {
            var factor = 1.0 / array.Length;
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= factor;
            }
        }

        /// <summary>
        /// 生成单边幅度谱（跳过 DC），返回频率与幅度。
        /// </summary>
        /// <param name="sampleRate">采样率（Hz）。</param>
        /// <param name="inDb">是否使用 dB 输出。</param>
        /// <returns>
        /// 长度 <c>N/2</c> 或 <c>(N+1)/2</c> 的数组；频率 <c>f[k] = k·sampleRate/N</c>；幅度 <c>(2/N)|X[k]|</c> 或其 dB。
        /// </returns>
        /// <remarks>
        /// 实信号单边谱假设共轭对称。Nyquist（偶数）被加倍，严格需求需单独处理。
        /// dB 计算使用偏移 <c>1e-20</c> 防止 <c>log(0)</c>。
        /// </remarks>
        /// <exception cref="ArgumentException">当长度 &lt; 2 时抛出。</exception>
        public (double Frequency, double Amplitude)[] GetSingleSidedSpectrum(
            double sampleRate,
            bool inDb = false)
        {
            int N = array.Length;
            if (N < 2) throw new ArgumentException("FFT length too small");

            int len = N / 2 + (N % 2);
            var spectrum = new (double Frequency, double Amplitude)[len];

            double df = sampleRate / N;
            double scale = 2.0 / N;

            for (int k = 1; k <= len; k++)
            {
                double mag = array[k].Magnitude * scale;
                double amplitude = inDb ? 20.0 * Math.Log10(mag + 1e-20) : mag;
                spectrum[k - 1] = (k * df, amplitude);
            }
            return spectrum;
        }

        /// <summary>
        /// 获取单边复谱只读视图（跳过 DC，含 Nyquist）。
        /// </summary>
        /// <returns>
        /// <see cref="ReadOnlySpan{T}"/> 指向原数组片段，长度 <c>N/2</c> 或 <c>(N+1)/2</c>。
        /// </returns>
        /// <remarks>
        /// 不复制数据；适用于需保留相位的频域操作（滤波/卷积/相位分析等）。
        /// </remarks>
        /// <exception cref="ArgumentException">当长度 &lt; 2 时抛出。</exception>
        public ReadOnlySpan<Complex> GetSingleSided()
        {
            int N = array.Length;
            if (N < 2) throw new ArgumentException("FFT length too small");
            int len = N / 2 + (N % 2);
            return array.AsSpan(1, len);
        }

        /// <summary>
        /// 生成单边复谱的能量校准副本（除 DC 外乘以 <c>2/N</c>）。
        /// </summary>
        /// <returns>
        /// 新的单边复谱数组：<c>Y[k-1] = X[k] · (2/N)</c>，<c>k = 1..len</c>。
        /// </returns>
        /// <remarks>
        /// 用于幅度/功率绘图、峰值与 SNR 评估等。偶数长度 Nyquist 同样加倍，可自行再行修正。
        /// </remarks>
        public PinnableArray<Complex> GetSingleSidedScaled()
        {
            int N = array.Length;
            int len = N / 2 + (N % 2);

            var singleSided = new PinnableArray<Complex>(len);
            double scale = 2.0 / N;
            for (int k = 1; k <= len; k++)
            {
                singleSided[k - 1] = array[k] * scale;
            }
            return singleSided;
        }
    }
}