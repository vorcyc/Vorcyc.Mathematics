using System.Numerics;
using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.MachineLearning.CurveFitting;

internal class LocallyWeightedRegression<T> where T : unmanaged, IFloatingPointIeee754<T>
{
    private readonly T[] _xData;
    private readonly T[] _yData;
    private readonly T _bandwidth;

    public LocallyWeightedRegression(Span<T> xData, Span<T> yData, T? bandwidth = null)
    {
        if (xData.Length != yData.Length || xData.Length < 2)
            throw new ArgumentException("数据点数量必须相等且至少有2个点");
        _xData = xData.ToArray();
        _yData = yData.ToArray();
        T defaultBandwidth = (_xData.AsSpan().Max() - _xData.AsSpan().Min()) * T.CreateChecked(0.3);
        _bandwidth = bandwidth ?? defaultBandwidth;
        if (_bandwidth <= T.Zero)
            throw new ArgumentException("带宽必须大于0");
    }

    public FitResult<T> Fit(ComputingContext? computingContext = null, CancellationToken cancellationToken = default)
    {
        int n = _xData.Length;
        var dispatch = CurveFittingExecution.ResolveDispatch<T>(computingContext, n, n);
        bool useSimd = dispatch == CurveFitDispatchKind.Simd;

        T[] fittedValues = new T[n];
        Func<T, T> predict = x => PredictAt(x, useSimd, cancellationToken);

        ComputingContextExecution.ForEach(computingContext, 0, n, i =>
        {
            fittedValues[i] = PredictAt(_xData[i], useSimd, cancellationToken);
        }, workPerItem: n, cancellationToken: cancellationToken);

        T mse = CurveFittingExecution.MeanSquaredError<T>(fittedValues, _yData, useSimd);
        return new FitResult<T>(predict, Array.Empty<T>(), mse);
    }

    private T PredictAt(T x, bool useSimd, CancellationToken cancellationToken = default)
    {
        CurveFittingExecution.AccumWeightedLinearSums(
            _xData, _yData, x, _bandwidth,
            out T wSum, out T wxSum, out T wySum, out T wxxSum, out T wxySum, useSimd, cancellationToken);

        T denominator = wSum * wxxSum - wxSum * wxSum;
        if (denominator == T.Zero)
            return wySum / wSum;
        T slope = (wSum * wxySum - wxSum * wySum) / denominator;
        T intercept = (wxxSum * wySum - wxSum * wxySum) / denominator;
        return slope * x + intercept;
    }

    internal static FitResult<T> Fit_Normal(
        Span<T> xData, Span<T> yData, T? bandwidth = null, ComputingContext? computingContext = null,
        CancellationToken cancellationToken = default)
        => new LocallyWeightedRegression<T>(xData, yData, bandwidth).Fit(computingContext, cancellationToken);
}
