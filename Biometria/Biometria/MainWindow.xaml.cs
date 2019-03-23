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

            histR[0] = Convert.ToInt32((rHistogram[0] * rHistogram.Length) / (AfterImg.Width * AfterImg.Height));
            histG[0] = Convert.ToInt32((gHistogram[0] * gHistogram.Length) / (AfterImg.Width * AfterImg.Height));
            histB[0] = Convert.ToInt32((bHistogram[0] * bHistogram.Length) / (AfterImg.Width * AfterImg.Height));

            long cumulativeR = rHistogram[0];
            long cumulativeG = gHistogram[0];
            long cumulativeB = bHistogram[0];

            for (var i = 1; i < histR.Length; i++)
            {
                cumulativeR += rHistogram[i];
                histR[i] = Convert.ToInt32((cumulativeR * rHistogram.Length) / (AfterImg.Width * AfterImg.Height));

                cumulativeG += gHistogram[i];
                histG[i] = Convert.ToInt32((cumulativeG * gHistogram.Length) / (AfterImg.Width * AfterImg.Height));

                cumulativeB += bHistogram[i];
                histB[i] = Convert.ToInt32((cumulativeB * bHistogram.Length) / (AfterImg.Width * AfterImg.Height));
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

                    var nValueR = (byte)histR[intensityR];
                    var nValueG = (byte)histG[intensityG];
                    var nValueB = (byte)histB[intensityB];

                    //]]if (histR[intensityR] < 255)
                      //  nValueR = 255;
                    //if (histG[intensityG] < 255)
                      //  nValueG = 255;
                    //if (histB[intensityB] < 255)
                      //  nValueB = 255;

                    bytes[2] = nValueR;
                    bytes[1] = nValueG;
                    bytes[0] = nValueB;
                    
                    bmp.WritePixels(rect, bytes, bytesPerPixel, 0);

                    
                }
            }
            AfterImg = bmp;
            AfterImage.Source = bmp;
        }

        private void btn_histogram_equalization_Click(object sender, RoutedEventArgs e)
        {
            EqualizeHistogram();
            DrawHistogram(histR, "R");
            DrawHistogram(histG, "G");
            DrawHistogram(histB, "B");

            DrawAverageHistogram(histR, histG, histB);
          
        }
    }
}
