using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NLog;

namespace NAPS2.Scan.Images
{
    public class FileBasedScannedImageFactory : IScannedImageFactory
    {
        private readonly Logger logger;

        public FileBasedScannedImageFactory(Logger logger)
        {
            this.logger = logger;
        }

        public IScannedImage Create(Bitmap img, ScanBitDepth bitDepth, bool highQuality)
        {
            return new FileBasedScannedImage(img, bitDepth, highQuality, logger);
        }
    }
}