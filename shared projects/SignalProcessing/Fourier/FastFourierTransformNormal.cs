namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

/* duan linli aka cyclone_dll
 * 这个版本是最早期的基础实现，后来直到2025.5.10又增加了SIMD和Parallel版本的。
 * 所以这个也同时改名成 Normal。
 * 19.11.5
 * VORCYC CO,.LTD
 */


//var input = new float[10] { -1, 2, -3, 4, -5, 6, -7, 8, -9, 10 };//, -11, 12, -13, 14, -15, 16 };
//FastFourierTransform.Forward(input, 0, out ComplexFP32[] output, 16);


//            foreach (var x in output)
//                Console.WriteLine(x);

//            Console.WriteLine("----------------");

//            FastFourierTransform.Inverse(output, 0, 16);

//            foreach (var x in output)
//                Console.WriteLine(x);

//不给足够的实际数组量也可以正确执行，结果只要关注实际数量即可




#if NET6_0_OR_GREATER
using static System.MathF;
#else
    using static Vorcyc.Offlet.Math.VMath;
#endif

internal static class FastFourierTransformNormal
{

    private const float PI = 3.14159265358979323846f;

    private const float Negative_PI = -3.14159265358979323846f;

    #region private methods

    //R to C
    private static unsafe void Rearrange(float* input, ComplexFp32* output, int N)
    {
        //  data entry position
        int target = 0;
        //   Process all positions of input signal
        for (int position = 0; position < N; ++position)
        {
            //  Set data entry
            output[target] = input[position];
            //  Bit mask
            int mask = N;
            //  While bit is set
            while ((target & (mask >>= 1)) != 0)
                //  Drop bit
                target &= ~mask;
            //  The current bit is 0 - set it
            target |= mask;
        }
    }


    private static void Rearrange(ReadOnlySpan<float> input, Span<ComplexFp32> output)
    {
        //  data entry position
        int target = 0;
        //   Process all positions of input signal
        for (int position = 0; position < input.Length; ++position)
        {
            //  Set data entry
            output[target] = input[position];
            //  Bit mask
            int mask = input.Length;
            //  While bit is set
            while ((target & (mask >>= 1)) != 0)
                //  Drop bit
                target &= ~mask;
            //  The current bit is 0 - set it
            target |= mask;
        }
    }


    //C to C
    private static unsafe void Rearrange(ComplexFp32* input, ComplexFp32* output, int N)
    {
        //   data entry position
        int target = 0;
        //   Process all positions of input signal
        for (int position = 0; position < N; ++position)
        {
            //  Set data entry
            output[target] = input[position];
            //   Bit mask
            int mask = N;
            //   While bit is set
            while ((target & (mask >>= 1)) != 0)
                //   Drop bit
                target &= ~mask;
            //   The current bit is 0 - set it
            target |= mask;
        }
    }


    private static void Rearrange(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output)
    {
        //   data entry position
        int target = 0;
        //   Process all positions of input signal
        for (int position = 0; position < input.Length; ++position)
        {
            //  Set data entry
            output[target] = input[position];
            //   Bit mask
            int mask = input.Length;
            //   While bit is set
            while ((target & (mask >>= 1)) != 0)
                //   Drop bit
                target &= ~mask;
            //   The current bit is 0 - set it
            target |= mask;
        }
    }


    //C to C , inplace
    private static unsafe void Rearrange(ComplexFp32* data, int N)
    {
        //   Swap position
        int target = 0;
        //   Process all positions of input signal
        for (int position = 0; position < N; ++position)
        {
            //   Only for not yet swapped entries
            if (target > position)
            {
                //   Swap entries
                ComplexFp32 temp = (data[target]);
                data[target] = data[position];
                data[position] = temp;
            }
            //   Bit mask
            int mask = N;
            //   While bit is set
            while ((target & (mask >>= 1)) != 0)
                //   Drop bit
                target &= ~mask;
            //   The current bit is 0 - set it
            target |= mask;
        }
    }


    private static void Rearrange(Span<ComplexFp32> data)
    {
        //   Swap position
        int target = 0;
        //   Process all positions of input signal
        for (int position = 0; position < data.Length; ++position)
        {
            //   Only for not yet swapped entries
            if (target > position)
            {
                //   Swap entries
                ComplexFp32 temp = (data[target]);
                data[target] = data[position];
                data[position] = temp;
            }
            //   Bit mask
            int mask = data.Length;
            //   While bit is set
            while ((target & (mask >>= 1)) != 0)
                //   Drop bit
                target &= ~mask;
            //   The current bit is 0 - set it
            target |= mask;
        }
    }


    //FFT implementation
    private static unsafe void Perform(ComplexFp32* data, int N, bool inverse = false)
    {
        float pi = inverse ? PI : Negative_PI;
        //   Iteration through dyads, quadruples, octads and so on...
        for (int step = 1; step < N; step <<= 1)
        {
            //   Jump to the next entry of the same transform factor
            int jump = step << 1;
            //   Angle increment
            float delta = pi / (float)step;
            //   Auxiliary sin(delta / 2)
            float sine = Sin(delta * .5f);
            //   Multiplier for trigonometric recurrence
            ComplexFp32 multiplier = new(-2.0f * sine * sine, Sin(delta));
            //   Start value for transform factor, fi = 0
            ComplexFp32 factor = ComplexFp32.One;// new ComplexFP32(1.0f);
                                                 //   Iteration through groups of different transform factor
            for (int group = 0; group < step; ++group)
            {
                //   Iteration within group 
                for (int pair = group; pair < N; pair += jump)
                {
                    //   Match position
                    int match = pair + step;
                    //   Second term of two-point transform
                    ComplexFp32 product = factor * data[match];
                    //   Transform for fi + pi
                    data[match] = data[pair] - product;
                    //   Transform for fi
                    data[pair] += product;
                }
                //   Successive transform factor via trigonometric recurrence
                factor = multiplier * factor + factor;
            }
        }
    }


    private static void Perform(Span<ComplexFp32> data, bool inverse = false)
    {
        float pi = inverse ? PI : Negative_PI;
        //   Iteration through dyads, quadruples, octads and so on...
        for (int step = 1; step < data.Length; step <<= 1)
        {
            //   Jump to the next entry of the same transform factor
            int jump = step << 1;
            //   Angle increment
            float delta = pi / (float)step;
            //   Auxiliary sin(delta / 2)
            float sine = Sin(delta * .5f);
            //   Multiplier for trigonometric recurrence
            ComplexFp32 multiplier = new ComplexFp32(-2.0f * sine * sine, Sin(delta));
            //   Start value for transform factor, fi = 0
            ComplexFp32 factor = ComplexFp32.One;// new ComplexFP32(1.0f);
                                                 //   Iteration through groups of different transform factor
            for (int group = 0; group < step; ++group)
            {
                //   Iteration within group 
                for (int pair = group; pair < data.Length; pair += jump)
                {
                    //   Match position
                    int match = pair + step;
                    //   Second term of two-point transform
                    ComplexFp32 product = factor * data[match];
                    //   Transform for fi + pi
                    data[match] = data[pair] - product;
                    //   Transform for fi
                    data[pair] += product;
                }
                //   Successive transform factor via trigonometric recurrence
                factor = multiplier * factor + factor;
            }
        }
    }


    //Scaling of inverse FFT result
    private static unsafe void Scale(ComplexFp32* data, int N)
    {
        float factor = 1.0f / N;
        //   Scale all data entries
        for (int position = 0; position < N; ++position)
            data[position] *= factor;
    }


    private static void Scale(Span<ComplexFp32> data)
    {
        float factor = 1.0f / data.Length;
        //   Scale all data entries
        for (int position = 0; position < data.Length; ++position)
            data[position] *= factor;
    }

    #endregion

    #region Forward

    /// <summary>
    /// Forward fast fourier transform , Real-number to Complex-number.
    /// </summary>
    /// <param name="input">input data : real-number sequence (tine-domain)</param>
    /// <param name="output">transform result</param>
    /// <param name="N">FFT length</param>
    /// <returns></returns>
    public static unsafe bool Forward(float* input, ComplexFp32* output, int N)
    {
        //   Check input parameters
        if (input == null || output == null || N < 1 || !N.IsPowerOf2())
            return false;
        //   Initialize data
        Rearrange(input, output, N);
        //   Call FFT implementation
        Perform(output, N);
        //   Succeeded
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

    /// <summary>
    /// Forward fast fourier transform , Real-number to Complex-number.
    /// </summary>
    /// <param name="input">input data : real-number sequence (tine-domain)</param>
    /// <param name="output">transform result</param>
    /// <param name="N">FFT length</param>
    /// <returns></returns>
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
        //   Check input parameters
        if (input == null || output == null || input.Length < 1 || !input.Length.IsPowerOf2())
            return false;
        //   Initialize data
        Rearrange(input, output);
        //   Call FFT implementation
        Perform(output);
        //   Succeeded
        return true;
    }



    /// <summary>
    /// Forward fast fourier transform , Complex-number to Complex-number.
    /// </summary>
    /// <param name="input">input data : complex-number sequence(time-domain)</param>
    /// <param name="output">transform result</param>
    /// <param name="N">FFT length</param>
    /// <returns></returns>
    public static unsafe bool Forward(ComplexFp32* input, ComplexFp32* output, int N)
    {
        //   Check input parameters
        if (input == null || output == null || N < 1 || !N.IsPowerOf2())
            return false;
        //   Initialize data
        Rearrange(input, output, N);
        //   Call FFT implementation
        Perform(output, N);
        //   Succeeded
        return true;
    }


    public static unsafe bool Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output)
    {
        var N = input.Length;
        //   Check input parameters
        if (input == null || output == null || N < 1 || !N.IsPowerOf2())
            return false;
        //   Initialize data
        Rearrange(input, output);
        //   Call FFT implementation
        Perform(output);
        //   Succeeded
        return true;
    }



    /// <summary>
    /// Forward fast fourier transform , Complex-number to Complex-number.
    /// </summary>
    /// <param name="input">input data : complex-number sequence(time-domain)</param>
    /// <param name="offset">offset</param>
    /// <param name="output">transform result</param>
    /// <param name="N">FFT length</param>
    /// <returns></returns>
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

    /// <summary>
    /// Forward fast fourier transform , Inplace Version.
    /// </summary>
    /// <param name="data">input and result</param>
    /// <param name="N">FFT length</param>
    /// <returns></returns>
    public static unsafe bool Forward(ComplexFp32* data, int N)
    {
        //   Check input parameters
        if (data == null || N < 1 || !N.IsPowerOf2())
            return false;
        //   Rearrange
        Rearrange(data, N);
        //   Call FFT implementation
        Perform(data, N);
        //   Succeeded
        return true;
    }



    /// <summary>
    /// Forward fast fourier transform , Inplace Version.
    /// </summary>
    /// <param name="data">input and result</param>
    /// <param name="offset"></param>
    /// <param name="N">>FFT length</param>
    /// <returns></returns>
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
        var N = data.Length;
        //   Check input parameters
        if (data == null || N < 1 || !N.IsPowerOf2())
            return false;
        //   Rearrange
        Rearrange(data);
        //   Call FFT implementation
        Perform(data);
        //   Succeeded
        return true;
    }


    #endregion

    #region Inverse

    /// <summary>
    /// Inverse fast fourier transform , Complex-number to complex-number.
    /// </summary>
    /// <param name="input">input : frequency-domain data</param>
    /// <param name="output">the result of time-domain data</param>
    /// <param name="N">the input length</param>
    /// <param name="scale">determine if scale</param>
    /// <returns></returns>
    public static unsafe bool Inverse(ComplexFp32* input, ComplexFp32* output, int N, bool scale = true)
    {
        //   Check input parameters
        if (input == null || output == null || N < 1 || !N.IsPowerOf2())
            return false;
        //   Initialize data
        Rearrange(input, output, N);
        //   Call FFT implementation
        Perform(output, N, true);
        //   Scale if necessary
        if (scale)
            Scale(output, N);
        //   Succeeded
        return true;
    }

    /// <summary>
    /// Inverse fast fourier transform , Complex-number to complex-number.
    /// </summary>
    /// <param name="input">input : frequency-domain data</param>
    /// <param name="output">the result of time-domain data</param>
    /// <param name="N">the input length</param>
    /// <param name="scale">determine if scale</param>
    /// <returns></returns>
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


    public static unsafe bool Inverse(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, bool scale = true)
    {
        var N = input.Length;
        //   Check input parameters
        if (input == null || output == null || N < 1 || !N.IsPowerOf2())
            return false;
        //   Initialize data
        Rearrange(input, output);
        //   Call FFT implementation
        Perform(output, true);
        //   Scale if necessary
        if (scale)
            Scale(output);
        //   Succeeded
        return true;
    }


    /// <summary>
    /// Inverse fast fourier transform , Inplace Version
    /// </summary>
    /// <param name="data">frequency-domain to time-domain</param>
    /// <param name="N">the input length</param>
    /// <param name="scale">determine if scale</param>
    /// <returns></returns>
    public static unsafe bool Inverse(ComplexFp32* data, int N, bool scale = true)
    {
        //   Check input parameters
        if (data == null || N < 1 || !N.IsPowerOf2())
            return false;
        //   Initialize data
        Rearrange(data, N);
        //   Call FFT implementation
        Perform(data, N, true);
        //   Scale if necessary
        if (scale)
            Scale(data, N);
        //   Succeeded
        return true;
    }

    /// <summary>
    /// Inverse fast fourier transform , Inplace Version
    /// </summary>
    /// <param name="data">frequency-domain to time-domain</param>
    /// <param name="N">the input length</param>
    /// <param name="scale">determine if scale</param>
    /// <returns></returns>
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


    public static unsafe bool Inverse(Span<ComplexFp32> data, bool scale = true)
    {
        var N = data.Length;
        //   Check input parameters
        if (data == null || N < 1 || !N.IsPowerOf2())
            return false;
        //   Initialize data
        Rearrange(data);
        //   Call FFT implementation
        Perform(data, true);
        //   Scale if necessary
        if (scale)
            Scale(data);
        //   Succeeded
        return true;
    }


    #endregion

}

