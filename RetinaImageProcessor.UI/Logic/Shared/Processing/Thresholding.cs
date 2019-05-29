// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.Shared.Processing.Thresholding
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using AForge.Imaging.Filters;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;

namespace RetinaImageProcessor.UI.Logic.Shared.Processing
{
  public class Thresholding
  {
    private Erosion Erosion = new Erosion();
    private ToGrayscaleConversion ToGrayscaleConversion = ToGrayscaleConversion.Instance;
    private static Thresholding instance;

    public static Thresholding Instance
    {
      get
      {
        if (Thresholding.instance == null)
          Thresholding.instance = new Thresholding();
        return Thresholding.instance;
      }
    }

    private Thresholding()
    {
    }

    public Bitmap ApplyThreshold(int threshold, Bitmap source)
    {
      Bitmap output = new Bitmap(source.Width, source.Height);
      Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Delegate) (() =>
      {
        for (int x = 0; x < source.Width; ++x)
        {
          for (int y = 0; y < source.Height; ++y)
          {
            if ((int) source.GetPixel(x, y).G > threshold)
              output.SetPixel(x, y, Color.White);
            else
              output.SetPixel(x, y, Color.Black);
          }
        }
      }));
      return this.ToGrayscaleConversion.ToGrayscale(output, ToGrayscaleModes.Green);
    }

    public Bitmap ApplyThresholdMask(int threshold, Bitmap source)
    {
      Bitmap bitmap = new Bitmap(source.Width, source.Height);
      for (int x = 0; x < source.Width; ++x)
      {
        for (int y = 0; y < source.Height; ++y)
        {
          if ((int) source.GetPixel(x, y).G > threshold)
            bitmap.SetPixel(x, y, Color.White);
          else
            bitmap.SetPixel(x, y, Color.Black);
        }
      }
      return this.Erosion.Apply(this.ToGrayscaleConversion.ToGrayscale(bitmap, ToGrayscaleModes.Green));
    }
  }
}
