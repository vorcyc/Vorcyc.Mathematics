using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace basic_benchmark;

public class FFT_new_old_benchmark
{


    [Params(256, 512, 1024, 2048, 4096, 8192, 16384, 32768)]
    public int N;


    //public float[] _array;
    private PinnableArray<float>? _array;
    private ComplexFp32[] _out;


    private RealFft _fft;
    private float[] _outReal;
    private float[] _outImg;

    private RealOnlyFFT_Fp32 _realOnly;

    [GlobalSetup]
    public void Setup()
    {
        //_array = new float[N];
        //for (int i = 0; i < _array.Length; i++)
        //{
        //    _array[i] = Random.Shared.NextSingle();
        //}
        _array?.Dispose();
        _array = null;
        _array = new(N, false);


        _out = new ComplexFp32[N];

        _fft = new RealFft(N);
        _outReal = new float[N];
        _outImg = new float[N];

        _realOnly = new RealOnlyFFT_Fp32(N);
    }


    [Benchmark]
    public bool my_method() => FastFourierTransform.Forward(_array.AsSpan(), _out);


    [Benchmark]
    public void other_method() => _fft.Direct(_array, _outReal, _outImg);


    [Benchmark]
    public void my_new() => _realOnly.Forward((Span<float>)_array, _out);

}


public class FFT
{
    /// <summary>
    /// 计算快速傅里叶变换（FFT）。
    /// </summary>
    /// <param name="realInput">实数数组，表示输入信号。</param>
    /// <returns>返回复数数组，表示频域信号。</returns>
    public static ComplexFp32[] Compute(float[] realInput)
    {
        int n = realInput.Length;
        if ((n & (n - 1)) != 0)
            throw new ArgumentException("数据长度必须是2的幂。");

        // 将实数输入转换为复数数组
        ComplexFp32[] data = new ComplexFp32[n];
        for (int i = 0; i < n; i++)
        {
            data[i] = new ComplexFp32(realInput[i], 0);
        }

        // 执行FFT
        Compute(data);

        return data;
    }

    /// <summary>
    /// 计算快速傅里叶变换（FFT）。
    /// </summary>
    /// <param name="data">复数数组，表示输入信号。</param>
    public static void Compute(ComplexFp32[] data)
    {
        int n = data.Length;
        if ((n & (n - 1)) != 0)
            throw new ArgumentException("数据长度必须是2的幂。");

        // 位反转置换
        int bits = (int)Math.Log2(n);
        for (int j = 1, i = 0; j < n; j++)
        {
            int bit = n >> 1;
            for (; i >= bit; bit >>= 1)
                i -= bit;
            i += bit;
            if (j < i)
            {
                var temp = data[j];
                data[j] = data[i];
                data[i] = temp;
            }
        }

        // FFT计算
        for (int len = 2; len <= n; len <<= 1)
        {
            float angle = -2 * MathF.PI / len;
            ComplexFp32 wlen = new ComplexFp32(MathF.Cos(angle), MathF.Sin(angle));
            for (int i = 0; i < n; i += len)
            {
                ComplexFp32 w = ComplexFp32.One;
                for (int j = 0; j < len / 2; j += Vector<Complex>.Count)
                {
                    var u = new Vector<ComplexFp32>(data, i + j);
                    var v = new Vector<ComplexFp32>(data, i + j + len / 2) * w;
                    (u + v).CopyTo(data, i + j);
                    (u - v).CopyTo(data, i + j + len / 2);
                    w *= wlen;
                }
            }
        }
    }
}

//public class FFT
//{
//    /// <summary>
//    /// 计算快速傅里叶变换（FFT）。
//    /// </summary>
//    /// <param name="realInput">实数数组，表示输入信号。</param>
//    /// <returns>返回复数数组，表示频域信号。</returns>
//    public static ComplexFp32[] Compute(float[] realInput)
//    {
//        int n = realInput.Length;
//        if ((n & (n - 1)) != 0)
//            throw new ArgumentException("数据长度必须是2的幂。");

//        // 将实数输入转换为复数数组
//        ComplexFp32[] data = new ComplexFp32[n];
//        for (int i = 0; i < n; i++)
//        {
//            data[i] = new ComplexFp32(realInput[i], 0);
//        }

//        // 执行FFT
//        Compute(data);

//        return data;
//    }

//    /// <summary>
//    /// 计算快速傅里叶变换（FFT）。
//    /// </summary>
//    /// <param name="data">复数数组，表示输入信号。</param>
//    public static void Compute(ComplexFp32[] data)
//    {
//        int n = data.Length;
//        if ((n & (n - 1)) != 0)
//            throw new ArgumentException("数据长度必须是2的幂。");

//        // 位反转置换
//        int bits = (int)Math.Log2(n);
//        for (int j = 1, i = 0; j < n; j++)
//        {
//            int bit = n >> 1;
//            for (; i >= bit; bit >>= 1)
//                i -= bit;
//            i += bit;
//            if (j < i)
//            {
//                var temp = data[j];
//                data[j] = data[i];
//                data[i] = temp;
//            }
//        }

//        // 并行处理
//        Parallel.For(0, bits, level =>
//        {
//            int len = 1 << (level + 1);
//            float angle = -2 * MathF.PI / len;
//            ComplexFp32 wlen = new(MathF.Cos(angle), MathF.Sin(angle));
//            for (int i = 0; i < n; i += len)
//            {
//                ComplexFp32 w = ComplexFp32.One;
//                for (int j = 0; j < len / 2; j++)
//                {
//                    ComplexFp32 u = data[i + j];
//                    ComplexFp32 v = data[i + j + len / 2] * w;
//                    data[i + j] = u + v;
//                    data[i + j + len / 2] = u - v;
//                    w *= wlen;
//                }
//            }
//        });
//    }
//}