
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms
{
    public class ScaleTransform : Transform
    {
        public ScaleTransform()
        {
            ScaleFactor = 1.0;
        }

        public ScaleTransform(double scaleFactor)
        {
            ScaleFactor = scaleFactor;
        }

        public double ScaleFactor { get; private set; }

        public override bool IsNull => ScaleFactor == 1;
    }
}
