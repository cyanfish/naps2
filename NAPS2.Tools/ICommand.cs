namespace NAPS2.Tools.Project;

public interface ICommand<in TOptions> where TOptions : OptionsBase
{
    public int Run(TOptions options);
}