using Eto.Forms;
using NAPS2.ImportExport;

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
        var sd = CreateSaveFileDialog();
        sd.FileName = GetDefaultFileName(defaultPath, lastExt!);
        _fileFilters.Set(sd, FileFilterGroup.Pdf | FileFilterGroup.Image, lastExt);
        SetDir(sd, defaultPath);
        EtoPlatform.Current.ConfigureFileDialog(sd);
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
        var sd = CreateSaveFileDialog();
        sd.FileName = GetDefaultFileName(defaultPath, "pdf");
        _fileFilters.Set(sd, FileFilterGroup.Pdf);
        SetDir(sd, defaultPath);
        EtoPlatform.Current.ConfigureFileDialog(sd);
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
        var sd = CreateSaveFileDialog();
        sd.FileName = GetDefaultFileName(defaultPath, lastExt!);
        var filterGroups = EtoPlatform.Current.IsGtk
            ? FileFilterGroup.AllImages | FileFilterGroup.Image
            : FileFilterGroup.Image;
        _fileFilters.Set(sd, filterGroups, lastExt);
        SetDir(sd, defaultPath);
        EtoPlatform.Current.ConfigureFileDialog(sd);
        if (sd.ShowDialog(null) == DialogResult.Ok)
        {
            savePath = sd.FileName;
            _config.User.Set(c => c.LastImageExt, (Path.GetExtension(sd.FileName) ?? "").Replace(".", ""));
            return true;
        }
        savePath = null;
        return false;
    }

    private string? GetDefaultFileName(string? defaultPath, string ext)
    {
        if (string.IsNullOrEmpty(defaultPath))
        {
            return _addExt ? $".{ext}" : null;
        }
        var normPath = NormalizePath(defaultPath);
        return _addExt && !Path.HasExtension(normPath)
            ? $"{normPath}.{ext}"
            : normPath;
    }

    private static SaveFileDialog CreateSaveFileDialog()
    {
        try
        {
            return new SaveFileDialog();
        }
        catch (FileNotFoundException) when (TrySetCurrentDirectoryForDialogFallback())
        {
            return new SaveFileDialog();
        }
    }

    private static OpenFileDialog CreateOpenFileDialog()
    {
        try
        {
            return new OpenFileDialog();
        }
        catch (FileNotFoundException) when (TrySetCurrentDirectoryForDialogFallback())
        {
            return new OpenFileDialog();
        }
    }

    private static bool TrySetCurrentDirectoryForDialogFallback()
    {
        var fallbackDirectory = GetDialogFallbackDirectory();
        if (fallbackDirectory == null)
        {
            return false;
        }
        Environment.CurrentDirectory = fallbackDirectory;
        return true;
    }

    private static string? GetDialogFallbackDirectory()
    {
        var fallbackDirectory = Environment.GetFolderPath(OperatingSystem.IsWindows()
            ? Environment.SpecialFolder.MyDocuments
            : Environment.SpecialFolder.UserProfile);
        return Directory.Exists(fallbackDirectory) ? fallbackDirectory : null;
    }

    private void SetDir(FileDialog dialog, string? defaultPath)
    {
        string? path = null;
        if (Paths.IsTestAppDataPath)
        {
            // For UI test automation we choose the appdata folder for test isolation and consistency
            path = Paths.AppData;
        }
        else if (!string.IsNullOrEmpty(defaultPath) && Path.IsPathRooted(defaultPath))
        {
            path = Path.GetDirectoryName(NormalizePath(defaultPath));
        }
        path ??= GetDialogFallbackDirectory();
        if (path != null)
        {
            dialog.Directory = UriHelper.FilePathToFileUri(Path.GetFullPath(path));
        }
    }

    private static string NormalizePath(string path)
    {
        string normPath = Placeholders.NonNumeric.Substitute(path);
        // If the path points to a directory, it should end in a trailing slash.
        // Otherwise, path functions will assume that the directory name is a file name.
        if (Directory.Exists(normPath) && !normPath.EndsWith(Path.DirectorySeparatorChar))
        {
            normPath += Path.DirectorySeparatorChar;
        }
        return normPath;
    }

    public override bool PromptToImport(out string[]? filePaths)
    {
        var ofd = CreateOpenFileDialog();
        ofd.MultiSelect = true;
        ofd.CheckFileExists = true;
        _fileFilters.Set(ofd,
            FileFilterGroup.AllFiles | FileFilterGroup.Pdf | FileFilterGroup.AllImages | FileFilterGroup.Image);
        SetDir(ofd, defaultPath: null);
        EtoPlatform.Current.ConfigureFileDialog(ofd);
        if (ofd.ShowDialog(null) == DialogResult.Ok)
        {
            filePaths = ofd.Filenames.ToArray();
            return true;
        }
        filePaths = null;
        return false;
    }
}
