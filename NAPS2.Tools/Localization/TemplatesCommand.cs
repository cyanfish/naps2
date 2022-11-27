using NAPS2.Tools.Project;

namespace NAPS2.Tools.Localization;

public class TemplatesCommand : ICommand<TemplatesOptions>
{
    public int Run(TemplatesOptions opts)
    {
        var ctx = new TemplatesContext();
        ctx.Load(Path.Combine(Paths.SolutionRoot, @"NAPS2.Sdk\Lang\Resources"), false);
        ctx.Load(Path.Combine(Paths.SolutionRoot, @"NAPS2.Lib\Lang\Resources"), false);
        ctx.Load(Path.Combine(Paths.SolutionRoot, @"NAPS2.Lib.WinForms\WinForms"), true);
        ctx.Save(Path.Combine(Paths.SolutionRoot, @"NAPS2.Lib\Lang\po\templates.pot"));
        return 0;
    }
}