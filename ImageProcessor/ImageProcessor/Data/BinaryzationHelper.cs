using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Data
{
    public static class BinaryzationHelper
    {
        public static bool IsBinaryImage(WriteableBitmap bitmap)
        {
            using (var context = bitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                var px = context.Pixels;
                var len = context.Length;
                for (var i = 0; i < len; ++i)
                {
                    // Extract
                    var c = px[i];
                    var a = (c >> 24) & 0x000000FF;
                    var r = (c >> 16) & 0x000000FF;
                    var g = (c >> 8) & 0x000000FF;
                    var b = (c) & 0x000000FF;

                    if (r != g || g != b || r != b || r != 0 && r != 255)
                        return false;
                }
            }

            return true;
        }

        public static WriteableBitmap ConvertToGrayscale(WriteableBitmap WriteableOutputImage)
        {
            using (var context = WriteableOutputImage.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                var nWidth = context.Width;
                var nHeight = context.Height;
                var px = context.Pixels;
                var result = BitmapFactory.New(nWidth, nHeight);

                using (var dest = result.GetBitmapContext())
                {
                    var rp = dest.Pixels;
                    var len = context.Length;
                    for (var i = 0; i < len; ++i)
                    {
                        // Extract
                        var c = px[i];
                        var a = (c >> 24) & 0x000000FF;
                        var r = (c >> 16) & 0x000000FF;
                        var g = (c >> 8) & 0x000000FF;
                        var b = (c) & 0x000000FF;

                        // Convert to gray with constant factors 0.2126, 0.7152, 0.0722
                        var gray = (r * 6966 + g * 23436 + b * 2366) >> 15;
                        r = g = b = gray;

                        // Set
                        rp[i] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }
                WriteableOutputImage = result;
            }

            return WriteableOutputImage;
        }

        public static WriteableBitmap ManualBinaryzation(int threshold, WriteableBitmap bitmap)
        {         
            bitmap.ForEach((x, y, curColor) =>
            {
                if (curColor.R > threshold)
                    return Color.FromArgb(255, 255, 255, 255);

                return Color.FromArgb(255, 0, 0, 0);
            });

            return bitmap;
        }


        public static WriteableBitmap NiblackBinaryzation(WriteableBitmap WriteableOutputImage, int size = 25, double k = 0.5)
        {
            NiblackThreshold niblack = new NiblackThreshold(size, k);
            WriteableOutputImage = niblack.Threshold(WriteableOutputImage);

            return WriteableOutputImage;
        }
    }
}
