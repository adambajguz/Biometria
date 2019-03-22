using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageProcessor.Pages
{
    public sealed partial class PixelManagerPage : Page
    {
        private WriteableBitmap editingBitmap;

        private readonly MainPage parentMainPage;

        public PixelManagerPage()
        {
            this.InitializeComponent();

            OriginalColorPreview.Fill = new SolidColorBrush(Colors.White);
            OriginalColorPreviewTooltip.Content = "#FFFFFF";
            NewColorPreviewTooltip.Content = "#FFFFFF";

            Frame frame = (Frame)Window.Current.Content;
            parentMainPage = (MainPage)frame.Content;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) => this.editingBitmap = e.Parameter as WriteableBitmap;


        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            NewColorPreview.Fill = new SolidColorBrush(sender.Color);
            NewColorPreviewTooltip.Content = sender.Color.ToString();

            PixelColorPicker2.Color = PixelColorPicker.Color;
        }

        private void ColorPicker2_ColorChanged(ColorPicker sender, ColorChangedEventArgs args) => PixelColorPicker.Color = PixelColorPicker2.Color;

        private async void ApplyColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (Int32.TryParse(XTextBox.Text, out int x))
            {

                if (Int32.TryParse(YTextBox.Text, out int y))
                {

                    Color pixelColor = PixelColorPicker.Color;
                    editingBitmap.SetPixel(x, y, pixelColor);

                    OriginalColorPreview.Fill = new SolidColorBrush(pixelColor);

                    await parentMainPage.UpdateOutputImage();
                }
            }
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

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) => sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
    }
}
