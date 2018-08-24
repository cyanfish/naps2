using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Platform
{
    public class MonoRuntimeCompat : IRuntimeCompat
    {
        public bool UseToolStripRenderHack => false;

        public bool SetToolbarFont => true;

        public bool IsImagePaddingSupported => false;

        public bool IsToolbarTextboxSupported => false;

        public bool SetImageListSizeOnImageCollection => true;

        public bool UseSpaceInListViewItem => true;

        public bool RefreshListViewAfterChange => true;

        public string ExeRunner => "mono";

        public bool UseWorker => false;
    }
}
