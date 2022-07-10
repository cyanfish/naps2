using System.IO.Compression;

namespace NAPS2.Tools.Project.Verification;

public class ZipArchiveVerifier
{
    public static void Verify(Platform platform, string version, bool verbose, bool noCleanup)
    {
        var zipPath = ProjectHelper.GetPackagePath("zip", platform, version);
        Console.WriteLine($"Extracting zip archive: {zipPath}");
        // TODO: We probably want other commands to use unique paths too
        var extractPath = Path.Combine(Paths.SetupObj, Path.GetRandomFileName());
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            Verifier.RunVerificationTests(Path.Combine(extractPath, "App"), verbose);
            Console.WriteLine(verbose ? $"Verified zip archive: {zipPath}" : "Done.");
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