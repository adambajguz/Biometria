using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Data
{
    public static class FingerprintHelper
    {
        public static Dictionary<Point, MinutiaeType> DetectMinutiae(WriteableBitmap bitmap)
        {
            Dictionary<Point, MinutiaeType> points = new Dictionary<Point, MinutiaeType>();

            using (BitmapContext context = bitmap.GetBitmapContext())
            {
                int width = context.Width;
                int height = context.Height;

                int[] mask = new int[9];


                for (int x = 1; x < width - 1; ++x)
                {
                    for (int y = 1; y < height - 1; ++y)
                    {
                        if (PixelHelper.IsBlack(context, x, y))
                        {
                            mask[0] = PixelHelper.IsWhite(context, x    , y + 1) ? 0 : 1;
                            mask[1] = PixelHelper.IsWhite(context, x - 1, y + 1) ? 0 : 1;
                            mask[2] = PixelHelper.IsWhite(context, x - 1,     y) ? 0 : 1;
                            mask[3] = PixelHelper.IsWhite(context, x - 1, y - 1) ? 0 : 1;
                            mask[4] = PixelHelper.IsWhite(context, x    , y - 1) ? 0 : 1;
                            mask[5] = PixelHelper.IsWhite(context, x + 1, y - 1) ? 0 : 1;
                            mask[6] = PixelHelper.IsWhite(context, x + 1,     y) ? 0 : 1;
                            mask[7] = PixelHelper.IsWhite(context, x + 1, y + 1) ? 0 : 1;
                            mask[8] = mask[0];


                            MinutiaeType cn = CalculateCN(mask);
                            if (IsMinutiae(cn))
                            {
                                points.Add(new Point(x, y), cn);

                                if (cn == MinutiaeType.Point)
                                    PixelHelper.SetPixel(context, x, y, Colors.Red);
                                else if (cn == MinutiaeType.Ending)
                                    PixelHelper.SetPixel(context, x, y, Colors.Green);
                                else if (cn == MinutiaeType.Fork)
                                    PixelHelper.SetPixel(context, x, y, Colors.Yellow);
                                else if (cn == MinutiaeType.Crossing)
                                    PixelHelper.SetPixel(context, x, y, Colors.DeepPink);
                            }
                        }
                    }
                }
            }

            return points;
        }

        // CN == 0 -> single point / isolated point           -> is minutiae
        // CN == 1 -> ending point / termination point        -> is minutiae
        // CN == 2 -> edge continuation / normal ridge pixel  -> is not minutiae
        // CN == 3 -> fork / bifurcation point                -> is minutiae
        // CN == 4 -> crossing                                -> is minutiae
        private static MinutiaeType CalculateCN(int[] mask)
        {
            int CN = 0;
            for (int i = 0; i < 8; ++i)
                CN += Math.Abs(mask[i] - mask[i + 1]);

            return (MinutiaeType)(CN >> 1);
        }

        private static bool IsMinutiae(MinutiaeType CN) => CN != MinutiaeType.Invalid;
    }
}
