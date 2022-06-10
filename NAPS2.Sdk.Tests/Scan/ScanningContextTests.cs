using Moq;
using NAPS2.Images.Gdi;
using NAPS2.Scan;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class ScanningContextTests : ContextualTexts
{
    [Fact]
    public void CreateAndNormallyDisposeImages()
    {
        var context = new ScanningContext(new GdiImageContext());
        var storage1 = new Mock<IImageStorage>();
        var storage2 = new Mock<IImageStorage>();
        var image1 = context.CreateProcessedImage(storage1.Object, BitDepth.Color, false, 75, Enumerable.Empty<Transform>());
        var image2 = context.CreateProcessedImage(storage2.Object, BitDepth.Color, false, 75, Enumerable.Empty<Transform>());
        
        storage1.VerifyNoOtherCalls();
        image1.Dispose();
        storage1.Verify(x => x.Dispose());
        storage2.VerifyNoOtherCalls();
        image2.Dispose();
        storage2.Verify(x => x.Dispose());
    }

    [Fact]
    public void CreateAndDisposeImagesByDisposingContext()
    {
        var context = new ScanningContext(new GdiImageContext());
        var storage1 = new Mock<IImageStorage>();
        var storage2 = new Mock<IImageStorage>();
        context.CreateProcessedImage(storage1.Object, BitDepth.Color, false, 75, Enumerable.Empty<Transform>());
        context.CreateProcessedImage(storage2.Object, BitDepth.Color, false, 75, Enumerable.Empty<Transform>());
        
        storage1.VerifyNoOtherCalls();
        storage2.VerifyNoOtherCalls();
        context.Dispose();
        storage1.Verify(x => x.Dispose());
        storage2.Verify(x => x.Dispose());
    }
}