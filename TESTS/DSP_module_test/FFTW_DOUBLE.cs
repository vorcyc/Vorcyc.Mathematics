using System.Numerics;
using Vorcyc.Mathematics.Extensions.FFTW;

namespace DSP_module_test;

class FFTW_DOUBLE
{
    public static void RUN()
    {
        Console.WriteLine("=== 双精度 FFTW 封装完整功能验证 ===\n");

        Test1D_Complex_RoundTrip();
        Test1D_Real_R2C_C2R_AutoScale();
        Test2D_Complex_RoundTrip();
        Test3D_Complex_Small_RoundTrip();
        TestND_Complex_RoundTrip_Small();

        Test1D_DCT_II_RoundTrip();
        Test2D_DCT_II_RoundTrip();
        TestND_DCT_II_RoundTrip();

        Test1D_DST_II_RoundTrip();
        Test1D_R2HC_HC2R_RoundTrip();

        Console.WriteLine("\n所有双精度测试执行完毕。按回车退出...");
        Console.ReadLine();
    }

    static void PrintResult(string name, double maxError, double threshold = 1e-10)
    {
        string status = maxError < threshold ? "通过" : "误差较大，请检查";
        Console.WriteLine($"  {name,-38} 最大误差: {maxError,14:E10}  → {status}");
    }

    static void Test1D_Complex_RoundTrip()
    {
        int N = 1024;
        var input = new Complex[N];
        for (int i = 0; i < N; i++)
        {
            input[i] = Math.Cos(2 * Math.PI * i * 5.7 / N) +
                       0.6 * Math.Sin(2 * Math.PI * i * 22.3 / N);
        }

        var freq = new Complex[N];
        var recovered = new Complex[N];

        Dft1D.Forward(input, freq);
        Dft1D.Inverse(freq, recovered);

        double scale = 1.0 / N;
        double maxErr = 0;
        for (int i = 0; i < N; i++)
        {
            double err = (input[i] - recovered[i] * scale).Magnitude;
            if (err > maxErr) maxErr = err;
        }

        PrintResult("1D C2C round-trip", maxErr);
    }

    static void Test1D_Real_R2C_C2R_AutoScale()
    {
        int N = 1024;
        var real = new double[N];
        for (int i = 0; i < N; i++)
            real[i] = Math.Sin(2 * Math.PI * i * 4.2 / N) + 0.7 * Math.Cos(2 * Math.PI * i * 15.8 / N);

        var spectrum = new Complex[N / 2 + 1];
        var recovered = new double[N];

        Dft1D.Forward(real, spectrum);
        Dft1D.Inverse(spectrum, recovered, scale: true);   // 1/N 自动缩放

        double maxDiff = 0;
        for (int i = 0; i < N; i++)
        {
            double diff = Math.Abs(real[i] - recovered[i]);
            if (diff > maxDiff) maxDiff = diff;
        }

        PrintResult("1D R2C → C2R (auto scale)", maxDiff);
    }

    static void Test2D_Complex_RoundTrip()
    {
        int nx = 128, ny = 64;
        int total = nx * ny;
        var input = new Complex[total];
        for (int y = 0; y < ny; y++)
            for (int x = 0; x < nx; x++)
            {
                double v = Math.Sin(2 * Math.PI * x * 3 / nx) * Math.Cos(2 * Math.PI * y * 5 / ny);
                input[y * nx + x] = new Complex(v, 0);
            }

        var freq = new Complex[total];
        var recovered = new Complex[total];

        Dft2D.Forward(input, freq, nx, ny);
        Dft2D.Inverse(freq, recovered, nx, ny);

        double scale = 1.0 / total;
        double maxErr = 0;
        for (int i = 0; i < total; i++)
        {
            double err = (input[i] - recovered[i] * scale).Magnitude;
            if (err > maxErr) maxErr = err;
        }

        PrintResult("2D C2C round-trip", maxErr, 1e-9);
    }

    static void Test3D_Complex_Small_RoundTrip()
    {
        int nx = 16, ny = 16, nz = 8;
        int total = nx * ny * nz;
        var input = new Complex[total];

        for (int z = 0; z < nz; z++)
            for (int y = 0; y < ny; y++)
                for (int x = 0; x < nx; x++)
                {
                    double v = Math.Sin(2 * Math.PI * x / nx * 2) + Math.Cos(2 * Math.PI * y / ny * 3);
                    input[(z * ny + y) * nx + x] = new Complex(v, 0.1 * v);
                }

        var freq = new Complex[total];
        var recovered = new Complex[total];

        Dft3D.Forward(input, freq, nx, ny, nz);
        Dft3D.Inverse(freq, recovered, nx, ny, nz);

        double scale = 1.0 / total;
        double maxErr = 0;
        for (int i = 0; i < total; i++)
        {
            double err = (input[i] - recovered[i] * scale).Magnitude;
            if (err > maxErr) maxErr = err;
        }

        PrintResult("3D C2C small round-trip", maxErr, 1e-8);
    }

    static void TestND_Complex_RoundTrip_Small()
    {
        int[] dims = [8, 12, 10];
        int total = 1;
        foreach (var d in dims) total *= d;

        var input = new Complex[total];
        for (int i = 0; i < total; i++)
            input[i] = Math.Cos(2 * Math.PI * i / total * 3.7);

        var freq = new Complex[total];
        var recovered = new Complex[total];

        DftND.Forward(input, freq, dims);
        DftND.Inverse(freq, recovered, dims);

        double scale = 1.0 / total;
        double maxErr = 0;
        for (int i = 0; i < total; i++)
        {
            double err = (input[i] - recovered[i] * scale).Magnitude;
            if (err > maxErr) maxErr = err;
        }

        PrintResult("nD C2C round-trip (small)", maxErr, 1e-9);
    }

    static void Test1D_DCT_II_RoundTrip()
    {
        int N = 512;
        var input = new double[N];
        for (int i = 0; i < N; i++)
            input[i] = Math.Sin(2 * Math.PI * i / N * 4.2) + 0.5;

        var dct = new double[N];
        var idct = new double[N];

        RealToRealTransforms.DctII1D(input, dct);
        RealToRealTransforms.DctIII1D(dct, idct);

        // FFTW REDFT01 followed by REDFT10 equals 2N × identity
        double scale = 1.0 / (2.0 * N);
        double maxDiff = 0;
        for (int i = 0; i < N; i++)
        {
            double diff = Math.Abs(input[i] - idct[i] * scale);
            if (diff > maxDiff) maxDiff = diff;
        }

        PrintResult("1D DCT-II → III round-trip", maxDiff);
    }

    static void Test2D_DCT_II_RoundTrip()
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

        // Each dimension contributes a factor of 2; total factor = 4 * (nx*ny)
        double scale = 1.0 / (4.0 * total);
        double maxDiff = 0;
        for (int i = 0; i < total; i++)
        {
            double diff = Math.Abs(input[i] - idct[i] * scale);
            if (diff > maxDiff) maxDiff = diff;
        }

        PrintResult("2D DCT-II → III round-trip", maxDiff);
    }

    static void TestND_DCT_II_RoundTrip()
    {
        int[] dims = [16, 24];
        int total = 1;
        foreach (var d in dims) total *= d;

        var input = new double[total];
        for (int i = 0; i < total; i++)
            input[i] = Math.Sin(2 * Math.PI * i / total * 1.8);

        var dct = new double[total];
        var idct = new double[total];

        RealToRealTransforms.DctII(input, dct, dims);
        RealToRealTransforms.DctIII(dct, idct, dims);

        // Factor = 2^dims.Length × product(dims)
        double scale = 1.0 / (Math.Pow(2.0, dims.Length) * total);
        double maxDiff = 0;
        for (int i = 0; i < total; i++)
        {
            double diff = Math.Abs(input[i] - idct[i] * scale);
            if (diff > maxDiff) maxDiff = diff;
        }

        PrintResult("nD DCT-II → III round-trip", maxDiff);
    }

    static void Test1D_DST_II_RoundTrip()
    {
        int N = 512;
        var input = new double[N];
        for (int i = 0; i < N; i++)
            input[i] = Math.Sin(2 * Math.PI * i / N * 3.9);

        var dst = new double[N];
        var idst = new double[N];

        RealToRealTransforms.DstII1D(input, dst);
        RealToRealTransforms.DstIII1D(dst, idst);

        // FFTW RODFT01 followed by RODFT10 equals 2N × identity
        double scale = 1.0 / (2.0 * N);
        double maxDiff = 0;
        for (int i = 0; i < N; i++)
        {
            double diff = Math.Abs(input[i] - idst[i] * scale);
            if (diff > maxDiff) maxDiff = diff;
        }

        PrintResult("1D DST-II → III round-trip", maxDiff);
    }

    static void Test1D_R2HC_HC2R_RoundTrip()
    {
        int N = 512;
        var input = new double[N];
        for (int i = 0; i < N; i++)
            input[i] = Math.Cos(2 * Math.PI * i * 6 / N) + 0.4 * Math.Sin(2 * Math.PI * i * 15 / N);

        var r2hc = new double[N];
        var hc2r = new double[N];

        RealToRealTransforms.R2HC1D(input, r2hc);
        RealToRealTransforms.HC2R1D(r2hc, hc2r);

        double scale = 1.0 / N;
        double maxDiff = 0;
        for (int i = 0; i < N; i++)
        {
            double diff = Math.Abs(input[i] - hc2r[i] * scale);
            if (diff > maxDiff) maxDiff = diff;
        }

        PrintResult("1D R2HC → HC2R round-trip", maxDiff);
    }
}