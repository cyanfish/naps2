using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images.Storage;

namespace NAPS2.Scan.Images
{
    public interface IBlankDetector
    {
        bool IsBlank(IMemoryStorage bitmap, int whiteThresholdNorm, int coverageThresholdNorm);

        bool ExcludePage(IMemoryStorage bitmap, ScanProfile scanProfile);
    }
}
