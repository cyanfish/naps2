using System.Windows.Forms;

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
            OverwritePrompt = false,
            AddExtension = true,
            // TODO: Move filter logic somewhere common
            Filter = MiscResources.FileTypePdf + @"|*.pdf|" +
                     MiscResources.FileTypeBmp + @"|*.bmp|" +
                     MiscResources.FileTypeEmf + @"|*.emf|" +
                     MiscResources.FileTypeExif + @"|*.exif|" +
                     MiscResources.FileTypeGif + @"|*.gif|" +
                     MiscResources.FileTypeJpeg + @"|*.jpg;*.jpeg|" +
                     MiscResources.FileTypePng + @"|*.png|" +
                     MiscResources.FileTypeTiff + @"|*.tiff;*.tif",
            FileName = Path.GetFileName(defaultPath),
            InitialDirectory = GetDir(defaultPath)
        };
        if (sd.ShowDialog() == DialogResult.OK)
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
            OverwritePrompt = false,
            AddExtension = true,
            Filter = MiscResources.FileTypePdf + @"|*.pdf",
            FileName = Path.GetFileName(defaultPath),
            InitialDirectory = GetDir(defaultPath)
        };
        if (sd.ShowDialog() == DialogResult.OK)
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
            OverwritePrompt = false,
            AddExtension = true,
            // TODO: Move filter logic somewhere common
            Filter = MiscResources.FileTypeBmp + @"|*.bmp|" +
                     MiscResources.FileTypeEmf + @"|*.emf|" +
                     MiscResources.FileTypeExif + @"|*.exif|" +
                     MiscResources.FileTypeGif + @"|*.gif|" +
                     MiscResources.FileTypeJpeg + @"|*.jpg;*.jpeg|" +
                     MiscResources.FileTypePng + @"|*.png|" +
                     MiscResources.FileTypeTiff + @"|*.tiff;*.tif",
            FileName = Path.GetFileName(defaultPath),
            InitialDirectory = GetDir(defaultPath)
        };
        switch (_config.Get(c => c.LastImageExt)?.ToLowerInvariant())
        {
            case "bmp":
                sd.FilterIndex = 1;
                break;
            case "emf":
                sd.FilterIndex = 2;
                break;
            case "exif":
                sd.FilterIndex = 3;
                break;
            case "gif":
                sd.FilterIndex = 4;
                break;
            case "png":
                sd.FilterIndex = 6;
                break;
            case "tif":
            case "tiff":
                sd.FilterIndex = 7;
                break;
            default: // Jpeg
                sd.FilterIndex = 5;
                break;
        }
        if (sd.ShowDialog() == DialogResult.OK)
        {
            savePath = sd.FileName;
            _config.User.Set(c => c.LastImageExt, (Path.GetExtension(sd.FileName) ?? "").Replace(".", ""));
            return true;
        }
        savePath = null;
        return false;
    }

    private string? GetDir(string? defaultPath)
    {
        return Path.IsPathRooted(defaultPath)
            ? Path.GetDirectoryName(defaultPath)
            : "";
    }
}