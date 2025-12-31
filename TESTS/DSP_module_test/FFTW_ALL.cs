using System;
using System.Numerics;
using Vorcyc.Mathematics.Extensions.FFTW;
using Vorcyc.Mathematics.Numerics;

namespace DSP_module_test;

public static class FFTW_ALL
{
    public static void Run()
    {
        Console.WriteLine("=== 双精度 FFTW 封装功能验证 ===\n");
        Double_Dft1D_C2C_RoundTrip();
        Double_Dft1D_R2C_C2R_RoundTrip();
        Double_Dft2D_C2C_RoundTrip();
        Double_Dft3D_C2C_RoundTrip();
        Double_DftND_C2C_RoundTrip();
        Double_R2R_DCT_II_III_1D_RoundTrip();
        Double_R2R_DCT_II_III_2D_RoundTrip();
        Double_R2R_DCT_II_III_ND_RoundTrip();
        Double_R2R_R2HC_HC2R_1D_RoundTrip();
        Double_R2R_DHT_1D_SelfInverse();
        Console.WriteLine("\n双精度测试完成。\n");

        Console.WriteLine("=== 单精度 FFTW 封装功能验证 ===\n");
        Single_Dft1D_C2C_RoundTrip();
        Single_Dft1D_R2C_C2R_RoundTrip();
        Single_Dft2D_C2C_RoundTrip();
        Single_Dft3D_C2C_RoundTrip();
        Single_DftND_C2C_RoundTrip();
        Single_R2R_DCT_II_III_1D_RoundTrip();
        Single_R2R_DCT_II_III_2D_RoundTrip();
        Single_R2R_DCT_II_III_ND_RoundTrip();
        Single_R2R_R2HC_HC2R_1D_RoundTrip();
        Single_R2R_DHT_1D_SelfInverse();
        Console.WriteLine("\n单精度测试完成。");
    }

    // ========== 输出工具 ==========
    static void PrintResult(string name, double maxError, double threshold)
    {
        string pass = maxError < threshold ? "通过" : "误差较大，需检查";
        Console.WriteLine($"  {name,-36} 最大误差: {maxError,14:E10}  → {pass}");
    }

    static double MaxAbs(double[] a, double[] b)
    {
        double max = 0;
        for (int i = 0; i < a.Length; i++)
        {
            var d = Math.Abs(a[i] - b[i]);
            if (d > max) max = d;
        }
        return max;
    }

    static double MaxAbs(Complex[] a, Complex[] b)
    {
        double max = 0;
        for (int i = 0; i < a.Length; i++)
        {
            var d = (a[i] - b[i]).Magnitude;
            if (d > max) max = d;
        }
        return max;
    }

    static float MaxAbs(float[] a, float[] b)
    {
        float max = 0;
        for (int i = 0; i < a.Length; i++)
        {
            var d = MathF.Abs(a[i] - b[i]);
            if (d > max) max = d;
        }
        return max;
    }

    static float MaxAbs(ComplexFp32[] a, ComplexFp32[] b)
    {
        float max = 0;
        for (int i = 0; i < a.Length; i++)
        {
            var d = (a[i] - b[i]).Magnitude;
            if (d > max) max = d;
        }
        return max;
    }

    // ========== 双精度 DFT ==========
    static void Double_Dft1D_C2C_RoundTrip()
    {
        int N = 512;
        var input = new Complex[N];
        for (int i = 0; i < N; i++)
        {
            var re = Math.Cos(2 * Math.PI * i * 7 / N);
            var im = 0.6 * Math.Sin(2 * Math.PI * i * 23 / N);
            input[i] = new Complex(re, im);
        }
        var freq = new Complex[N];
        var back = new Complex[N];

        Dft1D.Forward(input, freq);
        Dft1D.Inverse(freq, back);

        var scale = 1.0 / N;
        for (int i = 0; i < N; i++) back[i] *= scale;

        var err = MaxAbs(input, back);
        PrintResult("Double 1D C2C round-trip", err, 1e-12);
    }

    static void Double_Dft1D_R2C_C2R_RoundTrip()
    {
        int N = 1024;
        var real = new double[N];
        for (int i = 0; i < N; i++)
            real[i] = Math.Sin(2 * Math.PI * i * 5 / N) + 0.8 * Math.Cos(2 * Math.PI * i * 18 / N);

        var spectrum = new Complex[N / 2 + 1];
        var recovered = new double[N];

        Dft1D.Forward(real, spectrum);
        Dft1D.Inverse(spectrum, recovered, scale: true);

        var err = MaxAbs(real, recovered);
        PrintResult("Double 1D R2C → C2R (scale=true)", err, 1e-12);
    }

    static void Double_Dft2D_C2C_RoundTrip()
    {
        int nx = 64, ny = 64;
        var input = new Complex[nx * ny];
        for (int y = 0; y < ny; y++)
            for (int x = 0; x < nx; x++)
                input[y * nx + x] = new Complex(Math.Sin(2 * Math.PI * x * 4 / nx), Math.Cos(2 * Math.PI * y * 6 / ny) * 0.1);

        var freq = new Complex[input.Length];
        var back = new Complex[input.Length];

        Dft2D.Forward(input, freq, nx, ny);
        Dft2D.Inverse(freq, back, nx, ny);

        var scale = 1.0 / (nx * ny);
        for (int i = 0; i < back.Length; i++) back[i] *= scale;

        var err = MaxAbs(input, back);
        PrintResult("Double 2D C2C round-trip", err, 1e-12);
    }

    static void Double_Dft3D_C2C_RoundTrip()
    {
        int nx = 16, ny = 16, nz = 8;
        int total = nx * ny * nz;
        var input = new Complex[total];

        for (int z = 0; z < nz; z++)
            for (int y = 0; y < ny; y++)
                for (int x = 0; x < nx; x++)
                {
                    var re = Math.Sin(2 * Math.PI * x / nx * 2) + Math.Cos(2 * Math.PI * y / ny * 3);
                    var im = 0.1 * re;
                    input[(z * ny + y) * nx + x] = new Complex(re, im);
                }

        var freq = new Complex[total];
        var back = new Complex[total];

        Dft3D.Forward(input, freq, nx, ny, nz);
        Dft3D.Inverse(freq, back, nx, ny, nz);

        var scale = 1.0 / total;
        for (int i = 0; i < total; i++) back[i] *= scale;

        var err = MaxAbs(input, back);
        PrintResult("Double 3D C2C round-trip", err, 1e-11);
    }

    static void Double_DftND_C2C_RoundTrip()
    {
        int[] dims = new[] { 8, 12, 10 }; // 960
        int total = 1; foreach (var d in dims) total *= d;

        var input = new Complex[total];
        for (int i = 0; i < total; i++)
            input[i] = new Complex(Math.Cos(2 * Math.PI * i / total * 3.7), 0.0);

        var freq = new Complex[total];
        var back = new Complex[total];

        DftND.Forward(input, freq, dims);
        DftND.Inverse(freq, back, dims);

        var scale = 1.0 / total;
        for (int i = 0; i < total; i++) back[i] *= scale;

        var err = MaxAbs(input, back);
        PrintResult("Double nD C2C round-trip", err, 1e-11);
    }

    // ========== 双精度 R2R ==========
    static void Double_R2R_DCT_II_III_1D_RoundTrip()
    {
        int N = 512;
        var input = new double[N];
        for (int i = 0; i < N; i++)
            input[i] = Math.Sin(2 * Math.PI * i / N * 4.2) + 0.5;

        var dct = new double[N];
        var idct = new double[N];

        RealToRealTransforms.DctII1D(input, dct);
        RealToRealTransforms.DctIII1D(dct, idct);

        // DCT-II followed by DCT-III -> factor 2N
        var scale = 1.0 / (2.0 * N);
        for (int i = 0; i < N; i++) idct[i] *= scale;

        var err = MaxAbs(input, idct);
        PrintResult("Double 1D DCT-II → III round-trip", err, 1e-12);
    }

    static void Double_R2R_DCT_II_III_2D_RoundTrip()
    {
        int nx = 64, ny = 64;
        int total = nx * ny;
        var input = new double[total];
        for (int i = 0; i < total; i++)
            input[i] = Math.Cos(2 * Math.PI * i / total * 2.5);

        var dct = new double[total];
        var idct = new double[total];

        RealToRealTransforms.DctII2D(input, dct, nx, ny);
        RealToRealTransforms.DctIII2D(dct, idct, nx, ny);

        // per-dimension factor 2 → overall 4*(nx*ny)
        var scale = 1.0 / (4.0 * total);
        for (int i = 0; i < total; i++) idct[i] *= scale;

        var err = MaxAbs(input, idct);
        PrintResult("Double 2D DCT-II → III round-trip", err, 1e-12);
    }

    static void Double_R2R_DCT_II_III_ND_RoundTrip()
    {
        int[] dims = new[] { 16, 24 };
        int total = 1; foreach (var d in dims) total *= d;

        var input = new double[total];
        for (int i = 0; i < total; i++)
            input[i] = Math.Sin(2 * Math.PI * i / total * 1.8);

        var dct = new double[total];
        var idct = new double[total];

        RealToRealTransforms.DctII(input, dct, dims);
        RealToRealTransforms.DctIII(dct, idct, dims);

        // factor = 2^rank * product(dims)
        var scale = 1.0 / (Math.Pow(2.0, dims.Length) * total);
        for (int i = 0; i < total; i++) idct[i] *= scale;

        var err = MaxAbs(input, idct);
        PrintResult("Double nD DCT-II → III round-trip", err, 1e-12);
    }

    static void Double_R2R_R2HC_HC2R_1D_RoundTrip()
    {
        int N = 512;
        var input = new double[N];
        for (int i = 0; i < N; i++)
            input[i] = Math.Cos(2 * Math.PI * i * 6 / N) + 0.4 * Math.Sin(2 * Math.PI * i * 15 / N);

        var r2hc = new double[N];
        var hc2r = new double[N];

        RealToRealTransforms.R2HC1D(input, r2hc);
        RealToRealTransforms.HC2R1D(r2hc, hc2r);

        var scale = 1.0 / N;
        for (int i = 0; i < N; i++) hc2r[i] *= scale;

        var err = MaxAbs(input, hc2r);
        PrintResult("Double 1D R2HC → HC2R round-trip", err, 1e-12);
    }

    static void Double_R2R_DHT_1D_SelfInverse()
    {
        int N = 512;
        var input = new double[N];
        for (int i = 0; i < N; i++)
            input[i] = Math.Sin(2 * Math.PI * i * 7 / N) + Math.Cos(2 * Math.PI * i * 13 / N);

        var h1 = new double[N];
        var h2 = new double[N];

        RealToRealTransforms.Dht1D(input, h1);
        RealToRealTransforms.Dht1D(h1, h2);

        // H(H(x)) = N * x
        var scale = 1.0 / N;
        for (int i = 0; i < N; i++) h2[i] *= scale;

        var err = MaxAbs(input, h2);
        PrintResult("Double 1D DHT self-inverse", err, 1e-12);
    }

    // ========== 单精度 DFT ==========
    static void Single_Dft1D_C2C_RoundTrip()
    {
        int N = 512;
        var input = new ComplexFp32[N];
        for (int i = 0; i < N; i++)
        {
            float re = MathF.Cos(2 * MathF.PI * i * 7 / N);
            float im = 0.6f * MathF.Sin(2 * MathF.PI * i * 23 / N);
            input[i] = new ComplexFp32(re, im);
        }
        var freq = new ComplexFp32[N];
        var back = new ComplexFp32[N];

        Dft1D.Forward(input, freq);
        Dft1D.Inverse(freq, back);

        float scale = 1f / N;
        for (int i = 0; i < N; i++) back[i] *= scale;

        var err = MaxAbs(input, back);
        PrintResult("Single 1D C2C round-trip", err, 1e-6);
    }

    static void Single_Dft1D_R2C_C2R_RoundTrip()
    {
        int N = 1024;
        var real = new float[N];
        for (int i = 0; i < N; i++)
            real[i] = MathF.Sin(2 * MathF.PI * i * 5 / N) + 0.8f * MathF.Cos(2 * MathF.PI * i * 18 / N);

        var spectrum = new ComplexFp32[N / 2 + 1];
        var recovered = new float[N];

        Dft1D.Forward(real, spectrum);
        Dft1D.Inverse(spectrum, recovered, scale: true);

        var err = MaxAbs(real, recovered);
        PrintResult("Single 1D R2C → C2R (scale=true)", err, 1e-6);
    }

    static void Single_Dft2D_C2C_RoundTrip()
    {
        int nx = 64, ny = 64;
        var input = new ComplexFp32[nx * ny];
        for (int y = 0; y < ny; y++)
            for (int x = 0; x < nx; x++)
                input[y * nx + x] = new ComplexFp32(MathF.Sin(2 * MathF.PI * x * 4 / nx), 0.1f * MathF.Cos(2 * MathF.PI * y * 6 / ny));

        var freq = new ComplexFp32[input.Length];
        var back = new ComplexFp32[input.Length];

        Dft2D.Forward(input, freq, nx, ny);
        Dft2D.Inverse(freq, back, nx, ny);

        float scale = 1f / (nx * ny);
        for (int i = 0; i < back.Length; i++) back[i] *= scale;

        var err = MaxAbs(input, back);
        PrintResult("Single 2D C2C round-trip", err, 1e-6);
    }

    static void Single_Dft3D_C2C_RoundTrip()
    {
        int nx = 16, ny = 16, nz = 8;
        int total = nx * ny * nz;
        var input = new ComplexFp32[total];

        for (int z = 0; z < nz; z++)
            for (int y = 0; y < ny; y++)
                for (int x = 0; x < nx; x++)
                {
                    float re = MathF.Sin(2 * MathF.PI * x / nx * 2) + MathF.Cos(2 * MathF.PI * y / ny * 3);
                    float im = 0.1f * re;
                    input[(z * ny + y) * nx + x] = new ComplexFp32(re, im);
                }

        var freq = new ComplexFp32[total];
        var back = new ComplexFp32[total];

        Dft3D.Forward(input, freq, nx, ny, nz);
        Dft3D.Inverse(freq, back, nx, ny, nz);

        float scale = 1f / total;
        for (int i = 0; i < total; i++) back[i] *= scale;

        var err = MaxAbs(input, back);
        PrintResult("Single 3D C2C round-trip", err, 1e-6);
    }

    static void Single_DftND_C2C_RoundTrip()
    {
        int[] dims = new[] { 8, 12, 10 };
        int total = 1; foreach (var d in dims) total *= d;

        var input = new ComplexFp32[total];
        for (int i = 0; i < total; i++)
            input[i] = MathF.Cos(2 * MathF.PI * i / total * 3.7f);

        var freq = new ComplexFp32[total];
        var back = new ComplexFp32[total];

        DftND.Forward(input, freq, dims);
        DftND.Inverse(freq, back, dims);

        float scale = 1f / total;
        for (int i = 0; i < total; i++) back[i] *= scale;

        var err = MaxAbs(input, back);
        PrintResult("Single nD C2C round-trip", err, 1e-6);
    }

    // ========== 单精度 R2R ==========
    static void Single_R2R_DCT_II_III_1D_RoundTrip()
    {
        int N = 512;
        var input = new float[N];
        for (int i = 0; i < N; i++)
            input[i] = MathF.Sin(2 * MathF.PI * i / N * 4.2f) + 0.5f;

        var dct = new float[N];
        var idct = new float[N];

        RealToRealTransforms.DctII1D(input, dct);
        RealToRealTransforms.DctIII1D(dct, idct);

        float scale = 1f / (2f * N);
        for (int i = 0; i < N; i++) idct[i] *= scale;

        var err = MaxAbs(input, idct);
        PrintResult("Single 1D DCT-II → III round-trip", err, 1e-6);
    }

    static void Single_R2R_DCT_II_III_2D_RoundTrip()
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

        float scale = 1f / (4f * total);
        for (int i = 0; i < total; i++) idct[i] *= scale;

        var err = MaxAbs(input, idct);
        PrintResult("Single 2D DCT-II → III round-trip", err, 1e-6);
    }

    static void Single_R2R_DCT_II_III_ND_RoundTrip()
    {
        int[] dims = new[] { 16, 24 };
        int total = 1; foreach (var d in dims) total *= d;

        var input = new float[total];
        for (int i = 0; i < total; i++)
            input[i] = MathF.Sin(2 * MathF.PI * i / total * 1.8f);

        var dct = new float[total];
        var idct = new float[total];

        RealToRealTransforms.DctII(input, dct, dims);
        RealToRealTransforms.DctIII(dct, idct, dims);

        float scale = 1f / ((float)MathF.Pow(2f, dims.Length) * total);
        for (int i = 0; i < total; i++) idct[i] *= scale;

        var err = MaxAbs(input, idct);
        PrintResult("Single nD DCT-II → III round-trip", err, 1e-6);
    }

    static void Single_R2R_R2HC_HC2R_1D_RoundTrip()
    {
        int N = 512;
        var input = new float[N];
        for (int i = 0; i < N; i++)
            input[i] = MathF.Cos(2 * MathF.PI * i * 6 / N) + 0.4f * MathF.Sin(2 * MathF.PI * i * 15 / N);

        var r2hc = new float[N];
        var hc2r = new float[N];

        RealToRealTransforms.R2HC1D(input, r2hc);
        RealToRealTransforms.HC2R1D(r2hc, hc2r);

        float scale = 1f / N;
        for (int i = 0; i < N; i++) hc2r[i] *= scale;

        var err = MaxAbs(input, hc2r);
        PrintResult("Single 1D R2HC → HC2R round-trip", err, 1e-6);
    }

    static void Single_R2R_DHT_1D_SelfInverse()
    {
        int N = 512;
        var input = new float[N];
        for (int i = 0; i < N; i++)
            input[i] = MathF.Sin(2 * MathF.PI * i * 7 / N) + MathF.Cos(2 * MathF.PI * i * 13 / N);

        var h1 = new float[N];
        var h2 = new float[N];

        RealToRealTransforms.Dht1D(input, h1);
        RealToRealTransforms.Dht1D(h1, h2);

        float scale = 1f / N;
        for (int i = 0; i < N; i++) h2[i] *= scale;

        var err = MaxAbs(input, h2);
        PrintResult("Single 1D DHT self-inverse", err, 1e-6);
    }
}