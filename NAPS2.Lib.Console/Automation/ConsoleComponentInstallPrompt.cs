using NAPS2.Dependencies;
using NAPS2.Lang.ConsoleResources;

namespace NAPS2.Automation;

public class ConsoleComponentInstallPrompt : IComponentInstallPrompt
{
    private readonly ConsoleOutput _output;

    public ConsoleComponentInstallPrompt(ConsoleOutput output)
    {
        _output = output;
    }

    public bool PromptToInstall(ExternalComponent component, string promptText)
    {
        _output.Writer.WriteLine(ConsoleResources.ComponentNeeded, component.Id);
        return false;
    }
}