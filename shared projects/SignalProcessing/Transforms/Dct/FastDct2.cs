using System;
using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms
{
    /// <summary>
    /// Represents Discrete Cosine Transform of Type-II. 
    /// This FFT-based implementation of DCT-II is faster for bigger DCT sizes.
    /// </summary>
    public class FastDct2 : IDct
    {
        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Internal temporary buffer.
        /// </summary>
        private readonly float[] _temp;

        /// <summary>
        /// Gets size of DCT-II.
        /// </summary>
        public int Size => _fft.Size;

        /// <summary>
        /// Constructs <see cref="FastDct2"/> of given <paramref name="dctSize"/>.
        /// </summary>
        /// <param name="dctSize">Size of DCT-II</param>
        public FastDct2(int dctSize)
        {
            _fft = new Fft(dctSize);
            _temp = new float[dctSize];
        }

        /// <summary>
        /// Does DCT-II.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Direct(float[] input, float[] output)
        {
            Array.Clear(output, 0, output.Length);

            for (int m = 0; m < _temp.Length / 2; m++)
            {
                _temp[m] = input[2 * m];
                _temp[_temp.Length - 1 - m] = input[2 * m + 1];
            }

            _fft.Direct(_temp, output);

            // mutiply by exp(-j * pi * n / 2N):

            int N = _fft.Size; 
            for (int i = 0; i < N; i++)
            {
                output[i] = 2 * (_temp[i] * MathF.Cos(0.5f * ConstantsFp32.PI * i / N) - output[i] * MathF.Sin(-0.5f * ConstantsFp32.PI * i / N));
            }
        }

        /// <summary>
        /// Does normalized DCT-II.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void DirectNorm(float[] input, float[] output)
        {
            Array.Clear(output, 0, output.Length);

            for (int m = 0; m < _temp.Length / 2; m++)
            {
                _temp[m] = input[2 * m];
                _temp[_temp.Length - 1 - m] = input[2 * m + 1];
            }

            _fft.Direct(_temp, output);

            // mutiply by exp(-j * pi * n / 2N):

            int N = _fft.Size;
            float norm = MathF.Sqrt(0.5f / N);

            for (int i = 0; i < N; i++)
            {
                output[i] = 2 * norm * (_temp[i] * MathF.Cos(0.5f * ConstantsFp32.PI * i / N) - output[i] * MathF.Sin(-0.5f * ConstantsFp32.PI * i / N));
            }
            
            output[0] *= MathF.Sqrt(0.5f);
        }

        /// <summary>
        /// Does Inverse DCT-II.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Inverse(float[] input, float[] output)
        {
            // multiply by exp(j * pi * n / 2N):

            int N = _fft.Size;
            for (int i = 0; i < N; i++)
            {
                _temp[i] = input[i] * MathF.Cos(0.5f * ConstantsFp32.PI * i / N);
                output[i] = input[i] * MathF.Sin(0.5f * ConstantsFp32.PI * i / N);
            }
            _temp[0] *= 0.5f;
            output[0] *= 0.5f;

            _fft.Inverse(_temp, output);

            for (int m = 0; m < _temp.Length / 2; m++)
            {
                output[2 * m]     = 2 * _temp[m];
                output[2 * m + 1] = 2 * _temp[N - 1 - m];
            }
        }

        /// <summary>
        /// Does normalized Inverse DCT-II.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void InverseNorm(float[] input, float[] output)
        {
            Inverse(input, output);

            var norm0 = (1 / MathF.Sqrt(_fft.Size));
            var norm = MathF.Sqrt(0.5f / _fft.Size);

            for (var i = 0; i < output.Length; i++)
            {
                output[i] = (output[i] - input[0]) * norm + input[0] * norm0;
            }
        }
    }
}
