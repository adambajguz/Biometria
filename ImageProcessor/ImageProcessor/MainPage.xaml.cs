using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
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
        private double zoom = 1;

        public double Zoom
        {
            get
            {
                return zoom;
            }
            set
            {
                if (value <= 10 && value >= 0.25)
                    zoom = value;
                else
                    return;

                ZoomFactorTextBlock.Text = (zoom * 100) + "%";

                OutputCanvasScrollViewer.ChangeView(InputCanvasScrollViewer.HorizontalOffset, InputCanvasScrollViewer.VerticalOffset, null);
                InputImageCanvas.Invalidate();
                OutputImageCanvas.Invalidate();

                var size = InputVirtualBitmap.Size;
                InputImageCanvas.Width = size.Width * (zoom + 1);
                InputImageCanvas.Height = size.Height * (zoom + 1);
                OutputImageCanvas.Width = size.Width * (zoom + 1);
                OutputImageCanvas.Height = size.Height * (zoom + 1);
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        IRandomAccessStream InputImageStream;
        CanvasVirtualBitmap InputVirtualBitmap;

        IRandomAccessStream OutputImageStream;
        CanvasVirtualBitmap OutputVirtualBitmap;
        WriteableBitmap WritableOutputImage;

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
                if (InputImageStream != null)
                {
                    InputImageStream.Dispose();
                    InputImageStream = null;
                }

                using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
                {
                    WriteableBitmap writeableInputImage;

                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    SoftwareBitmap softwareBitmap1 = await decoder.GetSoftwareBitmapAsync();
                    ImageResolution.Text = softwareBitmap1.PixelWidth + " x " + softwareBitmap1.PixelHeight;

                    writeableInputImage = new WriteableBitmap(softwareBitmap1.PixelWidth, softwareBitmap1.PixelHeight);
                    writeableInputImage.SetSource(stream);

                    InputImageStream = stream;

                    await LoadInputVirtualBitmap();
                }

                //  inputImage.Source = writeableInputImage;


                using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    SoftwareBitmap softwareBitmap1 = await decoder.GetSoftwareBitmapAsync();
                    WritableOutputImage = new WriteableBitmap(softwareBitmap1.PixelWidth, softwareBitmap1.PixelHeight);
                    WritableOutputImage.SetSource(stream);

                    await UpdateOutputImage();
                }

                //outputImage.Source = writableOutputImage;
            }
            else
            {
                //Operation cancelled
            }

            //await Open();
        }

        private async Task<IRandomAccessStream> GetRandomAccessStreamFromSoftwareBitmap(SoftwareBitmap soft, Guid encoderId)
        {
            // Use an encoder to copy from SoftwareBitmap to an in-mem stream (FlushAsync)

            InMemoryRandomAccessStream inMemoryStream = new InMemoryRandomAccessStream();

            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, inMemoryStream);
            encoder.SetSoftwareBitmap(soft);

            await encoder.FlushAsync();

            return inMemoryStream;
        }

        private async Task UpdateOutputImage()
        {
            SoftwareBitmap softWriteableOutputImage = SoftwareBitmap.CreateCopyFromBuffer(WritableOutputImage.PixelBuffer, BitmapPixelFormat.Bgra8, WritableOutputImage.PixelWidth, WritableOutputImage.PixelHeight);

            OutputImageStream = await GetRandomAccessStreamFromSoftwareBitmap(softWriteableOutputImage, BitmapEncoder.PngEncoderId);

            await LoadOutputVirtualBitmap();
        }

        private async void SaveInputAsImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

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

                SoftwareBitmap softWriteableOutputImage = SoftwareBitmap.CreateCopyFromBuffer(WritableOutputImage.PixelBuffer, BitmapPixelFormat.Bgra8, WritableOutputImage.PixelWidth, WritableOutputImage.PixelHeight);

                SaveSoftwareBitmapToFile(softWriteableOutputImage, file);



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
            Zoom -= 0.25;
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            Zoom += 0.25;
        }

        private void ZoomPresetMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuFlyoutItem = (MenuFlyoutItem)sender;

            if (Int32.TryParse(menuFlyoutItem.Tag.ToString(), out int x))
            {
                Zoom = (double)x / 100;
            }
        }

        private void SplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            Zoom = 1;
        }


        private async void OpenPixelManagerMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            PixelManagerDialog dialog = new PixelManagerDialog(WritableOutputImage);
            await dialog.ShowAsync();

            if (dialog.ExitResult == PixelManagerDialogExitResult.BitmapChanged)
            {
                await UpdateOutputImage();
            }
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


        private void InputCanvasScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            InputImageCanvas.Invalidate();
            OutputCanvasScrollViewer.ChangeView(InputCanvasScrollViewer.HorizontalOffset, InputCanvasScrollViewer.VerticalOffset, null);
        }

        private void InputCanvasScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            InputImageCanvas.Invalidate();
            OutputCanvasScrollViewer.ChangeView(InputCanvasScrollViewer.HorizontalOffset, InputCanvasScrollViewer.VerticalOffset, null);
        }

        private void OutputCanvasScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            OutputImageCanvas.Invalidate();
        }

        private void OutputCanvasScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            OutputImageCanvas.Invalidate();
        }

        private void InputImageCanvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasVirtualControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            if (InputImageStream != null)
            {
                args.TrackAsyncAction(LoadInputVirtualBitmap().AsAsyncAction());
            }
        }

        private void InputImageCanvas_RegionsInvalidated(Microsoft.Graphics.Canvas.UI.Xaml.CanvasVirtualControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasRegionsInvalidatedEventArgs args)
        {
            foreach (var region in args.InvalidatedRegions)
            {
                using (var ds = InputImageCanvas.CreateDrawingSession(region))
                {
                    if (InputVirtualBitmap != null)
                        ds.DrawImage(InputVirtualBitmap, new Rect(0, 0, region.Width * Zoom, region.Height * Zoom), new Rect(0, 0, region.Width, region.Height), 1.0f, CanvasImageInterpolation.NearestNeighbor);
                }
            }
        }

        private void OutputImageCanvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasVirtualControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            if (InputImageStream != null)
            {
                args.TrackAsyncAction(LoadOutputVirtualBitmap().AsAsyncAction());
            }
        }

        private void OutputImageCanvas_RegionsInvalidated(Microsoft.Graphics.Canvas.UI.Xaml.CanvasVirtualControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasRegionsInvalidatedEventArgs args)
        {
            foreach (var region in args.InvalidatedRegions)
            {
                using (var ds = OutputImageCanvas.CreateDrawingSession(region))
                {
                    if (OutputVirtualBitmap != null)
                        ds.DrawImage(OutputVirtualBitmap, new Rect(0, 0, region.Width * Zoom, region.Height * Zoom), new Rect(0, 0, region.Width, region.Height), 1.0f, CanvasImageInterpolation.NearestNeighbor);
                }
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            InputImageCanvas.RemoveFromVisualTree();
            OutputImageCanvas.RemoveFromVisualTree();

            InputImageCanvas = null;
            OutputImageCanvas = null;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InputImageCanvas.Invalidate();
            OutputImageCanvas.Invalidate();
            InputCanvasScrollViewer.MaxWidth = double.MaxValue;
            InputCanvasScrollViewer.MaxHeight = double.MaxValue;

            OutputCanvasScrollViewer.MaxWidth = double.MaxValue;
            OutputCanvasScrollViewer.MaxHeight = double.MaxValue;
        }


        private async Task LoadInputVirtualBitmap()
        {
            if (InputVirtualBitmap != null)
            {
                InputVirtualBitmap.Dispose();
                InputVirtualBitmap = null;
            }

            //LoadedImageInfo = "";

            InputVirtualBitmap = await CanvasVirtualBitmap.LoadAsync(InputImageCanvas.Device, InputImageStream, CanvasVirtualBitmapOptions.None);

            if (InputImageCanvas == null)
            {
                // This can happen if the page is unloaded while LoadAsync is running
                return;
            }

            var size = InputVirtualBitmap.Size;
            InputImageCanvas.Width = size.Width * (Zoom + 1);
            InputImageCanvas.Height = size.Height * (Zoom + 1);
            InputImageCanvas.Invalidate();

            //LoadedImageInfo = string.Format("{0}x{1} image, is {2}CachedOnDemand", size.Width, size.Height, virtualBitmap.IsCachedOnDemand ? "" : "not ");
        }

        private async Task LoadOutputVirtualBitmap()
        {
            if (OutputVirtualBitmap != null)
            {
                OutputVirtualBitmap.Dispose();
                OutputVirtualBitmap = null;
            }

            //LoadedImageInfo = "";

            OutputVirtualBitmap = await CanvasVirtualBitmap.LoadAsync(OutputImageCanvas.Device, OutputImageStream, CanvasVirtualBitmapOptions.None);

            if (OutputImageCanvas == null)
            {
                // This can happen if the page is unloaded while LoadAsync is running
                return;
            }

            var size = OutputVirtualBitmap.Size;
            OutputImageCanvas.Width = size.Width * (Zoom + 1);
            OutputImageCanvas.Height = size.Height * (Zoom + 1);
            OutputImageCanvas.Invalidate();

            //LoadedImageInfo = string.Format("{0}x{1} image, is {2}CachedOnDemand", size.Width, size.Height, virtualBitmap.IsCachedOnDemand ? "" : "not ");
        }

        private void ExitMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }
    }
}
