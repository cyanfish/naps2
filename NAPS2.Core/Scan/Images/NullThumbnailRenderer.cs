using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Config;

namespace NAPS2.Scan.Images
{
    public class NullThumbnailRenderer : ThumbnailRenderer
    {
        public NullThumbnailRenderer(IUserConfigManager userConfigManager, ScannedImageRenderer scannedImageRenderer)
            : base(userConfigManager, scannedImageRenderer)
        {
        }
        
        public override Bitmap RenderThumbnail(Bitmap b, int size) => null;
    }
}
