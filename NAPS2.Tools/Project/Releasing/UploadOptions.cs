using CommandLine;

namespace NAPS2.Tools.Project.Releasing;

[Verb("upload", HelpText = "Upload the release binaries")]
public class UploadOptions : OptionsBase
{
    [Value(0, MetaName = "target", Required = true, HelpText = "github|sourceforge|static|sdk|all")]
    public string? Target { get; set; }

    [Option("version", Required = false, HelpText = "Version to upload")]
    public string? Version { get; set; }

    [Option("allow-old", Required = false, HelpText = "Allow old files")]
    public bool AllowOld { get; set; }

    [Option('t', "package-type", Required = false, HelpText = "all|exe|msi|zip|flatpak|pkg|deb|rpm")]
    public string? PackageType { get; set; }
}