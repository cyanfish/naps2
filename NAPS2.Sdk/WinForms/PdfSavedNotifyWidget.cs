using System.IO;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class PdfSavedNotifyWidget : NotifyWidget
    {
        private readonly string _path;

        public PdfSavedNotifyWidget(string path)
            : base(MiscResources.PdfSaved, Path.GetFileName(path), path, Path.GetDirectoryName(path))
        {
            _path = path;
        }

        public override NotifyWidgetBase Clone() => new PdfSavedNotifyWidget(_path);
    }
}
