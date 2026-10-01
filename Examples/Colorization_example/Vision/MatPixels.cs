using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Colorization_example.Vision;

/// <summary>8-bit BGR pixel access on <see cref="Mat"/>; Emgu.CV 5 no longer ships <c>Image&lt;,&gt;</c>.</summary>
internal static class MatPixels
{
  /// <summary>Returns a continuous 8-bit, 3-channel BGR copy of a gray, BGR or BGRA 8-bit image.</summary>
  public static Mat ToBgr8(Mat image)
  {
    if (image.Depth != DepthType.Cv8U)
    {
      throw new ArgumentException("Only 8-bit images are supported.", nameof(image));
    }

    switch (image.NumberOfChannels)
    {
      case 3:
        return image.Clone();
      case 1:
      case 4:
        var bgr = new Mat();
        CvInvoke.CvtColor(image, bgr, image.NumberOfChannels == 1 ? ColorConversion.Gray2Bgr : ColorConversion.Bgra2Bgr);
        return bgr;
      default:
        throw new ArgumentException($"Unsupported channel count: {image.NumberOfChannels}.", nameof(image));
    }
  }

  /// <summary>Copies a continuous 8-bit BGR image into a row-major array with 3 bytes per pixel.</summary>
  public static byte[] ReadBgr(Mat bgr)
  {
    var data = new byte[bgr.Rows * bgr.Cols * 3];
    bgr.CopyTo(data);
    return data;
  }

  /// <summary>Creates an 8-bit BGR image from a row-major array with 3 bytes per pixel.</summary>
  public static Mat CreateBgr(int width, int height, byte[] bgr)
  {
    var mat = new Mat(height, width, DepthType.Cv8U, 3);
    mat.SetTo(bgr);
    return mat;
  }

  public static byte ToByte(float value) => (byte)MathF.Round(Math.Clamp(value, 0f, 255f));
}
