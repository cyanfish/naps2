using System.Windows.Forms;
using NAPS2.Lang.ConsoleResources;

namespace NAPS2.Automation;

public class ConsoleOverwritePrompt : IOverwritePrompt
{
    public static bool ForceOverwrite { get; set; }

    private readonly ErrorOutput _errorOutput;

    public ConsoleOverwritePrompt(ErrorOutput errorOutput)
    {
        _errorOutput = errorOutput;
    }

    public OverwriteResponse ConfirmOverwrite(string path)
    {
        if (ForceOverwrite)
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