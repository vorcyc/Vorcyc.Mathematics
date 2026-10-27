using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.SignalProcessing.Operations;

public static partial class Operation
{
    /// <summary>
    /// Cross power spectral density of <paramref name="x"/> and <paramref name="y"/> by Welch's method.
    /// Complete windows only. Onesided scaling matches <see cref="Welch"/>.
    /// <c>Pxy = conj(X) Y</c>, so a transfer function from <paramref name="x"/> to <paramref name="y"/> is <c>Pxy / Pxx</c>.
    /// </summary>
    public static ComplexFp32[] Cpsd(
        Signal x,
        Signal y,
        int windowSize = 1024,
        int hopSize = 256,
        WindowType window = WindowType.Hann,
        int fftSize = 0,
        int samplingRate = 0)
    {
        var estimate = WelchCross(x, y, windowSize, hopSize, window, fftSize);
        return ScaleLikeWelch(estimate.Cross, estimate.Window, estimate.WindowSize, samplingRate);
    }

    /// <summary>
    /// Magnitude-squared coherence of <paramref name="x"/> and <paramref name="y"/>.
    /// </summary>
    public static float[] Mscohere(
        Signal x,
        Signal y,
        int windowSize = 1024,
        int hopSize = 256,
        WindowType window = WindowType.Hann,
        int fftSize = 0,
        int samplingRate = 0)
    {
        _ = samplingRate;
        var estimate = WelchCross(x, y, windowSize, hopSize, window, fftSize);
        var coherence = new float[estimate.PowerX.Length];
        for (int k = 0; k < coherence.Length; k++)
        {
            float pxx = estimate.PowerX[k];
            float pyy = estimate.PowerY[k];
            if (pxx <= 0f || pyy <= 0f)
                continue;

            float real = estimate.Cross[k].Real;
            float imag = estimate.Cross[k].Imaginary;
            coherence[k] = (real * real + imag * imag) / (pxx * pyy);
        }

        return coherence;
    }

    /// <summary>
    /// Non-parametric frequency-response estimate from <paramref name="input"/> to <paramref name="output"/>.
    /// This is <c>Pxy / Pxx</c>, not a parametric transfer-function fit.
    /// </summary>
    public static ComplexFp32[] TfEstimate(
        Signal input,
        Signal output,
        int windowSize = 1024,
        int hopSize = 256,
        WindowType window = WindowType.Hann,
        int fftSize = 0,
        int samplingRate = 0)
    {
        _ = samplingRate;
        var estimate = WelchCross(input, output, windowSize, hopSize, window, fftSize);
        var response = new ComplexFp32[estimate.PowerX.Length];
        for (int k = 0; k < response.Length; k++)
        {
            float pxx = estimate.PowerX[k];
            if (pxx <= 0f)
                continue;

            response[k] = new ComplexFp32(estimate.Cross[k].Real / pxx, estimate.Cross[k].Imaginary / pxx);
        }

        return response;
    }

    /// <summary>
    /// Thomson multitaper spectrum. Segments longer than 512 samples are averaged.
    /// <paramref name="taperCount"/> &lt;= 0 selects <c>2 * floor(timeBandwidth) - 1</c> Slepian tapers.
    /// </summary>
    public static float[] Pmtm(
        Signal signal,
        double timeBandwidth = 4.0,
        int taperCount = 0,
        int windowSize = 0,
        int hopSize = 0,
        int fftSize = 0,
        int samplingRate = 0)
    {
        ArgumentNullException.ThrowIfNull(signal);
        if (timeBandwidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeBandwidth));

        float[] samples = signal.Samples.ToArray();
        if (windowSize <= 0)
            windowSize = samples.Length <= 512 ? samples.Length : 512;
        if (windowSize < 8 || windowSize > samples.Length)
            throw new ArgumentException("The multitaper window must fit inside the signal and contain at least 8 samples.");
        if (hopSize <= 0)
            hopSize = Math.Max(1, windowSize / 2);

        int resolvedFft = ResolveFftSize(windowSize, fftSize);
        int tapers = taperCount > 0 ? taperCount : Math.Max(1, 2 * (int)Math.Floor(timeBandwidth) - 1);
        tapers = Math.Min(tapers, windowSize - 1);

        var (windows, lambdas) = DiscreteProlateSpheroidal(windowSize, timeBandwidth, tapers);
        int frames = (samples.Length - windowSize) / hopSize + 1;
        int bins = resolvedFft / 2 + 1;
        var accumulator = new double[bins];
        var fft = new RealFft(resolvedFft);
        var frame = new float[resolvedFft];
        var real = new float[bins];
        var imag = new float[bins];
        var taperedPower = new double[tapers][];
        for (int taper = 0; taper < tapers; taper++)
            taperedPower[taper] = new double[bins];

        for (int frameIndex = 0; frameIndex < frames; frameIndex++)
        {
            int offset = frameIndex * hopSize;
            double mean = 0;
            for (int i = 0; i < windowSize; i++)
                mean += samples[offset + i];
            mean /= windowSize;
            double variance = 0;
            for (int i = 0; i < windowSize; i++)
            {
                double delta = samples[offset + i] - mean;
                variance += delta * delta;
            }
            variance = Math.Max(variance / windowSize, 1e-12);

            for (int taper = 0; taper < tapers; taper++)
            {
                Array.Clear(frame);
                for (int i = 0; i < windowSize; i++)
                    frame[i] = (float)(windows[taper][i] * samples[offset + i]);
                fft.Direct(frame, real, imag);
                for (int bin = 0; bin < bins; bin++)
                    taperedPower[taper][bin] = (real[bin] * (double)real[bin] + imag[bin] * (double)imag[bin]) / resolvedFft;
            }

            var combined = AdaptiveCombine(taperedPower, lambdas, variance);
            for (int bin = 0; bin < bins; bin++)
                accumulator[bin] += combined[bin];
        }

        var spectrum = new float[bins];
        double invFrames = 1.0 / frames;
        for (int bin = 0; bin < bins; bin++)
        {
            double value = accumulator[bin] * invFrames;
            if (samplingRate > 0)
                value /= samplingRate;
            spectrum[bin] = (float)value;
        }

        ApplyOnesidedDoubling(spectrum);
        return spectrum;
    }

    /// <summary>
    /// Yule–Walker autoregressive power spectrum. The record is demeaned.
    /// Polynomial coefficients follow <c>1 + a1 z^{-1} + …</c>, and the spectrum is <c>σ² / |A|²</c>.
    /// </summary>
    public static float[] Pyulear(Signal signal, int order, int fftSize = 256, int samplingRate = 0)
    {
        double[] samples = Demeaned(signal);
        var (coefficients, sigma2) = YuleWalker(samples, order);
        return ArSpectrum(coefficients, sigma2, fftSize, samplingRate);
    }

    /// <summary>Burg autoregressive power spectrum. The record is demeaned.</summary>
    public static float[] Pburg(Signal signal, int order, int fftSize = 256, int samplingRate = 0)
    {
        double[] samples = Demeaned(signal);
        var (coefficients, sigma2) = Burg(samples, order);
        return ArSpectrum(coefficients, sigma2, fftSize, samplingRate);
    }

    /// <summary>Covariance-method autoregressive power spectrum. The record is demeaned.</summary>
    public static float[] Pcov(Signal signal, int order, int fftSize = 256, int samplingRate = 0)
    {
        double[] samples = Demeaned(signal);
        var (coefficients, sigma2) = Covariance(samples, order);
        return ArSpectrum(coefficients, sigma2, fftSize, samplingRate);
    }

    readonly struct WelchCrossEstimate(float[] powerX, float[] powerY, ComplexFp32[] cross, WindowType window, int windowSize)
    {
        public float[] PowerX { get; } = powerX;
        public float[] PowerY { get; } = powerY;
        public ComplexFp32[] Cross { get; } = cross;
        public WindowType Window { get; } = window;
        public int WindowSize { get; } = windowSize;
    }

    static WelchCrossEstimate WelchCross(Signal x, Signal y, int windowSize, int hopSize, WindowType window, int fftSize)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        if (x.Samples.Length != y.Samples.Length)
            throw new ArgumentException("The two signals must have the same length.");
        if (windowSize <= 0 || hopSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(windowSize));

        float[] left = x.Samples.ToArray();
        float[] right = y.Samples.ToArray();
        int resolvedFft = ResolveFftSize(windowSize, fftSize);
        int frames = left.Length >= windowSize ? (left.Length - windowSize) / hopSize + 1 : 0;
        int bins = resolvedFft / 2 + 1;
        var pxx = new double[bins];
        var pyy = new double[bins];
        var real = new double[bins];
        var imag = new double[bins];
        if (frames > 0)
        {
            var windowSamples = WindowBuilder.OfType(window, windowSize);
            var fft = new RealFft(resolvedFft);
            var frameX = new float[resolvedFft];
            var frameY = new float[resolvedFft];
            var reX = new float[bins];
            var imX = new float[bins];
            var reY = new float[bins];
            var imY = new float[bins];

            for (int frame = 0; frame < frames; frame++)
            {
                int offset = frame * hopSize;
                Array.Clear(frameX);
                Array.Clear(frameY);
                left.AsSpan(offset, windowSize).CopyTo(frameX);
                right.AsSpan(offset, windowSize).CopyTo(frameY);
                if (window != WindowType.Rectangular)
                {
                    frameX.AsSpan().ApplyWindow(windowSamples);
                    frameY.AsSpan().ApplyWindow(windowSamples);
                }

                fft.Direct(frameX, reX, imX);
                fft.Direct(frameY, reY, imY);
                for (int bin = 0; bin < bins; bin++)
                {
                    pxx[bin] += reX[bin] * (double)reX[bin] + imX[bin] * (double)imX[bin];
                    pyy[bin] += reY[bin] * (double)reY[bin] + imY[bin] * (double)imY[bin];
                    real[bin] += reX[bin] * (double)reY[bin] + imX[bin] * (double)imY[bin];
                    imag[bin] += reX[bin] * (double)imY[bin] - imX[bin] * (double)reY[bin];
                }
            }

            double inv = 1.0 / frames;
            for (int bin = 0; bin < bins; bin++)
            {
                pxx[bin] *= inv;
                pyy[bin] *= inv;
                real[bin] *= inv;
                imag[bin] *= inv;
            }
        }

        var powerX = new float[bins];
        var powerY = new float[bins];
        var cross = new ComplexFp32[bins];
        for (int bin = 0; bin < bins; bin++)
        {
            powerX[bin] = (float)pxx[bin];
            powerY[bin] = (float)pyy[bin];
            cross[bin] = new ComplexFp32((float)real[bin], (float)imag[bin]);
        }

        return new WelchCrossEstimate(powerX, powerY, cross, window, windowSize);
    }

    static ComplexFp32[] ScaleLikeWelch(ComplexFp32[] spectrum, WindowType window, int windowSize, int samplingRate)
    {
        float[] weights = WindowBuilder.OfType(window, windowSize);
        float scale;
        if (samplingRate > 0)
        {
            float energy = 0;
            for (int i = 0; i < weights.Length; i++)
                energy += weights[i] * weights[i];
            scale = 2f / (energy * samplingRate);
        }
        else
        {
            float sum = 0;
            for (int i = 0; i < weights.Length; i++)
                sum += weights[i];
            scale = 2f / (sum * sum);
        }

        var scaled = new ComplexFp32[spectrum.Length];
        if (scaled.Length == 0)
            return scaled;

        float half = scale * 0.5f;
        scaled[0] = new ComplexFp32(spectrum[0].Real * half, spectrum[0].Imaginary * half);
        int last = scaled.Length - 1;
        for (int bin = 1; bin < last; bin++)
            scaled[bin] = new ComplexFp32(spectrum[bin].Real * scale, spectrum[bin].Imaginary * scale);
        if (last > 0)
            scaled[last] = new ComplexFp32(spectrum[last].Real * half, spectrum[last].Imaginary * half);
        return scaled;
    }

    static int ResolveFftSize(int windowSize, int fftSize)
    {
        int size = fftSize >= windowSize ? fftSize : windowSize.NextPowerOf2();
        if (size < 2)
            size = 2;
        if (size < 2 || (size & (size - 1)) != 0)
            size = Math.Max(size, 2).NextPowerOf2();
        return size;
    }

    static void ApplyOnesidedDoubling(float[] spectrum)
    {
        for (int bin = 1; bin < spectrum.Length - 1; bin++)
            spectrum[bin] *= 2f;
    }

    static double[] Demeaned(Signal signal)
    {
        ArgumentNullException.ThrowIfNull(signal);
        var samples = new double[signal.Samples.Length];
        double mean = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = signal.Samples[i];
            mean += samples[i];
        }

        if (samples.Length == 0)
            throw new ArgumentException("The signal is empty.");

        mean /= samples.Length;
        for (int i = 0; i < samples.Length; i++)
            samples[i] -= mean;
        return samples;
    }

    static float[] ArSpectrum(double[] coefficients, double sigma2, int fftSize, int samplingRate)
    {
        if (fftSize < 2)
            throw new ArgumentOutOfRangeException(nameof(fftSize));

        int bins = fftSize / 2 + 1;
        var spectrum = new float[bins];
        for (int bin = 0; bin < bins; bin++)
        {
            double omega = 2.0 * Math.PI * bin / fftSize;
            double real = 0;
            double imag = 0;
            for (int lag = 0; lag < coefficients.Length; lag++)
            {
                real += coefficients[lag] * Math.Cos(omega * lag);
                imag -= coefficients[lag] * Math.Sin(omega * lag);
            }

            double power = sigma2 / Math.Max(real * real + imag * imag, 1e-18);
            if (samplingRate > 0)
                power /= samplingRate;
            spectrum[bin] = (float)power;
        }

        ApplyOnesidedDoubling(spectrum);
        return spectrum;
    }

    static (double[] Coefficients, double Sigma2) YuleWalker(double[] samples, int order)
    {
        RequireOrder(samples.Length, order);
        var correlation = new double[order + 1];
        for (int lag = 0; lag <= order; lag++)
        {
            double sum = 0;
            for (int i = 0; i < samples.Length - lag; i++)
                sum += samples[i] * samples[i + lag];
            correlation[lag] = sum / samples.Length;
        }

        var coefficients = new double[order + 1];
        coefficients[0] = 1;
        double error = correlation[0];
        if (error <= 0)
            return (coefficients, 0);

        for (int i = 1; i <= order; i++)
        {
            double reflection = 0;
            for (int j = 0; j < i; j++)
                reflection -= coefficients[j] * correlation[i - j];
            reflection /= error;

            var updated = new double[order + 1];
            updated[0] = 1;
            for (int j = 1; j < i; j++)
                updated[j] = coefficients[j] + reflection * coefficients[i - j];
            updated[i] = reflection;
            Array.Copy(updated, coefficients, i + 1);
            error *= 1.0 - reflection * reflection;
        }

        return (coefficients, Math.Max(error, 0));
    }

    static (double[] Coefficients, double Sigma2) Burg(double[] samples, int order)
    {
        RequireOrder(samples.Length, order);
        int n = samples.Length;
        var forward = (double[])samples.Clone();
        var backward = (double[])samples.Clone();
        var coefficients = new double[order + 1];
        coefficients[0] = 1;
        double power = 0;
        for (int i = 0; i < n; i++)
            power += samples[i] * samples[i];
        power /= n;

        for (int stage = 1; stage <= order; stage++)
        {
            double numerator = 0;
            double denominator = 0;
            for (int t = stage; t < n; t++)
            {
                numerator += forward[t] * backward[t - 1];
                denominator += forward[t] * forward[t] + backward[t - 1] * backward[t - 1];
            }

            double reflection = denominator == 0 ? 0 : -2.0 * numerator / denominator;
            reflection = Math.Clamp(reflection, -0.999, 0.999);

            var updated = new double[order + 1];
            updated[0] = 1;
            for (int j = 1; j < stage; j++)
                updated[j] = coefficients[j] + reflection * coefficients[stage - j];
            updated[stage] = reflection;

            for (int t = n - 1; t >= stage; t--)
            {
                double nextForward = forward[t] + reflection * backward[t - 1];
                double nextBackward = backward[t - 1] + reflection * forward[t];
                forward[t] = nextForward;
                backward[t] = nextBackward;
            }

            power *= 1.0 - reflection * reflection;
            Array.Copy(updated, coefficients, stage + 1);
        }

        return (coefficients, Math.Max(power, 0));
    }

    static (double[] Coefficients, double Sigma2) Covariance(double[] samples, int order)
    {
        RequireOrder(samples.Length, order);
        int n = samples.Length;
        var normal = new double[order, order];
        var rhs = new double[order];
        for (int row = 1; row <= order; row++)
        {
            for (int col = 1; col <= order; col++)
            {
                double sum = 0;
                for (int t = order; t < n; t++)
                    sum += samples[t - row] * samples[t - col];
                normal[row - 1, col - 1] = sum;
            }

            double cross = 0;
            for (int t = order; t < n; t++)
                cross += samples[t - row] * samples[t];
            rhs[row - 1] = -cross;
        }

        double[] solved = DenseLinearAlgebra.Solve(normal, rhs);
        var coefficients = new double[order + 1];
        coefficients[0] = 1;
        Array.Copy(solved, 0, coefficients, 1, order);

        double sse = 0;
        int count = n - order;
        for (int t = order; t < n; t++)
        {
            double error = samples[t];
            for (int lag = 1; lag <= order; lag++)
                error += coefficients[lag] * samples[t - lag];
            sse += error * error;
        }

        return (coefficients, count > 0 ? sse / count : 0);
    }

    static void RequireOrder(int length, int order)
    {
        if (order < 1 || order >= length / 2)
            throw new ArgumentOutOfRangeException(nameof(order), "The autoregressive order must be at least 1 and shorter than half the record.");
    }

    static (double[][] Tapers, double[] Lambdas) DiscreteProlateSpheroidal(int length, double timeBandwidth, int tapers)
    {
        double bandwidth = timeBandwidth / length;
        var kernel = new double[length];
        kernel[0] = 2.0 * bandwidth;
        for (int lag = 1; lag < length; lag++)
            kernel[lag] = Math.Sin(2.0 * Math.PI * bandwidth * lag) / (Math.PI * lag);

        var basis = new double[length, tapers];
        for (int taper = 0; taper < tapers; taper++)
        {
            for (int n = 0; n < length; n++)
                basis[n, taper] = Math.Sqrt(2.0 / (length + 1.0)) * Math.Sin(Math.PI * (taper + 1) * (n + 1) / (length + 1.0));
        }

        OrthonormalizeColumns(basis);
        var image = new double[length, tapers];
        for (int iteration = 0; iteration < 20; iteration++)
        {
            for (int taper = 0; taper < tapers; taper++)
            {
                var column = Column(basis, taper);
                var product = ProlateMultiply(kernel, column);
                for (int n = 0; n < length; n++)
                    image[n, taper] = product[n];
            }

            var gram = new double[tapers, tapers];
            for (int i = 0; i < tapers; i++)
            {
                for (int j = 0; j < tapers; j++)
                {
                    double sum = 0;
                    for (int n = 0; n < length; n++)
                        sum += basis[n, i] * image[n, j];
                    gram[i, j] = sum;
                }
            }

            var ritz = new double[tapers];
            var rotation = new double[tapers, tapers];
            DenseLinearAlgebra.JacobiEigen(gram, ritz, rotation);
            var rotated = DenseLinearAlgebra.Multiply(image, rotation);
            for (int n = 0; n < length; n++)
                for (int taper = 0; taper < tapers; taper++)
                    basis[n, taper] = rotated[n, taper];
            OrthonormalizeColumns(basis);
        }

        var lambdas = new double[tapers];
        var windows = new double[tapers][];
        for (int taper = 0; taper < tapers; taper++)
        {
            var column = Column(basis, taper);
            double sum = 0;
            for (int n = 0; n < length; n++)
                sum += column[n];
            if (sum < 0)
            {
                for (int n = 0; n < length; n++)
                    column[n] = -column[n];
            }

            var concentrated = ProlateMultiply(kernel, column);
            lambdas[taper] = Math.Clamp(DenseLinearAlgebra.Dot(column, concentrated), 0, 1);
            windows[taper] = column;
        }

        return (windows, lambdas);
    }

    static double[] ProlateMultiply(double[] kernel, double[] vector)
    {
        int n = vector.Length;
        var result = new double[n];
        for (int i = 0; i < n; i++)
        {
            double sum = kernel[0] * vector[i];
            for (int j = 0; j < n; j++)
            {
                if (i != j)
                    sum += kernel[Math.Abs(i - j)] * vector[j];
            }

            result[i] = sum;
        }

        return result;
    }

    static double[] Column(double[,] matrix, int column)
    {
        int rows = matrix.GetLength(0);
        var values = new double[rows];
        for (int row = 0; row < rows; row++)
            values[row] = matrix[row, column];
        return values;
    }

    static void OrthonormalizeColumns(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        for (int col = 0; col < cols; col++)
        {
            for (int previous = 0; previous < col; previous++)
            {
                double dot = 0;
                for (int row = 0; row < rows; row++)
                    dot += matrix[row, col] * matrix[row, previous];
                for (int row = 0; row < rows; row++)
                    matrix[row, col] -= dot * matrix[row, previous];
            }

            double norm = 0;
            for (int row = 0; row < rows; row++)
                norm += matrix[row, col] * matrix[row, col];
            norm = Math.Sqrt(norm);
            if (norm < 1e-14)
            {
                for (int row = 0; row < rows; row++)
                    matrix[row, col] = 0;
                matrix[col % rows, col] = 1;
            }
            else
            {
                for (int row = 0; row < rows; row++)
                    matrix[row, col] /= norm;
            }
        }
    }

    static double[] AdaptiveCombine(double[][] taperedPower, double[] lambdas, double variance)
    {
        int tapers = taperedPower.Length;
        int bins = taperedPower[0].Length;
        var spectrum = new double[bins];
        for (int bin = 0; bin < bins; bin++)
        {
            double sum = 0;
            for (int taper = 0; taper < tapers; taper++)
                sum += taperedPower[taper][bin];
            spectrum[bin] = sum / tapers;
        }

        for (int iteration = 0; iteration < 6; iteration++)
        {
            for (int bin = 0; bin < bins; bin++)
            {
                double numerator = 0;
                double denominator = 0;
                for (int taper = 0; taper < tapers; taper++)
                {
                    double lambda = lambdas[taper];
                    double current = Math.Max(spectrum[bin], 1e-18);
                    double weight = lambda * current / (lambda * current + (1.0 - lambda) * variance);
                    numerator += weight * weight * taperedPower[taper][bin];
                    denominator += weight * weight;
                }

                if (denominator > 0)
                    spectrum[bin] = numerator / denominator;
            }
        }

        return spectrum;
    }
}
