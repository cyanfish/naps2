using Eto.Forms;

namespace NAPS2.EtoForms;

public class EtoDialogHelper : DialogHelper
{
    private readonly Naps2Config _config;
    private readonly FileFilters _fileFilters;
    private bool _addExt = EtoPlatform.Current.IsGtk;

    public EtoDialogHelper(Naps2Config config, FileFilters fileFilters)
    {
        _config = config;
        _fileFilters = fileFilters;
    }

    public override bool PromptToSavePdfOrImage(string? defaultPath, out string? savePath)
    {
        var lastExt = _config.Get(c => c.LastPdfOrImageExt)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(lastExt))
        {
            lastExt = "pdf";
        }
        var defaultFileName = _addExt ? $".{lastExt}" : null;
        var sd = new SaveFileDialog
        {
            FileName = Path.IsPathRooted(defaultPath) ? Path.GetFileName(defaultPath) : defaultFileName
        };
        _fileFilters.Set(sd, FileFilterGroup.Pdf | FileFilterGroup.Image, lastExt);
        SetDir(sd, defaultPath);
        if (sd.ShowDialog(null) == DialogResult.Ok)
        {
            savePath = sd.FileName;
            _config.User.Set(c => c.LastPdfOrImageExt, (Path.GetExtension(sd.FileName) ?? "").Replace(".", ""));
            return true;
        }
        savePath = null;
        return false;
    }

    public override bool PromptToSavePdf(string? defaultPath, out string? savePath)
    {
        var defaultFileName = _addExt ? $".pdf" : null;
        var sd = new SaveFileDialog
        {
            FileName = Path.IsPathRooted(defaultPath) ? Path.GetFileName(defaultPath) : defaultFileName
        };
        _fileFilters.Set(sd, FileFilterGroup.Pdf);
        SetDir(sd, defaultPath);
        if (sd.ShowDialog(null) == DialogResult.Ok)
        {
            savePath = sd.FileName;
            return true;
        }
        savePath = null;
        return false;
    }

    public override bool PromptToSaveImage(string? defaultPath, out string? savePath)
    {
        var lastExt = _config.Get(c => c.LastImageExt)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(lastExt))
        {
            lastExt = "jpg";
        }
        var defaultFileName = _addExt ? $".{lastExt}" : null;
        var sd = new SaveFileDialog
        {
            FileName = Path.IsPathRooted(defaultPath) ? Path.GetFileName(defaultPath) : defaultFileName
        };
        var filterGroups = EtoPlatform.Current.IsGtk
            ? FileFilterGroup.AllImages | FileFilterGroup.Image
            : FileFilterGroup.Image;
        _fileFilters.Set(sd, filterGroups, lastExt);
        SetDir(sd, defaultPath);
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
        string? path;
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

    public override bool PromptToImport(out string[]? filePaths)
    {
        var ofd = new OpenFileDialog
        {
            MultiSelect = true,
            CheckFileExists = true
        };
        _fileFilters.Set(ofd,
            FileFilterGroup.AllFiles | FileFilterGroup.Pdf | FileFilterGroup.AllImages | FileFilterGroup.Image);
        if (Paths.IsTestAppDataPath)
        {
            // For UI test automation we choose the appdata folder to find the prepared files to import
            ofd.Directory = new Uri(Path.GetFullPath(Paths.AppData));
        }
        if (ofd.ShowDialog(null) == DialogResult.Ok)
        {
            filePaths = ofd.Filenames.ToArray();
            return true;
        }
        filePaths = null;
        return false;
    }
}