
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms
{
    public class TrueContrastTransform : Transform
    {
        public TrueContrastTransform()
        {
        }

        public TrueContrastTransform(int contrast)
        {
            Contrast = contrast;
        }

        public int Contrast { get; private set; }

        public override bool IsNull => Contrast == 0;
    }
}
