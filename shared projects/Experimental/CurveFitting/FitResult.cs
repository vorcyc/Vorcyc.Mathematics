using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

/// <summary>
/// 表示曲线拟合的结果。
/// </summary>
internal class FitResultOfDouble(Func<double, double> predict, double[] parameters, double mse)
{
    /// <summary>
    /// 拟合后的预测函数。
    /// </summary>
    public Func<double, double> Predict { get; } = predict ?? throw new ArgumentNullException(nameof(predict));

    /// <summary>
    /// 拟合参数，例如斜率、截距或多项式系数。
    /// </summary>
    public double[] Parameters { get; } = parameters ?? throw new ArgumentNullException(nameof(parameters));

    /// <summary>
    /// 均方误差 (MSE)，衡量拟合精度。
    /// </summary>
    public double MeanSquaredError { get; } = mse;

    /// <summary>
    /// 转换为泛型 FitResult。
    /// </summary>
    internal FitResult<T> ToGeneric<T>() where T : struct, IFloatingPointIeee754<T>
    {
        //var genericPredict = new Func<T, T>(x => (T)(object)Predict((double)(object)x));
        var genericPredict = new Func<T, T>(x => T.CreateChecked(Predict((double)(object)x)));
        var genericParameters = Array.ConvertAll(Parameters, p => T.CreateChecked(p));
        var genericMse = T.CreateChecked(MeanSquaredError);
        return new FitResult<T>(genericPredict, genericParameters, genericMse);
    }
}

/// <summary>
/// 表示曲线拟合的结果。
/// </summary>
internal class FitResultOfSingle(Func<float, float> predict, float[] parameters, float mse)
{
    /// <summary>
    /// 拟合后的预测函数。
    /// </summary>
    public Func<float, float> Predict { get; } = predict ?? throw new ArgumentNullException(nameof(predict));

    /// <summary>
    /// 拟合参数，例如斜率、截距或多项式系数。
    /// </summary>
    public float[] Parameters { get; } = parameters ?? throw new ArgumentNullException(nameof(parameters));

    /// <summary>
    /// 均方误差 (MSE)，衡量拟合精度。
    /// </summary>
    public float MeanSquaredError { get; } = mse;

    /// <summary>
    /// 转换为泛型 FitResult。
    /// </summary>
    internal FitResult<T> ToGeneric<T>() where T : struct, IFloatingPointIeee754<T>
    {
        var genericPredict = new Func<T, T>(x => (T)(object)Predict((float)(object)x));
        var genericParameters = Array.ConvertAll(Parameters, p => (T)(object)p);
        var genericMse = (T)(object)MeanSquaredError;
        return new FitResult<T>(genericPredict, genericParameters, genericMse);
    }
}

/// <summary>
/// 表示曲线拟合的结果。
/// </summary>
public class FitResult<T>(Func<T, T> predict, T[] parameters, T mse) where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>
    /// 拟合后的预测函数。
    /// </summary>
    public Func<T, T> Predict { get; } = predict ?? throw new ArgumentNullException(nameof(predict));

    /// <summary>
    /// 拟合参数，例如斜率、截距或多项式系数。
    /// </summary>
    public T[] Parameters { get; } = parameters ?? throw new ArgumentNullException(nameof(parameters));

    /// <summary>
    /// 均方误差 (MSE)，衡量拟合精度。
    /// </summary>
    public T MeanSquaredError { get; } = mse;
}

/// <summary>
/// 表示曲线拟合的结果，支持多列输入。
/// </summary>
public class MultiColumnFitResult<T>(Func<DataRow<T>, T>? predict, T[] parameters, T mse)
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>
    /// 拟合参数，例如权重和偏置。
    /// </summary>
    public T[] Parameters { get; } = parameters ?? throw new ArgumentNullException(nameof(parameters));

    /// <summary>
    /// 均方误差 (MSE)，衡量拟合精度。
    /// </summary>
    public T MeanSquaredError { get; } = mse;

    /// <summary>
    /// 可选的多列输入预测函数。
    /// </summary>
    public Func<DataRow<T>, T>? Predict { get; } = predict;
}


/// <summary>
/// 表示贝叶斯回归的结果，包括预测函数和参数的不确定性。
/// </summary>
public class BayesianFitResult<T> where T : struct, IFloatingPointIeee754<T>
{
    public Func<T[], T> PredictMean { get; }
    public Func<T[], T> PredictVariance { get; }
    public T[] MeanParameters { get; }
    public T[][] CovarianceMatrix { get; }
    public T MeanSquaredError { get; }

    public BayesianFitResult(Func<T[], T> predictMean, Func<T[], T> predictVariance, T[] meanParameters, T[][] covarianceMatrix, T mse)
    {
        PredictMean = predictMean ?? throw new ArgumentNullException(nameof(predictMean));
        PredictVariance = predictVariance ?? throw new ArgumentNullException(nameof(predictVariance));
        MeanParameters = meanParameters ?? throw new ArgumentNullException(nameof(meanParameters));
        CovarianceMatrix = covarianceMatrix ?? throw new ArgumentNullException(nameof(covarianceMatrix));
        MeanSquaredError = mse;
    }
}