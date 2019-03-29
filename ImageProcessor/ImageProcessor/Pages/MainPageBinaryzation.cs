using System;
using System.Threading.Tasks;
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
            using (var context = WriteableOutputImage.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                var nWidth = context.Width;
                var nHeight = context.Height;
                var px = context.Pixels;
                var result = BitmapFactory.New(nWidth, nHeight);

                using (var dest = result.GetBitmapContext())
                {
                    var rp = dest.Pixels;
                    var len = context.Length;
                    for (var i = 0; i < len; i++)
                    {
                        // Extract
                        var c = px[i];
                        var a = (c >> 24) & 0x000000FF;
                        var r = (c >> 16) & 0x000000FF;
                        var g = (c >> 8) & 0x000000FF;
                        var b = (c) & 0x000000FF;

                        // Convert to gray with constant factors 0.2126, 0.7152, 0.0722
                        var gray = (r * 6966 + g * 23436 + b * 2366) >> 15;
                        r = g = b = gray;

                        // Set
                        rp[i] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }
                WriteableOutputImage = result;
            }

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
            WriteableBitmap tmpBitmap = WriteableOutputImage.Clone();


            tmpBitmap.ForEach((x, y, curColor) =>
            {
                if (curColor.R != curColor.G || curColor.R != curColor.B || curColor.G != curColor.B)
                    ConvertToGrayScalePageMenuFlyoutItem_Click(null, null);

                if (curColor.R > threshold)
                    return Color.FromArgb(0, 255, 255, 255);

                return Color.FromArgb(0, 0, 0, 0);
            });

            WriteableOutputImage = tmpBitmap;
            await UpdateOutputImage();
        }
    }
}
