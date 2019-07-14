
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms
{
    public class SaturationTransform : Transform
    {
        public SaturationTransform()
        {
        }

        public SaturationTransform(int saturation)
        {
            Saturation = saturation;
        }

        public int Saturation { get; private set; }

        public override bool IsNull => Saturation == 0;
    }
}
