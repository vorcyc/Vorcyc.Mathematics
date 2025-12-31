/* duan linli aka cyclone_dll
 * 19.11.5  , 25.1.8 基于Span<T> 重构
 * VORCYC CO,.LTD
 */

//tex:
//Formula 1: $$(a+b)^2 = a^2 + 2ab + b^2$$
//Formula 2: $$a^2-b^2 = (a+b)(a-b)$$

//! 8.0f * Atan(1.0f)  = 2* Constants.M_PI

namespace Vorcyc.Mathematics.SignalProcessing.Windowing;

using System.Buffers;
using System.Numerics;
using static System.MathF;
using static Vorcyc.Mathematics.TrigonometryHelper;
using static Vorcyc.Mathematics.VMath;


public static partial class WindowApplier
{
    /// <summary>
    /// 8192×4=32KB，安全栈上限
    /// </summary>
    const int STACK_THRESHOLD = 8192;


    #region SIMD Helper

    /// <summary>
    /// 通用 SIMD 权重应用函数：values[i] *= weights[i]（向量化主循环 + 尾部标量）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ApplyWeights_SIMD(Span<float> values, ReadOnlySpan<float> weights)
    {
        int n = values.Length;
        if (n <= 1)
            return;

        int vecSize = Vector<float>.Count;
        int i = 0;

        for (; i <= n - vecSize; i += vecSize)
        {
            var vVals = new Vector<float>(values.Slice(i, vecSize));
            var vW = new Vector<float>(weights.Slice(i, vecSize));
            (vVals * vW).CopyTo(values.Slice(i, vecSize));
        }

        for (; i < n; i++)
            values[i] *= weights[i];
    }

    /// <summary>
    /// 使用 recurrence 关系生成 cos(k*theta) 序列：cos(0), cos(theta), cos(2theta), ...
    /// 比逐个 Vector.Cos 快很多，尤其在 .NET 8/9/10 上
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GenerateCosSequence(Span<float> cosOut, float theta)
    {
        int n = cosOut.Length;
        if (n == 0) return;

        float cosTheta = MathF.Cos(theta);
        float sinTheta = MathF.Sin(theta);

        float c = 1f;  // cos(0)
        float s = 0f;  // sin(0)

        for (int i = 0; i < n; i++)
        {
            cosOut[i] = c;

            float cn = c * cosTheta - s * sinTheta;
            float sn = s * cosTheta + c * sinTheta;
            c = cn;
            s = sn;
        }
    }

    /// <summary>
    /// 生成多谐波 cos 序列：cos(kθ), cos(2kθ), cos(3kθ), cos(4kθ)
    /// 使用 recurrence 生成基础 cos(kθ)，再用 Chebyshev 多项式计算高次谐波（更快、更精确）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GenerateCosMultiHarmonics(
        Span<float> cos1, Span<float> cos2, Span<float> cos3, Span<float> cos4,
        float theta)
    {
        int n = cos1.Length;
        if (n == 0) return;

        GenerateCosSequence(cos1, theta);  // 先生成 cos1 = cos(kθ)

        for (int i = 0; i < n; i++)
        {
            float c = cos1[i];
            cos2[i] = 2f * c * c - 1f;                    // cos(2x)
            cos3[i] = 4f * c * c * c - 3f * c;            // cos(3x)
            cos4[i] = 8f * c * c * c * c - 8f * c * c + 1f; // cos(4x)
        }
    }

    #endregion


    #region Rectangular

    //tex:$$ w(n) = 1 $$

    /// <summary>
    /// 计算矩形窗函数。
    /// </summary>
    /// <param name="values">输入数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Rectangular(Span<float> values)
    { }

    #endregion


    #region Triangular

    //tex:$$ w(n) = 1 - \left| \frac{n - (N-1)/2}{(N-1)/2} \right| $$



    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static void Triangular(Span<float> values)
    //{
    //    float factor = 2.0f / (values.Length - 1);
    //    for (int i = 0; i < (values.Length - 1) / 2; i++)
    //    {
    //        float tri = factor * i;
    //        values[i] *= tri;
    //    }
    //    for (int i = 0; i < values.Length; i++)
    //    {
    //        float tri = 2.0f - (factor * i);
    //        values[i] *= tri;
    //    }
    //}

    /// <summary>
    /// Applies a triangular window function to the elements of the specified span in place.
    /// </summary>
    /// <remarks>The triangular window is commonly used in signal processing to reduce spectral leakage when
    /// analyzing finite-length signals. The operation modifies the input span directly; no new array is created. If the
    /// span contains one or zero elements, no changes are made.</remarks>
    /// <param name="values">The span of single-precision floating-point values to which the triangular window will be applied. Each element
    /// is multiplied by its corresponding window coefficient.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Triangular(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = 2.0f / (n - 1);
        int mid = (n - 1) >> 1;

        // 左半段（含中点）：w(i) = factor * i
        for (int i = 0; i <= mid; i++)
        {
            values[i] *= factor * i;
        }

        // 右半段：w(i) = 2 - factor * i
        for (int i = mid + 1; i < n; i++)
        {
            values[i] *= 2.0f - factor * i;
        }
    }

    /// <summary>
    /// Applies a symmetric triangular window to the specified span of single-precision floating-point values in place.
    /// </summary>
    /// <remarks>The triangular window tapers the values at both ends of the span, emphasizing the center.
    /// This method uses SIMD acceleration when possible for improved performance. The operation is performed in place;
    /// the input span is modified directly.</remarks>
    /// <param name="values">The span of values to which the triangular window will be applied. The window is applied in place, modifying the
    /// contents of the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Triangular_SIMD(Span<float> values)
    {
        int n = values.Length;

        if (n <= 1) return;


        float inv = 2f / (n - 1);

        var vInv = new Vector<float>(inv);
        var vTwo = new Vector<float>(2f);
        var vOne = new Vector<float>(1f);

        // 预生成 lane 偏移
        Span<float> lane = stackalloc float[Vector<float>.Count];
        for (int k = 0; k < lane.Length; k++) lane[k] = k;
        var vLane = new Vector<float>(lane);

        int i = 0;
        int vecEnd = n - (n % Vector<float>.Count);

        while (i < vecEnd)
        {
            var vIdx = new Vector<float>(i) + vLane;
            var vX = vIdx * vInv;                     // 2*n/(N-1)
            var vAbs = Vector.Abs(vX - vOne);         // |2*n/(N-1) - 1|
            var vW = vOne - vAbs;                     // 1 - |...|

            var vData = new Vector<float>(values.Slice(i));
            vData *= vW;
            vData.CopyTo(values.Slice(i));

            i += Vector<float>.Count;
        }

        // 尾巴
        for (; i < n; i++)
        {
            float x = i * inv;
            values[i] *= 1f - MathF.Abs(x - 1f);
        }
    }

    #endregion


    #region Hamming

    //tex:$$ w(n) = 0.54 - 0.46 \cos\left( \frac{2\pi n}{N-1} \right) $$


    /// <summary>
    /// Applies a Hamming window to the specified span of values in place.
    /// </summary>
    /// <remarks>The Hamming window is commonly used in digital signal processing to reduce spectral leakage
    /// when performing a Fourier transform. The operation multiplies each element in the span by the corresponding
    /// Hamming window coefficient. If the span has a length of 1 or less, the method returns without modifying the
    /// values.</remarks>
    /// <param name="values">The span of floating-point values to which the Hamming window will be applied. The window is applied in place,
    /// modifying the contents of the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hamming(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / (n - 1); // symmetric
        for (int i = 0; i < n; i++)
        {
            float w = 0.54f - 0.46f * Cos(factor * i);
            values[i] *= w;
        }
    }

    /// <summary>
    /// Applies the Hamming window function to the specified span of single-precision floating-point values in place
    /// using SIMD acceleration.
    /// </summary>
    /// <remarks>The Hamming window is commonly used in signal processing to reduce spectral leakage when
    /// performing a Fourier transform. This method modifies the input span directly; the original values will be
    /// replaced by their windowed counterparts. The operation is optimized using SIMD instructions for improved
    /// performance on supported hardware.</remarks>
    /// <param name="values">The span of values to which the Hamming window will be applied. The window is applied in place, modifying the
    /// contents of the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hamming_SIMD(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float theta = ConstantsFp32.TWO_PI / (n - 1);

        Span<float> weights = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];  // 小 n 时栈分配，性能极好
        Span<float> cos = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];

        GenerateCosSequence(cos, theta);

        for (int i = 0; i < n; i++)
            weights[i] = 0.54f - 0.46f * cos[i];

        ApplyWeights_SIMD(values, weights);
    }




    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static void Hamming2(Span<float> values)
    //{
    //    float factor = ConstantsFp32.TWO_PI / (values.Length - 1);
    //    //ref float r = ref MemoryMarshal.GetReference(values);
    //    ref float r = ref values[0];
    //    for (int n = 0; n < values.Length; n++)
    //    {
    //        float ham = 0.54f - 0.46f * Cos(factor * n);
    //        Unsafe.Add(ref r, n) *= ham;
    //    }
    //}

    /// <summary>
    /// Applies an in-place periodic Hamming window to the specified span of single-precision floating-point values.
    /// </summary>
    /// <remarks>This method modifies the contents of the <paramref name="values"/> span directly. The
    /// periodic Hamming window is commonly used in digital signal processing to reduce spectral leakage when performing
    /// a discrete Fourier transform (DFT) on periodic signals. The window coefficients are computed using the periodic
    /// form, which is suitable for signals that are assumed to repeat seamlessly.</remarks>
    /// <param name="values">The span of values to which the periodic Hamming window is applied. Each element is multiplied by the
    /// corresponding window coefficient.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hamming_Periodic(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / n; // periodic
        for (int i = 0; i < n; i++)
        {
            float w = 0.54f - 0.46f * Cos(factor * i);
            values[i] *= w;
        }
    }

    /// <summary>
    /// Applies a periodic Hamming window to the specified span of single-precision floating-point values in place using
    /// SIMD acceleration where possible.
    /// </summary>
    /// <remarks>The periodic Hamming window is commonly used in signal processing to reduce spectral leakage
    /// when performing Fourier transforms. This method modifies the input span directly and does not allocate additional
    /// memory for the result. The operation is optimized using SIMD instructions when supported by the
    /// hardware.</remarks>
    /// <param name="values">The span of values to which the periodic Hamming window will be applied. The window is applied in place,
    /// modifying the contents of the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hamming_Periodic_SIMD(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / n; // periodic

        int vecWidth = Vector<float>.Count;
        var vFactor = new Vector<float>(factor);

        Span<float> laneSpan = stackalloc float[vecWidth];
        for (int k = 0; k < vecWidth; k++)
        {
            laneSpan[k] = k;
        }
        var vLane = new Vector<float>(laneSpan);

        Span<float> angle = stackalloc float[vecWidth];
        Span<float> weights = stackalloc float[vecWidth];

        int i = 0;
        int vecEnd = n - (n % vecWidth);
        while (i < vecEnd)
        {
            var vBase = new Vector<float>(i);
            var vIdx = vBase + vLane;
            var vArg = vIdx * vFactor;

            vArg.CopyTo(angle);
            for (int k = 0; k < vecWidth; k++)
            {
                weights[k] = 0.54f - 0.46f * Cos(angle[k]);
            }

            var vW = new Vector<float>(weights);
            var vVals = new Vector<float>(values.Slice(i, vecWidth));
            vVals *= vW;
            vVals.CopyTo(values.Slice(i, vecWidth));

            i += vecWidth;
        }

        for (; i < n; i++)
        {
            float w = 0.54f - 0.46f * Cos(factor * i);
            values[i] *= w;
        }
    }

    #endregion


    #region Blackman

    //tex:$$ w(n) = 0.42 - 0.5 \cos\left( \frac{2\pi n}{N-1} \right) + 0.08 \cos\left( \frac{4\pi n}{N-1} \right) $$

    /// <summary>
    /// Applies a Blackman window to the specified span of values in place.
    /// </summary>
    /// <remarks>The Blackman window is commonly used in signal processing to reduce spectral leakage when
    /// performing a Fourier transform. The operation multiplies each element in the span by the corresponding Blackman
    /// window coefficient. If the span has a length of 1 or less, no operation is performed.</remarks>
    /// <param name="values">The sequence of values to which the Blackman window will be applied. The window is applied in place, modifying
    /// each element of the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / (n - 1);
        for (int i = 0; i < n; i++)
        {
            float arg = factor * i;
            float w = 0.42f - 0.5f * Cos(arg) + 0.08f * Cos(2f * arg);
            values[i] *= w;
        }
    }


    /// <summary>
    /// Applies the Blackman window function to the specified span of single-precision floating-point values in place.
    /// </summary>
    /// <remarks>This method modifies the input span directly. The Blackman window is commonly used in signal
    /// processing to reduce spectral leakage when performing a Fourier transform. The operation is performed using SIMD
    /// acceleration when possible for improved performance.</remarks>
    /// <param name="values">The span of values to which the Blackman window will be applied. Each element will be multiplied by the
    /// corresponding Blackman window coefficient.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman_SIMD(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float theta = ConstantsFp32.TWO_PI / (n - 1);

        Span<float> weights = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];
        Span<float> cos1 = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];
        Span<float> cos2 = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];

        GenerateCosSequence(cos1, theta);

        for (int i = 0; i < n; i++)
        {
            float c1 = cos1[i];
            float c2 = 2f * c1 * c1 - 1f;
            weights[i] = 0.42f - 0.5f * c1 + 0.08f * c2;
        }

        ApplyWeights_SIMD(values, weights);
    }

    /// <summary>
    /// Applies a periodic Blackman window to the specified span of single-precision floating-point values in place.
    /// </summary>
    /// <remarks>The periodic Blackman window is commonly used in signal processing to reduce spectral leakage
    /// when performing Fourier transforms. This method modifies the input span directly; the original values will be
    /// replaced by their windowed counterparts. If the span is empty, the method performs no action.</remarks>
    /// <param name="values">The span of values to which the periodic Blackman window will be applied. The window is applied in place,
    /// modifying the contents of the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman_Periodic(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / n;
        for (int i = 0; i < n; i++)
        {
            float arg = factor * i;
            float w = 0.42f - 0.5f * Cos(arg) + 0.08f * Cos(2f * arg);
            values[i] *= w;
        }
    }


    /// <summary>
    /// Applies a periodic Blackman window to the specified span of single-precision floating-point values in place
    /// using SIMD acceleration where possible.
    /// </summary>
    /// <remarks>The periodic Blackman window is commonly used in signal processing to reduce spectral leakage
    /// when performing Fourier transforms. This method modifies the input span directly and does not allocate
    /// additional memory for the result. The operation is optimized using SIMD instructions when supported by the
    /// hardware.</remarks>
    /// <param name="values">The span of values to which the periodic Blackman window will be applied. The window is applied in place,
    /// modifying the contents of the span.</param>
    public static void Blackman_Periodic_SIMD(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float theta = ConstantsFp32.TWO_PI / n;

        Span<float> weights = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];
        Span<float> cos1 = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];
        Span<float> cos2 = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];

        GenerateCosSequence(cos1, theta);

        for (int i = 0; i < n; i++)
        {
            float c1 = cos1[i];
            float c2 = 2f * c1 * c1 - 1f;
            weights[i] = 0.42f - 0.5f * c1 + 0.08f * c2;
        }

        ApplyWeights_SIMD(values, weights);
    }

    #endregion


    #region Hann


    //tex:$$ w(n) = 0.5 \left( 1 - \cos\left( \frac{2\pi n}{N-1} \right) \right) $$



    /// <summary>
    /// Applies the Hann window function to the specified span of values in place.
    /// </summary>
    /// <remarks>The Hann window is commonly used in signal processing to reduce spectral leakage when
    /// performing a Fourier transform. This method modifies the input span directly; the original values are replaced
    /// with their windowed counterparts. If the span contains one or zero elements, no operation is
    /// performed.</remarks>
    /// <param name="values">The span of floating-point values to which the Hann window will be applied. Each element is multiplied by the
    /// corresponding Hann window coefficient.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hann(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / (n - 1); // symmetric
        for (int i = 0; i < n; i++)
        {
            float w = 0.5f * (1f - Cos(factor * i));
            values[i] *= w;
        }
    }

    /// <summary>
    /// Applies an in-place Hann window to the specified span of single-precision floating-point values using SIMD
    /// acceleration where possible.
    /// </summary>
    /// <remarks>The Hann window is commonly used in signal processing to reduce spectral leakage when
    /// performing a Fourier transform. This method modifies the input span directly. The operation is optimized for
    /// performance using SIMD instructions when available.</remarks>
    /// <param name="values">The span of values to which the Hann window will be applied. Each element is multiplied by the corresponding
    /// Hann window coefficient. The span must have a length of at least 2; otherwise, no operation is performed.</param>
    /// <summary>
    /// Applies an in-place Hann window using modern Vector.Cos SIMD (recommended for .NET 8+)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hann_SIMD(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float theta = ConstantsFp32.TWO_PI / (n - 1);
        var weights = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];
        var cos = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];

        GenerateCosSequence(cos, theta);

        for (int i = 0; i < n; i++)
            weights[i] = 0.5f * (1f - cos[i]);

        ApplyWeights_SIMD(values, weights);
    }

    /// <summary>
    /// Applies a periodic Hann window to the specified span of values in place.
    /// </summary>
    /// <remarks>The periodic Hann window is commonly used in signal processing to reduce spectral leakage
    /// when performing a discrete Fourier transform (DFT). This method modifies the input span directly; no new memory
    /// is allocated.</remarks>
    /// <param name="values">The span of single-precision floating-point values to which the periodic Hann window will be applied. The window
    /// is applied in place, modifying the contents of the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hann_Periodic(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / n; // periodic
        for (int i = 0; i < n; i++)
        {
            float w = 0.5f * (1f - Cos(factor * i));
            values[i] *= w;
        }
    }

    /// <summary>
    /// Applies an in-place periodic Hann window to the specified span of single-precision floating-point values using
    /// SIMD acceleration where possible.
    /// </summary>
    /// <remarks>The periodic Hann window is commonly used in signal processing to reduce spectral leakage
    /// when performing a discrete Fourier transform (DFT). This method modifies the input span directly and does not
    /// allocate additional memory beyond small stack allocations. The operation is optimized for performance using SIMD
    /// instructions when available.</remarks>
    /// <param name="values">The span of values to which the periodic Hann window will be applied. The window is applied in-place, modifying
    /// the contents of the span.</param>
    public static void Hann_Periodic_SIMD(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float theta = ConstantsFp32.TWO_PI / n;  // 注意：periodic 用 /n

        Span<float> weights = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];
        Span<float> cos = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];

        GenerateCosSequence(cos, theta);

        for (int i = 0; i < n; i++)
            weights[i] = 0.5f * (1f - cos[i]);

        ApplyWeights_SIMD(values, weights);
    }



    #endregion


    #region Gaussian


    //tex:$$ w(n) = \exp\left( -0.5 \left( \frac{n - (N-1)/2}{\sigma (N-1)/2} \right)^2 \right) $$

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static void Gaussian_old(Span<float> values)
    //{
    //    float factor = (values.Length - 1) * .5f;
    //    for (int i = 0; i < values.Length; i++)
    //    {
    //        float gaussian = Exp(-0.5f * Pow((i - factor) / (0.4f * factor), 2.0f));
    //        values[i] *= gaussian;
    //    }
    //}

    /// <summary>
    /// Applies a Gaussian window to the specified span of values in place.
    /// </summary>
    /// <remarks>This method modifies the input span directly. The Gaussian window is symmetric and uses a
    /// fixed standard deviation relative to the span length. If the span contains one or zero elements, no operation is
    /// performed.</remarks>
    /// <param name="values">The span of values to which the Gaussian window is applied. Each element is multiplied by the corresponding
    /// value of a normalized Gaussian function centered in the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Gaussian(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float mid = (n - 1) * 0.5f;
        float sigma = 0.4f;
        float denom = sigma * mid;

        for (int i = 0; i < n; i++)
        {
            float t = (i - mid) / denom;
            float g = Exp(-0.5f * t * t);
            values[i] *= g;
        }
    }

    /// <summary>
    /// Applies a Gaussian window to the specified span of single-precision floating-point values in place using SIMD
    /// acceleration.
    /// </summary>
    /// <remarks>The Gaussian window is centered over the input span and uses a fixed standard deviation. This
    /// method leverages SIMD instructions for improved performance on supported hardware. For spans with a length of
    /// one or less, the method performs no operation.</remarks>
    /// <param name="values">The span of values to which the Gaussian window is applied. The operation modifies the values in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Gaussian_SIMD(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float mid = (n - 1) * 0.5f;
        float sigma = 0.4f;
        float denom = sigma * mid;
        if (denom <= 0f) return;

        float invDenom = 1f / denom;

        int vecWidth = Vector<float>.Count;
        Span<float> lane = stackalloc float[vecWidth];
        for (int k = 0; k < vecWidth; k++) lane[k] = k;
        var vLane = new Vector<float>(lane);

        var vMid = new Vector<float>(mid);
        var vInvDenom = new Vector<float>(invDenom);

        Span<float> tBuf = stackalloc float[vecWidth];

        int i = 0;
        int vecEnd = n - (n % vecWidth);

        while (i < vecEnd)
        {
            var vIdx = new Vector<float>(i) + vLane;
            var vT = (vIdx - vMid) * vInvDenom;
            vT.CopyTo(tBuf);

            // 逐 lane 标量 exp + 直接写回（最快路径）
            for (int k = 0; k < vecWidth; k++)
            {
                float t = tBuf[k];
                float g = MathF.Exp(-0.5f * t * t);
                values[i + k] *= g;
            }

            i += vecWidth;
        }

        // 尾部标量
        for (; i < n; i++)
        {
            float t = (i - mid) * invDenom;
            values[i] *= MathF.Exp(-0.5f * t * t);
        }
    }

    /// <summary>
    /// Applies a Gaussian weighting to the elements of the specified span in place, using the provided standard
    /// deviation.
    /// </summary>
    /// <remarks>The weighting is centered on the midpoint of the span. If the span contains one or fewer
    /// elements, or if sigma is less than or equal to 0, the method performs no operation.</remarks>
    /// <param name="values">The span of single-precision floating-point values to be weighted. Each element is multiplied by the
    /// corresponding value of a normalized Gaussian curve.</param>
    /// <param name="sigma">The standard deviation of the Gaussian curve. Must be greater than 0.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Gaussian(Span<float> values, float sigma)
    {
        int n = values.Length;
        if (n <= 1 || sigma <= 0f) return;

        float mid = (n - 1) * 0.5f;
        float denom = sigma * mid;

        for (int i = 0; i < n; i++)
        {
            float t = (i - mid) / denom;
            float g = Exp(-0.5f * t * t);
            values[i] *= g;
        }
    }

    /// <summary>
    /// Applies a Gaussian weighting to each element in the specified span using the provided standard deviation.
    /// </summary>
    /// <remarks>This method modifies the input span directly, scaling each element by a Gaussian curve
    /// centered in the span. If the span contains one or fewer elements, or if sigma is less than or equal to zero, the
    /// method returns without modifying the data. The operation is optimized using SIMD instructions for improved
    /// performance on supported hardware.</remarks>
    /// <param name="values">The span of single-precision floating-point values to be modified in place. Each value is multiplied by a
    /// Gaussian weight based on its position.</param>
    /// <param name="sigma">The standard deviation to use for the Gaussian function. Must be greater than 0.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Gaussian_SIMD(Span<float> values, float sigma)
    {
        int n = values.Length;
        if (n <= 1 || sigma <= 0f) return;

        float mid = (n - 1) * 0.5f;
        float denom = sigma * mid;

        int vecWidth = Vector<float>.Count;

        // lanes: 0..(vecWidth-1)
        Span<float> laneSpan = stackalloc float[vecWidth];
        for (int k = 0; k < vecWidth; k++)
        {
            laneSpan[k] = k;
        }
        var vLane = new Vector<float>(laneSpan);

        var vMid = new Vector<float>(mid);
        var vDenom = new Vector<float>(denom);

        // 临时标量缓冲用于 Exp 逐 lane 计算
        Span<float> tBuf = stackalloc float[vecWidth];
        Span<float> wBuf = stackalloc float[vecWidth];

        int i = 0;
        int vecEnd = n - (n % vecWidth);
        while (i < vecEnd)
        {
            var vBase = new Vector<float>(i);
            var vIdx = vBase + vLane;

            // t = (i - mid) / denom
            var vT = (vIdx - vMid) / vDenom;

            // g = exp(-0.5 * t^2) 逐标量计算
            vT.CopyTo(tBuf);
            for (int lane = 0; lane < vecWidth; lane++)
            {
                float t = tBuf[lane];
                wBuf[lane] = Exp(-0.5f * t * t);
            }

            var vW = new Vector<float>(wBuf);
            var vVals = new Vector<float>(values.Slice(i, vecWidth));
            vVals *= vW;
            vVals.CopyTo(values.Slice(i, vecWidth));

            i += vecWidth;
        }

        // 尾部
        for (; i < n; i++)
        {
            float t = (i - mid) / denom;
            float g = Exp(-0.5f * t * t);
            values[i] *= g;
        }
    }


    #endregion


    #region Kaiser


    //tex:$$ w(n) = \frac{I_0\left( \alpha \sqrt{1 - \left( \frac{2n}{N-1} - 1 \right)^2} \right)}{I_0(\alpha)} $$


    /// <summary>
    /// Applies a Kaiser window to the specified sequence of values in place.
    /// </summary>
    /// <remarks>The Kaiser window is commonly used in digital signal processing to reduce spectral leakage when
    /// performing Fourier transforms. The operation multiplies each element in the span by the corresponding Kaiser window
    /// coefficient. The length of the span determines the window size. This method does not allocate additional memory and
    /// modifies the input span directly.</remarks>
    /// <param name="values">The sequence of values to which the Kaiser window is applied. The window is applied in place, modifying the contents
    /// of this span.</param>
    /// <param name="alpha">The shape parameter that determines the width of the Kaiser window. Higher values result in a window with less
    /// side-lobe energy. The default is 12.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Kaiser(Span<float> values, float alpha = 12f)
    {
        var n = values.Length;
        if (n <= 1) return;

        float factor = 2.0f / (n - 1);
        for (int i = 0; i < n; i++)
        {
            float kaiser = I0(alpha * Sqrt(1 - (i * factor - 1) * (i * factor - 1))) / I0(alpha);
            values[i] *= kaiser;
        }
    }

    /// <summary>
    /// Applies an in-place Kaiser window to the specified span of single-precision floating-point values using SIMD
    /// acceleration.
    /// </summary>
    /// <remarks>The Kaiser window is commonly used in digital signal processing to reduce spectral leakage.
    /// This method modifies the input span directly and does not allocate additional memory for the result. The
    /// operation is optimized using SIMD instructions for improved performance on supported hardware.</remarks>
    /// <param name="values">The span of single-precision floating-point values to which the Kaiser window will be applied. The values are
    /// modified in place.</param>
    /// <param name="alpha">The shape parameter of the Kaiser window. Higher values result in a window with less side-lobe energy. The
    /// default is 12.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Kaiser_SIMD(Span<float> values, float alpha = 12f)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = 2.0f / (n - 1);
        float i0Alpha = I0(alpha);

        int vecWidth = Vector<float>.Count;
        var vFactor = new Vector<float>(factor);
        var vOne = new Vector<float>(1.0f);

        // lanes: 0..(vecWidth-1)
        Span<float> laneSpan = stackalloc float[vecWidth];
        for (int k = 0; k < vecWidth; k++)
        {
            laneSpan[k] = k;
        }
        var vLane = new Vector<float>(laneSpan);

        // 临时缓冲用于逐 lane 计算 I0(...)
        Span<float> tBuf = stackalloc float[vecWidth];
        Span<float> wBuf = stackalloc float[vecWidth];

        int i = 0;
        int vecEnd = n - (n % vecWidth);
        while (i < vecEnd)
        {
            var vBase = new Vector<float>(i);
            var vIdx = vBase + vLane;

            // x = i*factor - 1
            var vX = vIdx * vFactor - vOne;

            // rad = sqrt(1 - x*x)
            var vRadSq = vOne - (vX * vX);

            // 拷到标量缓冲后逐 lane 计算 w = I0(alpha * sqrt(...)) / I0(alpha)
            vRadSq.CopyTo(tBuf);
            for (int k = 0; k < vecWidth; k++)
            {
                float radSq = tBuf[k];
                // 数值上 radSq 可能出现极小负数（浮点误差），钳制到 [0,1]
                if (radSq < 0f) radSq = 0f;
                if (radSq > 1f) radSq = 1f;

                float rad = Sqrt(radSq);
                float w = I0(alpha * rad) / i0Alpha;
                wBuf[k] = w;
            }

            var vW = new Vector<float>(wBuf);
            var vVals = new Vector<float>(values.Slice(i, vecWidth));
            vVals *= vW;
            vVals.CopyTo(values.Slice(i, vecWidth));

            i += vecWidth;
        }

        // 尾部
        for (; i < n; i++)
        {
            float x = i * factor - 1f;
            float radSq = 1f - x * x;
            if (radSq < 0f) radSq = 0f;
            if (radSq > 1f) radSq = 1f;

            float w = I0(alpha * Sqrt(radSq)) / i0Alpha;
            values[i] *= w;
        }
    }

    #endregion


    #region Kbd


    //tex:$$ w(n) = \sqrt{\frac{\sum_{k=0}^{n} I_0\left( \pi \alpha \sqrt{1 - \left( \frac{2k}{N} - 1 \right)^2} \right)}{\sum_{k=0}^{N/2} I_0\left( \pi \alpha \sqrt{1 - \left( \frac{2k}{N} - 1 \right)^2} \right)}} $$


    /// <summary>
    /// Applies a Kaiser-Bessel derived (KBD) window to the specified sequence of values in place.
    /// </summary>
    /// <remarks>The KBD window is commonly used in audio signal processing, particularly in applications such as the
    /// Modified Discrete Cosine Transform (MDCT). The length of the window is determined by the length of the input span.
    /// The operation modifies the input values directly; no new array is allocated for the result.</remarks>
    /// <param name="values">The sequence of values to which the KBD window will be applied. The window is applied in place, modifying the
    /// contents of this span.</param>
    /// <param name="alpha">The shape parameter for the KBD window. Higher values result in a window with stronger side lobe suppression. The
    /// default is 4.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Kbd(Span<float> values, float alpha = 4f)
    {
        var n = values.Length;
        if (n <= 1) return;

        var window = new float[n / 2 + 1];

        float factor = 4.0f / n;
        float sum = 0f;

        for (int i = 0; i <= n / 2; i++)
        {
            sum += I0(ConstantsFp32.PI * alpha * Sqrt(1 - (i * factor - 1) * (i * factor - 1)));
            window[i] = sum;
        }

        for (int i = 0; i < n / 2; i++)
        {
            var v = Sqrt(window[i] / sum);
            values[i] *= v;

            var backwardIndex = n - 1 - i;
            values[backwardIndex] *= v;
        }
    }


    /// <summary>
    /// Applies a Kaiser–Bessel-derived (KBD) window to the specified values using SIMD acceleration.
    /// </summary>
    /// <remarks>The method modifies the input span in place by scaling each element according to the computed
    /// KBD window. The operation is optimized using SIMD instructions for improved performance. The length of the input
    /// span determines the window size; if the span is empty, the method returns immediately. The method is suitable
    /// for digital signal processing scenarios such as audio or spectral analysis where KBD windowing is
    /// required.</remarks>
    /// <param name="values">The span of floating-point values to which the KBD window will be applied. The values are modified in place.</param>
    /// <param name="alpha">The alpha parameter that controls the shape of the KBD window. Higher values produce a window with stronger side
    /// lobe suppression. The default is 4.0.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Kbd_SIMD(Span<float> values, float alpha = 4f)
    {
        int n = values.Length;
        if (n <= 1) return;

        int half = n >> 1; // floor(n/2)
                           // 缓存半窗累计和，长度为 half+1（包含 k=half）
        var window = new float[half + 1];

        float factor = 4.0f / n;                  // 等于 2*(2/N)
        float scale = ConstantsFp32.PI * alpha;   // π α

        // SIMD 批量生成 radSq = 1 - (k*factor - 1)^2，然后逐 lane 计算 I0(scale * sqrt(radSq))
        int vecWidth = Vector<float>.Count;
        var vFactor = new Vector<float>(factor);
        var vOne = new Vector<float>(1.0f);
        var vScale = new Vector<float>(scale);

        Span<float> laneSpan = stackalloc float[vecWidth];
        for (int k = 0; k < vecWidth; k++)
        {
            laneSpan[k] = k;
        }
        var vLane = new Vector<float>(laneSpan);

        // 临时缓冲用于逐 lane 标量 I0 计算
        Span<float> tmp = stackalloc float[vecWidth];

        float sum = 0f;
        int i = 0;
        int vecEnd = (half + 1) - ((half + 1) % vecWidth);
        while (i < vecEnd)
        {
            var vBase = new Vector<float>(i);
            var vIdx = vBase + vLane;            // k = i + lane
            var vX = vIdx * vFactor - vOne;      // x = k*factor - 1
            var vRadSq = vOne - (vX * vX);       // 1 - x^2

            // 逐 lane：I0(scale * sqrt(radSq))
            vRadSq.CopyTo(tmp);
            for (int lane = 0; lane < vecWidth; lane++)
            {
                float radSq = tmp[lane];
                if (radSq < 0f) radSq = 0f;      // 浮点误差钳制
                if (radSq > 1f) radSq = 1f;

                float rad = Sqrt(radSq);
                float term = I0(scale * rad);
                sum += term;
                window[i + lane] = sum;
            }

            i += vecWidth;
        }

        // 尾部逐标量
        for (; i <= half; i++)
        {
            float x = i * factor - 1f;
            float radSq = 1f - x * x;
            if (radSq < 0f) radSq = 0f;
            if (radSq > 1f) radSq = 1f;

            float term = I0(scale * Sqrt(radSq));
            sum += term;
            window[i] = sum;
        }

        // 归一化并镜像到两端，平方根
        for (int k = 0; k < half; k++)
        {
            float w = Sqrt(window[k] / sum);
            values[k] *= w;

            int r = n - 1 - k;
            values[r] *= w;
        }

        // 当 n 为奇数时，中心元素单独处理（k == half）
        if ((n & 1) == 1)
        {
            float w = Sqrt(window[half] / sum);
            values[half] *= w;
        }
    }
    #endregion


    #region Bartlett_Hann


    //tex:$$ w(n) = 0.62 - 0.48 \left| \frac{n}{N-1} - 0.5 \right| - 0.38 \cos\left( \frac{2\pi n}{N-1} \right) $$

    /// <summary>
    /// Applies the Bartlett-Hann window function to the specified span of values in place.
    /// </summary>
    /// <remarks>The Bartlett-Hann window is commonly used in digital signal processing to reduce spectral leakage
    /// when performing a Fourier transform. This method modifies the input span directly; the original values are replaced
    /// with their windowed counterparts.</remarks>
    /// <param name="values">The sequence of values to which the Bartlett-Hann window will be applied. Each element is multiplied by the
    /// corresponding window coefficient. The span must have a length of at least 2.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bartlett_Hann(Span<float> values)
    {
        var n = values.Length;
        if (n <= 1) return;

        float factor = 1.0f / (n - 1);
        for (int i = 0; i < n; i++)
        {
            var bh = 0.62f - 0.48f * Abs(i * factor - 0.5f) - 0.38f * Cos(ConstantsFp32.TWO_PI * i * factor);
            values[i] *= bh;
        }
    }

    /// <summary>
    /// Applies the Bartlett-Hann window function to the specified span of single-precision floating-point values in place
    /// using SIMD acceleration where possible.
    /// </summary>
    /// <remarks>The Bartlett-Hann window is commonly used in digital signal processing to reduce spectral leakage
    /// when performing Fourier transforms. This method modifies the input span in place. If the span contains one or fewer
    /// elements, no operation is performed.</remarks>
    /// <param name="values">The span of values to which the Bartlett-Hann window will be applied. Each element will be multiplied by the
    /// corresponding Bartlett-Hann window coefficient.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bartlett_Hann_SIMD(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = 1.0f / (n - 1);
        float twoPi = ConstantsFp32.TWO_PI;

        int vecWidth = Vector<float>.Count;

        // lanes: 0..(vecWidth-1)
        Span<float> laneSpan = stackalloc float[vecWidth];
        for (int k = 0; k < vecWidth; k++)
        {
            laneSpan[k] = k;
        }
        var vLane = new Vector<float>(laneSpan);

        // 临时缓冲用于逐 lane 计算权重
        Span<float> idxBuf = stackalloc float[vecWidth];
        Span<float> wBuf = stackalloc float[vecWidth];

        int i = 0;
        int vecEnd = n - (n % vecWidth);
        while (i < vecEnd)
        {
            var vBase = new Vector<float>(i);
            var vIdx = vBase + vLane; // i + lane

            // 将索引拷到标量缓冲，逐 lane 计算权重：
            // w = 0.62 - 0.48 * |i*factor - 0.5| - 0.38 * cos(2π * i * factor)
            vIdx.CopyTo(idxBuf);
            for (int k = 0; k < vecWidth; k++)
            {
                float x = idxBuf[k] * factor;
                float w = 0.62f - 0.48f * Abs(x - 0.5f) - 0.38f * Cos(twoPi * x);
                wBuf[k] = w;
            }

            var vW = new Vector<float>(wBuf);
            var vVals = new Vector<float>(values.Slice(i, vecWidth));
            vVals *= vW;
            vVals.CopyTo(values.Slice(i, vecWidth));

            i += vecWidth;
        }

        // 尾部
        for (; i < n; i++)
        {
            float x = i * factor;
            float w = 0.62f - 0.48f * Abs(x - 0.5f) - 0.38f * Cos(twoPi * x);
            values[i] *= w;
        }
    }
    #endregion


    #region Lanczos


    //tex:$$ w(n) = \text{sinc}\left( \frac{2n}{N-1} - 1 \right) $$

    /// <summary>
    /// Applies the Lanczos window function to the specified span of values in place.
    /// </summary>
    /// <remarks>The Lanczos window is commonly used in signal processing to reduce spectral leakage when performing
    /// Fourier transforms. This method modifies the input span directly; the original values will be replaced with their
    /// windowed counterparts. The window is symmetric and spans the entire length of the input.</remarks>
    /// <param name="values">The span of floating-point values to which the Lanczos window will be applied. Each element will be multiplied by
    /// the corresponding Lanczos window coefficient.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Lanczos(Span<float> values)
    {
        var n = values.Length;
        if (n <= 1) return;

        float factor = 2.0f / (n - 1);
        for (int i = 0; i < n; i++)
        {
            var lanczos = Sinc(i * factor - 1);
            values[i] *= lanczos;
        }
    }

    /// <summary>
    /// Applies the Lanczos window function to the specified span of single-precision floating-point values in place
    /// using SIMD acceleration.
    /// </summary>
    /// <remarks>The Lanczos window is commonly used in signal processing to reduce spectral leakage. This
    /// method multiplies each element in the span by the corresponding Lanczos window coefficient, where the window is
    /// defined over the length of the span. The operation is performed in place and leverages SIMD instructions for
    /// improved performance on supported hardware. The input span must not be empty.</remarks>
    /// <param name="values">The span of values to which the Lanczos window will be applied. The operation modifies the values in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Lanczos_SIMD(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = 2.0f / (n - 1);

        int vecWidth = Vector<float>.Count;

        // lanes: 0..(vecWidth-1)
        Span<float> laneSpan = stackalloc float[vecWidth];
        for (int k = 0; k < vecWidth; k++)
        {
            laneSpan[k] = k;
        }
        var vLane = new Vector<float>(laneSpan);

        // 临时缓冲：x 参数与权重
        Span<float> xBuf = stackalloc float[vecWidth];
        Span<float> wBuf = stackalloc float[vecWidth];

        int i = 0;
        int vecEnd = n - (n % vecWidth);
        while (i < vecEnd)
        {
            var vBase = new Vector<float>(i);
            var vIdx = vBase + vLane;      // i + lane

            // x = i*factor - 1
            var vX = vIdx * new Vector<float>(factor) - new Vector<float>(1.0f);

            vX.CopyTo(xBuf);
            for (int k = 0; k < vecWidth; k++)
            {
                // w = sinc(x)
                wBuf[k] = Sinc(xBuf[k]);
            }

            var vW = new Vector<float>(wBuf);
            var vVals = new Vector<float>(values.Slice(i, vecWidth));
            vVals *= vW;
            vVals.CopyTo(values.Slice(i, vecWidth));

            i += vecWidth;
        }

        // 尾部元素
        for (; i < n; i++)
        {
            float x = i * factor - 1f;
            float w = Sinc(x);
            values[i] *= w;
        }
    }

    #endregion


    #region PowerOfSine


    //tex:$$ w(n) = \sin^\alpha\left( \frac{\pi n}{N} \right) $$


    /// <summary>
    /// Applies a power-of-sine window to the specified values in place.
    /// </summary>
    /// <remarks>This method modifies the contents of the <paramref name="values"/> span directly. The power-of-sine
    /// window is commonly used in digital signal processing to reduce spectral leakage. The window is defined as sin(pi * n
    /// / N) raised to the power of <paramref name="alpha"/>, where n is the index and N is the length of the
    /// span.</remarks>
    /// <param name="values">The span of values to be windowed. Each element is multiplied by the corresponding value of the power-of-sine
    /// window.</param>
    /// <param name="alpha">The exponent to which the sine function is raised. Must be greater than 0. The default value is 1.5.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PowerOfSine(Span<float> values, float alpha = 1.5f)
    {
        var n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.PI / n;
        for (int i = 0; i < n; i++)
        {
            var v = Pow(Sin(i * factor), alpha);
            values[i] *= v;
        }
    }

    /// <summary>
    /// Applies an in-place transformation to each element in the specified span by multiplying it by the sine of its
    /// normalized index raised to the specified power.
    /// </summary>
    /// <remarks>This method uses SIMD acceleration when possible for improved performance on large spans. The
    /// transformation for each element at index i is: value[i] *= MathF.Pow(MathF.Sin(i * π / N), alpha), where N is
    /// the length of the span. The method does not allocate additional memory beyond temporary stack storage.</remarks>
    /// <param name="values">The span of single-precision floating-point values to transform. Each element will be updated in place.</param>
    /// <param name="alpha">The exponent to which the sine of each normalized index is raised. The default value is 1.5.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PowerOfSine_SIMD(Span<float> values, float alpha = 1.5f)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.PI / n;

        int vecWidth = Vector<float>.Count;

        // lanes: 0..(vecWidth-1)
        Span<float> laneSpan = stackalloc float[vecWidth];
        for (int k = 0; k < vecWidth; k++)
        {
            laneSpan[k] = k;
        }
        var vLane = new Vector<float>(laneSpan);

        Span<float> angle = stackalloc float[vecWidth];
        Span<float> weights = stackalloc float[vecWidth];

        int i = 0;
        int vecEnd = n - (n % vecWidth);
        while (i < vecEnd)
        {
            var vBase = new Vector<float>(i);
            var vIdx = vBase + vLane;                 // i + lane
            var vArg = vIdx * new Vector<float>(factor);

            // 逐 lane: w = sin(arg)^alpha
            vArg.CopyTo(angle);
            for (int k = 0; k < vecWidth; k++)
            {
                float s = Sin(angle[k]);
                weights[k] = Pow(s, alpha);
            }

            var vW = new Vector<float>(weights);
            var vVals = new Vector<float>(values.Slice(i, vecWidth));
            vVals *= vW;
            vVals.CopyTo(values.Slice(i, vecWidth));

            i += vecWidth;
        }

        // 尾部
        for (; i < n; i++)
        {
            float w = Pow(Sin(i * factor), alpha);
            values[i] *= w;
        }
    }

    #endregion


    #region Flattop


    //tex:$$ w(n) = 0.216 - 0.417 \cos\left( \frac{2\pi n}{N-1} \right) + 0.278 \cos\left( \frac{4\pi n}{N-1} \right) - 0.084 \cos\left( \frac{6\pi n}{N-1} \right) + 0.007 \cos\left( \frac{8\pi n}{N-1} \right) $$


    /// <summary>
    /// Applies a Flattop window function to the specified span of values in place.
    /// </summary>
    /// <remarks>The Flattop window is commonly used in signal processing to reduce spectral leakage when performing
    /// a Fourier transform. This method multiplies each element of the span by the corresponding Flattop window
    /// coefficient. The operation is performed in place; the original values in the span are overwritten.</remarks>
    /// <param name="values">The span of floating-point values to which the Flattop window will be applied. The window is applied in place,
    /// modifying the contents of the span. The length of the span must be at least 1.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Flattop(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / (n - 1);

        for (int i = 0; i < n; i++)
        {
            var v = 0.216f - 0.417f * Cos(i * factor) + 0.278f * Cos(2 * i * factor) - 0.084f * Cos(3 * i * factor) + 0.007f * Cos(4 * i * factor);
            values[i] *= v;
        }
    }

    /// <summary>
    /// Applies a flat top window function to the specified span of single-precision floating-point values in place
    /// using SIMD acceleration where possible.
    /// </summary>
    /// <remarks>The flat top window is commonly used in signal processing to reduce spectral leakage when
    /// performing a Fourier transform. This method modifies the input span directly and does not allocate additional
    /// memory for the result. The operation is optimized using SIMD instructions when supported by the
    /// hardware.</remarks>
    /// <param name="values">The span of values to which the flat top window will be applied. The window is applied in place, modifying the
    /// contents of the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Flattop_SIMD(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float theta = ConstantsFp32.TWO_PI / (n - 1);

        Span<float> weights = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];
        Span<float> cos1 = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];
        Span<float> cos2 = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];
        Span<float> cos3 = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];
        Span<float> cos4 = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];

        GenerateCosMultiHarmonics(cos1, cos2, cos3, cos4, theta);

        for (int i = 0; i < n; i++)
        {
            weights[i] = 0.216f
                       - 0.417f * cos1[i]
                       + 0.278f * cos2[i]
                       - 0.084f * cos3[i]
                       + 0.007f * cos4[i];
        }

        ApplyWeights_SIMD(values, weights);
    }

    #endregion


    #region Liftering


    //tex:$$ w(n) = 1 + \frac{L}{2} \sin\left( \frac{\pi n}{L} \right) $$

    /// <summary>
    /// Applies sinusoidal liftering to the specified sequence of cepstral coefficients in place.
    /// </summary>
    /// <remarks>Liftering is commonly used in speech processing to emphasize or de-emphasize certain cepstral
    /// coefficients, which can improve recognition performance or analysis. If the span is empty or <paramref
    /// name="l"/> is less than or equal to 0, the method performs no operation.</remarks>
    /// <param name="values">The span of cepstral coefficient values to be modified. The liftering is applied in place to each element.</param>
    /// <param name="l">The liftering parameter that controls the strength and periodicity of the liftering effect. Must be greater than
    /// 0. The default value is 22.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Liftering(Span<float> values, int l = 22)
    {
        int n = values.Length;
        if (n <= 1 || l <= 0) return;

        for (int i = 0; i < n; i++)
        {
            float v = 1f + l * Sin(ConstantsFp32.PI * i / l) / 2f;
            values[i] *= v;
        }
    }


    /// <summary>
    /// Applies sinusoidal liftering to the specified sequence of values using SIMD acceleration where possible.
    /// </summary>
    /// <remarks>Liftering is commonly used in signal processing, such as in Mel-frequency cepstral
    /// coefficient (MFCC) feature extraction, to emphasize or de-emphasize certain coefficients. This method uses SIMD
    /// vectorization for improved performance on supported hardware. If the length of the input span is zero or
    /// <paramref name="l"/> is less than or equal to zero, the method returns without modifying the input.</remarks>
    /// <param name="values">The sequence of values to be liftered. The liftering is performed in place, modifying the contents of this span.</param>
    /// <param name="l">The liftering parameter that determines the strength and periodicity of the liftering function. Must be greater
    /// than 0. The default value is 22.</param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Liftering_SIMD(Span<float> values, int l = 22)
    {
        int n = values.Length;
        if (n <= 1 || l <= 0) return;

        float factor = ConstantsFp32.PI / l;
        float halfL = l * 0.5f;

        int vecWidth = Vector<float>.Count;

        // lanes: 0..(vecWidth-1)
        Span<float> laneSpan = stackalloc float[vecWidth];
        for (int k = 0; k < vecWidth; k++)
        {
            laneSpan[k] = k;
        }
        var vLane = new Vector<float>(laneSpan);

        Span<float> angle = stackalloc float[vecWidth];
        Span<float> weights = stackalloc float[vecWidth];

        int i = 0;
        int vecEnd = n - (n % vecWidth);
        while (i < vecEnd)
        {
            var vBase = new Vector<float>(i);
            var vIdx = vBase + vLane;
            var vArg = vIdx * new Vector<float>(factor);

            vArg.CopyTo(angle);
            for (int k = 0; k < vecWidth; k++)
            {
                // Correct: 1 + (L/2) * sin(...)
                float w = 1f + halfL * Sin(angle[k]);
                weights[k] = w;
            }

            var vW = new Vector<float>(weights);
            var vVals = new Vector<float>(values.Slice(i, vecWidth));
            vVals *= vW;
            vVals.CopyTo(values.Slice(i, vecWidth));

            i += vecWidth;
        }

        for (; i < n; i++)
        {
            float w = 1f + halfL * Sin(factor * i);
            values[i] *= w;
        }
    }



    #endregion


    #region Blackman_Harris


    //tex:$$ w(n) = 0.35875 - 0.48829 \cos\left( \frac{2\pi n}{N} \right) + 0.14128 \cos\left( \frac{4\pi n}{N} \right) - 0.01168 \cos\left( \frac{6\pi n}{N} \right) $$


    /// <summary>
    /// Applies the Blackman-Harris window function to the specified span of values in place.
    /// </summary>
    /// <remarks>The Blackman-Harris window is commonly used in signal processing to reduce spectral leakage
    /// when performing a Fourier transform. This method modifies the input span directly; the original values are
    /// replaced with their windowed counterparts. The operation is performed in place and does not allocate additional
    /// memory.</remarks>
    /// <param name="values">The span of floating-point values to which the Blackman-Harris window will be applied. Each element is
    /// multiplied by the corresponding window coefficient.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman_Harris(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float factor = ConstantsFp32.TWO_PI / n;
        for (int i = 0; i < n; i++)
        {
            float arg = factor * i;
            float harris =
                0.35875f -
                0.48829f * Cos(arg) +
                0.14128f * Cos(2 * arg) -
                0.01168f * Cos(3 * arg);

            values[i] *= harris;
        }
    }

    /// <summary>
    /// Applies the Blackman-Harris window function to the specified span of single-precision floating-point values in
    /// place.
    /// </summary>
    /// <remarks>The Blackman-Harris window is commonly used in signal processing to reduce spectral leakage
    /// before performing a Fourier transform. This method uses SIMD acceleration when possible for improved
    /// performance. If the span is empty, the method returns without modifying any values.</remarks>
    /// <param name="values">The span of values to which the Blackman-Harris window will be applied. The window is applied in place,
    /// modifying the contents of the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Blackman_Harris_SIMD(Span<float> values)
    {
        int n = values.Length;
        if (n <= 1) return;

        float theta = ConstantsFp32.TWO_PI / n;

        var weights = n <= STACK_THRESHOLD ? stackalloc float[n] : new float[n];
        var cos1 = n < STACK_THRESHOLD ? stackalloc float[n] : new float[n];

        GenerateCosSequence(cos1, theta);

        for (int i = 0; i < n; i++)
        {
            float c1 = cos1[i];
            float c2 = 2f * c1 * c1 - 1f;                          // cos(2x)
            float c3 = 4f * c1 * c1 * c1 - 3f * c1;                // cos(3x)

            float w = 0.35875f
                    - 0.48829f * c1
                    + 0.14128f * c2
                    - 0.01168f * c3;
            weights[i] = w;
        }

        ApplyWeights_SIMD(values, weights);
    }


    #endregion


    #region Apply

    /// <summary>
    /// Applies the specified window function to the elements of the given span in place.
    /// </summary>
    /// <remarks>Use this method to shape a signal or data sequence by applying a window function, which is
    /// commonly used in digital signal processing to reduce spectral leakage before performing a Fourier transform. The
    /// method modifies the input span directly; no new array is allocated.</remarks>
    /// <param name="values">The span of floating-point values to which the window function is applied. The values in the span are modified
    /// in place.</param>
    /// <param name="windowType">The type of window function to apply to the values.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Apply_Normal(Span<float> values, WindowType windowType)
    {
        switch (windowType)
        {
            case WindowType.Rectangular:
                Rectangular(values);
                break;
            case WindowType.Triangular:
                Triangular(values);
                break;
            case WindowType.Hamming:
                Hamming(values);
                break;
            case WindowType.HammingPeriodic:
                Hamming_Periodic(values);
                break;
            case WindowType.Blackman:
                Blackman(values);
                break;
            case WindowType.BlackmanPeriodic:
                Blackman_Periodic(values);
                break;
            case WindowType.Hann:
                Hann(values);
                break;
            case WindowType.HannPeriodic:
                Hann_Periodic(values);
                break;
            case WindowType.Gaussian:
                Gaussian(values);
                break;
            case WindowType.Kaiser:
                Kaiser(values);
                break;
            case WindowType.Kbd:
                Kbd(values);
                break;
            case WindowType.BartlettHann:
                Bartlett_Hann(values);
                break;
            case WindowType.Lanczos:
                Lanczos(values);
                break;
            case WindowType.PowerOfSine:
                PowerOfSine(values);
                break;
            case WindowType.Flattop:
                Flattop(values);
                break;
            case WindowType.Liftering:
                Liftering(values);
                break;
            case WindowType.BlackmanHarris:
                Blackman_Harris(values);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Applies the specified window using SIMD-optimized implementations where available.
    /// Falls back to the scalar implementation if a SIMD variant is not provided.
    /// </summary>
    /// <param name="values">Input data to be windowed in-place.</param>
    /// <param name="windowType">Window function type.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Apply_SIMD(Span<float> values, WindowType windowType)
    {
        switch (windowType)
        {
            case WindowType.Rectangular:
                Rectangular(values); // no-op
                break;

            case WindowType.Triangular:
                Triangular_SIMD(values);
                break;

            case WindowType.Hamming:
                Hamming_SIMD(values);
                break;

            case WindowType.HammingPeriodic:
                Hamming_Periodic_SIMD(values);
                break;

            case WindowType.Blackman:
                Blackman_SIMD(values);
                break;

            case WindowType.BlackmanPeriodic:
                Blackman_Periodic_SIMD(values);
                break;

            case WindowType.Hann:
                Hann_SIMD(values);
                break;

            case WindowType.HannPeriodic:
                Hann_Periodic_SIMD(values);
                break;

            case WindowType.Gaussian:
                Gaussian_SIMD(values);
                break;

            case WindowType.Kaiser:
                Kaiser_SIMD(values);
                break;

            case WindowType.Kbd:
                Kbd_SIMD(values);
                break;

            case WindowType.BartlettHann:
                Bartlett_Hann_SIMD(values);
                break;

            case WindowType.Lanczos:
                Lanczos_SIMD(values);
                break;

            case WindowType.PowerOfSine:
                PowerOfSine_SIMD(values);
                break;

            case WindowType.Flattop:
                Flattop_SIMD(values);
                break;

            case WindowType.Liftering:
                Liftering_SIMD(values);
                break;

            case WindowType.BlackmanHarris:
                Blackman_Harris_SIMD(values);
                break;

            default:
                break;
        }
    }


    /// <summary>
    /// Applies the specified window function to the elements of the provided span in place.
    /// </summary>
    /// <remarks>This method modifies the contents of <paramref name="values"/> directly. Using SIMD
    /// acceleration can improve performance on supported hardware, but results are otherwise equivalent to standard
    /// processing.</remarks>
    /// <param name="values">A span of single-precision floating-point values to which the window function will be applied. The values are
    /// modified in place.</param>
    /// <param name="windowType">The type of window function to apply to the values. Determines the shape of the windowing operation.</param>
    /// <param name="useSIMD">Specifies whether to use SIMD acceleration for the windowing operation. Set to <see langword="true"/> to enable
    /// hardware-accelerated processing; otherwise, <see langword="false"/> for standard processing. Defaults to <see
    /// langword="true"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Apply(Span<float> values, WindowType windowType, bool useSIMD = true)
    {
        if (useSIMD)
            Apply_SIMD(values, windowType);
        else
            Apply_Normal(values, windowType);
    }

    #endregion




    internal readonly struct WindowCompareResult
    {
        public string Name { get; }
        public bool Equal { get; }
        public int Length { get; }
        public float Tolerance { get; }
        public int MismatchIndex { get; }
        public float MaxAbsDiff { get; }

        public WindowCompareResult(string name, bool equal, int length, float tolerance, int mismatchIndex, float maxAbsDiff)
        {
            Name = name;
            Equal = equal;
            Length = length;
            Tolerance = tolerance;
            MismatchIndex = mismatchIndex;
            MaxAbsDiff = maxAbsDiff;
        }

        public override string ToString()
            => $"{Name}: Equal={Equal}, N={Length}, tol={Tolerance}, maxDiff={MaxAbsDiff}, mismatchIndex={MismatchIndex}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CompareWindowCoefficientsCore(
        Action<Span<float>> windowA,
        Action<Span<float>> windowB,
        int length,
        float tolerance,
        out int mismatchIndex,
        out float maxAbsDiff)
    {
        mismatchIndex = -1;
        maxAbsDiff = 0f;

        if (length <= 0 || windowA is null || windowB is null || tolerance < 0f)
        {
            return false;
        }

        var a = new float[length];
        var b = new float[length];

        // 全 1 向量，应用窗得到系数
        for (int i = 0; i < length; i++)
        {
            a[i] = 1f;
            b[i] = 1f;
        }

        windowA(a);
        windowB(b);

        bool equal = true;
        for (int i = 0; i < length; i++)
        {
            float diff = Abs(a[i] - b[i]);
            if (diff > maxAbsDiff)
            {
                maxAbsDiff = diff;
            }
            if (diff > tolerance && equal)
            {
                equal = false;
                mismatchIndex = i;
            }
        }
        return equal;
    }

    /// <summary>
    /// 对比所有常规/新的 SIMD 方法对的结果一致性。
    /// </summary>
    /// <param name="length">测试窗口长度 N。</param>
    /// <param name="tolerance">允许的最大绝对误差。</param>
    /// <returns>每一对窗的对比结果集合。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static WindowCompareResult[] CompareAllWindowPairs(int length, float tolerance = 1e-6f)
    {
        var results = new List<WindowCompareResult>(16);

        void Add(string name, Action<Span<float>> std, Action<Span<float>> simd)
        {
            int idx; float maxDiff;
            bool equal = CompareWindowCoefficientsCore(std, simd, length, tolerance, out idx, out maxDiff);
            results.Add(new WindowCompareResult(name, equal, length, tolerance, idx, maxDiff));
        }

        // Triangular
        Add("Triangular vs Triangular_SIMD", Triangular, Triangular_SIMD);

        // Hamming (symmetric form)
        Add("Hamming vs Hamming_SIMD", Hamming, Hamming_SIMD);

        // Hamming (periodic form)
        Add("Hamming_Periodic vs Hamming_Periodic_SIMD", Hamming_Periodic, Hamming_Periodic_SIMD);

        // Blackman (symmetric)
        Add("Blackman vs Blackman_SIMD", Blackman, Blackman_SIMD);

        // Blackman (periodic)
        Add("Blackman_Periodic vs Blackman_Periodic_SIMD", Blackman_Periodic, Blackman_Periodic_SIMD);

        // Hann (symmetric)
        Add("Hann vs Hann_SIMD", Hann, Hann_SIMD);

        // Hann (periodic)
        Add("Hann_Periodic vs Hann_Periodic_SIMD", Hann_Periodic, Hann_Periodic_SIMD);

        // Gaussian (fixed sigma=0.4)
        Add("Gaussian vs Gaussian_SIMD", Gaussian, Gaussian_SIMD);

        // Gaussian (parametric) —— 以 sigma=0.4 为例，可根据需要改参数
        Add("Gaussian(sigma=0.4) vs Gaussian_SIMD(sigma=0.4)",
            (Span<float> v) => Gaussian(v, 0.4f),
            (Span<float> v) => Gaussian_SIMD(v, 0.4f));

        // Kaiser (alpha=12)
        Add("Kaiser(alpha=12) vs Kaiser_SIMD(alpha=12)",
            (Span<float> v) => Kaiser(v, 12f),
            (Span<float> v) => Kaiser_SIMD(v, 12f));

        // KBD (alpha=4)
        Add("Kbd(alpha=4) vs Kbd_SIMD(alpha=4)",
            (Span<float> v) => Kbd(v, 4f),
            (Span<float> v) => Kbd_SIMD(v, 4f));

        // Bartlett-Hann
        Add("Bartlett_Hann vs Bartlett_Hann_SIMD", Bartlett_Hann, Bartlett_Hann_SIMD);

        // Lanczos
        Add("Lanczos vs Lanczos_SIMD", Lanczos, Lanczos_SIMD);

        // PowerOfSine (alpha=1.5)
        Add("PowerOfSine(alpha=1.5) vs PowerOfSine_SIMD(alpha=1.5)",
            (Span<float> v) => PowerOfSine(v, 1.5f),
            (Span<float> v) => PowerOfSine_SIMD(v, 1.5f));

        // Flattop
        Add("Flattop vs Flattop_SIMD", Flattop, Flattop_SIMD);

        // Liftering (l=22)
        Add("Liftering(l=22) vs Liftering_SIMD(l=22)",
            (Span<float> v) => Liftering(v, 22),
            (Span<float> v) => Liftering_SIMD(v, 22));

        // Blackman-Harris (periodic)
        Add("Blackman_Harris vs Blackman_Harris_SIMD", Blackman_Harris, Blackman_Harris_SIMD);

        return results.ToArray();
    }
}
