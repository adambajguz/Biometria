﻿using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Data
{
    public class KMMHelper
    {

        public int[] A = new int[] { 3, 5, 7, 12, 13, 14, 15, 20,
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

        public int[,] PixelInfo(BitmapContext context, int[,] pixels, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (GetPixel(context, i, j) == Colors.Black)
                        pixels[i, j] = 1;
                    else
                        pixels[i, j] = 0;
                }
            }
            return pixels;
        }

        public int[,] Mark_2s(int[,] pixels, int width, int height)
        {
            for (int i = 0; i < width; i++) //mark '2's
            {
                for (int j = 0; j < height; j++)
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

        public int[,] Mark_3s(int[,] pixels, int width, int height)
        {
            for (int i = 0; i < width; i++) //mark '3's
            {
                for (int j = 0; j < height; j++)
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

        public int[,] CalculateWeights(BitmapContext context, int[,] pixels, int[,] pixelsWeights, int width, int height)
        {
            for (int i = 0; i < width; i++) //calculate weight
            {
                for (int j = 0; j < height; j++)
                {
                    if (pixels[i, j] != 0)
                    {
                        pixelsWeights[i, j] = CalculateWeight(i, j, context, width, height);
                    }
                }
            }
            return pixelsWeights;
        }

        public int CalculateWeight(int i, int j, BitmapContext context, int width, int height)
        {
            int[] N = new int[] { 128, 1, 2, 64, 0, 4, 32, 16, 8 };
            int weight = 0;
            if (i - 1 > 0 && j - 1 > 0 && GetPixel(context, i - 1, j - 1) == Colors.Black)
                weight += N[0];
            if (j - 1 > 0 && GetPixel(context, i, j - 1) == Colors.Black)
                weight += N[1];
            if (i + 1 < width && j - 1 > 0 && GetPixel(context, i + 1, j - 1) == Colors.Black)
                weight += N[2];
            if (i - 1 > 0 && GetPixel(context, i - 1, j) == Colors.Black)
                weight += N[3];
            if (i + 1 < width && GetPixel(context, i + 1, j) == Colors.Black)
                weight += N[5];
            if (i - 1 > 0 && j + 1 < height && GetPixel(context, i - 1, j + 1) == Colors.Black)
                weight += N[6];
            if (j + 1 < height && GetPixel(context, i, j + 1) == Colors.Black)
                weight += N[7];
            if (i + 1 < width && j + 1 < height && GetPixel(context, i + 1, j + 1) == Colors.Black)
                weight += N[8];
            return weight;
        }

        public static Color GetPixel(BitmapContext context, int x, int y)
        {
            var c = context.Pixels[y * context.Width + x];
            var a = (byte)(c >> 24);

            // Prevent division by zero
            int ai = a;
            if (ai == 0)
            {
                ai = 1;
            }

            // Scale inverse alpha to use cheap integer mul bit shift
            ai = ((255 << 8) / ai);
            return Color.FromArgb(a,
                                 (byte)((((c >> 16) & 0xFF) * ai) >> 8),
                                 (byte)((((c >> 8) & 0xFF) * ai) >> 8),
                                 (byte)((((c & 0xFF) * ai) >> 8)));
        }
    }
}
