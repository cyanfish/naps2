using NAPS2.Scan;

namespace NAPS2.Images;

public class ExternalEditorSession : IDisposable
{
    public class Factory(ScanningContext scanningContext, UiImageList imageList, ErrorOutput errorOutput)
    {
        public ExternalEditorSession Create(UiImage uiImage, string externalEditorPath) =>
            new(scanningContext, imageList, errorOutput, uiImage, externalEditorPath);
    }

    private readonly ScanningContext _scanningContext;
    private readonly UiImageList _imageList;
    private readonly ErrorOutput _errorOutput;
    private readonly UiImage _uiImage;
    private readonly string _externalEditorPath;
    private readonly string _tempPath;
    private readonly FileSystemWatcher _watcher;
    private readonly TimedThrottle _throttle;

    private ExternalEditorSession(ScanningContext scanningContext, UiImageList imageList, ErrorOutput errorOutput,
        UiImage uiImage, string externalEditorPath)
    {
        _scanningContext = scanningContext;
        _imageList = imageList;
        _errorOutput = errorOutput;
        _uiImage = uiImage;
        _externalEditorPath = externalEditorPath;
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
        catch (IOException ex) when (ex is not FileNotFoundException)
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
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = _externalEditorPath,
                Arguments = _tempPath,
            });
            if (process == null)
            {
                throw new InvalidOperationException("Could not start process");
            }
        }
        catch (Exception ex)
        {
            _errorOutput.DisplayError(string.Format(UiStrings.ErrorStartingApplication, _externalEditorPath), ex);
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