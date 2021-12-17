using System.Windows.Forms;
using NAPS2.Dependencies;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms;

public class WinFormsComponentInstallPrompt : IComponentInstallPrompt
{
    private readonly IFormFactory _formFactory;

    public WinFormsComponentInstallPrompt(IFormFactory formFactory)
    {
        _formFactory = formFactory;
    }

    public bool PromptToInstall(ExternalComponent component, string promptText)
    {
        if (MessageBox.Show(promptText, MiscResources.DownloadNeeded, MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            var progressForm = _formFactory.Create<FDownloadProgress>();
            progressForm.QueueFile(component);
            progressForm.ShowDialog();
        }
        return component.IsInstalled;
    }
}