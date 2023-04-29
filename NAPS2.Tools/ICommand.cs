namespace NAPS2.Tools;

public interface ICommand<in TOptions> where TOptions : OptionsBase
{
    public int Run(TOptions options);
}