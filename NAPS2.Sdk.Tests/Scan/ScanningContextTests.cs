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
        var image1 = context.CreateProcessedImage(storage1.Object);
        var image2 = context.CreateProcessedImage(storage2.Object);
        
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
        // Create a simple image
        context.CreateProcessedImage(storage1.Object);
        // Create an image, clone it a couple times, and dispose the original
        // The underling storage shouldn't be disposed yet due to refcounting
        // This tests the interaction between refcounting and context disposal
        var imageToClone = context.CreateProcessedImage(storage2.Object);
        imageToClone.Clone();
        imageToClone.Clone();
        imageToClone.Dispose();
        
        storage1.VerifyNoOtherCalls();
        storage2.VerifyNoOtherCalls();
        context.Dispose();
        storage1.Verify(x => x.Dispose());
        storage2.Verify(x => x.Dispose());
    }
}