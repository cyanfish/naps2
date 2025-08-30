using CommandLine;

namespace NAPS2.Tools.Project.Workflows;

[Verb("publish", HelpText = "Build, test, package, and verify standard targets")]
public class PublishOptions : OptionsBase
{
    [Value(0, MetaName = "package type", Required = false, HelpText = "all|exe|msi|zip|flatpak|pkg|deb|rpm")]
    public string? PackageType { get; set; }
    
    [Option('p', "platform", Required = false, HelpText = "all|win|win32|win64|mac|macintel|macarm|linux")]
    public string? Platform { get; set; }

    [Option("noverify", Required = false, HelpText = "Don't run verification tests")]
    public bool NoVerify { get; set; }

    [Option("nogui", Required = false, HelpText = "Only run headless (no gui) tests")]
    public bool NoGui { get; set; }

    [Option("xcompile", Required = false, HelpText = "Cross-compile packages where possible (e.g. build linux-arm64 on linux-x64)")]
    public bool XCompile { get; set; }
}