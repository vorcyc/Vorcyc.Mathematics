using System.Numerics;

using Vorcyc.Mathematics.MachineLearning.Internal;



namespace Vorcyc.Mathematics.MachineLearning.Regression;



/// <summary>

/// 提供对数据集进行多元线性回归的方法。

/// </summary>

/// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>

public class MultipleLinearRegression<T> : IBatchRegressor<T>

    where T : struct, IFloatingPointIeee754<T>

{

    private T[] _coefficients = [];

    private T _intercept;

    private bool _isFitted;

    private T _rSquared;



    /// <summary>

    /// 初始化 <see cref="MultipleLinearRegression{T}"/> 类的新实例。

    /// </summary>

    public MultipleLinearRegression()

    {

        _isFitted = false;

    }



    /// <summary>回归系数。</summary>

    public IReadOnlyList<T> Coefficients =>

        _isFitted ? _coefficients : throw new InvalidOperationException("模型尚未拟合。");



    /// <summary>截距。</summary>

    public T Intercept =>

        _isFitted ? _intercept : throw new InvalidOperationException("模型尚未拟合。");



    /// <summary>决定系数 R²。</summary>

    public T RSquared =>

        _isFitted ? _rSquared : throw new InvalidOperationException("模型尚未拟合。");



    /// <inheritdoc />

    public MachineLearningTask Task => MachineLearningTask.Regression;



    /// <summary>

    /// 拟合多元线性回归模型。

    /// </summary>

    public void Fit(T[,] x, T[] y)

    {

        LinearRegressionModel.ValidateTrainingData(x, y);

        int cols = x!.GetLength(1);



        var designMatrix = RegressionMathHelper.BuildDesignMatrixWithIntercept(x);

        var solution = RegressionMathHelper.SolveLeastSquares(designMatrix, y);

        LinearRegressionModel.ApplyDesignSolution(solution, cols, out _intercept, out _coefficients);



        _isFitted = true;

        _rSquared = RegressionMathHelper.ComputeRSquared(

            y, i => Predict(Array2DHelpers.GetRow(x, i)));

    }



    /// <summary>

    /// 根据给定自变量预测因变量。

    /// </summary>

    public T Predict(T[] x)

    {

        if (!_isFitted)

            throw new InvalidOperationException("模型尚未拟合。");

        return LinearRegressionModel.PredictAffine(_intercept, _coefficients, x);

    }



    /// <inheritdoc />

    public void PredictBatch(T[,] x, Span<T> predictions)

    {

        if (!_isFitted)

            throw new InvalidOperationException("模型尚未拟合。");

        LinearRegressionModel.PredictAffineBatch(_intercept, _coefficients, x, predictions);

    }

}


