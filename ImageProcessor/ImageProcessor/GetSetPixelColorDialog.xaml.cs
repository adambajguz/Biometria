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
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageProcessor
{
    public enum GetSetPixelColorDialogExitResult
    {
        Close,
        CloseAfterChange,
        Nothing
    }

    public sealed partial class GetSetPixelColorDialog : ContentDialog
    {
        public GetSetPixelColorDialogExitResult ExitResult { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int PixelColor { get; set; }

        private SoftwareBitmap SoftwareBitmap;

        private PixelDataProvider ImagePixelDataProvider;

        private BitmapDecoder DecoderImage;
        byte[] bytes;

        public GetSetPixelColorDialog(SoftwareBitmap bitmap, PixelDataProvider imagePixelDataProvider, BitmapDecoder decoder)
        {
            this.InitializeComponent();

            this.SoftwareBitmap = bitmap;
            this.ExitResult = GetSetPixelColorDialogExitResult.Nothing;
            OriginalColorPreview.Fill = new SolidColorBrush(Colors.White);
            OriginalColorPreviewTooltip.Content = "#FFFFFF";
            NewColorPreviewTooltip.Content = "#FFFFFF";

            ImagePixelDataProvider = imagePixelDataProvider;
            DecoderImage = decoder;

            bytes = ImagePixelDataProvider.DetachPixelData();

            //byte[] bytes = ImagePixelDataProvider.DetachPixelData();

            //Color pixel = GetPixel(bytes, 1, 1, decoder.PixelWidth, decoder.PixelHeight);
        }

        public Color GetPixel(byte[] pixels, int x, int y, uint width, uint height)
        {
            int i = x;
            int j = y;
            int k = (i * (int)width + j) * 3;
            var r = pixels[k + 0];
            var g = pixels[k + 1];
            var b = pixels[k + 2];
            return Color.FromArgb(255, r, g, b);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            X = 1;
            Y = 2;
            PixelColor = 3;

            ExitResult = GetSetPixelColorDialogExitResult.Close;
            Hide();
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            NewColorPreview.Fill = new SolidColorBrush(sender.Color);
            NewColorPreviewTooltip.Content = sender.Color.ToString();
        }

        private void ApplyColorButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {


            if (Int32.TryParse(XTextBox.Text, out int x))
            {

                if (Int32.TryParse(YTextBox.Text, out int y))
                {

                    Color pixelColor = GetPixel(bytes, x, y, DecoderImage.PixelWidth, DecoderImage.PixelHeight);
                    PixelColorPicker.Color = pixelColor;

                    OriginalColorPreview.Fill = new SolidColorBrush(pixelColor);
                    NewColorPreview.Fill = new SolidColorBrush(pixelColor);
                }
            }
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

            if (Int32.TryParse(XTextBox.Text, out int x))
            {

                if (Int32.TryParse(YTextBox.Text, out int y))
                {

                    Color pixelColor = GetPixel(bytes, x, y, DecoderImage.PixelWidth, DecoderImage.PixelHeight);
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
