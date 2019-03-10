using Microsoft.Graphics.Canvas;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Popups;
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
        private double Zoom = 1;

        public MainPage()
        {
            this.InitializeComponent();
        }

        IRandomAccessStream InputImageStream;
        CanvasVirtualBitmap InputVirtualBitmap;
        CanvasVirtualBitmapOptions InputVirtualBitmapOptions;

        IRandomAccessStream OutputImageStream;
        CanvasVirtualBitmap OutputVirtualBitmap;
        CanvasVirtualBitmapOptions OutputVirtualBitmapOptions;


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

                WriteableBitmap writeableInputImage;
                using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
                {
                    SoftwareBitmap softwareBitmap1;
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    softwareBitmap1 = await decoder.GetSoftwareBitmapAsync();
                    writeableInputImage = new WriteableBitmap(softwareBitmap1.PixelWidth, softwareBitmap1.PixelHeight);
                    writeableInputImage.SetSource(stream);
                    writeableInputImage.SetPixel(0, 0, Color.FromArgb(255, 255, 0, 0));
                    writeableInputImage.SetPixel(0, 1, Color.FromArgb(255, 255, 0, 0));
                    writeableInputImage.SetPixel(0, 2, Color.FromArgb(255, 255, 0, 0));
                    writeableInputImage.SetPixel(0, 3, Color.FromArgb(255, 255, 0, 0));
                    writeableInputImage.SetPixel(0, 4, Color.FromArgb(255, 255, 0, 0));
                    writeableInputImage.SetPixel(0, 5, Color.FromArgb(255, 255, 0, 0));

                    InputImageStream = stream;
                    InputVirtualBitmapOptions = CanvasVirtualBitmapOptions.None;

                    await LoadInputVirtualBitmap();


                    SoftwareBitmap softWriteableInputImage = SoftwareBitmap.CreateCopyFromBuffer(writeableInputImage.PixelBuffer, BitmapPixelFormat.Bgra8, writeableInputImage.PixelWidth, writeableInputImage.PixelHeight);



                    InputImageStream = await EncodedBytes(softWriteableInputImage, BitmapEncoder.PngEncoderId);
                    InputVirtualBitmapOptions = CanvasVirtualBitmapOptions.None;

                    await LoadInputVirtualBitmap();
                    

                }








                //  inputImage.Source = writeableInputImage;


                WriteableBitmap writableOutputImage;
                using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
                {
                    SoftwareBitmap softwareBitmap1;
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    softwareBitmap1 = await decoder.GetSoftwareBitmapAsync();
                    writableOutputImage = new WriteableBitmap(softwareBitmap1.PixelWidth, softwareBitmap1.PixelHeight);
                    writableOutputImage.SetSource(stream);
                }



                //outputImage.Source = writableOutputImage;
            }
            else
            {
                //Operation cancelled
            }

            //await Open();
        }

        private async Task<IRandomAccessStream> EncodedBytes(SoftwareBitmap soft, Guid encoderId)
        {

            // First: Use an encoder to copy from SoftwareBitmap to an in-mem stream (FlushAsync)
            // Next:  Use ReadAsync on the in-mem stream to get byte[] array

            var ms = new InMemoryRandomAccessStream();
            
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, ms);
                encoder.SetSoftwareBitmap(soft);

                try
                {
                    await encoder.FlushAsync();
                }   
                catch (Exception ex) {

                }


                return ms;

            
        }


        private async Task Open()
        {
            var filePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add(".bmp");
            filePicker.FileTypeFilter.Add(".gif");
            filePicker.FileTypeFilter.Add(".tif");
            filePicker.FileTypeFilter.Add(".tiff");

            var file = await filePicker.PickSingleFileAsync();

            if (file == null)
                return;

            if (InputImageStream != null)
            {
                InputImageStream.Dispose();
                InputImageStream = null;
            }

            try
            {
                InputImageStream = await file.OpenReadAsync();
                InputVirtualBitmapOptions = CanvasVirtualBitmapOptions.None;

                await LoadInputVirtualBitmap();
            }
            catch
            {
                var message = string.Format("Error opening '{0}'", file.Name);

                var messageBox = new MessageDialog(message, "Virtual Bitmap Example").ShowAsync();
            }
        }




        private async void SaveInputAsImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void SaveOutputAsImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {

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
            if (Zoom < 0.25)
                Zoom = 0.25;
            else
                Zoom -= 0.25;

            ZoomFactorTextBlock.Text = Math.Ceiling(Zoom * 100) + "%";

            //inputImageScroll.ChangeView(null, null, (float)zoom);
            InputImageCanvas.Invalidate();
            OutputImageCanvas.Invalidate();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom > 10)
                Zoom = 10;
            else
                Zoom += 0.25;

            ZoomFactorTextBlock.Text = Math.Ceiling(Zoom * 100) + "%";
            InputImageCanvas.Invalidate();
            OutputImageCanvas.Invalidate();
        }

        private void ZoomPresetMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuFlyoutItem = (MenuFlyoutItem)sender;

            if (Int32.TryParse(menuFlyoutItem.Tag.ToString(), out int x))
            {
                Zoom = x / 100;

                ZoomFactorTextBlock.Text = menuFlyoutItem.Tag + "%";

                // inputImageScroll.ChangeView(null, null, zoom);
                InputImageCanvas.Invalidate();
                OutputImageCanvas.Invalidate();
            }
        }

        private void SplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            Zoom = 1;

            ZoomFactorTextBlock.Text = "100%";
            //inputImageScroll.ChangeView(null, null, 1);
            InputImageCanvas.Invalidate();
            OutputImageCanvas.Invalidate();
        }

        private async void GetSetPixelMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            //GetSetPixelColorDialog dialog = new GetSetPixelColorDialog(InputSoftwareBitmap, OutputImagePixelDataProvider, DecoderOutputImage);
            //await dialog.ShowAsync();
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
        }

        private void InputCanvasScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            InputImageCanvas.Invalidate();
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
        }


        private async Task LoadInputVirtualBitmap()
        {
            if (InputVirtualBitmap != null)
            {
                InputVirtualBitmap.Dispose();
                InputVirtualBitmap = null;
            }

            //LoadedImageInfo = "";

            InputVirtualBitmap = await CanvasVirtualBitmap.LoadAsync(InputImageCanvas.Device, InputImageStream, InputVirtualBitmapOptions);

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

            OutputVirtualBitmap = await CanvasVirtualBitmap.LoadAsync(OutputImageCanvas.Device, OutputImageStream, OutputVirtualBitmapOptions);

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
    }
}
