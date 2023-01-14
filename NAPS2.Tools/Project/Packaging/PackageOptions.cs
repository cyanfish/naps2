using CommandLine;

namespace NAPS2.Tools.Project.Packaging;

[Verb("pkg", HelpText = "Package the project, 'pkg {all|exe|msi|zip}'")]
public class PackageOptions : OptionsBase
{
    [Value(0, MetaName = "build type", Required = false, HelpText = "all|exe|msi|zip")]
    public string? BuildType { get; set; }

    [Option('p', "platform", Required = false, HelpText = "win|win32|win64|mac|macintel|macarm|linux")]
    public string? Platform { get; set; }

    [Option("build", Required = false, HelpText = "Build before packaging")]
    public bool Build { get; set; }

    [Option("nopre", Required = false, HelpText = "Skip pre-packaging steps")]
    public bool NoPre { get; set; }

    [Option("nosign", Required = false, HelpText = "Skip code signing/notarization")]
    public bool NoSign { get; set; }

    [Option("nonotarize", Required = false, HelpText = "Skip notarization only")]
    public bool NoNotarize { get; set; }
    
    // TODO: Add net target (net462/net6/net6-windows etc.)

    // TODO: Add an option to change the package name for building test packages
}