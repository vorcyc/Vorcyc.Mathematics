using System.Numerics;

using Vorcyc.Mathematics.MachineLearning.Internal;



namespace Vorcyc.Mathematics.MachineLearning;



/// <summary>

/// K 最近邻回归器，实现 IRegressor 与 IBatchRegressor 批量预测。

/// </summary>

public sealed class KnnRegressor<T> : IBatchRegressor<T>

    where T : struct, IFloatingPointIeee754<T>

{

    private T[,]? _features;

    private T[]? _targets;

    private readonly int _k;

    private readonly bool _distanceWeighted;



    /// <summary>

    /// 初始化 KNN 回归器。

    /// </summary>

    /// <param name="k">近邻数量。</param>

    /// <param name="distanceWeighted">是否对近邻目标做距离加权平均。</param>

    public KnnRegressor(int k = 3, bool distanceWeighted = false)

    {

        if (k <= 0)

            throw new ArgumentOutOfRangeException(nameof(k));

        _k = k;

        _distanceWeighted = distanceWeighted;

    }



    /// <summary>近邻数 k。</summary>

    public int K => _k;



    /// <inheritdoc />

    public MachineLearningTask Task => MachineLearningTask.Regression;



    /// <inheritdoc />

    public void Fit(T[,] x, T[] y)

    {

        if (x == null || y == null)

            throw new ArgumentException("输入不能为 null。");



        int rows = x.GetLength(0);

        if (rows == 0 || y.Length == 0 || rows != y.Length)

            throw new ArgumentException("样本数与标签数不匹配。");

        if (rows < _k)

            throw new ArgumentException($"训练样本数 ({rows}) 小于 k ({_k})。");



        _features = x;

        _targets = y;

    }



    /// <inheritdoc />

    public T Predict(T[] sample)

    {

        ValidateSample(sample);

        return KnnNeighborSearch.MeanTargetFromRows(

            _features!,

            _targets!,

            sample,

            _k,

            _distanceWeighted);

    }



    /// <inheritdoc />

    public void PredictBatch(T[,] x, Span<T> predictions)

    {

        if (x == null)

            throw new ArgumentNullException(nameof(x));

        if (_features == null || _targets == null)

            throw new InvalidOperationException("模型尚未拟合。");



        int rows = x.GetLength(0);

        int cols = x.GetLength(1);

        if (cols != _features.GetLength(1))

            throw new ArgumentException("特征维度与训练样本不一致。");

        if (predictions.Length < rows)

            throw new ArgumentException("predictions 长度不足。", nameof(predictions));



        for (int i = 0; i < rows; i++)

            predictions[i] = KnnNeighborSearch.MeanTargetFromQueryRow(

                _features,

                _targets!,

                x,

                i,

                _k,

                _distanceWeighted);

    }



    private void ValidateSample(T[] sample)

    {

        if (_features == null || _targets == null)

            throw new InvalidOperationException("模型尚未拟合。");

        if (sample == null)

            throw new ArgumentNullException(nameof(sample));

        if (sample.Length != _features.GetLength(1))

            throw new ArgumentException("特征维度与训练样本不一致。", nameof(sample));

        if (_targets.Length < _k)

            throw new InvalidOperationException($"训练样本数 ({_targets.Length}) 小于 k ({_k})。");

    }

}


