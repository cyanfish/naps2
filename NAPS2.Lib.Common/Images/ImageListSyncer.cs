using System.Threading;

namespace NAPS2.Images;

// TODO: We should differentiate between active interactions (e.g. clicking Delete on one image) and passive interactions (e.g. receiving the next image from a large import).
// TODO: An active interaction should force an immediate sync, it should not be throttled.
// TODO: Also, passive interactions should not change the selection, because that could confuse the user when the UI doesn't show their selection as up to date. 
public class ImageListSyncer
{
    private static readonly TimeSpan SYNC_INTERVAL = TimeSpan.FromMilliseconds(100);
    
    private readonly UiImageList _imageList;
    private readonly Action<ImageListDiffs> _diffCallback;
    private readonly SynchronizationContext _syncContext;
    private readonly TimedThrottle _syncThrottle;
    private readonly ImageListDiffer _differ;
    private bool _disposed;

    public ImageListSyncer(UiImageList imageList, Action<ImageListDiffs> diffCallback, SynchronizationContext syncContext)
    {
        _imageList = imageList;
        _diffCallback = diffCallback;
        _syncContext = syncContext;
        _imageList.ImagesUpdated += ImagesUpdated;
        _imageList.ImagesThumbnailInvalidated += ImagesUpdated;
        _syncThrottle = new TimedThrottle(Sync, SYNC_INTERVAL);
        _differ = new ImageListDiffer(_imageList);
    }

    private void ImagesUpdated(object? sender, EventArgs e)
    {
        _syncThrottle.RunAction(_syncContext);
    }

    private void Sync()
    {
        var diffs = _differ.GetAndFlushDiffs();
        if (diffs.HasAnyDiff)
        {
            _diffCallback(diffs);
        }
    }

    public void Dispose()
    {
        lock (this)
        {
            if (_disposed) return;
            _disposed = true;
            _imageList.ImagesUpdated -= ImagesUpdated;
        }
    }
    
}