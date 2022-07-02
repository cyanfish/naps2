using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.ConsoleResources;

namespace NAPS2.Automation;

public class ConsolePdfPasswordProvider : IPdfPasswordProvider
{
    private readonly AutomatedScanningOptions _options;
    private readonly ErrorOutput _errorOutput;

    public ConsolePdfPasswordProvider(AutomatedScanningOptions options, ErrorOutput errorOutput)
    {
        _options = options;
        _errorOutput = errorOutput;
    }

    public bool ProvidePassword(string fileName, int attemptCount, out string password)
    {
        password = _options.ImportPassword ?? "";
        if (attemptCount > 0)
        {
            _errorOutput.DisplayError(_options.ImportPassword == null
                ? ConsoleResources.ImportErrorNoPassword : ConsoleResources.ImportErrorWrongPassword);
            return false;
        }
        return true;
    }
}