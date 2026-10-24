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
        while (sd.ShowDialog(null) == DialogResult.Ok)
        {
            savePath = sd.FileName;
            if (EtoPlatform.Current.IsGtk)
            {
                savePath = CompleteImageFileName(sd.FileName, sd.CurrentFilter?.Extensions ?? []);
                if (savePath == null)
                {
                    MessageBox.Show(MiscResources.ChooseImageFileType, MessageBoxType.Warning);
                    continue;
                }
                if (savePath != sd.FileName && File.Exists(savePath))
                {
                    // The native dialog only confirmed the original name. Show the final
                    // target again so its overwrite confirmation covers the appended extension.
                    sd.FileName = savePath;
                    continue;
                }
            }
            _config.User.Set(c => c.LastImageExt, (Path.GetExtension(savePath) ?? "").Replace(".", ""));
            return true;
        }
        savePath = null;
        return false;
    }

    internal static string? CompleteImageFileName(string fileName, IEnumerable<string> extensions)
    {
        if (Path.HasExtension(fileName))
        {
            return fileName;
        }
        var filterExtensions = extensions.ToArray();
        var formats = filterExtensions.Select(ImageContext.GetFileFormatFromExtension).Distinct().ToArray();
        // Multiple extensions may be aliases for one format, such as .jpg and .jpeg.
        // An aggregate filter cannot identify which format the user intended.
        return formats.Length == 1 && formats[0] != ImageFileFormat.Unknown
            ? fileName + filterExtensions[0]
            : null;
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
        catch (FileNotFoundException)
        {
            SetCurrentDirectoryForDialogFallback();
            return new SaveFileDialog();
        }
    }

    private static OpenFileDialog CreateOpenFileDialog()
    {
        try
        {
            return new OpenFileDialog();
        }
        catch (FileNotFoundException)
        {
            SetCurrentDirectoryForDialogFallback();
            return new OpenFileDialog();
        }
    }

    private static void SetCurrentDirectoryForDialogFallback()
    {
        // Eto.GtkSharp throws an exception when creating dialogs if CurrentDirectory doesn't exist, so fall back to
        // a known good directory.
        Environment.CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    private void SetDir(SaveFileDialog dialog, string? defaultPath)
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
        if (Paths.IsTestAppDataPath)
        {
            // For UI test automation we choose the appdata folder to find the prepared files to import
            ofd.Directory = UriHelper.FilePathToFileUri(Path.GetFullPath(Paths.AppData));
        }
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