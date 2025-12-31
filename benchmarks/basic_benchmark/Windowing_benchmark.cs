using BenchmarkDotNet.Attributes;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace basic_benchmark
{
    public enum WindowCase
    {
        Hamming,
        Hamming_Periodic,
        Blackman,
        Blackman_Periodic,
        Hann,
        Hann_Periodic,
        Bartlett_Hann,
        Lanczos,
        Flattop,
        Blackman_Harris,

        // 需要额外参数的 case
        Gaussian_Default,     // 无参，默认 sigma=0.4
        Gaussian_Param,       // 指定 sigma
        Kaiser,               // 指定 alpha
        Kbd,                  // 指定 alpha
        PowerOfSine,          // 指定 alpha
        Liftering             // 指定 L
    }

    [MemoryDiagnoser]
    [WarmupCount(2)]           // 建议加点 warmup，减少噪声
    [IterationCount(15)]       // 每组迭代次数适中，平衡精度和时间
    public class WindowPairBenchmark
    {
        // 窗口长度（从小到大，覆盖常见 FFT / 音频场景）
        [Params(
            1024,       // 小窗口（语音帧）
            4096,
            16384,      // 常见 FFT
            65536,
            88200,      // 44.1kHz 2秒
            176400,     // 44.1kHz 4秒
            480000,     // 48kHz 10秒
            960000,     // 48kHz 20秒
            5000000,    // 大数组压力测试
            10000000    // 极限（视内存而定）
        )]
        public int N { get; set; }

        [ParamsAllValues]  // 自动遍历所有 WindowCase 枚举值（更方便）
        public WindowCase Case { get; set; }

        // 参数（仅在对应 Case 时使用）
        [Params(0.4f)] public float GaussianSigma { get; set; }
        [Params(12f)] public float KaiserAlpha { get; set; }
        [Params(4f)] public float KbdAlpha { get; set; }
        [Params(1.5f)] public float PowerOfSineAlpha { get; set; }
        [Params(22)] public int LifteringL { get; set; }

        private float[] _dataStd = null!;
        private float[] _dataSimd = null!;

        private Action<Span<float>> _stdAction = null!;
        private Action<Span<float>> _simdAction = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _dataStd = new float[N];
            _dataSimd = new float[N];

            switch (Case)
            {
                case WindowCase.Hamming:
                    _stdAction = WindowApplier.Hamming;
                    _simdAction = WindowApplier.Hamming_SIMD;
                    break;
                case WindowCase.Hamming_Periodic:
                    _stdAction = WindowApplier.Hamming_Periodic;
                    _simdAction = WindowApplier.Hamming_Periodic_SIMD;
                    break;
                case WindowCase.Blackman:
                    _stdAction = WindowApplier.Blackman;
                    _simdAction = WindowApplier.Blackman_SIMD;
                    break;
                case WindowCase.Blackman_Periodic:
                    _stdAction = WindowApplier.Blackman_Periodic;
                    _simdAction = WindowApplier.Blackman_Periodic_SIMD;
                    break;
                case WindowCase.Hann:
                    _stdAction = WindowApplier.Hann;
                    _simdAction = WindowApplier.Hann_SIMD;
                    break;
                case WindowCase.Hann_Periodic:
                    _stdAction = WindowApplier.Hann_Periodic;
                    _simdAction = WindowApplier.Hann_Periodic_SIMD;
                    break;
                case WindowCase.Bartlett_Hann:
                    _stdAction = WindowApplier.Bartlett_Hann;
                    _simdAction = WindowApplier.Bartlett_Hann_SIMD;
                    break;
                case WindowCase.Lanczos:
                    _stdAction = WindowApplier.Lanczos;
                    _simdAction = WindowApplier.Lanczos_SIMD;
                    break;
                case WindowCase.Flattop:
                    _stdAction = WindowApplier.Flattop;
                    _simdAction = WindowApplier.Flattop_SIMD;
                    break;
                case WindowCase.Blackman_Harris:
                    _stdAction = WindowApplier.Blackman_Harris;
                    _simdAction = WindowApplier.Blackman_Harris_SIMD;
                    break;

                // 有参数的
                case WindowCase.Gaussian_Default:
                    _stdAction = WindowApplier.Gaussian;
                    _simdAction = WindowApplier.Gaussian_SIMD;
                    break;
                case WindowCase.Gaussian_Param:
                    _stdAction = v => WindowApplier.Gaussian(v, GaussianSigma);
                    _simdAction = v => WindowApplier.Gaussian_SIMD(v, GaussianSigma);
                    break;
                case WindowCase.Kaiser:
                    _stdAction = v => WindowApplier.Kaiser(v, KaiserAlpha);
                    _simdAction = v => WindowApplier.Kaiser_SIMD(v, KaiserAlpha);
                    break;
                case WindowCase.Kbd:
                    _stdAction = v => WindowApplier.Kbd(v, KbdAlpha);
                    _simdAction = v => WindowApplier.Kbd_SIMD(v, KbdAlpha);
                    break;
                case WindowCase.PowerOfSine:
                    _stdAction = v => WindowApplier.PowerOfSine(v, PowerOfSineAlpha);
                    _simdAction = v => WindowApplier.PowerOfSine_SIMD(v, PowerOfSineAlpha);
                    break;
                case WindowCase.Liftering:
                    _stdAction = v => WindowApplier.Liftering(v, LifteringL);
                    _simdAction = v => WindowApplier.Liftering_SIMD(v, LifteringL);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Case));
            }
        }

        [IterationSetup]
        public void IterationSetup()
        {
            // 每次迭代前重置为全 1.0f（窗函数是乘法，就地修改）
            Array.Fill(_dataStd, 1f);
            Array.Fill(_dataSimd, 1f);
        }

        [Benchmark(Baseline = true)]
        public void Standard() => _stdAction(_dataStd);

        [Benchmark]
        public void Simd() => _simdAction(_dataSimd);
    }
}