using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.CurveFitting;

internal static class MovingAverageFitter
{
    /// <summary>
    /// 移动平均。分发同 VectorSpan：Parallel→ForEach（工作项标量）；Simd→窗口 SIMD 求和；Normal→标量。
    /// <see cref="ComputingContextExecution.ForEach"/> 始终传入调用方 context（同 KMeans / Matrix / Standardization）。
    /// </summary>
    public static FitResult<T> Fit<T>(
        T[] xData, T[] yData, int windowSize, ComputingContext? computingContext = null,
        CancellationToken cancellationToken = default)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length || xData.Length < 1)
            throw new ArgumentException("数据点数量必须相等且至少有1个点");
        if (windowSize <= 0 || windowSize % 2 == 0)
            throw new ArgumentException("窗口大小必须为正奇数");
        if (windowSize > xData.Length)
            throw new ArgumentException("窗口大小不能大于数据点数量");

        int n = xData.Length;
        int halfWindow = (windowSize - 1) / 2;

        for (int i = 1; i < n; i++)
        {
            CurveFittingExecution.ThrowIfCancelled(cancellationToken, i);
            if (xData[i] <= xData[i - 1])
                throw new ArgumentException("X 数据点必须单调递增");
        }

        var dispatch = CurveFittingExecution.ResolveDispatch<T>(computingContext, n, windowSize);
        bool useSimd = dispatch == CurveFitDispatchKind.Simd;

        T[] smoothed = new T[n];
        ComputingContextExecution.ForEach(computingContext, 0, n, i =>
        {
            int start = Math.Max(0, i - halfWindow);
            int end = Math.Min(n - 1, i + halfWindow);
            int count = end - start + 1;
            smoothed[i] = CurveFittingExecution.SumRange(yData.AsSpan(), start, end, useSimd) / T.CreateChecked(count);
        }, workPerItem: windowSize, cancellationToken: cancellationToken);

        Func<T, T> predict = x =>
        {
            if (x < xData[0] || x > xData[n - 1])
                throw new ArgumentOutOfRangeException(nameof(x), "预测点超出数据范围");

            int i = 0;
            while (i < n - 1 && x > xData[i]) i++;

            if (i == 0) return smoothed[0];
            if (i == n) return smoothed[n - 1];

            T x0 = xData[i - 1];
            T x1 = xData[i];
            T y0 = smoothed[i - 1];
            T y1 = smoothed[i];
            T t = (x - x0) / (x1 - x0);
            return y0 + t * (y1 - y0);
        };

        T mse = CurveFittingExecution.MeanSquaredError<T>(smoothed, yData, useSimd);
        return new FitResult<T>(predict, [T.CreateChecked(windowSize)], mse);
    }

    /// <summary>兼容旧名。</summary>
    public static FitResult<T> Fit_Normal<T>(
        T[] xData, T[] yData, int windowSize, ComputingContext? computingContext = null,
        CancellationToken cancellationToken = default)
        where T : unmanaged, IFloatingPointIeee754<T>
        => Fit(xData, yData, windowSize, computingContext, cancellationToken);
}
