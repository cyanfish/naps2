using NAPS2.Lang.ConsoleResources;

namespace NAPS2.Automation;

public class ConsoleOverwritePrompt : IOverwritePrompt
{
    private readonly AutomatedScanningOptions _options;
    private readonly ErrorOutput _errorOutput;

    public ConsoleOverwritePrompt(AutomatedScanningOptions options, ErrorOutput errorOutput)
    {
        _options = options;
        _errorOutput = errorOutput;
    }

    public OverwriteResponse ConfirmOverwrite(string path)
    {
        if (_options.ForceOverwrite)
        {
            return OverwriteResponse.Yes;
        }
        else
        {
            _errorOutput.DisplayError(string.Format(ConsoleResources.FileAlreadyExists, path));
            return OverwriteResponse.No;
        }
    }
}