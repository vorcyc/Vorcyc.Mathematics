using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.Cuda.API;

namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

internal static class CudaFastFourierTransform
{
    #region Helper Methods

    private static bool IsPowerOf2(int n)
    {
        return n > 0 && (n & (n - 1)) == 0;
    }

    private static void WorkaroundKnownIssue(CudaAccelerator accelerator, CuFFTAPI api)
    {
        CuFFTException.ThrowIfFailed(
            api.GetProperty(LibraryPropertyType.MAJOR_VERSION, out var major));
        CuFFTException.ThrowIfFailed(
            api.GetProperty(LibraryPropertyType.MINOR_VERSION, out var minor));
        if (major == 10 && minor >= 4)
        {
            CudaException.ThrowIfFailed(
                CudaAPI.CurrentAPI.SetCurrentContext(accelerator.NativePtr));
        }
    }

    #endregion

    #region Forward

    /// <summary>
    /// Forward fast fourier transform, Real-number to Complex-number.
    /// </summary>
    /// <param name="input">input data: real-number sequence (time-domain)</param>
    /// <param name="offset">input array offset</param>
    /// <param name="output">transform result</param>
    /// <param name="N">FFT length</param>
    /// <returns></returns>
    public static bool Forward(float[] input, int offset, out ComplexFp32[] output, int N)
    {
        output = null;
        // Check input parameters
        if (input == null || offset < 0 || offset + N > input.Length || N < 1 || !IsPowerOf2(N))
            return false;

        using var context = Context.Create(builder => builder.Cuda());
        foreach (var device in context)
        {
            using var accelerator = device.CreateAccelerator(context) as CudaAccelerator;
            using var stream = accelerator.CreateStream() as CudaStream;
            var cufft = new CuFFT();

            try
            {
                // Allocate device memory
                using var inputBuffer = accelerator.Allocate1D(input.AsSpan(offset, N).ToArray());
                using var outputBuffer = accelerator.Allocate1D<(float real, float imaginary)>(N / 2 + 1);

                // Create and execute FFT plan
                CuFFTException.ThrowIfFailed(
                    cufft.Plan1D(
                        out var plan,
                        N,
                        CuFFTType.CUFFT_R2C,
                        batch: 1));
                using (plan)
                {
                    plan.SetStream(stream);
                    CuFFTException.ThrowIfFailed(
                        plan.ExecR2C(
                            inputBuffer.View.BaseView,
                            outputBuffer.View.BaseView));
                }

                // Copy result to host
                var tempOutput = outputBuffer.GetAsArray1D(stream);
                output = new ComplexFp32[N / 2 + 1];
                for (int i = 0; i < N / 2 + 1; i++)
                    output[i] = new ComplexFp32(tempOutput[i].real, tempOutput[i].imaginary);

                WorkaroundKnownIssue(accelerator, cufft.API);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Forward fast fourier transform, Complex-number to Complex-number.
    /// </summary>
    /// <param name="input">input data: complex-number sequence (time-domain)</param>
    /// <param name="offset">input array offset</param>
    /// <param name="output">transform result</param>
    /// <param name="N">FFT length</param>
    /// <returns></returns>
    public static bool Forward(ComplexFp32[] input, int offset, ComplexFp32[] output, int N)
    {
        // Check input parameters
        if (input == null || output == null || offset < 0 || offset + N > input.Length || output.Length < N || N < 1 || !IsPowerOf2(N))
            return false;

        using var context = Context.Create(builder => builder.Cuda());
        foreach (var device in context)
        {
            using var accelerator = device.CreateAccelerator(context) as CudaAccelerator;
            using var stream = accelerator.CreateStream() as CudaStream;
            var cufft = new CuFFT();

            try
            {
                // Allocate device memory
                var inputTuples = new (float real, float imaginary)[N];
                for (int i = 0; i < N; i++)
                    inputTuples[i] = (input[offset + i].Real, input[offset + i].Imaginary);
                using var inputBuffer = accelerator.Allocate1D(inputTuples);
                using var outputBuffer = accelerator.Allocate1D<(float real, float imaginary)>(N);

                // Create and execute FFT plan
                CuFFTException.ThrowIfFailed(
                    cufft.Plan1D(
                        out var plan,
                        N,
                        CuFFTType.CUFFT_C2C,
                        batch: 1));
                using (plan)
                {
                    plan.SetStream(stream);
                    CuFFTException.ThrowIfFailed(
                        plan.ExecC2C(
                            inputBuffer.View.BaseView,
                            outputBuffer.View.BaseView,
                            CuFFTDirection.FORWARD));
                }

                // Copy result to host
                var tempOutput = outputBuffer.GetAsArray1D(stream);
                for (int i = 0; i < N; i++)
                    output[i] = new ComplexFp32(tempOutput[i].real, tempOutput[i].imaginary);

                WorkaroundKnownIssue(accelerator, cufft.API);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Forward fast fourier transform, Inplace Version.
    /// </summary>
    /// <param name="data">input and result</param>
    /// <param name="offset">data array offset</param>
    /// <param name="N">FFT length</param>
    /// <returns></returns>
    public static bool Forward(ComplexFp32[] data, int offset, int N)
    {
        // Check input parameters
        if (data == null || offset < 0 || offset + N > data.Length || N < 1 || !IsPowerOf2(N))
            return false;

        using var context = Context.Create(builder => builder.Cuda());
        foreach (var device in context)
        {
            using var accelerator = device.CreateAccelerator(context) as CudaAccelerator;
            using var stream = accelerator.CreateStream() as CudaStream;
            var cufft = new CuFFT();

            try
            {
                // Allocate device memory
                var inputTuples = new (float real, float imaginary)[N];
                for (int i = 0; i < N; i++)
                    inputTuples[i] = (data[offset + i].Real, data[offset + i].Imaginary);
                using var buffer = accelerator.Allocate1D(inputTuples);

                // Create and execute FFT plan
                CuFFTException.ThrowIfFailed(
                    cufft.Plan1D(
                        out var plan,
                        N,
                        CuFFTType.CUFFT_C2C,
                        batch: 1));
                using (plan)
                {
                    plan.SetStream(stream);
                    CuFFTException.ThrowIfFailed(
                        plan.ExecC2C(
                            buffer.View.BaseView,
                            buffer.View.BaseView,
                            CuFFTDirection.FORWARD));
                }

                // Copy result back to host
                var tempOutput = buffer.GetAsArray1D(stream);
                for (int i = 0; i < N; i++)
                    data[offset + i] = new ComplexFp32(tempOutput[i].real, tempOutput[i].imaginary);

                WorkaroundKnownIssue(accelerator, cufft.API);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    #endregion

    #region Inverse

    /// <summary>
    /// Inverse fast fourier transform, Complex-number to complex-number.
    /// </summary>
    /// <param name="input">input: frequency-domain data</param>
    /// <param name="inOffset">input array offset</param>
    /// <param name="output">the result of time-domain data</param>
    /// <param name="outOffset">output array offset</param>
    /// <param name="N">the input length</param>
    /// <param name="scale">determine if scale</param>
    /// <returns></returns>
    public static bool Inverse(ComplexFp32[] input, int inOffset, out ComplexFp32[] output, int outOffset, int N, bool scale = true)
    {
        output = new ComplexFp32[N];
        // Check input parameters
        if (input == null || inOffset < 0 || inOffset + N > input.Length || outOffset < 0 || outOffset + N > output.Length || N < 1 || !IsPowerOf2(N))
            return false;

        using var context = Context.Create(builder => builder.Cuda());
        foreach (var device in context)
        {
            using var accelerator = device.CreateAccelerator(context) as CudaAccelerator;
            using var stream = accelerator.CreateStream() as CudaStream;
            var cufft = new CuFFT();

            try
            {
                // Allocate device memory
                var inputTuples = new (float real, float imaginary)[N];
                for (int i = 0; i < N; i++)
                    inputTuples[i] = (input[inOffset + i].Real, input[inOffset + i].Imaginary);
                using var inputBuffer = accelerator.Allocate1D(inputTuples);
                using var outputBuffer = accelerator.Allocate1D<(float real, float imaginary)>(N);

                // Create and execute FFT plan
                CuFFTException.ThrowIfFailed(
                    cufft.Plan1D(
                        out var plan,
                        N,
                        CuFFTType.CUFFT_C2C,
                        batch: 1));
                using (plan)
                {
                    plan.SetStream(stream);
                    CuFFTException.ThrowIfFailed(
                        plan.ExecC2C(
                            inputBuffer.View.BaseView,
                            outputBuffer.View.BaseView,
                            CuFFTDirection.INVERSE));
                }

                // Copy result to host
                var tempOutput = outputBuffer.GetAsArray1D(stream);
                for (int i = 0; i < N; i++)
                {
                    var value = new ComplexFp32(tempOutput[i].real, tempOutput[i].imaginary);
                    output[outOffset + i] = scale ? value / N : value;
                }

                WorkaroundKnownIssue(accelerator, cufft.API);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Inverse fast fourier transform, Inplace Version.
    /// </summary>
    /// <param name="data">frequency-domain to time-domain</param>
    /// <param name="offset">data array offset</param>
    /// <param name="N">the input length</param>
    /// <param name="scale">determine if scale</param>
    /// <returns></returns>
    public static bool Inverse(ComplexFp32[] data, int offset, int N, bool scale = true)
    {
        // Check input parameters
        if (data == null || offset < 0 || offset + N > data.Length || N < 1 || !IsPowerOf2(N))
            return false;

        using var context = Context.Create(builder => builder.Cuda());
        foreach (var device in context)
        {
            using var accelerator = device.CreateAccelerator(context) as CudaAccelerator;
            using var stream = accelerator.CreateStream() as CudaStream;
            var cufft = new CuFFT();

            try
            {
                // Allocate device memory
                var inputTuples = new (float real, float imaginary)[N];
                for (int i = 0; i < N; i++)
                    inputTuples[i] = (data[offset + i].Real, data[offset + i].Imaginary);
                using var buffer = accelerator.Allocate1D(inputTuples);

                // Create and execute FFT plan
                CuFFTException.ThrowIfFailed(
                    cufft.Plan1D(
                        out var plan,
                        N,
                        CuFFTType.CUFFT_C2C,
                        batch: 1));
                using (plan)
                {
                    plan.SetStream(stream);
                    CuFFTException.ThrowIfFailed(
                        plan.ExecC2C(
                            buffer.View.BaseView,
                            buffer.View.BaseView,
                            CuFFTDirection.INVERSE));
                }

                // Copy result back to host
                var tempOutput = buffer.GetAsArray1D(stream);
                for (int i = 0; i < N; i++)
                {
                    var value = new ComplexFp32(tempOutput[i].real, tempOutput[i].imaginary);
                    data[offset + i] = scale ? value / N : value;
                }

                WorkaroundKnownIssue(accelerator, cufft.API);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    #endregion
}