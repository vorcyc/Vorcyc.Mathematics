using ILGPU;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System.Diagnostics.Contracts;
using System.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace core_module_test;

internal class CuFFT_test
{

    public static void go()
    {
        using var context = Context.Create(builder => builder.Cuda());
        foreach (var device in context)
        {
            using var accelerator = device.CreateAccelerator(context) as CudaAccelerator;
            Console.WriteLine($"Performing operations on {accelerator}");

            var input = new Complex[8];
            for (var i = 0; i < input.Length; i++)
                input[i] = new Complex(i + 1, 0);

            Console.WriteLine("Input Values:");
            for (var i = 0; i < input.Length; i++)
                Console.WriteLine($"  [{i}] = {input[i]}");

            var cufft = new CuFFT();
            CUDA_FFT.Forward(accelerator, cufft, input, out var output);

            var inverseInput = output;
            var inverseOutput = new Complex[inverseInput.Length];
            CUDA_FFT.Inverse(accelerator, cufft.API, inverseInput, out _);


        }
    }


    public static void my()
    {
        using var cufft = new CudaFFT();

        var input = new float[8];
        for (var i = 0; i < input.Length; i++)
            input[i] = i + 1;

        Console.WriteLine("Input Values:");
        for (var i = 0; i < input.Length; i++)
            Console.WriteLine($"  [{i}] = {input[i]}");

        cufft.Forward(input, out var output);
        Console.WriteLine("Output Values:");
        for (var i = 0; i < output.Length; i++)
            Console.WriteLine($"  [{i}] = {output[i]}");


        cufft.Inverse(output, out var inverseOutput);
        // Scale the output to obtain the inverse.
        for (var i = 0; i < inverseOutput.Length; i++)
            inverseOutput[i] /= inverseOutput.Length;

        Console.WriteLine("Inverse Values:");
        for (var i = 0; i < inverseOutput.Length; i++)
            Console.WriteLine($"  [{i}] = {inverseOutput[i].Real}");
    }



    //public static void clfft()
    //{

    //    // 创建上下文和加速器
    //    using var context = Context.Create(builder => builder.CPU().OpenCL());
    //    using var accelerator = context.CreateCLAccelerator(0);

    //    // 输入数据
    //    var data = new Complex[] { new Complex(1, 0), new Complex(2, 0), new Complex(3, 0), new Complex(4, 0) };
    //    using var buffer = accelerator.Allocate1D(data);

    //    // 执行 FFT
    //    ExecuteFFT(accelerator, buffer.View);

    //    // 获取结果
    //    var result = buffer.GetAsArray1D();
    //    foreach (var value in result)
    //    {
    //        Console.WriteLine(value);
    //    }

    //    static void ExecuteFFT(Accelerator accelerator, ArrayView1D<Complex, Stride1D.Dense> data)
    //    {
    //        // 这里你需要实现 FFT 的 OpenCL 内核
    //        // 这是一个简单的示例内核，实际的 FFT 内核会更复杂
    //        string kernelSource = @"
    //    __kernel void fft(__global float2* data) {
    //        int id = get_global_id(0);
    //        // 简单的 FFT 计算示例
    //        data[id].x = data[id].x * 2.0f;
    //        data[id].y = data[id].y * 2.0f;
    //    }";

    //        // 编译内核
    //        var program = accelerator.createp(kernelSource);
    //        var kernel = program.CreateKernel("fft");

    //        // 执行内核
    //        kernel((int)data.Length, data);
    //    }
    //}

}
