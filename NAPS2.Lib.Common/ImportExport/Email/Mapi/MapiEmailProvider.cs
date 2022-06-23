using System.Threading;
using NAPS2.Remoting.Worker;

namespace NAPS2.ImportExport.Email.Mapi;

public class MapiEmailProvider : IEmailProvider
{
    private readonly MapiDispatcher _mapiDispatcher;
    private readonly ScopedConfig _config;
    private readonly ErrorOutput _errorOutput;

    public MapiEmailProvider(MapiDispatcher mapiDispatcher, ScopedConfig config, ErrorOutput errorOutput)
    {
        _mapiDispatcher = mapiDispatcher;
        _config = config;
        _errorOutput = errorOutput;
    }

    private bool UseWorker => Environment.Is64BitProcess && PlatformCompat.Runtime.UseWorker;

    /// <summary>
    /// Sends an email described by the given message object.
    /// </summary>
    /// <param name="message">The object describing the email message.</param>
    /// <param name="progressCallback"></param>
    /// <param name="cancelToken"></param>
    /// <returns>Returns true if the message was sent, false if the user aborted.</returns>
    public Task<bool> SendEmail(EmailMessage message, ProgressHandler progressCallback, CancellationToken cancelToken)
    {
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
                Log.Error("Error sending email. MAPI error code: {0}", returnCode);
                _errorOutput.DisplayError(MiscResources.EmailError, $"MAPI returned error code: {returnCode}");
                return false;
            }

            return true;
        });
    }
}