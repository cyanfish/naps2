using CommandLine;

namespace NAPS2.Tools.Project.Releasing;

[Verb("upload", HelpText = "Upload the release binaries")]
public class UploadOptions : OptionsBase
{
    [Value(0, MetaName = "target", Required = true, HelpText = "github|sourceforge|all")]
    public string? Target { get; set; }
}