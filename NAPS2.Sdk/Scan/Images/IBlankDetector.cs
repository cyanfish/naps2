using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan.Images
{
    public interface IBlankDetector
    {
        bool IsBlank(Bitmap bitmap, int whiteThresholdNorm, int coverageThresholdNorm);

        bool ExcludePage(Bitmap bitmap, ScanProfile scanProfile);
    }
}
