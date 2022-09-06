using Eto.Forms;

namespace NAPS2.WinForms;

public class MessageBoxErrorOutput : ErrorOutput
{
    private readonly DialogHelper _dialogHelper;

    public MessageBoxErrorOutput(DialogHelper dialogHelper)
    {
        _dialogHelper = dialogHelper;
    }

    public override void DisplayError(string errorMessage)
    {
        Invoker.Current.SafeInvoke(() => MessageBox.Show(errorMessage, MiscResources.Error, MessageBoxButtons.OK, MessageBoxType.Error));
    }

    public override void DisplayError(string errorMessage, string details)
    {
        Invoker.Current.SafeInvoke(() => ShowErrorWithDetails(errorMessage, details));
    }

    public override void DisplayError(string errorMessage, Exception exception)
    {
        Invoker.Current.SafeInvoke(() => ShowErrorWithDetails(errorMessage, exception.ToString()));
    }
    private void ShowErrorWithDetails(string errorMessage, string details)
    {
        // TODO: Migrate error form
        MessageBox.Show(errorMessage, MessageBoxType.Error);
        // var form = new FError
        // {
        //     ErrorMessage = errorMessage,
        //     Details = details
        // };
        // form.ShowDialog();
    }
}