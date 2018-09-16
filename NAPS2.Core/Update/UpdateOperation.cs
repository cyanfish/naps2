using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Operation;

namespace NAPS2.Update
{
    public class UpdateOperation : OperationBase
    {
        private WebClient client;
        private UpdateInfo update;
        private string tempFolder;
        private string tempPath;

        static UpdateOperation()
        {
            try
            {
                const int tls12 = 3072;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)tls12;
            }
            catch (NotSupportedException)
            {
            }
        }

        public UpdateOperation()
        {
            ProgressTitle = MiscResources.UpdateProgress;
            AllowBackground = true;
            AllowCancel = true;
            Status = new OperationStatus
            {
                StatusText = MiscResources.Updating,
                ProgressType = OperationProgressType.MB
            };
        }

        public void Start(UpdateInfo updateInfo)
        {
            update = updateInfo;
            tempFolder = Path.Combine(Paths.Temp, Path.GetRandomFileName());
            Directory.CreateDirectory(tempFolder);
            tempPath = Path.Combine(tempFolder, updateInfo.DownloadUrl.Substring(updateInfo.DownloadUrl.LastIndexOf('/') + 1));
            client = new WebClient();
            client.DownloadProgressChanged += DownloadProgress;
            client.DownloadFileCompleted += DownloadCompleted;
            client.DownloadFileAsync(new Uri(updateInfo.DownloadUrl), tempPath);
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // TODO: Verify sha1/sig
            // TODO: Standalone install
            Process.Start(tempPath);
            // TODO: Clean up temp file somehow
            InvokeFinished();
            Application.OpenForms.OfType<Form>().FirstOrDefault()?.Close();
        }

        private void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            Status.CurrentProgress = (int)e.BytesReceived;
            Status.MaxProgress = (int)e.TotalBytesToReceive;
            InvokeStatusChanged();
        }
    }
}
