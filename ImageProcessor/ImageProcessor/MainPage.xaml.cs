using System;
using System.Collections.Generic;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
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
        private SoftwareBitmap InputSoftwareBitmap;

        private SoftwareBitmap OutputSoftwareBitmap;
        private PixelDataProvider OutputImagePixelDataProvider;
        private BitmapDecoder DecoderOutputImage;

        public MainPage()
        {
            this.InitializeComponent();
        }


        private async void OpenImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
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

                //https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/imaging
                SoftwareBitmap softwareBitmap;
                SoftwareBitmap softwareBitmapOutput;

                using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
                {
                    {
                        // Create the decoder from the stream
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                        // Get the SoftwareBitmap representation of the file
                        softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                        InputSoftwareBitmap = await decoder.GetSoftwareBitmapAsync();
                    }

                    {
                        DecoderOutputImage = await BitmapDecoder.CreateAsync(stream);

                        // Get the SoftwareBitmap representation of the file
                        softwareBitmapOutput = await DecoderOutputImage.GetSoftwareBitmapAsync();
                        OutputSoftwareBitmap = await DecoderOutputImage.GetSoftwareBitmapAsync();




                        OutputImagePixelDataProvider = await DecoderOutputImage.GetPixelDataAsync();

                    }
                }

                {
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

                {

                    if (softwareBitmapOutput.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmapOutput.BitmapAlphaMode == BitmapAlphaMode.Straight)
                    {
                        softwareBitmapOutput = SoftwareBitmap.Convert(softwareBitmapOutput, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);


                        var source = new SoftwareBitmapSource();
                        await source.SetBitmapAsync(softwareBitmap);

                        // Set the source of the Image control
                        outputImage.Source = source;
                    }
                }
            }
            else
            {
                //Operation cancelled
            }
        }

        private async void SaveInputAsImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

            FileSavePicker savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add(".jpg", new List<string>() { ".jpg" });
            savePicker.FileTypeChoices.Add(".jpeg", new List<string>() { ".jpeg" });
            savePicker.FileTypeChoices.Add(".png", new List<string>() { ".png" });
            savePicker.FileTypeChoices.Add(".bmp", new List<string>() { ".bmp" });
            savePicker.FileTypeChoices.Add(".gif", new List<string>() { ".gif" });
            savePicker.FileTypeChoices.Add(".tiff", new List<string>() { ".tiff" });
            savePicker.FileTypeChoices.Add(".tif", new List<string>() { ".tif" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New Document";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                // write to file
                SaveSoftwareBitmapToFile(OutputSoftwareBitmap, file);



                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    //OutputTextBlock.Text = "File " + file.Name + " was saved.";
                }
                else
                {
                    //OutputTextBlock.Text = "File " + file.Name + " couldn't be saved.";
                }
            }
            else
            {
                //OutputTextBlock.Text = "Operation cancelled.";
            }

        }

        private async void SaveOutputAsImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

            FileSavePicker savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add(".jpg", new List<string>() { ".jpg" });
            savePicker.FileTypeChoices.Add(".jpeg", new List<string>() { ".jpeg" });
            savePicker.FileTypeChoices.Add(".png", new List<string>() { ".png" });
            savePicker.FileTypeChoices.Add(".bmp", new List<string>() { ".bmp" });
            savePicker.FileTypeChoices.Add(".gif", new List<string>() { ".gif" });
            savePicker.FileTypeChoices.Add(".tiff", new List<string>() { ".tiff" });
            savePicker.FileTypeChoices.Add(".tif", new List<string>() { ".tif" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New Document";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                // write to file
                SaveSoftwareBitmapToFile(InputSoftwareBitmap, file);



                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    //OutputTextBlock.Text = "File " + file.Name + " was saved.";
                }
                else
                {
                    //OutputTextBlock.Text = "File " + file.Name + " couldn't be saved.";
                }
            }
            else
            {
                //OutputTextBlock.Text = "Operation cancelled.";
            }

        }

        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            Guid? encoderType = FileTypeExtensionToBitmapEncoder(outputFile.FileType);

            if (encoderType == null)
                return;

            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync((Guid)encoderType, stream);

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

        private Guid? FileTypeExtensionToBitmapEncoder(string extension)
        {
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return BitmapEncoder.JpegEncoderId;

                case ".png":
                    return BitmapEncoder.PngEncoderId;

                case ".bmp":
                    return BitmapEncoder.BmpEncoderId;

                case ".gif":
                    return BitmapEncoder.GifEncoderId;

                case ".tiff":
                case ".tif":
                    return BitmapEncoder.TiffEncoderId;
            }

            return null;
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            double zoom = inputImageScroll.ZoomFactor - 0.25;
            if (zoom < 0.25)
                zoom = 0.25;

            ZoomFactorTextBlock.Text = Math.Ceiling(zoom * 100) + "%";

            inputImageScroll.ChangeView(null, null, (float)zoom);
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            double zoom = inputImageScroll.ZoomFactor + 0.25;
            if (zoom > 10)
                zoom = 10;

            ZoomFactorTextBlock.Text = Math.Ceiling(zoom * 100) + "%";

            inputImageScroll.ChangeView(null, null, (float)zoom);
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

        private void SplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            ZoomFactorTextBlock.Text = "100%";

            inputImageScroll.ChangeView(null, null, 1);
        }

        private async void GetSetPixelMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            GetSetPixelColorDialog dialog = new GetSetPixelColorDialog(InputSoftwareBitmap, OutputImagePixelDataProvider, DecoderOutputImage);
            await dialog.ShowAsync();
        }

        private async void AboutMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog aboutDialog = new ContentDialog
            {
                Title = "ImageProcessor",
                Content = "Author: Adam Bajguz",
                CloseButtonText = "Close"
            };

            await aboutDialog.ShowAsync();
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InputImageScroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            outputImageScroll.ChangeView(inputImageScroll.HorizontalOffset, inputImageScroll.VerticalOffset, inputImageScroll.ZoomFactor);
        }
    }
}
