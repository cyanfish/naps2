namespace NAPS2.Tools.Project.Verification;

public static class MsiSetupVerifier
{
    public static void Verify(Platform platform, string version, bool verbose)
    {
        // TODO: Clear the installation folder / uninstall previous installers first
        var msiPath = ProjectHelper.GetPackagePath("msi", platform, version);
        Console.WriteLine($"Starting msi installer: {msiPath}");
        // TODO: Using /QN doesn't prompt for elevation if needed (could use /QB or /QR or get elevation ourselves)
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

        Verifier.RunVerificationTests(ProjectHelper.GetInstallationFolder(platform), verbose);
        Console.WriteLine(verbose ? $"Verified msi installer: {msiPath}" : "Done.");
    }
}