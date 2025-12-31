namespace DSP_module_test;

using System;
using Vorcyc.Mathematics.Extensions.FFTW;
using Vorcyc.Mathematics.Numerics;

class FFTW_SINGLE
{
    public static void RUN()
    {
        Console.WriteLine("=== 单精度 FFTW 封装功能验证 ===\n");

        Test1D_Complex_RoundTrip();
        Test1D_Real_R2C_C2R();
        Test2D_Complex_RoundTrip();
        Test3D_Complex_Small_RoundTrip();
        TestND_Complex_RoundTrip();
        Test1D_DCT_II_RoundTrip();
        Test2D_DCT_II_RoundTrip();
        TestND_DCT_II_RoundTrip();
        Test1D_R2HC_HC2R_RoundTrip();

        Console.WriteLine("\n单精度测试全部完成。");
        Console.ReadLine();
    }

    static void PrintResult(string name, float maxError, float threshold = 1e-6f)
    {
        string pass = maxError < threshold ? "通过" : "误差较大，需检查";
        Console.WriteLine($"  {name,-30} 最大误差: {maxError,12:E10}  → {pass}");
    }

    static void Test1D_Complex_RoundTrip()
    {
        int N = 512;
        var input = new ComplexFp32[N];
        for (int i = 0; i < N; i++)
            input[i] = MathF.Cos(2 * MathF.PI * i * 7 / N) + 0.6f * MathF.Sin(2 * MathF.PI * i * 23 / N);

        var freq = new ComplexFp32[N];
        var back = new ComplexFp32[N];

        Dft1D.Forward(input, freq);
        Dft1D.Inverse(freq, back);

        float scale = 1.0f / N;
        float maxErr = 0;
        for (int i = 0; i < N; i++)
        {
            float err = (input[i] - back[i] * scale).Magnitude;
            if (err > maxErr) maxErr = err;
        }

        PrintResult("1D C2C round-trip", maxErr);
    }

    static void Test1D_Real_R2C_C2R()
    {
        int N = 1024;
        var real = new float[N];
        for (int i = 0; i < N; i++)
            real[i] = MathF.Sin(2 * MathF.PI * i * 5 / N) + 0.8f * MathF.Cos(2 * MathF.PI * i * 18 / N);

        var spectrum = new ComplexFp32[N / 2 + 1];
        var recovered = new float[N];

        Dft1D.Forward(real, spectrum);
        Dft1D.Inverse(spectrum, recovered, scale: true); // 1/N 自动缩放

        float maxDiff = 0;
        for (int i = 0; i < N; i++)
        {
            float diff = MathF.Abs(real[i] - recovered[i]);
            if (diff > maxDiff) maxDiff = diff;
        }

        PrintResult("1D R2C → C2R (scale=true)", maxDiff);
    }

    static void Test2D_Complex_RoundTrip()
    {
        int nx = 64, ny = 64;
        var input = new ComplexFp32[nx * ny];
        for (int y = 0; y < ny; y++)
            for (int x = 0; x < nx; x++)
                input[y * nx + x] = MathF.Sin(2 * MathF.PI * x * 4 / nx) * MathF.Cos(2 * MathF.PI * y * 6 / ny);

        var freq = new ComplexFp32[input.Length];
        var back = new ComplexFp32[input.Length];

        Dft2D.Forward(input, freq, nx, ny);
        Dft2D.Inverse(freq, back, nx, ny);

        float scale = 1.0f / (nx * ny);
        float maxErr = 0;
        for (int i = 0; i < input.Length; i++)
        {
            float err = (input[i] - back[i] * scale).Magnitude;
            if (err > maxErr) maxErr = err;
        }

        PrintResult("2D C2C round-trip", maxErr, 1e-6f);
    }

    static void Test3D_Complex_Small_RoundTrip()
    {
        int nx = 16, ny = 16, nz = 8;
        int total = nx * ny * nz;
        var input = new ComplexFp32[total];

        for (int z = 0; z < nz; z++)
            for (int y = 0; y < ny; y++)
                for (int x = 0; x < nx; x++)
                {
                    float v = MathF.Sin(2 * MathF.PI * x / nx * 2) + MathF.Cos(2 * MathF.PI * y / ny * 3);
                    input[(z * ny + y) * nx + x] = new ComplexFp32(v, 0.1f * v);
                }

        var freq = new ComplexFp32[total];
        var back = new ComplexFp32[total];

        Dft3D.Forward(input, freq, nx, ny, nz);
        Dft3D.Inverse(freq, back, nx, ny, nz);

        float scale = 1.0f / total;
        float maxErr = 0;
        for (int i = 0; i < total; i++)
        {
            float err = (input[i] - back[i] * scale).Magnitude;
            if (err > maxErr) maxErr = err;
        }

        PrintResult("3D C2C small round-trip", maxErr, 1e-6f);
    }

    static void TestND_Complex_RoundTrip()
    {
        int[] dims = [8, 12, 10];
        int total = 1;
        foreach (var d in dims) total *= d;

        var input = new ComplexFp32[total];
        for (int i = 0; i < total; i++)
            input[i] = MathF.Cos(2 * MathF.PI * i / total * 3.7f);

        var freq = new ComplexFp32[total];
        var back = new ComplexFp32[total];

        DftND.Forward(input, freq, dims);
        DftND.Inverse(freq, back, dims);

        float scale = 1.0f / total;
        float maxErr = 0;
        for (int i = 0; i < total; i++)
        {
            float err = (input[i] - back[i] * scale).Magnitude;
            if (err > maxErr) maxErr = err;
        }

        PrintResult("nD C2C round-trip", maxErr, 1e-6f);
    }

    static void Test1D_DCT_II_RoundTrip()
    {
        int N = 512;
        var input = new float[N];
        for (int i = 0; i < N; i++)
            input[i] = MathF.Sin(2 * MathF.PI * i / N * 4.2f) + 0.5f;

        var dct = new float[N];
        var idct = new float[N];

        RealToRealTransforms.DctII1D(input, dct);
        RealToRealTransforms.DctIII1D(dct, idct);

        // FFTW REDFT01 (DCT-II) followed by REDFT10 (DCT-III) = 2N × identity
        float scale = 1.0f / (2.0f * N);
        float maxDiff = 0;
        for (int i = 0; i < N; i++)
        {
            float diff = MathF.Abs(input[i] - idct[i] * scale);
            if (diff > maxDiff) maxDiff = diff;
        }

        PrintResult("1D DCT-II → III round-trip", maxDiff);
    }

    static void Test2D_DCT_II_RoundTrip()
    {
        int nx = 64, ny = 64;
        int total = nx * ny;
        var input = new float[total];
        for (int i = 0; i < total; i++)
            input[i] = MathF.Cos(2 * MathF.PI * i / total * 2.5f);

        var dct = new float[total];
        var idct = new float[total];

        RealToRealTransforms.DctII2D(input, dct, nx, ny);
        RealToRealTransforms.DctIII2D(dct, idct, nx, ny);

        // 2D: total factor = 4 × (nx*ny)
        float scale = 1.0f / (4.0f * total);
        float maxDiff = 0;
        for (int i = 0; i < total; i++)
        {
            float diff = MathF.Abs(input[i] - idct[i] * scale);
            if (diff > maxDiff) maxDiff = diff;
        }

        PrintResult("2D DCT-II → III round-trip", maxDiff);
    }

    static void TestND_DCT_II_RoundTrip()
    {
        int[] dims = [16, 24];
        int total = 1;
        foreach (var d in dims) total *= d;

        var input = new float[total];
        for (int i = 0; i < total; i++)
            input[i] = MathF.Sin(2 * MathF.PI * i / total * 1.8f);

        var dct = new float[total];
        var idct = new float[total];

        RealToRealTransforms.DctII(input, dct, dims);
        RealToRealTransforms.DctIII(dct, idct, dims);

        // nD: factor = 2^dims.Length × product(dims)
        float scale = 1.0f / (MathF.Pow(2.0f, dims.Length) * total);
        float maxDiff = 0;
        for (int i = 0; i < total; i++)
        {
            float diff = MathF.Abs(input[i] - idct[i] * scale);
            if (diff > maxDiff) maxDiff = diff;
        }

        PrintResult("nD DCT-II → III round-trip", maxDiff);
    }

    static void Test1D_R2HC_HC2R_RoundTrip()
    {
        int N = 512;
        var input = new float[N];
        for (int i = 0; i < N; i++)
            input[i] = MathF.Cos(2 * MathF.PI * i * 6 / N) + 0.4f * MathF.Sin(2 * MathF.PI * i * 15 / N);

        var r2hc = new float[N];
        var hc2r = new float[N];

        RealToRealTransforms.R2HC1D(input, r2hc);
        RealToRealTransforms.HC2R1D(r2hc, hc2r);

        float scale = 1.0f / N;
        float maxDiff = 0;
        for (int i = 0; i < N; i++)
        {
            float diff = MathF.Abs(input[i] - hc2r[i] * scale);
            if (diff > maxDiff) maxDiff = diff;
        }

        PrintResult("1D R2HC → HC2R round-trip", maxDiff);
    }
}