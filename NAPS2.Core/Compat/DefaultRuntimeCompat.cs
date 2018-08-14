using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Compat
{
    public class DefaultRuntimeCompat : IRuntimeCompat
    {
        public bool UseToolStripRenderHack => true;

        public bool SetToolbarFont => false;

        public bool IsImagePaddingSupported => true;
    }
}
