using System.Net.Http;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using NAPS2.Scan;

namespace NAPS2.Dependencies;

public class DownloadController
{
    private readonly ScanningContext _scanningContext;
    private readonly ILogger _logger;

    private static readonly HttpClient _client = new();

    private readonly List<QueueItem> _filesToDownload = new();
    private bool _cancel;

    public DownloadController(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
        _logger = scanningContext.Logger;
    }

    public int FilesDownloaded { get; private set; }

    public int TotalFiles => _filesToDownload.Count;

    public long CurrentFileSize { get; private set; }

    public long CurrentFileProgress { get; private set; }

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
                if (fileToDownload.DownloadInfo.Sha1 == CalculateSha1(result))
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

            fileToDownload.TempFolder = Path.Combine(_scanningContext.TempFolderPath, Path.GetRandomFileName());
            Directory.CreateDirectory(fileToDownload.TempFolder);
            string p = Path.Combine(fileToDownload.TempFolder, fileToDownload.DownloadInfo.FileName);
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
            Directory.Delete(fileToDownload.TempFolder, true);
        }

        DownloadComplete?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private string CalculateSha1(Stream stream)
    {
        using var sha = SHA1.Create();
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

    public async Task<bool> StartDownloadsAsync()
    {
        DownloadProgress?.Invoke(this, EventArgs.Empty);
        return await InternalStartDownloadsAsync();
    }
}