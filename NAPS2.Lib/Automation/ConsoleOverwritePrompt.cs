using NAPS2.Lang.ConsoleResources;

namespace NAPS2.Automation;

/// <summary>
/// Overwriting is only done with --force, in which case operations don't prompt at all, so this just reports the
/// conflict as an error.
/// </summary>
public class ConsoleOverwritePrompt : IOverwritePrompt
{
    private readonly ErrorOutput _errorOutput;

    public ConsoleOverwritePrompt(ErrorOutput errorOutput)
    {
        _errorOutput = errorOutput;
    }

    public OverwriteResponse ConfirmOverwrite(string path)
    {
        _errorOutput.DisplayError(string.Format(ConsoleResources.FileAlreadyExists, path));
        return OverwriteResponse.No;
    }
}
