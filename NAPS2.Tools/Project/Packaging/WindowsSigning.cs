using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace NAPS2.Tools.Project.Packaging;

public static class WindowsSigning
{
    public static void SignContents(PackageInfo packageInfo)
    {
        // Exclude resource DLLs from signing as that saves 40% time/space and doesn't really provide any value.
        // TODO: Maybe reevaluate this
        foreach (var batch in packageInfo.Files
                     .Where(file => Path.GetExtension(file.FileName) is ".exe" or ".dll")
                     .Where(file => !file.FileName.EndsWith(".resources.dll"))
                     .Where(NeedsSignature)
                     .Chunk(10))
        {
            var files = string.Join(" ", batch.Select(file => $"\"{file.SourcePath}\""));
            if (files.Length > 0)
            {
                Cli.Run("signtool",
                    $"sign /tr http://timestamp.globalsign.com/tsa/r6advanced1 /td sha256 /fd sha256 /a /as {files}");
            }
        }
    }

    private static bool NeedsSignature(PackageFile file)
    {
        if (Path.GetExtension(file.FileName) == ".exe")
        {
            return true;
        }
        try
        {
            AssemblyName.GetAssemblyName(file.SourcePath);
        }
        catch (Exception)
        {
            // Not a .NET assembly
            return false;
        }
        try
        {
            X509Certificate.CreateFromSignedFile(file.SourcePath);
            // Already has a signature
            return false;
        }
        catch (Exception)
        {
            // No signature
            return true;
        }
    }

    public static void SignFile(string path)
    {
        Cli.Run("signtool",
            $"sign /tr http://timestamp.globalsign.com/tsa/r6advanced1 /td sha256 /fd sha256 /a \"{path}\"");
    }
}