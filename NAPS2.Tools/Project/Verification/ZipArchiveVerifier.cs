using System.IO.Compression;
using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Verification;

public class ZipArchiveVerifier
{
    public static void Verify(Platform platform, string version, bool noCleanup)
    {
        var zipPath = ProjectHelper.GetPackagePath("zip", platform, version);
        Output.Info($"Extracting zip archive: {zipPath}");
        // TODO: We probably want other commands to use unique paths too
        var extractPath = Path.Combine(Paths.SetupObj, Path.GetRandomFileName());
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            Verifier.RunVerificationTests(Path.Combine(extractPath, "App"));
            Output.OperationEnd($"Verified zip archive: {zipPath}");
        }
        finally
        {
            if (!noCleanup)
            {
                Directory.Delete(extractPath, true);
            }
        }
    }
}