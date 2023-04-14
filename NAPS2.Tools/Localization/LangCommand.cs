using NAPS2.Tools.Project;

namespace NAPS2.Tools.Localization;

public class LangCommand : ICommand<LangOptions>
{
    public int Run(LangOptions opts)
    {
        new TemplatesCommand().Run(new TemplatesOptions());
        new PushTemplatesCommand().Run(new PushTemplatesOptions());
        new PullTranslationsCommand().Run(new PullTranslationsOptions());
        new ResxCommand().Run(new ResxOptions());
        return 0;
    }
}