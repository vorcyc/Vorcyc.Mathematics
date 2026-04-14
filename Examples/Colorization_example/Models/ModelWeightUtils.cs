using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Colorization_example.Models;

internal static class ModelWeightUtils
{
  public static void CopyConvWeights(
    Convolution2DLayer<float> layer,
    Tensor<float>[] sourceFilters,
    Tensor<float> sourceBias)
  {
    var parameters = layer.Parameters;
    if (parameters.Count != sourceFilters.Length + 1)
    {
      throw new InvalidOperationException($"Convolution parameter count mismatch for {layer.Name}.");
    }

    for (int i = 0; i < sourceFilters.Length; i++)
    {
      CopyTensor(sourceFilters[i], parameters[i].Value);
    }

    CopyTensor(sourceBias, parameters[sourceFilters.Length].Value);
  }

  public static void CopySpatialLinearWeights(
    SpatialLinearLayer<float> layer,
    Tensor<float>[] sourceFilters,
    Tensor<float> sourceBias)
  {
    var parameters = layer.Parameters;
    if (parameters.Count != sourceFilters.Length + 1)
    {
      throw new InvalidOperationException($"Spatial linear parameter count mismatch for {layer.Name}.");
    }

    for (int i = 0; i < sourceFilters.Length; i++)
    {
      CopyTensor(sourceFilters[i], parameters[i].Value);
    }

    CopyTensor(sourceBias, parameters[sourceFilters.Length].Value);
  }

  public static void CopyBatchNormWeights(
    BatchNormLayer<float> layer,
    Tensor<float> mean,
    Tensor<float> variance,
    Tensor<float> beta,
    Tensor<float> gamma)
  {
    CopyTensor(gamma, layer.Scale.Value);
    CopyTensor(beta, layer.Shift.Value);
    CopyTensor(mean, layer.RunningMean);
    CopyTensor(variance, layer.RunningVariance);
  }

  public static void CopyTensor(Tensor<float> source, Tensor<float> destination)
  {
    if (source.Width != destination.Width
        || source.Height != destination.Height
        || source.Depth != destination.Depth)
    {
      throw new ArgumentException("Tensor shape mismatch while copying weights.");
    }

    source.Values.CopyTo(destination.Values);
  }
}
