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

        private async void IntensityButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] LUT = new byte[256];


            if (Int32.TryParse(IntensityR.Text, out int b))
            {
                for (int i = 0; i < 256; ++i)
                {
                    if ((b + i) > 255)
                    {
                        LUT[i] = 255;
                    }
                    else if ((b + i) < 0)
                    {
                        LUT[i] = 0;
                    }
                    else
                    {
                        LUT[i] = (byte)(b + i);
                    }
                }

                editingBitmap.ForEach((x, y, color) =>
                {
                    return Color.FromArgb(color.A,
                                         LUT[color.R],
                                         LUT[color.G],
                                         LUT[color.B]);
                });

                UpdateHistograms();
                parentMainPage.WriteableOutputImage = editingBitmap;
                await parentMainPage.UpdateOutputImage();
            }
        }

        private async void LightenButton_Click(object sender, RoutedEventArgs e)
        {
            double cR = (double)0, cG = (double)0, cB = (double)0;

            for (var i = 0; i < 256; i++)
            {
                if (bitmapHistogramData.R[i] > 0)
                    cR = i;

                if (bitmapHistogramData.G[i] > 0)
                    cG = i;

                if (bitmapHistogramData.B[i] > 0)
                    cB = i;
            }

            cR = 255 / (Math.Log(1 + cR));
            cG = 255 / (Math.Log(1 + cG));
            cB = 255 / (Math.Log(1 + cB));

            editingBitmap.ForEach((x, y, color) =>
            {
                return Color.FromArgb(color.A,
                                     (byte)(Math.Log(1 + color.R) * cR),
                                     (byte)(Math.Log(1 + color.G) * cG),
                                     (byte)(Math.Log(1 + color.B) * cB));
            });




            UpdateHistograms();
            parentMainPage.WriteableOutputImage = editingBitmap;
            await parentMainPage.UpdateOutputImage();
        }


        private async void DarkenButton_Click(object sender, RoutedEventArgs e)
        {
            double cR = (double)0, cG = (double)0, cB = (double)0;

            //for (var i = 0; i < 256; i++)
            //{
            //    if (bitmapHistogramData.R[i] > 0)
            //        cR = i;

            //    if (bitmapHistogramData.G[i] > 0)
            //        cG = i;

            //    if (bitmapHistogramData.B[i] > 0)
            //        cB = i;
            //}

            //cR = 255 / (Math.Log(1 + cR));
            //cG = 255 / (Math.Log(1 + cG));
            //cB = 255 / (Math.Log(1 + cB));

            cR = cG = cB = 20;

            double r = 1.01;
            editingBitmap.ForEach((x, y, color) =>
            {
                return Color.FromArgb(color.A,
                                     (byte)((Math.Pow(r, color.R) - 1) * cR),
                                     (byte)((Math.Pow(r, color.G) - 1) * cG),
                                     (byte)((Math.Pow(r, color.B) - 1) * cB));

                //return Color.FromArgb(color.A,
                //                     (byte)((Math.Pow(color.R, 2) + 1) * cR),
                //                     (byte)((Math.Pow(color.G, 2) + 1) * cG),
                //                     (byte)((Math.Pow(color.B, 2) + 1) * cB));
            });

            UpdateHistograms();
            parentMainPage.WriteableOutputImage = editingBitmap;
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
                    parentMainPage.WriteableOutputImage = editingBitmap;
                    await parentMainPage.UpdateOutputImage();
                }
            }
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
            parentMainPage.WriteableOutputImage = editingBitmap;
            await parentMainPage.UpdateOutputImage();
        }


    }

}
