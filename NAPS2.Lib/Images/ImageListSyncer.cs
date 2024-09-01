using System.Threading;

namespace NAPS2.Images;

/// <summary>
/// Sends changes in the given UiImageList to the given callback when images are updated. Passive updates (e.g.
/// receiving the next image from a scan/import) are throttled to avoid excessive UI updates.
///
/// If any images are present in the list upon construction, they will be immediately sent to the callback. 
/// </summary>
public class ImageListSyncer
{
    private static readonly TimeSpan SyncThrottleInterval = TimeSpan.FromMilliseconds(200);

    private readonly UiImageList _imageList;
    private readonly Action<ListViewDiffs<UiImage>> _diffCallback;
    private readonly SynchronizationContext _syncContext;
    private readonly TimedThrottle _syncThrottle;
    private readonly ImageListDiffer _differ;
    private bool _disposed;

    public ImageListSyncer(UiImageList imageList, Action<ListViewDiffs<UiImage>> diffCallback,
        SynchronizationContext syncContext)
    {
        _imageList = imageList;
        _diffCallback = diffCallback;
        _syncContext = syncContext;
        _imageList.ImagesUpdated += ImagesUpdated;
        _imageList.ImagesThumbnailChanged += ImagesUpdated;
        _imageList.ImagesThumbnailInvalidated += ImagesUpdated;
        _syncThrottle = new TimedThrottle(Sync, SyncThrottleInterval);
        _differ = new ImageListDiffer(_imageList);
        Sync();
    }

    private void ImagesUpdated(object? sender, ImageListEventArgs e)
    {
        if (e.IsPassiveInteraction)
        {
            _syncThrottle.RunAction(_syncContext);
        }
        else
        {
            _syncThrottle.RunActionNow(_syncContext);
        }
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
            _imageList.ImagesThumbnailChanged -= ImagesUpdated;
            _imageList.ImagesThumbnailInvalidated -= ImagesUpdated;
        }
    }
}