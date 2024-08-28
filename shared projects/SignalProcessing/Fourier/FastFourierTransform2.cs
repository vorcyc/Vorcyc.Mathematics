//namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

///* duan linli aka cyclone_dll
// * 19.11.5
// * VORCYC CO,.LTD
// */


////var input = new float[10] { -1, 2, -3, 4, -5, 6, -7, 8, -9, 10 };//, -11, 12, -13, 14, -15, 16 };
////FastFourierTransform.Forward(input, 0, out ComplexFP32[] output, 16);


////            foreach (var x in output)
////                Console.WriteLine(x);

////            Console.WriteLine("----------------");

////            FastFourierTransform.Inverse(output, 0, 16);

////            foreach (var x in output)
////                Console.WriteLine(x);

////不给足够的实际数组量也可以正确执行，结果只要关注实际数量即可


//#if NET6_0_OR_GREATER
//using static System.MathF;
//#else
//    using static Vorcyc.Offlet.Math.VMath;
//#endif

//public static class FastFourierTransform2
//{


//    private const float PI = 3.14159265358979323846f;

//    private const float Negative_PI = -3.14159265358979323846f;

//    #region Rearrange

//    /// <summary>
//    /// R to C , with allocated memory.
//    /// </summary>
//    /// <param name="input"></param>
//    /// <param name="output"></param>
//    private static void Rearrange(ReadOnlySpan<float> input, Span<ComplexFp32> output)
//    {        
//        //  data entry position
//        int target = 0;
//        //   Process all positions of input signal
//        for (int position = 0; position < input.Length; ++position)
//        {
//            //  Set data entry
//            output[target] = input[position];
//            //  Bit mask
//            int mask = input.Length;
//            //  While bit is set
//            while ((target & (mask >>= 1)) != 0)
//                //  Drop bit
//                target &= ~mask;
//            //  The current bit is 0 - set it
//            target |= mask;
//        }
//    }

//    /// <summary>
//    /// C to C , with allocated memory.
//    /// </summary>
//    /// <param name="input"></param>
//    /// <param name="output"></param>
//    private static void Rearrange(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output)
//    {
//        //   data entry position
//        int target = 0;
//        //   Process all positions of input signal
//        for (int position = 0; position < input.Length; ++position)
//        {
//            //  Set data entry
//            output[target] = input[position];
//            //   Bit mask
//            int mask = input.Length;
//            //   While bit is set
//            while ((target & (mask >>= 1)) != 0)
//                //   Drop bit
//                target &= ~mask;
//            //   The current bit is 0 - set it
//            target |= mask;
//        }
//    }

//    /// <summary>
//    /// C to C , inplace.
//    /// </summary>
//    /// <param name="data"></param>
//    private static void Rearrange(Span<ComplexFp32> data)
//    {
//        //   Swap position
//        int target = 0;
//        //   Process all positions of input signal
//        for (int position = 0; position < data.Length; ++position)
//        {
//            //   Only for not yet swapped entries
//            if (target > position)
//            {
//                //   Swap entries
//                ComplexFp32 temp = (data[target]);
//                data[target] = data[position];
//                data[position] = temp;
//            }
//            //   Bit mask
//            int mask = data.Length;
//            //   While bit is set
//            while ((target & (mask >>= 1)) != 0)
//                //   Drop bit
//                target &= ~mask;
//            //   The current bit is 0 - set it
//            target |= mask;
//        }
//    }

//    #endregion


//    #region FFT implementation

//    /// <summary>
//    /// FFT implementation
//    /// </summary>
//    /// <param name="data"></param>
//    /// <param name="inverse"></param>
//    private static void Perform(Span<ComplexFp32> data, bool inverse = false)
//    {
//        float pi = inverse ? PI : Negative_PI;
//        //   Iteration through dyads, quadruples, octads and so on...
//        for (int step = 1; step < data.Length; step <<= 1)
//        {
//            //   Jump to the next entry of the same transform factor
//            int jump = step << 1;
//            //   Angle increment
//            float delta = pi / (float)step;
//            //   Auxiliary sin(delta / 2)
//            float sine = Sin(delta * .5f);
//            //   Multiplier for trigonometric recurrence
//            ComplexFp32 multiplier = new ComplexFp32(-2.0f * sine * sine, Sin(delta));
//            //   Start value for transform factor, fi = 0
//            ComplexFp32 factor = ComplexFp32.One;// new ComplexFP32(1.0f);
//                                                 //   Iteration through groups of different transform factor
//            for (int group = 0; group < step; ++group)
//            {
//                //   Iteration within group 
//                for (int pair = group; pair < data.Length; pair += jump)
//                {
//                    //   Match position
//                    int match = pair + step;
//                    //   Second term of two-point transform
//                    ComplexFp32 product = factor * data[match];
//                    //   Transform for fi + pi
//                    data[match] = data[pair] - product;
//                    //   Transform for fi
//                    data[pair] += product;
//                }
//                //   Successive transform factor via trigonometric recurrence
//                factor = multiplier * factor + factor;
//            }
//        }
//    }

//    #endregion


//    #region Scaling of inverse FFT result

//    /// <summary>
//    /// Scaling of inverse FFT result
//    /// </summary>
//    /// <param name="data"></param>
//    private static void Scale(Span<ComplexFp32> data)
//    {
//        float factor = 1.0f / data.Length;
//        //   Scale all data entries
//        for (int position = 0; position < data.Length; ++position)
//            data[position] *= factor;
//    }


//    #endregion


//    #region Forward

//    /// <summary>
//    /// Peform the Forward fast fourier transform , Real-number to Complex-number.
//    /// <para>
//    /// In allocated memory buffer.
//    /// </para>
//    /// </summary>
//    /// <param name="input">input data : real-number sequence (tine-domain)</param>
//    /// <param name="output">transform result</param>
//    /// <returns>true if success , otherwise false.</returns>
//    public static bool Forward(ReadOnlySpan<float> input, Span<ComplexFp32> output)
//    {
//        //   Check input parameters
//        if (input == null || output == null || input.Length < 1 || !input.Length.IsPowerOf2())
//            return false;
//        //   Initialize data
//        Rearrange(input, output);
//        //   Call FFT implementation
//        Perform(output);
//        //   Succeeded
//        return true;
//    }

//    /// <summary>
//    /// Peform the Forward fast fourier transform , Real-number to Complex-number.
//    /// <para>
//    /// Allocating new memory buffer.
//    /// </para>
//    /// </summary>
//    /// <param name="input"></param>
//    /// <param name="output"></param>
//    /// <returns>true if success , otherwise false.</returns>
//    public static bool Forward(ReadOnlySpan<float> input, out Span<ComplexFp32> output)
//    {
//        var outputArray = new ComplexFp32[input.Length];
//        var success = Forward(input, outputArray);
//        output = outputArray;
//        return success;
//    }

//    /// <summary>
//    /// Forward fast fourier transform , Complex-number to Complex-number.
//    /// <para>
//    /// In allocated memory buffer.
//    /// </para>
//    /// </summary>
//    /// <param name="input">input data : complex-number sequence(time-domain)</param>
//    /// <param name="output">transform result</param>
//    /// <returns>true if success , otherwise false.</returns>
//    public static bool Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output)
//    {
//        var N = input.Length;
//        //   Check input parameters
//        if (input == null || output == null || N < 1 || !N.IsPowerOf2())
//            return false;
//        //   Initialize data
//        Rearrange(input, output);
//        //   Call FFT implementation
//        Perform(output);
//        //   Succeeded
//        return true;
//    }


//    /// <summary>
//    /// Peform the Forward fast fourier transform , Real-number to Complex-number.
//    /// <para>
//    /// Inplace version.
//    /// </para>
//    /// </summary>
//    /// <param name="data"></param>
//    /// <returns></returns>
//    public static bool Forward(Span<ComplexFp32> data)
//    {
//        var N = data.Length;
//        //   Check input parameters
//        if (data == null || N < 1 || !N.IsPowerOf2())
//            return false;
//        //   Rearrange
//        Rearrange(data);
//        //   Call FFT implementation
//        Perform(data);
//        //   Succeeded
//        return true;
//    }


//    #endregion


//    #region Inverse

//    /// <summary>
//    /// Inverse fast fourier transform , Complex-number to complex-number.
//    /// </summary>
//    /// <param name="input">input : frequency-domain data</param>
//    /// <param name="output">the result of time-domain data</param>
//    /// <param name="scale">determine if scale</param>
//    /// <returns></returns>
//    public static bool Inverse(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, bool scale = true)
//    {
//        var N = input.Length;
//        //   Check input parameters
//        if (input == null || output == null || N < 1 || !N.IsPowerOf2())
//            return false;
//        //   Initialize data
//        Rearrange(input, output);
//        //   Call FFT implementation
//        Perform(output, true);
//        //   Scale if necessary
//        if (scale)
//            Scale(output);
//        //   Succeeded
//        return true;
//    }

//    /// <summary>
//    /// Inverse fast fourier transform , Inplace Version
//    /// </summary>
//    /// <param name="data">frequency-domain to time-domain</param>
//    /// <param name="scale">determine if scale</param>
//    /// <returns></returns>
//    public static bool Inverse(Span<ComplexFp32> data, bool scale = true)
//    {
//        var N = data.Length;
//        //   Check input parameters
//        if (data == null || N < 1 || !N.IsPowerOf2())
//            return false;
//        //   Initialize data
//        Rearrange(data);
//        //   Call FFT implementation
//        Perform(data, true);
//        //   Scale if necessary
//        if (scale)
//            Scale(data);
//        //   Succeeded
//        return true;
//    }


//    #endregion
//}
