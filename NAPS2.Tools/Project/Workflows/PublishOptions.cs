using CommandLine;

namespace NAPS2.Tools.Project.Workflows;

[Verb("publish", HelpText = "Build, test, package, and verify standard targets")]
public class PublishOptions : OptionsBase
{
    [Value(0, MetaName = "build type", Required = false, HelpText = "all|exe|msi|zip")]
    public string? BuildType { get; set; }
    
    [Option('p', "platform", Required = false, HelpText = "all|win|win32|win64|mac|macintel|macarm|linux")]
    public string? Platform { get; set; }

    [Option("nocleanup", Required = false, HelpText = "Skip cleaning up temp files")]
    public bool NoCleanup { get; set; }
}