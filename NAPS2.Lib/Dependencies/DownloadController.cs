using System.Net.Http;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using NAPS2.Scan;

namespace NAPS2.Dependencies;

public class DownloadController
{
    private static readonly HttpClient DefaultHttpClient = new();

    private readonly ScanningContext _scanningContext;
    private readonly ILogger _logger;

    private readonly HttpClient _client;
    private readonly List<QueueItem> _filesToDownload = new();
    private bool _cancel;

    public DownloadController(ScanningContext scanningContext, HttpClient? client = null)
    {
        _scanningContext = scanningContext;
        _logger = scanningContext.Logger;
        _client = client ?? DefaultHttpClient;
    }

    public int FilesDownloaded { get; private set; }

    public int TotalFiles => _filesToDownload.Count;

    public long CurrentFileSize { get; private set; }

    public long CurrentFileProgress { get; private set; }

    public void QueueFile(DownloadInfo downloadInfo, Action<string> fileCallback)
    {
        _filesToDownload.Add(new QueueItem(downloadInfo, fileCallback));
    }

    public void QueueFile(IExternalComponent component)
    {
        _filesToDownload.Add(new QueueItem(component.DownloadInfo, component.Install));
    }

    public void Stop()
    {
        _cancel = true;
        _client.CancelPendingRequests();
    }

    public event EventHandler? DownloadError;

    public event EventHandler? DownloadComplete;

    public event EventHandler? DownloadProgress;

    private async Task<MemoryStream?> TryDownloadFromUrlAsync(string filename, string url)
    {
        CurrentFileProgress = 0;
        CurrentFileSize = 0;
        DownloadProgress?.Invoke(this, EventArgs.Empty);
        try
        {
            var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            CurrentFileSize = response.Content.Headers.ContentLength.GetValueOrDefault();

            using var contentStream = await response.Content.ReadAsStreamAsync();

            var result = new MemoryStream();
            long previousLength;
            byte[] buffer = new byte[1024 * 40];
            do
            {
                previousLength = result.Length;
                int length = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                if (length > 0)
                {
                    result.Write(buffer, 0, length);
                    CurrentFileProgress = result.Length;
                    DownloadProgress?.Invoke(this, EventArgs.Empty);
                }
                if (_cancel)
                {
                    throw new OperationCanceledException();
                }
            }
            while (previousLength < result.Length);
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FileName}", filename);
            return null;
        }
    }

    private async Task<MemoryStream?> TryDownloadQueueItemAsync(QueueItem fileToDownload)
    {
        foreach (var url in fileToDownload.DownloadInfo.Urls)
        {
            var result = await TryDownloadFromUrlAsync(fileToDownload.DownloadInfo.FileName, url);
            if (result != null)
            {
                result.Position = 0;
                if (fileToDownload.DownloadInfo.Sha256 == CalculateSha256(result))
                {
                    return result;
                }
                _logger.LogError("Error downloading file (invalid checksum): {FileName}", fileToDownload.DownloadInfo.FileName);
            }
        }
        return null;
    }

    private async Task<bool> InternalStartDownloadsAsync()
    {
        FilesDownloaded = 0;
        foreach (var fileToDownload in _filesToDownload)
        {
            MemoryStream? result;
            try
            {
                result = await TryDownloadQueueItemAsync(fileToDownload);
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            if (result == null)
            {
                DownloadComplete?.Invoke(this, EventArgs.Empty);
                DownloadError?.Invoke(this, EventArgs.Empty);
                return false;
            }

            string tempFolder = Path.Combine(_scanningContext.TempFolderPath, Path.GetRandomFileName());
            Directory.CreateDirectory(tempFolder);
            string p = Path.Combine(tempFolder, fileToDownload.DownloadInfo.FileName);
            try
            {
                result.Position = 0;
                var preparedFilePath = fileToDownload.DownloadInfo.Format.Prepare(result, p);
                fileToDownload.FileCallback(preparedFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing downloaded file");
                DownloadError?.Invoke(this, EventArgs.Empty);
            }
            FilesDownloaded++;
            Directory.Delete(tempFolder, true);
        }

        DownloadComplete?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private string CalculateSha256(Stream stream)
    {
        using var sha = SHA256.Create();
        byte[] checksum = sha.ComputeHash(stream);
        string str = BitConverter.ToString(checksum).Replace("-", String.Empty).ToLowerInvariant();
        return str;
    }

    private record QueueItem(DownloadInfo DownloadInfo, Action<string> FileCallback);

    public async Task<bool> StartDownloadsAsync()
    {
        DownloadProgress?.Invoke(this, EventArgs.Empty);
        return await InternalStartDownloadsAsync();
    }
}