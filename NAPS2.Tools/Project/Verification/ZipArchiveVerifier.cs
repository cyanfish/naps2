using System.IO.Compression;

namespace NAPS2.Tools.Project.Verification;

public class ZipArchiveVerifier
{
    public static void Verify(string zipPath, bool noCleanup)
    {
        // TODO: We probably want other commands to use unique paths too
        var extractPath = Path.Combine(Paths.SetupObj, Path.GetRandomFileName());
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            Cli.Run("dotnet", "test NAPS2.App.Tests -f net462", new()
            {
                { "NAPS2_TEST_ROOT", Path.Combine(extractPath, "App") }
            });
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