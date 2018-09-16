using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class OneImageSavedNotifyWidget : NotifyWidget
    {
        private readonly string path;

        public OneImageSavedNotifyWidget(string path)
            : base(MiscResources.ImageSaved, Path.GetFileName(path), path, Path.GetDirectoryName(path))
        {
            this.path = path;
        }

        public override NotifyWidgetBase Clone() => new OneImageSavedNotifyWidget(path);
    }
}
