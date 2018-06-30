using System.Drawing;

namespace NAPS2.Scan.Images
{
    public interface IBlankDetector
    {
        bool IsBlank(Bitmap bitmap, int whiteThresholdNorm, int coverageThresholdNorm);

        bool ExcludePage(Bitmap bitmap, ScanProfile scanProfile);
    }
}