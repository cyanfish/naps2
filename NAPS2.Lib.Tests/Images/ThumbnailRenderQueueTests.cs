using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Images;

public class ThumbnailRenderQueueTests : ContextualTests
{
    private readonly UiImageList _uiImageList;
    private readonly ThumbnailRenderQueue _thumbnailRenderQueue;

    public ThumbnailRenderQueueTests()
    {
        _uiImageList = new UiImageList();
        _thumbnailRenderQueue = new ThumbnailRenderQueue(ScanningContext, new ThumbnailRenderer(ImageContext));
    }

    public override void Dispose()
    {
        base.Dispose();
        _thumbnailRenderQueue.Dispose();
    }

    [Fact]
    public void StartRendering_WithoutThumbnailSize_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _thumbnailRenderQueue.StartRendering(_uiImageList));
    }

    [Fact]
    public void StartRendering_CalledTwice_Throws()
    {
        _thumbnailRenderQueue.SetThumbnailSize(128);
        _thumbnailRenderQueue.StartRendering(_uiImageList);
        Assert.Throws<InvalidOperationException>(() => _thumbnailRenderQueue.StartRendering(_uiImageList));
    }

    [Fact]
    public void StartRendering_CalledAfterDispose_Throws()
    {
        _thumbnailRenderQueue.SetThumbnailSize(128);
        _thumbnailRenderQueue.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _thumbnailRenderQueue.StartRendering(_uiImageList));
    }

    [Fact]
    public void StartRendering_WithThumbnailNeeded_Renders()
    {
        AppendToImageList(CreateScannedImage());
        Assert.Null(_uiImageList.Images[0].GetThumbnailClone());
        Assert.True(_uiImageList.Images[0].IsThumbnailDirty);
        
        _thumbnailRenderQueue.SetThumbnailSize(128);
        _thumbnailRenderQueue.StartRendering(_uiImageList);
        _thumbnailRenderQueue.WaitForRendering();
        
        Assert.NotNull(_uiImageList.Images[0].GetThumbnailClone());
        Assert.False(_uiImageList.Images[0].IsThumbnailDirty);
    }
    
    // TODO: Test a bunch more rendering cases
    // TODO: Maybe make GetNextThumbnailToRender internal-visible or split to another class. Or we could just test
    // against the results.
    // TODO: Can we add a WaitForRendering function to use in tests?

    private void AppendToImageList(ProcessedImage image)
    {
        _uiImageList.Mutate(new ListMutation<UiImage>.Append(new UiImage(image)));
    }
}