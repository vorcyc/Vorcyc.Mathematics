using Vorcyc.Mathematics.Extensions.FFTW.Interop;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.Extensions.FFTW.FftwSingle;

/// <summary>
/// 提供针对单精度复数与实数数据执行一维离散傅里叶变换 (DFT) 的静态方法集合。
/// 封装 FFTW (single precision) 的 1D 复数-复数、实数-复数、复数-实数正/逆变换接口，并负责规划(plan)的创建与销毁。
/// </summary>
/// <remarks>
/// 使用本类型的方法需注意：
/// 1. 使用 <see cref="PinnableArray{T}"/> 的变换方法要求传入的缓冲区已固定（Pinned）。未固定会抛出异常。
/// 2. 使用 <see cref="Span{T}"/> 重载的方法会在内部通过 <c>fixed</c> 语句暂时固定内存，调用结束即解除。
/// 3. 所有方法均非线程安全；不要在不同线程上同时对存在重叠内存区域的缓冲区调用这些方法。
/// 4. 逆变换（尤其是 Complex→Real）通常需要执行归一化 (按 1/N 缩放)。本类的 C2R 方法提供可选缩放操作。复数逆变换完成后亦可手动调用扩展方法实现缩放。
/// 5. 对于实数与复数之间的变换，长度关系遵循 FFTW 的紧凑存储格式：
///    - R2C：实数输入长度 = 复数输出长度 * 2 - 2
///    - C2R：复数输入长度 = (实数输出长度 / 2) + 1
/// </remarks>
public static class Dft1D
{
    // 1D complex-to-complex

    /// <summary>
    /// 对单精度复数缓冲区执行一维离散傅里叶变换 (DFT)，以原地方式 (in-place) 修改其内容。
    /// </summary>
    /// <param name="buffer">待变换的复数数据缓冲区。结果将覆盖原始数据。</param>
    /// <param name="direction">变换方向：正向 (<see cref="fftw_direction.Forward"/>) 或逆向 (<see cref="fftw_direction.Backward"/>)。</param>
    /// <param name="flags">规划策略标志，默认 <see cref="fftw_flags.Estimate"/>。可选 <c>Measure</c> / <c>Patient</c> / <c>Exhaustive</c> 等。</param>
    /// <remarks>
    /// 若执行逆向变换 (Backward)，FFT 输出通常需再乘以 1/N 进行归一化，其中 N 为变换长度。
    /// 可在缓冲区上调用 <see cref="PinnableArray{T}.ScaleInPlace(float)"/>实现缩放。
    /// </remarks>
    /// <exception cref="InvalidOperationException">当缓冲区未固定 (Pinned) 时抛出。</exception>
    public static void Dft1DComplexInPlace(PinnableArray<ComplexFp32> buffer, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(buffer);

        var plan = fftwf.dft_1d(buffer.Length, buffer, buffer, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create in-place complex plan.");

        try
        {
            fftwf.execute(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 对单精度复数数据执行原地一维离散傅里叶变换 (DFT)。
    /// </summary>
    /// <param name="buffer">复数输入与输出共享的缓冲区 (原地变换)。长度必须大于 0。</param>
    /// <param name="direction">变换方向：正向或逆向。</param>
    /// <param name="flags">规划策略标志，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 原地模式：输入数据在执行后被输出结果覆盖。<br/>
    /// - 非线程安全：不要在多个线程中对重叠内存调用此方法。<br/>
    /// - 若为逆变换，可在外部自行按 1/N 归一化。<br/>
    /// </remarks>
    /// <exception cref="ArgumentException">当 <paramref name="buffer"/> 为空时抛出。</exception>
    public static void Dft1DComplexInPlace(Span<ComplexFp32> buffer, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(buffer);

        unsafe
        {
            fixed (ComplexFp32* p = buffer)
            {
                var ptr = (IntPtr)p;

                var plan = fftwf.dft_1d(buffer.Length, ptr, ptr, direction, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create in-place complex plan.");
                try
                {
                    fftwf.execute(plan);
                }
                finally
                {
                    fftwf.destroy_plan(plan);
                }
            }
        }
    }

    /// <summary>
    /// 计算单精度复数输入数据的一维离散傅里叶变换 (DFT)，结果写入输出缓冲区（非原地）。
    /// </summary>
    /// <param name="input">已固定的复数输入缓冲区。</param>
    /// <param name="output">已固定的复数输出缓冲区，长度须与输入相同。</param>
    /// <param name="direction">变换方向：正向或逆向。</param>
    /// <param name="flags">规划策略标志，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// - 输入与输出必须均已固定 (Pinned)。<br/>
    /// - 本方法封装计划创建、执行与销毁。<br/>
    /// - 若为逆向变换，需在外部执行归一化 (乘以 1/N)。<br/>
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定时抛出。</exception>
    public static void Dft1DComplex(PinnableArray<ComplexFp32> input, PinnableArray<ComplexFp32> output, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(input);
        InvalidOperationException.ThrowIfUnpinned(output);
        ArgumentException.ThrowIfArrayLengthNotEqual(input, output);

        var plan = fftwf.dft_1d(input.Length, input, output, direction, flags);
        InvalidOperationException.ThrowIfZero(plan, "Failed to create complex plan.");

        try
        {
            fftwf.execute(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 执行单精度复数输入到单精度复数输出的一维离散傅里叶变换 (DFT)（非原地）。
    /// </summary>
    /// <param name="input">复数输入数据。长度必须大于 0。</param>
    /// <param name="output">复数输出数据缓冲区。长度须与 <paramref name="input"/> 相同。</param>
    /// <param name="direction">变换方向。</param>
    /// <param name="flags">规划策略标志。</param>
    /// <remarks>
    /// - 输入与输出通过固定指针传递给底层 FFTW。<br/>
    /// - 非线程安全；不要跨线程操作重叠内存。<br/>
    /// - 逆变换后通常需要外部归一化处理。<br/>
    /// </remarks>
    /// <exception cref="ArgumentException">当输入与输出长度不一致时抛出。</exception>
    public static void Dft1DComplex(Span<ComplexFp32> input, Span<ComplexFp32> output, fftw_direction direction, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(input);
        ArgumentNullException.ThrowIfEmpty(output);
        ArgumentException.ThrowIfArrayLengthNotEqual(input, output, "Input and output spans must have the same length.");

        unsafe
        {
            fixed (ComplexFp32* pInput = input)
            fixed (ComplexFp32* pOutput = output)
            {
                var plan = fftwf.dft_1d(input.Length, (IntPtr)pInput, (IntPtr)pOutput, direction, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create complex plan.");
                try
                {
                    fftwf.execute(plan);
                }
                finally
                {
                    fftwf.destroy_plan(plan);
                }
            }
        }
    }

    // 1D real/complex

    /// <summary>
    /// 执行单精度实数输入到单精度复数输出的一维实数→复数 (R2C) 傅里叶变换。
    /// </summary>
    /// <param name="realInput">已固定的实数输入缓冲区。长度为 N。</param>
    /// <param name="complexOutput">已固定的复数输出缓冲区。其长度应满足 FFTW 紧凑结构：<c>N = complexOutput.Length * 2 - 2</c>。</param>
    /// <param name="flags">规划策略标志，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// 输出使用半谱紧凑存储（包含从 0 到 Nyquist 频率的系数）。不执行归一化。若后续需要频谱幅度统一，可自行处理缩放。 
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定时抛出。</exception>
    /// <exception cref="InvalidOperationException">当计划创建失败时抛出。</exception>
    public static void Dft1DR2C(PinnableArray<float> realInput, PinnableArray<ComplexFp32> complexOutput, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(realInput);
        InvalidOperationException.ThrowIfUnpinned(complexOutput);
        ArgumentException.ThrowIfArrayLengthNotEqual(realInput, complexOutput);

        var plan = fftwf.dft_r2c_1d(realInput.Length, realInput, complexOutput, flags);

        InvalidOperationExceptionExtension.ThrowIfZero(plan, "Failed to create 1D real to complex plan.");

        try
        {
            fftwf.execute(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }
    }

    /// <summary>
    /// 执行单精度实数→复数的一维离散傅里叶变换 (R2C)。
    /// </summary>
    /// <param name="realInput">实数输入数据。长度 N 必须大于 0。</param>
    /// <param name="complexOutput">复数输出缓冲区。长度需满足 <c>realInput.Length == complexOutput.Length * 2 - 2</c>。</param>
    /// <param name="flags">规划策略标志。</param>
    /// <remarks>
    /// - 输出为紧凑半谱格式（包含 DC 与 Nyquist）。<br/>
    /// - 不包含归一化步骤。若须能量保持或逆变换匹配请自行缩放。<br/>
    /// </remarks>
    /// <exception cref="ArgumentException">当长度关系不满足要求时抛出。</exception>
    public static void Dft1DR2C(Span<float> realInput, Span<ComplexFp32> complexOutput, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(realInput);
        ArgumentNullException.ThrowIfEmpty(complexOutput);
        ArgumentException.ThrowIfArrayLengthNotEqual(realInput, complexOutput);

        unsafe
        {
            fixed (float* pRealInput = realInput)
            fixed (ComplexFp32* pComplexOutput = complexOutput)
            {
                var plan = fftwf.dft_r2c_1d(realInput.Length, (IntPtr)pRealInput, (IntPtr)pComplexOutput, flags);
                InvalidOperationExceptionExtension.ThrowIfZero(plan, "Failed to create 1D real to complex plan.");
                try
                {
                    fftwf.execute(plan);
                }
                finally
                {
                    fftwf.destroy_plan(plan);
                }
            }
        }
    }

    /// <summary>
    /// 执行单精度复数→实数的一维离散傅里叶逆变换 (C2R)，可选自动归一化。
    /// </summary>
    /// <param name="complexInput">已固定的复数输入半谱缓冲区 (长度 M)。</param>
    /// <param name="realOutput">已固定的实数输出缓冲区，长度应满足 <c>realOutput.Length == complexInput.Length * 2 - 2</c>。</param>
    /// <param name="scale">若为 <c>true</c>，执行结果按 1/N (N = complexInput.Length) 归一化。</param>
    /// <param name="flags">规划策略标志，默认 <see cref="fftw_flags.Estimate"/>。</param>
    /// <remarks>
    /// 输入应为由对应 R2C 变换生成的半谱紧凑格式。若 <paramref name="scale"/> 为 <c>false</c>，输出即为未归一化的逆变换结果。
    /// </remarks>
    /// <exception cref="InvalidOperationException">当输入或输出未固定时抛出。</exception>
    public static void Dft1DC2R(PinnableArray<ComplexFp32> complexInput, PinnableArray<float> realOutput, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        InvalidOperationException.ThrowIfUnpinned(complexInput);
        InvalidOperationException.ThrowIfUnpinned(realOutput);
        ArgumentException.ThrowIfArrayLengthNotEqual(realOutput, complexInput);

        var plan = fftwf.dft_c2r_1d(complexInput.Length, complexInput, realOutput, flags);

        InvalidOperationException.ThrowIfZero(plan, "Failed to create 1D complex to real plan.");

        try
        {
            fftwf.execute(plan);
        }
        finally
        {
            fftwf.destroy_plan(plan);
        }

        if (scale)
        {
            // 按 1/N 归一化输出。
            unsafe
            {
                var count = complexInput.Length;
                var factor = 1f / count;
                for (var i = 0; i < count; i++)
                {
                    realOutput[i] *= factor;
                }
            }
        }
    }

    /// <summary>
    /// 执行单精度复数→实数的一维逆离散傅里叶变换 (C2R)，可选按 1/N 归一化。
    /// </summary>
    /// <param name="complexInput">复数输入半谱缓冲区。</param>
    /// <param name="realOutput">实数输出缓冲区，长度须满足 <c>realOutput.Length == complexInput.Length * 2 - 2</c>。</param>
    /// <param name="scale">是否执行按 1/N 的归一化。</param>
    /// <param name="flags">规划策略标志。</param>
    /// <remarks>
    /// - 输入必须是对应 R2C 的半谱格式。<br/>
    /// - 若未缩放，输出为未归一化结果，可能需外部再缩放。<br/>
    /// </remarks>
    /// <exception cref="ArgumentException">当长度关系不满足要求时抛出。</exception>
    public static void Dft1DC2R(Span<ComplexFp32> complexInput, Span<float> realOutput, bool scale = true, fftw_flags flags = fftw_flags.Estimate)
    {
        ArgumentNullException.ThrowIfEmpty(complexInput);
        ArgumentNullException.ThrowIfEmpty(realOutput);
        ArgumentException.ThrowIfArrayLengthNotEqual(realOutput, complexInput);

        unsafe
        {
            fixed (ComplexFp32* pComplexInput = complexInput)
            fixed (float* pRealOutput = realOutput)
            {
                var plan = fftwf.dft_c2r_1d(complexInput.Length, (IntPtr)pComplexInput, (IntPtr)pRealOutput, flags);
                InvalidOperationException.ThrowIfZero(plan, "Failed to create 1D complex to real plan.");
                try
                {
                    fftwf.execute(plan);
                }
                finally
                {
                    fftwf.destroy_plan(plan);
                }
            }
        }
        if (scale)
        {
            unsafe
            {
                var count = complexInput.Length;
                var factor = 1f / count;
                for (var i = 0; i < count; i++)
                {
                    realOutput[i] *= factor;
                }
            }
        }
    }
}