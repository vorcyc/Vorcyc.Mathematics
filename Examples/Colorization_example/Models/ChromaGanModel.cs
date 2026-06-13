using Colorization_example.Legacy;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.LinearAlgebra;
using LegacyLayers = Vorcyc.Mathematics.DeepLearning.Layers.Layers;

namespace Colorization_example.Models;

/// <summary>
/// ChromaGAN people-photo colorization model using the Vorcyc.Mathematics 0.9 DeepLearning modules API.
/// </summary>
internal sealed class ChromaGanModel
{
  private readonly ReLUActivation<float> _relu = new("relu");
  private readonly SigmoidActivation<float> _sigmoid = new("sigmoid");
  private readonly MaxPool2DLayer<float> _maxPool = new("maxpool");

  public Convolution2DLayer<float> VGG_Conv1 { get; } = new(3, 64, 3, name: "vgg.conv1");
  public Convolution2DLayer<float> VGG_Conv2 { get; } = new(64, 64, 3, name: "vgg.conv2");
  public Convolution2DLayer<float> VGG_Conv3 { get; } = new(64, 128, 3, name: "vgg.conv3");
  public Convolution2DLayer<float> VGG_Conv4 { get; } = new(128, 128, 3, name: "vgg.conv4");
  public Convolution2DLayer<float> VGG_Conv5 { get; } = new(128, 256, 3, name: "vgg.conv5");
  public Convolution2DLayer<float> VGG_Conv6 { get; } = new(256, 256, 3, name: "vgg.conv6");
  public Convolution2DLayer<float> VGG_Conv7 { get; } = new(256, 256, 3, name: "vgg.conv7");
  public Convolution2DLayer<float> VGG_Conv8 { get; } = new(256, 512, 3, name: "vgg.conv8");
  public Convolution2DLayer<float> VGG_Conv9 { get; } = new(512, 512, 3, name: "vgg.conv9");
  public Convolution2DLayer<float> VGG_Conv10 { get; } = new(512, 512, 3, name: "vgg.conv10");

  public Convolution2DLayer<float> Global_Conv1 { get; } = new(512, 512, 3, stride: 2, name: "global.conv1");
  public BatchNormLayer<float> Global_BN1 { get; } = new(512, "global.bn1");
  public Convolution2DLayer<float> Global_Conv2 { get; } = new(512, 512, 3, name: "global.conv2");
  public BatchNormLayer<float> Global_BN2 { get; } = new(512, "global.bn2");
  public Convolution2DLayer<float> Global_Conv3 { get; } = new(512, 512, 3, stride: 2, name: "global.conv3");
  public BatchNormLayer<float> Global_BN3 { get; } = new(512, "global.bn3");
  public Convolution2DLayer<float> Global_Conv4 { get; } = new(512, 512, 3, name: "global.conv4");
  public BatchNormLayer<float> Global_BN4 { get; } = new(512, "global.bn4");

  public SpatialLinearLayer<float> Global2_Dense1 { get; } = new(25088, 1024, "global2.dense1");
  public SpatialLinearLayer<float> Global2_Dense2 { get; } = new(1024, 512, "global2.dense2");
  public SpatialLinearLayer<float> Global2_Dense3 { get; } = new(512, 256, "global2.dense3");

  public Convolution2DLayer<float> Midlevel_Conv1 { get; } = new(512, 512, 3, name: "midlevel.conv1");
  public BatchNormLayer<float> Midlevel_BN1 { get; } = new(512, "midlevel.bn1");
  public Convolution2DLayer<float> Midlevel_Conv2 { get; } = new(512, 256, 3, name: "midlevel.conv2");
  public BatchNormLayer<float> Midlevel_BN2 { get; } = new(256, "midlevel.bn2");

  public Convolution2DLayer<float> Output_Conv1 { get; } = new(512, 256, 1, name: "output.conv1");
  public Convolution2DLayer<float> Output_Conv2 { get; } = new(256, 128, 3, name: "output.conv2");
  public Convolution2DLayer<float> Output_Conv3 { get; } = new(128, 64, 3, name: "output.conv3");
  public Convolution2DLayer<float> Output_Conv4 { get; } = new(64, 64, 3, name: "output.conv4");
  public Convolution2DLayer<float> Output_Conv5 { get; } = new(64, 32, 3, name: "output.conv5");
  public Convolution2DLayer<float> Output_Conv6 { get; } = new(32, 2, 3, name: "output.conv6");

  public static ChromaGanModel Create() => new();

  public static ChromaGanModel FromHModel(string hmodelPath)
  {
    var model = new ChromaGanModel();
    model.LoadLegacyWeights(HModelChromaGanWeights.LoadFromFile(hmodelPath));
    return model;
  }

  public void LoadLegacyWeights(HModelChromaGanWeights weights)
  {
    ModelWeightUtils.CopyConvWeights(VGG_Conv1, weights.VGG_Conv1_Weights, weights.VGG_Conv1_Biases);
    ModelWeightUtils.CopyConvWeights(VGG_Conv2, weights.VGG_Conv2_Weights, weights.VGG_Conv2_Biases);
    ModelWeightUtils.CopyConvWeights(VGG_Conv3, weights.VGG_Conv3_Weights, weights.VGG_Conv3_Biases);
    ModelWeightUtils.CopyConvWeights(VGG_Conv4, weights.VGG_Conv4_Weights, weights.VGG_Conv4_Biases);
    ModelWeightUtils.CopyConvWeights(VGG_Conv5, weights.VGG_Conv5_Weights, weights.VGG_Conv5_Biases);
    ModelWeightUtils.CopyConvWeights(VGG_Conv6, weights.VGG_Conv6_Weights, weights.VGG_Conv6_Biases);
    ModelWeightUtils.CopyConvWeights(VGG_Conv7, weights.VGG_Conv7_Weights, weights.VGG_Conv7_Biases);
    ModelWeightUtils.CopyConvWeights(VGG_Conv8, weights.VGG_Conv8_Weights, weights.VGG_Conv8_Biases);
    ModelWeightUtils.CopyConvWeights(VGG_Conv9, weights.VGG_Conv9_Weights, weights.VGG_Conv9_Biases);
    ModelWeightUtils.CopyConvWeights(VGG_Conv10, weights.VGG_Conv10_Weights, weights.VGG_Conv10_Biases);

    ModelWeightUtils.CopyConvWeights(Global_Conv1, weights.Global_Conv1_Weights, weights.Global_Conv1_Biases);
    ModelWeightUtils.CopyBatchNormWeights(Global_BN1, weights.Global_BN1_Mean, weights.Global_BN1_Variance, weights.Global_BN1_Beta, weights.Global_BN1_Gamma);
    ModelWeightUtils.CopyConvWeights(Global_Conv2, weights.Global_Conv2_Weights, weights.Global_Conv2_Biases);
    ModelWeightUtils.CopyBatchNormWeights(Global_BN2, weights.Global_BN2_Mean, weights.Global_BN2_Variance, weights.Global_BN2_Beta, weights.Global_BN2_Gamma);
    ModelWeightUtils.CopyConvWeights(Global_Conv3, weights.Global_Conv3_Weights, weights.Global_Conv3_Biases);
    ModelWeightUtils.CopyBatchNormWeights(Global_BN3, weights.Global_BN3_Mean, weights.Global_BN3_Variance, weights.Global_BN3_Beta, weights.Global_BN3_Gamma);
    ModelWeightUtils.CopyConvWeights(Global_Conv4, weights.Global_Conv4_Weights, weights.Global_Conv4_Biases);
    ModelWeightUtils.CopyBatchNormWeights(Global_BN4, weights.Global_BN4_Mean, weights.Global_BN4_Variance, weights.Global_BN4_Beta, weights.Global_BN4_Gamma);

    ModelWeightUtils.CopySpatialLinearWeights(Global2_Dense1, weights.Global2_Dense1_Weights, weights.Global2_Dense1_Biases);
    ModelWeightUtils.CopySpatialLinearWeights(Global2_Dense2, weights.Global2_Dense2_Weights, weights.Global2_Dense2_Biases);
    ModelWeightUtils.CopySpatialLinearWeights(Global2_Dense3, weights.Global2_Dense3_Weights, weights.Global2_Dense3_Biases);

    ModelWeightUtils.CopyConvWeights(Midlevel_Conv1, weights.Midlevel_Conv1_Weights, weights.Midlevel_Conv1_Biases);
    ModelWeightUtils.CopyBatchNormWeights(Midlevel_BN1, weights.Midlevel_BN1_Mean, weights.Midlevel_BN1_Variance, weights.Midlevel_BN1_Beta, weights.Midlevel_BN1_Gamma);
    ModelWeightUtils.CopyConvWeights(Midlevel_Conv2, weights.Midlevel_Conv2_Weights, weights.Midlevel_Conv2_Biases);
    ModelWeightUtils.CopyBatchNormWeights(Midlevel_BN2, weights.Midlevel_BN2_Mean, weights.Midlevel_BN2_Variance, weights.Midlevel_BN2_Beta, weights.Midlevel_BN2_Gamma);

    ModelWeightUtils.CopyConvWeights(Output_Conv1, weights.Output_Conv1_Weights, weights.Output_Conv1_Biases);
    ModelWeightUtils.CopyConvWeights(Output_Conv2, weights.Output_Conv2_Weights, weights.Output_Conv2_Biases);
    ModelWeightUtils.CopyConvWeights(Output_Conv3, weights.Output_Conv3_Weights, weights.Output_Conv3_Biases);
    ModelWeightUtils.CopyConvWeights(Output_Conv4, weights.Output_Conv4_Weights, weights.Output_Conv4_Biases);
    ModelWeightUtils.CopyConvWeights(Output_Conv5, weights.Output_Conv5_Weights, weights.Output_Conv5_Biases);
    ModelWeightUtils.CopyConvWeights(Output_Conv6, weights.Output_Conv6_Weights, weights.Output_Conv6_Biases);
  }

  public Tensor<float> Colorize(Tensor<float> input)
  {
    var encoded = Encode(input);
    var midlevel = encoded;
    var globalBranch = encoded;

    globalBranch = ForwardConv(globalBranch, Global_Conv1);
    globalBranch = ForwardBatchNorm(globalBranch, Global_BN1);
    globalBranch = ForwardConv(globalBranch, Global_Conv2);
    globalBranch = ForwardBatchNorm(globalBranch, Global_BN2);
    globalBranch = ForwardConv(globalBranch, Global_Conv3);
    globalBranch = ForwardBatchNorm(globalBranch, Global_BN3);
    globalBranch = ForwardConv(globalBranch, Global_Conv4);
    globalBranch = ForwardBatchNorm(globalBranch, Global_BN4);

    globalBranch = Global2_Dense1.Forward(globalBranch, training: false);
    globalBranch = Global2_Dense2.Forward(globalBranch, training: false);
    globalBranch = Global2_Dense3.Forward(globalBranch, training: false);

    midlevel = ForwardConv(midlevel, Midlevel_Conv1);
    midlevel = ForwardBatchNorm(midlevel, Midlevel_BN1);
    midlevel = ForwardConv(midlevel, Midlevel_Conv2);
    midlevel = ForwardBatchNorm(midlevel, Midlevel_BN2);

    var fused = LegacyLayers.Fusion(midlevel, globalBranch);

    var output = ForwardConv(fused, Output_Conv1);
    output = ForwardConv(output, Output_Conv2);
    output = LegacyLayers.Upsample2D(output);
    output = ForwardConv(output, Output_Conv3);
    output = ForwardConv(output, Output_Conv4);
    output = LegacyLayers.Upsample2D(output);
    output = ForwardConv(output, Output_Conv5);
    output = ForwardConv(output, Output_Conv6);
    output = _sigmoid.Forward(output, training: false);
    output = LegacyLayers.Upsample2D(output);
    return output;
  }

  internal IEnumerable<(string? Name, Tensor<float> Tensor)> EnumerateWeightTensors()
  {
    foreach (var layer in EnumerateTrainableLayers())
    {
      if (layer is BatchNormLayer<float> batchNorm)
      {
        foreach (var parameter in batchNorm.Parameters)
        {
          yield return (parameter.Name, parameter.Value);
        }

        yield return ($"{batchNorm.Name}.running_mean", batchNorm.RunningMean);
        yield return ($"{batchNorm.Name}.running_var", batchNorm.RunningVariance);
        continue;
      }

      foreach (var parameter in layer.Parameters)
      {
        yield return (parameter.Name, parameter.Value);
      }
    }
  }

  private Tensor<float> Encode(Tensor<float> input)
  {
    var temp = ForwardConv(input, VGG_Conv1);
    temp = ForwardConv(temp, VGG_Conv2);
    temp = _maxPool.Forward(temp, training: false);
    temp = ForwardConv(temp, VGG_Conv3);
    temp = ForwardConv(temp, VGG_Conv4);
    temp = _maxPool.Forward(temp, training: false);
    temp = ForwardConv(temp, VGG_Conv5);
    temp = ForwardConv(temp, VGG_Conv6);
    temp = ForwardConv(temp, VGG_Conv7);
    temp = _maxPool.Forward(temp, training: false);
    temp = ForwardConv(temp, VGG_Conv8);
    temp = ForwardConv(temp, VGG_Conv9);
    temp = ForwardConv(temp, VGG_Conv10);
    return temp;
  }

  private Tensor<float> ForwardConv(Tensor<float> input, Convolution2DLayer<float> layer)
  {
    var output = layer.Forward(input, training: false);
    return _relu.Forward(output, training: false);
  }

  private Tensor<float> ForwardBatchNorm(Tensor<float> input, BatchNormLayer<float> layer)
  {
    var output = layer.Forward(input, training: false);
    return _relu.Forward(output, training: false);
  }

  private IEnumerable<ILayer<float>> EnumerateTrainableLayers()
  {
    yield return VGG_Conv1;
    yield return VGG_Conv2;
    yield return VGG_Conv3;
    yield return VGG_Conv4;
    yield return VGG_Conv5;
    yield return VGG_Conv6;
    yield return VGG_Conv7;
    yield return VGG_Conv8;
    yield return VGG_Conv9;
    yield return VGG_Conv10;
    yield return Global_Conv1;
    yield return Global_BN1;
    yield return Global_Conv2;
    yield return Global_BN2;
    yield return Global_Conv3;
    yield return Global_BN3;
    yield return Global_Conv4;
    yield return Global_BN4;
    yield return Global2_Dense1;
    yield return Global2_Dense2;
    yield return Global2_Dense3;
    yield return Midlevel_Conv1;
    yield return Midlevel_BN1;
    yield return Midlevel_Conv2;
    yield return Midlevel_BN2;
    yield return Output_Conv1;
    yield return Output_Conv2;
    yield return Output_Conv3;
    yield return Output_Conv4;
    yield return Output_Conv5;
    yield return Output_Conv6;
  }
}


