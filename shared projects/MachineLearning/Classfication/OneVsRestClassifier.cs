using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Classfication;

/// <summary>
/// One-vs-Rest multi-class wrapper that combines multiple binary logistic regression models into a multi-class classifier.
/// </summary>
public sealed class OneVsRestClassifier<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly T _learningRate;
    private readonly int _epochs;
    private readonly T _lambda;
    private LogisticRegression<T>[] _binaryModels = [];

    /// <summary>
    /// Initializes an OvR classifier.
    /// </summary>
    /// <param name="learningRate">Learning rate forwarded to the inner binary models.</param>
    /// <param name="epochs">Number of training epochs forwarded to the inner binary models.</param>
    /// <param name="lambda">L2 regularization coefficient forwarded to the inner binary models.</param>
    /// <param name="context">Execution policy context; when null the ambient scope or default context is used.</param>
    public OneVsRestClassifier(T learningRate = default, int epochs = 1500, T lambda = default, ComputingContext? context = null)
    {
        _learningRate = learningRate;
        _epochs = epochs;
        _lambda = lambda;
        Context = context;
    }

    /// <summary>
    /// Execution policy honored by per-class fitting and batch prediction. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Classification;

    /// <summary>Number of classes.</summary>
    public int NumClasses => _binaryModels.Length;

    /// <inheritdoc />
    public void Fit(T[,] x, int[] y)
    {
        if (x == null || y == null)
            throw new ArgumentException("Input cannot be null.");
        if (x.GetLength(0) != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");

        int classes = y.Max() + 1;
        if (y.Min() < 0)
            throw new ArgumentException("Labels must be non-negative integers.");

        _binaryModels = new LogisticRegression<T>[classes];
        int rows = y.Length;
        var models = _binaryModels;
        var context = Context;
        T learningRate = _learningRate;
        int epochs = _epochs;
        T lambda = _lambda;
        ComputingContextExecution.ForEach(
            context,
            0,
            classes,
            c =>
            {
                var binaryLabels = new int[rows];
                for (int i = 0; i < rows; i++)
                    binaryLabels[i] = y[i] == c ? 1 : 0;

                var model = new LogisticRegression<T>(learningRate, epochs, lambda, context: context);
                model.Fit(x, binaryLabels);
                models[c] = model;
            },
            workPerItem: (long)rows * epochs);
    }

    /// <inheritdoc />
    public int Predict(T[] sample)
    {
        if (_binaryModels.Length == 0)
            throw new InvalidOperationException("The model has not been fitted yet.");

        int bestClass = 0;
        T bestProbability = T.CreateChecked(-1.0);
        for (int c = 0; c < _binaryModels.Length; c++)
        {
            T probability = _binaryModels[c].PredictProbability(sample);
            if (probability > bestProbability)
            {
                bestProbability = probability;
                bestClass = c;
            }
        }
        return bestClass;
    }

    /// <inheritdoc />
    public void PredictBatch(T[,] x, Span<int> predictions)
    {
        if (_binaryModels.Length == 0)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        int classes = _binaryModels.Length;
        if (ComputingContextExecution.UseParallelIndexed(Context, rows, (long)cols * classes))
        {
            var buffer = new int[rows];
            ComputingContextExecution.ForEach(
                Context,
                0,
                rows,
                i =>
                {
                    var localSample = new T[cols];
                    for (int j = 0; j < cols; j++)
                        localSample[j] = x[i, j];
                    buffer[i] = Predict(localSample);
                },
                workPerItem: (long)cols * classes);
            new ReadOnlySpan<int>(buffer).CopyTo(predictions);
            return;
        }

        var sample = new T[cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                sample[j] = x[i, j];
            predictions[i] = Predict(sample);
        }
    }
}
