namespace NAPS2.Tools.Project.Verification;

public static class MsiInstaller
{
    public static void Install(Platform platform, string version, bool verbose)
    {
        if (!ProjectHelper.RequireElevation()) return;
        ProjectHelper.DeleteInstallationFolder(platform);

        var msiPath = ProjectHelper.GetPackagePath("msi", platform, version);
        Console.WriteLine($"Starting msi installer: {msiPath}");

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "msiexec.exe",
            Arguments = $"/i \"{msiPath}\" /QN"
        });
        if (process == null)
        {
            throw new Exception($"Could not start installer: {msiPath}");
        }
        process.WaitForExit();

        // For consistency with exe (which auto starts the application), start it here too
        Process.Start(Path.Combine(ProjectHelper.GetInstallationFolder(platform), "NAPS2.exe"));

        Console.WriteLine("Installed.");
    }
}