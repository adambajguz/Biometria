using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Data
{

    public static class Otsu
    {
        // function is used to compute the q values in the equation
        private static int Px(int init, int end, int[] hist)
        {
            int sum = 0;
            for (int i = init; i <= end; ++i)
                sum += hist[i];

            return sum;
        }

        // function is used to compute the mean values in the equation (mu)
        private static int Mx(int init, int end, int[] hist)
        {
            int sum = 0;
            for (int i = init; i <= end; ++i)
                sum += i * hist[i];

            return sum;
        }

        private static int FindMaxElementIndex(double[] vec)
        {
            double maxVec = 0;
            int idx = 0;

            for (int i = 1; i < vec.Length - 1; ++i)
                if (vec[i] > maxVec)
                {
                    maxVec = vec[i];
                    idx = i;
                }

            return idx;
        }

        public static int GetOtsuThreshold(WriteableBitmap bmp)
        {
            double[] vet = new double[256];

            ImageHistogramData imageHistogramData = new ImageHistogramData(bmp);
            int[] hist = imageHistogramData.R;

            // loop through all possible t values and maximize between class variance
            for (int k = 1; k != 255; ++k)
            {
                double p1 = Px(0, k, hist);
                double p2 = Px(k + 1, 255, hist);
                double p12 = p1 * p2;
                if (p12 == 0)
                    p12 = 1;

                double diff = (Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                vet[k] = diff * diff / p12;
                //vet[k] = (float)Math.Pow((Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1), 2) / p12;
            }

            return FindMaxElementIndex(vet);
        }
    }

}
