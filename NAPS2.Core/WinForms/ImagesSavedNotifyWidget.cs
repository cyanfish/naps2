using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class ImagesSavedNotifyWidget : NotifyWidget
    {
        public ImagesSavedNotifyWidget(int imageCount, string path)
            : base(string.Format(MiscResources.ImagesSaved, imageCount), Path.GetFileName(path), path, Path.GetDirectoryName(path))
        {
        }
    }
}
