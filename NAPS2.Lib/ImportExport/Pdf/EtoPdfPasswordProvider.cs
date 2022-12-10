using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;

namespace NAPS2.ImportExport.Pdf;

public class EtoPdfPasswordProvider : IPdfPasswordProvider
{
    private readonly IFormFactory _formFactory;

    public EtoPdfPasswordProvider(IFormFactory formFactory)
    {
        _formFactory = formFactory;
    }

    public bool ProvidePassword(string fileName, int attemptCount, out string password)
    {
        (password, var result) = Invoker.Current.InvokeGet(() =>
        {
            var passwordForm = _formFactory.Create<PdfPasswordForm>();
            passwordForm.FileName = fileName;
            passwordForm.ShowModal();
            return (passwordForm.Password!, passwordForm.Result);
        });
        return result;
    }
}