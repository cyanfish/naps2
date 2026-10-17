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

    public override void DisplayError(string errorMessage, string details)
    {
        Invoker.Current.Invoke(() => CreateErrorForm(errorMessage, details).ShowModal());
    }

    public override void DisplayError(string errorMessage, Exception exception)
    {
        Invoker.Current.Invoke(() => CreateErrorForm(errorMessage, GetDetails(exception)).ShowModal());
    }

    public override Task<bool> DisplayErrorWithRetry(string errorMessage, Exception exception)
    {
        var form = Invoker.Current.InvokeGet(() => CreateErrorForm(errorMessage, GetDetails(exception), true));
        // Don't block the caller while the form is open; it can await the result instead
        Invoker.Current.InvokeDispatch(form.ShowModal);
        return form.RetryTask;
    }

    private static string GetDetails(Exception exception)
    {
        // If the error is wrapped in ScanDriverUnknownException, we only need to display the inner exception
        var displayException =
            exception is ScanDriverUnknownException { InnerException: { } inner }
                ? inner
                : exception;
        // Note we don't want to use the ToStringDemystified() helper
        // https://github.com/benaadams/Ben.Demystifier/issues/85
        return displayException.Demystify().ToString();
    }

    private ErrorForm CreateErrorForm(string errorMessage, string details, bool showRetry = false)
    {
        var form = _formFactory.Create<ErrorForm>();
        form.ErrorMessage = errorMessage;
        form.Details = details;
        form.ShowRetry = showRetry;
        return form;
    }
}