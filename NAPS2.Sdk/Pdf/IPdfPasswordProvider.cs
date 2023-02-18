namespace NAPS2.Pdf;

public interface IPdfPasswordProvider
{
    bool ProvidePassword(string fileName, int attemptCount, out string password);
}