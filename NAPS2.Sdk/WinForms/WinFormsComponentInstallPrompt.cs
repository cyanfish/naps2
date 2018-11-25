using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Dependencies;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class WinFormsComponentInstallPrompt : IComponentInstallPrompt
    {
        private readonly IFormFactory formFactory;

        public WinFormsComponentInstallPrompt(IFormFactory formFactory)
        {
            this.formFactory = formFactory;
        }

        public bool PromptToInstall(ExternalComponent component, string promptText)
        {
            if (MessageBox.Show(promptText, MiscResources.DownloadNeeded, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var progressForm = formFactory.Create<FDownloadProgress>();
                progressForm.QueueFile(component);
                progressForm.ShowDialog();
            }
            return component.IsInstalled;
        }
    }
}
