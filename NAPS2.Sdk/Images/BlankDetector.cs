using System;
using NAPS2.Images.Storage;
using NAPS2.Scan;
using NAPS2.Util;

namespace NAPS2.Images
{
    public abstract class BlankDetector
    {
        private static BlankDetector _default = new ThresholdBlankDetector();

        public static BlankDetector Default
        {
            get
            {
                TestingContext.NoStaticDefaults();
                return _default;
            }
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public abstract bool IsBlank(IImage image, int whiteThresholdNorm, int coverageThresholdNorm);

        public abstract bool ExcludePage(IImage image, ScanProfile scanProfile);
    }
}
