namespace NAPS2.Pdf;

/// <summary>
/// Provides a callback to prompt the user for a password to import an encrypted PDF. Alternatively, if you already have
/// the password, you can just specify it in ImportParams.
/// </summary>
public interface IPdfPasswordProvider
{
    bool ProvidePassword(string fileName, int attemptCount, out string password);
}