using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("pkg", HelpText = "Package the project, 'pkg {all|exe|msi|zip|7z}'")]
public class PackageOptions
{
    [Value(0, MetaName = "what", Required = true, HelpText = "all|exe|msi|zip|7z")]
    public string? What { get; set; }
}