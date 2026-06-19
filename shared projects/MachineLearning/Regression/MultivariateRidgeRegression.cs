using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// Multivariate ridge regression that fits a multi-dimensional feature matrix with L2-regularized least squares.
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
    /// Initializes a multivariate ridge regression model.
    /// </summary>
    /// <param name="lambda">L2 regularization coefficient (the intercept is not penalized).</param>
    /// <param name="context">Execution policy context; when null the ambient scope or default context is used.</param>
    public MultivariateRidgeRegression(T lambda, ComputingContext? context = null)
    {
        if (lambda < T.Zero)
            throw new ArgumentException("The regularization parameter must be non-negative.", nameof(lambda));
        _lambda = lambda;
        Context = context;
    }

    /// <summary>
    /// Execution policy honored by batch prediction. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>Feature coefficients.</summary>
    public IReadOnlyList<T> Coefficients =>
        _isFitted ? _coefficients : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <summary>Intercept.</summary>
    public T Intercept =>
        _isFitted ? _intercept : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <summary>Coefficient of determination R虏.</summary>
    public T RSquared =>
        _isFitted ? _rSquared : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <summary>
    /// Fits the multivariate ridge regression model.
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
    /// Predicts a single sample.
    /// </summary>
    public T Predict(T[] x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
        return LinearRegressionModel.PredictAffine(_intercept, _coefficients, x);
    }

    /// <inheritdoc />
    public void PredictBatch(T[,] x, Span<T> predictions)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
        LinearRegressionModel.PredictAffineBatch(_intercept, _coefficients, x, predictions, Context);
    }
}
