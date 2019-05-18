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

            if (BinaryzationHelper.IsBinaryImage(editingBitmap))
                NextStep();
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
            KMMHelper.KMM(editingBitmap);

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
    }

}
