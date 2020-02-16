using System.IO;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class ImagesSavedNotifyWidget : NotifyWidget
    {
        private readonly int _imageCount;
        private readonly string _path;

        public ImagesSavedNotifyWidget(int imageCount, string path)
            : base(string.Format(MiscResources.ImagesSaved, imageCount), Path.GetFileName(path), path, Path.GetDirectoryName(path))
        {
            _imageCount = imageCount;
            _path = path;
        }

        public override NotifyWidgetBase Clone() => new ImagesSavedNotifyWidget(_imageCount, _path);
    }
}
