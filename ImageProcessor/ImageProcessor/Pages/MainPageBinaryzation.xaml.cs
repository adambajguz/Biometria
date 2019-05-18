using System;
using System.Threading.Tasks;
using ImageProcessor.Data;
using ImageProcessor.Dialogs;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Pages
{
    public partial class MainPage
    {

        private async void ConvertToGrayScalePageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            WriteableOutputImage = BinaryzationHelper.ConvertToGrayscale(WriteableOutputImage);

            AddToUndo(WriteableOutputImage.Clone());
            await UpdateOutputImage();
        }

        private async void ManualBinaryzationPageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ManualBinaryzationDialog dialog = new ManualBinaryzationDialog();
            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Secondary)
                await ManualBinaryzation(dialog.TresholdValue);
            else
            {
                // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }
        }

        private async Task ManualBinaryzation(int threshold)
        {
            ConvertToGrayScalePageMenuFlyoutItem_Click(null, null);

            BinaryzationHelper.ManualBinaryzation(threshold, WriteableOutputImage);

            AddToUndo(WriteableOutputImage.Clone());
            await UpdateOutputImage();
        }


        private async void OtsuBinaryzationPageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertToGrayScalePageMenuFlyoutItem_Click(null, null);

            int threshold = Otsu.GetOtsuThreshold(WriteableOutputImage);

            WriteableOutputImage.ForEach((x, y, curColor) =>
            {
                if (curColor.R > threshold)
                    return Color.FromArgb(255, 255, 255, 255);

                return Color.FromArgb(255, 0, 0, 0);
            });

            AddToUndo(WriteableOutputImage.Clone());
            await UpdateOutputImage();

            ContentDialog dialog = new ContentDialog
            {
                Title = "Binaryzation",
                Content = "Otsu threshold value = " + threshold,
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await dialog.ShowAsync();
        }

        private async void NiblackinaryzationPageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

            NiblackBinaryzationDialog dialog = new NiblackBinaryzationDialog();
            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Secondary)
                await NiblackBinaryzation(dialog.SValue, dialog.KValue);
            else
            {
                // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }
        }


        private async Task NiblackBinaryzation(int size = 25, double k = 0.5)
        {
            ConvertToGrayScalePageMenuFlyoutItem_Click(null, null);

            NiblackThreshold niblack = new NiblackThreshold(size, k);
            WriteableOutputImage = niblack.Threshold(WriteableOutputImage);

            AddToUndo(WriteableOutputImage.Clone());
            await UpdateOutputImage();
        }
    }
}
