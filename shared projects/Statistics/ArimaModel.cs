using System.Numerics;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// ARIMA / SARIMA model. Coefficients are in regression form:
/// <c>w_t = c + Σ φ_k w_{t-k} + Σ θ_k e_{t-k} + e_t</c> on the differenced series.
/// An AR(1) <c>y_t = φ y_{t-1} + e_t</c> stores <c>φ</c>, not <c>−φ</c>.
/// When both a nonseasonal and a seasonal order are positive, product lags are estimated as well.
/// </summary>
public sealed class ArimaModel<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly double[] _series;
    private readonly DiffStep[] _differences;
    private readonly int[] _arLags;
    private readonly double[] _arCoefficients;
    private readonly int[] _maLags;
    private readonly double[] _maCoefficients;
    private readonly double _constant;
    private readonly double[] _innovations;

    private ArimaModel(
        double[] series,
        DiffStep[] differences,
        int p,
        int d,
        int q,
        int seasonLength,
        int seasonalP,
        int seasonalD,
        int seasonalQ,
        int[] arLags,
        double[] arCoefficients,
        int[] maLags,
        double[] maCoefficients,
        double constant,
        double sigma2,
        double[] innovations)
    {
        _series = series;
        _differences = differences;
        _arLags = arLags;
        _arCoefficients = arCoefficients;
        _maLags = maLags;
        _maCoefficients = maCoefficients;
        _constant = constant;
        _innovations = innovations;
        P = p;
        D = d;
        Q = q;
        SeasonLength = seasonLength;
        SeasonalP = seasonalP;
        SeasonalD = seasonalD;
        SeasonalQ = seasonalQ;
        Sigma2 = T.CreateChecked(sigma2);
        Constant = T.CreateChecked(constant);
        Ar = Pick<T>(arLags, arCoefficients, NonseasonalLags(p));
        Ma = Pick<T>(maLags, maCoefficients, NonseasonalLags(q));
        SeasonalAr = Pick<T>(arLags, arCoefficients, SeasonalLags(seasonalP, seasonLength));
        SeasonalMa = Pick<T>(maLags, maCoefficients, SeasonalLags(seasonalQ, seasonLength));
        Residuals = SeriesConvert.FromDouble<T>(innovations);
    }

    public int P { get; }
    public int D { get; }
    public int Q { get; }
    public int SeasonLength { get; }
    public int SeasonalP { get; }
    public int SeasonalD { get; }
    public int SeasonalQ { get; }
    public T Constant { get; }
    public T Sigma2 { get; }
    public T[] Ar { get; }
    public T[] Ma { get; }
    public T[] SeasonalAr { get; }
    public T[] SeasonalMa { get; }
    public T[] Residuals { get; }

    public static ArimaModel<T> Estimate(
        ReadOnlySpan<T> series,
        int p,
        int d,
        int q,
        int seasonLength = 1,
        int seasonalP = 0,
        int seasonalD = 0,
        int seasonalQ = 0,
        bool? includeConstant = null)
    {
        if (p < 0 || d < 0 || q < 0 || seasonalP < 0 || seasonalD < 0 || seasonalQ < 0)
            throw new ArgumentOutOfRangeException(nameof(p));
        if ((seasonalP > 0 || seasonalD > 0 || seasonalQ > 0) && seasonLength <= 1)
            throw new ArgumentException("A seasonal model needs a season length greater than 1.");

        bool constantTerm = includeConstant ?? (d == 0 && seasonalD == 0);
        var y = SeriesConvert.ToDouble(series);
        var steps = new List<DiffStep>();
        var current = y;
        for (int i = 0; i < d; i++)
        {
            if (current.Length < 2)
                throw new ArgumentException("The series is too short to difference.");
            steps.Add(new DiffStep(1, [current[^1]]));
            current = Difference(current, 1);
        }

        int period = seasonLength <= 1 ? 1 : seasonLength;
        for (int i = 0; i < seasonalD; i++)
        {
            if (current.Length <= period)
                throw new ArgumentException("The series is too short to seasonally difference.");
            steps.Add(new DiffStep(period, current[^period..]));
            current = Difference(current, period);
        }

        var arLags = ExpandLags(p, seasonalP, period);
        var maLags = ExpandLags(q, seasonalQ, period);
        int warmup = Math.Max(1, Math.Max(arLags.Length == 0 ? 1 : arLags[^1], maLags.Length == 0 ? 1 : maLags[^1]));
        if (current.Length <= warmup + 2)
            throw new ArgumentException("The series is too short for the requested orders.");

        double[] innovationsOnDifference = FitArma(current, arLags, maLags, constantTerm, out double[] ar, out double[] ma, out double constant, out double sigma2);
        var innovations = new double[y.Length];
        int lost = y.Length - current.Length;
        for (int i = 0; i < innovationsOnDifference.Length; i++)
            innovations[i + lost] = innovationsOnDifference[i];

        return new ArimaModel<T>(
            y,
            steps.ToArray(),
            p,
            d,
            q,
            period,
            seasonalP,
            seasonalD,
            seasonalQ,
            arLags,
            ar,
            maLags,
            ma,
            constant,
            sigma2,
            innovations);
    }

    public T[] Forecast(int horizon)
    {
        if (horizon < 0)
            throw new ArgumentOutOfRangeException(nameof(horizon));
        if (horizon == 0)
            return [];

        var current = (double[])_series.Clone();
        foreach (var step in _differences)
            current = Difference(current, step.Lag);

        int lost = _series.Length - current.Length;
        var history = new double[current.Length + horizon];
        var errors = new double[current.Length + horizon];
        current.AsSpan().CopyTo(history);
        for (int i = 0; i < current.Length; i++)
            errors[i] = _innovations[i + lost];

        int start = current.Length;
        for (int h = 0; h < horizon; h++)
        {
            int t = start + h;
            history[t] = PredictOne(history, errors, t);
            errors[t] = 0;
        }

        var future = history[start..];
        for (int step = _differences.Length - 1; step >= 0; step--)
            future = Integrate(_differences[step], future);
        return SeriesConvert.FromDouble<T>(future);
    }

    double PredictOne(double[] history, double[] errors, int t)
    {
        double value = _constant;
        for (int i = 0; i < _arLags.Length; i++)
        {
            int lag = _arLags[i];
            if (t - lag >= 0)
                value += _arCoefficients[i] * history[t - lag];
        }

        for (int i = 0; i < _maLags.Length; i++)
        {
            int lag = _maLags[i];
            if (t - lag >= 0)
                value += _maCoefficients[i] * errors[t - lag];
        }

        return value;
    }

    static double[] FitArma(
        double[] series,
        int[] arLags,
        int[] maLags,
        bool includeConstant,
        out double[] ar,
        out double[] ma,
        out double constant,
        out double sigma2)
    {
        var errors = new double[series.Length];
        if (maLags.Length > 0)
        {
            int longOrder = Math.Min(series.Length / 4, Math.Max(maLags[^1] + 4, 8));
            longOrder = Math.Max(longOrder, 1);
            var longLags = Enumerable.Range(1, longOrder).ToArray();
            var longCoefficients = Regress(series, longLags, [], errors, includeConstant: false, out _, out _);
            ApplyResiduals(series, longLags, longCoefficients, [], [], 0, errors);
        }

        ar = [];
        ma = [];
        constant = 0;
        sigma2 = 0;
        int passes = maLags.Length == 0 ? 1 : 3;
        for (int pass = 0; pass < passes; pass++)
        {
            var coefficients = Regress(series, arLags, maLags, errors, includeConstant, out constant, out sigma2);
            ar = coefficients[..arLags.Length];
            ma = coefficients[arLags.Length..];
            ApplyResiduals(series, arLags, ar, maLags, ma, constant, errors);
            sigma2 = InnovationVariance(errors, Math.Max(arLags.Concat(maLags).DefaultIfEmpty(1).Max(), 1));
        }

        return errors;
    }

    static double[] Regress(double[] series, int[] arLags, int[] maLags, double[] errors, bool includeConstant, out double constant, out double sigma2)
    {
        int maxLag = 1;
        if (arLags.Length > 0)
            maxLag = Math.Max(maxLag, arLags[^1]);
        if (maLags.Length > 0)
            maxLag = Math.Max(maxLag, maLags[^1]);
        int rows = series.Length - maxLag;
        int columns = arLags.Length + maLags.Length + (includeConstant ? 1 : 0);
        if (columns == 0)
        {
            constant = 0;
            sigma2 = InnovationVariance(series, 0);
            return [];
        }

        var design = new double[rows, columns];
        var target = new double[rows];
        for (int row = 0; row < rows; row++)
        {
            int t = row + maxLag;
            target[row] = series[t];
            int column = 0;
            for (int i = 0; i < arLags.Length; i++, column++)
                design[row, column] = series[t - arLags[i]];
            for (int i = 0; i < maLags.Length; i++, column++)
                design[row, column] = errors[t - maLags[i]];
            if (includeConstant)
                design[row, column] = 1;
        }

        double[] solved = DenseLinearAlgebra.LeastSquares(design, target);
        constant = includeConstant ? solved[^1] : 0;
        var withoutConstant = includeConstant ? solved[..^1] : solved;
        var fittedErrors = new double[series.Length];
        var ar = withoutConstant[..arLags.Length];
        var ma = withoutConstant[arLags.Length..];
        ApplyResiduals(series, arLags, ar, maLags, ma, constant, fittedErrors);
        sigma2 = InnovationVariance(fittedErrors, maxLag);
        return withoutConstant;
    }

    static void ApplyResiduals(double[] series, int[] arLags, double[] ar, int[] maLags, double[] ma, double constant, double[] errors)
    {
        for (int t = 0; t < series.Length; t++)
        {
            double prediction = constant;
            bool ready = true;
            for (int i = 0; i < arLags.Length; i++)
            {
                if (t - arLags[i] < 0)
                {
                    ready = false;
                    break;
                }

                prediction += ar[i] * series[t - arLags[i]];
            }

            if (!ready)
            {
                errors[t] = 0;
                continue;
            }

            for (int i = 0; i < maLags.Length; i++)
            {
                if (t - maLags[i] >= 0)
                    prediction += ma[i] * errors[t - maLags[i]];
            }

            errors[t] = series[t] - prediction;
        }
    }

    static double InnovationVariance(double[] errors, int warmup)
    {
        double sum = 0;
        int count = 0;
        for (int i = warmup; i < errors.Length; i++)
        {
            sum += errors[i] * errors[i];
            count++;
        }

        return count == 0 ? 0 : sum / count;
    }

    static int[] ExpandLags(int order, int seasonalOrder, int seasonLength)
    {
        var lags = new SortedSet<int>();
        for (int i = 1; i <= order; i++)
            lags.Add(i);
        if (seasonLength > 1)
        {
            for (int j = 1; j <= seasonalOrder; j++)
                lags.Add(j * seasonLength);
            for (int i = 1; i <= order; i++)
            {
                for (int j = 1; j <= seasonalOrder; j++)
                    lags.Add(i + j * seasonLength);
            }
        }

        return lags.ToArray();
    }

    static int[] NonseasonalLags(int order) => Enumerable.Range(1, Math.Max(order, 0)).ToArray();

    static int[] SeasonalLags(int order, int seasonLength)
    {
        if (order <= 0 || seasonLength <= 1)
            return [];
        var lags = new int[order];
        for (int i = 0; i < order; i++)
            lags[i] = (i + 1) * seasonLength;
        return lags;
    }

    static TValue[] Pick<TValue>(int[] lags, double[] coefficients, int[] wanted)
        where TValue : IFloatingPointIeee754<TValue>
    {
        var result = new TValue[wanted.Length];
        for (int i = 0; i < wanted.Length; i++)
        {
            int index = Array.IndexOf(lags, wanted[i]);
            result[i] = TValue.CreateChecked(index >= 0 ? coefficients[index] : 0);
        }

        return result;
    }

    static double[] Difference(double[] values, int lag)
    {
        var result = new double[values.Length - lag];
        for (int i = lag; i < values.Length; i++)
            result[i - lag] = values[i] - values[i - lag];
        return result;
    }

    static double[] Integrate(DiffStep step, double[] future)
    {
        var history = new List<double>(step.Tail);
        var integrated = new double[future.Length];
        for (int t = 0; t < future.Length; t++)
        {
            double value = future[t] + history[^step.Lag];
            integrated[t] = value;
            history.Add(value);
        }

        return integrated;
    }

    readonly record struct DiffStep(int Lag, double[] Tail);
}
