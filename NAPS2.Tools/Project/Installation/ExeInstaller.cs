using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Installation;

public static class ExeInstaller
{
    public static void Install(Platform platform, string version, bool run)
    {
        ProjectHelper.DeleteInstallationFolder(Platform.Win64);

        var exePath = ProjectHelper.GetPackagePath("exe", platform, version);
        Output.Info($"Starting exe installer: {exePath}");

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = "/SILENT /CLOSEAPPLICATIONS"
        });
        if (process == null)
        {
            throw new Exception($"Could not start installer: {exePath}");
        }
        process.WaitForExit();

        if (!run)
        {
            ProjectHelper.CloseMostRecentNaps2();
        }

        Output.Info("Installed.");
    }
}