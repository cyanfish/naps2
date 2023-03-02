using CommandLine;

namespace NAPS2.Tools.Project.Verification;

[Verb("verify", HelpText = "Verify the packaged app, 'verify {all|exe|msi|zip|flatpak|pkg|deb|rpm}'")]
public class VerifyOptions : OptionsBase
{
    [Value(0, MetaName = "package type", Required = true, HelpText = "all|exe|msi|zip|flatpak|pkg|deb|rpm")]
    public string? PackageType { get; set; }
    
    [Option('p', "platform", Required = false, HelpText = "win|win32|win64|mac|macintel|macarm|linux")]
    public string? Platform { get; set; }

    [Option("nocleanup", Required = false, HelpText = "Skip cleaning up temp files")]
    public bool NoCleanup { get; set; }
}