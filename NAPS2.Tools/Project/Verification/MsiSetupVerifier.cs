namespace NAPS2.Tools.Project.Verification;

public static class MsiSetupVerifier
{
    public static void Verify(Platform platform, string version, bool verbose)
    {
        MsiInstaller.Install(platform, version, false, verbose);
        Verifier.RunVerificationTests(ProjectHelper.GetInstallationFolder(platform), verbose);

        var msiPath = ProjectHelper.GetPackagePath("msi", platform, version);
        Console.WriteLine(verbose ? $"Verified msi installer: {msiPath}" : "Done.");
    }
}