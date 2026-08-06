using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Transforms.Base;

using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms
{
    /// <summary>
    /// Represents Fast Hilbert Transform.
    /// </summary>
    public class HilbertTransform : ITransform
    {
        /// <summary>
        /// Gets size of Hilbert transform.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Intermediate buffer for real parts.
        /// </summary>
        private readonly float[] _re;

        /// <summary>
        /// Intermediate buffer for imaginary parts.
        /// </summary>
        private readonly float[] _im;

        /// <summary>
        /// Constructs Hilbert transformer. Transform <paramref name="size"/> must be a power of 2.
        /// </summary>
        public HilbertTransform(int size = 512)
        {
            Size = size;
            _fft = new Fft(size);
            _re = new float[size];
            _im = new float[size];
        }

        /// <summary>
        /// Computes complex analytic signal (real and imaginary parts) from <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Input data</param>
        public ComplexDiscreteSignal AnalyticSignal(float[] input)
            => AnalyticSignal(input.AsSpan());

        /// <summary>
        /// Computes complex analytic signal (real and imaginary parts) from <paramref name="input"/>.
        /// </summary>
        public ComplexDiscreteSignal AnalyticSignal(ReadOnlySpan<float> input)
            => AnalyticSignal(input, context: null);

        /// <summary>
        /// Computes complex analytic signal with optional <paramref name="context"/> for the inner FFT.
        /// </summary>
        public ComplexDiscreteSignal AnalyticSignal(ReadOnlySpan<float> input, ComputingContext? context)
        {
            Direct(input, _im, context);

            for (int i = 0; i < Size; i++)
            {
                _re[i] /= Size;
                _im[i] /= Size;
            }

            return new ComplexDiscreteSignal(1, _re, _im, allocateNew: true);
        }

        /// <summary>
        /// Computes magnitudes of the analytic signal without allocating a <see cref="ComplexDiscreteSignal"/>.
        /// </summary>
        public void AnalyticMagnitude(ReadOnlySpan<float> input, Span<float> magnitude)
            => AnalyticMagnitude(input, magnitude, context: null);

        /// <summary>
        /// Computes magnitudes of the analytic signal with optional <paramref name="context"/> for the inner FFT.
        /// </summary>
        public void AnalyticMagnitude(ReadOnlySpan<float> input, Span<float> magnitude, ComputingContext? context)
        {
            Direct(input, _im, context);

            var n = Math.Min(Size, magnitude.Length);
            for (var i = 0; i < n; i++)
            {
                var re = _re[i] / Size;
                var im = _im[i] / Size;
                magnitude[i] = MathF.Sqrt(re * re + im * im);
            }
        }

        /// <summary>
        /// Does Fast Hilbert Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Direct(float[] input, float[] output)
            => Direct(input.AsSpan(), output, context: null);

        /// <summary>
        /// Does Fast Hilbert Transform from sample span.
        /// </summary>
        public void Direct(ReadOnlySpan<float> input, float[] output)
            => Direct(input, output, context: null);

        /// <summary>
        /// Does Fast Hilbert Transform with optional <paramref name="context"/> for the inner FFT.
        /// </summary>
        public void Direct(ReadOnlySpan<float> input, float[] output, ComputingContext? context)
        {
            // just here, for code brevity, use alias _im for output (i.e. it's not internal _im)
            var _im = output;

            Array.Clear(_re, 0, _re.Length);
            Array.Clear(_im, 0, _im.Length);

            input.Slice(0, Math.Min(input.Length, Size)).CopyTo(_re);

            _fft.Direct(_re, _im, context);

            for (var i = 1; i < _re.Length / 2; i++)
            {
                _re[i] *= 2;
                _im[i] *= 2;
            }

            for (var i = _re.Length / 2 + 1; i < _re.Length; i++)
            {
                _re[i] = 0.0f;
                _im[i] = 0.0f;
            }

            _fft.Inverse(_re, _im, context);
        }

        /// <summary>
        /// Does normalized Fast Hilbert Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void DirectNorm(float[] input, float[] output)
            => DirectNorm(input, output, context: null);

        /// <summary>
        /// Does normalized Fast Hilbert Transform with optional <paramref name="context"/>.
        /// </summary>
        public void DirectNorm(float[] input, float[] output, ComputingContext? context)
        {
            Direct(input.AsSpan(), output, context);

            for (int i = 0; i < Size; i++)
            {
                output[i] /= Size;
            }
        }

        /// <summary>
        /// Does Inverse Fast Hilbert Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Inverse(float[] input, float[] output)
            => Inverse(input, output, context: null);

        /// <summary>
        /// Does Inverse Fast Hilbert Transform with optional <paramref name="context"/>.
        /// </summary>
        public void Inverse(float[] input, float[] output, ComputingContext? context)
        {
            Direct(input.AsSpan(), output, context);

            for (var i = 0; i < output.Length; i++)
            {
                output[i] = -output[i];
            }
        }

        /// <summary>
        /// Does normalized Inverse Fast Hilbert Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void InverseNorm(float[] input, float[] output)
            => InverseNorm(input, output, context: null);

        /// <summary>
        /// Does normalized Inverse Fast Hilbert Transform with optional <paramref name="context"/>.
        /// </summary>
        public void InverseNorm(float[] input, float[] output, ComputingContext? context)
        {
            DirectNorm(input, output, context);

            for (var i = 0; i < output.Length; i++)
            {
                output[i] = -output[i];
            }
        }
    }
}
