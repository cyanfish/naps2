namespace NAPS2.Tools.Project.Verification;

public static class ExeSetupVerifier
{
    public static void Verify(Platform platform, string version, bool verbose)
    {
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

        Verifier.RunVerificationTests(ProjectHelper.GetInstallationFolder(platform), verbose);
        Console.WriteLine(verbose ? $"Verified exe installer: {exePath}" : "Done.");
    }
}