using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class PdfSavedNotifyWidget : NotifyWidget
    {
        public PdfSavedNotifyWidget(string path)
            : base(MiscResources.PdfSaved, Path.GetFileName(path), path, Path.GetDirectoryName(path))
        {
        }
    }
}
