using System.Numerics;

using Vorcyc.Mathematics.MachineLearning.Internal;

using Vorcyc.Mathematics.MachineLearning.Preprocessing;



namespace Vorcyc.Mathematics.MachineLearning;



/// <summary>

/// 回归流水线：预处理器链与回归器。

/// </summary>

public sealed class RegressionPipeline<T> : IBatchRegressor<T>

    where T : struct, IFloatingPointIeee754<T>

{

    private readonly List<IPreprocessor<T>> _preprocessors = [];

    private IRegressor<T>? _regressor;



    /// <inheritdoc />

    public MachineLearningTask Task => MachineLearningTask.Regression;



    /// <summary>追加预处理步骤。</summary>

    public RegressionPipeline<T> AddPreprocessor(IPreprocessor<T> preprocessor)

    {

        _preprocessors.Add(preprocessor);

        return this;

    }



    /// <summary>设置回归器。</summary>

    public RegressionPipeline<T> SetRegressor(IRegressor<T> regressor)

    {

        _regressor = regressor;

        return this;

    }



    /// <inheritdoc />

    public void Fit(T[,] x, T[] y)

    {

        if (_regressor == null)

            throw new InvalidOperationException("必须先设置回归器。");



        var transformed = PipelineCore<T>.FitTransformChain(_preprocessors, x);

        _regressor.Fit(transformed, y);

    }



    /// <inheritdoc />

    public T Predict(T[] sample)

    {

        if (_regressor == null)

            throw new InvalidOperationException("模型尚未拟合。");



        return _regressor.Predict(PipelineCore<T>.TransformSample(_preprocessors, sample));

    }



    /// <inheritdoc />

    public void PredictBatch(T[,] x, Span<T> predictions)

    {

        if (_regressor == null)

            throw new InvalidOperationException("模型尚未拟合。");

        if (x == null)

            throw new ArgumentNullException(nameof(x));



        int rows = x.GetLength(0);

        if (predictions.Length < rows)

            throw new ArgumentException("predictions 长度不足。", nameof(predictions));



        var transformed = PipelineCore<T>.TransformChain(_preprocessors, x);



        if (_regressor is IBatchRegressor<T> batchRegressor)

        {

            batchRegressor.PredictBatch(transformed, predictions[..rows]);

            return;

        }



        int cols = transformed.GetLength(1);

        var sample = new T[cols];

        for (int i = 0; i < rows; i++)

        {

            for (int j = 0; j < cols; j++)

                sample[j] = transformed[i, j];

            predictions[i] = _regressor.Predict(sample);

        }

    }

}


