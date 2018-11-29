using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Images.Transforms
{
    [Serializable]
    public class ThumbnailTransform : Transform
    {
        public int Size { get; set; } = UserConfig.Current.ThumbnailSize;
    }
}
