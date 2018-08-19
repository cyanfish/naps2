using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Platform
{
    public interface IRuntimeCompat
    {
        bool UseToolStripRenderHack { get; }

        bool SetToolbarFont { get; }

        bool IsImagePaddingSupported { get; }
    }
}
