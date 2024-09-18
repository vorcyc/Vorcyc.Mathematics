﻿using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.Cuda.API;
using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

public static class CUDA_FFT
{



    // Use the CuFFT wrapper to perform a forward transform.
    public static void Forward(
        CudaAccelerator accelerator,
        CuFFT cufft,
        Complex[] input,
        out Complex[] output)
    {
        using var stream = accelerator.CreateStream() as CudaStream;
        using var inputBuffer = accelerator.Allocate1D(input);
        using var outputBuffer = accelerator.Allocate1D<Complex>(input.Length);

        CuFFTException.ThrowIfFailed(
            cufft.Plan1D(
                out var plan,
                input.Length,
                CuFFTType.CUFFT_Z2Z,
                batch: 1));
        using (plan)
        {
            plan.SetStream(stream);
            CuFFTException.ThrowIfFailed(
                plan.ExecZ2Z(
                    inputBuffer.View.BaseView,
                    outputBuffer.View.BaseView,
                    CuFFTDirection.FORWARD));

            output = outputBuffer.GetAsArray1D(stream);
        }
        WorkaroundKnownIssue(accelerator, cufft.API);

        Console.WriteLine("Output Values:");
        for (var i = 0; i < output.Length; i++)
            Console.WriteLine($"  [{i}] = {output[i]}");
    }

    // Use the low-level CuFFT API to perform an inverse transform.
    public static void Inverse(
        CudaAccelerator accelerator,
        CuFFTAPI api,
        Complex[] input,
        out Complex[] output)
    {
        using var stream = accelerator.CreateStream() as CudaStream;
        using var inputBuffer = accelerator.Allocate1D(input);
        using var outputBuffer = accelerator.Allocate1D<Complex>(input.Length);

        CuFFTException.ThrowIfFailed(
            api.Plan1D(
                out var plan,
                input.Length,
                CuFFTType.CUFFT_Z2Z,
                batch: 1));
        try
        {
            CuFFTException.ThrowIfFailed(
                api.SetStream(plan, stream));
            CuFFTException.ThrowIfFailed(
                api.ExecZ2Z(
                    plan,
                    inputBuffer.View.BaseView,
                    outputBuffer.View.BaseView,
                    CuFFTDirection.INVERSE));

            output = outputBuffer.GetAsArray1D(stream);
        }
        finally
        {
            CuFFTException.ThrowIfFailed(
                api.Destroy(plan));
        }
        WorkaroundKnownIssue(accelerator, api);

        // Scale the output to obtain the inverse.
        for (var i = 0; i < output.Length; i++)
            output[i] /= output.Length;

        Console.WriteLine("Inverse Values:");
        for (var i = 0; i < output.Length; i++)
            Console.WriteLine($"  [{i}] = {output[i].Real}");
    }


    static void WorkaroundKnownIssue(CudaAccelerator accelerator, CuFFTAPI api)
    {
        // The CUDA release notes for 11.2 to 11.3 (inclusive) contains a known issue:
        // - cuFFT planning and plan estimation functions may not restore correct
        //   context affecting CUDA driver API applications.
        //
        // This workaround restores the accelerator context so that deallocation of
        // the memory buffers can be performed on the correct context.
        //
        // Based on the versions of CuFFT released, we would need to apply the
        // workaround to CuFFT v10.4.x.
        //
        // Release 11.1.1   CuFFT v10.3.0.105
        // Release 11.2     CuFFT v10.4.0.72
        // Release 11.3     CuFFT v10.4.2.58
        // Release 11.4     CuFFT v10.5.0.43
        //
        // However, based on actual testing, the issue still persists in later
        // versions. It appears to have been fixed in Release 12.0, which ships
        // with CuFFT v11. So, we will apply the workaround from v10.4.x and later
        // versions, up to v11 (exclusive).
        //
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

}
