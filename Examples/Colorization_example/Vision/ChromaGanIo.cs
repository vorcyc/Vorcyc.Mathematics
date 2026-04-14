using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Colorization_example.Vision;

internal static class ChromaGanIo
{
  public static Tensor<float> Preprocess(Mat image)
  {
    using var resized = new Mat();
    CvInvoke.Resize(image, resized, new Size(224, 224));
    using var bgrImage = resized.ToImage<Bgr, byte>();

    var tensor = new Tensor<float>(224, 224, 3);
    for (int y = 0; y < bgrImage.Height; y++)
    {
      for (int x = 0; x < bgrImage.Width; x++)
      {
        byte b = bgrImage.Data[y, x, 0];
        byte g = bgrImage.Data[y, x, 1];
        byte r = bgrImage.Data[y, x, 2];
        float l = RgbToL(r, g, b) / 100f;
        tensor[x, y, 0] = l;
        tensor[x, y, 1] = l;
        tensor[x, y, 2] = l;
      }
    }

    return tensor;
  }

  public static Mat Deprocess(Mat original, Tensor<float> chromaAb)
  {
    using var resized = new Mat();
    CvInvoke.Resize(original, resized, new Size(chromaAb.Width, chromaAb.Height));
    using var imageAbSource = resized.ToImage<Bgr, byte>();

    for (int y = 0; y < imageAbSource.Height; y++)
    {
      for (int x = 0; x < imageAbSource.Width; x++)
      {
        float r = imageAbSource.Data[y, x, 2];
        float g = imageAbSource.Data[y, x, 1];
        float b = imageAbSource.Data[y, x, 0];

        LabToRgb(
          RgbToL((byte)r, (byte)g, (byte)b),
          (chromaAb[x, y, 0] * 2f - 1f) * 150f,
          (chromaAb[x, y, 1] * 2f - 1f) * 150f,
          ref r,
          ref g,
          ref b);

        imageAbSource[y, x] = new Bgr(
          Math.Clamp(b, 0f, 255f),
          Math.Clamp(g, 0f, 255f),
          Math.Clamp(r, 0f, 255f));
      }
    }

    using var upscaled = new Mat();
    CvInvoke.Resize(imageAbSource, upscaled, original.Size);
    using var imageAbResized = upscaled.ToImage<Bgr, byte>();
    using var imageL = original.ToImage<Bgr, byte>();

    for (int y = 0; y < original.Height; y++)
    {
      for (int x = 0; x < original.Width; x++)
      {
        float rAb = imageAbResized.Data[y, x, 2];
        float gAb = imageAbResized.Data[y, x, 1];
        float bAb = imageAbResized.Data[y, x, 0];
        RgbToLab(rAb, gAb, bAb, out _, out float colorA, out float colorB);

        byte rL = imageL.Data[y, x, 2];
        byte gL = imageL.Data[y, x, 1];
        byte bL = imageL.Data[y, x, 0];
        float contentL = RgbToL(rL, gL, bL);

        float r = 0f;
        float g = 0f;
        float b = 0f;
        LabToRgb(contentL, colorA, colorB, ref r, ref g, ref b);
        imageAbResized[y, x] = new Bgr(
          Math.Clamp(b, 0f, 255f),
          Math.Clamp(g, 0f, 255f),
          Math.Clamp(r, 0f, 255f));
      }
    }

    return imageAbResized.Mat.Clone();
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
