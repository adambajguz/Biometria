using System;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Data
{
    public static class GradientMagnitude
    {

        public static WriteableBitmap CalculateGradientMagnitude(WriteableBitmap inputOutput, WriteableBitmap input2)
        {
            using (var context = inputOutput.GetBitmapContext())
            {
                using (var context2 = inputOutput.GetBitmapContext())
                {
                    int[] pixels = context.Pixels;
                    int[] pixels2 = context2.Pixels;

                    int w = context.Width;
                    int h = context.Height;
                    int index = 0;

                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            Color srcColor;
                            {
                                int c = pixels[index];

                                // Premultiplied Alpha!
                                byte a = (byte)(c >> 24);
                                // Prevent division by zero
                                int ai = a;
                                if (ai == 0)
                                {
                                    ai = 1;
                                }
                                // Scale inverse alpha to use cheap integer mul bit shift
                                ai = ((255 << 8) / ai);
                                srcColor = Color.FromArgb(a,
                                                              (byte)((((c >> 16) & 0xFF) * ai) >> 8),
                                                              (byte)((((c >> 8) & 0xFF) * ai) >> 8),
                                                              (byte)((((c & 0xFF) * ai) >> 8)));

                            }

                            Color srcColor2;
                            {
                                int c = pixels[index];

                                // Premultiplied Alpha!
                                byte a = (byte)(c >> 24);
                                // Prevent division by zero
                                int ai = a;
                                if (ai == 0)
                                {
                                    ai = 1;
                                }
                                // Scale inverse alpha to use cheap integer mul bit shift
                                ai = ((255 << 8) / ai);
                                srcColor2 = Color.FromArgb(a,
                                                              (byte)((((c >> 16) & 0xFF) * ai) >> 8),
                                                              (byte)((((c >> 8) & 0xFF) * ai) >> 8),
                                                              (byte)((((c & 0xFF) * ai) >> 8)));

                            }

                            double rO = Math.Sqrt(srcColor.R * srcColor.R + srcColor2.R + srcColor2.R);
                            double gO = Math.Sqrt(srcColor.G * srcColor.G + srcColor2.G + srcColor2.G);
                            double bO = Math.Sqrt(srcColor.B * srcColor.B + srcColor2.B + srcColor2.B);
                            if (rO > 255)
                                rO = 255;
                            if (rO < 0)
                                rO = 0;

                            if (gO > 255)
                                gO = 255;
                            if (gO < 0)
                                gO = 0;

                            if (bO > 255)
                                bO = 255;
                            if (bO < 0)
                                bO = 0;

                            byte aO = srcColor.A;

                            Color ou = Color.FromArgb(aO, (byte)rO, (byte)gO, (byte)bO);

                            pixels[index++] = ConvertColor(ou);
                        }
                    }
                }
            }
            return inputOutput;
        }

        public static int ConvertColor(Color color)
        {
            var col = 0;

            if (color.A != 0)
            {
                var a = color.A + 1;
                col = (color.A << 24)
                  | ((byte)((color.R * a) >> 8) << 16)
                  | ((byte)((color.G * a) >> 8) << 8)
                  | ((byte)((color.B * a) >> 8));
            }

            return col;
        }
    }
}
