using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan.Images
{
    internal static class TransformationHelper
    {
        public static RotateFlipType CombineRotation(RotateFlipType currentTransform, RotateFlipType nextTransform)
        {
            if ((int)currentTransform >= 4)
            {
                throw new ArgumentException("The currentTransform argument must not include a flip.");
            }
            if ((int)nextTransform >= 4)
            {
                throw new ArgumentException("The nextTransform argument must not include a flip.");
            }
            return FromRotation(GetRotation(currentTransform) + GetRotation(nextTransform));
        }

        public static int GetRotation(RotateFlipType rotateFlipType)
        {
            switch (rotateFlipType)
            {
                case RotateFlipType.RotateNoneFlipNone:
                    return 0;
                case RotateFlipType.Rotate90FlipNone:
                    return 1;
                case RotateFlipType.Rotate180FlipNone:
                    return 2;
                case RotateFlipType.Rotate270FlipNone:
                    return 3;
            }
            throw new ArgumentException();
        }

        public static RotateFlipType FromRotation(int rotation)
        {
            switch (rotation % 4)
            {
                case 0:
                    return RotateFlipType.RotateNoneFlipNone;
                case 1:
                    return RotateFlipType.Rotate90FlipNone;
                case 2:
                    return RotateFlipType.Rotate180FlipNone;
                case 3:
                    return RotateFlipType.Rotate270FlipNone;
            }
            throw new ArgumentException();
        }
    }
}