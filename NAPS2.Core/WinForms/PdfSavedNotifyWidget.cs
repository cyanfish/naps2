using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class PdfSavedNotifyWidget : NotifyWidget
    {
        private readonly string path;

        public PdfSavedNotifyWidget(string path)
            : base(MiscResources.PdfSaved, Path.GetFileName(path), path, Path.GetDirectoryName(path))
        {
            this.path = path;
        }

        public override NotifyWidgetBase Clone() => new PdfSavedNotifyWidget(path);
    }
}
