using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Compat
{
    public class MonoRuntimeCompat : IRuntimeCompat
    {
        public bool UseToolStripRenderHack => false;

        public bool SetToolbarFont => true;

        public bool IsImagePaddingSupported => false;
    }
}
