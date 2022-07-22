using System.Drawing;
using NAPS2.Images.Gdi;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class ImageSerializerTests : ContextualTests
{
    // TODO: Add scanning context tests that verify created images have the right storage

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void SerializesMetadata(StorageConfig config)
    {
        config.Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext());

        using var sourceImage = ScanningContext.CreateProcessedImage(
            new GdiImage(SharedData.color_image), // TODO: Use an actual grayscale image
            BitDepth.Grayscale,
            true,
            -1,
            new []{ new BrightnessTransform(300) });

        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());
        
        Assert.Single(destImage.TransformState.Transforms);
        Assert.Equal(300, Assert.IsType<BrightnessTransform>(destImage.TransformState.Transforms[0]).Brightness);
        Assert.True(destImage.Metadata.Lossless);
        Assert.Equal(BitDepth.Grayscale, destImage.Metadata.BitDepth);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void DeserializeToFileStorage(StorageConfig config)
    {
        config.Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext(),
            FileStorageManager.CreateFolder(Path.Combine(FolderPath, "dest")));

        using var sourceImage = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());
        
        Assert.IsType<ImageFileStorage>(destImage.Storage);
        // Check that disposing the original doesn't interfere with rendering, i.e. not using the same backing file
        sourceImage.Dispose();
        ImageAsserts.Similar(TransformTestsData.color_image, ImageContext.Render(destImage));
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void DeserializeToMemoryStorage(StorageConfig config)
    {
        config.Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext());

        using var sourceImage = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());
        
        Assert.IsType<GdiImage>(destImage.Storage);
        // Check that disposing the original doesn't interfere with rendering, i.e. not using the same image
        sourceImage.Dispose();
        ImageAsserts.Similar(SharedData.color_image, ImageContext.Render(destImage));
    }

    // TODO: Add tests for serialize/deserialize options, pdf files, etc.
}