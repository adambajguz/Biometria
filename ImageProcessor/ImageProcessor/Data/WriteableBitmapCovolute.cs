using System;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageProcessor.Data
{
    public static class WriteableBitmapCovolute
    {
        public static int[,] KernelGaussianBlur5x5 = {
                                                       {1,  4,  7,  4, 1},
                                                       {4, 16, 26, 16, 4},
                                                       {7, 26, 41, 26, 7},
                                                       {4, 16, 26, 16, 4},
                                                       {1,  4,  7,  4, 1}
                                                 };

        public static int[,] KernelGaussianBlur3x3 = {
                                                       {16, 26, 16},
                                                       {26, 41, 26},
                                                       {16, 26, 16}
                                                    };


        public static int[,] KernelSharpen3x3 = {
                                                 { 0, -2,  0},
                                                 {-2, 11, -2},
                                                 { 0, -2,  0}
                                              };


  
        // returns a new WriteableBitmap that is a filtered version of the input.</returns>
        public static WriteableBitmap Convolute(WriteableBitmap bmp, int[,] kernel)
        {
            var kernelFactorSum = 0;
            foreach (var b in kernel)
            {
                kernelFactorSum += Math.Abs(b);
            }

            return bmp.Convolute(kernel, kernelFactorSum, 0);
        }

    
        public static WriteableBitmap Convolute(this WriteableBitmap bmp, int[,] kernel, int kernelFactorSum, int kernelOffsetSum)
        {
            var kh = kernel.GetUpperBound(0) + 1;
            var kw = kernel.GetUpperBound(1) + 1;

            if ((kw & 1) == 0)
            {
                throw new InvalidOperationException("Kernel width must be odd!");
            }
            if ((kh & 1) == 0)
            {
                throw new InvalidOperationException("Kernel height must be odd!");
            }

            using (var srcContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                var w = srcContext.Width;
                var h = srcContext.Height;
                var result = BitmapFactory.New(w, h);

                using (var resultContext = result.GetBitmapContext())
                {
                    var pixels = srcContext.Pixels;
                    var resultPixels = resultContext.Pixels;
                    var index = 0;
                    var kwh = kw >> 1;
                    var khh = kh >> 1;

                    for (var y = 0; y < h; y++)
                    {
                        for (var x = 0; x < w; x++)
                        {
                            var a = 0;
                            var r = 0;
                            var g = 0;
                            var b = 0;

                            for (var kx = -kwh; kx <= kwh; kx++)
                            {
                                var px = kx + x;
                                // Repeat pixels at borders
                                if (px < 0)
                                {
                                    px = 0;
                                }
                                else if (px >= w)
                                {
                                    px = w - 1;
                                }

                                for (var ky = -khh; ky <= khh; ky++)
                                {
                                    var py = ky + y;
                                    // Repeat pixels at borders
                                    if (py < 0)
                                    {
                                        py = 0;
                                    }
                                    else if (py >= h)
                                    {
                                        py = h - 1;
                                    }

                                    var col = pixels[py * w + px];
                                    var k = kernel[ky + kwh, kx + khh];
                                    a += ((col >> 24) & 0x000000FF);//* k;
                                    r += ((col >> 16) & 0x000000FF) * k;
                                    g += ((col >> 8) & 0x000000FF) * k;
                                    b += ((col) & 0x000000FF) * k;
                                }
                            }

                            var ta = ((a / kernelFactorSum) + kernelOffsetSum);
                            var tr = ((r / kernelFactorSum) + kernelOffsetSum);
                            var tg = ((g / kernelFactorSum) + kernelOffsetSum);
                            var tb = ((b / kernelFactorSum) + kernelOffsetSum);

                            // Clamp to byte boundaries
                            var ba = (byte)((ta > 255) ? 255 : ((ta < 0) ? 0 : ta));
                            var br = (byte)((tr > 255) ? 255 : ((tr < 0) ? 0 : tr));
                            var bg = (byte)((tg > 255) ? 255 : ((tg < 0) ? 0 : tg));
                            var bb = (byte)((tb > 255) ? 255 : ((tb < 0) ? 0 : tb));

                            resultPixels[index++] = (ba << 24) | (br << 16) | (bg << 8) | (bb);
                        }
                    }
                    return result;
                }
            }
        }
    }
}