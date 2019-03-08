using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
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

        public int Color { get; set; }

        private SoftwareBitmap SoftwareBitmap;

        public GetSetPixelColorDialog(SoftwareBitmap bitmap)
        {
            this.InitializeComponent();

            this.SoftwareBitmap = bitmap;
            this.ExitResult = GetSetPixelColorDialogExitResult.Nothing;
            OriginalColorPreview.Fill = new SolidColorBrush(Windows.UI.Colors.SteelBlue);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            X = 1;
            Y = 2;
            Color = 3;

            ExitResult = GetSetPixelColorDialogExitResult.Close;
            Hide();
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            NewColorPreview.Fill = new SolidColorBrush(sender.Color);
        }

        private void ApplyColorButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
