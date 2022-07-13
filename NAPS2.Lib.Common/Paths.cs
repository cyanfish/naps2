using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2;

public static class Paths
{
    private static readonly string ExecutablePath = Application.StartupPath;
    private static readonly string AppDataPath;
    private static readonly string TempPath;
    private static readonly string RecoveryPath;
    private static readonly string ComponentsPath;

    static Paths()
    {
#if STANDALONE
        AppDataPath = Path.Combine(ExecutablePath, "..", "Data");
#else
        AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NAPS2");
#endif
        var dataPathFromEnv = Environment.GetEnvironmentVariable("NAPS2_TEST_DATA");
        if (!string.IsNullOrEmpty(dataPathFromEnv))
        {
            AppDataPath = dataPathFromEnv;
        }
        var args = Environment.GetCommandLineArgs();
        var flagIndex = Array.IndexOf(args, "/Naps2TestData");
        if (flagIndex >= 0 && flagIndex < args.Length - 1)
        {
            AppDataPath = args[flagIndex + 1];
        }

        TempPath = Path.Combine(AppDataPath, "temp");
        RecoveryPath = Path.Combine(AppDataPath, "recovery");
        ComponentsPath = Path.Combine(AppDataPath, "components");
    }

    public static string AppData => EnsureFolderExists(AppDataPath);

    public static string Executable => EnsureFolderExists(ExecutablePath);

    public static string Temp => EnsureFolderExists(TempPath);

    public static string Recovery => EnsureFolderExists(RecoveryPath);

    public static string Components => EnsureFolderExists(ComponentsPath);

    /// <summary>
    /// Safely deletes the NAPS2 temp folder. If other NAPS2 or NAPS2.Console processes are running, the folder will be left alone.
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
            }
        }
        catch (Exception e)
        {
            Log.ErrorException("Error clearing temp files", e);
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