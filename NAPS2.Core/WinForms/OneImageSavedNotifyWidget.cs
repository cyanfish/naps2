using NAPS2.Lang.Resources;
using System.IO;

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