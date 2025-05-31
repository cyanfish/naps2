using System.Text.RegularExpressions;
using Gdk;
using Gtk;
using NAPS2.Images.Gtk;

namespace NAPS2.Platform;

public class LinuxOpenWith : IOpenWith
{
    private readonly ImageContext _imageContext;

    public LinuxOpenWith(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public IEnumerable<OpenWithEntry> GetEntries(string fileExt)
    {
        string mimeType = fileExt switch
        {
            ".jpg" => "image/jpeg",
            _ => throw new NotSupportedException("Unsupported mime type/extension")
        };
        var desktopFiles = new List<FileInfo>();
        var systemAppDir = new DirectoryInfo("/usr/share/applications");
        var userAppDir = new DirectoryInfo(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local/share/applications"));
        if (systemAppDir.Exists)
        {
            desktopFiles.AddRange(systemAppDir.EnumerateFiles("*.desktop"));
        }
        if (userAppDir.Exists)
        {
            desktopFiles.AddRange(userAppDir.EnumerateFiles("*.desktop"));
        }
        foreach (var file in desktopFiles)
        {
            var data = ParseDesktopFile(file);
            var mimeTypes = data.Get("MimeType", "").Split(";");
            if (!mimeTypes.Contains(mimeType)) continue;
            yield return new OpenWithEntry(
                file.FullName,
                data.Get("Name") ?? Path.GetFileNameWithoutExtension(file.Name),
                data.Get("Icon") ?? "",
                0);
        }
    }

    private Dictionary<string, string> ParseDesktopFile(FileInfo file)
    {
        var data = new Dictionary<string, string>();
        using var stream = file.OpenText();
        while (stream.ReadLine() is { } line)
        {
            var parts = line.Split("=");
            if (parts.Length == 2)
            {
                data[parts[0]] = parts[1].TrimEnd();
            }
        }
        return data;
    }

    public void OpenWith(string entryId, IEnumerable<string> filePaths)
    {
        var data = ParseDesktopFile(new FileInfo(entryId));
        var exec = data.Get("Exec");
        if (exec == null) throw new InvalidOperationException($"Could not find Exec for {entryId}");
        var parts = exec.Split(" ", 2);
        string exe = parts[0];
        string argsSpec = parts.Length > 1 ? parts[1]: "";
        // https://specifications.freedesktop.org/desktop-entry-spec/latest/exec-variables.html
        var match = Regex.Match(argsSpec, "%[ufUF]");
        string expandedFilePaths = string.Join(" ", filePaths.Select(path => $"\"{path}\""));
        string args = match.Success ? match.Result(expandedFilePaths) : $"{argsSpec} {expandedFilePaths}";
        Process.Start(exe, args);
    }

    public IMemoryImage? LoadIcon(OpenWithEntry entry)
    {
        if (entry.IconPath.StartsWith("/"))
        {
            return _imageContext.Load(entry.IconPath);
        }
        Pixbuf? resolvedIcon = IconTheme.Default.LookupIcon(entry.IconPath, 64, IconLookupFlags.UseBuiltin)?.LoadIcon();
        if (resolvedIcon != null)
        {
            return new GtkImage(resolvedIcon);
        }
        return null;
    }
}