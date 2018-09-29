using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;
using NAPS2.Dependencies;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FDownloadProgress : FormBase
    {
        private readonly List<QueueItem> filesToDownload = new List<QueueItem>();
        private int filesDownloaded = 0;
        private int urlIndex = 0;
        private double currentFileSize = 0.0;
        private double currentFileProgress = 0.0;
        private bool hasError;
        private bool cancel;

        private readonly WebClient client = new WebClient();

        static FDownloadProgress()
        {
            try
            {
                const int tls12 = 3072;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType) tls12;
            }
            catch (NotSupportedException)
            {
            }
        }

        public FDownloadProgress()
        {
            InitializeComponent();

            client.DownloadFileCompleted += client_DownloadFileCompleted;
            client.DownloadProgressChanged += client_DownloadProgressChanged;
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(progressBarTop, progressBarSub)
                    .WidthToForm()
                .Bind(btnCancel)
                    .RightToForm()
                .Activate();

            DisplayProgress();

            StartNextDownload();
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            currentFileProgress = e.BytesReceived;
            currentFileSize = e.TotalBytesToReceive;
            DisplayProgress();
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var file = filesToDownload[filesDownloaded];
            if (e.Error != null)
            {
                hasError = true;
                if (!cancel)
                {
                    Log.ErrorException("Error downloading file: " + file.DownloadInfo.FileName, e.Error);
                }
            }
            else if (file.DownloadInfo.Sha1 != CalculateSha1((Path.Combine(file.TempFolder, file.DownloadInfo.FileName))))
            {
                hasError = true;
                Log.Error("Error downloading file (invalid checksum): " + file.DownloadInfo.FileName);
            }
            else
            {
                filesDownloaded++;
            }
            currentFileProgress = 0;
            currentFileSize = 0;
            DisplayProgress();
            StartNextDownload();
        }

        private string CalculateSha1(string filePath)
        {
            using (var sha = new SHA1Managed())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] checksum = sha.ComputeHash(stream);
                    string str = BitConverter.ToString(checksum).Replace("-", String.Empty).ToLowerInvariant();
                    return str;
                }
            }
        }

        private void StartNextDownload()
        {
            if (hasError)
            {
                var prev = filesToDownload[filesDownloaded];
                Directory.Delete(prev.TempFolder, true);
                if (cancel)
                {
                    return;
                }
                // Retry if possible
                urlIndex++;
                hasError = false;
            }
            else
            {
                urlIndex = 0;
            }
            if (filesDownloaded > 0 && urlIndex == 0)
            {
                var prev = filesToDownload[filesDownloaded - 1];
                var filePath = Path.Combine(prev.TempFolder, prev.DownloadInfo.FileName);
                try
                {
                    var preparedFilePath = prev.DownloadInfo.Format.Prepare(filePath);
                    prev.FileCallback(preparedFilePath);
                }
                catch (Exception ex)
                {
                    Log.ErrorException("Error preparing downloaded file", ex);
                    MessageBox.Show(MiscResources.FilesCouldNotBeDownloaded, MiscResources.DownloadError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Directory.Delete(prev.TempFolder, true);
            }
            if (filesDownloaded >= filesToDownload.Count)
            {
                Close();
                return;
            }
            if (urlIndex >= filesToDownload[filesDownloaded].DownloadInfo.Urls.Count)
            {
                Close();
                MessageBox.Show(MiscResources.FilesCouldNotBeDownloaded, MiscResources.DownloadError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var next = filesToDownload[filesDownloaded];
            next.TempFolder = Path.Combine(Paths.Temp, Path.GetRandomFileName());
            Directory.CreateDirectory(next.TempFolder);
            client.DownloadFileAsync(new Uri(next.DownloadInfo.Urls[urlIndex]), Path.Combine(next.TempFolder, next.DownloadInfo.FileName));
        }

        public void QueueFile(DownloadInfo downloadInfo, Action<string> fileCallback)
        {
            filesToDownload.Add(new QueueItem { DownloadInfo = downloadInfo, FileCallback = fileCallback });
        }

        public void QueueFile(IExternalComponent component)
        {
            filesToDownload.Add(new QueueItem { DownloadInfo = component.DownloadInfo, FileCallback = component.Install });
        }

        private void DisplayProgress()
        {
            labelTop.Text = string.Format(MiscResources.FilesProgressFormat, filesDownloaded, filesToDownload.Count);
            progressBarTop.Maximum = filesToDownload.Count * 1000;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            progressBarTop.Value = filesDownloaded * 1000 + (currentFileSize == 0 ? 0 : (int)(currentFileProgress / currentFileSize * 1000));
            labelSub.Text = string.Format(MiscResources.SizeProgress, (currentFileProgress / 1000000.0).ToString("f1"), (currentFileSize / 1000000.0).ToString("f1"));
            progressBarSub.Maximum = (int)(currentFileSize);
            progressBarSub.Value = (int)(currentFileProgress);
            Refresh();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FDownloadProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            cancel = true;
            client.CancelAsync();
        }

        private class QueueItem
        {
            public DownloadInfo DownloadInfo { get; set; }

            public string TempFolder { get; set; }

            public Action<string> FileCallback { get; set; }
        }
    }
}
