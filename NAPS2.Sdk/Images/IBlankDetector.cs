using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images.Storage;
using NAPS2.Scan;

namespace NAPS2.Images
{
    public interface IBlankDetector
    {
        bool IsBlank(IImage image, int whiteThresholdNorm, int coverageThresholdNorm);

        bool ExcludePage(IImage image, ScanProfile scanProfile);
    }
}
