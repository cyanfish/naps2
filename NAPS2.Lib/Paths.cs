namespace NAPS2;

public static class Paths
{
    private static readonly string ExecutablePath = AssemblyHelper.EntryFolder;
    private static readonly string AppDataPath;
    private static readonly string TempPath;
    private static readonly string RecoveryPath;
    private static readonly string ComponentsPath;

    static Paths()
    {
#if ZIP
        AppDataPath = Path.Combine(ExecutablePath, "..", "Data");
#else
        var userAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#if NET6_0_OR_GREATER
        var subfolder = OperatingSystem.IsWindows() ? "NAPS2" : "naps2";
        if (string.IsNullOrEmpty(userAppData) && OperatingSystem.IsMacOS())
        {
            // Not sure if this is necessary but older macOS (10.15) didn't seem to get the appdata path
            // TODO: Technically this should be "%HOME%/.config" but keeping this for now, for backwards compatibility
            userAppData = Environment.ExpandEnvironmentVariables("/Users/%USER%/.config");
        }
        else if (OperatingSystem.IsMacOS())
        {
            // For .NET 8, the macOS ApplicationData path has changed from the Linux-style "~/.config" to the more
            // correct "~/Library/Application Support". For backwards compatibility we keep .config for now.
            // TODO: Migrate macOS application data to "~/Library/Application Support"
            userAppData = Environment.ExpandEnvironmentVariables("%HOME%/.config");
        }
#else
        var subfolder = "NAPS2";
#endif
        AppDataPath = Path.Combine(userAppData, subfolder);
#endif
        var dataPathFromEnv = Environment.GetEnvironmentVariable("NAPS2_TEST_DATA");
        if (!string.IsNullOrEmpty(dataPathFromEnv))
        {
            AppDataPath = dataPathFromEnv;
            IsTestAppDataPath = true;
        }
        var args = Environment.GetCommandLineArgs();
        var flagIndex = Array.IndexOf(args, "/Naps2TestData");
        if (flagIndex >= 0 && flagIndex < args.Length - 1)
        {
            AppDataPath = args[flagIndex + 1];
            IsTestAppDataPath = true;
        }

        TempPath = Path.Combine(AppDataPath, "temp");
        RecoveryPath = Path.Combine(AppDataPath, "recovery");
        ComponentsPath = Path.Combine(AppDataPath, "components");
    }

    /// <summary>
    /// Whether we're in a test and NAPS2_TEST_DATA or /Naps2TestData is set. 
    /// </summary>
    public static readonly bool IsTestAppDataPath;

    public static string AppData => EnsureFolderExists(AppDataPath);

    public static string Executable => EnsureFolderExists(ExecutablePath);

    public static string Temp => EnsureFolderExists(TempPath);

    public static string Recovery => EnsureFolderExists(RecoveryPath);

    public static string Components => EnsureFolderExists(ComponentsPath);

    /// <summary>
    /// Safely clears the NAPS2 temp folder. If other NAPS2 or NAPS2.Console processes are running, the folder will be left alone.
    /// </summary>
    public static void ClearTemp()
    {
        try
        {
            if (!Directory.Exists(TempPath)) return;

            var otherNaps2Processes = Process.GetProcesses().Where(x =>
                x.ProcessName.IndexOf("NAPS2", StringComparison.OrdinalIgnoreCase) >= 0 &&
                x.Id != Process.GetCurrentProcess().Id);
            if (!otherNaps2Processes.Any())
            {
                Directory.Delete(TempPath, true);
                Directory.CreateDirectory(TempPath);
            }
        }
        catch (Exception)
        {
            // Ignore errors clearing temp files
        }
    }

    private static string EnsureFolderExists(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        return folderPath;
    }
}