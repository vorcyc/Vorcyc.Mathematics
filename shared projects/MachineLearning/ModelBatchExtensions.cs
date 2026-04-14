using System.Numerics;



namespace Vorcyc.Mathematics.MachineLearning;



/// <summary>

/// 分类器与回归器的批量预测扩展。

/// </summary>

public static class ModelBatchExtensions

{

    /// <summary>

    /// 对特征矩阵的每一行进行批量分类预测。

    /// </summary>

    public static int[] PredictBatch<T>(this IClassifier<T> classifier, T[,] x)

        where T : struct, IFloatingPointIeee754<T>

    {

        if (classifier == null)

            throw new ArgumentNullException(nameof(classifier));

        if (x == null)

            throw new ArgumentNullException(nameof(x));



        int rows = x.GetLength(0);

        var predictions = new int[rows];

        PredictBatch(classifier, x, predictions);

        return predictions;

    }



    /// <summary>

    /// 将预测结果写入 <paramref name="predictions"/>（长度 ≥ 行数）。

    /// </summary>

    public static void PredictBatch<T>(

        this IClassifier<T> classifier,

        T[,] x,

        Span<int> predictions)

        where T : struct, IFloatingPointIeee754<T>

    {

        if (classifier == null)

            throw new ArgumentNullException(nameof(classifier));

        if (x == null)

            throw new ArgumentNullException(nameof(x));



        int rows = x.GetLength(0);

        if (predictions.Length < rows)

            throw new ArgumentException("predictions 长度不足。", nameof(predictions));



        if (classifier is IBatchClassifier<T> batchClassifier)

        {

            batchClassifier.PredictBatch(x, predictions[..rows]);

            return;

        }



        int cols = x.GetLength(1);

        var sample = new T[cols];

        for (int i = 0; i < rows; i++)

        {

            for (int j = 0; j < cols; j++)

                sample[j] = x[i, j];

            predictions[i] = classifier.Predict(sample);

        }

    }



    /// <summary>

    /// 对特征矩阵的每一行进行批量回归预测。

    /// </summary>

    public static T[] PredictBatch<T>(this IRegressor<T> regressor, T[,] x)

        where T : struct, IFloatingPointIeee754<T>

    {

        if (regressor == null)

            throw new ArgumentNullException(nameof(regressor));

        if (x == null)

            throw new ArgumentNullException(nameof(x));



        int rows = x.GetLength(0);

        var predictions = new T[rows];

        PredictBatch(regressor, x, predictions);

        return predictions;

    }



    /// <summary>

    /// 将回归预测写入 <paramref name="predictions"/>。

    /// </summary>

    public static void PredictBatch<T>(

        this IRegressor<T> regressor,

        T[,] x,

        Span<T> predictions)

        where T : struct, IFloatingPointIeee754<T>

    {

        if (regressor == null)

            throw new ArgumentNullException(nameof(regressor));

        if (x == null)

            throw new ArgumentNullException(nameof(x));



        int rows = x.GetLength(0);

        if (predictions.Length < rows)

            throw new ArgumentException("predictions 长度不足。", nameof(predictions));



        if (regressor is IBatchRegressor<T> batchRegressor)

        {

            batchRegressor.PredictBatch(x, predictions[..rows]);

            return;

        }



        int cols = x.GetLength(1);

        var sample = new T[cols];

        for (int i = 0; i < rows; i++)

        {

            for (int j = 0; j < cols; j++)

                sample[j] = x[i, j];

            predictions[i] = regressor.Predict(sample);

        }

    }

}



/// <summary>

/// 支持高效批量分类预测的实现可声明此接口。

/// </summary>

public interface IBatchClassifier<T> : IClassifier<T>

    where T : struct, IFloatingPointIeee754<T>

{

    /// <summary>批量预测，写入 <paramref name="predictions"/>。</summary>

    void PredictBatch(T[,] x, Span<int> predictions);

}



/// <summary>

/// 支持高效批量回归预测的实现可声明此接口。

/// </summary>

public interface IBatchRegressor<T> : IRegressor<T>

    where T : struct, IFloatingPointIeee754<T>

{

    /// <summary>批量预测，写入 <paramref name="predictions"/>。</summary>

    void PredictBatch(T[,] x, Span<T> predictions);

}


