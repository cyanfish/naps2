namespace NAPS2.Tools.Localization;

public static class TemplatesCommand
{
    public static int Run(TemplatesOptions opts)
    {
        var ctx = new TemplatesContext();
        ctx.Load(Path.Combine(Paths.SolutionRoot, @"NAPS2.Core\Lang\Resources"), false);
        ctx.Load(Path.Combine(Paths.SolutionRoot, @"NAPS2.Core\WinForms"), true);
        ctx.Save(Path.Combine(Paths.SolutionRoot, @"NAPS2.Core\Lang\po\templates.pot"));
        return 0;
    }
}