using Eto.Forms;

namespace NAPS2.EtoForms;

public class EtoOverwritePrompt : IOverwritePrompt
{
    public OverwriteResponse ConfirmOverwrite(string path)
    {
        string fileName = Path.GetFileName(path);
        var dialogResult = Invoker.Current.InvokeGet(() =>
            MessageDialog.Show(new MessageDialogOptions
                {
                    Title = UiStrings.OverwriteFile,
                    Text = string.Format(UiStrings.ConfirmOverwriteFile, fileName),
                    Type = MessageBoxType.Warning,
                    PrimaryText = UiStrings.Overwrite,
                    SecondaryText = UiStrings.Skip
                }));
        return dialogResult switch
        {
            MessageDialogResult.Primary => OverwriteResponse.Yes,
            MessageDialogResult.Secondary => OverwriteResponse.No,
            _ => OverwriteResponse.Abort
        };
    }
}