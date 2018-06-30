﻿using NAPS2.Dependencies;
using NAPS2.Lang.ConsoleResources;
using System;

namespace NAPS2.Automation
{
    public class ConsoleComponentInstallPrompt : IComponentInstallPrompt
    {
        public bool PromptToInstall(DownloadInfo download, ExternalComponent component, string promptText)
        {
            Console.WriteLine(ConsoleResources.ComponentNeeded, component.Id);
            return false;
        }
    }
}