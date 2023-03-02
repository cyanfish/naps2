using CommandLine;

namespace NAPS2.Tools.Project.Installation;

[Verb("install", HelpText = "Install the packaged app, 'install {exe|msi|flatpak|pkg|deb|rpm}'")]
public class InstallOptions : OptionsBase
{
    [Value(0, MetaName = "package type", Required = true, HelpText = "exe|msi|flatpak|pkg|deb|rpm")]
    public string? PackageType { get; set; }

    [Option('p', "platform", Required = false, HelpText = "win|win32|win64|mac|macintel|macarm|linux")]
    public string? Platform { get; set; }
    
    [Option('r', "run", Required = false, HelpText = "Run NAPS2 after installation")]
    public bool Run { get; set; }
}