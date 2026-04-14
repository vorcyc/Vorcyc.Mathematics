using System.Numerics;

using Vorcyc.Mathematics.MachineLearning.Internal;



namespace Vorcyc.Mathematics.MachineLearning;



/// <summary>

/// K 最近邻整数标签分类器，实现 <see cref="IClassifier{T}"/>。

/// </summary>

public sealed class KnnClassifier<T> : IBatchClassifier<T>

    where T : struct, IFloatingPointIeee754<T>

{

    private T[,]? _features;

    private int[]? _labels;

    private readonly int _k;



    /// <summary>

    /// 初始化 KNN 分类器。

    /// </summary>

    /// <param name="k">近邻数量。</param>

    public KnnClassifier(int k = 3)

    {

        if (k <= 0)

            throw new ArgumentOutOfRangeException(nameof(k));

        _k = k;

    }



    /// <summary>近邻数 k。</summary>

    public int K => _k;



    /// <inheritdoc />

    public MachineLearningTask Task => MachineLearningTask.Classification;



    /// <inheritdoc />

    public void Fit(T[,] x, int[] y)

    {

        if (x == null || y == null)

            throw new ArgumentException("输入不能为 null。");

        int rows = x.GetLength(0);

        if (rows == 0 || y.Length == 0 || rows != y.Length)

            throw new ArgumentException("样本数与标签数不匹配。");

        if (y.Min() < 0)

            throw new ArgumentException("标签必须为非负整数。");



        _features = x;

        _labels = y;

    }



    /// <inheritdoc />

    public int Predict(T[] sample)

    {

        ValidateSample(sample);

        return KnnNeighborSearch.MajorityLabelFromRows(_features!, _labels!, sample, _k);

    }



    /// <inheritdoc />

    public void PredictBatch(T[,] x, Span<int> predictions)

    {

        if (x == null)

            throw new ArgumentNullException(nameof(x));

        if (_features == null || _labels == null)

            throw new InvalidOperationException("模型尚未拟合。");



        int rows = x.GetLength(0);

        if (predictions.Length < rows)

            throw new ArgumentException("predictions 长度不足。", nameof(predictions));



        int cols = x.GetLength(1);

        if (cols != _features.GetLength(1))

            throw new ArgumentException("特征维度与训练样本不一致。");



        for (int i = 0; i < rows; i++)

            predictions[i] = KnnNeighborSearch.MajorityLabelFromQueryRow(_features, _labels, x, i, _k);

    }



    private void ValidateSample(T[] sample)

    {

        if (_features == null || _labels == null)

            throw new InvalidOperationException("模型尚未拟合。");

        if (sample == null)

            throw new ArgumentNullException(nameof(sample));

        if (sample.Length != _features.GetLength(1))

            throw new ArgumentException("特征维度与训练样本不一致。", nameof(sample));

        if (_labels.Length < _k)

            throw new InvalidOperationException($"训练样本数 ({_labels.Length}) 小于 k ({_k})。");

    }

}


