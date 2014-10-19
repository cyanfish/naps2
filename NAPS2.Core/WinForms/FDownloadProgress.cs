using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using NAPS2.Lang.Resources;

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
            filesDownloaded++;
            currentFileProgress = 0;
            currentFileSize = 0;
            DisplayProgress();
            StartNextDownload();
        }

        private void StartNextDownload()
        {
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
            client.DownloadFileAsync(new Uri(new Uri(next.Root), next.Filename), Path.Combine(next.TempFolder, next.Filename));
        }

        public void QueueFile(string root, string filename, Action<string> fileCallback)
        {
            filesToDownload.Add(new QueueItem { Root = root, Filename = filename, FileCallback = fileCallback });
        }

        private readonly List<QueueItem> filesToDownload = new List<QueueItem>();
        private int filesDownloaded = 0;
        private double currentFileSize = 0.0;
        private double currentFileProgress = 0.0;

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
            labelSub.Text = string.Format(MiscResources.SizeProgress, currentFileProgress.ToString("f1"), currentFileSize.ToString("f1"));
            progressBarSub.Maximum = (int)(currentFileSize);
            progressBarSub.Value = (int)(currentFileProgress);
            Refresh();
        }

        private class QueueItem
        {
            public string Root { get; set; }

            public string Filename { get; set; }

            public string TempFolder { get; set; }

            public Action<string> FileCallback { get; set; }
        }
    }
}
