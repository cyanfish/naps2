using NAPS2.Tools.Project.Installation;
using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Verification;

public static class MsiSetupVerifier
{
    public static void Verify(Platform platform, string version)
    {
        MsiInstaller.Install(platform, version, false);
        Verifier.RunVerificationTests(ProjectHelper.GetInstallationFolder(platform));

        var msiPath = ProjectHelper.GetPackagePath("msi", platform, version);
        Output.OperationEnd($"Verified msi installer: {msiPath}");
    }
}