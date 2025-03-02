using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal static class MovingAverageFitter
{

    /// <summary>
    /// 移动平均拟合：平滑时间序列数据。
    /// </summary>
    /// <typeparam name="T">浮点类型</typeparam>
    /// <param name="xData">X 数据点（通常为时间序列的自变量，需单调递增）</param>
    /// <param name="yData">Y 数据点（时间序列的因变量）</param>
    /// <param name="windowSize">移动窗口大小（必须为奇数且大于0）</param>
    /// <returns>拟合结果</returns>
    public static FitResult<T> Fit_Normal<T>(T[] xData, T[] yData, int windowSize)
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

        // 检查 xData 是否单调递增
        for (int i = 1; i < n; i++)
            if (xData[i] <= xData[i - 1])
                throw new ArgumentException("X 数据点必须单调递增");

        // 计算平滑值
        T[] smoothed = new T[n];
        for (int i = 0; i < n; i++)
        {
            int start = Math.Max(0, i - halfWindow);
            int end = Math.Min(n - 1, i + halfWindow);
            int count = end - start + 1;

            T sum = T.Zero;
            for (int j = start; j <= end; j++)
                sum += yData[j];

            smoothed[i] = sum / T.CreateChecked(count);
        }

        // 预测函数：使用线性插值在 xData 之间进行预测
        Func<T, T> predict = x =>
        {
            if (x < xData[0] || x > xData[n - 1])
                throw new ArgumentOutOfRangeException(nameof(x), "预测点超出数据范围");

            // 找到 x 所在的区间
            int i = 0;
            while (i < n - 1 && x > xData[i]) i++;

            if (i == 0) return smoothed[0];
            if (i == n) return smoothed[n - 1];

            // 线性插值
            T x0 = xData[i - 1];
            T x1 = xData[i];
            T y0 = smoothed[i - 1];
            T y1 = smoothed[i];
            T t = (x - x0) / (x1 - x0);
            return y0 + t * (y1 - y0);
        };

        // 计算 MSE
        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T diff = smoothed[i] - yData[i];
            mse += diff * diff;
        }
        mse /= T.CreateChecked(n);

        // 参数：返回窗口大小作为唯一的“参数”
        T[] parameters = new T[] { T.CreateChecked(windowSize) };

        return new FitResult<T>(predict, parameters, mse);
    }
}
