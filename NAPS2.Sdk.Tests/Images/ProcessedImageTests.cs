using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class ProcessedImageTests : ContextualTests
{
    [Fact]
    public void Construct()
    {
        var storage = LoadImage(ImageResources.dog);

        var image1 = ScanningContext.CreateProcessedImage(storage);
        ImageAsserts.Similar(storage, (IMemoryImage) image1.Storage, 0);
        Assert.False(image1.Metadata.Lossless);
        Assert.True(image1.TransformState.IsEmpty);

        var image2 = ScanningContext.CreateProcessedImage(storage, lossless: true,
            transforms: [new CropTransform(0, 50, 0, 50)]);
        ImageAsserts.Similar(storage, (IMemoryImage) image2.Storage, 0);
        Assert.True(image2.Metadata.Lossless);
        Assert.Single(image2.TransformState.Transforms);
        var cropTransform = Assert.IsType<CropTransform>(image2.TransformState.Transforms[0]);
        Assert.Equal(0, cropTransform.Left);
        Assert.Equal(0, cropTransform.Top);
        Assert.Equal(50, cropTransform.Right);
        Assert.Equal(50, cropTransform.Bottom);
    }

    [Fact]
    public void StorageDisposed()
    {
        var storageMock = Substitute.For<IImageStorage>();
        var image = ScanningContext.CreateProcessedImage(storageMock);

        image.Dispose();

        storageMock.Received().Dispose();
    }

    [Fact]
    public void StorageDisposedOnlyAfterAllClonesDisposed()
    {
        var storageMock = Substitute.For<IImageStorage>();
        var image = ScanningContext.CreateProcessedImage(storageMock);
        var image2 = image.Clone();
        var image3 = image.Clone();
        var image4 = image2.Clone();

        image.Dispose();
        storageMock.DidNotReceive().Dispose();

        image2.Dispose();
        storageMock.DidNotReceive().Dispose();

        // Check extra calls on a single reference don't have an effect
        image3.Dispose();
        image3.Dispose();
        image3.Dispose();
        storageMock.DidNotReceive().Dispose();

        image4.Dispose();
        storageMock.Received().Dispose();
    }

    [Fact]
    public void TransformSimplification()
    {
        var storageMock = Substitute.For<IImageStorage>();
        var image = ScanningContext.CreateProcessedImage(storageMock);

        // 90deg transform 
        var image2 = image.WithTransform(new RotationTransform(90));
        Assert.Single(image2.TransformState.Transforms);
        var transform = Assert.IsType<RotationTransform>(image2.TransformState.Transforms[0]);
        Assert.Equal(90, transform.Angle);
        Assert.NotEqual(image, image2);

        // Another 90deg transform should simplify to 180deg
        var image3 = image2.WithTransform(new RotationTransform(90));
        Assert.Single(image3.TransformState.Transforms);
        var transform2 = Assert.IsType<RotationTransform>(image3.TransformState.Transforms[0]);
        Assert.Equal(180, transform2.Angle);
        Assert.NotEqual(image, image3);
        Assert.NotEqual(image2, image3);

        // Another 180deg transform should simplify to 360deg and remove the whole transform
        var image4 = image3.WithTransform(new RotationTransform(180));
        Assert.True(image4.TransformState.IsEmpty);
        // Equality should match the original image
        Assert.Equal(image, image4);

        image.Dispose();
        image2.Dispose();
        image3.Dispose();
        storageMock.DidNotReceive().Dispose();
        image4.Dispose();
        storageMock.Received().Dispose();
    }

    [Fact]
    public void MultipleTransforms()
    {
        var storageMock = Substitute.For<IImageStorage>();
        var image = ScanningContext.CreateProcessedImage(storageMock);

        // 90deg transform 
        var image2 = image.WithTransform(new RotationTransform(90));
        Assert.Single(image2.TransformState.Transforms);
        var transform = Assert.IsType<RotationTransform>(image2.TransformState.Transforms[0]);
        Assert.Equal(90, transform.Angle);
        Assert.NotEqual(image, image2);

        // Brightness transform
        var image3 = image2.WithTransform(new BrightnessTransform(100));
        Assert.Equal(2, image3.TransformState.Transforms.Count);
        var transform1 = Assert.IsType<RotationTransform>(image3.TransformState.Transforms[0]);
        Assert.Equal(90, transform1.Angle);
        var transform2 = Assert.IsType<BrightnessTransform>(image3.TransformState.Transforms[1]);
        Assert.Equal(100, transform2.Brightness);
        Assert.NotEqual(image, image3);

        // Remove all transforms
        var image4 = image3.WithNoTransforms();
        Assert.True(image4.TransformState.IsEmpty);
        // Equality should match the original image
        Assert.Equal(image, image4);

        image.Dispose();
        image2.Dispose();
        image3.Dispose();
        storageMock.DidNotReceive().Dispose();
        image4.Dispose();
        storageMock.Received().Dispose();
    }

    [Fact]
    public void CloneAfterDisposed()
    {
        var storageMock = Substitute.For<IImageStorage>();
        var image = ScanningContext.CreateProcessedImage(storageMock);

        image.Dispose();
        storageMock.Received().Dispose();

        Assert.Throws<ObjectDisposedException>(() => image.Clone());
    }
}