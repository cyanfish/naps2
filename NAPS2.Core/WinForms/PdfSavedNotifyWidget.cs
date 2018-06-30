using NAPS2.Lang.Resources;
using System.IO;

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