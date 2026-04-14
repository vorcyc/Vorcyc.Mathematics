namespace Vorcyc.Mathematics.DeepLearning.Training;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// A single training example consisting of an input tensor and a target tensor.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public readonly record struct TrainingSample<T>(Tensor<T> Input, Tensor<T> Target)
    where T : unmanaged, IBinaryFloatingPointIeee754<T>;
