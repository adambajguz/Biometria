using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImageProcessor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                // Set additional encoding parameters, if needed
                //encoder.BitmapTransform.ScaledWidth = 320;
                //encoder.BitmapTransform.ScaledHeight = 240;
                //encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise90Degrees;
                //encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                encoder.IsThumbnailGenerated = true;

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                    switch (err.HResult)
                    {
                        case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                            // If the encoder does not support writing a thumbnail, then try again
                            // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw;
                    }
                }

                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }


            }
        }
        private StorageFile InputFileImage;

        private async void OpenImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".tif");
            picker.FileTypeFilter.Add(".tiff");

            StorageFile inputFile = await picker.PickSingleFileAsync();
            if (inputFile != null)
            {
                // Application now has read/write access to the picked file

                InputFileImage = inputFile;

                //https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/imaging
                SoftwareBitmap softwareBitmap;

                using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
                {
                    // Create the decoder from the stream
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                    // Get the SoftwareBitmap representation of the file
                    softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                }


                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                var source = new SoftwareBitmapSource();
                await source.SetBitmapAsync(softwareBitmap);
                
                // Set the source of the Image control
                inputImage.Source = source;

                ZoomFactorTextBlock.Text = inputImageScroll.ZoomFactor * 100 + "%";

                ImageFileTextBox.Text = inputFile.Path;
                ImageResolution.Text = softwareBitmap.PixelWidth + " x " + softwareBitmap.PixelHeight;
            }
            else
            {
                //Operation cancelled
            }
        }


        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            float zoom = inputImageScroll.ZoomFactor - (float)0.25;
            if (zoom < (float)0.25)
                zoom = (float)0.25;

            ZoomFactorTextBlock.Text = Math.Ceiling(zoom * 100) + "%";

            inputImageScroll.ChangeView(null, null, zoom);

        }
        private async void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            float zoom = inputImageScroll.ZoomFactor + (float)0.25;
            if (zoom > 10)
                zoom = 10;

            ZoomFactorTextBlock.Text = Math.Ceiling(zoom * 100) + "%";

            inputImageScroll.ChangeView(null, null, zoom);
        }

        private void ZoomPresetMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuFlyoutItem = (MenuFlyoutItem)sender;

            if (Int32.TryParse(menuFlyoutItem.Tag.ToString(), out int x))
            {
                float zoom = x / 100;

                ZoomFactorTextBlock.Text = menuFlyoutItem.Tag + "%";

                inputImageScroll.ChangeView(null, null, zoom);
            }
        }

    }
}
