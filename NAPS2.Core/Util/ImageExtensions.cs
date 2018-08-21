using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Util
{
    public static class ImageExtensions
    {
        public static void SafeSetResolution(this Bitmap image, float xDpi, float yDpi)
        {
            if (xDpi > 0 && yDpi > 0)
            {
                image.SetResolution(xDpi, yDpi);
            }
        }
    }
}
