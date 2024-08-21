using CommandLine;

namespace NAPS2.Tools.Project.Packaging;

[Verb("pkg", HelpText = "Package the project, 'pkg {all|exe|msi|zip|flatpak|pkg|deb|rpm}'")]
public class PackageOptions : OptionsBase
{
    [Value(0, MetaName = "package type", Required = false, HelpText = "all|exe|msi|zip|flatpak|pkg|deb|rpm")]
    public string? PackageType { get; set; }

    [Option('p', "platform", Required = false, HelpText = "win|win32|win64|mac|macintel|macarm|linux")]
    public string? Platform { get; set; }

    [Option("name", Required = false, HelpText = "Name to be appended to the package filename")]
    public string? Name { get; set; }

    [Option("nopre", Required = false, HelpText = "Skip pre-packaging steps")]
    public bool NoPre { get; set; }

    [Option("nosign", Required = false, HelpText = "Skip code signing/notarization")]
    public bool NoSign { get; set; }

    [Option("nonotarize", Required = false, HelpText = "Skip notarization only")]
    public bool NoNotarize { get; set; }

    [Option("xcompile", Required = false, HelpText = "Cross-compile packages where possible (e.g. build linux-arm64 on linux-x64)")]
    public bool XCompile { get; set; }
}