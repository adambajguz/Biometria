using System;
using System.Linq;
using ImageProcessor.Data;
using LiveCharts;
using LiveCharts.Uwp;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageProcessor.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistogramManipulationPage : Page
    {
        private WriteableBitmap editingBitmap;

        private readonly MainPage parentMainPage;

        private ImageHistogramData bitmapHistogramData;

        public HistogramManipulationPage()
        {
            this.InitializeComponent();

            Frame frame = (Frame)Window.Current.Content;
            parentMainPage = (MainPage)frame.Content;

            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.editingBitmap = e.Parameter as WriteableBitmap;

            UpdateHistograms();
        }

        private void UpdateHistograms()
        {
            bitmapHistogramData = new ImageHistogramData(editingBitmap);
            SetHistograms(bitmapHistogramData);
        }

        private void SetHistograms(ImageHistogramData data)
        {
            SeriesCollection SeriesCollectionR = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Red",
                    Values = new ChartValues<int>(data.R.AsEnumerable()),
                    Fill = new SolidColorBrush(Colors.Red),
                }
            };
            RPlot.Series = SeriesCollectionR;

            SeriesCollection SeriesCollectionG = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Green",
                    Values = new ChartValues<int>(data.G.AsEnumerable()),
                    Fill = new SolidColorBrush(Colors.Green),
                }
            };
            GPlot.Series = SeriesCollectionG;

            SeriesCollection SeriesCollectionB = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Blue",
                    Values = new ChartValues<int>(data.B.AsEnumerable()),
                    Fill = new SolidColorBrush(Colors.Blue),
                }
            };
            BPlot.Series = SeriesCollectionB;

            SeriesCollection SeriesCollectionC = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "(R+G+B)/3",
                    Values = new ChartValues<int>(data.C.AsEnumerable()),
                    Fill = new SolidColorBrush(Colors.Black),
                }
            };
            CPlot.Series = SeriesCollectionC;
        }

        private double CalcC(int Max) => 255 / Math.Log(1 + Max);

        private async void LightenButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateHistograms();
            parentMainPage.WritableOutputImage = editingBitmap;
            await parentMainPage.UpdateOutputImage();
        }


        private async void DarkenButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateHistograms();
            parentMainPage.WritableOutputImage = editingBitmap;
            await parentMainPage.UpdateOutputImage();
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

        private async void StretchHistogramButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] LUT_R = new byte[256];
            byte[] LUT_G = new byte[256];
            byte[] LUT_B = new byte[256];



            if (Int32.TryParse(StretchA.Text, out int a))
            {
                if (Int32.TryParse(StretchB.Text, out int b))
                {
                    ////przelicz tablice LUT, tak by rozciagnac histogram
                    LUT_R = UpdateLUT(255.0 / (b - a), -a);
                    LUT_G = UpdateLUT(255.0 / (b - a), -a);
                    LUT_B = UpdateLUT(255.0 / (b - a), -a);

                    editingBitmap.ForEach((x, y, color) =>
                    {
                        return Color.FromArgb(color.A,
                                             LUT_R[color.R],
                                             LUT_G[color.G],
                                             LUT_B[color.B]);
                    });

                    UpdateHistograms();
                    parentMainPage.WritableOutputImage = editingBitmap;
                    await parentMainPage.UpdateOutputImage();
                }
            }
        }

        private double[] UpdateLUT2(double[] D)
        {
            double[] LUT = new double[256];



            int i;
            double D0min;
            //znajdz pierwszą niezerową wartosc dystrybuanty
            i = 0;
            while (D[i] == 0) i++;
            D0min = D[i];

            for (i = 0; i < 256; i++)
                LUT[i] = (((D[i] - D0min) / (1 - D0min)) * (256 - 1));
            return LUT;
        }

        private async void EqualizeHistogramButton_Click(object sender, RoutedEventArgs e)
        {
            int[] rHistogram = bitmapHistogramData.R;
            int[] gHistogram = bitmapHistogramData.G;
            int[] bHistogram = bitmapHistogramData.R;

            int[] histR = new int[256];
            int[] histG = new int[256];
            int[] histB = new int[256];

            int totalPixels = editingBitmap.PixelWidth * editingBitmap.PixelHeight;
            histR[0] = Convert.ToInt32((rHistogram[0] * rHistogram.Length) / (totalPixels));
            histG[0] = Convert.ToInt32((gHistogram[0] * gHistogram.Length) / (totalPixels));
            histB[0] = Convert.ToInt32((bHistogram[0] * bHistogram.Length) / (totalPixels));

            long cumulativeR = rHistogram[0];
            long cumulativeG = gHistogram[0];
            long cumulativeB = bHistogram[0];

            for (var i = 1; i < histR.Length; i++)
            {
                cumulativeR += rHistogram[i];
                histR[i] = Convert.ToInt32((cumulativeR * rHistogram.Length) / (totalPixels));

                cumulativeG += gHistogram[i];
                histG[i] = Convert.ToInt32((cumulativeG * gHistogram.Length) / (totalPixels));

                cumulativeB += bHistogram[i];
                histB[i] = Convert.ToInt32((cumulativeB * bHistogram.Length) / (totalPixels));
            }

   


            editingBitmap.ForEach((x, y, color) =>
            {
                return Color.FromArgb(color.A,
                                     (byte)histR[color.R],
                                     (byte)histG[color.G],
                                     (byte)histB[color.B]);
            });



            UpdateHistograms();
            parentMainPage.WritableOutputImage = editingBitmap;
            await parentMainPage.UpdateOutputImage();
        }


        //private async void EqualizeHistogramButton_Click(object sender, RoutedEventArgs e)
        //{
        //    double[] D_R = new double[256];
        //    double[] D_G = new double[256];
        //    double[] D_B = new double[256];

        //    int totalPixels = editingBitmap.PixelHeight * editingBitmap.PixelWidth;

        //    D_R[0] += bitmapHistogramData.R[0] / totalPixels;
        //    D_G[0] += bitmapHistogramData.G[0] / totalPixels;
        //    D_B[0] += bitmapHistogramData.B[0] / totalPixels;
        //    for (int i = 1; i < 256; ++i)
        //    {
        //        D_R[i] = (bitmapHistogramData.R[i] + bitmapHistogramData.R[i - 1]);
        //        D_G[i] = (bitmapHistogramData.G[i] + bitmapHistogramData.G[i - 1]);
        //        D_B[i] = (bitmapHistogramData.B[i] + bitmapHistogramData.B[i - 1]);

        //        D_R[i] /= totalPixels;
        //        D_G[i] /= totalPixels;
        //        D_B[i] /= totalPixels;
        //    }



        //    //przelicz tablice LUT, tak by rozciagnac histogram
        //    double[] LUT_R = UpdateLUT2(D_R);
        //    double[] LUT_G = UpdateLUT2(D_G);
        //    double[] LUT_B = UpdateLUT2(D_B);


        //    //LUT_R = UpdateLUT(255.0 / (LUT_R_max - LUT_R_min), -LUT_R_min);
        //    //LUT_G = UpdateLUT(255.0 / (LUT_G_max - LUT_G_min), -LUT_G_min);
        //    //LUT_B = UpdateLUT(255.0 / (LUT_B_max - LUT_B_min), -LUT_B_min);


        //    //for (int i = 0; i < LUT_R.Length; ++i)
        //    //{
        //    //    LUT_R[i] = (byte)(((255 * (i - LUT_R_min)) / (LUT_R_max - LUT_R_min))%256);
        //    //    LUT_G[i] = (byte)(((255 * (i - LUT_G_min)) / (LUT_G_max - LUT_G_min))%256);
        //    //    LUT_B[i] = (byte)(((255 * (i - LUT_B_min)) / (LUT_B_max - LUT_B_min))%256);
        //    //}

        //    editingBitmap.ForEach((x, y, color) =>
        //    {
        //        return Color.FromArgb(color.A,
        //                             (byte)LUT_R[color.R],
        //                             (byte)LUT_G[color.G],
        //                             (byte)LUT_B[color.B]);
        //    });

        //    UpdateHistograms();
        //    parentMainPage.WritableOutputImage = editingBitmap;
        //    await parentMainPage.UpdateOutputImage();
        //}
    }

}
