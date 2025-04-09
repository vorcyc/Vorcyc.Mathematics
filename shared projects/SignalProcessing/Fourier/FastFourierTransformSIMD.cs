
namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics.Numerics;

#if NET6_0_OR_GREATER
using static System.MathF;
#else
using static Vorcyc.Offlet.Math.VMath;
#endif

public static class FastFourierTransformSIMD
{
    private const float PI = 3.14159265358979323846f;
    private const float Negative_PI = -3.14159265358979323846f;

    #region private methods

    /// <summary>
    /// Rearranges input data (real or complex) into bit-reversed order for FFT, non-inplace.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Rearrange<TInput>(ReadOnlySpan<TInput> input, Span<ComplexFp32> output)
        where TInput : struct
    {
        int target = 0;
        for (int position = 0; position < input.Length; ++position)
        {
            output[target] = input[position] switch
            {
                float f => new ComplexFp32(f, 0),
                ComplexFp32 c => c,
                _ => throw new NotSupportedException("Input type must be float or ComplexFp32.")
            };
            int mask = input.Length;
            while ((target & (mask >>= 1)) != 0)
                target &= ~mask;
            target |= mask;
        }
    }

    /// <summary>
    /// Rearranges complex data into bit-reversed order for FFT, inplace.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Perform(Span<ComplexFp32> data, bool inverse = false)
    {
        float pi = inverse ? PI : Negative_PI;
        int vectorSize = Vector<float>.Count;

        for (int step = 1; step < data.Length; step <<= 1)
        {
            int jump = step << 1;
            float delta = pi / step;
            float sine = Sin(delta * 0.5f);
            ComplexFp32 multiplier = new(-2.0f * sine * sine, Sin(delta));
            ComplexFp32 factor = ComplexFp32.One;

            for (int group = 0; group < step; ++group)
            {
                int vectorPairs = data.Length / jump / vectorSize * vectorSize;
                using var pool = MemoryPool<float>.Shared.Rent(vectorSize * 4);
                Span<float> buffer = pool.Memory.Span;
                Span<float> real1 = buffer.Slice(0, vectorSize);
                Span<float> imag1 = buffer.Slice(vectorSize, vectorSize);
                Span<float> real2 = buffer.Slice(vectorSize * 2, vectorSize);
                Span<float> imag2 = buffer.Slice(vectorSize * 3, vectorSize);

                for (int i = 0; i < vectorPairs; i += vectorSize)
                {
                    int basePair = group + i * jump;
                    int baseMatch = basePair + step;

                    for (int j = 0; j < vectorSize; j++)
                    {
                        real1[j] = data[basePair + j * jump].Real;
                        imag1[j] = data[basePair + j * jump].Imaginary;
                        real2[j] = data[baseMatch + j * jump].Real;
                        imag2[j] = data[baseMatch + j * jump].Imaginary;
                    }

                    Vector<float> vReal1 = new Vector<float>(real1);
                    Vector<float> vImag1 = new Vector<float>(imag1);
                    Vector<float> vReal2 = new Vector<float>(real2);
                    Vector<float> vImag2 = new Vector<float>(imag2);

                    Vector<float> factorReal = Vector<float>.One * factor.Real;
                    Vector<float> factorImag = Vector<float>.One * factor.Imaginary;
                    Vector<float> prodReal = vReal2 * factorReal - vImag2 * factorImag;
                    Vector<float> prodImag = vReal2 * factorImag + vImag2 * factorReal;

                    Vector<float> resultReal2 = vReal1 - prodReal;
                    Vector<float> resultImag2 = vImag1 - prodImag;
                    Vector<float> resultReal1 = vReal1 + prodReal;
                    Vector<float> resultImag1 = vImag1 + prodImag;

                    for (int j = 0; j < vectorSize; j++)
                    {
                        data[basePair + j * jump] = new ComplexFp32(resultReal1[j], resultImag1[j]);
                        data[baseMatch + j * jump] = new ComplexFp32(resultReal2[j], resultImag2[j]);
                    }
                }

                for (int pair = group + vectorPairs * jump; pair < data.Length; pair += jump)
                {
                    int match = pair + step;
                    ComplexFp32 product = factor * data[match];
                    data[match] = data[pair] - product;
                    data[pair] += product;
                }

                factor = multiplier * factor + factor;
            }
        }
    }

    /// <summary>
    /// Scales the inverse FFT result by 1/N using SIMD.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Scale(Span<ComplexFp32> data)
    {
        float factor = 1.0f / data.Length;
        int vectorSize = Vector<float>.Count;
        Vector<float> vFactor = new Vector<float>(factor);

        // SIMD scaling
        for (int i = 0; i < data.Length / vectorSize; i++)
        {
            Span<float> real = stackalloc float[vectorSize];
            Span<float> imag = stackalloc float[vectorSize];
            for (int j = 0; j < vectorSize; j++)
            {
                real[j] = data[i * vectorSize + j].Real;
                imag[j] = data[i * vectorSize + j].Imaginary;
            }

            Vector<float> vReal = new Vector<float>(real);
            Vector<float> vImag = new Vector<float>(imag);
            vReal *= vFactor;
            vImag *= vFactor;

            for (int j = 0; j < vectorSize; j++)
            {
                data[i * vectorSize + j] = new ComplexFp32(vReal[j], vImag[j]);
            }
        }

        // Handle remaining elements
        for (int i = (data.Length / vectorSize) * vectorSize; i < data.Length; i++)
        {
            data[i] *= factor;
        }
    }

    #endregion

    #region Forward

    public static unsafe bool Forward(float* input, ComplexFp32* output, int N)
    {
        if (input is null || output is null || N < 1 || !N.IsPowerOf2())
            return false;
        var inputSpan = new ReadOnlySpan<float>(input, N);
        var outputSpan = new Span<ComplexFp32>(output, N);
        Rearrange<float>(inputSpan, outputSpan);
        Perform(outputSpan);
        return true;
    }

    //过渡版本
    private static unsafe bool Forward(float[] input, int offset, ComplexFp32* output, int N)
    {
        fixed (float* pIn = input)
        {
            return Forward(pIn + offset, output, N);
        }
    }

    public static bool Forward(float[] input, int offset, out ComplexFp32[] output, int N)
    {
        unsafe
        {
            var result = new ComplexFp32[N];
            fixed (ComplexFp32* pOut = result)
            {
                var success = Forward(input, offset, pOut, N);
                output = result;
                return success;
            }
        }
    }

    public static bool Forward(ReadOnlySpan<float> input, Span<ComplexFp32> output)
    {
        if (input.IsEmpty || output.IsEmpty || input.Length < 1 || !input.Length.IsPowerOf2() || input.Length != output.Length)
            return false;
        Rearrange<float>(input, output);
        Perform(output);
        return true;
    }

    public static unsafe bool Forward(ComplexFp32* input, ComplexFp32* output, int N)
    {
        if (input is null || output is null || N < 1 || !N.IsPowerOf2())
            return false;
        var inputSpan = new ReadOnlySpan<ComplexFp32>(input, N);
        var outputSpan = new Span<ComplexFp32>(output, N);
        Rearrange<ComplexFp32>(inputSpan, outputSpan);
        Perform(outputSpan);
        return true;
    }

    public static bool Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output)
    {
        if (input.IsEmpty || output.IsEmpty || input.Length < 1 || !input.Length.IsPowerOf2() || input.Length != output.Length)
            return false;
        Rearrange<ComplexFp32>(input, output);
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
        var dataSpan = new Span<ComplexFp32>(data, N);
        Rearrange(dataSpan);
        Perform(dataSpan);
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
        var inputSpan = new ReadOnlySpan<ComplexFp32>(input, N);
        var outputSpan = new Span<ComplexFp32>(output, N);
        Rearrange<ComplexFp32>(inputSpan, outputSpan);
        Perform(outputSpan, true);
        if (scale)
            Scale(outputSpan);
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
        if (input.IsEmpty || output.IsEmpty || input.Length < 1 || !input.Length.IsPowerOf2() || input.Length != output.Length)
            return false;
        Rearrange<ComplexFp32>(input, output);
        Perform(output, true);
        if (scale)
            Scale(output);
        return true;
    }

    public static unsafe bool Inverse(ComplexFp32* data, int N, bool scale = true)
    {
        if (data is null || N < 1 || !N.IsPowerOf2())
            return false;
        var dataSpan = new Span<ComplexFp32>(data, N);
        Rearrange(dataSpan);
        Perform(dataSpan, true);
        if (scale)
            Scale(dataSpan);
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