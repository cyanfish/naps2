namespace NAPS2.Tools.Localization;

public class TemplatesCommand : ICommand<TemplatesOptions>
{
    public int Run(TemplatesOptions opts)
    {
        var ctx = new TemplatesContext();
        ctx.Load(Path.Combine(Paths.SolutionRoot, "NAPS2.Sdk", "Lang", "Resources"), false);
        ctx.Load(Path.Combine(Paths.SolutionRoot, "NAPS2.Lib", "Lang", "Resources"), false);
        ctx.Save(Paths.TemplatesFile);
        return 0;
    }
}