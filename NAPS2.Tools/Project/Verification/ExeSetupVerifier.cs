using NAPS2.Tools.Project.Installation;
using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Verification;

public static class ExeSetupVerifier
{
    public static void Verify(Platform platform, string version)
    {
        // Verify upgrades delete old files, including moving from "Program Files (x86)" to "Program Files".
        using var upgradeTest = new UpgradeTest(platform);

        ExeInstaller.Install(platform, version, false);
        Verifier.RunVerificationTests(ProjectHelper.GetInstallationFolder(platform));

        upgradeTest.Verify();

        var exePath = ProjectHelper.GetPackagePath("exe", platform, version);
        Output.OperationEnd($"Verified exe installer: {exePath}");
    }

    private class UpgradeTest : IDisposable
    {
        private readonly Platform _platform;
        private readonly string _install32;
        private readonly string _testFilePath;

        public UpgradeTest(Platform platform)
        {
            _platform = platform;
            _install32 = Path.Combine(Environment.ExpandEnvironmentVariables("%PROGRAMFILES(X86)%"), "NAPS2");
            _testFilePath = Path.Combine(_install32, "_verify_testfile.exe");
            Directory.CreateDirectory(_install32);
            File.WriteAllText(_testFilePath, "");
        }

        public void Verify()
        {
            if (File.Exists(_testFilePath))
            {
                throw new Exception("Verification error: Exe installer did not delete old files");
            }
            if (Directory.Exists(_install32))
            {
                throw new Exception("Verification error: Exe installer did not delete old install dir");
            }
        }

        public void Dispose()
        {
            try
            {
                File.Delete(_testFilePath);
            }
            catch (IOException)
            {
            }
        }
    }
}