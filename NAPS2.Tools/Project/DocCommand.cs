namespace NAPS2.Tools.Project;

public class DocCommand : ICommand<DocOptions>
{
    public int Run(DocOptions options)
    {
        if (options.DocCommand == "serve")
        {
            Cli.Run("docfx", "NAPS2.Sdk/_doc/docfx.json --serve", alwaysVerbose: true);
        }
        return 0;
    }
}