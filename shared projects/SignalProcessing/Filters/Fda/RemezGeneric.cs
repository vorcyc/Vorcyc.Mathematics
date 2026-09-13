using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

/// <summary>
/// Parks–McClellan / Remez equiripple designer in <typeparamref name="T"/>.
/// </summary>
public class Remez<T>
    where T : unmanaged, IFloatingPointIeee754<T>
{
    public int Order { get; }
    public int Iterations { get; private set; }
    public int K { get; }
    public T[] InterpolatedResponse { get; }
    public T[] Error { get; }
    public T[] ExtremalFrequencies
    {
        get
        {
            var f = new T[K];
            for (var i = 0; i < K; i++)
                f[i] = _grid[_extrs[i]];
            return f;
        }
    }

    private static readonly T Tolerance = T.CreateChecked(1e-7);
    private readonly int[] _extrs;
    private T[] _grid = [];
    private readonly T[] _freqs;
    private T[] _desired = [];
    private T[] _weights = [];
    private readonly T[] _points;
    private readonly T[] _gammas;
    private readonly T[] _cosTable;

    public Remez(int order, T[] frequencies, T[] desired, T[] weights, int gridDensity = 16)
    {
        Guard.AgainstEvenNumber(order, "The order of the filter");
        GuardFilterParams(frequencies, desired, weights);

        Order = order;
        K = Order / 2 + 2;
        _freqs = frequencies;
        MakeGrid(desired, weights, gridDensity);
        InterpolatedResponse = new T[_grid.Length];
        Error = new T[_grid.Length];
        _extrs = new int[K];
        _points = new T[K];
        _gammas = new T[K];
        _cosTable = new T[K];
    }

    public T[] Design(int maxIterations = 100)
    {
        InitExtrema();
        var extrCandidates = new int[Math.Max(2 * K, _grid.Length)];

        for (Iterations = 0; Iterations < maxIterations; Iterations++)
        {
            UpdateCoefficients();
            for (var i = 0; i < _grid.Length; i++)
                InterpolatedResponse[i] = Lagrange(_grid[i]);
            for (var i = 0; i < _grid.Length; i++)
                Error[i] = _weights[i] * (_desired[i] - InterpolatedResponse[i]);

            var extrCount = 0;
            var n = _grid.Length;
            if (T.Abs(Error[0]) > T.Abs(Error[1]))
                extrCandidates[extrCount++] = 0;
            for (var i = 1; i < n - 1; i++)
            {
                if ((Error[i] > T.Zero && Error[i] >= Error[i - 1] && Error[i] > Error[i + 1]) ||
                    (Error[i] < T.Zero && Error[i] <= Error[i - 1] && Error[i] < Error[i + 1]))
                    extrCandidates[extrCount++] = i;
            }
            if (T.Abs(Error[n - 1]) > T.Abs(Error[n - 2]))
                extrCandidates[extrCount++] = n - 1;

            if (extrCount < K) break;

            while (extrCount > K)
            {
                var indexToRemove = 0;
                for (var i = 1; i < extrCount; i++)
                {
                    if (T.Abs(Error[extrCandidates[i]]) < T.Abs(Error[extrCandidates[indexToRemove]]))
                        indexToRemove = i;
                }
                extrCount--;
                for (var i = indexToRemove; i < extrCount; i++)
                    extrCandidates[i] = extrCandidates[i + 1];
            }

            Array.Copy(extrCandidates, _extrs, K);

            var maxError = T.Abs(Error[0]);
            var minError = maxError;
            for (var k = 0; k < K; k++)
            {
                var error = T.Abs(Error[_extrs[k]]);
                if (error < minError) minError = error;
                if (error > maxError) maxError = error;
            }
            if ((maxError - minError) / minError < T.CreateChecked(1e-6)) break;
        }

        return ImpulseResponse();
    }

    public static T DbToPassbandWeight(T ripple)
    {
        var g = T.Pow(T.CreateChecked(10), ripple / T.CreateChecked(20));
        return (g - T.One) / (g + T.One);
    }

    public static T DbToStopbandWeight(T ripple)
        => T.Pow(T.CreateChecked(10), -ripple / T.CreateChecked(20));

    public static int EstimateOrder(T fp, T fa, T dp, T da)
    {
        if (dp < da)
            (dp, da) = (da, dp);

        var bw = fa - fp;
        var logDp = T.Log10(dp);
        var logDa = T.Log10(da);
        var d = (T.CreateChecked(0.005309) * logDp * logDp + T.CreateChecked(0.07114) * logDp - T.CreateChecked(0.4761)) * logDa
                - (T.CreateChecked(0.00266) * logDp * logDp + T.CreateChecked(0.5941) * logDp + T.CreateChecked(0.4278));
        var f = T.CreateChecked(0.51244) * (logDp - logDa) + T.CreateChecked(11.012);
        var l = (int)double.CreateChecked((d - f * bw * bw) / bw + T.CreateChecked(1.5));
        return l % 2 == 1 ? l : l + 1;
    }

    public static int EstimateOrder(T[] frequencies, T[] deltas)
    {
        var maxOrder = 0;
        for (int fi = 1, di = 0; di < deltas.Length - 1; fi += 2, di++)
        {
            var order = EstimateOrder(frequencies[fi], frequencies[fi + 1], deltas[di], deltas[di + 1]);
            if (order > maxOrder) maxOrder = order;
        }
        return maxOrder;
    }

    private void MakeGrid(T[] desired, T[] weights, int gridDensity)
    {
        var gridSize = 0;
        var bandSizes = new int[_freqs.Length / 2];
        var step = T.CreateChecked(0.5) / T.CreateChecked(gridDensity * (K - 1));
        for (var i = 0; i < bandSizes.Length; i++)
        {
            bandSizes[i] = (int)double.CreateChecked((_freqs[2 * i + 1] - _freqs[2 * i]) / step + T.CreateChecked(0.5));
            gridSize += bandSizes[i];
        }

        _grid = new T[gridSize];
        _weights = new T[gridSize];
        _desired = new T[gridSize];
        var gi = 0;
        for (var i = 0; i < bandSizes.Length; i++)
        {
            var freq = _freqs[2 * i];
            for (var k = 0; k < bandSizes[i]; k++, gi++, freq += step)
            {
                _grid[gi] = freq;
                _weights[gi] = weights[i];
                _desired[gi] = desired[i];
            }
            _grid[gi - 1] = _freqs[2 * i + 1];
        }
    }

    private void InitExtrema()
    {
        var n = _grid.Length;
        for (var k = 0; k < K; k++)
            _extrs[k] = (int)(k * (n - 1.0) / (K - 1));
    }

    private void UpdateCoefficients()
    {
        for (int i = 0; i < _cosTable.Length; i++)
            _cosTable[i] = T.Cos(T.CreateChecked(2) * T.Pi * _grid[_extrs[i]]);

        var num = T.Zero;
        var den = T.Zero;
        for (int i = 0, sign = 1; i < K; i++, sign = -sign)
        {
            _gammas[i] = Gamma(i);
            num += _gammas[i] * _desired[_extrs[i]];
            den += T.CreateChecked(sign) * _gammas[i] / _weights[_extrs[i]];
        }

        var delta = num / den;
        for (int i = 0, sign = 1; i < K; i++, sign = -sign)
            _points[i] = _desired[_extrs[i]] - T.CreateChecked(sign) * delta / _weights[_extrs[i]];
    }

    private T[] ImpulseResponse()
    {
        UpdateCoefficients();
        var halfOrder = Order / 2;
        var lagr = new T[halfOrder + 1];
        for (var i = 0; i <= halfOrder; i++)
            lagr[i] = Lagrange(T.CreateChecked(i) / T.CreateChecked(Order));

        var kernel = new T[Order];
        for (var k = 0; k < Order; k++)
        {
            var sum = T.Zero;
            for (var i = 1; i <= halfOrder; i++)
                sum += lagr[i] * T.Cos(T.CreateChecked(2) * T.Pi * T.CreateChecked(i) * T.CreateChecked(k - halfOrder) / T.CreateChecked(Order));
            kernel[k] = (lagr[0] + T.CreateChecked(2) * sum) / T.CreateChecked(Order);
        }
        return kernel;
    }

    private T Gamma(int k)
    {
        var jet = (K - 1) / 15 + 1;
        var den = T.One;
        for (var j = 0; j < jet; j++)
        {
            for (var i = j; i < K; i += jet)
            {
                if (i != k)
                    den *= T.CreateChecked(2) * (_cosTable[k] - _cosTable[i]);
            }
        }
        if (T.Abs(den) < Tolerance) den = Tolerance;
        return T.One / den;
    }

    private T Lagrange(T freq)
    {
        var num = T.Zero;
        var den = T.Zero;
        var cosFreq = T.Cos(T.CreateChecked(2) * T.Pi * freq);
        for (var i = 0; i < K; i++)
        {
            var cosDiff = cosFreq - _cosTable[i];
            if (T.Abs(cosDiff) < Tolerance) return _points[i];
            cosDiff = _gammas[i] / cosDiff;
            den += cosDiff;
            num += cosDiff * _points[i];
        }
        return num / den;
    }

    private static void GuardFilterParams(T[] freqs, T[] desired, T[] weights)
    {
        var n = freqs.Length;
        if (n < 4 || n % 2 != 0)
            throw new ArgumentException("Frequency array must have even number of at least 4 values!");
        if (freqs[0] != T.Zero || freqs[n - 1] != T.CreateChecked(0.5))
            throw new ArgumentException("Frequency array must start with 0 and end with 0.5!");
        Guard.AgainstInequality(desired.Length, n / 2, "Size of desired array", "half-size of freqs array");
        Guard.AgainstInequality(weights.Length, n / 2, "Size of weights array", "half-size of freqs array");
    }
}
