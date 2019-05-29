// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.MatchedFilters.View.MatchedFiltersView
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
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

namespace RetinaImageProcessor.UI.Logic.MatchedFilters.View
{
  public partial class MatchedFiltersView : UserControl, IStatefulView, INotifyPropertyChanged, IComponentConnector
  {
    private ImageComparison ImageComparison = ImageComparison.Instance;
    private List<IntPoint> strongTerminations = new List<IntPoint>();
    private List<IntPoint> weakTerminations = new List<IntPoint>();
    private List<IntPoint> bifurcations = new List<IntPoint>();
    private List<IntPoint> crossovers = new List<IntPoint>();
    private Invert invert = new Invert();
    private FeatureExtraction FeatureExtraction = FeatureExtraction.Instance;
    private MaskCreation MaskCreation = MaskCreation.Instance;
    private ToGrayscaleConversion ToGrayscaleConversion = ToGrayscaleConversion.Instance;
    private ToGrayscaleModes grayscaleMode = ToGrayscaleModes.Green;
    private int maskThreshold = 20;
    private int brightnessValue = -40;
    private double contrastThresholdBefore = 40.0;
    private double contrastThresholdAfter = 15.0;
    private int binarizationThreshold = 50;
    private int medianWindowSize = 3;
    private float truePositiveRate = 0.0f;
    private float trueNegativeRate = 0.0f;
    private float falsePositiveRate = 0.0f;
    private float accuracy = 0.0f;
    private bool isButtonEnabled = true;
    private NoiseRemoval NoiseRemoval = NoiseRemoval.Instance;
    private Thinning Thinning = Thinning.Instance;
    private Thresholding Thresholding = Thresholding.Instance;
    private Bitmap mask;
    private string imageName;
    private string path;
    private List<StateMatchedFilters> states;
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
    private int[,] kernel;
    private Bitmap[] angularBitmaps;
    private double[] angles;
    private List<System.Windows.Controls.Image> GaussianImages;
    internal Slider maskSlider;
    internal Slider brightnessSlider;
    internal Slider constrastBeforeSlider;
    internal Slider constrastAfterSlider;
    internal Slider binarizationSlider;
    internal TextBox RemoveVesselTextBox;
    internal System.Windows.Controls.Image PreviousImage1;
    internal System.Windows.Controls.Image PreviousImage2;
    internal System.Windows.Controls.Image MaskImage;
    internal System.Windows.Controls.Image CoOccurenceImage;
    internal System.Windows.Controls.Image CurrentImage;
    internal System.Windows.Controls.Image CompareImage;
    internal System.Windows.Controls.Image GaussianImage1;
    internal System.Windows.Controls.Image GaussianImage2;
    internal System.Windows.Controls.Image GaussianImage3;
    internal System.Windows.Controls.Image GaussianImage4;
    internal System.Windows.Controls.Image GaussianImage5;
    internal System.Windows.Controls.Image GaussianImage6;
    internal System.Windows.Controls.Image GaussianImage7;
    internal System.Windows.Controls.Image GaussianImage8;
    internal System.Windows.Controls.Image GaussianImage9;
    internal System.Windows.Controls.Image GaussianImage10;
    internal System.Windows.Controls.Image GaussianImage11;
    internal System.Windows.Controls.Image GaussianImage12;
    private bool _contentLoaded;

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

    private void ExtractMinutiae_Click(object sender, RoutedEventArgs e)
    {
      this.IsButtonEnabled = false;
      this.weakTerminations.Clear();
      this.strongTerminations.Clear();
      this.bifurcations.Clear();
      this.crossovers.Clear();
      Bitmap minutiae = this.FeatureExtraction.ExtractMinutiae(this.workingBitmap, this.strongTerminations, this.weakTerminations, this.bifurcations, this.crossovers);
      this.invert.ApplyInPlace(minutiae);
      this.FeatureExtraction.MarkMinutiae(minutiae, this.strongTerminations, this.weakTerminations, this.bifurcations, this.crossovers);
      this.workingBitmap = minutiae;
      this.TerminationCount = this.strongTerminations.Count;
      this.BifurcationCount = this.bifurcations.Count;
      this.CrossoverCount = this.crossovers.Count;
      this.LastOperation = StateOperations.ExtractMinutiae;
      this.ShowNextImage();
    }

    public bool IsMaskHandpicked { get; set; } = false;

    private void CreateMask_Click(object sender, RoutedEventArgs e)
    {
      this.IsButtonEnabled = false;
      this.mask = this.Thresholding.ApplyThresholdMask(this.MaskThreshold, this.workingBitmap);
      this.workingBitmap = this.MaskCreation.ApplyMask(this.workingBitmap, this.mask);
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
      this.workingBitmap = MatchedFiltersView.ResizeImage(this.workingBitmap);
      this.CurrentImage.Source = MatchedFiltersView.getImageSourceFromBitmap(this.workingBitmap);
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
        this.compareBitmap = MatchedFiltersView.ResizeImageToSize(this.compareBitmap, this.workingBitmap.Width, this.workingBitmap.Height);
        this.CompareImage.Source = MatchedFiltersView.getImageSourceFromBitmap(this.compareBitmap);
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
        this.SetMask(MatchedFiltersView.ResizeImageToSize(AForge.Imaging.Image.FromFile(openFileDialog.FileName), this.workingBitmap.Width, this.workingBitmap.Height));
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
      streamWriter.WriteLine("Algorithm: Matched Filters");
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
      string str = Path.Combine(this.path, "MatchedFilters", Path.GetFileNameWithoutExtension(this.imageName));
      Directory.CreateDirectory(str);
      this.SaveResults(Path.Combine(str, "results.txt"));
    }

    private void SaveImageFile(Bitmap bitmap, string index, StateOperations operation)
    {
      string str = Path.Combine(this.path, "MatchedFilters", Path.GetFileNameWithoutExtension(this.imageName));
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

    public int BrightnessValue
    {
      get
      {
        return this.brightnessValue;
      }
      set
      {
        this.brightnessValue = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (BrightnessValue)));
        this.UpdateSliderIndicators();
      }
    }

    public double ContrastThresholdBefore
    {
      get
      {
        return this.contrastThresholdBefore;
      }
      set
      {
        this.contrastThresholdBefore = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (ContrastThresholdBefore)));
        this.UpdateSliderIndicators();
      }
    }

    public double ContrastThresholdAfter
    {
      get
      {
        return this.contrastThresholdAfter;
      }
      set
      {
        this.contrastThresholdAfter = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (ContrastThresholdAfter)));
        this.UpdateSliderIndicators();
      }
    }

    public int MedianWindowSize
    {
      get
      {
        return this.medianWindowSize;
      }
      set
      {
        this.medianWindowSize = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (MedianWindowSize)));
      }
    }

    public int BinarizationThreshold
    {
      get
      {
        return this.binarizationThreshold;
      }
      set
      {
        this.binarizationThreshold = value;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (BinarizationThreshold)));
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

    public MatchedFiltersView()
    {
      this.InitializeComponent();
      this.DataContext = (object) this;
      this.CurrentImage.Source = MatchedFiltersView.getImageSourceFromBitmap(new Bitmap(500, 500));
      this.GaussianImages = new List<System.Windows.Controls.Image>()
      {
        this.GaussianImage1,
        this.GaussianImage2,
        this.GaussianImage3,
        this.GaussianImage4,
        this.GaussianImage5,
        this.GaussianImage6,
        this.GaussianImage7,
        this.GaussianImage8,
        this.GaussianImage9,
        this.GaussianImage10,
        this.GaussianImage11,
        this.GaussianImage12
      };
      this.states = new List<StateMatchedFilters>(16);
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
        MatchedFiltersView.DeleteObject(hbitmap);
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
      this.CoOccurenceImage.Source = (ImageSource) null;
      this.MaskImage.Source = (ImageSource) null;
      this.PreviousImage1.Source = (ImageSource) null;
      this.PreviousImage2.Source = (ImageSource) null;
      foreach (System.Windows.Controls.Image gaussianImage in this.GaussianImages)
        gaussianImage.Source = (ImageSource) null;
      this.HistogramPoints = (PointCollection) null;
    }

    private void ShowNextMask()
    {
      StateMatchedFilters stateMatchedFilters;
      if (this.stateId != -1)
        stateMatchedFilters = new StateMatchedFilters(this.states[this.stateId], true)
        {
          maskState = this.mask,
          lastOperation = this.LastOperation
        };
      else
        stateMatchedFilters = new StateMatchedFilters()
        {
          maskState = this.mask,
          lastOperation = this.LastOperation
        };
      if (this.stateId < this.states.Count - 1)
        this.states.RemoveRange(this.stateId + 1, this.states.Count - 1 - this.stateId);
      this.states.Add(stateMatchedFilters);
      ++this.stateId;
      this.UpdateView();
    }

    private void ShowNextImage()
    {
      StateMatchedFilters stateMatchedFilters;
      if (this.stateId != -1)
        stateMatchedFilters = new StateMatchedFilters(this.states[this.stateId], false)
        {
          current = this.workingBitmap,
          lastOperation = this.LastOperation
        };
      else
        stateMatchedFilters = new StateMatchedFilters()
        {
          current = this.workingBitmap,
          lastOperation = this.LastOperation
        };
      if (this.stateId < this.states.Count - 1)
        this.states.RemoveRange(this.stateId + 1, this.states.Count - 1 - this.stateId);
      this.states.Add(stateMatchedFilters);
      ++this.stateId;
      this.UpdateView();
    }

    private void UpdateView()
    {
      StateMatchedFilters state = this.states[this.stateId];
      Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Delegate) (() =>
      {
        this.ClearView();
        this.LastOperation = state.lastOperation;
        if (state.previous1 != null)
          this.PreviousImage1.Source = MatchedFiltersView.getImageSourceFromBitmap(state.previous1);
        if (state.previous2 != null)
          this.PreviousImage2.Source = MatchedFiltersView.getImageSourceFromBitmap(state.previous2);
        if (state.maskState != null)
        {
          this.mask = state.maskState;
          this.MaskImage.Source = MatchedFiltersView.getImageSourceFromBitmap(state.maskState);
        }
        if (state.coOccurence != null)
          this.CoOccurenceImage.Source = MatchedFiltersView.getImageSourceFromBitmap(state.coOccurence);
        if (state.gaussians != null && (uint) state.gaussians.Count > 0U)
        {
          for (int index = 0; index < 12; ++index)
            this.GaussianImages[index].Source = MatchedFiltersView.getImageSourceFromBitmap(state.gaussians[index]);
        }
        if (state.current != null)
        {
          this.CurrentImage.Source = MatchedFiltersView.getImageSourceFromBitmap(state.current);
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
        if (index == this.MaskThreshold || index == -this.BrightnessValue || (index == 256 - this.BrightnessValue || index == this.BinarizationThreshold) || (index == (int) this.ContrastThresholdBefore || index == (int) this.ContrastThresholdAfter || index == (int) byte.MaxValue - (int) this.ContrastThresholdBefore) || index == (int) byte.MaxValue - (int) this.ContrastThresholdAfter)
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
          this.HistogramEqualization_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "2", StateOperations.EqualizeHistogram);
          this.BrightnessCorrection_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "3", StateOperations.CorrectBrightness);
          this.EnhanceContrastBeforeSegmentation_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "4", StateOperations.EnhanceContrastBeforeSegmentation);
          this.GaussianMatchedFilter_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "5", StateOperations.SegmentVessels);
          this.EnhanceContrastAfterSegmentation_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "6", StateOperations.EnhanceContrastAfterSegmentation);
          this.FindEntropyBinarizationThreshold_Click(sender, e);
          this.Binarization_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "7", StateOperations.Binarize);
          this.RemoveLooseVessels_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "8", StateOperations.RemoveLooseVessels);
          if (this.compareBitmap != null)
            this.CompareImages_Click(sender, e);
          this.K3MThinning_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "9", StateOperations.Skeletonize);
          this.ExtractMinutiae_Click(sender, e);
          this.SaveImageFile(this.workingBitmap, "10", StateOperations.ExtractMinutiae);
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

    private void BrightnessCorrection_Click(object sender, RoutedEventArgs e)
    {
      if (this.mask == null)
      {
        int num1 = (int) MessageBox.Show("Create Mask First.");
      }
      else
      {
        this.IsButtonEnabled = false;
        Bitmap maskedImage = AForge.Imaging.Image.Clone(this.workingBitmap, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        for (int x = 0; x < this.workingBitmap.Width; ++x)
        {
          for (int y = 0; y < this.workingBitmap.Height; ++y)
          {
            int num2 = (int) this.workingBitmap.GetPixel(x, y).G + this.BrightnessValue;
            if (num2 > (int) byte.MaxValue)
              maskedImage.SetPixel(x, y, System.Drawing.Color.White);
            else if (num2 < 0)
              maskedImage.SetPixel(x, y, System.Drawing.Color.Black);
            else
              maskedImage.SetPixel(x, y, System.Drawing.Color.FromArgb(num2, num2, num2));
          }
        }
        this.workingBitmap = this.MaskCreation.ApplyMask(maskedImage, this.mask);
        this.LastOperation = StateOperations.CorrectBrightness;
        this.ShowNextImage();
      }
    }

    private void HistogramEqualization_Click(object sender, RoutedEventArgs e)
    {
      if (this.mask == null)
      {
        int num = (int) MessageBox.Show("Create Mask First.");
      }
      else
      {
        this.IsButtonEnabled = false;
        this.workingBitmap = new HistogramEqualization().Apply(this.workingBitmap);
        this.workingBitmap = this.MaskCreation.ApplyMask(this.workingBitmap, this.mask);
        this.LastOperation = StateOperations.EqualizeHistogram;
        this.ShowNextImage();
      }
    }

    private void EnhanceContrastBeforeSegmentation_Click(object sender, RoutedEventArgs e)
    {
      if (this.mask == null)
      {
        int num = (int) MessageBox.Show("Create Mask First.");
      }
      else
      {
        this.LastOperation = StateOperations.EnhanceContrastBeforeSegmentation;
        this.EnhanceContrast(this.ContrastThresholdBefore);
      }
    }

    private void EnhanceContrastAfterSegmentation_Click(object sender, RoutedEventArgs e)
    {
      if (this.mask == null)
      {
        int num = (int) MessageBox.Show("Create Mask First.");
      }
      else
      {
        this.LastOperation = StateOperations.EnhanceContrastAfterSegmentation;
        this.EnhanceContrast(this.ContrastThresholdAfter);
      }
    }

    private void EnhanceContrast(double threshold)
    {
      this.IsButtonEnabled = false;
      int[] numArray = new int[256];
      for (int index = 0; index < 256; ++index)
      {
        numArray[index] = (int) ((double) sbyte.MaxValue + (double) ((index - (int) sbyte.MaxValue) * (int) byte.MaxValue) / ((double) byte.MaxValue - 2.0 * threshold) + 0.5);
        if (numArray[index] > (int) byte.MaxValue)
          numArray[index] = (int) byte.MaxValue;
        else if (numArray[index] < 0)
          numArray[index] = 0;
      }
      Bitmap bitmap = new Bitmap(this.workingBitmap.Width, this.workingBitmap.Height);
      for (int x = 0; x < bitmap.Width; ++x)
      {
        for (int y = 0; y < bitmap.Height; ++y)
        {
          int num = numArray[(int) this.workingBitmap.GetPixel(x, y).G];
          bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb((int) byte.MaxValue, num, num, num));
        }
      }
      this.workingBitmap = bitmap;
      this.workingBitmap = this.MaskCreation.ApplyMask(this.workingBitmap, this.mask);
      this.ShowNextImage();
    }

    private void MedianFilter_Click(object sender, RoutedEventArgs e)
    {
      this.IsButtonEnabled = false;
      this.workingBitmap = this.NoiseRemoval.MedianFilter(3, 1, this.workingBitmap);
      this.LastOperation = StateOperations.RemoveNoise;
      this.ShowNextImage();
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

    private void Binarization_Click(object sender, RoutedEventArgs e)
    {
      this.IsButtonEnabled = false;
      this.workingBitmap = this.Thresholding.ApplyThreshold(this.BinarizationThreshold, this.workingBitmap);
      this.LastOperation = StateOperations.Binarize;
      this.ShowNextImage();
    }

    private void FindEntropyBinarizationThreshold_Click(object sender, RoutedEventArgs e)
    {
      this.BinarizationThreshold = this.FindEntropyBinarizationThreshold();
    }

    private int FindEntropyBinarizationThreshold()
    {
      int[,] numArray1 = new int[256, 256];
      for (int x = 0; x < this.workingBitmap.Width; ++x)
      {
        for (int y = 0; y < this.workingBitmap.Height; ++y)
        {
          System.Drawing.Color pixel = this.workingBitmap.GetPixel(x, y);
          int g1 = (int) pixel.G;
          int index = -1;
          if (y + 1 < this.workingBitmap.Height)
          {
            pixel = this.workingBitmap.GetPixel(x, y + 1);
            index = (int) pixel.G;
            ++numArray1[g1, index];
          }
          if (x + 1 < this.workingBitmap.Width)
          {
            pixel = this.workingBitmap.GetPixel(x + 1, y);
            int g2 = (int) pixel.G;
            if (index != g2)
              ++numArray1[g1, g2];
          }
        }
      }
      Bitmap matrix = new Bitmap(256, 256);
      for (int x = 1; x < matrix.Width; ++x)
      {
        for (int y = 1; y < matrix.Height; ++y)
        {
          if (numArray1[x, y] > 0)
            matrix.SetPixel(x, y, System.Drawing.Color.White);
          else
            matrix.SetPixel(x, y, System.Drawing.Color.Black);
        }
      }
      Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Delegate) (() => this.states[this.stateId].coOccurence = matrix));
      int[] numArray2 = new int[256];
      numArray2[0] = numArray1[0, 0];
      for (int index1 = 1; index1 < 256; ++index1)
      {
        numArray2[index1] = numArray2[index1 - 1] + numArray1[index1, index1];
        for (int index2 = 0; index2 < index1; ++index2)
          numArray2[index1] += numArray1[index1, index2] + numArray1[index2, index1];
      }
      int[] numArray3 = new int[256];
      numArray3[(int) byte.MaxValue] = numArray1[(int) byte.MaxValue, (int) byte.MaxValue];
      for (int index = 254; index >= 0; --index)
      {
        numArray3[index] = numArray3[index + 1] + numArray1[index, index];
        for (int maxValue = (int) byte.MaxValue; maxValue > index; --maxValue)
          numArray3[index] += numArray1[index, maxValue] + numArray1[maxValue, index];
      }
      double[] numArray4 = new double[256];
      for (int index1 = 0; index1 < 256; ++index1)
      {
        double num = 0.0;
        for (int index2 = 0; index2 < index1; ++index2)
        {
          for (int index3 = 0; index3 < index1; ++index3)
          {
            double a = (double) numArray1[index2, index3] / (double) numArray2[index1];
            if (a != 0.0)
              num += a * Math.Log(a, 2.0);
          }
        }
        numArray4[index1] = -0.5 * num;
      }
      double[] numArray5 = new double[256];
      for (int index1 = 0; index1 < 256; ++index1)
      {
        double num1 = 0.0;
        for (int index2 = index1 + 1; index2 < 256; ++index2)
        {
          for (int index3 = index1 + 1; index3 < 256; ++index3)
          {
            if ((uint) numArray3[index1] > 0U)
            {
              double a = (double) numArray1[index2, index3] / (double) numArray3[index1];
              if (a != 0.0)
              {
                double num2 = a * Math.Log(a, 2.0);
                num1 += num2;
              }
            }
          }
        }
        numArray5[index1] = -0.5 * num1;
      }
      int num3 = 0;
      double num4 = 0.0;
      for (int index = 0; index < 256; ++index)
      {
        if (numArray4[index] + numArray5[index] > num4)
        {
          num4 = numArray4[index] + numArray5[index];
          num3 = index;
        }
      }
      return num3;
    }

    private void GaussianMatchedFilter_Click(object sender, RoutedEventArgs e)
    {
      if (this.mask == null)
      {
        int num1 = (int) MessageBox.Show("Create Mask First.");
      }
      else
      {
        this.IsButtonEnabled = false;
        if (this.kernel == null)
          this.CalculateFirstKernel();
        if (this.angles == null)
        {
          this.angles = new double[12];
          for (int index = 1; index < this.angles.Length; ++index)
            this.angles[index] = this.angles[index - 1] + 15.0;
        }
        this.angularBitmaps = new Bitmap[12];
        for (int index = 0; index < 12; ++index)
          this.angularBitmaps[index] = new Bitmap((System.Drawing.Image) this.workingBitmap);
        this.states[this.stateId].gaussians = new List<Bitmap>(12);
        for (int i = 0; i < 12; i++)
        {
          this.CalculateGaussianForAngle(i);
          Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Delegate) (() =>
          {
            this.states[this.stateId].gaussians.Add(this.angularBitmaps[i]);
            this.GaussianImages[i].Source = MatchedFiltersView.getImageSourceFromBitmap(this.angularBitmaps[i]);
          }));
        }
        Bitmap backup = this.workingBitmap;
        Bitmap output = new Bitmap(this.workingBitmap.Width, this.workingBitmap.Height);
        for (int i = 0; i < output.Width; i++)
        {
          if (i % (output.Width / 12) == 0)
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Delegate) (() => this.CurrentImage.Source = MatchedFiltersView.getImageSourceFromBitmap(output)));
          for (int j = 0; j < output.Height; j++)
          {
            if (this.mask.GetPixel(i, j).G > (byte) 0)
            {
              byte num2 = ((IEnumerable<Bitmap>) this.angularBitmaps).Max<Bitmap, byte>((Func<Bitmap, byte>) (x => x.GetPixel(i, j).G));
              output.SetPixel(i, j, System.Drawing.Color.FromArgb((int) byte.MaxValue, (int) num2, (int) num2, (int) num2));
            }
          }
        }
        this.workingBitmap = output;
        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Delegate) (() => this.CurrentImage.Source = MatchedFiltersView.getImageSourceFromBitmap(backup)));
        this.LastOperation = StateOperations.SegmentVessels;
        this.ShowNextImage();
      }
    }

    private void CalculateGaussianForAngle(int id)
    {
      double angle = this.angles[id];
      int[,] kernel = new int[15, 15];
      double num1 = Math.Sin(Math.PI * angle / 180.0);
      double num2 = Math.Cos(Math.PI * angle / 180.0);
      int num3 = 7;
      for (int index1 = 0; index1 < 15; ++index1)
      {
        for (int index2 = 0; index2 < 15; ++index2)
        {
          int num4 = (int) Math.Round((double) (index1 - num3) * num2 - (double) (index2 - num3) * num1);
          int num5 = (int) Math.Round((double) (index1 - num3) * num1 + (double) (index2 - num3) * num2);
          int index3 = num4 + num3;
          int index4 = num5 + num3;
          if (index3 > 0 && index3 < 15 && index4 > 0 && index4 < 15)
            kernel[index3, index4] = this.kernel[index1, index2];
        }
      }
      new Convolution(kernel, 70)
      {
        DynamicDivisorForEdges = false
      }.ApplyInPlace(this.angularBitmaps[id]);
    }

    private void CalculateFirstKernel()
    {
      int num1 = 2;
      int num2 = 9;
      double num3 = 2.0 * Math.Pow((double) num1, 2.0);
      int length1 = 6 * num1 + 1;
      int length2 = num2;
      double[,] numArray = new double[length1, length2];
      int num4 = length1 / 2;
      for (int index1 = 0; index1 <= length1 / 2; ++index1)
      {
        numArray[num4 + index1, 0] = -Math.Exp(-Math.Pow((double) index1, 2.0) / num3);
        numArray[num4 - index1, 0] = numArray[num4 + index1, 0];
        for (int index2 = 1; index2 < length2; ++index2)
        {
          numArray[num4 + index1, index2] = numArray[num4 + index1, 0];
          numArray[num4 - index1, index2] = numArray[num4 - index1, 0];
        }
      }
      double num5 = 0.0;
      for (int index = 0; index < length1; ++index)
        num5 += 9.0 * numArray[index, 0];
      double num6 = num5 / (double) numArray.Length;
      this.kernel = new int[15, 15];
      int num7 = 1;
      int num8 = 3;
      for (int index1 = 0; index1 < length1; ++index1)
      {
        for (int index2 = 0; index2 < length2; ++index2)
          this.kernel[index1 + num7, index2 + num8] = (int) Math.Round((numArray[index1, index2] - num6) * 10.0);
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/RetinaImageProcessor.UI;component/logic/matchedfilters/view/matchedfiltersview.xaml", UriKind.Relative));
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
          ((ButtonBase) target).Click += new RoutedEventHandler(this.HistogramEqualization_Click);
          break;
        case 18:
          this.brightnessSlider = (Slider) target;
          break;
        case 19:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.BrightnessCorrection_Click);
          break;
        case 20:
          this.constrastBeforeSlider = (Slider) target;
          break;
        case 21:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.EnhanceContrastBeforeSegmentation_Click);
          break;
        case 22:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.GaussianMatchedFilter_Click);
          break;
        case 23:
          this.constrastAfterSlider = (Slider) target;
          break;
        case 24:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.EnhanceContrastAfterSegmentation_Click);
          break;
        case 25:
          this.binarizationSlider = (Slider) target;
          break;
        case 26:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.FindEntropyBinarizationThreshold_Click);
          break;
        case 27:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.Binarization_Click);
          break;
        case 28:
          this.RemoveVesselTextBox = (TextBox) target;
          this.RemoveVesselTextBox.PreviewTextInput += new TextCompositionEventHandler(this.NumberValidationTextBox);
          break;
        case 29:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.RemoveLooseVessels_Click);
          break;
        case 30:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.CompareImages_Click);
          break;
        case 31:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.K3MThinning_Click);
          break;
        case 32:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.ExtractMinutiae_Click);
          break;
        case 33:
          this.PreviousImage1 = (System.Windows.Controls.Image) target;
          break;
        case 34:
          this.PreviousImage2 = (System.Windows.Controls.Image) target;
          break;
        case 35:
          this.MaskImage = (System.Windows.Controls.Image) target;
          break;
        case 36:
          this.CoOccurenceImage = (System.Windows.Controls.Image) target;
          break;
        case 37:
          this.CurrentImage = (System.Windows.Controls.Image) target;
          break;
        case 38:
          this.CompareImage = (System.Windows.Controls.Image) target;
          break;
        case 39:
          this.GaussianImage1 = (System.Windows.Controls.Image) target;
          break;
        case 40:
          this.GaussianImage2 = (System.Windows.Controls.Image) target;
          break;
        case 41:
          this.GaussianImage3 = (System.Windows.Controls.Image) target;
          break;
        case 42:
          this.GaussianImage4 = (System.Windows.Controls.Image) target;
          break;
        case 43:
          this.GaussianImage5 = (System.Windows.Controls.Image) target;
          break;
        case 44:
          this.GaussianImage6 = (System.Windows.Controls.Image) target;
          break;
        case 45:
          this.GaussianImage7 = (System.Windows.Controls.Image) target;
          break;
        case 46:
          this.GaussianImage8 = (System.Windows.Controls.Image) target;
          break;
        case 47:
          this.GaussianImage9 = (System.Windows.Controls.Image) target;
          break;
        case 48:
          this.GaussianImage10 = (System.Windows.Controls.Image) target;
          break;
        case 49:
          this.GaussianImage11 = (System.Windows.Controls.Image) target;
          break;
        case 50:
          this.GaussianImage12 = (System.Windows.Controls.Image) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
