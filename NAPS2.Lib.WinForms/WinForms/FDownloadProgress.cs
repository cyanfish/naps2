using System.ComponentModel;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;
using NAPS2.Dependencies;

namespace NAPS2.WinForms;

public partial class FDownloadProgress : FormBase
{
    private readonly List<QueueItem> _filesToDownload = new List<QueueItem>();
    private int _filesDownloaded = 0;
    private int _urlIndex = 0;
    private double _currentFileSize = 0.0;
    private double _currentFileProgress = 0.0;
    private bool _hasError;
    private bool _cancel;

    private readonly WebClient _client = new WebClient();

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

        _client.DownloadFileCompleted += client_DownloadFileCompleted;
        _client.DownloadProgressChanged += client_DownloadProgressChanged;
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
        _currentFileProgress = e.BytesReceived;
        _currentFileSize = e.TotalBytesToReceive;
        DisplayProgress();
    }

    void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        var file = _filesToDownload[_filesDownloaded];
        if (e.Error != null)
        {
            _hasError = true;
            if (!_cancel)
            {
                Log.ErrorException("Error downloading file: " + file.DownloadInfo.FileName, e.Error);
            }
        }
        else if (file.DownloadInfo.Sha1 != CalculateSha1((Path.Combine(file.TempFolder, file.DownloadInfo.FileName))))
        {
            _hasError = true;
            Log.Error("Error downloading file (invalid checksum): " + file.DownloadInfo.FileName);
        }
        else
        {
            _filesDownloaded++;
        }
        _currentFileProgress = 0;
        _currentFileSize = 0;
        DisplayProgress();
        StartNextDownload();
    }

    private string CalculateSha1(string filePath)
    {
        using var sha = new SHA1CryptoServiceProvider();
        using FileStream stream = File.OpenRead(filePath);
        byte[] checksum = sha.ComputeHash(stream);
        string str = BitConverter.ToString(checksum).Replace("-", String.Empty).ToLowerInvariant();
        return str;
    }

    private void StartNextDownload()
    {
        if (_hasError)
        {
            var prev = _filesToDownload[_filesDownloaded];
            Directory.Delete(prev.TempFolder, true);
            if (_cancel)
            {
                return;
            }
            // Retry if possible
            _urlIndex++;
            _hasError = false;
        }
        else
        {
            _urlIndex = 0;
        }
        if (_filesDownloaded > 0 && _urlIndex == 0)
        {
            var prev = _filesToDownload[_filesDownloaded - 1];
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
        if (_filesDownloaded >= _filesToDownload.Count)
        {
            Close();
            return;
        }
        if (_urlIndex >= _filesToDownload[_filesDownloaded].DownloadInfo.Urls.Count)
        {
            Close();
            MessageBox.Show(MiscResources.FilesCouldNotBeDownloaded, MiscResources.DownloadError, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        var next = _filesToDownload[_filesDownloaded];
        next.TempFolder = Path.Combine(Paths.Temp, Path.GetRandomFileName());
        Directory.CreateDirectory(next.TempFolder);
        _client.DownloadFileAsync(new Uri(next.DownloadInfo.Urls[_urlIndex]), Path.Combine(next.TempFolder, next.DownloadInfo.FileName));
    }

    public void QueueFile(DownloadInfo downloadInfo, Action<string> fileCallback)
    {
        _filesToDownload.Add(new QueueItem { DownloadInfo = downloadInfo, FileCallback = fileCallback });
    }

    public void QueueFile(IExternalComponent component)
    {
        _filesToDownload.Add(new QueueItem { DownloadInfo = component.DownloadInfo, FileCallback = component.Install });
    }

    private void DisplayProgress()
    {
        labelTop.Text = string.Format(MiscResources.FilesProgressFormat, _filesDownloaded, _filesToDownload.Count);
        progressBarTop.Maximum = _filesToDownload.Count * 1000;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        progressBarTop.Value = _filesDownloaded * 1000 + (_currentFileSize == 0 ? 0 : (int)(_currentFileProgress / _currentFileSize * 1000));
        labelSub.Text = string.Format(MiscResources.SizeProgress, (_currentFileProgress / 1000000.0).ToString("f1"), (_currentFileSize / 1000000.0).ToString("f1"));
        progressBarSub.Maximum = (int)(_currentFileSize);
        progressBarSub.Value = (int)(_currentFileProgress);
        Refresh();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void FDownloadProgress_FormClosing(object sender, FormClosingEventArgs e)
    {
        _cancel = true;
        _client.CancelAsync();
    }

    private class QueueItem
    {
        public DownloadInfo DownloadInfo { get; set; }

        public string TempFolder { get; set; }

        public Action<string> FileCallback { get; set; }
    }
}