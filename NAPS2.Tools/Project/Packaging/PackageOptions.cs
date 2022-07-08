using CommandLine;

namespace NAPS2.Tools.Project.Packaging;

[Verb("pkg", HelpText = "Package the project, 'pkg {all|exe|msi|zip}'")]
public class PackageOptions
{
    [Value(0, MetaName = "what", Required = true, HelpText = "all|exe|msi|zip")]
    public string? What { get; set; }
    
    // TODO: Allow platform combos (e.g. win32+win64)
    [Option('p', "platform", Required = false, HelpText = "win32|win64|mac|macarm|linux")]
    public string? Platform { get; set; }
    
    // TODO: Add net target (net462/net5/net5-windows etc.)
}