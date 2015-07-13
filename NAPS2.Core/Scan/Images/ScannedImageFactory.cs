using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Scan.Images
{
    public class ScannedImageFactory : IScannedImageFactory
    {
        private readonly IUserConfigManager userConfigManager;

        public ScannedImageFactory(IUserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        public IScannedImage Create(Bitmap img, ScanBitDepth bitDepth, bool highQuality)
        {
            return new ScannedImage(img, bitDepth, highQuality, userConfigManager);
        }
    }
}