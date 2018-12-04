using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images.Storage;
using NAPS2.Scan;

namespace NAPS2.Images
{
    public abstract class BlankDetector
    {
        private static BlankDetector _default = new ThresholdBlankDetector();

        public static BlankDetector Default
        {
            get => _default;
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public abstract bool IsBlank(IImage image, int whiteThresholdNorm, int coverageThresholdNorm);

        public abstract bool ExcludePage(IImage image, ScanProfile scanProfile);
    }
}
