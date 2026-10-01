using System.Drawing;
using Emgu.CV;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Colorization_example.Vision;

internal static class ColorNetIo
{
  private const float LumaOffset = 0.44505388568813414f;

  public static (Tensor<float> original, Tensor<float> inputTensor224) Preprocess(Mat image)
  {
    using var bgr8 = MatPixels.ToBgr8(image);
    var bgr = MatPixels.ReadBgr(bgr8);
    var original = new Tensor<float>(bgr8.Cols, bgr8.Rows, 1);

    for (int y = 0; y < bgr8.Rows; y++)
    {
      for (int x = 0; x < bgr8.Cols; x++)
      {
        original[x, y, 0] = Luma(bgr, (y * bgr8.Cols + x) * 3) - LumaOffset;
      }
    }

    using var resized = new Mat();
    CvInvoke.Resize(bgr8, resized, new Size(224, 224));
    var resizedBgr = MatPixels.ReadBgr(resized);
    var inputTensor224 = new Tensor<float>(224, 224, 1);

    for (int y = 0; y < 224; y++)
    {
      for (int x = 0; x < 224; x++)
      {
        inputTensor224[x, y, 0] = Luma(resizedBgr, (y * 224 + x) * 3) - LumaOffset;
      }
    }

    return (original, inputTensor224);
  }

  public static Mat Deprocess(Tensor<float> luma, Tensor<float> chromaUv)
  {
    int width = Math.Min(luma.Width, chromaUv.Width);
    int height = Math.Min(luma.Height, chromaUv.Height);
    var bgr = new byte[width * height * 3];

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

        int i = (y * width + x) * 3;
        bgr[i] = MatPixels.ToByte(b);
        bgr[i + 1] = MatPixels.ToByte(g);
        bgr[i + 2] = MatPixels.ToByte(r);
      }
    }

    return MatPixels.CreateBgr(width, height, bgr);
  }

  private static float Luma(byte[] bgr, int index)
    => 0.299f * (bgr[index + 2] / 255f) + 0.587f * (bgr[index + 1] / 255f) + 0.114f * (bgr[index] / 255f);

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
