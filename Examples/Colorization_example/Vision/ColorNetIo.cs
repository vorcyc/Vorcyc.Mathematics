using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Colorization_example.Vision;

internal static class ColorNetIo
{
  private const float LumaOffset = 0.44505388568813414f;

  public static (Tensor<float> original, Tensor<float> inputTensor224) Preprocess(Mat image)
  {
    using var rgbImage = image.ToImage<Rgb, byte>();
    var data = rgbImage.Data;
    var original = new Tensor<float>(image.Cols, image.Rows, 1);

    for (int y = 0; y < rgbImage.Height; y++)
    {
      for (int x = 0; x < rgbImage.Width; x++)
      {
        float b = data[y, x, 2] / 255f;
        float g = data[y, x, 1] / 255f;
        float r = data[y, x, 0] / 255f;
        float yValue = 0.299f * r + 0.587f * g + 0.114f * b;
        original[x, y, 0] = yValue - LumaOffset;
      }
    }

    using var resized = new Mat();
    CvInvoke.Resize(image, resized, new Size(224, 224));
    using var resizedRgb = resized.ToImage<Rgb, byte>();
    var resizedData = resizedRgb.Data;
    var inputTensor224 = new Tensor<float>(224, 224, 1);

    for (int y = 0; y < resizedRgb.Height; y++)
    {
      for (int x = 0; x < resizedRgb.Width; x++)
      {
        float b = resizedData[y, x, 2] / 255f;
        float g = resizedData[y, x, 1] / 255f;
        float r = resizedData[y, x, 0] / 255f;
        float yValue = 0.299f * r + 0.587f * g + 0.114f * b;
        inputTensor224[x, y, 0] = yValue - LumaOffset;
      }
    }

    return (original, inputTensor224);
  }

  public static Mat Deprocess(Tensor<float> luma, Tensor<float> chromaUv)
  {
    int width = Math.Min(luma.Width, chromaUv.Width);
    int height = Math.Min(luma.Height, chromaUv.Height);
    using var image = new Image<Bgr, byte>(width, height);

    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        LabToRgb(
          (luma[x, y, 0] + LumaOffset) * 100f,
          (chromaUv[x, y, 0] * 2f - 1f) * 100f,
          (chromaUv[x, y, 1] * 2f - 1f) * 100f,
          out float r,
          out float g,
          out float b);

        image[y, x] = new Bgr(
          Math.Clamp(b, 0f, 255f),
          Math.Clamp(g, 0f, 255f),
          Math.Clamp(r, 0f, 255f));
      }
    }

    return image.Mat.Clone();
  }

  private static void LabToRgb(float l, float a, float b, out float r, out float g, out float blue)
  {
    float varY = (l + 16f) / 116f;
    float varX = a / 500f + varY;
    float varZ = varY - b / 200f;

    varY = MathF.Pow(varY, 3f) > 0.008856f ? MathF.Pow(varY, 3f) : (varY - 16f / 116f) / 7.787f;
    varX = MathF.Pow(varX, 3f) > 0.008856f ? MathF.Pow(varX, 3f) : (varX - 16f / 116f) / 7.787f;
    varZ = MathF.Pow(varZ, 3f) > 0.008856f ? MathF.Pow(varZ, 3f) : (varZ - 16f / 116f) / 7.787f;

    float x = 95.047f * varX;
    float y = 100f * varY;
    float z = 108.883f * varZ;
    varX = x / 100f;
    varY = y / 100f;
    varZ = z / 100f;

    float varR = varX * 3.2406f + varY * -1.5372f + varZ * -0.4986f;
    float varG = varX * -0.9689f + varY * 1.8758f + varZ * 0.0415f;
    float varB = varX * 0.0557f + varY * -0.2040f + varZ * 1.0570f;

    r = GammaCorrect(varR) * 255f;
    g = GammaCorrect(varG) * 255f;
    blue = GammaCorrect(varB) * 255f;
  }

  private static float GammaCorrect(float channel)
    => channel > 0.0031308f
      ? 1.055f * MathF.Pow(channel, 1f / 2.4f) - 0.055f
      : 12.92f * channel;
}
