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
        Invoker.Current.Invoke(() => MessageBox.Show(errorMessage, MiscResources.Error, MessageBoxButtons.OK, MessageBoxType.Error));
    }

    public override void DisplayError(string errorMessage, string details)
    {
        Invoker.Current.Invoke(() => ShowErrorWithDetails(errorMessage, details));
    }

    public override void DisplayError(string errorMessage, Exception exception)
    {
        Invoker.Current.Invoke(() => ShowErrorWithDetails(errorMessage, exception.ToString()));
    }

    private void ShowErrorWithDetails(string errorMessage, string details)
    {
        var form = _formFactory.Create<ErrorForm>();
        form.ErrorMessage = errorMessage;
        form.Details = details;
        form.ShowModal();
    }
}