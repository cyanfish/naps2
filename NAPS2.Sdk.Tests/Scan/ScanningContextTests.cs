using NAPS2.Scan;
using NSubstitute;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class ScanningContextTests : ContextualTests
{
    [Fact]
    public void CreateAndNormallyDisposeImages()
    {
        var context = new ScanningContext(TestImageContextFactory.Get());
        var storage1 = Substitute.For<IImageStorage>();
        var storage2 = Substitute.For<IImageStorage>();
        var image1 = context.CreateProcessedImage(storage1);
        var image2 = context.CreateProcessedImage(storage2);
        
        storage1.DidNotReceive().Dispose();
        image1.Dispose();
        storage1.Received().Dispose();
        storage2.DidNotReceive().Dispose();
        image2.Dispose();
        storage2.Received().Dispose();
    }

    [Fact]
    public void CreateAndDisposeImagesByDisposingContext()
    {
        var context = new ScanningContext(TestImageContextFactory.Get());
        var storage1 = Substitute.For<IImageStorage>();
        var storage2 = Substitute.For<IImageStorage>();
        // Create a simple image
        context.CreateProcessedImage(storage1);
        // Create an image, clone it a couple times, and dispose the original
        // The underling storage shouldn't be disposed yet due to refcounting
        // This tests the interaction between refcounting and context disposal
        var imageToClone = context.CreateProcessedImage(storage2);
        imageToClone.Clone();
        imageToClone.Clone();
        imageToClone.Dispose();
        
        storage1.DidNotReceive().Dispose();
        storage2.DidNotReceive().Dispose();
        context.Dispose();
        storage1.Received().Dispose();
        storage2.Received().Dispose();
    }
}