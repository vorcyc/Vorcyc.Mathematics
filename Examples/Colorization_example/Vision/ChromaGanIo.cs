using System.Drawing;
using Emgu.CV;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Colorization_example.Vision;

internal static class ChromaGanIo
{
  public static Tensor<float> Preprocess(Mat image)
  {
    using var bgr8 = MatPixels.ToBgr8(image);
    using var resized = new Mat();
    CvInvoke.Resize(bgr8, resized, new Size(224, 224));
    var bgr = MatPixels.ReadBgr(resized);

    var tensor = new Tensor<float>(224, 224, 3);
    for (int y = 0; y < 224; y++)
    {
      for (int x = 0; x < 224; x++)
      {
        int i = (y * 224 + x) * 3;
        float l = RgbToL(bgr[i + 2], bgr[i + 1], bgr[i]) / 100f;
        tensor[x, y, 0] = l;
        tensor[x, y, 1] = l;
        tensor[x, y, 2] = l;
      }
    }

    return tensor;
  }

  public static Mat Deprocess(Mat original, Tensor<float> chromaAb)
  {
    int abWidth = chromaAb.Width;
    int abHeight = chromaAb.Height;
    using var originalBgr = MatPixels.ToBgr8(original);
    using var resized = new Mat();
    CvInvoke.Resize(originalBgr, resized, new Size(abWidth, abHeight));
    var ab = MatPixels.ReadBgr(resized);

    for (int y = 0; y < abHeight; y++)
    {
      for (int x = 0; x < abWidth; x++)
      {
        int i = (y * abWidth + x) * 3;
        float r = ab[i + 2];
        float g = ab[i + 1];
        float b = ab[i];

        LabToRgb(
          RgbToL((byte)r, (byte)g, (byte)b),
          (chromaAb[x, y, 0] * 2f - 1f) * 150f,
          (chromaAb[x, y, 1] * 2f - 1f) * 150f,
          ref r,
          ref g,
          ref b);

        ab[i] = MatPixels.ToByte(b);
        ab[i + 1] = MatPixels.ToByte(g);
        ab[i + 2] = MatPixels.ToByte(r);
      }
    }

    using var abSmall = MatPixels.CreateBgr(abWidth, abHeight, ab);
    using var upscaled = new Mat();
    CvInvoke.Resize(abSmall, upscaled, originalBgr.Size);
    var abResized = MatPixels.ReadBgr(upscaled);
    var contentBgr = MatPixels.ReadBgr(originalBgr);

    int width = originalBgr.Cols;
    int height = originalBgr.Rows;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        int i = (y * width + x) * 3;
        RgbToLab(abResized[i + 2], abResized[i + 1], abResized[i], out _, out float colorA, out float colorB);
        float contentL = RgbToL(contentBgr[i + 2], contentBgr[i + 1], contentBgr[i]);

        float r = 0f;
        float g = 0f;
        float b = 0f;
        LabToRgb(contentL, colorA, colorB, ref r, ref g, ref b);
        abResized[i] = MatPixels.ToByte(b);
        abResized[i + 1] = MatPixels.ToByte(g);
        abResized[i + 2] = MatPixels.ToByte(r);
      }
    }

    return MatPixels.CreateBgr(width, height, abResized);
  }

  private static float RgbToL(byte r, byte g, byte b)
  {
    float varR = NormalizeRgbChannel(r);
    float varG = NormalizeRgbChannel(g);
    float varB = NormalizeRgbChannel(b);
    varR *= 100f;
    varG *= 100f;
    varB *= 100f;

    float y = varR * 0.2126f + varG * 0.7152f + varB * 0.0722f;
    float varY = y / 100f;
    varY = varY > 0.008856f ? MathF.Pow(varY, 1f / 3f) : (7.787f * varY) + (16f / 116f);
    return (116f * varY) - 16f;
  }

  private static float NormalizeRgbChannel(byte channel)
  {
    float value = channel / 255f;
    return value > 0.04045f
      ? MathF.Pow((value + 0.055f) / 1.055f, 2.4f)
      : value / 12.92f;
  }

  private static void RgbToLab(float r, float g, float b, out float l, out float a, out float labB)
  {
    r /= 255f;
    g /= 255f;
    b /= 255f;
    r = r > 0.04045f ? MathF.Pow((r + 0.055f) / 1.055f, 2.4f) * 100f : r / 12.92f * 100f;
    g = g > 0.04045f ? MathF.Pow((g + 0.055f) / 1.055f, 2.4f) * 100f : g / 12.92f * 100f;
    b = b > 0.04045f ? MathF.Pow((b + 0.055f) / 1.055f, 2.4f) * 100f : b / 12.92f * 100f;

    float x = (r * 0.4124f + g * 0.3576f + b * 0.1805f) / 95.047f;
    float y = (r * 0.2126f + g * 0.7152f + b * 0.0722f) / 100f;
    float z = (r * 0.0193f + g * 0.1192f + b * 0.9505f) / 108.883f;

    x = x > 0.008856f ? MathF.Pow(x, 0.3333f) : (7.787f * x) + (16f / 116f);
    y = y > 0.008856f ? MathF.Pow(y, 0.3333f) : (7.787f * y) + (16f / 116f);
    z = z > 0.008856f ? MathF.Pow(z, 0.3333f) : (7.787f * z) + (16f / 116f);

    l = 116f * y - 16f;
    a = 500f * (x - y);
    labB = 200f * (y - z);
  }

  private static void LabToRgb(float l, float a, float b, ref float r, ref float g, ref float blue)
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
