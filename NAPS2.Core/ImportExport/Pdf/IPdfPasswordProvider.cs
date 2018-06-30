namespace NAPS2.ImportExport.Pdf
{
    public interface IPdfPasswordProvider
    {
        bool ProvidePassword(string fileName, int attemptCount, out string password);
    }
}