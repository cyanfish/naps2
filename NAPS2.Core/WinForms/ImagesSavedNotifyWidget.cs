using NAPS2.Lang.Resources;
using System.IO;

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