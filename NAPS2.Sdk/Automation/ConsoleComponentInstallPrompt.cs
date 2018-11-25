using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Dependencies;
using NAPS2.Lang.ConsoleResources;

namespace NAPS2.Automation
{
    public class ConsoleComponentInstallPrompt : IComponentInstallPrompt
    {
        public bool PromptToInstall(ExternalComponent component, string promptText)
        {
            Console.WriteLine(ConsoleResources.ComponentNeeded, component.Id);
            return false;
        }
    }
}
