// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.Shared.Processing.FeatureExtraction
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using AForge;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace RetinaImageProcessor.UI.Logic.Shared.Processing
{
  public class FeatureExtraction
  {
    private Pen weakTerminationPen = new Pen(Color.Gray);
    private Pen strongTerminationPen = new Pen(Color.Green);
    private Pen bifurcationPen = new Pen(Color.Red);
    private Pen crossoverPen = new Pen(Color.Blue);
    private int weakTerminationMaxLength = 10;
    private int weakBifurcationMaxLength = 10;
    private static FeatureExtraction instance;

    public static FeatureExtraction Instance
    {
      get
      {
        if (FeatureExtraction.instance == null)
          FeatureExtraction.instance = new FeatureExtraction();
        return FeatureExtraction.instance;
      }
    }

    private FeatureExtraction()
    {
    }

    public Bitmap ExtractMinutiae(
      Bitmap inputBitmap,
      List<IntPoint> strongTerminations,
      List<IntPoint> weakTerminations,
      List<IntPoint> bifurcations,
      List<IntPoint> crossovers)
    {
      List<IntPoint> currentVessel = new List<IntPoint>();
      bool[,] flagArray = new bool[inputBitmap.Width, inputBitmap.Height];
      Bitmap bitmap = AForge.Imaging.Image.Clone(inputBitmap, PixelFormat.Format24bppRgb);
      for (int x = 0; x < bitmap.Width; ++x)
      {
        for (int y = 0; y < bitmap.Height; ++y)
          bitmap.SetPixel(x, y, inputBitmap.GetPixel(x, y));
      }
      for (int x = 1; x < inputBitmap.Width - 1; ++x)
      {
        for (int y = 1; y < inputBitmap.Height - 1; ++y)
        {
          if (inputBitmap.GetPixel(x, y).G == byte.MaxValue)
            this.CountMinutiae(x, y, inputBitmap, strongTerminations, bifurcations, crossovers);
        }
      }
      foreach (IntPoint strongTermination in strongTerminations)
      {
        bool[,] visitedPixels = new bool[inputBitmap.Width, inputBitmap.Height];
        currentVessel.Clear();
        visitedPixels[strongTermination.X, strongTermination.Y] = true;
        currentVessel.Add(strongTermination);
        this.ScanNeighbourhoodForAnotherMinutiae(strongTermination.X, strongTermination.Y, inputBitmap, currentVessel, visitedPixels, strongTerminations, weakTerminations, bifurcations, crossovers);
      }
      strongTerminations.RemoveAll((Predicate<IntPoint>) (pt => weakTerminations.Contains(pt)));
      List<IntPoint> foundWeakBifurcations = new List<IntPoint>();
      foreach (IntPoint bifurcation in bifurcations)
      {
        bool[,] visitedPixels = new bool[inputBitmap.Width, inputBitmap.Height];
        currentVessel.Clear();
        visitedPixels[bifurcation.X, bifurcation.Y] = true;
        currentVessel.Add(bifurcation);
        int[] branchLengths = new int[3];
        this.ScanNeighbourhoodForBifurcationLengthsInit(bifurcation.X, bifurcation.Y, inputBitmap, visitedPixels, branchLengths, 0);
        if (((IEnumerable<int>) branchLengths).Any<int>((Func<int, bool>) (x => x < this.weakBifurcationMaxLength)))
          foundWeakBifurcations.Add(bifurcation);
      }
      bifurcations.RemoveAll((Predicate<IntPoint>) (pt => foundWeakBifurcations.Contains(pt)));
      List<IntPoint> foundWeakCrossovers = new List<IntPoint>();
      foreach (IntPoint crossover in crossovers)
      {
        bool[,] visitedPixels = new bool[inputBitmap.Width, inputBitmap.Height];
        currentVessel.Clear();
        visitedPixels[crossover.X, crossover.Y] = true;
        currentVessel.Add(crossover);
        int[] branchLengths = new int[4];
        this.ScanNeighbourhoodForBifurcationLengthsInit(crossover.X, crossover.Y, inputBitmap, visitedPixels, branchLengths, 0);
        if (((IEnumerable<int>) branchLengths).Any<int>((Func<int, bool>) (x => x < this.weakBifurcationMaxLength)))
          foundWeakCrossovers.Add(crossover);
      }
      crossovers.RemoveAll((Predicate<IntPoint>) (pt => foundWeakCrossovers.Contains(pt)));
      return bitmap;
    }

    public void CountMinutiae(
      int x,
      int y,
      Bitmap inputBitmap,
      List<IntPoint> strongTerminations,
      List<IntPoint> bifurcations,
      List<IntPoint> crossovers)
    {
      int num = 0;
      for (int index1 = -1; index1 <= 1; ++index1)
      {
        for (int index2 = -1; index2 <= 1; ++index2)
        {
          if (inputBitmap.GetPixel(x + index1, y + index2).G == byte.MaxValue)
            ++num;
        }
      }
      switch (num)
      {
        case 2:
          strongTerminations.Add(new IntPoint(x, y));
          break;
        case 4:
          for (int index1 = -1; index1 <= 1; ++index1)
          {
            for (int index2 = -1; index2 <= 1; ++index2)
            {
              IntPoint intPoint = new IntPoint(x + index1, y + index2);
              if (bifurcations.Contains(intPoint) || crossovers.Contains(intPoint))
                return;
            }
          }
          bifurcations.Add(new IntPoint(x, y));
          break;
        case 5:
          crossovers.Add(new IntPoint(x, y));
          break;
      }
    }

    public void ScanNeighbourhoodForBifurcationLengthsInit(
      int x,
      int y,
      Bitmap inputBitmap,
      bool[,] visitedPixels,
      int[] branchLengths,
      int branchID)
    {
      for (int index1 = -1; index1 <= 1; ++index1)
      {
        for (int index2 = -1; index2 <= 1; ++index2)
        {
          int index3 = x + index1;
          int index4 = y + index2;
          visitedPixels[index3, index4] = true;
        }
      }
      for (int index1 = -1; index1 <= 1; ++index1)
      {
        for (int index2 = -1; index2 <= 1; ++index2)
        {
          int x1 = x + index1;
          int y1 = y + index2;
          if ((index1 != 0 || (uint) index2 > 0U) && inputBitmap.GetPixel(x1, y1).G == byte.MaxValue)
          {
            ++branchID;
            this.ScanNeighbourhoodForBifurcationLengths(x1, y1, inputBitmap, visitedPixels, branchLengths, branchID);
          }
        }
      }
    }

    public void ScanNeighbourhoodForBifurcationLengths(
      int x,
      int y,
      Bitmap inputBitmap,
      bool[,] visitedPixels,
      int[] branchLengths,
      int branchID)
    {
      if (branchLengths[branchID - 1] >= this.weakBifurcationMaxLength)
        return;
      for (int index1 = -1; index1 <= 1; ++index1)
      {
        for (int index2 = -1; index2 <= 1; ++index2)
        {
          int x1 = x + index1;
          int y1 = y + index2;
          if (x1 >= 0 && x1 < inputBitmap.Width && y1 >= 0 && y1 < inputBitmap.Height && !visitedPixels[x1, y1])
          {
            visitedPixels[x1, y1] = true;
            if (inputBitmap.GetPixel(x1, y1).G == byte.MaxValue)
            {
              ++branchLengths[branchID - 1];
              this.ScanNeighbourhoodForBifurcationLengths(x1, y1, inputBitmap, visitedPixels, branchLengths, branchID);
            }
          }
        }
      }
    }

    public void ScanNeighbourhoodForAnotherMinutiae(
      int x,
      int y,
      Bitmap inputBitmap,
      List<IntPoint> currentVessel,
      bool[,] visitedPixels,
      List<IntPoint> strongTerminations,
      List<IntPoint> weakTerminations,
      List<IntPoint> bifurcations,
      List<IntPoint> crossovers)
    {
      if (currentVessel.Count >= this.weakTerminationMaxLength)
        return;
      for (int index1 = -1; index1 <= 1; ++index1)
      {
        for (int index2 = -1; index2 <= 1; ++index2)
        {
          int x1 = x + index1;
          int y1 = y + index2;
          if (x1 >= 0 && x1 < inputBitmap.Width && y1 >= 0 && y1 < inputBitmap.Height && !visitedPixels[x1, y1])
          {
            visitedPixels[x1, y1] = true;
            if (inputBitmap.GetPixel(x1, y1).G == byte.MaxValue)
            {
              IntPoint intPoint = new IntPoint(x1, y1);
              if (bifurcations.Contains(intPoint) || crossovers.Contains(intPoint) || strongTerminations.Contains(intPoint))
              {
                weakTerminations.Add(currentVessel[0]);
                return;
              }
              currentVessel.Add(intPoint);
              this.ScanNeighbourhoodForAnotherMinutiae(x1, y1, inputBitmap, currentVessel, visitedPixels, strongTerminations, weakTerminations, bifurcations, crossovers);
            }
          }
        }
      }
    }

    public void MarkMinutiae(
      Bitmap output,
      List<IntPoint> strongTerminations,
      List<IntPoint> weakTerminations,
      List<IntPoint> bifurcations,
      List<IntPoint> crossovers)
    {
      Graphics graphics = Graphics.FromImage((System.Drawing.Image) output);
      foreach (IntPoint weakTermination in weakTerminations)
        ;
      foreach (IntPoint strongTermination in strongTerminations)
        graphics.DrawRectangle(this.strongTerminationPen, strongTermination.X - 3, strongTermination.Y - 3, 6, 6);
      foreach (IntPoint bifurcation in bifurcations)
        graphics.DrawRectangle(this.bifurcationPen, bifurcation.X - 3, bifurcation.Y - 3, 6, 6);
      foreach (IntPoint crossover in crossovers)
        graphics.DrawRectangle(this.crossoverPen, crossover.X - 3, crossover.Y - 3, 6, 6);
    }
  }
}
