using Colorization_example.Legacy;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.LinearAlgebra;
using LegacyLayers = Vorcyc.Mathematics.DeepLearning.Layers.Layers;

namespace Colorization_example.Models;

/// <summary>
/// ColorNet image colorization model implemented with the Vorcyc.Mathematics 0.9 DeepLearning modules API.
/// </summary>
internal sealed class ColorNetModel
{
  private readonly ReLUActivation<float> _relu = new("relu");
  private readonly SigmoidActivation<float> _sigmoid = new("sigmoid");

  public Convolution2DLayer<float> LLFN_Conv1 { get; } = new(1, 64, 3, stride: 2, name: "llfn.conv1");
  public Convolution2DLayer<float> LLFN_Conv2 { get; } = new(64, 128, 3, name: "llfn.conv2");
  public Convolution2DLayer<float> LLFN_Conv3 { get; } = new(128, 128, 3, stride: 2, name: "llfn.conv3");
  public Convolution2DLayer<float> LLFN_Conv4 { get; } = new(128, 256, 3, name: "llfn.conv4");
  public Convolution2DLayer<float> LLFN_Conv5 { get; } = new(256, 256, 3, stride: 2, name: "llfn.conv5");
  public Convolution2DLayer<float> LLFN_Conv6 { get; } = new(256, 512, 3, name: "llfn.conv6");

  public Convolution2DLayer<float> MLFN_Conv1 { get; } = new(512, 512, 3, name: "mlfn.conv1");
  public Convolution2DLayer<float> MLFN_Conv2 { get; } = new(512, 256, 3, name: "mlfn.conv2");

  public Convolution2DLayer<float> GFN_Conv1 { get; } = new(512, 512, 3, stride: 2, name: "gfn.conv1");
  public Convolution2DLayer<float> GFN_Conv2 { get; } = new(512, 512, 3, name: "gfn.conv2");
  public Convolution2DLayer<float> GFN_Conv3 { get; } = new(512, 512, 3, stride: 2, name: "gfn.conv3");
  public Convolution2DLayer<float> GFN_Conv4 { get; } = new(512, 512, 3, name: "gfn.conv4");
  public SpatialLinearLayer<float> GFN_Linear1 { get; } = new(25088, 1024, "gfn.linear1");
  public SpatialLinearLayer<float> GFN_Linear2 { get; } = new(1024, 512, "gfn.linear2");
  public SpatialLinearLayer<float> GFN_Linear3 { get; } = new(512, 256, "gfn.linear3");

  public Convolution2DLayer<float> CN_Conv1 { get; } = new(512, 256, 3, name: "cn.conv1");
  public Convolution2DLayer<float> CN_Conv2 { get; } = new(256, 128, 3, name: "cn.conv2");
  public Convolution2DLayer<float> CN_Conv3 { get; } = new(128, 64, 3, name: "cn.conv3");
  public Convolution2DLayer<float> CN_Conv4 { get; } = new(64, 64, 3, name: "cn.conv4");
  public Convolution2DLayer<float> CN_Conv5 { get; } = new(64, 32, 3, name: "cn.conv5");
  public Convolution2DLayer<float> CN_Conv6 { get; } = new(32, 2, 3, name: "cn.conv6");

  public IReadOnlyList<Parameter<float>> Parameters
  {
    get
    {
      var list = new List<Parameter<float>>();
      foreach (var layer in EnumerateLayers())
      {
        list.AddRange(layer.Parameters);
      }

      return list;
    }
  }

  public static ColorNetModel Create()
    => new();

  public static ColorNetModel FromHModel(string hmodelPath)
  {
    var weights = HModelColorNetWeights.LoadFromFile(hmodelPath);
    var model = new ColorNetModel();
    model.LoadLegacyWeights(weights);
    return model;
  }

  public void LoadLegacyWeights(HModelColorNetWeights weights)
  {
    ModelWeightUtils.CopyConvWeights(LLFN_Conv1, weights.LLFN_Conv1_Weights, weights.LLFN_Conv1_Biases);
    ModelWeightUtils.CopyConvWeights(LLFN_Conv2, weights.LLFN_Conv2_Weights, weights.LLFN_Conv2_Biases);
    ModelWeightUtils.CopyConvWeights(LLFN_Conv3, weights.LLFN_Conv3_Weights, weights.LLFN_Conv3_Biases);
    ModelWeightUtils.CopyConvWeights(LLFN_Conv4, weights.LLFN_Conv4_Weights, weights.LLFN_Conv4_Biases);
    ModelWeightUtils.CopyConvWeights(LLFN_Conv5, weights.LLFN_Conv5_Weights, weights.LLFN_Conv5_Biases);
    ModelWeightUtils.CopyConvWeights(LLFN_Conv6, weights.LLFN_Conv6_Weights, weights.LLFN_Conv6_Biases);

    ModelWeightUtils.CopyConvWeights(MLFN_Conv1, weights.MLFN_Conv1_Weights, weights.MLFN_Conv1_Biases);
    ModelWeightUtils.CopyConvWeights(MLFN_Conv2, weights.MLFN_Conv2_Weights, weights.MLFN_Conv2_Biases);

    ModelWeightUtils.CopyConvWeights(GFN_Conv1, weights.GFN_Conv1_Weights, weights.GFN_Conv1_Biases);
    ModelWeightUtils.CopyConvWeights(GFN_Conv2, weights.GFN_Conv2_Weights, weights.GFN_Conv2_Biases);
    ModelWeightUtils.CopyConvWeights(GFN_Conv3, weights.GFN_Conv3_Weights, weights.GFN_Conv3_Biases);
    ModelWeightUtils.CopyConvWeights(GFN_Conv4, weights.GFN_Conv4_Weights, weights.GFN_Conv4_Biases);
    ModelWeightUtils.CopySpatialLinearWeights(GFN_Linear1, weights.GFN_Linear1_Weights, weights.GFN_Linear1_Biases);
    ModelWeightUtils.CopySpatialLinearWeights(GFN_Linear2, weights.GFN_Linear2_Weights, weights.GFN_Linear2_Biases);
    ModelWeightUtils.CopySpatialLinearWeights(GFN_Linear3, weights.GFN_Linear3_Weights, weights.GFN_Linear3_Biases);

    ModelWeightUtils.CopyConvWeights(CN_Conv1, weights.CN_Conv1_Weights, weights.CN_Conv1_Biases);
    ModelWeightUtils.CopyConvWeights(CN_Conv2, weights.CN_Conv2_Weights, weights.CN_Conv2_Biases);
    ModelWeightUtils.CopyConvWeights(CN_Conv3, weights.CN_Conv3_Weights, weights.CN_Conv3_Biases);
    ModelWeightUtils.CopyConvWeights(CN_Conv4, weights.CN_Conv4_Weights, weights.CN_Conv4_Biases);
    ModelWeightUtils.CopyConvWeights(CN_Conv5, weights.CN_Conv5_Weights, weights.CN_Conv5_Biases);
    ModelWeightUtils.CopyConvWeights(CN_Conv6, weights.CN_Conv6_Weights, weights.CN_Conv6_Biases);
  }

  public Tensor<float> Colorize(Tensor<float> fullResolutionLuma, Tensor<float> resizedLuma224)
  {
    var localBranch = RunLowLevelFeatureNet(fullResolutionLuma);
    var globalBranch = RunLowLevelFeatureNet(resizedLuma224);

    localBranch = ForwardConv(localBranch, MLFN_Conv1);
    localBranch = ForwardConv(localBranch, MLFN_Conv2);

    globalBranch = ForwardConv(globalBranch, GFN_Conv1);
    globalBranch = ForwardConv(globalBranch, GFN_Conv2);
    globalBranch = ForwardConv(globalBranch, GFN_Conv3);
    globalBranch = ForwardConv(globalBranch, GFN_Conv4);
    globalBranch = GFN_Linear1.Forward(globalBranch, training: false);
    globalBranch = _relu.Forward(globalBranch, training: false);
    globalBranch = GFN_Linear2.Forward(globalBranch, training: false);
    globalBranch = _relu.Forward(globalBranch, training: false);
    globalBranch = GFN_Linear3.Forward(globalBranch, training: false);
    globalBranch = _relu.Forward(globalBranch, training: false);

    var fused = LegacyLayers.JoinLayer(localBranch, globalBranch);

    var output = ForwardConv(fused, CN_Conv1);
    output = ForwardConv(output, CN_Conv2);
    output = LegacyLayers.Upsample2D(output);
    output = ForwardConv(output, CN_Conv3);
    output = ForwardConv(output, CN_Conv4);
    output = LegacyLayers.Upsample2D(output);
    output = ForwardConv(output, CN_Conv5);
    output = ForwardConv(output, CN_Conv6);
    output = _sigmoid.Forward(output, training: false);
    output = LegacyLayers.Upsample2D(output);
    return output;
  }

  internal IEnumerable<ILayer<float>> EnumerateLayers()
  {
    yield return LLFN_Conv1;
    yield return LLFN_Conv2;
    yield return LLFN_Conv3;
    yield return LLFN_Conv4;
    yield return LLFN_Conv5;
    yield return LLFN_Conv6;
    yield return MLFN_Conv1;
    yield return MLFN_Conv2;
    yield return GFN_Conv1;
    yield return GFN_Conv2;
    yield return GFN_Conv3;
    yield return GFN_Conv4;
    yield return GFN_Linear1;
    yield return GFN_Linear2;
    yield return GFN_Linear3;
    yield return CN_Conv1;
    yield return CN_Conv2;
    yield return CN_Conv3;
    yield return CN_Conv4;
    yield return CN_Conv5;
    yield return CN_Conv6;
  }

  private Tensor<float> RunLowLevelFeatureNet(Tensor<float> input)
  {
    var temp = ForwardConv(input, LLFN_Conv1);
    temp = ForwardConv(temp, LLFN_Conv2);
    temp = ForwardConv(temp, LLFN_Conv3);
    temp = ForwardConv(temp, LLFN_Conv4);
    temp = ForwardConv(temp, LLFN_Conv5);
    temp = ForwardConv(temp, LLFN_Conv6);
    return temp;
  }

  private Tensor<float> ForwardConv(Tensor<float> input, Convolution2DLayer<float> layer)
  {
    var output = layer.Forward(input, training: false);
    return _relu.Forward(output, training: false);
  }

}
