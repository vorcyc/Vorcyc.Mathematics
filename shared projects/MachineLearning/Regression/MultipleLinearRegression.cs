using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// Provides methods for multiple linear regression over a dataset.
/// </summary>
/// <typeparam name="T">Numeric type; must implement <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
public class MultipleLinearRegression<T> : IBatchRegressor<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T[] _coefficients = [];
    private T _intercept;
    private bool _isFitted;
    private T _rSquared;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleLinearRegression{T}"/> class.
    /// </summary>
    /// <param name="context">Execution policy context; when null the ambient scope or default context is used.</param>
    public MultipleLinearRegression(ComputingContext? context = null)
    {
        _isFitted = false;
        Context = context;
    }

    /// <summary>
    /// Execution policy honored by batch prediction. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>Regression coefficients.</summary>
    public IReadOnlyList<T> Coefficients =>
        _isFitted ? _coefficients : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <summary>Intercept.</summary>
    public T Intercept =>
        _isFitted ? _intercept : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <summary>Coefficient of determination R².</summary>
    public T RSquared =>
        _isFitted ? _rSquared : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <summary>
    /// Fits the multiple linear regression model.
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
    /// Predicts the dependent variable for the given independent variables.
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
