using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageProcessor.Dialogs
{
    public enum PixelManagerDialogExitResult
    {
        Nothing,
        BitmapChanged
    }

    public sealed partial class PixelManagerDialog : ContentDialog
    {
        public PixelManagerDialogExitResult ExitResult { get; set; }

        private WriteableBitmap editingBitmap;

        public PixelManagerDialog(WriteableBitmap writeableBitmap)
        {
            this.InitializeComponent();

            this.editingBitmap = writeableBitmap;
            this.ExitResult = PixelManagerDialogExitResult.Nothing;

            OriginalColorPreview.Fill = new SolidColorBrush(Colors.White);
            OriginalColorPreviewTooltip.Content = "#FFFFFF";
            NewColorPreviewTooltip.Content = "#FFFFFF";
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            NewColorPreview.Fill = new SolidColorBrush(sender.Color);
            NewColorPreviewTooltip.Content = sender.Color.ToString();
        }

        private void ApplyColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (Int32.TryParse(XTextBox.Text, out int x))
            {

                if (Int32.TryParse(YTextBox.Text, out int y))
                {

                    Color pixelColor = PixelColorPicker.Color;
                    editingBitmap.SetPixel(x, y, pixelColor);

                    OriginalColorPreview.Fill = new SolidColorBrush(pixelColor);
                }
            }

            ExitResult = PixelManagerDialogExitResult.BitmapChanged;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Int32.TryParse(XTextBox.Text, out int x))
            {

                if (Int32.TryParse(YTextBox.Text, out int y))
                {

                    Color pixelColor = editingBitmap.GetPixel(x, y);
                    PixelColorPicker.Color = pixelColor;

                    OriginalColorPreview.Fill = new SolidColorBrush(pixelColor);
                    NewColorPreview.Fill = new SolidColorBrush(pixelColor);

                    ApplyColorButton.IsEnabled = true;

                    return;
                }
            }

            ApplyColorButton.IsEnabled = false;
        }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
        }
    }
}
