using System;
using ImageProcessor.Data;
using ImageProcessor.Dialogs;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Pages
{
    public partial class MainPage
    {
        private static readonly int[,] PrewittX = {
                                                    {1, 0, -1},
                                                    {1, 0, -1},
                                                    {1, 0, -1}
                                                  };
        private static readonly int[,] PrewittY = {
                                                     { 1,  1,  1},
                                                     { 0,  0,  0},
                                                     {-1, -1, -1}
                                                  };

        private static readonly int[,] SobelX = {
                                                   {-1, 0, 1},
                                                   {-2, 0, 2},
                                                   {-1, 0, 1}
                                                };
        private static readonly int[,] SobelY = {
                                                    {-1, -2, -1},
                                                    { 0,  0,  0},
                                                    { 1,  2,  1}
                                                };
        //https://homepages.inf.ed.ac.uk/rbf/HIPR2/log.htm
        private static readonly int[,] Laplace1 = {
                                                    { 0, -1,  0},
                                                    {-1,  4, -1},
                                                    { 0, -1,  0}
                                                };

        private static readonly int[,] Laplace2 = {
                                                    {-1, -1, -1},
                                                    {-1,  8, -1},
                                                    {-1, -1, -1}
                                                };


        private async void CustomPageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            CustomConvolutionFilterDialog dialog = new CustomConvolutionFilterDialog();
            ContentDialogResult result = await dialog.ShowAsync();


            if (result == ContentDialogResult.Secondary)
            {
                WriteableOutputImage = WriteableBitmapCovolute.Convolute(WriteableOutputImage, await dialog.GetKernel());

                await UpdateOutputImage();
            }
            else
            {
                // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }
        }

        private async void PrewittXPageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            WriteableOutputImage = WriteableBitmapCovolute.Convolute(WriteableOutputImage, PrewittX);

            await UpdateOutputImage();
        }

        private async void PrewittYPageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            WriteableOutputImage = WriteableBitmapCovolute.Convolute(WriteableOutputImage, PrewittY);

            await UpdateOutputImage();
        }

        private async void SobelXPageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            WriteableOutputImage = WriteableBitmapCovolute.Convolute(WriteableOutputImage, SobelX);

            await UpdateOutputImage();
        }

        private async void SobelYPageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            WriteableOutputImage = WriteableBitmapCovolute.Convolute(WriteableOutputImage, SobelY);

            await UpdateOutputImage();
        }

        private async void Laplace1PageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            WriteableOutputImage = WriteableBitmapCovolute.Convolute(WriteableOutputImage, Laplace1);

            await UpdateOutputImage();
        }

        private async void Laplace2PageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            WriteableOutputImage = WriteableBitmapCovolute.Convolute(WriteableOutputImage, Laplace2);

            await UpdateOutputImage();
        }

        private async void KuwaharaPageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            KuwaharaFilter(WriteableOutputImage, 5);

            await UpdateOutputImage();
        }

        private async void Median3x3PageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MedianFilter(WriteableOutputImage, 3);

            await UpdateOutputImage();
        }

        private async void Medain5x5PageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MedianFilter(WriteableOutputImage, 5);

            await UpdateOutputImage();
        }

        public void MedianFilter(WriteableBitmap Input, int maskSize)
        {
            int width = Input.PixelWidth;
            int height = Input.PixelHeight;

            WriteableBitmap originalImage = Input.Clone();

            using (BitmapContext contextOriginal = originalImage.GetBitmapContext())
            {
                using (BitmapContext context = Input.GetBitmapContext())
                {

                    // red, green and blue are a 2D square of odd size like 3x3, 5x5, 7x7, ... For simplicity stored it into 1D array. 
                    byte[] red, green, blue;

                    /** Median Filter operation */
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            byte a = PixelHelper.GetPixel(contextOriginal, x, y).A;
                            red = new byte[maskSize * maskSize];
                            green = new byte[maskSize * maskSize];
                            blue = new byte[maskSize * maskSize];
                            int count = 0;
                            for (int r = y - (maskSize / 2); r <= y + (maskSize / 2); r++)
                            {
                                for (int c = x - (maskSize / 2); c <= x + (maskSize / 2); c++)
                                {
                                    if (r < 0 || r >= height || c < 0 || c >= width)
                                    {
                                        /** Some portion of the mask is outside the image. */
                                        continue;
                                    }
                                    else
                                    {
                                        Color tmp = PixelHelper.GetPixel(contextOriginal, c, r);
                                        red[count] = tmp.R;
                                        green[count] = tmp.G;
                                        blue[count] = tmp.B;
                                        ++count;
                                    }
                                }
                            }
                            Array.Sort(red);
                            Array.Sort(green);
                            Array.Sort(blue);

                            int index = (count % 2 == 0) ? count / 2 - 1 : count / 2;
                            PixelHelper.SetPixel(context, x, y, a, red[index], green[index], blue[index]);
                        }
                    }

                }
            }

        }

        public void KuwaharaFilter(WriteableBitmap Image, int size)
        {
            int width = Image.PixelWidth;
            int height = Image.PixelHeight;

            WriteableBitmap originalImage = Image.Clone();

            using (BitmapContext contextOriginal = originalImage.GetBitmapContext())
            {
                using (BitmapContext context = Image.GetBitmapContext())
                {

                    int[] ApetureMinX = { -(size / 2), 0, -(size / 2), 0 };
                    int[] ApetureMaxX = { 0, (size / 2), 0, (size / 2) };
                    int[] ApetureMinY = { -(size / 2), -(size / 2), 0, 0 };
                    int[] ApetureMaxY = { 0, 0, (size / 2), (size / 2) };

                    for (int x = 0; x < width; ++x)
                    {
                        for (int y = 0; y < height; ++y)
                        {
                            int[] RValues = { 0, 0, 0, 0 };
                            int[] GValues = { 0, 0, 0, 0 };
                            int[] BValues = { 0, 0, 0, 0 };
                            int[] NumPixels = { 0, 0, 0, 0 };
                            int[] MaxRValue = { 0, 0, 0, 0 };
                            int[] MaxGValue = { 0, 0, 0, 0 };
                            int[] MaxBValue = { 0, 0, 0, 0 };
                            int[] MinRValue = { 255, 255, 255, 255 };
                            int[] MinGValue = { 255, 255, 255, 255 };
                            int[] MinBValue = { 255, 255, 255, 255 };
                            for (int i = 0; i < 4; ++i)
                            {
                                for (int x2 = ApetureMinX[i]; x2 < ApetureMaxX[i]; ++x2)
                                {
                                    int TempX = x + x2;
                                    if (TempX >= 0 && TempX < width)
                                    {
                                        for (int y2 = ApetureMinY[i]; y2 < ApetureMaxY[i]; ++y2)
                                        {
                                            int TempY = y + y2;
                                            if (TempY >= 0 && TempY < height)
                                            {
                                                Color TempColor = PixelHelper.GetPixel(contextOriginal, TempX, TempY);
                                                RValues[i] += TempColor.R;
                                                GValues[i] += TempColor.G;
                                                BValues[i] += TempColor.B;

                                                if (TempColor.R > MaxRValue[i])
                                                    MaxRValue[i] = TempColor.R;
                                                else if (TempColor.R < MinRValue[i])
                                                    MinRValue[i] = TempColor.R;

                                                if (TempColor.G > MaxGValue[i])
                                                    MaxGValue[i] = TempColor.G;
                                                else if (TempColor.G < MinGValue[i])
                                                    MinGValue[i] = TempColor.G;

                                                if (TempColor.B > MaxBValue[i])
                                                    MaxBValue[i] = TempColor.B;
                                                else if (TempColor.B < MinBValue[i])
                                                    MinBValue[i] = TempColor.B;

                                                ++NumPixels[i];
                                            }
                                        }
                                    }
                                }
                            }
                            int j = 0;
                            int MinDifference = 10000;
                            for (int i = 0; i < 4; ++i)
                            {
                                int CurrentDifference = (MaxRValue[i] - MinRValue[i]) + (MaxGValue[i] - MinGValue[i]) + (MaxBValue[i] - MinBValue[i]);
                                if (CurrentDifference < MinDifference && NumPixels[i] > 0)
                                {
                                    j = i;
                                    MinDifference = CurrentDifference;
                                }
                            }

                            PixelHelper.SetPixel(context, x, y, (byte)(RValues[j] / NumPixels[j]), (byte)(GValues[j] / NumPixels[j]), (byte)(BValues[j] / NumPixels[j]));
                        }
                    }
                }
            }
        }
    }
}
