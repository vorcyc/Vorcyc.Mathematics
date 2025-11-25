using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Fda
{
    /// <summary>
    /// Provides methods for filter design and analysis.
    /// </summary>
    public static partial class DesignFilter
    {
        #region Iir(Notch|Peak|Comb) filter design

        /// <summary>
        /// Designs IIR notch filter.
        /// </summary>
        /// <param name="frequency">Normalized center frequency in range [0..0.5]</param>
        /// <param name="q">Q factor (characterizes notch filter -3dB bandwidth relative to its center frequency)</param>
        public static TransferFunction IirNotch(float frequency, float q = 20.0f)
        {
            Guard.AgainstInvalidRange(frequency, 0, 0.5, "Center frequency");

            var w0 = 2 * frequency * ConstantsFp32.PI;
            var bw = w0 / q;
            var gb = 1 / MathF.Sqrt(2);

            var beta = MathF.Sqrt(1 - gb * gb) / gb * MathF.Tan(bw / 2);

            var gain = 1 / (1 + beta);

            var num = new[] { gain, -2 * MathF.Cos(w0) * gain, gain };
            var den = new[] { 1, -2 * MathF.Cos(w0) * gain, 2 * gain - 1 };

            return new TransferFunction(num, den);
        }

        /// <summary>
        /// Designs IIR peak filter.
        /// </summary>
        /// <param name="frequency">Normalized center frequency in range [0..0.5]</param>
        /// <param name="q">Q factor (characterizes peak filter -3dB bandwidth relative to its center frequency)</param>
        public static TransferFunction IirPeak(float frequency, float q = 20.0f)
        {
            Guard.AgainstInvalidRange(frequency, 0, 0.5, "Center frequency");

            var w0 = 2 * frequency * ConstantsFp32.PI;
            var bw = w0 / q;
            var gb = 1 / MathF.Sqrt(2);

            var beta = gb / MathF.Sqrt(1 - gb * gb) * MathF.Tan(bw / 2);

            var gain = 1 / (1 + beta);

            var num = new[] { 1 - gain, 0, gain - 1 };
            var den = new[] { 1, -2 * MathF.Cos(w0) * gain, 2 * gain - 1 };

            return new TransferFunction(num, den);
        }

        /// <summary>
        /// Designs IIR comb notch filter.
        /// </summary>
        /// <param name="frequency">Normalized center frequency in range [0..0.5]</param>
        /// <param name="q">Q factor (characterizes notch filter -3dB bandwidth relative to its center frequency)</param>
        public static TransferFunction IirCombNotch(float frequency, float q = 20.0f)
        {
            Guard.AgainstInvalidRange(frequency, 0, 0.5, "Center frequency");

            var w0 = 2 * frequency * ConstantsFp32.PI;
            var bw = w0 / q;
            var gb = 1 / MathF.Sqrt(2);

            var N = (int)(1 / frequency);

            var beta = MathF.Sqrt((1 - gb * gb) / (gb * gb)) * MathF.Tan(N * bw / 4);

            var num = new float[N + 1];
            var den = new float[N + 1];

            num[0] = 1 / (1 + beta);
            num[num.Length - 1] = -1 / (1 + beta);

            den[0] = 1;
            den[den.Length - 1] = -(1 - beta) / (1 + beta);

            return new TransferFunction(num, den);
        }

        /// <summary>
        /// Designs IIR comb peak filter.
        /// </summary>
        /// <param name="frequency">Normalized center frequency in range [0..0.5]</param>
        /// <param name="q">Q factor (characterizes peak filter -3dB bandwidth relative to its center frequency)</param>
        public static TransferFunction IirCombPeak(float frequency, float q = 20.0f)
        {
            Guard.AgainstInvalidRange(frequency, 0, 0.5, "Center frequency");

            var w0 = 2 * frequency * ConstantsFp32.PI;
            var bw = w0 / q;
            var gb = 1 / MathF.Sqrt(2);

            var N = (int)(1 / frequency);

            var beta = MathF.Sqrt(gb * gb / (1 - gb * gb)) * MathF.Tan(N * bw / 4);

            var num = new float[N + 1];
            var den = new float[N + 1];

            num[0] = beta / (1 + beta);
            num[num.Length - 1] = -beta / (1 + beta);

            den[0] = 1;
            den[den.Length - 1] = (1 - beta) / (1 + beta);

            return new TransferFunction(num, den);
        }

        #endregion


        #region design transfer functions for IIR pole filters (Butterworth, Chebyshev, etc.)

        /// <summary>
        /// Designs lowpass pole filter.
        /// </summary>
        /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
        /// <param name="poles">Analog prototype poles</param>
        /// <param name="zeros">Analog prototype zeros</param>
        public static TransferFunction IirLpTf(float frequency, ComplexFp32[] poles, ComplexFp32[]? zeros = null)
        {
            Guard.AgainstInvalidRange(frequency, 0, 0.5f, "Cutoff frequency");

            var pre = new float[poles.Length];
            var pim = new float[poles.Length];

            var warpedFreq = MathF.Tan(ConstantsFp32.PI * frequency);

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < poles.Length; k++)
            {
                var p = warpedFreq * poles[k];
                pre[k] = p.Real;
                pim[k] = p.Imaginary;
            }

            // 2) switch to z-domain

            VMath.BilinearTransform(pre, pim);


            // === if zeros are also specified do the same steps 1-2 with zeros ===

            float[] zre, zim;

            if (zeros != null)
            {
                zre = new float[zeros.Length];
                zim = new float[zeros.Length];

                for (var k = 0; k < zeros.Length; k++)
                {
                    var z = warpedFreq * zeros[k];
                    zre[k] = z.Real;
                    zim[k] = z.Imaginary;
                }

                VMath.BilinearTransform(zre, zim);
            }
            // otherwise create zeros (same amount as poles) and set them all to -1
            else
            {
                zre = Enumerable.Repeat(-1.0f, poles.Length).ToArray();
                zim = new float[poles.Length];
            }

            // ===


            // 3) return TF with normalized coefficients

            var tf = new TransferFunction(new ComplexDiscreteSignal(1, zre, zim),
                                          new ComplexDiscreteSignal(1, pre, pim));
            tf.NormalizeAt(0);

            return tf;
        }

        /// <summary>
        /// Designs highpass pole filter.
        /// </summary>
        /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
        /// <param name="poles">Analog prototype poles</param>
        /// <param name="zeros">Analog prototype zeros</param>
        public static TransferFunction IirHpTf(float frequency, ComplexFp32[] poles, ComplexFp32[] zeros = null)
        {
            Guard.AgainstInvalidRange(frequency, 0, 0.5f, "Cutoff frequency");

            var pre = new float[poles.Length];
            var pim = new float[poles.Length];

            var warpedFreq = MathF.Tan(ConstantsFp32.PI * frequency);

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < poles.Length; k++)
            {
                var p = warpedFreq / poles[k];
                pre[k] = p.Real;
                pim[k] = p.Imaginary;
            }

            // 2) switch to z-domain

            VMath.BilinearTransform(pre, pim);


            // === if zeros are also specified do the same steps 1-2 with zeros ===

            float[] zre, zim;

            if (zeros != null)
            {
                zre = new float[zeros.Length];
                zim = new float[zeros.Length];

                for (var k = 0; k < zeros.Length; k++)
                {
                    var z = warpedFreq / zeros[k];
                    zre[k] = z.Real;
                    zim[k] = z.Imaginary;
                }

                VMath.BilinearTransform(zre, zim);
            }
            // otherwise create zeros (same amount as poles) and set them all to -1
            else
            {
                zre = Enumerable.Repeat(1.0f, poles.Length).ToArray();
                zim = new float[poles.Length];
            }

            // ===


            // 3) return TF with normalized coefficients

            var tf = new TransferFunction(new ComplexDiscreteSignal(1, zre, zim),
                                          new ComplexDiscreteSignal(1, pre, pim));
            tf.NormalizeAt(ConstantsFp32.PI);

            return tf;
        }

        /// <summary>
        /// Designs bandpass pole filter.
        /// </summary>
        /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
        /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
        /// <param name="poles">Analog prototype poles</param>
        /// <param name="zeros">Analog prototype zeros</param>
        public static TransferFunction IirBpTf(float frequencyLow, float frequencyHigh, ComplexFp32[] poles, ComplexFp32[]? zeros = null)
        {
            Guard.AgainstInvalidRange(frequencyLow, 0, 0.5f, "lower frequency");
            Guard.AgainstInvalidRange(frequencyHigh, 0, 0.5f, "upper frequency");
            Guard.AgainstInvalidRange(frequencyLow, frequencyHigh, "lower frequency", "upper frequency");

            var pre = new float[poles.Length * 2];
            var pim = new float[poles.Length * 2];

            var centerFreq = 2 * ConstantsFp32.PI * (frequencyLow + frequencyHigh) / 2;

            var warpedFreq1 = MathF.Tan(ConstantsFp32.PI * frequencyLow);
            var warpedFreq2 = MathF.Tan(ConstantsFp32.PI * frequencyHigh);

            var f0 = MathF.Sqrt(warpedFreq1 * warpedFreq2);
            var bw = warpedFreq2 - warpedFreq1;

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < poles.Length; k++)
            {
                var alpha = bw / 2 * poles[k];
                var beta = ComplexFp32.Sqrt(1 - ComplexFp32.Pow(f0 / alpha, 2));

                var p1 = alpha * (1 + beta);
                pre[k] = p1.Real;
                pim[k] = p1.Imaginary;

                var p2 = alpha * (1 - beta);
                pre[poles.Length + k] = p2.Real;
                pim[poles.Length + k] = p2.Imaginary;
            }

            // 2) switch to z-domain

            VMath.BilinearTransform(pre, pim);


            // === if zeros are also specified do the same steps 1-2 with zeros ===

            float[] zre, zim;

            if (zeros != null)
            {
                zre = new float[zeros.Length * 2];
                zim = new float[zeros.Length * 2];

                for (var k = 0; k < zeros.Length; k++)
                {
                    var alpha = bw / 2 * zeros[k];
                    var beta = ComplexFp32.Sqrt(1 - ComplexFp32.Pow(f0 / alpha, 2));

                    var z1 = alpha * (1 + beta);
                    zre[k] = z1.Real;
                    zim[k] = z1.Imaginary;

                    var z2 = alpha * (1 - beta);
                    zre[zeros.Length + k] = z2.Real;
                    zim[zeros.Length + k] = z2.Imaginary;
                }

                VMath.BilinearTransform(zre, zim);
            }
            // otherwise create zeros (same amount as poles) and set them all to [-1, -1, -1, ..., 1, 1, 1]
            else
            {
                zre = Enumerable.Repeat(-1.0f, poles.Length)
                                .Concat(Enumerable.Repeat(1.0f, poles.Length))
                                .ToArray();
                zim = new float[poles.Length * 2];
            }

            // ===


            // 3) return TF with normalized coefficients

            var tf = new TransferFunction(new ComplexDiscreteSignal(1, zre, zim),
                                          new ComplexDiscreteSignal(1, pre, pim));
            tf.NormalizeAt(centerFreq);

            return tf;
        }

        /// <summary>
        /// Designs bandstop pole filter.
        /// </summary>
        /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
        /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
        /// <param name="poles">Analog prototype poles</param>
        /// <param name="zeros">Analog prototype zeros</param>
        public static TransferFunction IirBsTf(float frequencyLow, float frequencyHigh, ComplexFp32[] poles, ComplexFp32[]? zeros = null)
        {
            Guard.AgainstInvalidRange(frequencyLow, 0, 0.5f, "lower frequency");
            Guard.AgainstInvalidRange(frequencyHigh, 0, 0.5f, "upper frequency");
            Guard.AgainstInvalidRange(frequencyLow, frequencyHigh, "lower frequency", "upper frequency");

            // Calculation of filter coefficients is based on Neil Robertson's post:
            // https://www.dsprelated.com/showarticle/1131.php

            var pre = new float[poles.Length * 2];
            var pim = new float[poles.Length * 2];

            var f1 = MathF.Tan(ConstantsFp32.PI * frequencyLow);
            var f2 = MathF.Tan(ConstantsFp32.PI * frequencyHigh);
            var f0 = MathF.Sqrt(f1 * f2);
            var bw = f2 - f1;

            var centerFreq = 2 * MathF.Atan(f0);


            // 1) poles and zeros of analog filter (scaled)

            for (var k = 0; k < poles.Length; k++)
            {
                var alpha = bw / 2 / poles[k];
                var beta = ComplexFp32.Sqrt(1 - ComplexFp32.Pow(f0 / alpha, 2));

                var p1 = alpha * (1 + beta);
                pre[k] = p1.Real;
                pim[k] = p1.Imaginary;

                var p2 = alpha * (1 - beta);
                pre[poles.Length + k] = p2.Real;
                pim[poles.Length + k] = p2.Imaginary;
            }

            // 2) switch to z-domain

            VMath.BilinearTransform(pre, pim);


            // === if zeros are also specified do the same steps 1-2 with zeros ===

            float[] zre, zim;

            if (zeros != null)
            {
                zre = new float[zeros.Length * 2];
                zim = new float[zeros.Length * 2];

                for (var k = 0; k < zeros.Length; k++)
                {
                    var alpha = bw / 2 / zeros[k];
                    var beta = ComplexFp32.Sqrt(1 - ComplexFp32.Pow(f0 / alpha, 2));

                    var z1 = alpha * (1 + beta);
                    zre[k] = z1.Real;
                    zim[k] = z1.Imaginary;

                    var z2 = alpha * (1 - beta);
                    zre[zeros.Length + k] = z2.Real;
                    zim[zeros.Length + k] = z2.Imaginary;
                }

                VMath.BilinearTransform(zre, zim);
            }
            // otherwise create zeros (same amount as poles) and set the following values:
            else
            {
                zre = new float[poles.Length * 2];
                zim = new float[poles.Length * 2];

                for (var k = 0; k < poles.Length; k++)
                {
                    zre[k] = MathF.Cos(centerFreq);
                    zim[k] = MathF.Sin(centerFreq);
                    zre[poles.Length + k] = MathF.Cos(-centerFreq);
                    zim[poles.Length + k] = MathF.Sin(-centerFreq);
                }
            }

            // ===


            // 3) return TF with normalized coefficients

            var tf = new TransferFunction(new ComplexDiscreteSignal(1, zre, zim),
                                          new ComplexDiscreteSignal(1, pre, pim));
            tf.NormalizeAt(0);

            return tf;
        }

        #endregion


        #region second order sections

        private static readonly Func<ComplexFp32, bool> Any = c => true;
        private static readonly Func<ComplexFp32, bool> IsReal = c => MathF.Abs(c.Imaginary) < 1e-10f;
        private static readonly Func<ComplexFp32, bool> IsComplex = c => MathF.Abs(c.Imaginary) > 1e-10f;

        /// <summary>
        /// Converts second-order sections to transfer function (zeros-poles-gain).
        /// </summary>
        /// <param name="sos">Array of SOS transfer functions</param>
        public static TransferFunction SosToTf(TransferFunction[] sos)
        {
            return sos.Aggregate((tf, s) => tf * s);
        }

        /// <summary>
        /// Converts transfer function (zeros-poles-gain) to second-order sections.
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public static TransferFunction[] TfToSos(TransferFunction tf)
        {
            var zeros = tf.Zeros.ToList();
            var poles = tf.Poles.ToList();

            if (zeros.Count != poles.Count)
            {
                if (zeros.Count > poles.Count) poles.AddRange(new ComplexFp32[zeros.Count - poles.Count]);
                if (zeros.Count < poles.Count) zeros.AddRange(new ComplexFp32[poles.Count - zeros.Count]);
            }

            var sosCount = (poles.Count + 1) / 2;

            if (poles.Count % 2 == 1)
            {
                zeros.Add(ComplexFp32.Zero);
                poles.Add(ComplexFp32.Zero);
            }

            RemoveConjugated(zeros);
            RemoveConjugated(poles);

            var gains = new float[sosCount];
            gains[0] = tf.Gain;
            for (var i = 1; i < gains.Length; i++) gains[i] = 1;

            var sos = new TransferFunction[sosCount];

            // reverse order of sections

            for (var i = sosCount - 1; i >= 0; i--)
            {
                ComplexFp32 z1, z2, p1, p2;

                // Select the next pole closest to unit circle

                var pos = ClosestToUnitCircle(poles, Any);
                p1 = poles[pos];
                poles.RemoveAt(pos);

                if (IsReal(p1) && poles.All(IsComplex))
                {
                    pos = ClosestToComplexValue(zeros, p1, IsReal);     // closest to pole p1
                    z1 = zeros[pos];
                    zeros.RemoveAt(pos);

                    p2 = ComplexFp32.Zero;
                    z2 = ComplexFp32.Zero;
                }
                else
                {
                    if (IsComplex(p1) && zeros.Count(IsReal) == 1)
                    {
                        pos = ClosestToComplexValue(zeros, p1, IsComplex);
                    }
                    else
                    {
                        pos = ClosestToComplexValue(zeros, p1, Any);
                    }

                    z1 = zeros[pos];
                    zeros.RemoveAt(pos);

                    if (IsComplex(p1))
                    {
                        p2 = ComplexFp32.Conjugate(p1);

                        if (IsComplex(z1))
                        {
                            z2 = ComplexFp32.Conjugate(z1);
                        }
                        else
                        {
                            pos = ClosestToComplexValue(zeros, p1, IsReal);
                            z2 = zeros[pos];
                            zeros.RemoveAt(pos);
                        }
                    }
                    else
                    {
                        if (IsComplex(z1))
                        {
                            z2 = ComplexFp32.Conjugate(z1);

                            pos = ClosestToComplexValue(poles, z1, IsReal);
                            p2 = poles[pos];
                            poles.RemoveAt(pos);
                        }
                        else
                        {
                            pos = ClosestToUnitCircle(poles, IsReal);
                            p2 = poles[pos];
                            poles.RemoveAt(pos);

                            pos = ClosestToComplexValue(zeros, p2, IsReal);
                            z2 = zeros[pos];
                            zeros.RemoveAt(pos);
                        }
                    }
                }

                sos[i] = new TransferFunction(new[] { z1, z2 }, new[] { p1, p2 }, gains[i]);
            }

            return sos;
        }

        private static int ClosestToComplexValue(List<ComplexFp32> arr, ComplexFp32 value, Func<ComplexFp32, bool> condition)
        {
            var pos = 0;
            var minDistance = float.MaxValue;

            for (var i = 0; i < arr.Count; i++)
            {
                if (!condition(arr[i])) continue;

                var distance = ComplexFp32.Abs(arr[i] - value);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    pos = i;
                }
            }

            return pos;
        }

        private static int ClosestToUnitCircle(List<ComplexFp32> arr, Func<ComplexFp32, bool> condition)
        {
            var pos = 0;
            var minDistance = float.MaxValue;

            for (var i = 0; i < arr.Count; i++)
            {
                if (!condition(arr[i])) continue;

                var distance = ComplexFp32.Abs(ComplexFp32.Abs(arr[i]) - 1.0);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    pos = i;
                }
            }

            return pos;
        }

        /// <summary>
        /// Leaves only one of two conjugated numbers in the list of complex numbers.
        /// </summary>
        /// <param name="c">List of complex numbers</param>
        private static void RemoveConjugated(List<ComplexFp32> c)
        {
            for (var i = 0; i < c.Count; i++)
            {
                if (IsReal(c[i])) continue;

                var j = i + 1;
                for (; j < c.Count; j++)
                {
                    if (MathF.Abs(c[i].Real - c[j].Real) < 1e-10f &&
                        MathF.Abs(c[i].Imaginary + c[j].Imaginary) < 1e-10f)
                    {
                        break;
                    }
                }

                if (j == c.Count)
                {
                    throw new ArgumentException($"Complex array does not contain conjugated pair for {c[i]}");
                }

                c.RemoveAt(j);
            }
        }

        #endregion
    }
}
