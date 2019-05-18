using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Data
{
    public static class PixelHelper
    {
        public static void SetPixel(BitmapContext context, int x, int y, Color color) => context.Pixels[y * context.Width + x] = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
        public static void SetPixel(BitmapContext context, int x, int y, byte a, byte r, byte g, byte b) => context.Pixels[y * context.Width + x] = (a << 24) | (r << 16) | (g << 8) | b;
        public static void SetPixel(BitmapContext context, int x, int y, byte r, byte g, byte b) => context.Pixels[y * context.Width + x] = (255 << 24) | (r << 16) | (g << 8) | b;

        public static void SetPixel(BitmapContext context, int i, Color color) => context.Pixels[i] = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
        public static void SetPixel(BitmapContext context, int i, byte a, byte r, byte g, byte b) => context.Pixels[i] = (a << 24) | (r << 16) | (g << 8) | b;
        public static void SetPixel(BitmapContext context, int i, byte r, byte g, byte b) => context.Pixels[i] = (255 << 24) | (r << 16) | (g << 8) | b;

        public static void SetBlack(BitmapContext context, int i)
        {
            unchecked
            {
                context.Pixels[i] = (int)0xff000000;
            }
        }

        public static void SetBlack(BitmapContext context, int x, int y)
        {
            unchecked
            {
                context.Pixels[y * context.Width + x] = (int)0xff000000;
            }
        }

        public static void SetWhite(BitmapContext context, int i)
        {
            unchecked
            {
                context.Pixels[i] = (int)0xffffffff;
            }
        }

        public static void SetWhite(BitmapContext context, int x, int y)
        {
            unchecked
            {
                context.Pixels[y * context.Width + x] = (int)0xffffffff;
            }
        }

        public static bool IsBlack(BitmapContext context, int i) => (uint)context.Pixels[i] == 0xff000000;
        public static bool IsBlack(BitmapContext context, int x, int y) => (uint)context.Pixels[y * context.Width + x] == 0xff000000;

        public static bool IsWhite(BitmapContext context, int i) => (uint)context.Pixels[i] == 0xffffffff;
        public static bool IsWhite(BitmapContext context, int x, int y) => (uint)context.Pixels[y * context.Width + x] == 0xffffffff;

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
