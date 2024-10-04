namespace NAPS2.Tools.Project.Packaging;

public static class WindowsSigning
{
    public static void SignContents(PackageInfo packageInfo)
    {
        // Exclude resource DLLs from signing as that saves 40% time/space and doesn't really provide any value.
        // TODO: Maybe reevaluate this
        foreach (var batch in packageInfo.Files.Where(file => !file.FileName.EndsWith(".resources.dll")).Chunk(10))
        {
            var files = string.Join(" ",
                batch
                    .Where(file => Path.GetExtension(file.FileName) is ".exe" or ".dll")
                    .Select(file => $"\"{file.SourcePath}\""));
            if (files.Length > 0)
            {
                Cli.Run("signtool",
                    $"sign /tr http://timestamp.globalsign.com/tsa/r6advanced1 /td sha256 /fd sha256 /a {files}");
            }
        }
    }

    public static void SignFile(string path)
    {
        Cli.Run("signtool",
            $"sign /tr http://timestamp.globalsign.com/tsa/r6advanced1 /td sha256 /fd sha256 /a \"{path}\"");
    }
}