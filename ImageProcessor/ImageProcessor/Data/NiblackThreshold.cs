using System;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Data
{
    public class NiblackThreshold
    {
        private int radius = 15;
        private double k = 0.2D;
        private double c = 0;

        /// <summary>
        ///   Gets or sets the filter convolution
        ///   radius. Default is 15.
        /// </summary>
        /// 
        public int Radius
        {
            get => radius;
            set => radius = value;
        }

        /// <summary>
        ///   Gets or sets the user-defined 
        ///   parameter k. Default is 0.2.
        /// </summary>
        /// 
        public double K
        {
            get => k;
            set => k = value;
        }

        /// <summary>
        ///   Gets or sets the mean offset C. This value should
        ///   be between 0 and 255. The default value is 0.
        /// </summary>
        /// 
        public double C
        {
            get => c;
            set => c = value;
        }

        public void ProcessFilter(WriteableBitmap sourceData)
        {
            int width = sourceData.PixelWidth;
            int height = sourceData.PixelHeight;
            int size = radius * 2;


            sourceData.ForEach((x, y, curColor) =>
            {
                long sum = 0;
                int count = 0;

                for (int i = 0; i < size; i++)
                {
                    int ir = i - radius;
                    int t = y + ir;

                    if (t < 0)
                        continue;
                    if (t >= height)
                        break;

                    for (int j = 0; j < size; j++)
                    {
                        int jr = j - radius;
                        t = x + jr;

                        if (t < 0)
                            continue;
                        if (t >= width)
                            continue;

                        sum += curColor.R;
                        count++;
                    }
                }

                double mean = sum / (double)count;
                double variance = 0;

                for (int i = 0; i < size; i++)
                {
                    int ir = i - radius;
                    int t = y + ir;

                    if (t < 0)
                        continue;
                    if (t >= height)
                        break;

                    for (int j = 0; j < size; j++)
                    {
                        int jr = j - radius;
                        t = x + jr;

                        if (t < 0)
                            continue;
                        if (t >= width)
                            continue;

                        byte val = curColor.R;
                        variance += (val - mean) * (val - mean);
                    }
                }

                variance /= count - 1;

                double cut = mean + k * Math.Sqrt(variance) - c;

                byte rgb = (curColor.R > cut) ? (byte)255 : (byte)0;

                return Color.FromArgb(255, rgb, rgb, rgb);
            });



        }
    }
}
