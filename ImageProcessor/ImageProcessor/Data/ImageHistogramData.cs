using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Data
{
    public class ImageHistogramData
    {
        public int[] R { get; private set; } = new int[256];

        public int[] G { get; private set; } = new int[256];

        public int[] B { get; private set; } = new int[256];

        public int[] C
        {
            get
            {
                int[] tmp = new int[256];

                for (int i = 0; i < 256; ++i)
                    tmp[i] = (R[i] + G[i] + B[i]) / 3;

                return tmp;
            }
        }

        private readonly WriteableBitmap Bitmap;

        public ImageHistogramData(WriteableBitmap bitmap)
        {
            this.Bitmap = bitmap;
            Calculate();
        }

        private void Calculate()
        {
            using (var context = Bitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                int[] pixels = context.Pixels;

                for (int x = 0; x < Bitmap.PixelWidth; ++x)
                    for (int y = 0; y < Bitmap.PixelHeight; ++y)
                    {

                        var c = pixels[y * context.Width + x];
                        var a = (byte)(c >> 24);

                        // Prevent division by zero
                        int ai = a;
                        if (ai == 0)
                        {
                            ai = 1;
                        }

                        // Scale inverse alpha to use cheap integer mul bit shift
                        ai = ((255 << 8) / ai);
                        Color tmp = Color.FromArgb(a, (byte)((((c >> 16) & 0xFF) * ai) >> 8),
                                                      (byte)((((c >> 8) & 0xFF) * ai) >> 8),
                                                      (byte)((((c & 0xFF) * ai) >> 8)));


                        ++R[tmp.R];
                        ++G[tmp.G];
                        ++B[tmp.B];
                    }
            }
        }
    }
}
