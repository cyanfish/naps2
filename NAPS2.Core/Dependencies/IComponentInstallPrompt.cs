using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Dependencies
{
    public interface IComponentInstallPrompt
    {
        bool PromptToInstall(ExternalComponent component, string promptText);
    }
}
