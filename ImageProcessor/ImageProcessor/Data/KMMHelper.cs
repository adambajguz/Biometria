using System.Linq;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Data
{
    public static class KMMHelper
    {
        public static int[] A = new int[] { 3, 5, 7, 12, 13, 14, 15, 20,
                                21, 22, 23, 28, 29, 30, 31, 48,
                                52, 53, 54, 55, 56, 60, 61, 62,
                                63, 65, 67, 69, 71, 77, 79, 80,
                                81, 83, 84, 85, 86, 87, 88, 89,
                                91, 92, 93, 94, 95, 97, 99, 101,
                                103, 109, 111, 112, 113, 115, 116, 117,
                                118, 119, 120, 121, 123, 124, 125, 126,
                                127, 131, 133, 135, 141, 143, 149, 151,
                                157, 159, 181, 183, 189, 191, 192, 193,
                                195, 197, 199, 205, 207, 208, 209, 211,
                                212, 213, 214, 215, 216, 217, 219, 220,
                                221, 222, 223, 224, 225, 227, 229, 231,
                                237, 239, 240, 241, 243, 244, 245, 246,
                                247, 248, 249, 251, 252, 253, 254, 255 };

        public static WriteableBitmap KMM(WriteableBitmap bitmap)
        {
            using (BitmapContext context = bitmap.GetBitmapContext())
            {
                int[,] pixels = PixelInfo(context);

                bool changed;
                do
                {
                    changed = false;

                    pixels = Mark2s(pixels);
                    pixels = Mark3s(pixels);

                    changed = Delete4s(context, pixels, changed);
                    changed = Delete2s(context, pixels, changed);
                    changed = Delete3s(context, pixels, changed);
                } while (changed);
            }

            return bitmap;
        }

        #region Deletes
        private static bool Delete3s(BitmapContext context, int[,] pixels, bool change)
        {
            int width = context.Width;
            int height = context.Height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (pixels[i, j] == 3)
                    {
                        int weight = CalculateWeight(i, j, context);
                        if (A.Contains(weight))
                        {
                            pixels[i, j] = 0;
                            PixelHelper.SetWhite(context, i, j);
                            change = true;
                        }
                        else
                        {
                            pixels[i, j] = 1;
                        }
                    }
                }
            }

            return change;
        }

        private static bool Delete2s(BitmapContext context, int[,] pixels, bool change)
        {
            int width = context.Width;
            int height = context.Height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (pixels[i, j] == 2)
                    {
                        int weight = CalculateWeight(i, j, context);
                        if (A.Contains(weight))
                        {
                            pixels[i, j] = 0;
                            PixelHelper.SetWhite(context, i, j);
                            change = true;
                        }
                        else
                        {
                            pixels[i, j] = 1;
                        }
                    }
                }
            }

            return change;
        }

        private static bool Delete4s(BitmapContext context, int[,] pixels, bool change)
        {
            int width = context.Width;
            int height = context.Height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (pixels[i, j] == 4)
                    {
                        int weight = CalculateWeight(i, j, context);
                        if (A.Contains(weight))
                        {
                            pixels[i, j] = 0;
                            PixelHelper.SetWhite(context, i, j);
                            change = true;
                        }
                    }
                }
            }

            return change;
        }
        #endregion

        #region Marks
        private static int[,] Mark2s(int[,] pixels)
        {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    if (pixels[i, j] == 1)
                    {
                        if (i > 0 && pixels[i - 1, j] == 0)
                            pixels[i, j] = 2;
                        else if (j > 0 && pixels[i, j - 1] == 0)
                            pixels[i, j] = 2;
                        else if (i < width - 1 && pixels[i + 1, j] == 0)
                            pixels[i, j] = 2;
                        else if (j < height - 1 && pixels[i, j + 1] == 0)
                            pixels[i, j] = 2;
                    }
                }
            }

            return pixels;
        }

        private static int[,] Mark3s(int[,] pixels)
        {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    if (pixels[i, j] == 1)
                    {
                        if (i > 0 && j > 0 && pixels[i - 1, j - 1] == 0)
                            pixels[i, j] = 3;
                        else if (i < width - 1 && j > 0 && pixels[i + 1, j - 1] == 0)
                            pixels[i, j] = 3;
                        else if (i < width - 1 && j < height - 1 && pixels[i + 1, j + 1] == 0)
                            pixels[i, j] = 3;
                        else if (i > 0 && j < height - 1 && pixels[i - 1, j + 1] == 0)
                            pixels[i, j] = 3;
                    }
                }
            }

            return pixels;
        }
        #endregion

        private static int[,] PixelInfo(BitmapContext context)
        {
            int width = context.Width;
            int height = context.Height;

            int[,] pixels = new int[width, height];
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    if (PixelHelper.IsBlack(context, i, j))
                        pixels[i, j] = 1;
                    else
                        pixels[i, j] = 0;
                }
            }
            return pixels;
        }

        private static int CalculateWeight(int i, int j, BitmapContext context)
        {
            int width = context.Width;
            int height = context.Height;

            int[] N = new int[] { 128, 1, 2, 64, 0, 4, 32, 16, 8 };
            int weight = 0;

            if (i - 1 > 0)
            {
                if (j - 1 > 0 && PixelHelper.IsBlack(context, i - 1, j - 1))
                    weight += N[0];
                if (PixelHelper.IsBlack(context, i - 1, j))
                    weight += N[3];
                if (j + 1 < height && PixelHelper.IsBlack(context, i - 1, j + 1))
                    weight += N[6];
            }
            if (i + 1 < width)
            {
                if (j - 1 > 0 && PixelHelper.IsBlack(context, i + 1, j - 1))
                    weight += N[2];
                if (PixelHelper.IsBlack(context, i + 1, j))
                    weight += N[5];
                if (j + 1 < height && PixelHelper.IsBlack(context, i + 1, j + 1))
                    weight += N[8];
            }

            if (j - 1 > 0 && PixelHelper.IsBlack(context, i, j - 1))
                weight += N[1];
            if (j + 1 < height && PixelHelper.IsBlack(context, i, j + 1))
                weight += N[7];

            return weight;
        }
    }
}
