namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Confusion matrix.
/// </summary>
public sealed class ConfusionMatrix
{
    /// <summary>
    /// Initializes the confusion matrix.
    /// </summary>
    /// <param name="matrix">Rows = actual classes, columns = predicted classes.</param>
    /// <param name="classLabels">The class labels.</param>
    public ConfusionMatrix(int[,] matrix, IReadOnlyList<int> classLabels)
    {
        Matrix = matrix;
        ClassLabels = classLabels;
    }

    /// <summary>Matrix element [actual, predicted].</summary>
    public int[,] Matrix { get; }

    /// <summary>The order of the class labels.</summary>
    public IReadOnlyList<int> ClassLabels { get; }

    /// <summary>The number of classes.</summary>
    public int NumClasses => ClassLabels.Count;
}

/// <summary>
/// Classification evaluation metrics.
/// </summary>
public static class ClassificationMetrics
{
    /// <summary>
    /// Builds the confusion matrix, optionally honoring a <see cref="ComputingContext"/> for the histogram pass.
    /// </summary>
    public static ConfusionMatrix ConfusionMatrix(ReadOnlySpan<int> actual, ReadOnlySpan<int> predicted, ComputingContext? context = null)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("The label lengths must be the same.");
        if (actual.Length == 0)
            throw new ArgumentException("The input cannot be empty.");

        var actualArray = actual.ToArray();
        var predictedArray = predicted.ToArray();
        var labels = actualArray.Concat(predictedArray).Distinct().Order().ToArray();
        int k = labels.Length;
        var labelToIndex = labels.Select((label, index) => (label, index)).ToDictionary(t => t.label, t => t.index);

        int n = actualArray.Length;
        var mode = ComputingContext.Resolve(context).ResolveCpuMode(n);
        var matrix = mode == CpuExecutionMode.Parallel && ComputingContextExecution.UseParallel(context, n)
            ? BuildMatrixParallel(actualArray, predictedArray, labelToIndex, k, context)
            : BuildMatrixScalar(actualArray, predictedArray, labelToIndex, k);

        return new ConfusionMatrix(matrix, labels);
    }

    private static int[,] BuildMatrixScalar(int[] actual, int[] predicted, Dictionary<int, int> labelToIndex, int k)
    {
        var matrix = new int[k, k];
        for (int i = 0; i < actual.Length; i++)
        {
            int a = labelToIndex[actual[i]];
            int p = labelToIndex[predicted[i]];
            matrix[a, p]++;
        }

        return matrix;
    }

    private static int[,] BuildMatrixParallel(int[] actual, int[] predicted, Dictionary<int, int> labelToIndex, int k, ComputingContext? context)
    {
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new int[workers][,];
        int length = actual.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            var local = new int[k, k];
            for (int i = start; i < end; i++)
            {
                int a = labelToIndex[actual[i]];
                int p = labelToIndex[predicted[i]];
                local[a, p]++;
            }

            partials[worker] = local;
        });

        var matrix = new int[k, k];
        foreach (var local in partials)
        {
            if (local is null)
            {
                continue;
            }

            for (int r = 0; r < k; r++)
            {
                for (int c = 0; c < k; c++)
                {
                    matrix[r, c] += local[r, c];
                }
            }
        }

        return matrix;
    }

    /// <summary>
    /// Computes the per-class precision.
    /// </summary>
    public static double Precision(ConfusionMatrix cm, int classLabel)
    {
        int index = cm.ClassLabels.ToList().IndexOf(classLabel);
        if (index < 0)
            throw new ArgumentException("The class does not exist.", nameof(classLabel));

        int tp = cm.Matrix[index, index];
        int predictedPositive = 0;
        for (int i = 0; i < cm.NumClasses; i++)
            predictedPositive += cm.Matrix[i, index];
        return predictedPositive == 0 ? 0.0 : (double)tp / predictedPositive;
    }

    /// <summary>
    /// Computes the per-class recall.
    /// </summary>
    public static double Recall(ConfusionMatrix cm, int classLabel)
    {
        int index = cm.ClassLabels.ToList().IndexOf(classLabel);
        if (index < 0)
            throw new ArgumentException("The class does not exist.", nameof(classLabel));

        int tp = cm.Matrix[index, index];
        int actualPositive = 0;
        for (int j = 0; j < cm.NumClasses; j++)
            actualPositive += cm.Matrix[index, j];
        return actualPositive == 0 ? 0.0 : (double)tp / actualPositive;
    }

    /// <summary>
    /// Computes the per-class F1 score.
    /// </summary>
    public static double F1Score(ConfusionMatrix cm, int classLabel)
    {
        double precision = Precision(cm, classLabel);
        double recall = Recall(cm, classLabel);
        return precision + recall == 0 ? 0.0 : 2.0 * precision * recall / (precision + recall);
    }

    /// <summary>
    /// Macro-averaged F1.
    /// </summary>
    public static double MacroF1(ConfusionMatrix cm)
    {
        if (cm.NumClasses == 0)
            return 0.0;
        double sum = 0.0;
        foreach (int label in cm.ClassLabels)
            sum += F1Score(cm, label);
        return sum / cm.NumClasses;
    }

    /// <summary>
    /// Micro-averaged F1 (equivalent to accuracy in the single-label multiclass case).
    /// </summary>
    public static double MicroF1(ConfusionMatrix cm)
    {
        int tp = 0;
        int total = 0;
        for (int i = 0; i < cm.NumClasses; i++)
        {
            tp += cm.Matrix[i, i];
            for (int j = 0; j < cm.NumClasses; j++)
                total += cm.Matrix[i, j];
        }
        return total == 0 ? 0.0 : (double)tp / total;
    }

    /// <summary>
    /// Computes macro-averaged F1 directly from labels.
    /// </summary>
    public static double MacroF1(ReadOnlySpan<int> actual, ReadOnlySpan<int> predicted, ComputingContext? context = null) =>
        MacroF1(ConfusionMatrix(actual, predicted, context));

    /// <summary>
    /// Computes micro-averaged F1 directly from labels.
    /// </summary>
    public static double MicroF1(ReadOnlySpan<int> actual, ReadOnlySpan<int> predicted, ComputingContext? context = null) =>
        MicroF1(ConfusionMatrix(actual, predicted, context));
}
