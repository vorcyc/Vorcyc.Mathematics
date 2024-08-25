namespace Vorcyc.Mathematics.Statistics;

using System.Numerics;

/// <summary>
/// 一元线性回归工具
/// </summary>
public static class SimpleLinearRegression
{


    /// <summary>
    /// 计算线性回归模型的<b><i>斜率</i></b>和<b><i>截距</i></b>。
    /// </summary>
    /// <typeparam name="T">浮点数类型，必须实现 <see cref="IFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
    /// <param name="data">包含数据点的数组。</param>
    /// <returns>返回一个元组，包含斜率和截距。</returns>
    public static (T slope, T intercept) ComputeParameters<T>(Point<T>[] data)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (data is null || data.Length < 2)
        {
            throw new ArgumentException("Data cannot be null or the number of data points must be at least 2.");
        }

        T sumX = T.Zero;
        T sumY = T.Zero;
        T sumXY = T.Zero;
        T sumX2 = T.Zero;
        T n = T.CreateChecked(data.Length);

        foreach (var point in data)
        {
            sumX += point.X;
            sumY += point.Y;
            sumXY += point.X * point.Y;
            sumX2 += point.X * point.X;
        }

        T avgX = sumX / n;
        T avgY = sumY / n;

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        var intercept = avgY - slope * avgX;

        return (slope, intercept);
    }

    //https://baike.baidu.com/item/%E4%B8%80%E5%85%83%E7%BA%BF%E6%80%A7%E5%9B%9E%E5%BD%92%E6%96%B9%E7%A8%8B/6953911?fr=aladdin
    //一元线性回归方程



    /// <summary>
    /// 计算线性回归模型的参数，包括截距、斜率和相关系数。
    /// </summary>
    /// <typeparam name="T">浮点数类型，必须实现 <see cref="IFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
    /// <param name="x">自变量的数组。</param>
    /// <param name="y">因变量的数组。</param>
    /// <returns>返回一个元组，包含<b><i>斜率</i></b>、<b><i>截距</i></b>和<b><i>相关系数</i></b>。</returns>
    /// <exception cref="ArgumentException">当 x 和 y 的长度不相等或长度小于 2 时抛出。</exception>
    public static (T slope, T intercept, T correlationCoefficient) ComputeParameters<T>(Span<T> x, Span<T> y)
        where T : IFloatingPointIeee754<T>
    {
        if (x.Length != y.Length || x.Length < 2)
        {
            throw new ArgumentException("The lengths of x and y must be equal and greater than 1.");
        }

        // 计算自变量 x 的平均值
        var avgX = x.Average();
        // 计算因变量 y 的平均值
        var avgY = y.Average();

        // 初始化变量，用于存储平方和和协方差
        T sumOfSquaresX = T.Zero;
        T sumOfSquaresY = T.Zero;
        T sumOfProductsXY = T.Zero;
        T two = T.CreateChecked(2);

        // 遍历所有数据点，计算平方和和协方差
        for (int i = 0; i < x.Length; i++)
        {
            var xn = x[i];
            // 计算自变量 x 的平方和
            sumOfSquaresX += T.Pow(xn - avgX, two);

            var yn = y[i];
            // 计算因变量 y 的平方和
            sumOfSquaresY += T.Pow(yn - avgY, two);

            // 计算自变量和因变量的协方差
            sumOfProductsXY += (xn - avgX) * (yn - avgY);
        }

        // 计算斜率 b
        var b = sumOfProductsXY / sumOfSquaresX;
        // 计算截距 a
        var a = avgY - b * avgX;
        // 计算相关系数 r
        var r = sumOfProductsXY / T.Sqrt(sumOfSquaresX * sumOfSquaresY);

        // 返回截距、斜率和相关系数
        return (b, a, r);
    }


    /// <summary>
    /// 计算线性回归模型的斜率和截距。
    /// </summary>
    /// <param name="x">自变量的数组。</param>
    /// <param name="y">因变量的数组。</param>
    /// <returns>返回一个元组，包含<b><i>斜率</i></b>和<b><i>截距</i></b>。</returns>
    /// <exception cref="ArgumentException">当 x 和 y 的长度不相等或长度小于 2 时抛出。</exception>
    /// <remarks>
    /// 这个版本比<see cref="ComputeParameters{T}(Span{T}, Span{T})"/>更快！因为那个版本需要计算相关系数
    /// </remarks>
    public static (float slope, float intercept) ComputeParameters(Span<float> x, Span<float> y)
    {
        if (x.Length != y.Length || x.Length < 2)
        {
            throw new ArgumentException("The lengths of x and y must be equal and greater than 1.");
        }

        float sumX = 0f;
        float sumY = 0f;
        float sumXY = 0f;
        float sumX2 = 0f;
        float n = x.Length;

        for (int i = 0; i < x.Length; i++)
        {
            sumX += x[i];
            sumY += y[i];
            sumXY += x[i] * y[i];
            sumX2 += x[i] * x[i];
        }

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        var intercept = (sumY - slope * sumX) / n;

        return (slope, intercept);
    }

    /// <summary>
    /// 计算线性回归模型的斜率和截距。
    /// </summary>
    /// <param name="x">自变量的数组。</param>
    /// <param name="y">因变量的数组。</param>
    /// <returns>返回一个元组，包含<b><i>斜率</i></b>和<b><i>截距</i></b>。</returns>
    /// <exception cref="ArgumentException">当 x 和 y 的长度不相等或长度小于 2 时抛出。</exception>
    /// <remarks>
    /// 这个版本比<see cref="ComputeParameters{T}(Span{T}, Span{T})"/>更快！因为那个版本需要计算相关系数
    /// </remarks>
    public static (double slope, double intercept) ComputeParameters(Span<double> x, Span<double> y)
    {
        if (x.Length != y.Length || x.Length < 2)
        {
            throw new ArgumentException("The lengths of x and y must be equal and greater than 1.");
        }

        double sumX = 0;
        double sumY = 0;
        double sumXY = 0;
        double sumX2 = 0;
        double n = x.Length;

        for (int i = 0; i < x.Length; i++)
        {
            sumX += x[i];
            sumY += y[i];
            sumXY += x[i] * y[i];
            sumX2 += x[i] * x[i];
        }

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        var intercept = (sumY - slope * sumX) / n;

        return (slope, intercept);
    }





    /// <summary>
    /// 根据给定的 y 值、斜率和截距计算 x 值。
    /// </summary>
    /// <typeparam name="T">浮点数类型，必须实现 <see cref="IFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
    /// <param name="y">因变量的值。</param>
    /// <param name="slope">线性回归模型的斜率。</param>
    /// <param name="intercept">线性回归模型的截距。</param>
    /// <returns>返回计算得到的 x 值。</returns>
    public static T GetX<T>(T y, T slope, T intercept) where T : IFloatingPointIeee754<T>
        => (y - intercept) / slope;

    /// <summary>
    /// 根据给定的 y 值和线性回归模型的参数计算 x 值。
    /// </summary>
    /// <typeparam name="T">浮点数类型，必须实现 <see cref="IFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
    /// <param name="y">因变量的值。</param>
    /// <param name="paramaters">包含斜率和截距的元组。</param>
    /// <returns>返回计算得到的 x 值。</returns>
    public static T GetX<T>(T y, (T slope, T intercept) paramaters) where T : IFloatingPointIeee754<T>
        => (y - paramaters.intercept) / paramaters.slope;

    /// <summary>
    /// 根据给定的 x 值、斜率和截距计算 y 值。
    /// </summary>
    /// <typeparam name="T">浮点数类型，必须实现 <see cref="IFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
    /// <param name="x">自变量的值。</param>
    /// <param name="slope">线性回归模型的斜率。</param>
    /// <param name="intercept">线性回归模型的截距。</param>
    /// <returns>返回计算得到的 y 值。</returns>
    public static T GetY<T>(T x, T slope, T intercept) where T : IFloatingPointIeee754<T>
        => slope * x + intercept;

    /// <summary>
    /// 根据给定的 x 值和线性回归模型的参数计算 y 值。
    /// </summary>
    /// <typeparam name="T">浮点数类型，必须实现 <see cref="IFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
    /// <param name="x">自变量的值。</param>
    /// <param name="paramaters">包含斜率和截距的元组。</param>
    /// <returns>返回计算得到的 y 值。</returns>
    public static T GetY<T>(T x, (T slope, T intercept) paramaters) where T : IFloatingPointIeee754<T>
        => paramaters.slope * x + paramaters.intercept;















}
