namespace NAPS2.ImportExport.Email.Mapi;

public class MapiEmailProvider : IEmailProvider
{
    private readonly MapiDispatcher _mapiDispatcher;
    private readonly Naps2Config _config;
    private readonly ErrorOutput _errorOutput;

    public MapiEmailProvider(MapiDispatcher mapiDispatcher, Naps2Config config, ErrorOutput errorOutput)
    {
        _mapiDispatcher = mapiDispatcher;
        _config = config;
        _errorOutput = errorOutput;
    }

    /// <summary>
    /// Sends an email described by the given message object.
    /// </summary>
    /// <param name="message">The object describing the email message.</param>
    /// <param name="progress"></param>
    /// <returns>Returns true if the message was sent, false if the user aborted.</returns>
    public Task<bool> SendEmail(EmailMessage message, ProgressHandler progress = default)
    {
#if NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindowsVersionAtLeast(7)) throw new InvalidOperationException("Windows-only");
#endif
        return Task.Run(async () =>
        {
            var clientName = _config.Get(c => c.EmailSetup.SystemProviderName);
            MapiSendMailReturnCode returnCode = await _mapiDispatcher.SendEmail(clientName, message);

            // Process the result
            if (returnCode == MapiSendMailReturnCode.UserAbort)
            {
                return false;
            }

            if (returnCode != MapiSendMailReturnCode.Success)
            {
                Log.Error($"Error sending email. MAPI error code: {returnCode}");
                _errorOutput.DisplayError(MiscResources.EmailError, $"MAPI returned error code: {returnCode}");
                return false;
            }

            return true;
        });
    }
}