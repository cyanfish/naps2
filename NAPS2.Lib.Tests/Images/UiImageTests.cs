using System.Threading;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Images;

// TODO: Add tests
public class UiImageTests : ContextualTests
{
    private static readonly TimeSpan Timeout = UiImage.StaleThumbnailDelay + TimeSpan.FromSeconds(10);
    private static readonly TimeSpan Tolerance = TimeSpan.FromMilliseconds(50);

    private readonly UiImage _image;
    private readonly SemaphoreSlim _thumbnailChanged = new(0);

    public UiImageTests()
    {
        _image = new UiImage(CreateScannedImage());
        _image.SetThumbnail(CreateThumbnail(), _image.TransformState);
        _image.ThumbnailChanged += (_, _) => _thumbnailChanged.Release();
    }

    [Fact]
    public async Task ThumbnailStale_AfterDelayUntilRendered()
    {
        var stopwatch = Stopwatch.StartNew();
        _image.AddTransform(new BrightnessTransform(100));

        Assert.True(_image.IsThumbnailDirty);
        Assert.False(_image.IsThumbnailStale);

        Assert.True(await _thumbnailChanged.WaitAsync(Timeout));
        Assert.True(stopwatch.Elapsed >= UiImage.StaleThumbnailDelay - Tolerance);
        Assert.True(_image.IsThumbnailStale);
        Assert.True(_image.GetImageRenderState().IsThumbnailStale);

        _image.SetThumbnail(CreateThumbnail(), _image.TransformState);

        Assert.False(_image.IsThumbnailStale);
    }

    [Fact]
    public async Task ThumbnailStale_NotIfRenderedWithinDelay()
    {
        _image.AddTransform(new BrightnessTransform(100));
        _image.SetThumbnail(CreateThumbnail(), _image.TransformState);
        Assert.True(await _thumbnailChanged.WaitAsync(0));

        Assert.False(await _thumbnailChanged.WaitAsync(UiImage.StaleThumbnailDelay + TimeSpan.FromMilliseconds(300)));
        Assert.False(_image.IsThumbnailStale);
    }

    [Fact]
    public async Task ThumbnailStale_DelayRestartsAfterRender()
    {
        _image.AddTransform(new BrightnessTransform(100));
        _image.SetThumbnail(CreateThumbnail(), _image.TransformState);
        Assert.True(await _thumbnailChanged.WaitAsync(0));
        await Task.Delay(UiImage.StaleThumbnailDelay - TimeSpan.FromMilliseconds(200));

        var stopwatch = Stopwatch.StartNew();
        _image.AddTransform(new BrightnessTransform(200));

        Assert.True(await _thumbnailChanged.WaitAsync(Timeout));
        Assert.True(stopwatch.Elapsed >= UiImage.StaleThumbnailDelay - Tolerance);
        Assert.True(_image.IsThumbnailStale);
    }

    private IMemoryImage CreateThumbnail() => ImageContext.Create(16, 16, ImagePixelFormat.RGB24);
}
