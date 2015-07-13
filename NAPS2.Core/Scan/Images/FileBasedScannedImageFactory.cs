using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Scan.Images
{
    public class FileBasedScannedImageFactory : IScannedImageFactory
    {
        private readonly IUserConfigManager userConfigManager;

        public FileBasedScannedImageFactory(IUserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        public IScannedImage Create(Bitmap img, ScanBitDepth bitDepth, bool highQuality)
        {
            return new FileBasedScannedImage(img, bitDepth, highQuality, userConfigManager);
        }
    }
}