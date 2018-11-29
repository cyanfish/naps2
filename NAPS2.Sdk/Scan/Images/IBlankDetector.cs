using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images.Storage;

namespace NAPS2.Scan.Images
{
    public interface IBlankDetector
    {
        bool IsBlank(IImage image, int whiteThresholdNorm, int coverageThresholdNorm);

        bool ExcludePage(IImage image, ScanProfile scanProfile);
    }
}
