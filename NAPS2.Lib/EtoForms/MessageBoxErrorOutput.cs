using Eto.Forms;
using NAPS2.EtoForms.Ui;
using NAPS2.Scan.Exceptions;

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
        Invoker.Current.Invoke(() =>
            MessageBox.Show(errorMessage, MiscResources.Error, MessageBoxButtons.OK, MessageBoxType.Error));
    }

    public override void DisplayError(string errorMessage, string details, string? link = null)
    {
        Invoker.Current.Invoke(() => ShowErrorWithDetails(errorMessage, details, link));
    }

    public override void DisplayError(string errorMessage, Exception exception, string? link = null)
    {
        // If the error is wrapped in ScanDriverUnknownException, we only need to display the inner exception
        var displayException =
            exception is ScanDriverUnknownException { InnerException: { } inner }
                ? inner
                : exception;
        // Note we don't want to use the ToStringDemystified() helper
        // https://github.com/benaadams/Ben.Demystifier/issues/85
        Invoker.Current.Invoke(() => ShowErrorWithDetails(errorMessage, displayException.Demystify().ToString(), link));
    }

    private void ShowErrorWithDetails(string errorMessage, string details, string? link = null)
    {
        var form = _formFactory.Create<ErrorForm>();
        form.ErrorMessage = errorMessage;
        form.Link = link;
        form.Details = details;
        form.ShowModal();
    }
}