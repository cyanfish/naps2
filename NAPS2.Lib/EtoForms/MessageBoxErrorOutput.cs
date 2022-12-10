using Eto.Forms;
using NAPS2.EtoForms.Ui;

namespace NAPS2.EtoForms;

public class MessageBoxErrorOutput : ErrorOutput
{
    private readonly IFormFactory _formFactory;

    public MessageBoxErrorOutput(IFormFactory formFactory)
    {
        _formFactory = formFactory;
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
        var form = _formFactory.Create<ErrorForm>();
        form.ErrorMessage = errorMessage;
        form.Details = details;
        form.ShowModal();
    }
}