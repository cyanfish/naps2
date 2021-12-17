using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.ConsoleResources;

namespace NAPS2.Automation;

public class ConsolePdfPasswordProvider : IPdfPasswordProvider
{
    private readonly ErrorOutput _errorOutput;

    public ConsolePdfPasswordProvider(ErrorOutput errorOutput)
    {
        _errorOutput = errorOutput;
    }

    public bool ProvidePassword(string fileName, int attemptCount, out string password)
    {
        password = PasswordToProvide ?? "";
        if (attemptCount > 0)
        {
            _errorOutput.DisplayError(PasswordToProvide == null
                ? ConsoleResources.ImportErrorNoPassword : ConsoleResources.ImportErrorWrongPassword);
            return false;
        }
        return true;
    }

    public static string PasswordToProvide { get; set; }
}