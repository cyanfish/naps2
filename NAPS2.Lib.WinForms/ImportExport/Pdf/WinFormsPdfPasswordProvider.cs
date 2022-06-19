using System.Windows.Forms;
using NAPS2.WinForms;

namespace NAPS2.ImportExport.Pdf;

public class WinFormsPdfPasswordProvider : IPdfPasswordProvider
{
    private readonly IFormFactory _formFactory;

    public WinFormsPdfPasswordProvider(IFormFactory formFactory)
    {
        _formFactory = formFactory;
    }

    public bool ProvidePassword(string fileName, int attemptCount, out string password)
    {
        var passwordForm = _formFactory.Create<FPdfPassword>();
        passwordForm.FileName = fileName;
        var dialogResult = passwordForm.ShowDialog();
        password = passwordForm.Password;
        return dialogResult == DialogResult.OK;
    }
}