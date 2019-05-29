// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.Shared.Processing.MaskCreation
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace RetinaImageProcessor.UI.Logic.Shared.Processing
{
  public class MaskCreation
  {
    private static MaskCreation instance;

    public static MaskCreation Instance
    {
      get
      {
        if (MaskCreation.instance == null)
          MaskCreation.instance = new MaskCreation();
        return MaskCreation.instance;
      }
    }

    private MaskCreation()
    {
    }

    public Bitmap ApplyMask(Bitmap maskedImage, Bitmap mask)
    {
      maskedImage = AForge.Imaging.Image.Clone(maskedImage, PixelFormat.Format32bppArgb);
      for (int x = 0; x < maskedImage.Width; ++x)
      {
        for (int y = 0; y < maskedImage.Height; ++y)
        {
          if (mask.GetPixel(x, y).G == (byte) 0)
            maskedImage.SetPixel(x, y, Color.Black);
        }
      }
      maskedImage = new Grayscale(0.0, 1.0, 0.0).Apply(maskedImage);
      return maskedImage;
    }

    public int FindMaskBinarizationThreshold(Bitmap bitmap)
    {
      ImageStatistics imageStatistics = new ImageStatistics(bitmap);
      int[] numArray = !imageStatistics.IsGrayscale ? imageStatistics.Green.Values : imageStatistics.Gray.Values;
      int num1 = ((IEnumerable<int>) numArray).Max() / 10;
      int num2 = 0;
      for (int index = 0; index < numArray.Length; ++index)
      {
        if (num2 > 10)
          return index;
        if (numArray[index] < num1)
          ++num2;
        else
          num2 = 0;
      }
      return -1;
    }
  }
}
