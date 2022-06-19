using System.Windows.Forms;

namespace NAPS2.WinForms;

public class WinFormsOverwritePrompt : OverwritePrompt
{
    public override DialogResult ConfirmOverwrite(string path)
    {
        string fileName = Path.GetFileName(path);
        var dialogResult = MessageBox.Show(string.Format(MiscResources.ConfirmOverwriteFile, fileName),
            MiscResources.OverwriteFile, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
        return dialogResult;
    }
}