using System.ComponentModel;
using System.Net;
using System.Security.Cryptography;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.Dependencies;
using NAPS2.EtoForms.Layout;
using Label = Eto.Forms.Label;

namespace NAPS2.EtoForms.Ui;

public class DownloadProgressForm : EtoDialogBase
{
    private readonly Label _totalStatus = new();
    private readonly Label _fileStatus = new();
    private readonly ProgressBar _totalProgressBar = new();
    private readonly ProgressBar _fileProgressBar = new();

    private readonly List<QueueItem> _filesToDownload = new List<QueueItem>();
    private int _filesDownloaded = 0;
    private int _urlIndex = 0;
    private double _currentFileSize = 0.0;
    private double _currentFileProgress = 0.0;
    private bool _hasError;
    private bool _cancel;

    private readonly WebClient _client = new WebClient();

    public DownloadProgressForm(Naps2Config config) : base(config)
    {
        // TODO: Is this needed for net462?
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        _client.DownloadFileCompleted += client_DownloadFileCompleted;
        _client.DownloadProgressChanged += client_DownloadProgressChanged;
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.DownloadProgressFormTitle;
        Icon = new Icon(1f, Icons.text_small.ToEtoImage());

        FormStateController.RestoreFormState = false;

        LayoutController.Content = L.Column(
            _totalStatus,
            EtoPlatform.Current.FormatProgressBar(_totalProgressBar),
            EtoPlatform.Current.FormatProgressBar(_fileProgressBar),
            L.Row(
                _fileStatus.XScale()
                    .Align(EtoPlatform.Current.IsWinForms ? LayoutAlignment.Center : LayoutAlignment.Leading),
                C.Button(UiStrings.Cancel, Close)
            )
        );
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        DisplayProgress();
        StartNextDownload();
    }

    void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        Console.WriteLine($"bytes {e.BytesReceived} {e.TotalBytesToReceive}");
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
                MessageBox.Show(MiscResources.FilesCouldNotBeDownloaded, MiscResources.DownloadError, MessageBoxButtons.OK, MessageBoxType.Error);
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
            MessageBox.Show(MiscResources.FilesCouldNotBeDownloaded, MiscResources.DownloadError, MessageBoxButtons.OK, MessageBoxType.Error);
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
        _totalStatus.Text = string.Format(MiscResources.FilesProgressFormat, _filesDownloaded, _filesToDownload.Count);
        _totalProgressBar.MaxValue = _filesToDownload.Count * 1000;
        _totalProgressBar.Value = _filesDownloaded * 1000 + (_currentFileSize == 0 ? 0 : (int)(_currentFileProgress / _currentFileSize * 1000));
        _fileStatus.Text = string.Format(MiscResources.SizeProgress, (_currentFileProgress / 1e6).ToString("f1"), (_currentFileSize / 1e6).ToString("f1"));
        if (_currentFileSize > 0)
        {
            _fileProgressBar.MaxValue = (int) (_currentFileSize);
            _fileProgressBar.Value = (int) (_currentFileProgress);
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
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