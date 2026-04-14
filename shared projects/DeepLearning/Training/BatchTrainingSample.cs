namespace Vorcyc.Mathematics.DeepLearning.Training;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// A training batch for <see cref="CnnMlpModel{T}"/> with NHWC inputs and 1×N×F targets.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <param name="Input">NHWC batch input.</param>
/// <param name="Target">Legacy 1×N×F target tensor.</param>
public readonly record struct BatchTrainingSample<T>(BatchTensor<T> Input, Tensor<T> Target)
    where T : unmanaged, IBinaryFloatingPointIeee754<T>;
