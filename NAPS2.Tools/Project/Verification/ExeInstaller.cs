namespace NAPS2.Tools.Project.Verification;

public static class ExeInstaller
{
    public static void Install(Platform platform, string version, bool verbose)
    {
        if (!ProjectHelper.RequireElevation()) return;
        ProjectHelper.DeleteInstallationFolder(platform);

        var exePath = ProjectHelper.GetPackagePath("exe", platform, version);
        Console.WriteLine($"Starting exe installer: {exePath}");

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

        Console.WriteLine("Installed.");
    }
}