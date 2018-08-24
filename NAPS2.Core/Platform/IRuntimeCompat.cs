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

        bool IsToolbarTextboxSupported { get; }

        bool SetImageListSizeOnImageCollection { get; }
        
        bool UseSpaceInListViewItem { get; }

        bool RefreshListViewAfterChange { get; }

        string ExeRunner { get; }

        bool UseWorker { get; }
    }
}
