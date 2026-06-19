using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning.Classfication;

/// <summary>
/// Binary logistic regression that fits a sigmoid model with gradient descent.
/// </summary>
/// <typeparam name="T">Floating-point type.</typeparam>
public class LogisticRegression<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T[]? _weights;
    private T _bias;
    private bool _isFitted;

    /// <summary>
    /// Initializes a logistic regression model.
    /// </summary>
    /// <param name="learningRate">Learning rate; when default, 0.01 is used.</param>
    /// <param name="epochs">Number of training epochs.</param>
    /// <param name="lambda">L2 regularization coefficient.</param>
    /// <param name="batchSize">Mini-batch size; &lt;=0 means full batch.</param>
    /// <param name="seed">Mini-batch shuffle seed; null uses non-deterministic shuffling.</param>
    /// <param name="context">Execution policy context; when null the ambient scope or default context is used.</param>
    public LogisticRegression(
        T learningRate = default,
        int epochs = 1000,
        T lambda = default,
        int batchSize = 0,
        int? seed = null,
        ComputingContext? context = null)
    {
        LearningRate = learningRate.Equals(default) ? T.CreateChecked(0.01) : learningRate;
        Epochs = epochs;
        Lambda = lambda.Equals(default) ? T.Zero : lambda;
        BatchSize = batchSize;
        Seed = seed;
        Context = context;
    }

    /// <summary>Learning rate.</summary>
    public T LearningRate { get; }

    /// <summary>Number of training epochs.</summary>
    public int Epochs { get; }

    /// <summary>L2 regularization coefficient.</summary>
    public T Lambda { get; }

    /// <summary>Mini-batch size; &lt;=0 means full-batch gradient descent.</summary>
    public int BatchSize { get; }

    /// <summary>Mini-batch shuffle seed.</summary>
    public int? Seed { get; }

    /// <summary>
    /// Execution policy honored by batch prediction. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>Weights after fitting.</summary>
    public IReadOnlyList<T> Weights =>
        _isFitted ? _weights! : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <summary>Bias after fitting.</summary>
    public T Bias => _isFitted ? _bias : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Classification;

    /// <summary>
    /// Fits the model using data whose labels are 0 or 1.
    /// </summary>
    public void Fit(T[,] x, int[] y)
    {
        if (x == null || y == null)
            throw new ArgumentException("Input cannot be null.");

        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || y.Length == 0)
            throw new ArgumentException("Training data cannot be empty.");
        if (rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");

        _weights = new T[cols];
        _bias = T.Zero;

        var weightGrad = new T[cols];
        int batchSize = BatchSize <= 0 ? rows : Math.Min(BatchSize, rows);

        for (int epoch = 0; epoch < Epochs; epoch++)
        {
            var order = DataSplit.CreateShuffledIndices(rows, Seed.HasValue ? Seed.Value + epoch : null);

            for (int batchStart = 0; batchStart < rows; batchStart += batchSize)
            {
                int batchCount = Math.Min(batchSize, rows - batchStart);
                Array.Clear(weightGrad, 0, cols);
                T biasGrad = T.Zero;

                for (int b = 0; b < batchCount; b++)
                {
                    int i = order[batchStart + b];
                    T label = T.CreateChecked(y[i]);
                    T prediction = Sigmoid(_bias + NumericKernels.DotRow(x, i, _weights));
                    T error = prediction - label;

                    for (int j = 0; j < cols; j++)
                        weightGrad[j] += error * x[i, j];
                    biasGrad += error;
                }

                T invN = T.One / T.CreateChecked(batchCount);
                for (int j = 0; j < cols; j++)
                {
                    weightGrad[j] = weightGrad[j] * invN + Lambda * _weights[j];
                    _weights[j] -= LearningRate * weightGrad[j];
                }
                _bias -= LearningRate * (biasGrad * invN);
            }
        }

        _isFitted = true;
    }

    /// <summary>
    /// Predicts the probability of belonging to the positive class (1).
    /// </summary>
    public T PredictProbability(T[] x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (x == null || x.Length != _weights!.Length)
            throw new ArgumentException("Feature dimension does not match the model.", nameof(x));

        return Sigmoid(LinearScore(x));
    }

    /// <inheritdoc />
    int IClassifier<T>.Predict(T[] sample) => Predict(sample);

    /// <summary>
    /// Predicts the class label (0 or 1).
    /// </summary>
    public int Predict(T[] x, T threshold = default)
    {
        threshold = threshold.Equals(default) ? T.CreateChecked(0.5) : threshold;
        return PredictProbability(x) >= threshold ? 1 : 0;
    }

    /// <inheritdoc />
    public void PredictBatch(T[,] x, Span<int> predictions)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (cols != _weights!.Length)
            throw new ArgumentException("Feature dimension does not match the model.");
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        T threshold = T.CreateChecked(0.5);
        var weights = _weights!;
        T bias = _bias;
        if (ComputingContextExecution.UseParallelIndexed(Context, rows, cols))
        {
            var buffer = new int[rows];
            ComputingContextExecution.ForEach(
                Context,
                0,
                rows,
                i =>
                {
                    T probability = Sigmoid(bias + NumericKernels.DotRow(x, i, weights));
                    buffer[i] = probability >= threshold ? 1 : 0;
                },
                workPerItem: cols);
            new ReadOnlySpan<int>(buffer).CopyTo(predictions);
            return;
        }

        for (int i = 0; i < rows; i++)
        {
            T probability = Sigmoid(_bias + NumericKernels.DotRow(x, i, _weights!));
            predictions[i] = probability >= threshold ? 1 : 0;
        }
    }

    private T LinearScore(ReadOnlySpan<T> x) =>
        _bias + NumericKernels.Dot(_weights!, x);

    private static T Sigmoid(T z)
    {
        if (z >= T.Zero)
        {
            T expNegZ = T.Exp(-z);
            return T.One / (T.One + expNegZ);
        }

        T expZ = T.Exp(z);
        return expZ / (T.One + expZ);
    }
}
