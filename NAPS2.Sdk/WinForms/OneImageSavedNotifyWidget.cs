using System.IO;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms;

public class OneImageSavedNotifyWidget : NotifyWidget
{
    private readonly string _path;

    public OneImageSavedNotifyWidget(string path)
        : base(MiscResources.ImageSaved, Path.GetFileName(path), path, Path.GetDirectoryName(path))
    {
        _path = path;
    }

    public override NotifyWidgetBase Clone() => new OneImageSavedNotifyWidget(_path);
}