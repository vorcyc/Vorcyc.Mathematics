using System.Numerics;

using Vorcyc.Mathematics.MachineLearning.Internal;

using Vorcyc.Mathematics.MachineLearning.Preprocessing;



namespace Vorcyc.Mathematics.MachineLearning;



/// <summary>

/// 分类流水线：预处理器链 + 分类器。

/// </summary>

public sealed class ClassificationPipeline<T> : IBatchClassifier<T>

    where T : struct, IFloatingPointIeee754<T>

{

    private readonly List<IPreprocessor<T>> _preprocessors = [];

    private IClassifier<T>? _classifier;



    /// <inheritdoc />

    public MachineLearningTask Task => MachineLearningTask.Classification;



    /// <summary>追加预处理步骤。</summary>

    public ClassificationPipeline<T> AddPreprocessor(IPreprocessor<T> preprocessor)

    {

        _preprocessors.Add(preprocessor);

        return this;

    }



    /// <summary>设置分类器。</summary>

    public ClassificationPipeline<T> SetClassifier(IClassifier<T> classifier)

    {

        _classifier = classifier;

        return this;

    }



    /// <inheritdoc />

    public void Fit(T[,] x, int[] y)

    {

        if (_classifier == null)

            throw new InvalidOperationException("必须先设置分类器。");



        var transformed = PipelineCore<T>.FitTransformChain(_preprocessors, x);

        _classifier.Fit(transformed, y);

    }



    /// <inheritdoc />

    public int Predict(T[] sample)

    {

        if (_classifier == null)

            throw new InvalidOperationException("模型尚未拟合。");



        return _classifier.Predict(PipelineCore<T>.TransformSample(_preprocessors, sample));

    }



    /// <inheritdoc />

    public void PredictBatch(T[,] x, Span<int> predictions)

    {

        if (_classifier == null)

            throw new InvalidOperationException("模型尚未拟合。");

        if (x == null)

            throw new ArgumentNullException(nameof(x));



        int rows = x.GetLength(0);

        if (predictions.Length < rows)

            throw new ArgumentException("predictions 长度不足。", nameof(predictions));



        var transformed = PipelineCore<T>.TransformChain(_preprocessors, x);



        if (_classifier is IBatchClassifier<T> batchClassifier)

        {

            batchClassifier.PredictBatch(transformed, predictions[..rows]);

            return;

        }



        int cols = transformed.GetLength(1);

        var sample = new T[cols];

        for (int i = 0; i < rows; i++)

        {

            for (int j = 0; j < cols; j++)

                sample[j] = transformed[i, j];

            predictions[i] = _classifier.Predict(sample);

        }

    }

}


