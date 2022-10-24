using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("share", HelpText = "Syncs the packaged files with OneDrive, 'share {both|in|out}'")]
public class ShareOptions
{
    [Value(0, MetaName = "share type", Default = "both", Required = false, HelpText = "both|in|out")]
    public string? ShareType { get; set; }

    [Option('v', "verbose", Required = false, HelpText = "Show full output")]
    public bool Verbose { get; set; }
}