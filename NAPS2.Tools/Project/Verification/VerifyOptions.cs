using CommandLine;

namespace NAPS2.Tools.Project.Verification;

[Verb("verify", HelpText = "Verify the packaged app, 'verify {all|exe|msi|zip}'")]
public class VerifyOptions : OptionsBase
{
    [Value(0, MetaName = "build type", Required = true, HelpText = "all|exe|msi|zip")]
    public string? BuildType { get; set; }
    
    [Option('p', "platform", Required = false, HelpText = "win|win32|win64|mac|macintel|macarm|linux")]
    public string? Platform { get; set; }

    [Option("nocleanup", Required = false, HelpText = "Skip cleaning up temp files")]
    public bool NoCleanup { get; set; }
}