using Eto.Forms;

namespace NAPS2.WinForms;

public class WinFormsDialogHelper : DialogHelper
{
    private readonly Naps2Config _config;

    public WinFormsDialogHelper(Naps2Config config)
    {
        _config = config;
    }

    public override bool PromptToSavePdfOrImage(string? defaultPath, out string savePath)
    {
        var sd = new SaveFileDialog
        {
            CheckFileExists = false,
            // TODO
            // AddExtension = true,
            // TODO: Move filter logic somewhere common
            Filters =
            {
                new FileFilter(MiscResources.FileTypePdf, ".pdf"),
                new FileFilter(MiscResources.FileTypeBmp, ".bmp"),
                new FileFilter(MiscResources.FileTypeEmf, ".emf"),
                new FileFilter(MiscResources.FileTypeExif, ".exif"),
                new FileFilter(MiscResources.FileTypeGif, ".gif"),
                new FileFilter(MiscResources.FileTypeJpeg, ".jpg", ".jpeg"),
                new FileFilter(MiscResources.FileTypePng, ".png"),
                new FileFilter(MiscResources.FileTypeTiff, ".tiff", ".tif"),
            },
            FileName = Path.GetFileName(defaultPath)
        };
        SetDir(sd, defaultPath);
        if (sd.ShowDialog(null) == DialogResult.Ok)
        {
            savePath = sd.FileName;
            return true;
        }
        savePath = null;
        return false;
    }

    public override bool PromptToSavePdf(string? defaultPath, out string savePath)
    {
        var sd = new SaveFileDialog
        {
            CheckFileExists = false,
            // AddExtension = true,
            Filters =
            {
                new FileFilter(MiscResources.FileTypePdf, ".pdf"),
            },
            FileName = Path.GetFileName(defaultPath)
        };
        SetDir(sd, defaultPath);
        if (sd.ShowDialog(null) == DialogResult.Ok)
        {
            savePath = sd.FileName;
            return true;
        }
        savePath = null;
        return false;
    }

    public override bool PromptToSaveImage(string? defaultPath, out string savePath)
    {
        var sd = new SaveFileDialog
        {
            CheckFileExists = false,
            // AddExtension = true,
            // TODO: Move filter logic somewhere common
            Filters =
            {
                new FileFilter(MiscResources.FileTypeBmp, ".bmp"),
                new FileFilter(MiscResources.FileTypeEmf, ".emf"),
                new FileFilter(MiscResources.FileTypeExif, ".exif"),
                new FileFilter(MiscResources.FileTypeGif, ".gif"),
                new FileFilter(MiscResources.FileTypeJpeg, ".jpg", ".jpeg"),
                new FileFilter(MiscResources.FileTypePng, ".png"),
                new FileFilter(MiscResources.FileTypeTiff, ".tiff", ".tif"),
            },
            FileName = Path.GetFileName(defaultPath)
        };
        SetDir(sd, defaultPath);
        switch (_config.Get(c => c.LastImageExt)?.ToLowerInvariant())
        {
            case "bmp":
                sd.CurrentFilterIndex = 0;
                break;
            case "emf":
                sd.CurrentFilterIndex = 1;
                break;
            case "exif":
                sd.CurrentFilterIndex = 2;
                break;
            case "gif":
                sd.CurrentFilterIndex = 3;
                break;
            case "png":
                sd.CurrentFilterIndex = 5;
                break;
            case "tif":
            case "tiff":
                sd.CurrentFilterIndex = 6;
                break;
            default: // Jpeg
                sd.CurrentFilterIndex = 4;
                break;
        }
        if (sd.ShowDialog(null) == DialogResult.Ok)
        {
            savePath = sd.FileName;
            _config.User.Set(c => c.LastImageExt, (Path.GetExtension(sd.FileName) ?? "").Replace(".", ""));
            return true;
        }
        savePath = null;
        return false;
    }

    private void SetDir(SaveFileDialog dialog, string? defaultPath)
    {
        string? path = null;
        if (Paths.IsTestAppDataPath)
        {
            // For UI test automation we choose the appdata folder for test isolation and consistency
            path = Paths.AppData;
        }
        else
        {
            path = Path.IsPathRooted(defaultPath)
                ? Path.GetDirectoryName(defaultPath)
                : null;
        }
        if (path != null)
        {
            dialog.Directory = new Uri(Path.GetFullPath(path));
        }
    }
}