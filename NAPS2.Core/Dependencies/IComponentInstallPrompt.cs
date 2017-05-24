using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Dependencies
{
    public interface IComponentInstallPrompt
    {
        bool PromptToInstall(DownloadInfo download, ExternalComponent component, string promptText);
    }
}
