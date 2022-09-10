using Eto.Forms;

namespace NAPS2.WinForms;

public class WinFormsOverwritePrompt : IOverwritePrompt
{
    public OverwriteResponse ConfirmOverwrite(string path)
    {
        string fileName = Path.GetFileName(path);
        var dialogResult = MessageBox.Show(string.Format(MiscResources.ConfirmOverwriteFile, fileName),
            MiscResources.OverwriteFile, MessageBoxButtons.YesNoCancel, MessageBoxType.Warning);
        return dialogResult switch
        {
            DialogResult.Yes => OverwriteResponse.Yes,
            DialogResult.No => OverwriteResponse.No,
            _ => OverwriteResponse.Abort
        };
    }
}