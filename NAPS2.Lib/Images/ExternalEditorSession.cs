using Microsoft.Extensions.Logging;
using NAPS2.Scan;

namespace NAPS2.Images;

public class ExternalEditorSession : IDisposable
{
    private readonly ScanningContext _scanningContext;
    private readonly UiImageList _imageList;
    private readonly UiImage _uiImage;
    private readonly FileSystemWatcher _watcher;
    private readonly TimedThrottle _throttle;

    public ExternalEditorSession(ScanningContext scanningContext, UiImageList imageList, UiImage uiImage)
    {
        _scanningContext = scanningContext;
        _imageList = imageList;
        _uiImage = uiImage;
        TempFilePath = CreateUserEditableFile();
        _watcher = CreateWatcher();
        _throttle = new TimedThrottle(ExternalImageEdited, TimeSpan.FromSeconds(1));
    }

    public string TempFilePath { get; }

    private string CreateUserEditableFile()
    {
        string dir = Path.Combine(Paths.Temp, Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, $"page{_imageList.Images.IndexOf(_uiImage) + 1}");
        using var processedImage = _uiImage.GetClonedImage();
        using var renderedImage = processedImage.Render();
        return ImageExportHelper.SaveSmallestFormat(path, renderedImage, false, -1, out _);
    }

    private FileSystemWatcher CreateWatcher()
    {
        var watcher = new FileSystemWatcher(Path.GetDirectoryName(TempFilePath)!, Path.GetFileName(TempFilePath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true,
        };
        // Editors might either write the file directly, or they might create a temporary file which gets renamed for
        // an atomic write. Either way we need to be able to detect the action.
        watcher.Created += (_, _) => _throttle.RunAction(null);
        watcher.Changed += (_, _) => _throttle.RunAction(null);
        watcher.Renamed += (_, e) =>
        {
            if (Path.GetFileName(e.FullPath) == Path.GetFileName(TempFilePath))
            {
                _throttle.RunAction(null);
            }
        };
        return watcher;
    }

    private void ExternalImageEdited()
    {
        IMemoryImage newMemoryImage;
        var fallback = new ExpFallback(50, 1000);
        while (true)
        {
            try
            {
                newMemoryImage = _scanningContext.ImageContext.Load(TempFilePath);
                break;
            }
            catch (Exception ex) when (ex is not FileNotFoundException)
            {
                if (fallback.IsAtMax)
                {
                    _scanningContext.Logger.LogError(ex, "Error loading externally-edited image");
                    return;
                }
                fallback.Increase();
            }
        }
        using (newMemoryImage)
        {
            var newImage = _scanningContext.CreateProcessedImage(newMemoryImage);
            lock (_uiImage)
            {
                if (!_uiImage.IsDisposed)
                {
                    _uiImage.ReplaceInternalImage(newImage);
                    _imageList.ClearUndoStack();
                }
                else
                {
                    newImage.Dispose();
                }
            }
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
        try
        {
            File.Delete(TempFilePath);
            Directory.Delete(Path.GetDirectoryName(TempFilePath)!, true);
        }
        catch (IOException)
        {
            // Best effort cleanup; the temp folder will get cleared the next time we start NAPS2 anyway
        }
    }
}