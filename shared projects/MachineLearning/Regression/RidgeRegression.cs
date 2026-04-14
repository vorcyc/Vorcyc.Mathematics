using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// 岭回归，支持一维多项式基上的 L2 正则化最小二乘。
/// </summary>
public class RidgeRegression<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly T _lambda;
    private readonly int _degree;
    private T[]? _coefficients;
    private T _rSquared;

    /// <summary>
    /// 初始化岭回归模型。
    /// </summary>
    /// <param name="lambda">正则化参数。</param>
    /// <param name="degree">多项式阶数，默认为 1（线性）。</param>
    public RidgeRegression(T lambda, int degree = 1)
    {
        if (lambda < T.Zero)
            throw new ArgumentException("正则化参数必须非负。", nameof(lambda));
        if (degree < 0)
            throw new ArgumentException("多项式阶数必须非负。", nameof(degree));

        _lambda = lambda;
        _degree = degree;
    }

    /// <summary>拟合后的系数。</summary>
    public IReadOnlyList<T> Coefficients =>
        _coefficients ?? throw new InvalidOperationException("模型尚未拟合。");

    /// <summary>决定系数 R²。</summary>
    public T RSquared => _coefficients == null
        ? throw new InvalidOperationException("模型尚未拟合。")
        : _rSquared;

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <summary>
    /// 拟合岭回归模型。
    /// </summary>
    public void Fit(Span<T> x, Span<T> y)
    {
        if (x.Length == 0 || y.Length == 0)
            throw new ArgumentException("输入数组不能为空。");
        if (x.Length != y.Length)
            throw new ArgumentException("自变量和因变量数组的长度必须相同。");
        if (x.Length <= _degree)
            throw new ArgumentException("数据点数量必须大于多项式阶数。");

        var designMatrix = RegressionMathHelper.BuildVandermonde(x, _degree);
        _coefficients = RegressionMathHelper.SolveRidgeLeastSquares(
            designMatrix, y, _lambda, regularizeIntercept: false);
        var xValues = x.ToArray();
        _rSquared = RegressionMathHelper.ComputeRSquared(y, i => Predict(xValues[i]));
    }

    /// <summary>
    /// 预测给定输入的输出。
    /// </summary>
    public T Predict(T x)
    {
        if (_coefficients == null)
            throw new InvalidOperationException("模型尚未拟合。");
        return RegressionMathHelper.PredictVandermonde(x, _coefficients, _degree);
    }
}
