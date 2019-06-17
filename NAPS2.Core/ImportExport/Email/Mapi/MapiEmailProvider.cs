using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Platform;
using NAPS2.Util;
using NAPS2.Worker;

namespace NAPS2.ImportExport.Email.Mapi
{
    public class MapiEmailProvider : IEmailProvider
    {
        private readonly IWorkerServiceFactory workerServiceFactory;
        private readonly MapiWrapper mapiWrapper;
        private readonly IErrorOutput errorOutput;

        public MapiEmailProvider(IWorkerServiceFactory workerServiceFactory, MapiWrapper mapiWrapper, IErrorOutput errorOutput)
        {
            this.workerServiceFactory = workerServiceFactory;
            this.mapiWrapper = mapiWrapper;
            this.errorOutput = errorOutput;
        }

        private bool UseWorker => Environment.Is64BitProcess && PlatformCompat.Runtime.UseWorker;

        /// <summary>
        /// Sends an email described by the given message object.
        /// </summary>
        /// <param name="message">The object describing the email message.</param>
        /// <param name="progressCallback"></param>
        /// <param name="cancelToken"></param>
        /// <returns>Returns true if the message was sent, false if the user aborted.</returns>
        public async Task<bool> SendEmail(EmailMessage message, ProgressHandler progressCallback, CancellationToken cancelToken)
        {
            return await Task.Factory.StartNew(() =>
            {
                MapiSendMailReturnCode returnCode;

                if (UseWorker && !mapiWrapper.CanLoadClient)
                {
                    using (var worker = workerServiceFactory.Create())
                    {
                        returnCode = worker.Service.SendMapiEmail(message);
                    }
                }
                else
                {
                    returnCode = mapiWrapper.SendEmail(message);
                    // It's difficult to get 32/64 bit right when using mapi32.dll. This can help sometimes.
                    // https://docs.microsoft.com/en-us/office/client-developer/outlook/mapi/building-mapi-applications-on-32-bit-and-64-bit-platforms
                    if (UseWorker && returnCode == MapiSendMailReturnCode.Failure)
                    {
                        using (var worker = workerServiceFactory.Create())
                        {
                            returnCode = worker.Service.SendMapiEmail(message);
                        }
                    }
                }

                // Process the result
                if (returnCode == MapiSendMailReturnCode.UserAbort)
                {
                    return false;
                }

                if (returnCode != MapiSendMailReturnCode.Success)
                {
                    Log.Error("Error sending email. MAPI error code: {0}", returnCode);
                    errorOutput.DisplayError(MiscResources.EmailError, $"MAPI returned error code: {returnCode}");
                    return false;
                }

                return true;
            });
        }
    }
}
