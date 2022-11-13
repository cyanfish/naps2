using CommandLine;

namespace NAPS2.Tools.Project.Installation;

[Verb("install", HelpText = "Install the packaged app, 'install {exe|msi}'")]
public class InstallOptions : OptionsBase
{
    [Value(0, MetaName = "build type", Required = true, HelpText = "exe|msi")]
    public string? BuildType { get; set; }

    [Option('p', "platform", Required = false, HelpText = "win32|win64|mac|macarm|linux")]
    public string? Platform { get; set; }
    
    [Option('r', "run", Required = false, HelpText = "Run NAPS2 after installation")]
    public bool Run { get; set; }
}