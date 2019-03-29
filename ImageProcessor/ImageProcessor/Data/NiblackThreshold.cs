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
               

                return Color.FromArgb(255, 0, 0, 0);
            });



        }
    }
}
