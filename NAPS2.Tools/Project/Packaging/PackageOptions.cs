using CommandLine;

namespace NAPS2.Tools.Project.Packaging;

[Verb("pkg", HelpText = "Package the project, 'pkg {all|exe|msi|zip}'")]
public class PackageOptions
{
    [Value(0, MetaName = "build type", Required = true, HelpText = "all|exe|msi|zip")]
    public string? BuildType { get; set; }
    
    // TODO: Allow platform combos (e.g. win32+win64)
    [Option('p', "platform", Required = false, HelpText = "win32|win64|mac|macarm|linux")]
    public string? Platform { get; set; }

    [Option('v', "verbose", Required = false, HelpText = "Show full output")]
    public bool Verbose { get; set; }
    
    // TODO: Add net target (net462/net6/net6-windows etc.)
}