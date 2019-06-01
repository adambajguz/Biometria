using System;

namespace KeystrokeDynamics.Helpers
{
    class Metrices
    {//METRYKI
        public static long euklides(long x, long[] y) //pobrany i w bazie
        {
            double suma = 0;
            for (long i = 0; i < y.Length; i++)
            {
                suma += (x - y[i]) * (x - y[i]);
            }

            return (long)Math.Sqrt(suma);
        }
        public static long manhatan(long x, long[] y) //pobrany i w bazie
        {
            long suma = 0;
            for (long i = 0; i < y.Length; i++)
            {
                suma += Math.Abs(x - y[i]);
            }

            return suma;
        }
        public static long czebyszew(long x, long[] y) //pobrany i w bazie
        {
            long suma = -1;
            for (long i = 0; i < y.Length; i++)
            {
                suma = Math.Max(suma, Math.Abs(x - y[i]));
            }

            return suma;
        }
    }
}
