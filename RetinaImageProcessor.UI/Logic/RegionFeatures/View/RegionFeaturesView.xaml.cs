// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.RegionFeatures.View.RegionFeaturesView
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using Microsoft.Win32;
using RetinaImageProcessor.UI.Logic.Shared.Processing;
using RetinaImageProcessor.UI.Logic.Shared.View;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RetinaImageProcessor.UI.Logic.RegionFeatures.View
{
  public partial class RegionFeaturesView : UserControl, IStatefulView, INotifyPropertyChanged, IComponentConnector
  {
    private static float EXTENSIBILITY1 = 0.35f;
    private static float EXTENSIBILITY2 = 0.25f;
    private static float SOLIDITY = 0.53f;
    private static float VRATIO = 2.2f;
    private List<IntPoint> strongTerminations = new List<IntPoint>();
    private List<IntPoint> weakTerminations = new List<IntPoint>();
    private List<IntPoint> bifurcations = new List<IntPoint>();
    private List<IntPoint> crossovers = new List<IntPoint>();
    private Invert invert = new Invert();
    private FeatureExtraction FeatureExtraction = FeatureExtraction.Instance;
    private ImageComparison ImageComparison = ImageComparison.Instance;
    private int threshold1 = 89;
    private int threshold2 = 51;
    private MaskCreation MaskCreation = MaskCreation.Instance;
    private GrahamConvexHull GrahamConvexHull = new GrahamConvexHull();
    private NoiseRemoval NoiseRemoval = NoiseRemoval.Instance;
    private Thinning Thinning = Thinning.Instance;
    private Thresholding Thresholding = Thresholding.Instance;
    private ToGrayscaleConversion ToGrayscaleConversion = ToGrayscaleConversion.Instance;
    private Bitmap[] scalingCoefficients = new Bitmap[4];
    private Bitmap[] waveletCoefficient = new Bitmap[4];
    private int[] filterSequence = new int[25]
    {
      1,
      1,
      1,
      1,
      1,
      1,
      4,
      4,
      4,
      1,
      1,
      4,
      6,
      4,
      1,
      1,
      4,
      4,
      4,
      1,
      1,
      1,
      1,
      1,
      1
    };
    private ToGrayscaleModes grayscaleMode = ToGrayscaleModes.Green;
    private int maskThreshold = 20;
    private float truePositiveRate = 0.0f;
    private float trueNegativeRate = 0.0f;
    private float falsePositiveRate = 0.0f;
    private float accuracy = 0.0f;
    private bool isButtonEnabled = true;
    private SortedDictionary<int, List<IntPoint>> hierarchies;
    private Bitmap mask;
    private float internalFactor;
    private int areaTrshld1;
    private int areaTrshld2;
    private int areaTrshld3;
    private int areaTrshld4;
    private short[,] structElem0;
    private Bitmap[] angularBitmaps;
    private double[] angles;
    private List<System.Windows.Controls.Image> TopHatImages;
    private string imageName;
    private string path;
    private List<StateRegionFeatures> states;
    private int stateId;
    private Bitmap workingBitmap;
    private Bitmap compareBitmap;
    private string lastOperationLabel;
    private string processedFileLabel;
    private string comparedFileLabel;
    private string maskFileLabel;
    private StateOperations lastOperation;
    private int terminationCount;
    private int bifurcationCount;
    private int crossoverCount;
    private PointCollection histogramPoints;
    private PointCollection sliderPoints;
    internal Slider maskSlider;
    internal TextBox RemoveVesselTextBox;
    internal System.Windows.Controls.Image PreviousImage1;
    internal System.Windows.Controls.Image PreviousImage2;
    internal System.Windows.Controls.Image TopHatImage1;
    internal System.Windows.Controls.Image TopHatImage2;
    internal System.Windows.Controls.Image TopHatImage3;
    internal System.Windows.Controls.Image TopHatImage4;
    internal System.Windows.Controls.Image TopHatImage5;
    internal System.Windows.Controls.Image TopHatImage6;
    internal System.Windows.Controls.Image TopHatImage7;
    internal System.Windows.Controls.Image TopHatImage8;
    internal System.Windows.Controls.Image TopHatImage9;
    internal System.Windows.Controls.Image TopHatImage10;
    internal System.Windows.Controls.Image TopHatImage11;
    internal System.Windows.Controls.Image TopHatImage12;
    internal System.Windows.Controls.Image TopHat;
    internal System.Windows.Controls.Image PreliminaryVesselImage;
    internal System.Windows.Controls.Image UndeterminedRegionsImage;
    internal System.Windows.Controls.Image CurrentImage;
    internal System.Windows.Controls.Image CompareImage;
    internal System.Windows.Controls.Image MaskImage;
    private bool _contentLoaded;

    private void ExtractMinutiae_Click(object sender, RoutedEventArgs e)
    {
      this.IsButtonEnabled = false;
      this.weakTerminations.Clear();
      this.strongTerminations.Clear();
      this.bifurcations.Clear();
      this.crossovers.Clear();
      Bitmap output = this.invert.Apply(this.FeatureExtraction.ExtractMinutiae(this.workingBitmap, this.strongTerminations, this.weakTerminations, this.bifurcations, this.crossovers));
      this.FeatureExtraction.MarkMinutiae(output, this.strongTerminations, this.weakTerminations, this.bifurcations, this.crossovers);
      this.workingBitmap = output;
      this.TerminationCount = this.strongTerminations.Count;
      this.BifurcationCount = this.bifurcations.Count;
      this.CrossoverCount = this.crossovers.Count;
      this.LastOperation = StateOperations.ExtractMinutiae;
      this.ShowNextImage();
    }

    private void ApplyHierarchicalGrowth_Click(object sender, RoutedEventArgs e)
    {
      if (this.workingBitmap != null && this.states[this.stateId].undeterminedRegions != null && this.states[this.stateId].topHat != null)
      {
        this.IsButtonEnabled = false;
        this.Initialization();
        this.HierarchyUpdate();
        this.LastOperation = StateOperations.ApplyHierarchicalGrowth;
        this.ShowNextImage();
      }
      else
      {
        int num = (int) MessageBox.Show("Preliminary Vessels Data Or Undetermined Regions Data Not Found.");
      }
    }

    private void Initialization()
    {
      Bitmap undeterminedRegions = this.states[this.stateId].undeterminedRegions;
      bool[,] flagArray1 = new bool[this.workingBitmap.Width, this.workingBitmap.Height];
      for (int x = 0; x < this.workingBitmap.Width; ++x)
      {
        for (int y = 0; y < this.workingBitmap.Height; ++y)
          flagArray1[x, y] = this.workingBitmap.GetPixel(x, y).G == byte.MaxValue;
      }
      bool[,] flagArray2 = new bool[undeterminedRegions.Width, undeterminedRegions.Height];
      for (int x = 0; x < undeterminedRegions.Width; ++x)
      {
        for (int y = 0; y < undeterminedRegions.Height; ++y)
          flagArray2[x, y] = undeterminedRegions.GetPixel(x, y).G == byte.MaxValue;
      }
      this.hierarchies = new SortedDictionary<int, List<IntPoint>>();
      int num1 = this.workingBitmap.Width / 8;
      int num2 = (int) System.Math.Sqrt((double) (2 * num1 * num1));
      for (int x = 0; x < undeterminedRegions.Width; ++x)
      {
        for (int y = 0; y < undeterminedRegions.Height; ++y)
        {
          int key = num2;
          if (flagArray2[x, y])
          {
            int num3 = x - num1 > 0 ? x - num1 : 0;
            int num4 = x + num1 < undeterminedRegions.Width ? x + num1 : undeterminedRegions.Width;
            int num5 = y - num1 > 0 ? y - num1 : 0;
            int num6 = y + num1 < undeterminedRegions.Height ? y + num1 : undeterminedRegions.Height;
            for (int index1 = num3; index1 < num4; ++index1)
            {
              for (int index2 = num5; index2 < num6; ++index2)
              {
                if (index1 != x && index2 != y && flagArray1[index1, index2])
                {
                  int num7 = (int) System.Math.Sqrt((double) ((index1 - x) * (index1 - x) + (index2 - y) * (index2 - y)));
                  if (num7 < key)
                    key = num7;
                }
              }
            }
            List<IntPoint> intPointList;
            if (!this.hierarchies.TryGetValue(key, out intPointList))
            {
              intPointList = new List<IntPoint>();
              this.hierarchies.Add(key, intPointList);
            }
            intPointList.Add(new IntPoint(x, y));
          }
        }
      }
    }

    private void HierarchyUpdate()
    {
      this.workingBitmap = AForge.Imaging.Image.Clone(this.workingBitmap, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      Bitmap topHat = this.states[this.stateId].topHat;
      int[,] numArray = new int[topHat.Width, topHat.Height];
      for (int x = 0; x < topHat.Width; ++x)
      {
        for (int y = 0; y < topHat.Height; ++y)
          numArray[x, y] = (int) topHat.GetPixel(x, y).G;
      }
      double num1 = 4.0 * System.Math.Sqrt(2.0);
      double num2 = 1.0;
      foreach (List<IntPoint> intPointList in this.hierarchies.Values)
      {
        foreach (IntPoint intPoint in intPointList)
        {
          double num3 = double.MaxValue;
          int x = -1;
          int y = -1;
          for (int index1 = -4; index1 <= 4; ++index1)
          {
            for (int index2 = -4; index2 <= 4; ++index2)
            {
              if (index1 != 0 && (uint) index2 > 0U)
              {
                int index3 = intPoint.X + index1;
                int index4 = intPoint.Y + index2;
                if (index3 > -1 && index3 < topHat.Width - 1 && index4 > -1 && index4 < topHat.Height - 1)
                {
                  double num4 = (double) System.Math.Abs(numArray[intPoint.X, intPoint.Y] - numArray[index3, index4]) / (double) byte.MaxValue + 0.5 * ((System.Math.Sqrt(System.Math.Pow((double) index1, 2.0) + System.Math.Pow((double) index2, 2.0)) - num2) / (num1 - num2));
                  if (num4 < num3)
                  {
                    num3 = num4;
                    x = index3;
                    y = index4;
                  }
                }
              }
            }
          }
          if (this.workingBitmap.GetPixel(x, y).G == byte.MaxValue)
            this.workingBitmap.SetPixel(intPoint.X, intPoint.Y, System.Drawing.Color.White);
        }
      }
    }

    private void CompareImages_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        ComparisonStatistics comparisonStatistics = this.ImageComparison.CompareImages(this.workingBitmap, this.compareBitmap);
        this.Accuracy = comparisonStatistics.Accuracy;
        this.FalsePositiveRate = comparisonStatistics.FalsePositiveRate;
        this.TrueNegativeRate = comparisonStatistics.TrueNegativeRate;
        this.TruePositiveRate = comparisonStatistics.TruePositiveRate;
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.Message);
      }
    }

    private void ImageDivision_Click(object sender, RoutedEventArgs e)
    {
      if (this.states[this.stateId].topHat != null)
      {
        Bitmap topHat = this.states[this.stateId].topHat;
        this.IsButtonEnabled = false;
        Bitmap preliminaryVessels = new Bitmap(topHat.Width, topHat.Height);
        Bitmap undeterminedRegions = new Bitmap(topHat.Width, topHat.Height);
        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Delegate) (() =>
        {
          for (int x = 0; x < topHat.Width; ++x)
          {
            for (int y = 0; y < topHat.Height; ++y)
            {
              System.Drawing.Color pixel = topHat.GetPixel(x, y);
              if ((int) pixel.G >= this.threshold1)
              {
                preliminaryVessels.SetPixel(x, y, System.Drawing.Color.White);
                undeterminedRegions.SetPixel(x, y, System.Drawing.Color.Black);
              }
              else
              {
                pixel = topHat.GetPixel(x, y);
                if ((int) pixel.G >= this.threshold2)
                {
                  undeterminedRegions.SetPixel(x, y, System.Drawing.Color.White);
                  preliminaryVessels.SetPixel(x, y, System.Drawing.Color.Black);
                }
                else
                {
                  preliminaryVessels.SetPixel(x, y, System.Drawing.Color.Black);
                  undeterminedRegions.SetPixel(x, y, System.Drawing.Color.Black);
                }
              }
            }
          }
        }));
        this.states[this.stateId].preliminaryVessels = preliminaryVessels;
        this.states[this.stateId].undeterminedRegions = undeterminedRegions;
        this.LastOperation = StateOperations.DivideImage;
        this.ShowNextImage();
      }
      else
      {
        int num = (int) MessageBox.Show("Top Hat Result Not Found.");
      }
    }

    public bool IsMaskHandpicked { get; set; } = false;

    private void CreateMask_Click(object sender, RoutedEventArgs e)
    {
      this.IsButtonEnabled = false;
      this.mask = this.Thresholding.ApplyThresholdMask(this.MaskThreshold, this.workingBitmap);
      this.workingBitmap = this.MaskCreation.ApplyMask(this.workingBitmap, this.mask);
      this.internalFactor = 7f * (float) System.Math.Max(this.workingBitmap.Width, this.workingBitmap.Height) / (float) System.Math.Min(this.workingBitmap.Width, this.workingBitmap.Height);
      this.areaTrshld1 = (int) System.Math.Round((double) this.internalFactor * 2.0);
      this.areaTrshld2 = (int) System.Math.Round((double) this.internalFactor * 8.0);
      this.areaTrshld3 = (int) System.Math.Round((double) this.internalFactor * 2.5);
      this.areaTrshld4 = (int) System.Math.Round((double) this.internalFactor * 35.0);
      this.LastOperation = StateOperations.CreateMask;
      this.ShowNextMask();
    }

    private void SetMask(Bitmap newMask)
    {
      this.IsButtonEnabled = false;
      this.mask = this.ToGrayscaleConversion.ToGrayscale(newMask, ToGrayscaleModes.Green);
      this.IsMaskHandpicked = true;
      if (this.workingBitmap != null)
        this.workingBitmap = this.MaskCreation.ApplyMask(this.workingBitmap, this.mask);
      this.internalFactor = 7f * (float) System.Math.Max(this.workingBitmap.Width, this.workingBitmap.Height) / (float) System.Math.Min(this.workingBitmap.Width, this.workingBitmap.Height);
      this.areaTrshld1 = (int) System.Math.Round((double) this.internalFactor * 2.0);
      this.areaTrshld2 = (int) System.Math.Round((double) this.internalFactor * 8.0);
      this.areaTrshld3 = (int) System.Math.Round((double) this.internalFactor * 2.5);
      this.areaTrshld4 = (int) System.Math.Round((double) this.internalFactor * 35.0);
      this.LastOperation = StateOperations.CreateMask;
      this.ShowNextMask();
    }

    private void FindMaskBinarizationThreshold_Click(object sender, RoutedEventArgs e)
    {
      int binarizationThreshold = this.MaskCreation.FindMaskBinarizationThreshold(this.workingBitmap);
      if (binarizationThreshold == -1)
      {
        int num = (int) MessageBox.Show("Couldn't find threshold.");
      }
      else
        this.MaskThreshold = binarizationThreshold;
    }

    private void MedianFilter_Click(object sender, RoutedEventArgs e)
    {
      this.IsButtonEnabled = false;
      this.workingBitmap = this.NoiseRemoval.MedianFilter(3, 1, this.workingBitmap);
      this.LastOperation = StateOperations.RemoveNoise;
      this.ShowNextImage();
    }

    private void DenoisePreliminaryVessels_Click(object sender, RoutedEventArgs e)
    {
      Bitmap preliminaryVessels = this.states[this.stateId].preliminaryVessels;
      if (preliminaryVessels != null)
      {
        this.IsButtonEnabled = false;
        bool[,] visitedPixels = new bool[preliminaryVessels.Width, preliminaryVessels.Height];
        List<IntPoint> currentVessel = new List<IntPoint>();
        Bitmap bitmap = AForge.Imaging.Image.Clone(preliminaryVessels, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        this.DenoisePreliminaryVessels(preliminaryVessels, visitedPixels, currentVessel, bitmap);
        this.states[this.stateId].preliminaryVessels = this.ToGrayscaleConversion.ToGrayscale(bitmap, ToGrayscaleModes.Green);
        this.LastOperation = StateOperations.DenoisePreliminaryVessels;
        this.ShowNextImage();
      }
      else
      {
        int num = (int) MessageBox.Show("Preliminary Vessels Data Not Found.");
      }
    }

    private void DenoisePreliminaryVessels(
      Bitmap preliminaryVessels,
      bool[,] visitedPixels,
      List<IntPoint> currentVessel,
      Bitmap output)
    {
      for (int x = 1; x < preliminaryVessels.Width - 1; ++x)
      {
        for (int y = 1; y < preliminaryVessels.Height - 1; ++y)
        {
          if (!visitedPixels[x, y])
          {
            visitedPixels[x, y] = true;
            if (preliminaryVessels.GetPixel(x, y).G == byte.MaxValue)
              this.NoiseRemoval.DiscoverConnectedRegion(preliminaryVessels, visitedPixels, currentVessel, x, y);
            int count = currentVessel.Count;
            if ((uint) count > 0U)
            {
              IntPoint minXY;
              IntPoint maxXY;
              PointsCloud.GetBoundingRectangle((IEnumerable<IntPoint>) currentVessel, out minXY, out maxXY);
              IntPoint intPoint1 = IntPoint.Subtract(maxXY, minXY);
              int num1 = intPoint1.X * intPoint1.Y;
              float num2 = (float) count / (float) num1;
              float num3 = (float) intPoint1.X / (float) intPoint1.Y;
              int num4 = this.ConvexArea(this.GrahamConvexHull.FindHull(currentVessel), minXY, maxXY);
              double num5 = (double) count / (double) num4;
              if (count >= this.areaTrshld1 && count <= this.areaTrshld2 && ((double) num2 <= (double) RegionFeaturesView.EXTENSIBILITY1 && (double) num3 <= (double) RegionFeaturesView.VRATIO && num5 >= (double) RegionFeaturesView.SOLIDITY))
              {
                foreach (IntPoint intPoint2 in currentVessel)
                  output.SetPixel(intPoint2.X, intPoint2.Y, System.Drawing.Color.Black);
              }
            }
            currentVessel.Clear();
          }
        }
      }
    }

    private int ConvexArea(List<IntPoint> polygon, IntPoint minXY, IntPoint maxXY)
    {
      int num = 0;
      for (int x = minXY.X; x <= maxXY.X; ++x)
      {
        for (int y = minXY.Y; y <= maxXY.Y; ++y)
        {
          if (RegionFeaturesView.IsPointInPolygon(polygon, new IntPoint(x, y)))
            ++num;
        }
      }
      return num;
    }

    private double PolygonArea(List<IntPoint> polygon)
    {
      int count = polygon.Count;
      double num = 0.0;
      for (int index1 = 0; index1 < count; ++index1)
      {
        int index2 = (index1 + 1) % count;
        num = num + (double) (polygon[index1].X * polygon[index2].Y) - (double) (polygon[index2].X * polygon[index1].Y);
      }
      return System.Math.Abs(num) / 2.0;
    }

    public static bool IsPointInPolygon(List<IntPoint> polygon, IntPoint point)
    {
      bool flag = false;
      int index1 = polygon.Count - 1;
      for (int index2 = 0; index2 < polygon.Count; ++index2)
      {
        if ((polygon[index2].Y < point.Y && polygon[index1].Y >= point.Y || polygon[index1].Y < point.Y && polygon[index2].Y >= point.Y) && polygon[index2].X + (point.Y - polygon[index2].Y) / (polygon[index1].Y - polygon[index2].Y) * (polygon[index1].X - polygon[index2].X) < point.X)
          flag = !flag;
        index1 = index2;
      }
      return flag;
    }

    private void RemoveNonVesselRegions_Click(object sender, RoutedEventArgs e)
    {
      if (this.workingBitmap != null)
      {
        this.IsButtonEnabled = false;
        bool[,] visitedPixels = new bool[this.workingBitmap.Width, this.workingBitmap.Height];
        List<IntPoint> currentVessel = new List<IntPoint>();
        Bitmap bitmap = AForge.Imaging.Image.Clone(this.workingBitmap, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        for (int x = 1; x < this.workingBitmap.Width - 1; ++x)
        {
          for (int y = 1; y < this.workingBitmap.Height - 1; ++y)
          {
            if (!visitedPixels[x, y])
            {
              visitedPixels[x, y] = true;
              if (this.workingBitmap.GetPixel(x, y).G == byte.MaxValue)
                this.NoiseRemoval.DiscoverConnectedRegion(this.workingBitmap, visitedPixels, currentVessel, x, y);
              int count = currentVessel.Count;
              if ((uint) count > 0U && count <= this.areaTrshld4)
              {
                IntPoint minXY;
                IntPoint maxXY;
                PointsCloud.GetBoundingRectangle((IEnumerable<IntPoint>) currentVessel, out minXY, out maxXY);
                IntPoint intPoint1 = IntPoint.Subtract(maxXY, minXY);
                int num1 = intPoint1.X * intPoint1.Y;
                float num2 = (float) count / (float) num1;
                float num3 = (float) intPoint1.X / (float) intPoint1.Y;
                if ((double) num2 > (double) RegionFeaturesView.EXTENSIBILITY2 && (double) num3 < (double) RegionFeaturesView.VRATIO)
                {
                  foreach (IntPoint intPoint2 in currentVessel)
                    bitmap.SetPixel(intPoint2.X, intPoint2.Y, System.Drawing.Color.Black);
                }
              }
              currentVessel.Clear();
            }
          }
        }
        this.workingBitmap = this.ToGrayscaleConversion.ToGrayscale(bitmap, ToGrayscaleModes.Green);
        this.LastOperation = StateOperations.RemoveNonVesselRegions;
        this.ShowNextImage();
      }
      else
      {
        int num = (int) MessageBox.Show("Image Not Found.");
      }
    }

    private void RemoveLooseVessels_Click(object sender, RoutedEventArgs e)
    {
      this.IsButtonEnabled = false;
      this.workingBitmap = this.NoiseRemoval.RemoveLooseVessels(this.MaxVesselLengthToRemove, this.workingBitmap);
      this.LastOperation = StateOperations.RemoveLooseVessels;
      this.ShowNextImage();
    }

    private void K3MThinning_Click(object sender, RoutedEventArgs e)
    {
      if (this.workingBitmap != null)
      {
        this.IsButtonEnabled = false;
        this.workingBitmap = this.Thinning.K3MThinning(this.workingBitmap);
        this.LastOperation = StateOperations.Skeletonize;
        this.ShowNextImage();
      }
      else
      {
        int num = (int) MessageBox.Show("Image Not Found.");
      }
    }

    private void ExtractWaveletSkeleton_Click(object sender, RoutedEventArgs e)
    {
      this.IsButtonEnabled = false;
      List<List<IntPoint>> intPointListList = new List<List<IntPoint>>();
      bool[,] visitedPixels = new bool[this.workingBitmap.Width, this.workingBitmap.Height];
      List<IntPoint> currentVessel = new List<IntPoint>();
      Bitmap bitmap = AForge.Imaging.Image.Clone(this.workingBitmap, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      this.ExtractWaveletSkeleton(visitedPixels, currentVessel, bitmap);
      this.workingBitmap = this.Thinning.K3MThinning(bitmap);
      this.LastOperation = StateOperations.ExtractSkeleton;
      this.ShowNextImage();
    }

    private void ExtractWaveletSkeleton(
      bool[,] visitedPixels,
      List<IntPoint> currentVessel,
      Bitmap output)
    {
      for (int x = 1; x < this.workingBitmap.Width - 1; ++x)
      {
        for (int y = 1; y < this.workingBitmap.Height - 1; ++y)
        {
          if (!visitedPixels[x, y])
          {
            visitedPixels[x, y] = true;
            if (this.workingBitmap.GetPixel(x, y).G == byte.MaxValue)
              this.NoiseRemoval.DiscoverConnectedRegion(this.workingBitmap, visitedPixels, currentVessel, x, y);
            int count = currentVessel.Count;
            if (count > 0)
            {
              IntPoint minXY;
              IntPoint maxXY;
              PointsCloud.GetBoundingRectangle((IEnumerable<IntPoint>) currentVessel, out minXY, out maxXY);
              IntPoint intPoint1 = IntPoint.Subtract(maxXY, minXY);
              int num1 = intPoint1.X * intPoint1.Y;
              float num2 = (float) count / (float) num1;
              float num3 = (float) intPoint1.X / (float) intPoint1.Y;
              if (count < this.areaTrshld3)
              {
                foreach (IntPoint intPoint2 in currentVessel)
                  output.SetPixel(intPoint2.X, intPoint2.Y, System.Drawing.Color.Black);
              }
              else if (count >= this.areaTrshld3 && count <= this.areaTrshld4 && ((double) num2 <= (double) RegionFeaturesView.EXTENSIBILITY2 || (double) num3 > (double) RegionFeaturesView.VRATIO))
              {
                foreach (IntPoint intPoint2 in currentVessel)
                  output.SetPixel(intPoint2.X, intPoint2.Y, System.Drawing.Color.Black);
              }
            }
            currentVessel.Clear();
          }
        }
      }
    }

    private void MergeWithPreliminaryVessels_Click(object sender, RoutedEventArgs e)
    {
      if (this.states[this.stateId].preliminaryVessels != null)
      {
        this.IsButtonEnabled = false;
        this.workingBitmap = this.ToGrayscaleConversion.ToGrayscale(this.workingBitmap, ToGrayscaleModes.Green);
        this.workingBitmap = new Merge(this.states[this.stateId].preliminaryVessels).Apply(this.workingBitmap);
        this.LastOperation = StateOperations.MergeWithPreliminaryVessels;
        this.ShowNextImage();
      }
      else
      {
        int num = (int) MessageBox.Show("Preliminary Vessels Data Not Found.");
      }
    }

    private void Binarize_Click(object sender, RoutedEventArgs e)
    {
      this.IsButtonEnabled = false;
      this.workingBitmap = this.Thresholding.ApplyThreshold(1, this.workingBitmap);
      this.LastOperation = StateOperations.Binarize;
      this.ShowNextImage();
    }

    private void ToGrayscale_Click(object sender, RoutedEventArgs e)
    {
      if (this.workingBitmap != null)
      {
        this.IsButtonEnabled = false;
        this.workingBitmap = this.ToGrayscaleConversion.ToGrayscale(this.workingBitmap, this.grayscaleMode);
        this.LastOperation = StateOperations.ConvertToGrayscale;
        this.ShowNextImage();
      }
      else
      {
        int num = (int) MessageBox.Show("Load Image First.");
      }
    }

    private void TopHatVesselEnhancement_Click(object sender, RoutedEventArgs e)
    {
      if (this.mask == null)
      {
        int num1 = (int) MessageBox.Show("Create Mask First.");
      }
      else
      {
        this.IsButtonEnabled = false;
        this.workingBitmap = AForge.Imaging.Image.Clone(this.workingBitmap, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        Bitmap source = new Invert().Apply(this.workingBitmap);
        if (this.structElem0 == null)
        {
          this.structElem0 = new short[21, 21];
          for (int index = 0; index < 21; ++index)
            this.structElem0[index, 10] = (short) 1;
        }
        if (this.angles == null)
        {
          this.angles = new double[12];
          for (int index = 1; index < this.angles.Length; ++index)
            this.angles[index] = this.angles[index - 1] + 15.0;
        }
        this.angularBitmaps = new Bitmap[12];
        for (int index = 0; index < 12; ++index)
          this.angularBitmaps[index] = AForge.Imaging.Image.Clone(source, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        this.states[this.stateId].topHats = new List<Bitmap>(12);
        for (int i = 0; i < 12; i++)
        {
          this.CalculateTopHatForAngle(i);
          Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Delegate) (() =>
          {
            this.states[this.stateId].topHats.Add(this.angularBitmaps[i]);
            this.TopHatImages[i].Source = RegionFeaturesView.getImageSourceFromBitmap(this.angularBitmaps[i]);
          }));
        }
        Bitmap bitmap = new Bitmap(source.Width, source.Height);
        int[,] numArray = new int[source.Width, source.Height];
        int num2 = 0;
        for (int i = 0; i < bitmap.Width; i++)
        {
          for (int j = 0; j < bitmap.Height; j++)
          {
            if (this.mask.GetPixel(i, j).G > (byte) 0)
            {
              numArray[i, j] = ((IEnumerable<Bitmap>) this.angularBitmaps).Sum<Bitmap>((Func<Bitmap, int>) (x => (int) x.GetPixel(i, j).G));
              if (numArray[i, j] > num2)
                num2 = numArray[i, j];
            }
          }
        }
        for (int x = 0; x < bitmap.Width; ++x)
        {
          for (int y = 0; y < bitmap.Height; ++y)
          {
            if (this.mask.GetPixel(x, y).G > (byte) 0)
            {
              int num3 = 5 * (numArray[x, y] * (int) byte.MaxValue / num2) / 2;
              if (num3 > (int) byte.MaxValue)
                num3 = (int) byte.MaxValue;
              bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb((int) byte.MaxValue, num3, num3, num3));
            }
            else
              bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb((int) byte.MaxValue, 0, 0, 0));
          }
        }
        this.states[this.stateId].topHat = bitmap;
        this.LastOperation = StateOperations.ApplyTopHatVesselEnhancement;
        this.ShowNextImage();
      }
    }

    private void CalculateTopHatForAngle(int id)
    {
      double angle = this.angles[id];
      short[,] se = new short[21, 21];
      double num1 = System.Math.Sin(System.Math.PI * angle / 180.0);
      double num2 = System.Math.Cos(System.Math.PI * angle / 180.0);
      int num3 = 10;
      for (int index1 = 0; index1 < 21; ++index1)
      {
        for (int index2 = 0; index2 < 21; ++index2)
        {
          int num4 = (int) System.Math.Round((double) (index1 - num3) * num2 - (double) (index2 - num3) * num1);
          int num5 = (int) System.Math.Round((double) (index1 - num3) * num1 + (double) (index2 - num3) * num2);
          int index3 = num4 + num3;
          int index4 = num5 + num3;
          if (index3 > 0 && index3 < 21 && index4 > 0 && index4 < 21)
            se[index3, index4] = this.structElem0[index1, index2];
        }
      }
      new AForge.Imaging.Filters.TopHat(se).ApplyInPlace(this.angularBitmaps[id]);
    }

    private void ApplyIsotropicUndecimatedWaveletTransform_Click(object sender, RoutedEventArgs e)
    {
      this.scalingCoefficients[0] = this.ToGrayscaleConversion.ToGrayscale(this.workingBitmap, ToGrayscaleModes.Average);
      for (int index1 = 0; index1 < 3; ++index1)
      {
        this.IsButtonEnabled = false;
        int num = (int) System.Math.Round(System.Math.Pow(2.0, (double) index1)) - 1;
        int length = 5 + num * 4;
        int[,] kernel = new int[length, length];
        int index2 = 0;
        for (int index3 = 0; index3 < length; ++index3)
        {
          for (int index4 = 0; index4 < length; ++index4)
          {
            if (index3 % (num + 1) == 0 && index4 % (num + 1) == 0)
            {
              kernel[index3, index4] = this.filterSequence[index2];
              ++index2;
            }
          }
        }
        Convolution convolution = new Convolution(kernel, 54);
        this.scalingCoefficients[index1 + 1] = convolution.Apply(this.scalingCoefficients[index1]);
        this.SaveImageFile(this.scalingCoefficients[index1 + 1], "ScalingCoeff" + (object) (index1 + 1), StateOperations.ApplyIsotropicUndecimatedWaveletTransform);
        Subtract subtract = new Subtract(this.scalingCoefficients[index1]);
        this.waveletCoefficient[index1 + 1] = subtract.Apply(this.scalingCoefficients[index1 + 1]);
        this.SaveImageFile(this.waveletCoefficient[index1 + 1], "WaveletCoeff" + (object) (index1 + 1), StateOperations.ApplyIsotropicUndecimatedWaveletTransform);
      }
      this.workingBitmap = AForge.Imaging.Image.Clone(new Closing().Apply(new Add(this.waveletCoefficient[2]).Apply(this.waveletCoefficient[3])), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      for (int x = 0; x < this.workingBitmap.Width; ++x)
      {
        for (int y = 0; y < this.workingBitmap.Height; ++y)
        {
          if (this.mask.GetPixel(x, y).G == (byte) 0)
            this.workingBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb((int) byte.MaxValue, 0, 0, 0));
        }
      }
      this.LastOperation = StateOperations.ApplyIsotropicUndecimatedWaveletTransform;
      this.ShowNextImage();
    }

    private void LoadImageFile_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif)|*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif|All files (*.*)|*.*";
      bool? nullable = openFileDialog.ShowDialog();
      if (!nullable.HasValue || !nullable.Value)
        return;
      this.ProcessedFileLabel = openFileDialog.FileName;
      this.imageName = Path.GetFileName(openFileDialog.FileName);
      this.path = Path.GetDirectoryName(openFileDialog.FileName);
      this.ClearStates();
      this.ClearView();
      this.workingBitmap = AForge.Imaging.Image.FromFile(openFileDialog.FileName);
      this.workingBitmap = RegionFeaturesView.ResizeImage(this.workingBitmap);
      this.CurrentImage.Source = RegionFeaturesView.getImageSourceFromBitmap(this.workingBitmap);
      this.IsMaskHandpicked = false;
      this.LastOperation = StateOperations.LoadImage;
      this.ShowNextImage();
    }

    private void LoadCompareImageFile_Click(object sender, RoutedEventArgs e)
    {
      if (this.workingBitmap != null)
      {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif)|*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif|All files (*.*)|*.*";
        bool? nullable = openFileDialog.ShowDialog();
        if (!nullable.HasValue || !nullable.Value)
          return;
        this.ComparedFileLabel = openFileDialog.FileName;
        this.compareBitmap = AForge.Imaging.Image.FromFile(openFileDialog.FileName);
        this.compareBitmap = RegionFeaturesView.ResizeImageToSize(this.compareBitmap, this.workingBitmap.Width, this.workingBitmap.Height);
        this.CompareImage.Source = RegionFeaturesView.getImageSourceFromBitmap(this.compareBitmap);
      }
      else
      {
        int num = (int) MessageBox.Show("Load Image To Process First.");
      }
    }

    private void LoadMaskImageFile_Click(object sender, RoutedEventArgs e)
    {
      if (this.workingBitmap != null)
      {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif)|*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif|All files (*.*)|*.*";
        bool? nullable = openFileDialog.ShowDialog();
        if (!nullable.HasValue || !nullable.Value)
          return;
        this.MaskFileLabel = openFileDialog.FileName;
        this.SetMask(RegionFeaturesView.ResizeImageToSize(AForge.Imaging.Image.FromFile(openFileDialog.FileName), this.workingBitmap.Width, this.workingBitmap.Height));
      }
      else
      {
        int num = (int) MessageBox.Show("Load Image To Process First.");
      }
    }

    private void SaveImageFile_Click(object sender, RoutedEventArgs e)
    {
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.Filter = "Tiff Image (.tif)|*.tif|Gif Image (.gif)|*.gif|JPEG Image (.jpg)|*.jpeg|Png Image (.png)|*.png";
      saveFileDialog.FileName = this.imageName.Substring(0, this.imageName.Length - 4) + "_results.tif";
      saveFileDialog.DefaultExt = "tif";
      ImageFormat format = ImageFormat.Tiff;
      bool? nullable = saveFileDialog.ShowDialog();
      if (!nullable.HasValue || !nullable.Value)
        return;
      string extension = Path.GetExtension(saveFileDialog.FileName);
      if (!(extension == ".jpg"))
      {
        if (!(extension == ".tif"))
        {
          if (extension == ".gif")
            format = ImageFormat.Gif;
        }
        else
          format = ImageFormat.Tiff;
      }
      else
        format = ImageFormat.Jpeg;
      this.workingBitmap.Save(saveFileDialog.FileName, format);
    }

    private void SaveResultsFile_Click(object sender, RoutedEventArgs e)
    {
      if (this.workingBitmap == null)
        return;
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";
      saveFileDialog.DefaultExt = ".txt";
      saveFileDialog.FileName = this.imageName.Substring(0, this.imageName.Length - 4) + "_results.txt";
      bool? nullable = saveFileDialog.ShowDialog();
      string fileName = saveFileDialog.FileName;
      if (nullable.HasValue && nullable.Value)
      {
        this.SaveResults(fileName);
      }
      else
      {
        int num = (int) MessageBox.Show("Error saving file.");
      }
    }

    private void SaveResults(string fileName)
    {
      StreamWriter streamWriter = new StreamWriter(fileName, false);
      streamWriter.WriteLine("Algorithm: Region Features");
      streamWriter.WriteLine("Image_File_Path: " + this.ProcessedFileLabel);
      if (!string.IsNullOrEmpty(this.MaskFileLabel))
        streamWriter.WriteLine("Mask_File_Name: " + this.MaskFileLabel);
      streamWriter.WriteLine("Terminations: " + (object) this.TerminationCount);
      streamWriter.WriteLine("Bifurcations: " + (object) this.BifurcationCount);
      streamWriter.WriteLine("Crossovers: " + (object) this.CrossoverCount);
      if (this.compareBitmap != null)
      {
        streamWriter.WriteLine("Compared_Image_File_Path: " + this.ComparedFileLabel);
        streamWriter.WriteLine("Accuracy: " + (object) this.Accuracy);
        streamWriter.WriteLine("Sensitivity(TPR): " + (object) this.TruePositiveRate);
        streamWriter.WriteLine("Specificity(TNR): " + (object) this.TrueNegativeRate);
        streamWriter.WriteLine("False_Positive_Rate(FPR): " + (object) this.FalsePositiveRate);
      }
      streamWriter.Close();
    }

    private void SaveResultsFile()
    {
      string str = Path.Combine(this.path, "RegionFeatures", Path.GetFileNameWithoutExtension(this.imageName));
      Directory.CreateDirectory(str);
      this.SaveResults(Path.Combine(str, "results.txt"));
    }

    private void SaveImageFile(Bitmap bitmap, string index, StateOperations operation)
    {
      string str = Path.Combine(this.path, "RegionFeatures", Path.GetFileNameWithoutExtension(this.imageName));
      Directory.CreateDirectory(str);
      bitmap.Save(Path.Combine(str, index + this.ToFriendlyCase(operation.ToString()) + ".tif"), ImageFormat.Tiff);
    }

    private void SaveMaskFile_Click(object sender, RoutedEventArgs e)
    {
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.Filter = "Tiff Image (.tif)|*.tif|Gif Image (.gif)|*.gif|JPEG Image (.jpg)|*.jpeg|Png Image (.png)|*.png";
      saveFileDialog.FileName = this.imageName.Substring(0, this.imageName.Length - 4) + "_mask.tif";
      saveFileDialog.DefaultExt = "tif";
      ImageFormat format = ImageFormat.Tiff;
      bool? nullable = saveFileDialog.ShowDialog();
      if (!nullable.HasValue || !nullable.Value)
        return;
      string extension = Path.GetExtension(saveFileDialog.FileName);
      if (!(extension == ".jpg"))
      {
        if (!(extension == ".tif"))
        {
          if (extension == ".gif")
            format = ImageFormat.Gif;
        }
        else
          format = ImageFormat.Tiff;
      }
      else
        format = ImageFormat.Jpeg;
      this.mask.Save(saveFileDialog.FileName, format);
    }

    public static Bitmap ResizeImage(Bitmap image)
    {
      double num = 800.0 / (double) image.Width;
      if (num >= 1.0)
        return image;
      Rectangle destRect = new Rectangle(0, 0, (int) ((double) image.Width * num), (int) ((double) image.Height * num));
      Bitmap bitmap = new Bitmap((int) ((double) image.Width * num), (int) ((double) image.Height * num));
      bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
      using (Graphics graphics = Graphics.FromImage((System.Drawing.Image) bitmap))
      {
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        using (ImageAttributes imageAttr = new ImageAttributes())
        {
          imageAttr.SetWrapMode(WrapMode.TileFlipXY);
          graphics.DrawImage((System.Drawing.Image) image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttr);
        }
      }
      return bitmap;
    }

    public static Bitmap ResizeImageToSize(Bitmap image, int width, int height)
    {
      Rectangle destRect = new Rectangle(0, 0, width, height);
      Bitmap bitmap = new Bitmap(width, height);
      bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
      using (Graphics graphics = Graphics.FromImage((System.Drawing.Image) bitmap))
      {
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        using (ImageAttributes imageAttr = new ImageAttributes())
        {
          imageAttr.SetWrapMode(WrapMode.TileFlipXY);
          graphics.DrawImage((System.Drawing.Image) image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttr);
        }
      }
      return bitmap;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public int MaxVesselLengthToRemove { get; set; } = 60;

    public int TerminationCount
    {
      get
      {
        return this.terminationCount;
      }
      set
      {
        this.terminationCount = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (TerminationCount)));
      }
    }

    public int BifurcationCount
    {
      get
      {
        return this.bifurcationCount;
      }
      set
      {
        this.bifurcationCount = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (BifurcationCount)));
      }
    }

    public int CrossoverCount
    {
      get
      {
        return this.crossoverCount;
      }
      set
      {
        this.crossoverCount = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (CrossoverCount)));
      }
    }

    public float Accuracy
    {
      get
      {
        return this.accuracy;
      }
      set
      {
        this.accuracy = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (Accuracy)));
      }
    }

    public float TruePositiveRate
    {
      get
      {
        return this.truePositiveRate;
      }
      set
      {
        this.truePositiveRate = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (TruePositiveRate)));
      }
    }

    public float TrueNegativeRate
    {
      get
      {
        return this.trueNegativeRate;
      }
      set
      {
        this.trueNegativeRate = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (TrueNegativeRate)));
      }
    }

    public float FalsePositiveRate
    {
      get
      {
        return this.falsePositiveRate;
      }
      set
      {
        this.falsePositiveRate = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (FalsePositiveRate)));
      }
    }

    public int MaskThreshold
    {
      get
      {
        return this.maskThreshold;
      }
      set
      {
        this.maskThreshold = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (MaskThreshold)));
        this.UpdateSliderIndicators();
      }
    }

    public bool IsButtonEnabled
    {
      get
      {
        return this.isButtonEnabled;
      }
      set
      {
        this.isButtonEnabled = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (IsButtonEnabled)));
      }
    }

    public PointCollection HistogramPoints
    {
      get
      {
        return this.histogramPoints;
      }
      set
      {
        this.histogramPoints = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (HistogramPoints)));
      }
    }

    public PointCollection SliderPoints
    {
      get
      {
        return this.sliderPoints;
      }
      set
      {
        this.sliderPoints = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (SliderPoints)));
      }
    }

    public string LastOperationLabel
    {
      get
      {
        return this.lastOperationLabel;
      }
      set
      {
        this.lastOperationLabel = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (LastOperationLabel)));
      }
    }

    public string ProcessedFileLabel
    {
      get
      {
        return this.processedFileLabel;
      }
      set
      {
        this.processedFileLabel = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (ProcessedFileLabel)));
      }
    }

    public string ComparedFileLabel
    {
      get
      {
        return this.comparedFileLabel;
      }
      set
      {
        this.comparedFileLabel = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (ComparedFileLabel)));
      }
    }

    public string MaskFileLabel
    {
      get
      {
        return this.maskFileLabel;
      }
      set
      {
        this.maskFileLabel = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (MaskFileLabel)));
      }
    }

    public StateOperations LastOperation
    {
      get
      {
        return this.lastOperation;
      }
      set
      {
        this.lastOperation = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (LastOperation)));
        this.LastOperationLabel = this.ToFriendlyCase(this.LastOperation.ToString());
      }
    }

    public RegionFeaturesView()
    {
      this.InitializeComponent();
      this.DataContext = (object) this;
      this.TopHatImages = new List<System.Windows.Controls.Image>()
      {
        this.TopHatImage1,
        this.TopHatImage2,
        this.TopHatImage3,
        this.TopHatImage4,
        this.TopHatImage5,
        this.TopHatImage6,
        this.TopHatImage7,
        this.TopHatImage8,
        this.TopHatImage9,
        this.TopHatImage10,
        this.TopHatImage11,
        this.TopHatImage12
      };
      this.states = new List<StateRegionFeatures>(16);
      this.stateId = -1;
    }

    public void Undo_Click(object sender, RoutedEventArgs e)
    {
      this.UndoState();
      e.Handled = true;
    }

    public void Redo_Click(object sender, RoutedEventArgs e)
    {
      this.RedoState();
      e.Handled = true;
    }

    public void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = this.stateId < this.states.Count - 1 && this.IsButtonEnabled;
    }

    public void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = this.stateId > 0 && this.IsButtonEnabled;
    }

    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    private static ImageSource getImageSourceFromBitmap(Bitmap bitmap)
    {
      IntPtr hbitmap = bitmap.GetHbitmap();
      try
      {
        BitmapSource sourceFromHbitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        if (sourceFromHbitmap.CanFreeze)
          sourceFromHbitmap.Freeze();
        return (ImageSource) sourceFromHbitmap;
      }
      finally
      {
        RegionFeaturesView.DeleteObject(hbitmap);
      }
    }

    private void ClearStates()
    {
      this.states.Clear();
      this.stateId = -1;
    }

    private void ClearView()
    {
      this.CurrentImage.Source = (ImageSource) null;
      this.PreliminaryVesselImage.Source = (ImageSource) null;
      this.UndeterminedRegionsImage.Source = (ImageSource) null;
      this.TopHat.Source = (ImageSource) null;
      this.MaskImage.Source = (ImageSource) null;
      this.PreviousImage1.Source = (ImageSource) null;
      this.PreviousImage2.Source = (ImageSource) null;
      foreach (System.Windows.Controls.Image topHatImage in this.TopHatImages)
        topHatImage.Source = (ImageSource) null;
      this.HistogramPoints = (PointCollection) null;
    }

    private void ShowNextMask()
    {
      StateRegionFeatures stateRegionFeatures;
      if (this.stateId != -1)
        stateRegionFeatures = new StateRegionFeatures(this.states[this.stateId], true)
        {
          maskState = this.mask,
          lastOperation = this.LastOperation
        };
      else
        stateRegionFeatures = new StateRegionFeatures()
        {
          maskState = this.mask,
          lastOperation = this.LastOperation
        };
      if (this.stateId < this.states.Count - 1)
        this.states.RemoveRange(this.stateId + 1, this.states.Count - 1 - this.stateId);
      this.states.Add(stateRegionFeatures);
      ++this.stateId;
      this.UpdateView();
    }

    private void ShowNextImage()
    {
      StateRegionFeatures stateRegionFeatures;
      if (this.stateId != -1)
        stateRegionFeatures = new StateRegionFeatures(this.states[this.stateId], false)
        {
          current = this.workingBitmap,
          lastOperation = this.LastOperation
        };
      else
        stateRegionFeatures = new StateRegionFeatures()
        {
          current = this.workingBitmap,
          lastOperation = this.LastOperation
        };
      if (this.stateId < this.states.Count - 1)
        this.states.RemoveRange(this.stateId + 1, this.states.Count - 1 - this.stateId);
      this.states.Add(stateRegionFeatures);
      ++this.stateId;
      this.UpdateView();
    }

    private void UpdateView()
    {
      StateRegionFeatures state = this.states[this.stateId];
      Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Delegate) (() =>
      {
        this.ClearView();
        this.LastOperation = state.lastOperation;
        if (state.previous1 != null)
          this.PreviousImage1.Source = RegionFeaturesView.getImageSourceFromBitmap(state.previous1);
        if (state.previous2 != null)
          this.PreviousImage2.Source = RegionFeaturesView.getImageSourceFromBitmap(state.previous2);
        if (state.maskState != null)
        {
          this.mask = state.maskState;
          this.MaskImage.Source = RegionFeaturesView.getImageSourceFromBitmap(state.maskState);
        }
        if (state.preliminaryVessels != null)
          this.PreliminaryVesselImage.Source = RegionFeaturesView.getImageSourceFromBitmap(state.preliminaryVessels);
        if (state.undeterminedRegions != null)
          this.UndeterminedRegionsImage.Source = RegionFeaturesView.getImageSourceFromBitmap(state.undeterminedRegions);
        if (state.topHat != null)
          this.TopHat.Source = RegionFeaturesView.getImageSourceFromBitmap(state.topHat);
        if (state.topHats != null && (uint) state.topHats.Count > 0U)
        {
          for (int index = 0; index < 12; ++index)
            this.TopHatImages[index].Source = RegionFeaturesView.getImageSourceFromBitmap(state.topHats[index]);
        }
        if (state.current != null)
        {
          this.CurrentImage.Source = RegionFeaturesView.getImageSourceFromBitmap(state.current);
          this.workingBitmap = state.current;
          if (state.maskState != null)
            this.UpdateHistogram(state.current, state.maskState);
          else
            this.UpdateHistogram(state.current, (Bitmap) null);
        }
        this.IsButtonEnabled = true;
      }));
      GC.Collect();
    }

    private void UpdateHistogram(Bitmap bitmap, Bitmap mask = null)
    {
      this.UpdateSliderIndicators();
      ImageStatistics imageStatistics = mask == null ? new ImageStatistics(bitmap) : new ImageStatistics(bitmap, mask);
      int[] numArray = !imageStatistics.IsGrayscale ? imageStatistics.Green.Values : imageStatistics.Gray.Values;
      int num = ((IEnumerable<int>) numArray).Max();
      PointCollection pointCollection = new PointCollection();
      pointCollection.Add(new System.Windows.Point(0.0, (double) num));
      for (int index = 0; index < numArray.Length; ++index)
        pointCollection.Add(new System.Windows.Point((double) index, (double) (num - numArray[index])));
      pointCollection.Add(new System.Windows.Point((double) (numArray.Length - 1), (double) num));
      this.HistogramPoints = pointCollection;
    }

    private void UpdateSliderIndicators()
    {
      int num = 25;
      PointCollection pointCollection = new PointCollection();
      pointCollection.Add(new System.Windows.Point(0.0, (double) num));
      for (int index = 0; index < 256; ++index)
      {
        if (index == this.MaskThreshold)
          pointCollection.Add(new System.Windows.Point((double) index, (double) (num / 2)));
        else
          pointCollection.Add(new System.Windows.Point((double) index, (double) num));
      }
      pointCollection.Add(new System.Windows.Point(256.0, (double) num));
      this.SliderPoints = pointCollection;
    }

    private void RedoState()
    {
      ++this.stateId;
      this.UpdateView();
    }

    private void UndoState()
    {
      --this.stateId;
      this.UpdateView();
    }

    private void AllUpToExtractMinutiae_Click(object sender, RoutedEventArgs e)
    {
      if (this.workingBitmap != null)
      {
        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Delegate) (() =>
        {
          this.ToGrayscale_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "0", StateOperations.ConvertToGrayscale);
          this.MedianFilter_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "1", StateOperations.RemoveNoise);
          if (!this.IsMaskHandpicked)
          {
            this.FindMaskBinarizationThreshold_Click(sender, e);
            this.CreateMask_Click(sender, e);
            this.SaveImageFile(this.mask, "", StateOperations.CreateMask);
          }
          this.TopHatVesselEnhancement_Click(sender, e);
          this.SaveImageFile(this.states[this.stateId].topHat, "2", StateOperations.ApplyTopHatVesselEnhancement);
          this.ImageDivision_Click(sender, e);
          this.SaveImageFile(this.states[this.stateId].preliminaryVessels, "3", StateOperations.DivideImage);
          this.DenoisePreliminaryVessels_Click(sender, e);
          this.SaveImageFile(this.states[this.stateId].preliminaryVessels, "4", StateOperations.DenoisePreliminaryVessels);
          this.ApplyIsotropicUndecimatedWaveletTransform_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "5", StateOperations.ApplyIsotropicUndecimatedWaveletTransform);
          this.Binarize_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "6", StateOperations.Binarize);
          this.ExtractWaveletSkeleton_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "7", StateOperations.ExtractSkeleton);
          this.MergeWithPreliminaryVessels_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "8", StateOperations.MergeWithPreliminaryVessels);
          this.ApplyHierarchicalGrowth_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "9", StateOperations.ApplyHierarchicalGrowth);
          this.RemoveNonVesselRegions_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "10", StateOperations.RemoveNonVesselRegions);
          if (this.compareBitmap != null)
            this.CompareImages_Click(sender, e);
          this.K3MThinning_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "11", StateOperations.Skeletonize);
          this.RemoveLooseVessels_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "12", StateOperations.RemoveLooseVessels);
          this.ExtractMinutiae_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "13", StateOperations.ExtractMinutiae);
          this.SaveResultsFile();
          CommandManager.InvalidateRequerySuggested();
        }));
      }
      else
      {
        int num = (int) MessageBox.Show("Load Image First.");
      }
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
      Regex regex = new Regex("[^0-9]+");
      e.Handled = regex.IsMatch(e.Text);
    }

    private string ToFriendlyCase(string EnumString)
    {
      return Regex.Replace(EnumString, "(?!^)([A-Z])", " $1");
    }

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
      this.grayscaleMode = (ToGrayscaleModes) Enum.Parse(typeof (ToGrayscaleModes), ((ContentControl) sender).Content.ToString());
    }

    private void GrayscaleRadioButtonGreen_Loaded(object sender, RoutedEventArgs e)
    {
      ((ToggleButton) sender).IsChecked = new bool?(true);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/RetinaImageProcessor.UI;component/logic/regionfeatures/view/regionfeaturesview.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((MenuItem) target).Click += new RoutedEventHandler(this.LoadImageFile_Click);
          break;
        case 2:
          ((MenuItem) target).Click += new RoutedEventHandler(this.LoadMaskImageFile_Click);
          break;
        case 3:
          ((MenuItem) target).Click += new RoutedEventHandler(this.LoadCompareImageFile_Click);
          break;
        case 4:
          ((MenuItem) target).Click += new RoutedEventHandler(this.SaveImageFile_Click);
          break;
        case 5:
          ((MenuItem) target).Click += new RoutedEventHandler(this.SaveResultsFile_Click);
          break;
        case 6:
          ((MenuItem) target).Click += new RoutedEventHandler(this.SaveMaskFile_Click);
          break;
        case 7:
          ((MenuItem) target).Click += new RoutedEventHandler(this.AllUpToExtractMinutiae_Click);
          break;
        case 8:
          ((ToggleButton) target).Checked += new RoutedEventHandler(this.RadioButton_Checked);
          break;
        case 9:
          ((ToggleButton) target).Checked += new RoutedEventHandler(this.RadioButton_Checked);
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.GrayscaleRadioButtonGreen_Loaded);
          break;
        case 10:
          ((ToggleButton) target).Checked += new RoutedEventHandler(this.RadioButton_Checked);
          break;
        case 11:
          ((ToggleButton) target).Checked += new RoutedEventHandler(this.RadioButton_Checked);
          break;
        case 12:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.ToGrayscale_Click);
          break;
        case 13:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.MedianFilter_Click);
          break;
        case 14:
          this.maskSlider = (Slider) target;
          break;
        case 15:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.FindMaskBinarizationThreshold_Click);
          break;
        case 16:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.CreateMask_Click);
          break;
        case 17:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.TopHatVesselEnhancement_Click);
          break;
        case 18:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.ImageDivision_Click);
          break;
        case 19:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.DenoisePreliminaryVessels_Click);
          break;
        case 20:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.ApplyIsotropicUndecimatedWaveletTransform_Click);
          break;
        case 21:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.Binarize_Click);
          break;
        case 22:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.ExtractWaveletSkeleton_Click);
          break;
        case 23:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.MergeWithPreliminaryVessels_Click);
          break;
        case 24:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.ApplyHierarchicalGrowth_Click);
          break;
        case 25:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.RemoveNonVesselRegions_Click);
          break;
        case 26:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.CompareImages_Click);
          break;
        case 27:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.K3MThinning_Click);
          break;
        case 28:
          this.RemoveVesselTextBox = (TextBox) target;
          this.RemoveVesselTextBox.PreviewTextInput += new TextCompositionEventHandler(this.NumberValidationTextBox);
          break;
        case 29:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.RemoveLooseVessels_Click);
          break;
        case 30:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.ExtractMinutiae_Click);
          break;
        case 31:
          this.PreviousImage1 = (System.Windows.Controls.Image) target;
          break;
        case 32:
          this.PreviousImage2 = (System.Windows.Controls.Image) target;
          break;
        case 33:
          this.TopHatImage1 = (System.Windows.Controls.Image) target;
          break;
        case 34:
          this.TopHatImage2 = (System.Windows.Controls.Image) target;
          break;
        case 35:
          this.TopHatImage3 = (System.Windows.Controls.Image) target;
          break;
        case 36:
          this.TopHatImage4 = (System.Windows.Controls.Image) target;
          break;
        case 37:
          this.TopHatImage5 = (System.Windows.Controls.Image) target;
          break;
        case 38:
          this.TopHatImage6 = (System.Windows.Controls.Image) target;
          break;
        case 39:
          this.TopHatImage7 = (System.Windows.Controls.Image) target;
          break;
        case 40:
          this.TopHatImage8 = (System.Windows.Controls.Image) target;
          break;
        case 41:
          this.TopHatImage9 = (System.Windows.Controls.Image) target;
          break;
        case 42:
          this.TopHatImage10 = (System.Windows.Controls.Image) target;
          break;
        case 43:
          this.TopHatImage11 = (System.Windows.Controls.Image) target;
          break;
        case 44:
          this.TopHatImage12 = (System.Windows.Controls.Image) target;
          break;
        case 45:
          this.TopHat = (System.Windows.Controls.Image) target;
          break;
        case 46:
          this.PreliminaryVesselImage = (System.Windows.Controls.Image) target;
          break;
        case 47:
          this.UndeterminedRegionsImage = (System.Windows.Controls.Image) target;
          break;
        case 48:
          this.CurrentImage = (System.Windows.Controls.Image) target;
          break;
        case 49:
          this.CompareImage = (System.Windows.Controls.Image) target;
          break;
        case 50:
          this.MaskImage = (System.Windows.Controls.Image) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
