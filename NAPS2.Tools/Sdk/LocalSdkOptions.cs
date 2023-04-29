using CommandLine;

namespace NAPS2.Tools.Sdk;

[Verb("localsdk", HelpText = "Builds SDK nuget packages and add to local nuget source'")]
public class LocalSdkOptions : OptionsBase
{
    [Value(0, MetaName = "local source", Required = true, HelpText = "Path to local nuget package source")]
    public string? LocalSource { get; set; }

    [Option("nobuild", Required = false, HelpText = "Skip build")]
    public bool NoBuild { get; set; }

    // TODO: Add a --purge option to delete userfolder/.nuget/{project} folders
    // TODO: Add a --restore {csproj} option to run dotnet restore on the given csproj
}