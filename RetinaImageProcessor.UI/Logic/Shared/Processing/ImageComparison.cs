// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.Shared.Processing.ImageComparison
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;

namespace RetinaImageProcessor.UI.Logic.Shared.Processing
{
  public class ImageComparison
  {
    private static ImageComparison instance;

    public static ImageComparison Instance
    {
      get
      {
        if (ImageComparison.instance == null)
          ImageComparison.instance = new ImageComparison();
        return ImageComparison.instance;
      }
    }

    private ImageComparison()
    {
    }

    public ComparisonStatistics CompareImages(
      Bitmap inputBitmap,
      Bitmap compareBitmap)
    {
      int num1 = 0;
      int num2 = 0;
      int num3 = 0;
      int num4 = 0;
      int num5 = 0;
      if (compareBitmap == null || inputBitmap == null)
        throw new Exception("Load Images First.");
      if (compareBitmap.Width != inputBitmap.Width || compareBitmap.Height != inputBitmap.Height)
        throw new Exception("Compared images have different size.");
      Bitmap thresholdedCompare = new Bitmap(compareBitmap.Width, compareBitmap.Height);
      Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Delegate) (() =>
      {
        for (int x = 0; x < compareBitmap.Width; ++x)
        {
          for (int y = 0; y < compareBitmap.Height; ++y)
          {
            if (compareBitmap.GetPixel(x, y).G > (byte) 128)
              thresholdedCompare.SetPixel(x, y, Color.White);
            else
              thresholdedCompare.SetPixel(x, y, Color.Black);
          }
        }
      }));
      for (int x = 0; x < inputBitmap.Width; ++x)
      {
        for (int y = 0; y < inputBitmap.Height; ++y)
        {
          Color pixel = inputBitmap.GetPixel(x, y);
          int g1 = (int) pixel.G;
          pixel = thresholdedCompare.GetPixel(x, y);
          int g2 = (int) pixel.G;
          if (g1 == g2)
          {
            pixel = inputBitmap.GetPixel(x, y);
            if (pixel.G == byte.MaxValue)
            {
              ++num3;
              ++num1;
            }
            else
            {
              pixel = inputBitmap.GetPixel(x, y);
              if (pixel.G == (byte) 0)
              {
                ++num4;
                ++num2;
              }
            }
          }
          else
          {
            pixel = inputBitmap.GetPixel(x, y);
            if (pixel.G == byte.MaxValue)
            {
              ++num5;
              ++num2;
            }
            else
              ++num1;
          }
        }
      }
      return new ComparisonStatistics()
      {
        Accuracy = (float) (num3 + num4) / (float) (num1 + num2),
        TruePositiveRate = (float) num3 / (float) num1,
        TrueNegativeRate = (float) num4 / (float) num2,
        FalsePositiveRate = (float) num5 / (float) num2
      };
    }
  }
}
