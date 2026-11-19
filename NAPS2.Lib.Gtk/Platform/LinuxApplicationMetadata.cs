namespace NAPS2.Platform;

public static class LinuxApplicationMetadata
{
    private const string UNQUALIFIED_APP_ID = "naps2";
    private const string QUALIFIED_APP_ID = "com.naps2.Naps2";
    private const string APP_NAME = "NAPS2";

    /// <summary>
    /// Sets the GLib program name and application name. This must be called after Gtk is initialized, as that resets
    /// the program name.
    /// </summary>
    public static void Set()
    {
        GLib.Global.ProgramName = UseQualifiedAppId() ? QUALIFIED_APP_ID : UNQUALIFIED_APP_ID;
        GLib.Global.ApplicationName = APP_NAME;
    }

    private static bool UseQualifiedAppId()
    {
        if (File.Exists("/.flatpak-info"))
        {
            return true;
        }
        // Default to the unqualified app id, but otherwise base on the presence of a .desktop file
        var appDirs = GetXdgDataDirs().Select(dir => Path.Combine(dir, "applications")).ToList();
        bool HasDesktopFile(string id) => appDirs.Any(dir => File.Exists(Path.Combine(dir, $"{id}.desktop")));
        return !HasDesktopFile(UNQUALIFIED_APP_ID) && HasDesktopFile(QUALIFIED_APP_ID);
    }

    private static IEnumerable<string> GetXdgDataDirs()
    {
        var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        yield return string.IsNullOrEmpty(dataHome)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share")
            : dataHome;
        var dataDirs = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
        foreach (var dir in (string.IsNullOrEmpty(dataDirs) ? "/usr/local/share:/usr/share" : dataDirs)
                     .Split(':', StringSplitOptions.RemoveEmptyEntries))
        {
            yield return dir;
        }
    }
}
