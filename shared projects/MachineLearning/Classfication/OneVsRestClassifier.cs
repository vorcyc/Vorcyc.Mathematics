using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Classfication;

/// <summary>
/// One-vs-Rest 多分类包装器，将多个二分类逻辑回归组合为多分类器。
/// </summary>
public sealed class OneVsRestClassifier<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly T _learningRate;
    private readonly int _epochs;
    private readonly T _lambda;
    private LogisticRegression<T>[] _binaryModels = [];

    /// <summary>
    /// 初始化 OvR 分类器。
    /// </summary>
    public OneVsRestClassifier(T learningRate = default, int epochs = 1500, T lambda = default)
    {
        _learningRate = learningRate;
        _epochs = epochs;
        _lambda = lambda;
    }

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Classification;

    /// <summary>类别数。</summary>
    public int NumClasses => _binaryModels.Length;

    /// <inheritdoc />
    public void Fit(T[,] x, int[] y)
    {
        if (x == null || y == null)
            throw new ArgumentException("输入不能为 null。");
        if (x.GetLength(0) != y.Length)
            throw new ArgumentException("样本数与标签数不匹配。");

        int classes = y.Max() + 1;
        if (y.Min() < 0)
            throw new ArgumentException("标签必须为非负整数。");

        _binaryModels = new LogisticRegression<T>[classes];
        int rows = y.Length;
        for (int c = 0; c < classes; c++)
        {
            var binaryLabels = new int[rows];
            for (int i = 0; i < rows; i++)
                binaryLabels[i] = y[i] == c ? 1 : 0;

            var model = new LogisticRegression<T>(_learningRate, _epochs, _lambda);
            model.Fit(x, binaryLabels);
            _binaryModels[c] = model;
        }
    }

    /// <inheritdoc />
    public int Predict(T[] sample)
    {
        if (_binaryModels.Length == 0)
            throw new InvalidOperationException("模型尚未拟合。");

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
            throw new InvalidOperationException("模型尚未拟合。");
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (predictions.Length < rows)
            throw new ArgumentException("predictions 长度不足。", nameof(predictions));

        var sample = new T[cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                sample[j] = x[i, j];
            predictions[i] = Predict(sample);
        }
    }
}
