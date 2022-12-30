using System.ComponentModel;
using System.Net;
using System.Security.Cryptography;
using NAPS2.Scan;

namespace NAPS2.Dependencies;

public class DownloadController
{
    private readonly ScanningContext _scanningContext;

    // TODO: Migrate to HttpClient
#pragma warning disable SYSLIB0014
    private readonly WebClient _client = new();
#pragma warning restore SYSLIB0014

    private readonly List<QueueItem> _filesToDownload = new();
    private readonly TaskCompletionSource<bool> _completionSource = new();
    private int _urlIndex;
    private bool _hasError;
    private bool _cancel;

    public DownloadController(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
        // TODO: Is this needed for net462?
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        _client.DownloadFileCompleted += client_DownloadFileCompleted;
        _client.DownloadProgressChanged += client_DownloadProgressChanged;
    }

    public int FilesDownloaded { get; private set; }

    public int TotalFiles => _filesToDownload.Count;

    public double CurrentFileSize { get; private set; }

    public double CurrentFileProgress { get; private set; }

    public Task CompletionTask => _completionSource.Task;

    void client_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        CurrentFileProgress = e.BytesReceived;
        CurrentFileSize = e.TotalBytesToReceive;
        DownloadProgress?.Invoke(this, EventArgs.Empty);
    }

    void client_DownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        var file = _filesToDownload[FilesDownloaded];
        if (e.Error != null)
        {
            _hasError = true;
            if (!_cancel)
            {
                Log.ErrorException("Error downloading file: " + file.DownloadInfo.FileName, e.Error);
            }
        }
        else if (file.DownloadInfo.Sha1 != CalculateSha1(Path.Combine(file.TempFolder!, file.DownloadInfo.FileName)))
        {
            _hasError = true;
            Log.Error("Error downloading file (invalid checksum): " + file.DownloadInfo.FileName);
        }
        else
        {
            FilesDownloaded++;
        }
        CurrentFileProgress = 0;
        CurrentFileSize = 0;
        DownloadProgress?.Invoke(this, EventArgs.Empty);
        StartNextDownload();
    }

    public void QueueFile(DownloadInfo downloadInfo, Action<string> fileCallback)
    {
        _filesToDownload.Add(new QueueItem { DownloadInfo = downloadInfo, FileCallback = fileCallback });
    }

    public void QueueFile(IExternalComponent component)
    {
        _filesToDownload.Add(new QueueItem { DownloadInfo = component.DownloadInfo, FileCallback = component.Install });
    }

    public void Stop()
    {
        _cancel = true;
        _client.CancelAsync();
    }

    public event EventHandler? DownloadError;

    public event EventHandler? DownloadComplete;

    public event EventHandler? DownloadProgress;

    private void StartNextDownload()
    {
        if (_hasError)
        {
            var prev = _filesToDownload[FilesDownloaded];
            Directory.Delete(prev.TempFolder!, true);
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
        if (FilesDownloaded > 0 && _urlIndex == 0)
        {
            var prev = _filesToDownload[FilesDownloaded - 1];
            var filePath = Path.Combine(prev.TempFolder!, prev.DownloadInfo.FileName);
            try
            {
                var preparedFilePath = prev.DownloadInfo.Format.Prepare(filePath);
                prev.FileCallback(preparedFilePath);
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error preparing downloaded file", ex);
                DownloadError?.Invoke(this, EventArgs.Empty);
            }
            Directory.Delete(prev.TempFolder!, true);
        }
        if (FilesDownloaded >= _filesToDownload.Count)
        {
            DownloadComplete?.Invoke(this, EventArgs.Empty);
            _completionSource.SetResult(true);
            return;
        }
        if (_urlIndex >= _filesToDownload[FilesDownloaded].DownloadInfo.Urls.Count)
        {
            DownloadComplete?.Invoke(this, EventArgs.Empty);
            _completionSource.SetResult(false);
            DownloadError?.Invoke(this, EventArgs.Empty);
            return;
        }
        var next = _filesToDownload[FilesDownloaded];
        next.TempFolder = Path.Combine(_scanningContext.TempFolderPath, Path.GetRandomFileName());
        Directory.CreateDirectory(next.TempFolder);
        _client.DownloadFileAsync(new Uri(next.DownloadInfo.Urls[_urlIndex]), Path.Combine(next.TempFolder, next.DownloadInfo.FileName));
    }

    private string CalculateSha1(string filePath)
    {
        using var sha = SHA1.Create();
        using FileStream stream = File.OpenRead(filePath);
        byte[] checksum = sha.ComputeHash(stream);
        string str = BitConverter.ToString(checksum).Replace("-", String.Empty).ToLowerInvariant();
        return str;
    }

    private class QueueItem
    {
        public required DownloadInfo DownloadInfo { get; set; }

        public string? TempFolder { get; set; }

        public required Action<string> FileCallback { get; set; }
    }

    public void Start()
    {
        DownloadProgress?.Invoke(this, EventArgs.Empty);
        StartNextDownload();
    }
}