using NAPS2.Lang.Resources;
using NAPS2.Util;
using System.IO;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class WinFormsOverwritePrompt : IOverwritePrompt
    {
        public DialogResult ConfirmOverwrite(string path)
        {
            string fileName = Path.GetFileName(path);
            var dialogResult = MessageBox.Show(string.Format(MiscResources.ConfirmOverwriteFile, fileName),
                MiscResources.OverwriteFile, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            return dialogResult;
        }
    }
}