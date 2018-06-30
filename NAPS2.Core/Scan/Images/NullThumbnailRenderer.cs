using NAPS2.Config;
using System.Drawing;

namespace NAPS2.Scan.Images
{
    public class NullThumbnailRenderer : ThumbnailRenderer
    {
        public NullThumbnailRenderer(IUserConfigManager userConfigManager, ScannedImageRenderer scannedImageRenderer)
            : base(userConfigManager, scannedImageRenderer)
        {
        }

        public override Bitmap RenderThumbnail(Bitmap b, int size)
        {
            return null;
        }
    }
}