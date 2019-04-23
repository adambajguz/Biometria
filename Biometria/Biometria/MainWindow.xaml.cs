using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Windows.Storage.Streams;
using LiveCharts;
using LiveCharts.Wpf;
using Accord.Imaging.Filters;
using Accord.Imaging;

namespace Biometria
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource bitmap;
        Color c;
        WriteableBitmap wb;
        BitmapSource AfterImg;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "Image files (*.png, *.jpeg, *.jpg, *.bmp, *.gif, *.tiff, *.tif) | *.png; *.jpeg; *.jpg; *.bmp; *.gif; *.tiff; *.tif";
            fileDialog.DefaultExt = ".png";
            bool? dialogOk = fileDialog.ShowDialog();

            if (dialogOk == true)
            {
                tbxFiles.Text = fileDialog.FileName;
                OpenedImage.Source = new BitmapImage(new Uri(fileDialog.FileName));
                ImageSource imageSource = OpenedImage.Source;
                bitmap = (BitmapSource)imageSource;
                BeforeImage.Source = new BitmapImage(new Uri(fileDialog.FileName));
                AfterImage.Source = new BitmapImage(new Uri(fileDialog.FileName));
                BeforeImageF.Source = new BitmapImage(new Uri(fileDialog.FileName));
                AfterImageF.Source = new BitmapImage(new Uri(fileDialog.FileName));
                AfterImg = (BitmapSource)AfterImage.Source;
                wb = new WriteableBitmap((BitmapSource)imageSource);
            }
        }

        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SelectedColorText.Text = ClrPcker_Background.SelectedColor.ToString();
        }

        private void OpenedImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            XText.Text = Convert.ToInt32((e.GetPosition(OpenedImage).X * bitmap.PixelWidth / OpenedImage.ActualWidth)).ToString();
            YText.Text = Convert.ToInt32((e.GetPosition(OpenedImage).Y * bitmap.PixelHeight / OpenedImage.ActualHeight)).ToString();

            int x = Convert.ToInt32(e.GetPosition(OpenedImage).X * bitmap.PixelWidth / OpenedImage.ActualWidth);
            int y = Convert.ToInt32(e.GetPosition(OpenedImage).Y * bitmap.PixelHeight / OpenedImage.ActualHeight);

            c = GetPixelColor(bitmap, x, y);

            PickedPixelColorFromImage.Text = c.ToString();
            SelectedColorRectangle.Fill = new SolidColorBrush(c);
        }

        public static Color GetPixelColor(BitmapSource bitmap, int x, int y)
        {
            Color color;

            if (bitmap.Format != PixelFormats.Bgra32)
                bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            var bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var rect = new Int32Rect(x, y, 1, 1);

            bitmap.CopyPixels(rect, bytes, bytesPerPixel, 0);

            if (bitmap.Format == PixelFormats.Bgra32)
            {
                color = Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
            }
            else if (bitmap.Format == PixelFormats.Bgr32)
            {
                color = Color.FromRgb(bytes[2], bytes[1], bytes[0]);
            }
            // handle other required formats
            else
            {
                color = Colors.Black;
            }

            return color;
        }

        private void ChangePixelBtn_Click(object sender, RoutedEventArgs e)
        {
            Color c = (Color)ClrPcker_Background.SelectedColor;


            if (bitmap.Format != PixelFormats.Bgra32)
                bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            var bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var rect = new Int32Rect(Convert.ToInt32(XText.Text), Convert.ToInt32(YText.Text), 1, 1);

            bitmap.CopyPixels(rect, bytes, bytesPerPixel, 0);

            bytes[3] = c.A;
            bytes[2] = c.R;
            bytes[1] = c.G;
            bytes[0] = c.B;


            var bmp = new WriteableBitmap(bitmap);
            bmp.WritePixels(rect, bytes, bytesPerPixel, 0);
            OpenedImage.Source = bmp;
            BeforeImage.Source = bmp;
            bitmap = bmp;
        }

        private double _zoomValue = 1.0;

        private void OpenedImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                _zoomValue += 0.1;
            }
            else
            {
                _zoomValue -= 0.1;
            }

            ScaleTransform scale = new ScaleTransform(_zoomValue, _zoomValue);
            OpenedImage.LayoutTransform = scale;
            e.Handled = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Image files (*.png, *.jpeg, *.jpg, *.bmp, *.gif, *.tiff, *.tif) | *.png; *.jpeg; *.jpg; *.bmp; *.gif; *.tiff, *.tif";
            fileDialog.DefaultExt = ".png";
            Nullable<bool> dialogOk = fileDialog.ShowDialog();

            if (dialogOk == true)
            {
                var extension = System.IO.Path.GetExtension(fileDialog.FileName);

                switch (extension.ToLower())
                {
                    case ".jpg":
                        var encoderjpg = new JpegBitmapEncoder();
                        SaveUsingEncoder(OpenedImage, fileDialog.FileName, encoderjpg);
                        break;
                    case ".png":
                        var encoderpng = new PngBitmapEncoder();
                        SaveUsingEncoder(OpenedImage, fileDialog.FileName, encoderpng);
                        break;
                    case ".bmp":
                        var encoderbmp = new BmpBitmapEncoder();
                        SaveUsingEncoder(OpenedImage, fileDialog.FileName, encoderbmp);
                        break;
                    case ".tiff":
                        var encodertiff = new TiffBitmapEncoder();
                        SaveUsingEncoder(OpenedImage, fileDialog.FileName, encodertiff);
                        break;
                    case ".gif":
                        var encodergif = new GifBitmapEncoder();
                        SaveUsingEncoder(OpenedImage, fileDialog.FileName, encodergif);
                        break;

                }

            }
        }


        public int[] GetHistogram(string RGBType)
        {
            var histogram = new int[256];

            if (bitmap.Format != PixelFormats.Bgr24)
                bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgr24, null, 0);
            OpenedImage.Source = bitmap;
            BeforeImage.Source = bitmap;
            var bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];

            for (int i = 0; i < bitmap.PixelHeight; i++)
            {
                for (var j = 0; j < bitmap.PixelWidth; j++)
                {
                    var rect = new Int32Rect(j, i, 1, 1);
                    bitmap.CopyPixels(rect, bytes, bytesPerPixel, 0);

                    switch (RGBType)
                    {
                        case "R":
                            histogram[bytes[2]]++;
                            break;
                        case "G":
                            histogram[bytes[1]]++;
                            break;
                        case "B":
                            histogram[bytes[0]]++;
                            break;
                    }

                }

            }

            return histogram;
        }

        public int[] GetHistogramOfAfterImage(string RGBType)
        {
            var histogram = new int[256];

            if (AfterImg.Format != PixelFormats.Bgr24)
                AfterImg = new FormatConvertedBitmap(AfterImg, PixelFormats.Bgr24, null, 0);
            OpenedImage.Source = AfterImg;
            AfterImage.Source = AfterImg;
            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];

            for (int i = 0; i < AfterImg.PixelHeight; i++)
            {
                for (var j = 0; j < AfterImg.PixelWidth; j++)
                {
                    var rect = new Int32Rect(j, i, 1, 1);
                    AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                    switch (RGBType)
                    {
                        case "R":
                            histogram[bytes[2]]++;
                            break;
                        case "G":
                            histogram[bytes[1]]++;
                            break;
                        case "B":
                            histogram[bytes[0]]++;
                            break;
                    }

                }

            }

            return histogram;
        }

        private static void SaveUsingEncoder(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            Size visualSize = new Size(visual.ActualWidth, visual.ActualHeight);
            visual.Measure(visualSize);
            visual.Arrange(new Rect(visualSize));
            bitmap.Render(visual);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }

        public SeriesCollection SeriesCollectionR { get; set; }
        public SeriesCollection SeriesCollectionG { get; set; }
        public SeriesCollection SeriesCollectionB { get; set; }
        public SeriesCollection SeriesCollectionAverage { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }


        public void DrawHistogram(int[] histogram, string SeriesCollectionn)
        {
            var XValues = new ChartValues<int>() { 0 - 255 };
            var YValues = new ChartValues<int>();
            for (int i = 0; i < 256; i++)
            {
                YValues.Add(histogram[i]);
            }

            switch (SeriesCollectionn)
            {
                case "R":
                    SeriesCollectionR = new SeriesCollection
                    {
                new LineSeries
                {
                    Title="Red",
                    Stroke =Brushes.Red,
                    Values = YValues
                }
            };
                    R.Series = SeriesCollectionR;
                    break;
                case "G":
                    SeriesCollectionG = new SeriesCollection
            {
                new LineSeries
                {
                    Title="Green",
                    Stroke=Brushes.Green,
                    Values = YValues
                }
            };
                    G.Series = SeriesCollectionG;
                    break;
                case "B":
                    SeriesCollectionB = new SeriesCollection
            {
                new LineSeries
                {
                    Title="Blue",
                    Stroke=Brushes.Blue,
                    Values = YValues
                }
            };
                    B.Series = SeriesCollectionB;
                    break;
            }

            DataContext = this;

        }

        public void DrawAverageHistogram(int[] redHistogram, int[] greenHistogram, int[] blueHistogram)
        {
            var YValues = new ChartValues<int>();
            for (int i = 0; i < 256; i++)
            {
                YValues.Add(Convert.ToInt32((redHistogram[i] + greenHistogram[i] + blueHistogram[i]) / 3));
            }

            SeriesCollectionAverage = new SeriesCollection
            {
                new LineSeries
                {
                    Title="Average",
                    Stroke=Brushes.BlueViolet,
                    Values = YValues
                }
            };
            AV.Series = SeriesCollectionAverage;
        }



        private void TabItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int[] histogramR = GetHistogram("R");
            DrawHistogram(histogramR, "R");

            int[] histogramG = GetHistogram("G");
            DrawHistogram(histogramG, "G");

            int[] histogramB = GetHistogram("B");
            DrawHistogram(histogramB, "B");

            DrawAverageHistogram(histogramR, histogramG, histogramB);
        }

        private void None_effect_Click(object sender, RoutedEventArgs e)
        {
            int[] histogramR = GetHistogram("R");
            DrawHistogram(histogramR, "R");

            int[] histogramG = GetHistogram("G");
            DrawHistogram(histogramG, "G");

            int[] histogramB = GetHistogram("B");
            DrawHistogram(histogramB, "B");

            DrawAverageHistogram(histogramR, histogramG, histogramB);
            AfterImage.Source = BeforeImage.Source;
            AfterImage.Source = BeforeImage.Source;
            AfterImg = AfterImg = (BitmapSource)AfterImage.Source;
            wb = new WriteableBitmap((BitmapSource)BeforeImage.Source);
        }

        public void Darken()
        {
            double cR = (double)0, cG = (double)0, cB = (double)0;

            if (AfterImg.Format != PixelFormats.Bgr24)
                AfterImg = new FormatConvertedBitmap(AfterImg, PixelFormats.Bgr24, null, 0);

            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var bmp = new WriteableBitmap(AfterImg);

            cR = cG = cB = 20;

            double r = 1.01;

            for (var i = 0; i < AfterImg.PixelHeight; i++)
            {
                for (var j = 0; j < AfterImg.PixelWidth; j++)
                {

                    var rect = new Int32Rect(j, i, 1, 1);
                    AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                    var intensityR = bytes[2];
                    var intensityG = bytes[1];
                    var intensityB = bytes[0];

                    bytes[2] = (byte)((Math.Pow(r, intensityR) + 1) * cR);
                    bytes[1] = (byte)((Math.Pow(r, intensityG) + 1) * cG);
                    bytes[0] = (byte)((Math.Pow(r, intensityB) + 1) * cB);

                    bmp.WritePixels(rect, bytes, bytesPerPixel, 0);
                }
            }
            AfterImg = bmp;
            AfterImage.Source = bmp;
        }


        private void btn_histogram_darkening_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;
            Darken();

            int[] AHistogramR = GetHistogramOfAfterImage("R");
            DrawHistogram(AHistogramR, "R");
            int[] AHistogramG = GetHistogramOfAfterImage("G");
            DrawHistogram(AHistogramG, "G");
            int[] AHistogramB = GetHistogramOfAfterImage("B");
            DrawHistogram(AHistogramB, "B");

            DrawAverageHistogram(AHistogramR, AHistogramG, AHistogramB);
        }

        public void Lighten()
        {
            double cR = (double)0, cG = (double)0, cB = (double)0;

            if (AfterImg.Format != PixelFormats.Bgr24)
                AfterImg = new FormatConvertedBitmap(AfterImg, PixelFormats.Bgr24, null, 0);

            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var bmp = new WriteableBitmap(AfterImg);

            int[] AHistogramR = GetHistogramOfAfterImage("R");
            int[] AHistogramG = GetHistogramOfAfterImage("G");
            int[] AHistogramB = GetHistogramOfAfterImage("B");

            for (var i = 0; i < 256; i++)
            {
                if (AHistogramR[i] > 0)
                    cR = i;

                if (AHistogramG[i] > 0)
                    cG = i;

                if (AHistogramB[i] > 0)
                    cB = i;
            }

            cR = 255 / (Math.Log(1 + cR));
            cG = 255 / (Math.Log(1 + cG));
            cB = 255 / (Math.Log(1 + cB));

            for (var i = 0; i < AfterImg.PixelHeight; i++)
            {
                for (var j = 0; j < AfterImg.PixelWidth; j++)
                {

                    var rect = new Int32Rect(j, i, 1, 1);
                    AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                    var intensityR = bytes[2];
                    var intensityG = bytes[1];
                    var intensityB = bytes[0];

                    bytes[2] = (byte)(Math.Min(255, Math.Log(1 + intensityR) * cR));
                    bytes[1] = (byte)(Math.Min(255, Math.Log(1 + intensityG) * cG));
                    bytes[0] = (byte)(Math.Min(255, Math.Log(1 + intensityB) * cB));

                    bmp.WritePixels(rect, bytes, bytesPerPixel, 0);
                }
            }
            AfterImg = bmp;
            AfterImage.Source = bmp;
        }


        private void btn_histogram_lightening_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;
            Lighten();

            int[] AHistogramR = GetHistogramOfAfterImage("R");
            DrawHistogram(AHistogramR, "R");
            int[] AHistogramG = GetHistogramOfAfterImage("G");
            DrawHistogram(AHistogramG, "G");
            int[] AHistogramB = GetHistogramOfAfterImage("B");
            DrawHistogram(AHistogramB, "B");

            DrawAverageHistogram(AHistogramR, AHistogramG, AHistogramB);
        }

        private byte[] UpdateLUT(double a, int b)
        {
            byte[] LUT = new byte[256];

            for (int i = 0; i < 256; i++)
                if ((a * (i + b)) > 255)
                    LUT[i] = 255;
                else if ((a * (i + b)) < 0)
                    LUT[i] = 0;
                else
                    LUT[i] = (byte)(a * (i + b));

            return LUT;
        }

        private void StretchHistogram()
        {
            byte[] LUT_R = new byte[256];
            byte[] LUT_G = new byte[256];
            byte[] LUT_B = new byte[256];

            if (AfterImg.Format != PixelFormats.Bgr24)
                AfterImg = new FormatConvertedBitmap(AfterImg, PixelFormats.Bgr24, null, 0);

            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var bmp = new WriteableBitmap(AfterImg);

            if (Int32.TryParse(StretchA.Text, out int a))
            {
                if (Int32.TryParse(StretchB.Text, out int b))
                {
                    ////przelicz tablice LUT, tak by rozciagnac histogram
                    LUT_R = UpdateLUT(255.0 / (b - a), -a);
                    LUT_G = UpdateLUT(255.0 / (b - a), -a);
                    LUT_B = UpdateLUT(255.0 / (b - a), -a);


                    for (var i = 0; i < AfterImg.PixelHeight; i++)
                    {
                        for (var j = 0; j < AfterImg.PixelWidth; j++)
                        {

                            var rect = new Int32Rect(j, i, 1, 1);
                            AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                            var intensityR = bytes[2];
                            var intensityG = bytes[1];
                            var intensityB = bytes[0];

                            bytes[2] = (byte)LUT_R[intensityR];
                            bytes[1] = (byte)LUT_G[intensityG];
                            bytes[0] = (byte)LUT_B[intensityB];

                            bmp.WritePixels(rect, bytes, bytesPerPixel, 0);
                        }
                    }
                    AfterImg = bmp;
                    AfterImage.Source = bmp;
                }
            }
        }

        private void btn_histogram_stretching_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;
            StretchHistogram();

            int[] AHistogramR = GetHistogramOfAfterImage("R");
            DrawHistogram(AHistogramR, "R");
            int[] AHistogramG = GetHistogramOfAfterImage("G");
            DrawHistogram(AHistogramG, "G");
            int[] AHistogramB = GetHistogramOfAfterImage("B");
            DrawHistogram(AHistogramB, "B");

            DrawAverageHistogram(AHistogramR, AHistogramG, AHistogramB);
        }


        int[] rHistogram;
        int[] gHistogram;
        int[] bHistogram;

        int[] histR;
        int[] histG;
        int[] histB;

        public void EqualizeHistogram()
        {
            rHistogram = GetHistogram("R");
            gHistogram = GetHistogram("G");
            bHistogram = GetHistogram("B");

            histR = new int[256];
            histG = new int[256];
            histB = new int[256];


            if (AfterImg.Format != PixelFormats.Bgr24)
                AfterImg = new FormatConvertedBitmap(AfterImg, PixelFormats.Bgr24, null, 0);
            
            int totalpixels = AfterImg.PixelWidth * AfterImg.PixelHeight;

            histR[0] = Convert.ToInt32((rHistogram[0] * rHistogram.Length) / totalpixels);
            histG[0] = Convert.ToInt32((gHistogram[0] * gHistogram.Length) / totalpixels);
            histB[0] = Convert.ToInt32((bHistogram[0] * bHistogram.Length) / totalpixels);
            AfterImage.Source = AfterImg;

            long cumulativeR = rHistogram[0];
            long cumulativeG = gHistogram[0];
            long cumulativeB = bHistogram[0];

            for (var i = 1; i < histR.Length; i++)
            {
                cumulativeR += rHistogram[i];
                histR[i] = Convert.ToInt32((cumulativeR * rHistogram.Length) / totalpixels);

                cumulativeG += gHistogram[i];
                histG[i] = Convert.ToInt32((cumulativeG * gHistogram.Length) / totalpixels);

                cumulativeB += bHistogram[i];
                histB[i] = Convert.ToInt32((cumulativeB * bHistogram.Length) / totalpixels);
            }

            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var bmp = new WriteableBitmap(AfterImg);

            for (var i = 0; i < AfterImg.PixelHeight; i++)
            {
                for (var j = 0; j < AfterImg.PixelWidth; j++)
                {

                    var rect = new Int32Rect(j, i, 1, 1);
                    AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                    var intensityR = bytes[2];
                    var intensityG = bytes[1];
                    var intensityB = bytes[0];

                    bytes[2] = (byte)histR[intensityR];
                    bytes[1] = (byte)histG[intensityG];
                    bytes[0] = (byte)histB[intensityB];

                    bmp.WritePixels(rect, bytes, bytesPerPixel, 0);
                }
            }
            AfterImg = bmp;
            AfterImage.Source = bmp;
        }

        private void btn_histogram_equalization_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;
            EqualizeHistogram();
            int[] AHistogramR = GetHistogramOfAfterImage("R");
            DrawHistogram(AHistogramR, "R");
            int[] AHistogramG = GetHistogramOfAfterImage("G");
            DrawHistogram(AHistogramG, "G");
            int[] AHistogramB = GetHistogramOfAfterImage("B");
            DrawHistogram(AHistogramB, "B");

            DrawAverageHistogram(AHistogramR, AHistogramG, AHistogramB);

        }

        //binarization

        public void Greyscaling()
        {

            if (AfterImg.Format != PixelFormats.Bgr24)
                AfterImg = new FormatConvertedBitmap(AfterImg, PixelFormats.Bgr24, null, 0);


            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var bmp = new WriteableBitmap(AfterImg);

            for (var i = 0; i < AfterImg.PixelHeight; i++)
            {
                for (var j = 0; j < AfterImg.PixelWidth; j++)
                {

                    var rect = new Int32Rect(j, i, 1, 1);
                    AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                    var intensityR = bytes[2];
                    var intensityG = bytes[1];
                    var intensityB = bytes[0];

                    int r;
                    int g;
                    int b;

                    var gray = (intensityR * 6966 + intensityG * 23436 + intensityB * 2366) >> 15;
                    r = g = b = gray;

                    bytes[2] = (byte)r;
                    bytes[1] = (byte)g;
                    bytes[0] = (byte)b;

                    bmp.WritePixels(rect, bytes, bytesPerPixel, 0);
                }
            }
            AfterImg = bmp;
            AfterImage.Source = bmp;
        }

        private void btn_binarization_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;

            Greyscaling();

            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var bmp = new WriteableBitmap(AfterImg);

            for (var i = 0; i < AfterImg.PixelHeight; i++)
            {
                for (var j = 0; j < AfterImg.PixelWidth; j++)
                {
                    var rect = new Int32Rect(j, i, 1, 1);
                    AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                    var intensityR = bytes[2];
                    var intensityG = bytes[1];
                    var intensityB = bytes[0];

                    if (intensityR > Convert.ToInt32(binarization_threshold.Text))
                    {
                        bytes[2] = (byte)(255);
                        bytes[1] = (byte)(255);
                        bytes[0] = (byte)255;
                    }

                    else
                    {
                        bytes[2] = (byte)(0);
                        bytes[1] = (byte)(0);
                        bytes[0] = (byte)0;
                    }
                    bmp.WritePixels(rect, bytes, bytesPerPixel, 0);
                }
            }

            AfterImg = bmp;
            AfterImage.Source = bmp;


            int[] AHistogramR = GetHistogramOfAfterImage("R");
            DrawHistogram(AHistogramR, "R");
            int[] AHistogramG = GetHistogramOfAfterImage("G");
            DrawHistogram(AHistogramG, "G");
            int[] AHistogramB = GetHistogramOfAfterImage("B");
            DrawHistogram(AHistogramB, "B");

            DrawAverageHistogram(AHistogramR, AHistogramG, AHistogramB);
        }

        private void btn_greyscaling_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;
            Greyscaling();


            int[] AHistogramR = GetHistogramOfAfterImage("R");
            DrawHistogram(AHistogramR, "R");
            int[] AHistogramG = GetHistogramOfAfterImage("G");
            DrawHistogram(AHistogramG, "G");
            int[] AHistogramB = GetHistogramOfAfterImage("B");
            DrawHistogram(AHistogramB, "B");

            DrawAverageHistogram(AHistogramR, AHistogramG, AHistogramB);
        }

        public class Otsu
        {
            // function is used to compute the q values in the equation
            private static int Px(int init, int end, int[] hist)
            {
                int sum = 0;
                for (int i = init; i <= end; ++i)
                    sum += hist[i];

                return sum;
            }

            // function is used to compute the mean values in the equation (mu)
            private static int Mx(int init, int end, int[] hist)
            {
                int sum = 0;
                for (int i = init; i <= end; ++i)
                    sum += i * hist[i];

                return sum;
            }

            // finds the maximum element in a vector
            private static int FindMax(double[] vec)
            {
                double maxVec = 0;
                int idx = 0;

                for (int i = 1; i < vec.Length - 1; ++i)
                    if (vec[i] > maxVec)
                    {
                        maxVec = vec[i];
                        idx = i;
                    }

                return idx;
            }

            // find otsu threshold
            public static int GetOtsuThreshold(int[] imageHistogramData)
            {
                double[] vet = new double[256];

                int[] hist = imageHistogramData;

                // loop through all possible t values and maximize between class variance
                for (int k = 1; k != 255; ++k)
                {
                    double p1 = Px(0, k, hist);
                    double p2 = Px(k + 1, 255, hist);
                    double p12 = p1 * p2;
                    if (p12 == 0)
                        p12 = 1;

                    double diff = (Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                    vet[k] = diff * diff / p12;
                    //vet[k] = (float)Math.Pow((Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1), 2) / p12;
                }

                return FindMax(vet);
            }
        }

        private void btn_otsu_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;
            Greyscaling();

            int threshold = Otsu.GetOtsuThreshold(GetHistogramOfAfterImage("R"));

            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var bmp = new WriteableBitmap(AfterImg);

            for (var i = 0; i < AfterImg.PixelHeight; i++)
            {
                for (var j = 0; j < AfterImg.PixelWidth; j++)
                {
                    var rect = new Int32Rect(j, i, 1, 1);
                    AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                    var intensityR = bytes[2];
                    var intensityG = bytes[1];
                    var intensityB = bytes[0];

                    if (intensityR > threshold)
                    {
                        bytes[2] = (byte)(255);
                        bytes[1] = (byte)(255);
                        bytes[0] = (byte)255;
                    }

                    else
                    {
                        bytes[2] = (byte)(0);
                        bytes[1] = (byte)(0);
                        bytes[0] = (byte)0;
                    }
                    bmp.WritePixels(rect, bytes, bytesPerPixel, 0);
                }
            }

            AfterImg = bmp;
            AfterImage.Source = bmp;


            int[] AHistogramR = GetHistogramOfAfterImage("R");
            DrawHistogram(AHistogramR, "R");
            int[] AHistogramG = GetHistogramOfAfterImage("G");
            DrawHistogram(AHistogramG, "G");
            int[] AHistogramB = GetHistogramOfAfterImage("B");
            DrawHistogram(AHistogramB, "B");

            DrawAverageHistogram(AHistogramR, AHistogramG, AHistogramB);
        }


        public void Threshold(int neighborhood, double k)
        {


            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var bmp = new WriteableBitmap(AfterImg);

            short[,] data = new short[bmp.PixelHeight, bmp.PixelWidth];

            for (var i = 0; i < AfterImg.PixelHeight; i++)
            {
                for (var j = 0; j < AfterImg.PixelWidth; j++)
                {
                    var rect = new Int32Rect(j, i, 1, 1);
                    AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                    var intensityR = bytes[2];

                    data[i, j] = (byte)(intensityR);
                }
            }

            double[,] dataFloat = ArrayUtil.toDoubleArray(data);
            double[,] stdev = ArrayUtil.stdevNeighborhood(dataFloat, neighborhood);
            double[,] mean = ArrayUtil.meanNeighborhood(dataFloat, neighborhood);
            double[,] threshold = ArrayUtil.add(dataFloat, mean, ArrayUtil.multiplyEach(dataFloat, stdev, k));


            for (var i = 0; i < AfterImg.PixelHeight; i++)
            {
                for (var j = 0; j < AfterImg.PixelWidth; j++)
                {
                    var rect = new Int32Rect(j, i, 1, 1);
                    AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                    var intensityR = bytes[2];
                    var intensityG = bytes[1];
                    var intensityB = bytes[0];

                    if (intensityR > threshold[i, j])
                    {
                        bytes[2] = (byte)(255);
                        bytes[1] = (byte)(255);
                        bytes[0] = (byte)255;
                    }

                    else
                    {
                        bytes[2] = (byte)(0);
                        bytes[1] = (byte)(0);
                        bytes[0] = (byte)0;
                    }
                    bmp.WritePixels(rect, bytes, bytesPerPixel, 0);

                }
            }

            AfterImg = bmp;
            AfterImage.Source = bmp;
        }


        private void btn_niblack_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;
            Greyscaling();
            Threshold(Convert.ToInt32(XNiblackText.Text), Convert.ToDouble(KNiblackText.Text));
            int[] AHistogramR = GetHistogramOfAfterImage("R");
            DrawHistogram(AHistogramR, "R");
            int[] AHistogramG = GetHistogramOfAfterImage("G");
            DrawHistogram(AHistogramG, "G");
            int[] AHistogramB = GetHistogramOfAfterImage("B");
            DrawHistogram(AHistogramB, "B");

            DrawAverageHistogram(AHistogramR, AHistogramG, AHistogramB);
        }

        public int[,] kernel = new int[3, 3];

        public void setCustomKernel()
        {
            kernel[0, 0] = Convert.ToInt32(t1x1Text.Text);
            kernel[1, 0] = Convert.ToInt32(t2x1Text.Text);
            kernel[2, 0] = Convert.ToInt32(t3x1Text.Text);
            kernel[0, 1] = Convert.ToInt32(t1x2Text.Text);
            kernel[1, 1] = Convert.ToInt32(t2x2Text.Text);
            kernel[2, 1] = Convert.ToInt32(t3x2Text.Text);
            kernel[0, 2] = Convert.ToInt32(t1x3Text.Text);
            kernel[1, 2] = Convert.ToInt32(t2x3Text.Text);
            kernel[2, 2] = Convert.ToInt32(t3x3Text.Text);
        }

        public void setLowPassKernel()
        {
            kernel[0, 0] = 1;
            kernel[0, 1] = 2;
            kernel[0, 2] = 1;
            kernel[1, 0] = 2;
            kernel[1, 1] = 4;
            kernel[1, 2] = 2;
            kernel[2, 0] = 1;
            kernel[2, 1] = 2;
            kernel[2, 2] = 1;
        }

        public void setHighPassKernel()
        {
            kernel[0, 0] = -1;
            kernel[0, 1] = -1;
            kernel[0, 2] = -1;
            kernel[1, 0] = -1;
            kernel[1, 1] = 9;
            kernel[1, 2] = -1;
            kernel[2, 0] = -1;
            kernel[2, 1] = -1;
            kernel[2, 2] = -1;
        }

        public  BitmapSource Convolute( int[,] kernel)
        {
            var kernelFactorSum = 0;
            foreach (var b in kernel)
            {
                kernelFactorSum += Math.Abs(b);
            }

            return Convolute(kernel, kernelFactorSum, 0);
        }

        public BitmapSource Convolute(int[,] kernel, int kernelFactorSum, int kernelOffsetSum)
        {

            if (AfterImg.Format != PixelFormats.Bgr24)
                AfterImg = new FormatConvertedBitmap(AfterImg, PixelFormats.Bgr24, null, 0);

            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var bmp = new WriteableBitmap(AfterImg);

            var kh = kernel.GetUpperBound(0) + 1;
            var kw = kernel.GetUpperBound(1) + 1;

            if ((kw & 1) == 0)
            {
                throw new InvalidOperationException("Kernel width must be odd!");
            }
            if ((kh & 1) == 0)
            {
                throw new InvalidOperationException("Kernel height must be odd!");
            }
            
            var w = AfterImg.PixelWidth;
            var h = AfterImg.PixelHeight;
            var result = BitmapFactory.New(w, h);
            var kwh = kw >> 1;
            var khh = kh >> 1;

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var a = 0;
                    var r = 0;
                    var g = 0;
                    var b = 0;

                    

                    for (var kx = -kwh; kx <= kwh; kx++)
                    {
                        var px = kx + x;
                        if (px < 0)
                        {
                            px = 0;
                        }
                        else if (px >= w)
                        {
                            px = w - 1;
                        }

                        for (var ky = -khh; ky <= khh; ky++)
                        {
                            var py = ky + y;
                            if (py < 0)
                            {
                                py = 0;
                            }
                            else if (py >= h)
                            {
                                py = h - 1;
                            }

                            var rect2 = new Int32Rect(px, py, 1, 1);
                            AfterImg.CopyPixels(rect2, bytes, bytesPerPixel, 0);

                            var intensityR = bytes[2];
                            var intensityG = bytes[1];
                            var intensityB = bytes[0];


                            var k = kernel[ky + kwh, kx + khh];
                            r += intensityR * k;
                            g += intensityG * k;
                            b += intensityB * k;
                        }
                    }
                    
                    var tr = ((r / kernelFactorSum) + kernelOffsetSum);
                    var tg = ((g / kernelFactorSum) + kernelOffsetSum);
                    var tb = ((b / kernelFactorSum) + kernelOffsetSum);
                    
                    var br = (byte)((tr > 255) ? 255 : ((tr < 0) ? 0 : tr));
                    var bg = (byte)((tg > 255) ? 255 : ((tg < 0) ? 0 : tg));
                    var bb = (byte)((tb > 255) ? 255 : ((tb < 0) ? 0 : tb));

                    var rect = new Int32Rect(x, y, 1, 1);
                    AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                    bytes[2] = br;
                    bytes[1] = bg;
                    bytes[0] = bb;

                    bmp.WritePixels(rect, bytes, bytesPerPixel, 0);
                }
                
            }
            return bmp;
        }



        private void LowPassBtn_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;

            setLowPassKernel();
            var bmp = Convolute(kernel);

            AfterImg = bmp;
            AfterImage.Source = bmp;
            AfterImageF.Source = bmp;
        }

        private void HighPassBtn_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;

            setHighPassKernel();
            var bmp = Convolute(kernel);

            AfterImg = bmp;
            AfterImage.Source = bmp;
            AfterImageF.Source = bmp;
        }

        private void CustomPassBtn_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;

            setCustomKernel();
            var bmp = Convolute(kernel);

            AfterImg = bmp;
            AfterImage.Source = bmp;
            AfterImageF.Source = bmp;
        }

        private static readonly int[,] PrewittX = { {1, 0, -1},{1, 0, -1}, {1, 0, -1}};

        private static readonly int[,] PrewittY = { { 1,  1,  1},{ 0,  0,  0}, {-1, -1, -1}};

        private void PrevittXBtn_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;
            
            var bmp = Convolute(PrewittX);

            AfterImg = bmp;
            AfterImage.Source = bmp;
            AfterImageF.Source = bmp;
        }

        private void PrevittYBtn_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;

            var bmp = Convolute(PrewittY);

            AfterImg = bmp;
            AfterImage.Source = bmp;
            AfterImageF.Source = bmp;
        }

        private static readonly int[,] SobelX = {{-1, 0, 1}, {-2, 0, 2},{-1, 0, 1}};

        private static readonly int[,] SobelY = {{-1, -2, -1}, { 0,  0,  0},{ 1,  2,  1}};

        private void SobelXBtn_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;

            var bmp = Convolute(SobelX);

            AfterImg = bmp;
            AfterImage.Source = bmp;
            AfterImageF.Source = bmp;
        }

        private void SobelYBtn_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;

            var bmp = Convolute(SobelY);

            AfterImg = bmp;
            AfterImage.Source = bmp;
            AfterImageF.Source = bmp;
        }

        private static readonly int[,] Laplace = {{-1, -1, -1},{-1,  8, -1},{-1, -1, -1}};

        private void LaplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;

            var bmp = Convolute(Laplace);

            AfterImg = bmp;
            AfterImage.Source = bmp;
            AfterImageF.Source = bmp;
        }

        public WriteableBitmap KuwaharaFilter(int size)
        {
            int width = AfterImg.PixelWidth;
            int height = AfterImg.PixelHeight;


            if (AfterImg.Format != PixelFormats.Bgr24)
                AfterImg = new FormatConvertedBitmap(AfterImg, PixelFormats.Bgr24, null, 0);

            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var bmp = new WriteableBitmap(AfterImg);
            

                    int[] ApetureMinX = { -(size / 2), 0, -(size / 2), 0 };
                    int[] ApetureMaxX = { 0, (size / 2), 0, (size / 2) };
                    int[] ApetureMinY = { -(size / 2), -(size / 2), 0, 0 };
                    int[] ApetureMaxY = { 0, 0, (size / 2), (size / 2) };

                    for (int x = 0; x < width; ++x)
                    {
                        for (int y = 0; y < height; ++y)
                        {
                            int[] RValues = { 0, 0, 0, 0 };
                            int[] GValues = { 0, 0, 0, 0 };
                            int[] BValues = { 0, 0, 0, 0 };
                            int[] NumPixels = { 0, 0, 0, 0 };
                            int[] MaxRValue = { 0, 0, 0, 0 };
                            int[] MaxGValue = { 0, 0, 0, 0 };
                            int[] MaxBValue = { 0, 0, 0, 0 };
                            int[] MinRValue = { 255, 255, 255, 255 };
                            int[] MinGValue = { 255, 255, 255, 255 };
                            int[] MinBValue = { 255, 255, 255, 255 };
                            for (int i = 0; i < 4; ++i)
                            {
                                for (int x2 = ApetureMinX[i]; x2 < ApetureMaxX[i]; ++x2)
                                {
                                    int TempX = x + x2;
                                    if (TempX >= 0 && TempX < width)
                                    {
                                        for (int y2 = ApetureMinY[i]; y2 < ApetureMaxY[i]; ++y2)
                                        {
                                            int TempY = y + y2;
                                            if (TempY >= 0 && TempY < height)
                                            {
                                                var rect2 = new Int32Rect(TempX, TempY, 1, 1);
                                                AfterImg.CopyPixels(rect2, bytes, bytesPerPixel, 0);

                                                var intensityR = bytes[2];
                                                var intensityG = bytes[1];
                                                var intensityB = bytes[0];
                                                
                                                RValues[i] += intensityR;
                                                GValues[i] += intensityG;
                                                BValues[i] += intensityB;

                                                if (intensityR > MaxRValue[i])
                                                    MaxRValue[i] = intensityR;
                                                else if (intensityR < MinRValue[i])
                                                    MinRValue[i] = intensityR;

                                                if (intensityG > MaxGValue[i])
                                                    MaxGValue[i] = intensityG;
                                                else if (intensityG < MinGValue[i])
                                                    MinGValue[i] = intensityG;

                                                if (intensityB > MaxBValue[i])
                                                    MaxBValue[i] = intensityB;
                                                else if (intensityB < MinBValue[i])
                                                    MinBValue[i] = intensityB;

                                                ++NumPixels[i];
                                            }
                                        }
                                    }
                                }
                            }
                            int j = 0;
                            int MinDifference = 10000;
                            for (int i = 0; i < 4; ++i)
                            {
                                int CurrentDifference = (MaxRValue[i] - MinRValue[i]) + (MaxGValue[i] - MinGValue[i]) + (MaxBValue[i] - MinBValue[i]);
                                if (CurrentDifference < MinDifference && NumPixels[i] > 0)
                                {
                                    j = i;
                                    MinDifference = CurrentDifference;
                                }
                            }

                            var rect = new Int32Rect(x, y, 1, 1);
                            AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                            bytes[2] = (byte)(RValues[j] / NumPixels[j]);
                            bytes[1] = (byte)(GValues[j] / NumPixels[j]);
                            bytes[0] = (byte)(BValues[j] / NumPixels[j]);

                            bmp.WritePixels(rect, bytes, bytesPerPixel, 0);
                        }
                    }
            return bmp;
        }

        private void KuwaharaBtn_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;

            var bmp = KuwaharaFilter(5);

            AfterImg = bmp;
            AfterImage.Source = bmp;
            AfterImageF.Source = bmp;
        }

        public WriteableBitmap MedianFilter(int maskSize)
        {
            int width = AfterImg.PixelWidth;
            int height = AfterImg.PixelHeight;

            if (AfterImg.Format != PixelFormats.Bgr24)
                AfterImg = new FormatConvertedBitmap(AfterImg, PixelFormats.Bgr24, null, 0);

            var bytesPerPixel = (AfterImg.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var bmp = new WriteableBitmap(AfterImg);
            
                    byte[] red, green, blue;
            
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            red = new byte[maskSize * maskSize];
                            green = new byte[maskSize * maskSize];
                            blue = new byte[maskSize * maskSize];
                            int count = 0;
                            for (int r = y - (maskSize / 2); r <= y + (maskSize / 2); r++)
                            {
                                for (int c = x - (maskSize / 2); c <= x + (maskSize / 2); c++)
                                {
                                    if (r < 0 || r >= height || c < 0 || c >= width)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        var rect2 = new Int32Rect(c, r, 1, 1);
                                        AfterImg.CopyPixels(rect2, bytes, bytesPerPixel, 0);

                                        var intensityR = bytes[2];
                                        var intensityG = bytes[1];
                                        var intensityB = bytes[0];
                                        
                                        red[count] = intensityR;
                                        green[count] = intensityG;
                                        blue[count] = intensityB;
                                        ++count;
                                    }
                                }
                            }
                            Array.Sort(red);
                            Array.Sort(green);
                            Array.Sort(blue);

                            int index = (count % 2 == 0) ? count / 2 - 1 : count / 2;

                            var rect = new Int32Rect(x, y, 1, 1);
                            AfterImg.CopyPixels(rect, bytes, bytesPerPixel, 0);

                            bytes[2] = (byte)red[index];
                            bytes[1] = (byte)green[index];
                            bytes[0] = (byte)blue[index];

                            bmp.WritePixels(rect, bytes, bytesPerPixel, 0);
                        }
                    }

              
            return bmp;
        }

        private void Median3x3Btn_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;

            var bmp = MedianFilter(3);

            AfterImg = bmp;
            AfterImage.Source = bmp;
            AfterImageF.Source = bmp;
        }

        private void Median5x5Btn_Click(object sender, RoutedEventArgs e)
        {
            AfterImg = (BitmapSource)BeforeImage.Source;

            var bmp = MedianFilter(5);

            AfterImg = bmp;
            AfterImage.Source = bmp;
            AfterImageF.Source = bmp;
        }
    }
}
