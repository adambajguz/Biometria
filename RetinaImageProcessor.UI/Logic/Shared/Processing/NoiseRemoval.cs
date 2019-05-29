// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.Shared.Processing.NoiseRemoval
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using AForge;
using AForge.Imaging.Filters;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace RetinaImageProcessor.UI.Logic.Shared.Processing
{
  public class NoiseRemoval
  {
    private static NoiseRemoval instance;

    public static NoiseRemoval Instance
    {
      get
      {
        if (NoiseRemoval.instance == null)
          NoiseRemoval.instance = new NoiseRemoval();
        return NoiseRemoval.instance;
      }
    }

    private NoiseRemoval()
    {
    }

    public Bitmap MedianFilter(int window, int count, Bitmap workingBitmap)
    {
      Bitmap bitmap = (Bitmap) null;
      Median median = new Median(window);
      for (int index = count; index <= count; ++index)
        bitmap = median.Apply(workingBitmap);
      return bitmap ?? workingBitmap;
    }

    public Bitmap RemoveLooseVessels(int MaxVesselLengthToRemove, Bitmap workingBitmap)
    {
      bool[,] visitedPixels = new bool[workingBitmap.Width, workingBitmap.Height];
      List<IntPoint> currentVessel = new List<IntPoint>();
      Bitmap bitmap = AForge.Imaging.Image.Clone(workingBitmap, PixelFormat.Format32bppArgb);
      for (int x = 1; x < workingBitmap.Width - 1; ++x)
      {
        for (int y = 1; y < workingBitmap.Height - 1; ++y)
        {
          if (!visitedPixels[x, y])
          {
            visitedPixels[x, y] = true;
            if (workingBitmap.GetPixel(x, y).G == byte.MaxValue)
            {
              currentVessel.Add(new IntPoint(x, y));
              this.DiscoverConnectedRegion(workingBitmap, visitedPixels, currentVessel, x, y);
            }
            if (currentVessel.Count < MaxVesselLengthToRemove)
            {
              foreach (IntPoint intPoint in currentVessel)
                bitmap.SetPixel(intPoint.X, intPoint.Y, Color.Black);
            }
            currentVessel.Clear();
          }
        }
      }
      return bitmap;
    }

    public void DiscoverConnectedRegion(
      Bitmap preliminaryVessels,
      bool[,] visitedPixels,
      List<IntPoint> currentVessel,
      int x,
      int y)
    {
      currentVessel.Add(new IntPoint(x, y));
      List<IntPoint> intPointList = new List<IntPoint>();
      intPointList.Add(new IntPoint(x, y));
      while (intPointList.Count > 0)
      {
        int index1 = intPointList.Count - 1;
        IntPoint intPoint = intPointList[intPointList.Count - 1];
        for (int index2 = -1; index2 < 2; ++index2)
        {
          for (int index3 = -1; index3 < 2; ++index3)
          {
            int x1 = intPoint.X + index2;
            int y1 = intPoint.Y + index3;
            if (x1 > -1 && x1 < preliminaryVessels.Width - 1 && y1 > -1 && y1 < preliminaryVessels.Height - 1 && !visitedPixels[x1, y1])
            {
              visitedPixels[x1, y1] = true;
              if (preliminaryVessels.GetPixel(x1, y1).G == byte.MaxValue)
              {
                currentVessel.Add(new IntPoint(x1, y1));
                intPointList.Add(new IntPoint(x1, y1));
              }
            }
          }
        }
        intPointList.RemoveAt(index1);
      }
    }

    public void ScanNeighbours(
      int x,
      int y,
      Bitmap workingBitmap,
      bool[,] visitedPixels,
      List<IntPoint> currentVessel)
    {
      for (int index1 = -1; index1 < 2; ++index1)
      {
        for (int index2 = -1; index2 < 2; ++index2)
        {
          int x1 = x + index1;
          int y1 = y + index2;
          if (x1 > -1 && x1 < workingBitmap.Width - 1 && y1 > -1 && y1 < workingBitmap.Height - 1 && !visitedPixels[x1, y1])
          {
            visitedPixels[x1, y1] = true;
            if (workingBitmap.GetPixel(x1, y1).G == byte.MaxValue)
            {
              currentVessel.Add(new IntPoint(x1, y1));
              this.ScanNeighbours(x1, y1, workingBitmap, visitedPixels, currentVessel);
            }
          }
        }
      }
    }
  }
}
