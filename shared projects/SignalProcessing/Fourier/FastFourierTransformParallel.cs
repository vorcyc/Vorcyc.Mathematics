
namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

using System.Threading.Tasks;
using Vorcyc.Mathematics.Numerics;

#if NET6_0_OR_GREATER
using static System.MathF;
#else
using static Vorcyc.Offlet.Math.VMath;
#endif

internal static class FastFourierTransformParallel
{
    private const float PI = 3.14159265358979323846f;
    private const float Negative_PI = -3.14159265358979323846f;
    private const int ParallelThreshold = 8192; // 小输入使用单线程

    #region Private Methods

    private static unsafe void Rearrange(float* input, ComplexFp32* output, int N)
    {
        int target = 0;
        for (int position = 0; position < N; ++position)
        {
            output[target] = input[position];
            int mask = N;
            while ((target & (mask >>= 1)) != 0)
                target &= ~mask;
            target |= mask;
        }
    }

    private static void Rearrange(ReadOnlySpan<float> input, Span<ComplexFp32> output)
    {
        int target = 0;
        for (int position = 0; position < input.Length; ++position)
        {
            output[target] = input[position];
            int mask = input.Length;
            while ((target & (mask >>= 1)) != 0)
                target &= ~mask;
            target |= mask;
        }
    }

    private static unsafe void Rearrange(ComplexFp32* input, ComplexFp32* output, int N)
    {
        int target = 0;
        for (int position = 0; position < N; ++position)
        {
            output[target] = input[position];
            int mask = N;
            while ((target & (mask >>= 1)) != 0)
                target &= ~mask;
            target |= mask;
        }
    }

    private static void Rearrange(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output)
    {
        int target = 0;
        for (int position = 0; position < input.Length; ++position)
        {
            output[target] = input[position];
            int mask = input.Length;
            while ((target & (mask >>= 1)) != 0)
                target &= ~mask;
            target |= mask;
        }
    }

    private static unsafe void Rearrange(ComplexFp32* data, int N)
    {
        int target = 0;
        for (int position = 0; position < N; ++position)
        {
            if (target > position)
            {
                ComplexFp32 temp = data[target];
                data[target] = data[position];
                data[position] = temp;
            }
            int mask = N;
            while ((target & (mask >>= 1)) != 0)
                target &= ~mask;
            target |= mask;
        }
    }

    private static void Rearrange(Span<ComplexFp32> data)
    {
        int target = 0;
        for (int position = 0; position < data.Length; ++position)
        {
            if (target > position)
            {
                ComplexFp32 temp = data[target];
                data[target] = data[position];
                data[position] = temp;
            }
            int mask = data.Length;
            while ((target & (mask >>= 1)) != 0)
                target &= ~mask;
            target |= mask;
        }
    }

    private static unsafe void Perform(ComplexFp32* data, int N, bool inverse = false)
    {
        float pi = inverse ? PI : Negative_PI;

        for (int step = 1; step < N; step <<= 1)
        {
            int jump = step << 1;
            float delta = pi / step;
            float sine = Sin(delta * 0.5f);
            ComplexFp32 multiplier = new(-2.0f * sine * sine, Sin(delta));

            // 预计算所有 factor 值
            ComplexFp32[] factors = new ComplexFp32[step];
            factors[0] = ComplexFp32.One;
            for (int i = 1; i < step; i++)
                factors[i] = multiplier * factors[i - 1] + factors[i - 1];

            if (N < ParallelThreshold)
            {
                // 单线程处理小输入
                for (int group = 0; group < step; ++group)
                {
                    ComplexFp32 factor = factors[group];
                    for (int pair = group; pair < N; pair += jump)
                    {
                        int match = pair + step;
                        ComplexFp32 product = factor * data[match];
                        data[match] = data[pair] - product;
                        data[pair] += product;
                    }
                }
            }
            else
            {
                // 并行处理
                Parallel.For(0, step, group =>
                {
                    ComplexFp32 factor = factors[group];
                    for (int pair = group; pair < N; pair += jump)
                    {
                        int match = pair + step;
                        ComplexFp32 product = factor * data[match];
                        data[match] = data[pair] - product;
                        data[pair] += product;
                    }
                });
            }
        }
    }

    private static void Perform(Span<ComplexFp32> data, bool inverse = false)
    {
        unsafe
        {
            fixed (ComplexFp32* ptr = data)
            {
                Perform(ptr, data.Length, inverse);
            }
        }
    }

    private static unsafe void Scale(ComplexFp32* data, int N)
    {
        float factor = 1.0f / N;
        for (int position = 0; position < N; ++position)
            data[position] *= factor;
    }

    private static void Scale(Span<ComplexFp32> data)
    {
        float factor = 1.0f / data.Length;
        for (int position = 0; position < data.Length; ++position)
            data[position] *= factor;
    }

    #endregion

    #region Forward

    public static unsafe bool Forward(float* input, ComplexFp32* output, int N)
    {
        if (input is null || output is null || N < 1 || !N.IsPowerOf2())
            return false;
        Rearrange(input, output, N);
        Perform(output, N);
        return true;
    }

    public static bool Forward(float[] input, int offset, out ComplexFp32[] output, int N)
    {
        unsafe
        {
            var result = new ComplexFp32[N];
            fixed (float* pIn = input)
            fixed (ComplexFp32* pOut = result)
            {
                var success = Forward(pIn + offset, pOut, N);
                output = result;
                return success;
            }
        }
    }

    public static bool Forward(ReadOnlySpan<float> input, Span<ComplexFp32> output)
    {
        if (input.IsEmpty || output.IsEmpty || input.Length < 1 || !input.Length.IsPowerOf2())
            return false;
        Rearrange(input, output);
        Perform(output);
        return true;
    }

    public static unsafe bool Forward(ComplexFp32* input, ComplexFp32* output, int N)
    {
        if (input is null || output is null || N < 1 || !N.IsPowerOf2())
            return false;
        Rearrange(input, output, N);
        Perform(output, N);
        return true;
    }

    public static bool Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output)
    {
        if (input.IsEmpty || output.IsEmpty || input.Length < 1 || !input.Length.IsPowerOf2())
            return false;
        Rearrange(input, output);
        Perform(output);
        return true;
    }

    public static bool Forward(ComplexFp32[] input, int offset, ComplexFp32[] output, int N)
    {
        unsafe
        {
            fixed (ComplexFp32* pIn = input, pOut = output)
            {
                return Forward(pIn + offset, pOut, N);
            }
        }
    }

    public static unsafe bool Forward(ComplexFp32* data, int N)
    {
        if (data is null || N < 1 || !N.IsPowerOf2())
            return false;
        Rearrange(data, N);
        Perform(data, N);
        return true;
    }

    public static bool Forward(ComplexFp32[] data, int offset, int N)
    {
        unsafe
        {
            fixed (ComplexFp32* pData = data)
            {
                return Forward(pData + offset, N);
            }
        }
    }

    public static bool Forward(Span<ComplexFp32> data)
    {
        if (data.IsEmpty || data.Length < 1 || !data.Length.IsPowerOf2())
            return false;
        Rearrange(data);
        Perform(data);
        return true;
    }

    #endregion

    #region Inverse

    public static unsafe bool Inverse(ComplexFp32* input, ComplexFp32* output, int N, bool scale = true)
    {
        if (input is null || output is null || N < 1 || !N.IsPowerOf2())
            return false;
        Rearrange(input, output, N);
        Perform(output, N, true);
        if (scale)
            Scale(output, N);
        return true;
    }

    public static bool Inverse(ComplexFp32[] input, int inOffset, out ComplexFp32[] output, int outOffset, int N, bool scale = true)
    {
        unsafe
        {
            var result = new ComplexFp32[N];
            fixed (ComplexFp32* pIn = input, pOut = result)
            {
                var success = Inverse(pIn + inOffset, pOut + outOffset, N, scale);
                output = result;
                return success;
            }
        }
    }

    public static bool Inverse(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, bool scale = true)
    {
        if (input.IsEmpty || output.IsEmpty || input.Length < 1 || !input.Length.IsPowerOf2())
            return false;
        Rearrange(input, output);
        Perform(output, true);
        if (scale)
            Scale(output);
        return true;
    }

    public static unsafe bool Inverse(ComplexFp32* data, int N, bool scale = true)
    {
        if (data is null || N < 1 || !N.IsPowerOf2())
            return false;
        Rearrange(data, N);
        Perform(data, N, true);
        if (scale)
            Scale(data, N);
        return true;
    }

    public static bool Inverse(ComplexFp32[] data, int offset, int N, bool scale = true)
    {
        unsafe
        {
            fixed (ComplexFp32* pData = data)
            {
                return Inverse(pData + offset, N, scale);
            }
        }
    }

    public static bool Inverse(Span<ComplexFp32> data, bool scale = true)
    {
        if (data.IsEmpty || data.Length < 1 || !data.Length.IsPowerOf2())
            return false;
        Rearrange(data);
        Perform(data, true);
        if (scale)
            Scale(data);
        return true;
    }

    #endregion
}