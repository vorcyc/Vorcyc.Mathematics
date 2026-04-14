using System.Numerics;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics.MachineLearning.Internal;




namespace Vorcyc.Mathematics.MachineLearning;



/// <summary>

/// K 最近邻回归。

/// </summary>

/// <typeparam name="T">特征与目标的数值类型。</typeparam>

public class KNN<T> : IMachineLearning

    where T : struct, IFloatingPointIeee754<T>

{

    private readonly List<T[]> _featureRows = [];

    private readonly List<T> _targets = [];



    /// <inheritdoc />

    public MachineLearningTask Task => MachineLearningTask.Regression;



    /// <summary>

    /// 添加二维回归样本。

    /// </summary>

    public void Add(Point<T> point, T target) =>

        AddRegression([point.X, point.Y], target);



    /// <summary>

    /// 添加任意维回归样本。

    /// </summary>

    public void Add(ReadOnlySpan<T> features, T target) =>

        AddRegression(features, target);



    /// <summary>

    /// 对二维点进行回归。

    /// </summary>

    public T Regress(Point<T> point, int k) =>

        Regress([point.X, point.Y], k);



    /// <summary>

    /// 对特征向量进行回归。

    /// </summary>

    public T Regress(ReadOnlySpan<T> features, int k, bool distanceWeighted = false)

    {

        ValidateK(k);

        if (_featureRows.Count == 0)

            throw new InvalidOperationException("训练集为空。");

        if (features.Length != _featureRows[0].Length)

            throw new ArgumentException("特征维度与训练样本不一致。");

        if (_featureRows.Count < k)

            throw new ArgumentException($"可用样本数 ({_featureRows.Count}) 小于 k ({k})。");



        return KnnNeighborSearch.MeanTargetFromVectors(

            CollectionsMarshal.AsSpan(_featureRows),

            CollectionsMarshal.AsSpan(_targets),

            features,

            k,

            distanceWeighted);

    }



    /// <summary>

    /// 对特征矩阵的每一行进行批量回归预测。

    /// </summary>

    public T[] RegressBatch(T[,] x, int k, bool distanceWeighted = false)

    {

        if (x == null)

            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);

        var predictions = new T[rows];

        RegressBatch(x, k, predictions, distanceWeighted);

        return predictions;

    }



    /// <summary>

    /// 将回归预测写入 <paramref name="predictions"/>。

    /// </summary>

    public void RegressBatch(T[,] x, int k, Span<T> predictions, bool distanceWeighted = false)

    {

        ValidateK(k);

        if (x == null)

            throw new ArgumentNullException(nameof(x));

        if (_featureRows.Count == 0)

            throw new InvalidOperationException("训练集为空。");

        if (_featureRows.Count < k)

            throw new ArgumentException($"可用样本数 ({_featureRows.Count}) 小于 k ({k})。");



        int rows = x.GetLength(0);

        int cols = x.GetLength(1);

        if (cols != _featureRows[0].Length)

            throw new ArgumentException("特征维度与训练样本不一致。");

        if (predictions.Length < rows)

            throw new ArgumentException("predictions 长度不足。", nameof(predictions));



        var storedFeatures = CollectionsMarshal.AsSpan(_featureRows);

        var storedTargets = CollectionsMarshal.AsSpan(_targets);

        var sample = new T[cols];

        for (int i = 0; i < rows; i++)

        {

            for (int j = 0; j < cols; j++)

                sample[j] = x[i, j];

            predictions[i] = KnnNeighborSearch.MeanTargetFromVectors(

                storedFeatures,

                storedTargets,

                sample,

                k,

                distanceWeighted);

        }

    }



    private void AddRegression(ReadOnlySpan<T> features, T target)

    {

        var row = new T[features.Length];

        features.CopyTo(row);

        _featureRows.Add(row);

        _targets.Add(target);

    }



    private static void ValidateK(int k)

    {

        if (k <= 0)

            throw new ArgumentOutOfRangeException(nameof(k), "k 必须大于 0。");

    }

}


