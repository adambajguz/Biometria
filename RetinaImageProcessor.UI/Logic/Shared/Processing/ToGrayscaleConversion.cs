// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.Shared.Processing.ToGrayscaleConversion
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using AForge.Imaging.Filters;
using System.Drawing;
using System.Drawing.Imaging;

namespace RetinaImageProcessor.UI.Logic.Shared.Processing
{
  public class ToGrayscaleConversion
  {
    private Grayscale filterR = new Grayscale(1.0, 0.0, 0.0);
    private Grayscale filterG = new Grayscale(0.0, 1.0, 0.0);
    private Grayscale filterB = new Grayscale(0.0, 0.0, 1.0);
    private Grayscale filterAvg = new Grayscale(0.333, 0.333, 0.333);
    private static ToGrayscaleConversion instance;

    public static ToGrayscaleConversion Instance
    {
      get
      {
        if (ToGrayscaleConversion.instance == null)
          ToGrayscaleConversion.instance = new ToGrayscaleConversion();
        return ToGrayscaleConversion.instance;
      }
    }

    private ToGrayscaleConversion()
    {
    }

    public Bitmap ToGrayscale(Bitmap bitmap, ToGrayscaleModes mode)
    {
      if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
      {
        switch (mode)
        {
          case ToGrayscaleModes.Red:
            bitmap = this.filterR.Apply(bitmap);
            break;
          case ToGrayscaleModes.Green:
            bitmap = this.filterG.Apply(bitmap);
            break;
          case ToGrayscaleModes.Blue:
            bitmap = this.filterB.Apply(bitmap);
            break;
          case ToGrayscaleModes.Average:
            bitmap = this.filterAvg.Apply(bitmap);
            break;
        }
      }
      return bitmap;
    }
  }
}
