using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FDownloadProgress : FormBase
    {
        public FDownloadProgress()
        {
            InitializeComponent();

            client.DownloadFileCompleted += client_DownloadFileCompleted;
            client.DownloadProgressChanged += client_DownloadProgressChanged;
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
                Log.ErrorException("Error downloading file: " + file.Filename, e.Error);
            }
            else if (file.Sha1 != CalculateSha1((Path.Combine(file.TempFolder, file.Filename))))
            {
                hasError = true;
                Log.Error("Error downloading file (invalid checksum): " + file.Filename);
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
                Close();
                MessageBox.Show(MiscResources.FilesCouldNotBeDownloaded, MiscResources.DownloadError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (filesDownloaded > 0)
            {
                var prev = filesToDownload[filesDownloaded - 1];
                prev.FileCallback(Path.Combine(prev.TempFolder, prev.Filename));
                Directory.Delete(prev.TempFolder, true);
            }
            if (filesDownloaded >= filesToDownload.Count)
            {
                Close();
                return;
            }
            var next = filesToDownload[filesDownloaded];
            next.TempFolder = Path.Combine(Paths.Temp, Path.GetRandomFileName());
            Directory.CreateDirectory(next.TempFolder);
            client.DownloadFileAsync(new Uri(string.Format(next.Root, next.Filename)), Path.Combine(next.TempFolder, next.Filename));
        }

        public void QueueFile(string root, string filename, string sha1, Action<string> fileCallback)
        {
            filesToDownload.Add(new QueueItem { Root = root, Filename = filename, Sha1 = sha1, FileCallback = fileCallback });
        }

        private readonly List<QueueItem> filesToDownload = new List<QueueItem>();
        private int filesDownloaded = 0;
        private double currentFileSize = 0.0;
        private double currentFileProgress = 0.0;
        private bool hasError;

        private readonly WebClient client = new WebClient();

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

        private void DisplayProgress()
        {
            labelTop.Text = string.Format(MiscResources.FilesProgress, filesDownloaded, filesToDownload.Count);
            progressBarTop.Maximum = filesToDownload.Count;
            progressBarTop.Value = filesDownloaded;
            labelSub.Text = string.Format(MiscResources.SizeProgress, (currentFileProgress / 1000000.0).ToString("f1"), (currentFileSize / 1000000.0).ToString("f1"));
            progressBarSub.Maximum = (int)(currentFileSize);
            progressBarSub.Value = (int)(currentFileProgress);
            Refresh();
        }

        private class QueueItem
        {
            public string Root { get; set; }

            public string Filename { get; set; }

            public string TempFolder { get; set; }

            public string Sha1 { get; set; }

            public Action<string> FileCallback { get; set; }
        }
    }
}
