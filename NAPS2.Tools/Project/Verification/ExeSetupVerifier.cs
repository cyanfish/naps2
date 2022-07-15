using NAPS2.Tools.Project.Installation;
using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Verification;

public static class ExeSetupVerifier
{
    public static void Verify(Platform platform, string version, bool verbose)
    {
        ExeInstaller.Install(platform, version, false, verbose);
        Verifier.RunVerificationTests(ProjectHelper.GetInstallationFolder(platform), verbose);

        var exePath = ProjectHelper.GetPackagePath("exe", platform, version);
        Console.WriteLine(verbose ? $"Verified exe installer: {exePath}" : "Done.");
    }
}