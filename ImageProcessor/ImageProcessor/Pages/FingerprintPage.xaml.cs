using System;
using System.Linq;
using ImageProcessor.Data;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageProcessor.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FingerprintPage : Page
    {
        public int ThresholdValue
        {
            get => (int)SliderValue.Value;
            set => SliderValue.Value = (double)value;
        }

        public int ThresholdValueNiblackS
        {
            get => (int)SliderValue2.Value;
            set => SliderValue2.Value = (double)value;
        }

        public int ThresholdValueNiblackK
        {
            get => (int)SliderValue3.Value;
            set => SliderValue3.Value = (double)value;
        }

        private WriteableBitmap editingBitmap;

        private readonly MainPage parentMainPage;


        public FingerprintPage()
        {
            this.InitializeComponent();

            Frame frame = (Frame)Window.Current.Content;
            parentMainPage = (MainPage)frame.Content;

            SliderValue3.ValueChanged += SliderValue3_ValueChanged;
            SliderValue3_ValueChanged(null, null);
            NextStep();
            DataContext = this;
        }

        private void SliderValue3_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e) => SliderValue3Text.Text = Math.Round(SliderValue3.Value, 2).ToString();

        private int StepCounter = 0;
        private void NextStep()
        {
            if (StepCounter == 0)
            {
                ApplySkeletonization.IsEnabled = false;
                DetectMinutia.IsEnabled = false;
                FilterMinutia.IsEnabled = false;
            }
            else if (StepCounter == 1)
            {
                ApplySkeletonization.IsEnabled = true;

                GetOtsuThreshold.IsEnabled = false;
                ApplyManualBinaryzation.IsEnabled = false;
                ApplyNiblack.IsEnabled = false;
                SliderValue.IsEnabled = false;
                SliderValue2.IsEnabled = false;
                SliderValue3.IsEnabled = false;
            }
            else if (StepCounter == 2)
            {
                ApplySkeletonization.IsEnabled = false;
                DetectMinutia.IsEnabled = true;
            }
            else if (StepCounter == 3)
            {
                DetectMinutia.IsEnabled = false;
                FilterMinutia.IsEnabled = true;
            }
            else if (StepCounter == 4)
            {
                FilterMinutia.IsEnabled = false;
            }

            StepCounter++;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.editingBitmap = e.Parameter as WriteableBitmap;
        }

        private void GetOtsuThreshold_Click(object sender, RoutedEventArgs e)
        {
            var gray = BinaryzationHelper.ConvertToGrayscale(editingBitmap.Clone());

            ThresholdValue = Otsu.GetOtsuThreshold(gray);
        }

        private async void ApplyManualBinaryzation_Click(object sender, RoutedEventArgs e)
        {
            BinaryzationHelper.ConvertToGrayscale(editingBitmap);

            parentMainPage.WriteableOutputImage = editingBitmap;
            await parentMainPage.UpdateOutputImage();

            BinaryzationHelper.ManualBinaryzation(ThresholdValue, editingBitmap);

            parentMainPage.WriteableOutputImage = editingBitmap;
            await parentMainPage.UpdateOutputImage();
            NextStep();
        }

        private async void ApplyNiblack_Click(object sender, RoutedEventArgs e)
        {
            BinaryzationHelper.NiblackBinaryzation(editingBitmap, ThresholdValueNiblackS, ThresholdValueNiblackK);

            parentMainPage.WriteableOutputImage = editingBitmap;
            await parentMainPage.UpdateOutputImage();
            NextStep();
        }

        private async void ApplySkeletonization_Click(object sender, RoutedEventArgs e)
        {
            int width = editingBitmap.PixelWidth;
            int height = editingBitmap.PixelHeight;

            // jesli nie potrzebujemy robić GetPixel na oryginalnych pixelach a tylko na zmienionych to originalImage i contextOriginal sa niepotrzebne
            WriteableBitmap originalImage = editingBitmap.Clone();

            using (BitmapContext contextOriginal = originalImage.GetBitmapContext())
            using (BitmapContext context = editingBitmap.GetBitmapContext())
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color pixel = GetPixel(contextOriginal, x, y);
                        pixel.G = 0;
                        pixel.B = 0;

                        SetPixel(context, x, y, pixel);
                    }
                }

                KMMHelper kmmHelper = new KMMHelper();

                int[,] pixels = new int[width, height];
                pixels = kmmHelper.PixelInfo(context, pixels, width, height);
                int[,] pixelsWeights = new int[width, height];

                bool change = false;
                do
                {
                    pixels = kmmHelper.Mark_2s(pixels, width, height);
                    pixels = kmmHelper.Mark_3s(pixels, width, height);
                    pixelsWeights = kmmHelper.CalculateWeights(context, pixels, pixelsWeights, width, height);

                    for (int i = 0; i < width; i++) //delete '4's
                    {
                        for (int j = 0; j < height; j++)
                        {
                            if (pixels[i, j] == 4)
                            {
                                if (kmmHelper.A.Contains(pixelsWeights[i, j]))
                                {
                                    pixels[i, j] = 0;
                                    SetPixel(context, i, j, Colors.White);
                                    change = true;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < width; i++) //delete not needed '2's
                    {
                        for (int j = 0; j < height; j++)
                        {
                            if (pixels[i, j] == 2)
                            {
                                if (kmmHelper.A.Contains(kmmHelper.CalculateWeight(i, j, context, width, height)))
                                {
                                    pixels[i, j] = 0;
                                    SetPixel(context, i, j, Colors.White);
                                    change = true;
                                }
                                else
                                {
                                    pixels[i, j] = 1;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < width; i++) //delete not needed '3's
                    {
                        for (int j = 0; j < height; j++)
                        {
                            if (pixels[i, j] == 3)
                            {
                                if (kmmHelper.A.Contains(kmmHelper.CalculateWeight(i, j, context, width, height)))
                                {
                                    pixels[i, j] = 0;
                                    SetPixel(context, i, j, Colors.White);
                                    change = true;
                                }
                                else
                                {
                                    pixels[i, j] = 1;
                                }
                            }
                        }
                    }
                } while (change);
            }

            parentMainPage.WriteableOutputImage = editingBitmap;
            await parentMainPage.UpdateOutputImage();
            NextStep();
        }




        private async void DetectMinutia_Click(object sender, RoutedEventArgs e)
        {


            parentMainPage.WriteableOutputImage = editingBitmap;
            await parentMainPage.UpdateOutputImage();
            NextStep();
        }

        private async void FilterMinutia_Click(object sender, RoutedEventArgs e)
        {


            parentMainPage.WriteableOutputImage = editingBitmap;
            await parentMainPage.UpdateOutputImage();
            NextStep();
        }

        public static void SetPixel(BitmapContext context, int x, int y, Color color) => context.Pixels[y * context.Width + x] = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
        public static void SetPixel(BitmapContext context, int x, int y, byte a, byte r, byte g, byte b) => context.Pixels[y * context.Width + x] = (a << 24) | (r << 16) | (g << 8) | b;
        public static void SetPixel(BitmapContext context, int x, int y, byte r, byte g, byte b) => context.Pixels[y * context.Width + x] = (255 << 24) | (r << 16) | (g << 8) | b;

        public static Color GetPixel(BitmapContext context, int x, int y)
        {
            var c = context.Pixels[y * context.Width + x];
            var a = (byte)(c >> 24);

            // Prevent division by zero
            int ai = a;
            if (ai == 0)
            {
                ai = 1;
            }

            // Scale inverse alpha to use cheap integer mul bit shift
            ai = ((255 << 8) / ai);
            return Color.FromArgb(a,
                                 (byte)((((c >> 16) & 0xFF) * ai) >> 8),
                                 (byte)((((c >> 8) & 0xFF) * ai) >> 8),
                                 (byte)((((c & 0xFF) * ai) >> 8)));
        }
    }

}
