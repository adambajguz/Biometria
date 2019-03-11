using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageProcessor
{
    public enum GetSetPixelColorDialogExitResult
    {
        Nothing,
        BitmapChanged
    }

    public sealed partial class GetSetPixelColorDialog : ContentDialog
    {
        public GetSetPixelColorDialogExitResult ExitResult { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int PixelColor { get; set; }

        public WriteableBitmap EditingBitmap;

        public GetSetPixelColorDialog(WriteableBitmap writeableBitmap)
        {
            this.InitializeComponent();

            this.EditingBitmap = writeableBitmap;
            this.ExitResult = GetSetPixelColorDialogExitResult.Nothing;

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
                    EditingBitmap.SetPixel(x, y, pixelColor);

                    OriginalColorPreview.Fill = new SolidColorBrush(pixelColor);
                }
            }

            ExitResult = GetSetPixelColorDialogExitResult.BitmapChanged;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            GetColorOnPosition();
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            GetColorOnPosition();
        }

        private void GetColorOnPosition()
        {
            if (Int32.TryParse(XTextBox.Text, out int x))
            {

                if (Int32.TryParse(YTextBox.Text, out int y))
                {

                    Color pixelColor = EditingBitmap.GetPixel(x, y);
                    PixelColorPicker.Color = pixelColor;

                    OriginalColorPreview.Fill = new SolidColorBrush(pixelColor);
                    NewColorPreview.Fill = new SolidColorBrush(pixelColor);
                }
            }
        }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
        }

        private void TextBox_TextChanging_1(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
        }


    }
}
