using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Platform
{
    public class DefaultRuntimeCompat : IRuntimeCompat
    {
        public bool UseToolStripRenderHack => true;

        public bool SetToolbarFont => false;

        public bool IsImagePaddingSupported => true;

        public bool SetImageListSizeOnImageCollection => false;
    }
}
