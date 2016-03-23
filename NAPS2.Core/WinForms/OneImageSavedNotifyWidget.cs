using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class OneImageSavedNotifyWidget : NotifyWidget
    {
        public OneImageSavedNotifyWidget(string path)
            : base(MiscResources.ImageSaved, Path.GetFileName(path), path, Path.GetDirectoryName(path))
        {
        }
    }
}
