using System.Text.RegularExpressions;

namespace NAPS2.Tools.Project.Releasing;

public class SetVersionCommand : ICommand<SetVersionOptions>
{
    public int Run(SetVersionOptions opts)
    {
        var versionName = opts.VersionName!;
        if (!Regex.IsMatch(versionName, @"[1-9][0-9]*\.[0-9]+(\.[0-9]+|b[1-9][0-9]*)"))
        {
            throw new Exception("Invalid version format, expected X.Y.Z or X.YbZ");
        }
        var versionNumber = versionName.Replace("b", ".");

        Output.Info($"Setting version to {versionName} ({versionNumber})");

        var versionTargets = Path.Combine(Paths.Setup, "targets", "VersionTargets.targets");
        ReplaceInFile(
            versionTargets,
            @"<Version>.*</Version>",
            $"<Version>{versionNumber}</Version>");
        ReplaceInFile(
            versionTargets,
            @"<VersionName>.*</VersionName>",
            $"<VersionName>{versionName}</VersionName>");

        var macProj = Path.Combine(Paths.SolutionRoot, "NAPS2.App.Mac", "Info.plist");
        ReplaceInFile(
            macProj,
            $@"<key>CFBundleShortVersionString</key>{Environment.NewLine}    <string>.*</string>",
            $"<key>CFBundleShortVersionString</key>{Environment.NewLine}    <string>{versionNumber}</string>");

        Output.OperationEnd("Version set.");
        return 0;
    }

    private void ReplaceInFile(string path, string pattern, string replacement)
    {
        var text = File.ReadAllText(path);
        if (!Regex.IsMatch(text, pattern))
        {
            throw new Exception($"Could not find match for '{pattern}' in '{Path.GetFileName(path)}'");
        }
        text = Regex.Replace(text, pattern, replacement);
        File.WriteAllText(path, text);
    }
}