using System.IO;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Util;

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