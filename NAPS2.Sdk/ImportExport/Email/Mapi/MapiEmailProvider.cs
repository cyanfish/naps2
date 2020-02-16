using System;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Platform;
using NAPS2.Remoting.Worker;
using NAPS2.Util;

namespace NAPS2.ImportExport.Email.Mapi
{
    public class MapiEmailProvider : IEmailProvider
    {
        private readonly IWorkerFactory _workerFactory;
        private readonly IMapiWrapper _mapiWrapper;
        private readonly ErrorOutput _errorOutput;

        public MapiEmailProvider(IWorkerFactory workerFactory, IMapiWrapper mapiWrapper, ErrorOutput errorOutput)
        {
            _workerFactory = workerFactory;
            _mapiWrapper = mapiWrapper;
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
                MapiSendMailReturnCode returnCode;

                if (UseWorker && !_mapiWrapper.CanLoadClient)
                {
                    using var worker = _workerFactory.Create();
                    returnCode = await worker.Service.SendMapiEmail(message);
                }
                else
                {
                    returnCode = await _mapiWrapper.SendEmail(message);
                }

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
}
