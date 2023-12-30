namespace NAPS2.Tools.Project;

public class DocCommand : ICommand<DocOptions>
{
    public int Run(DocOptions options)
    {
        File.Copy(
            Path.Combine(Paths.SolutionRoot, "NAPS2.Sdk", "README.md"),
            Path.Combine(Paths.SolutionRoot, "NAPS2.Sdk", "_doc", "index.md"),
            true);
        if (options.DocCommand == "build")
        {
            Cli.Run("docfx", "NAPS2.Sdk/_doc/docfx.json", alwaysVerbose: true);
        }
        if (options.DocCommand == "serve")
        {
            Cli.Run("docfx", "NAPS2.Sdk/_doc/docfx.json --serve", alwaysVerbose: true);
        }
        return 0;
    }
}