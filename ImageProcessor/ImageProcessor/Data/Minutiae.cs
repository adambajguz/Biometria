namespace ImageProcessor.Data
{
    public class Minutiae
    {
        public int X { get; }
        public int Y { get; }
        public MinutiaeType Type { get; }

        public Minutiae(int x, int y, MinutiaeType type)
        {
            X = x;
            Y = y;
            Type = type;
        }
    }
}
