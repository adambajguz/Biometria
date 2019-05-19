using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Data
{
    public static class FingerprintHelper
    {
        public static List<Point> DetectMinutiae(WriteableBitmap bitmap)
        {
            List<Point> points = new List<Point>();

            using (BitmapContext context = bitmap.GetBitmapContext())
            {
                int width = context.Width;
                int height = context.Height;

                for (int x = 1; x < width - 1; ++x)
                {
                    for (int y = 1; y < height - 1; ++y)
                    {
                        if (PixelHelper.IsBlack(context, x, y))
                        {
                            int[,] area = new int[3, 3];
                            area[0, 0] = PixelHelper.IsBlack(context, x - 1, y - 1) ? 1 : 0;
                            area[0, 1] = PixelHelper.IsBlack(context, x, y - 1) ? 1 : 0;
                            area[0, 2] = PixelHelper.IsBlack(context, x + 1, y - 1) ? 1 : 0;

                            area[1, 0] = PixelHelper.IsBlack(context, x - 1, y) ? 1 : 0;
                            //area[1, 1] = PixelHelper.IsBlack(context, x    , y    ) ? 1 : 0;
                            area[1, 2] = PixelHelper.IsBlack(context, x + 1, y) ? 1 : 0;

                            area[2, 0] = PixelHelper.IsBlack(context, x - 1, y + 1) ? 1 : 0;
                            area[2, 1] = PixelHelper.IsBlack(context, x, y + 1) ? 1 : 0;
                            area[2, 2] = PixelHelper.IsBlack(context, x + 1, y + 1) ? 1 : 0;


                            int cn = CalculateCN(area);
                            if (IsMinutiae(cn))
                            {
                                points.Add(new Point(x, y));

                                if (cn == 0)
                                    PixelHelper.SetPixel(context, x, y, Colors.Red);
                                else if (cn == 1)
                                    PixelHelper.SetPixel(context, x, y, Colors.Green);
                                else if (cn == 3)
                                    PixelHelper.SetPixel(context, x, y, Colors.Yellow);
                                else if (cn == 4)
                                    PixelHelper.SetPixel(context, x, y, Colors.DeepPink);
                            }
                        }
                    }
                }
            }

            return points;
        }

        private static int CalculateCN(int[,] area3x3)
        {
            int CN = IntAbs(area3x3[1, 2] - area3x3[0, 2]) +
                     IntAbs(area3x3[0, 2] - area3x3[0, 1]) +
                     IntAbs(area3x3[0, 1] - area3x3[0, 0]) +
                     IntAbs(area3x3[0, 0] - area3x3[1, 0]) +
                     IntAbs(area3x3[1, 0] - area3x3[2, 0]) +
                     IntAbs(area3x3[2, 0] - area3x3[2, 1]) +
                     IntAbs(area3x3[2, 1] - area3x3[2, 2]) +
                     IntAbs(area3x3[2, 2] - area3x3[1, 2]);

            return CN / 2;
        }

        private static bool IsMinutiae(int CN) => CN != 2;


        private static int IntAbs(int x) => x > 0 ? x : checked(-x);
    }
}
