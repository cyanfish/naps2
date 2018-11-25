using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class ImagesSavedNotifyWidget : NotifyWidget
    {
        private readonly int imageCount;
        private readonly string path;

        public ImagesSavedNotifyWidget(int imageCount, string path)
            : base(string.Format(MiscResources.ImagesSaved, imageCount), Path.GetFileName(path), path, Path.GetDirectoryName(path))
        {
            this.imageCount = imageCount;
            this.path = path;
        }

        public override NotifyWidgetBase Clone() => new ImagesSavedNotifyWidget(imageCount, path);
    }
}
