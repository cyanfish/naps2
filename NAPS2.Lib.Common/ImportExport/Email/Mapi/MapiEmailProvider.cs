using System.Threading;
using NAPS2.Remoting.Worker;

namespace NAPS2.ImportExport.Email.Mapi;

public class MapiEmailProvider : IEmailProvider
{
    private readonly IWorkerFactory _workerFactory;
    private readonly MapiDispatcher _mapiDispatcher;
    private readonly IConfigProvider<EmailSetup> _emailSetupProvider;
    private readonly ErrorOutput _errorOutput;

    public MapiEmailProvider(IWorkerFactory workerFactory, MapiDispatcher mapiDispatcher, IConfigProvider<EmailSetup> emailSetupProvider, ErrorOutput errorOutput)
    {
        _workerFactory = workerFactory;
        _mapiDispatcher = mapiDispatcher;
        _emailSetupProvider = emailSetupProvider;
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
            var clientName = _emailSetupProvider.Get(c => c.SystemProviderName);
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