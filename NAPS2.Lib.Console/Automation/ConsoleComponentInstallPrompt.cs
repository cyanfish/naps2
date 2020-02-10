using System;
using NAPS2.Dependencies;
using NAPS2.Lang.ConsoleResources;

namespace NAPS2.Automation
{
    public class ConsoleComponentInstallPrompt : IComponentInstallPrompt
    {
        private readonly ConsoleOutput output;

        public ConsoleComponentInstallPrompt(ConsoleOutput output)
        {
            this.output = output;
        }

        public bool PromptToInstall(ExternalComponent component, string promptText)
        {
            output.Writer.WriteLine(ConsoleResources.ComponentNeeded, component.Id);
            return false;
        }
    }
}
