using NAPS2.Scan;

namespace NAPS2.Images;

public class ExternalEditorSession : IDisposable
{
    public class Factory(
        ScanningContext scanningContext,
        UiImageList imageList,
        ErrorOutput errorOutput,
        IOpenWith openWith)
    {
        public ExternalEditorSession Create(UiImage uiImage, string appPath) =>
            new(scanningContext, imageList, errorOutput, openWith, uiImage, appPath);
    }

    private readonly ScanningContext _scanningContext;
    private readonly UiImageList _imageList;
    private readonly ErrorOutput _errorOutput;
    private readonly IOpenWith _openWith;
    private readonly UiImage _uiImage;
    private readonly string _appPath;
    private readonly string _tempPath;
    private readonly FileSystemWatcher _watcher;
    private readonly TimedThrottle _throttle;

    private ExternalEditorSession(ScanningContext scanningContext, UiImageList imageList, ErrorOutput errorOutput,
        IOpenWith openWith, UiImage uiImage, string appPath)
    {
        _scanningContext = scanningContext;
        _imageList = imageList;
        _errorOutput = errorOutput;
        _openWith = openWith;
        _uiImage = uiImage;
        _appPath = appPath;
        _tempPath = CreateUserEditableFile();
        _watcher = CreateWatcher();
        _throttle = new TimedThrottle(ExternalImageEdited, TimeSpan.FromSeconds(1));
        StartExternalEditor();
    }

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
        var watcher = new FileSystemWatcher(Path.GetDirectoryName(_tempPath)!, Path.GetFileName(_tempPath))
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
            if (Path.GetFileName(e.FullPath) == Path.GetFileName(_tempPath))
            {
                _throttle.RunAction(null);
            }
        };
        return watcher;
    }

    private void ExternalImageEdited()
    {
        IMemoryImage newMemoryImage;
        try
        {
            newMemoryImage = _scanningContext.ImageContext.Load(_tempPath);
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            // We might have tried to read the file while the application is still writing it
            // If we re-run the throttle it will try again after a delay
            _throttle.RunAction(null);
            return;
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

    private void StartExternalEditor()
    {
        try
        {
            _openWith.OpenWith(_appPath, _tempPath);
        }
        catch (Exception ex)
        {
            _errorOutput.DisplayError(string.Format(UiStrings.ErrorStartingApplication, _appPath), ex);
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
        try
        {
            File.Delete(_tempPath);
            Directory.Delete(Path.GetDirectoryName(_tempPath)!, true);
        }
        catch (IOException)
        {
            // Best effort cleanup; the temp folder will get cleared the next time we start NAPS2 anyway
        }
    }
}