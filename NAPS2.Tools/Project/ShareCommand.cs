using System.Text.RegularExpressions;

namespace NAPS2.Tools.Project;

public class ShareCommand : ICommand<ShareOptions>
{
    private static readonly string[] Extensions = [".exe", ".msi", ".zip", ".pkg", ".flatpak", ".deb", ".rpm"];

    public int Run(ShareOptions opts)
    {
        bool doIn = opts.ShareType is "both" or "in";
        bool doOut = opts.ShareType is "both" or "out";

        var version = ProjectHelper.GetCurrentVersionName();

        var syncBaseFolder = N2Config.ShareDir;
        if (IsSshPath(syncBaseFolder))
        {
            return RunRsync(syncBaseFolder, version, doIn, doOut);
        }
        if (!Directory.Exists(syncBaseFolder))
        {
            throw new InvalidOperationException($"Sync folder does not exist: {syncBaseFolder}");
        }

        var syncFolder = Path.Combine(syncBaseFolder, version);
        if (!Directory.Exists(syncFolder)) Directory.CreateDirectory(syncFolder);

        var localFolder = Path.Combine(Paths.Publish, version);
        if (!Directory.Exists(localFolder)) Directory.CreateDirectory(localFolder);

        Output.Info($"Syncing {localFolder} {GetArrow(doIn, doOut)} {syncFolder}");

        if (doIn)
        {
            foreach (var file in GetFiles(syncFolder))
            {
                CopyFileIfNewer(file, localFolder);
            }
        }
        if (doOut)
        {
            foreach (var file in GetFiles(localFolder))
            {
                CopyFileIfNewer(file, syncFolder);
            }
        }
        Output.Info("Done.");
        return 0;
    }

    // e.g. "user@host:some/path". Requires "user@" or a multi-char host so Windows drive letters aren't matched.
    private static bool IsSshPath(string path) =>
        Regex.IsMatch(path, @"^([^/\\:@\s]+@[^/\\:@\s]+|[^/\\:@\s]{2,}):");

    private static int RunRsync(string syncBaseFolder, string version, bool doIn, bool doOut)
    {
        var syncFolder = $"{syncBaseFolder.TrimEnd('/')}/{version}/";
        var localFolder = Path.Combine(Paths.Publish, version);
        if (!Directory.Exists(localFolder)) Directory.CreateDirectory(localFolder);
        Output.Info($"Syncing {localFolder} {GetArrow(doIn, doOut)} {syncFolder}");

        // -r is needed to sync a directory's contents (subdirectories are still skipped by --exclude=*),
        // -t preserves mtimes, -u skips files that are newer on the receiver
        var filters = string.Join(" ", Extensions.Select(ext => $"--include=*{ext}")) + " --exclude=*";
        var args = $"-rtu {(Output.EnableVerbose ? "-v " : "")}{filters}";
        // Use relative local paths (via the working dir) so they can't be mistaken for remote paths.
        // Out goes first as it creates the remote version folder if needed.
        if (doOut) Cli.Run("rsync", $"{args} ./ {syncFolder}", workingDir: localFolder);
        if (doIn) Cli.Run("rsync", $"{args} {syncFolder} ./", workingDir: localFolder);
        Output.Info("Done.");
        return 0;
    }

    private static string GetArrow(bool doIn, bool doOut) => $"{(doIn ? "<" : "")}-{(doOut ? ">" : "")}";

    private static void CopyFileIfNewer(FileInfo file, string targetFolder)
    {
        var targetFile = new FileInfo(Path.Combine(targetFolder, file.Name));
        if (!targetFile.Exists || targetFile.LastWriteTimeUtc < file.LastWriteTimeUtc)
        {
            if (targetFile.Exists)
            {
                var tempPath = targetFile.FullName + ".old";
                File.Move(targetFile.FullName, tempPath);
                try
                {
                    Output.Info($"Replacing {file.FullName} -> {targetFile.FullName}");
                    file.CopyTo(targetFile.FullName);
                    File.Delete(tempPath);
                }
                catch (Exception)
                {
                    File.Move(tempPath, targetFile.FullName);
                    throw;
                }
            }
            else
            {
                Output.Info($"Copying {file.FullName} -> {targetFile.FullName}");
                file.CopyTo(targetFile.FullName);
            }
        }
        else
        {
            var time = targetFile.LastWriteTimeUtc == file.LastWriteTimeUtc ? "same" : "older";
            Output.Verbose($"Ignoring {file.FullName} ({time})");
        }
    }

    private static IEnumerable<FileInfo> GetFiles(string folderPath)
    {
        return new DirectoryInfo(folderPath).EnumerateFiles()
            .Where(x => Extensions.Contains(x.Extension));
    }
}