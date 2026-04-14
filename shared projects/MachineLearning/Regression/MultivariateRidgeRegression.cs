using System.Numerics;

using Vorcyc.Mathematics.MachineLearning.Internal;



namespace Vorcyc.Mathematics.MachineLearning.Regression;



/// <summary>

/// 多元岭回归，对多维特征矩阵进行 L2 正则化最小二乘拟合。

/// </summary>

public class MultivariateRidgeRegression<T> : IBatchRegressor<T>

    where T : struct, IFloatingPointIeee754<T>

{

    private readonly T _lambda;

    private T[] _coefficients = [];

    private T _intercept;

    private T _rSquared;

    private bool _isFitted;



    /// <summary>

    /// 初始化多元岭回归模型。

    /// </summary>

    /// <param name="lambda">L2 正则化系数（不惩罚截距项）。</param>

    public MultivariateRidgeRegression(T lambda)

    {

        if (lambda < T.Zero)

            throw new ArgumentException("正则化参数必须非负。", nameof(lambda));

        _lambda = lambda;

    }



    /// <summary>特征系数。</summary>

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

    /// 拟合多元岭回归模型。

    /// </summary>

    public void Fit(T[,] x, T[] y)

    {

        LinearRegressionModel.ValidateTrainingData(x, y);

        int cols = x!.GetLength(1);



        var designMatrix = RegressionMathHelper.BuildDesignMatrixWithIntercept(x);

        var solution = RegressionMathHelper.SolveRidgeLeastSquares(

            designMatrix, y, _lambda, regularizeIntercept: false);

        LinearRegressionModel.ApplyDesignSolution(solution, cols, out _intercept, out _coefficients);



        _isFitted = true;

        _rSquared = RegressionMathHelper.ComputeRSquared(

            y, i => Predict(Array2DHelpers.GetRow(x, i)));

    }



    /// <summary>

    /// 预测单个样本。

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


