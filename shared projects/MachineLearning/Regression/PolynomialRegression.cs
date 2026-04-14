using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// 多项式回归，用于一维非线性拟合。
/// </summary>
public class PolynomialRegression<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly int _degree;
    private T[]? _coefficients;
    private T _rSquared;

    /// <summary>
    /// 初始化多项式回归模型。
    /// </summary>
    /// <param name="degree">多项式阶数。</param>
    public PolynomialRegression(int degree)
    {
        if (degree < 0)
            throw new ArgumentException("多项式阶数必须大于等于 0。");
        _degree = degree;
    }

    /// <summary>拟合后的系数。</summary>
    public T[] Coefficients =>
        _coefficients?.ToArray() ?? throw new InvalidOperationException("模型尚未拟合。");

    /// <summary>决定系数 R²。</summary>
    public T RSquared => _rSquared;

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <summary>
    /// 拟合多项式回归模型。
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
        _coefficients = RegressionMathHelper.SolveLeastSquares(designMatrix, y);
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
