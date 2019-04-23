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

<<<<<<< Updated upstream
            int totalpixels = AfterImg.PixelWidth * AfterImg.PixelHeight;

            histR[0] = Convert.ToInt32((rHistogram[0] * rHistogram.Length) / totalpixels);
            histG[0] = Convert.ToInt32((gHistogram[0] * gHistogram.Length) / totalpixels);
            histB[0] = Convert.ToInt32((bHistogram[0] * bHistogram.Length) / totalpixels);
=======
            AfterImage.Source = AfterImg;

            histR[0] = Convert.ToInt32((rHistogram[0] * rHistogram.Length) / (AfterImg.Width * AfterImg.Height));
            histG[0] = Convert.ToInt32((gHistogram[0] * gHistogram.Length) / (AfterImg.Width * AfterImg.Height));
            histB[0] = Convert.ToInt32((bHistogram[0] * bHistogram.Length) / (AfterImg.Width * AfterImg.Height));
>>>>>>> Stashed changes

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
    }
}
