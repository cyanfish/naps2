using Eto.Forms;

namespace NAPS2.EtoForms;

public class EtoOverwritePrompt : IOverwritePrompt
{
    public OverwriteResponse ConfirmOverwrite(string path, bool multipleFiles)
    {
        string fileName = Path.GetFileName(path);
        var dialogResult = Invoker.Current.InvokeGet(() =>
            MessageDialog.Show(new MessageDialogOptions
                {
                    Title = UiStrings.OverwriteFile,
                    Text = string.Format(UiStrings.ConfirmOverwriteFile, fileName),
                    Type = MessageBoxType.Warning,
                    PrimaryText = UiStrings.Overwrite,
                    SecondaryText = multipleFiles ? UiStrings.Skip : null
                }));
        return dialogResult switch
        {
            MessageDialogResult.Primary => OverwriteResponse.Overwrite,
            MessageDialogResult.Secondary => OverwriteResponse.Skip,
            _ => OverwriteResponse.Abort
        };
    }
}