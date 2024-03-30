using Eto.Forms;

namespace NAPS2.EtoForms;

public class EtoOverwritePrompt : IOverwritePrompt
{
    public OverwriteResponse ConfirmOverwrite(string path)
    {
        string fileName = Path.GetFileName(path);
        var dialogResult = Invoker.Current.InvokeGet(() =>
            MessageBox.Show(string.Format(MiscResources.ConfirmOverwriteFile, fileName),
                MiscResources.OverwriteFile, MessageBoxButtons.YesNoCancel, MessageBoxType.Warning,
                MessageBoxDefaultButton.Yes));
        return dialogResult switch
        {
            DialogResult.Yes => OverwriteResponse.Yes,
            DialogResult.No => OverwriteResponse.No,
            _ => OverwriteResponse.Abort
        };
    }
}