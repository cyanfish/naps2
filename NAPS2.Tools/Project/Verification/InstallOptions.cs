using CommandLine;

namespace NAPS2.Tools.Project.Verification;

[Verb("install", HelpText = "Install the packaged app")]
public class InstallOptions
{
    [Value(0, MetaName = "what", Required = true, HelpText = "exe|msi")]
    public string? What { get; set; }
    
    [Option('p', "platform", Required = false, HelpText = "win32|win64|mac|macarm|linux")]
    public string? Platform { get; set; }

    [Option('v', "verbose", Required = false, HelpText = "Show full output")]
    public bool Verbose { get; set; }

    [Option("noelevation", Required = false)]
    public bool NoElevation { get; set; }
}