using NAPS2.EtoForms;

namespace NAPS2.Images;

public class ThumbnailController : IDisposable
{
    private readonly ThumbnailRenderQueue _renderQueue;
    private readonly Naps2Config _config;

    private double _oversample = 1;

    public ThumbnailController(ThumbnailRenderQueue renderQueue, Naps2Config config)
    {
        _renderQueue = renderQueue;
        _config = config;
    }

    public IListView<UiImage>? ListView { get; set; }

    public double Oversample
    {
        get => _oversample;
        set
        {
            _oversample = value;
            _renderQueue.SetThumbnailSize(RenderSize);
        }
    }

    /// <summary>
    /// The size to render thumbnail images at. This can be bigger than VisibleSize if the screen is high-dpi.
    /// </summary>
    public int RenderSize => (int) (VisibleSize * Oversample);

    /// <summary>
    /// The configured size of the thumbnails.
    /// </summary>
    public int VisibleSize
    {
        get
        {
            var size = _config.Get(c => c.ThumbnailSize);
            if (size == 0)
            {
                return ThumbnailSizes.DEFAULT_SIZE;
            }
            return ThumbnailSizes.Validate(size);
        }
        set
        {
            var thumbnailSize = ThumbnailSizes.Validate(value);
            _config.User.Set(c => c.ThumbnailSize, thumbnailSize);
            if (ListView?.ImageSize == thumbnailSize)
            {
                // Same size so no resizing needed
                return;
            }
            Reload();
            ThumbnailSizeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? ThumbnailSizeChanged;

    public void Reload()
    {
        if (ListView != null)
        {
            // Adjust the visible thumbnail display with the new size
            ListView.ImageSize = VisibleSize;
            ListView.RegenerateImages();
        }

        // Render high-quality thumbnails at the new size in a background task
        // The existing (poorly scaled) thumbnails are used in the meantime
        _renderQueue.SetThumbnailSize(RenderSize);
    }

    public void Init(UiImageList imageList)
    {
        _renderQueue.StartRendering(imageList);
        Reload();
    }

    public void Dispose()
    {
        _renderQueue.Dispose();
    }
}