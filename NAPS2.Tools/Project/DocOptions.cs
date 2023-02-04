using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("doc", HelpText = "Docfx control")]
public class DocOptions : OptionsBase
{
    [Value(0, MetaName = "doc command", Required = true, HelpText = "serve|?")]
    public string? DocCommand { get; set; }
}