using NAPS2.Pdf;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

// TODO: Add tests for barcode detection
public class ImageSerializerTests : ContextualTests
{
    // TODO: Add scanning context tests that verify created images have the right storage

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void SerializesMetadata(StorageConfig config)
    {
        config.Apply(this);

        using var destContext = new ScanningContext(TestImageContextFactory.Get());

        using var sourceImage = ScanningContext.CreateProcessedImage(
            LoadImage(ImageResources.dog), // TODO: Use an actual grayscale image
            true,
            -1,
            PageSize.A4,
            new[] { new BrightnessTransform(300) });

        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.Single(destImage.TransformState.Transforms);
        Assert.Equal(300, Assert.IsType<BrightnessTransform>(destImage.TransformState.Transforms[0]).Brightness);
        Assert.True(destImage.Metadata.Lossless);
        Assert.Equal(PageSize.A4, destImage.Metadata.PageSize);
        ImageAsserts.Similar(ImageResources.dog_b_p300, destImage);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void DeserializeToFileStorage(StorageConfig config)
    {
        config.Apply(this);
        using var destContext = CreateDestContextWithFileStorage();

        using var sourceImage = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.IsType<ImageFileStorage>(destImage.Storage);
        // Check that disposing the original doesn't interfere with rendering, i.e. not using the same backing file
        sourceImage.Dispose();
        ImageAsserts.Similar(ImageResources.dog, destImage);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void DeserializeToMemoryStorage(StorageConfig config)
    {
        config.Apply(this);
        using var destContext = new ScanningContext(TestImageContextFactory.Get());

        using var sourceImage = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.IsAssignableFrom<IMemoryImage>(destImage.Storage);
        // Check that disposing the original doesn't interfere with rendering, i.e. not using the same image
        sourceImage.Dispose();
        ImageAsserts.Similar(ImageResources.dog, destImage);
    }

    [Fact]
    public void ShareFileStorage()
    {
        new StorageConfig.File().Apply(this);
        using var destContext = CreateDestContextWithFileStorage();

        using var sourceImage = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions
        {
            ShareFileStorage = true
        });

        var sourceStorage = Assert.IsType<ImageFileStorage>(sourceImage.Storage);
        var destStorage = Assert.IsType<ImageFileStorage>(destImage.Storage);
        Assert.Equal(sourceStorage.FullPath, destStorage.FullPath);
        ImageAsserts.Similar(ImageResources.dog, destImage);

        destImage.Dispose();
        Assert.True(File.Exists(sourceStorage.FullPath));
        sourceImage.Dispose();
        Assert.False(File.Exists(sourceStorage.FullPath));
    }

    [Fact]
    public void TransferOwnership()
    {
        new StorageConfig.File().Apply(this);
        using var destContext = CreateDestContextWithFileStorage();

        using var sourceImage = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            TransferOwnership = true
        });
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        var sourceStorage = Assert.IsType<ImageFileStorage>(sourceImage.Storage);
        var destStorage = Assert.IsType<ImageFileStorage>(destImage.Storage);
        Assert.Equal(sourceStorage.FullPath, destStorage.FullPath);
        ImageAsserts.Similar(ImageResources.dog, destImage);

        sourceImage.Dispose();
        Assert.True(File.Exists(sourceStorage.FullPath));
        destImage.Dispose();
        Assert.False(File.Exists(sourceStorage.FullPath));
    }

    [Fact]
    public void CrossDevice()
    {
        new StorageConfig.File().Apply(this);
        using var destContext = CreateDestContextWithFileStorage();

        using var sourceImage = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            CrossDevice = true
        });
        // Disposing before deserialization should have no effect with CrossDevice set
        sourceImage.Dispose();
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        ImageAsserts.Similar(ImageResources.dog, destImage);
    }

    [Fact]
    public void DisposeBeforeDeserialization()
    {
        new StorageConfig.File().Apply(this);
        using var destContext = CreateDestContextWithFileStorage();

        using var sourceImage = CreateScannedImage();
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        sourceImage.Dispose();

        Assert.Throws<FileNotFoundException>(() =>
            ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions()));
    }

    [Fact]
    public void ShareFileStorage_DisposeBeforeDeserialization()
    {
        new StorageConfig.File().Apply(this);
        using var destContext = CreateDestContextWithFileStorage();

        using var sourceImage = CreateScannedImage();
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        sourceImage.Dispose();

        Assert.Throws<FileNotFoundException>(() =>
            ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions
            {
                ShareFileStorage = true
            }));
    }

    [Fact]
    public void TransferOwnershipOfSharedFile()
    {
        new StorageConfig.File().Apply(this);
        using var destContext = CreateDestContextWithFileStorage();

        using var sourceImage = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions
        {
            ShareFileStorage = true
        });

        Assert.Throws<ArgumentException>(() => ImageSerializer.Serialize(destImage, new SerializeImageOptions
        {
            TransferOwnership = true
        }));
    }

    [Fact]
    public void TransferOwnership_FailsWithClone()
    {
        new StorageConfig.File().Apply(this);

        using var sourceImage = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        using var clone = sourceImage.Clone();
        Assert.Throws<ArgumentException>(() => ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            TransferOwnership = true
        }));
    }

    [Fact]
    public void TransferOwnership_DisposesMemory()
    {
        using var sourceImage = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            TransferOwnership = true
        });
        Assert.Throws<ObjectDisposedException>(() => sourceImage.Clone());
    }

    [Fact]
    public void RequireFileStorage_NoFileStorage()
    {
        using var sourceImage = CreateScannedImage();
        Assert.Throws<ArgumentException>(() => ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            RequireFileStorage = true
        }));
    }

    [Fact]
    public void RequireFileStorage_AndCrossDevice()
    {
        using var sourceImage = CreateScannedImage();
        Assert.Throws<ArgumentException>(() => ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            RequireFileStorage = true,
            CrossDevice = true
        }));
    }

    [Fact]
    public void CrossDevice_AndRenderedFilePath()
    {
        using var sourceImage = CreateScannedImage();
        Assert.Throws<ArgumentException>(() => ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            CrossDevice = true,
            RenderedFilePath = "something.jpg"
        }));
    }

    [Fact]
    public void CrossDevice_AndTransferOwnership()
    {
        using var sourceImage = CreateScannedImage();
        Assert.Throws<ArgumentException>(() => ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            CrossDevice = true,
            TransferOwnership = true
        }));
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task PdfDeserializeToFileStorage(StorageConfig config)
    {
        config.Apply(this);
        using var destContext = CreateDestContextWithFileStorage();

        var importPath = Path.Combine(FolderPath, "import.pdf");
        File.WriteAllBytes(importPath, PdfResources.word_generated_pdf);

        using var sourceImage = await new PdfImporter(ScanningContext).Import(importPath).FirstAsync();
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.IsType<ImageFileStorage>(destImage.Storage);
        // Check that disposing the original doesn't interfere with rendering, i.e. not using the same backing file
        sourceImage.Dispose();
        ImageAsserts.Similar(PdfResources.word_p1, destImage, ignoreResolution: true);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task PdfDeserializeToMemoryStorage(StorageConfig config)
    {
        config.Apply(this);
        using var destContext = new ScanningContext(TestImageContextFactory.Get(new PdfiumPdfRenderer()));

        var importPath = Path.Combine(FolderPath, "import.pdf");
        File.WriteAllBytes(importPath, PdfResources.word_generated_pdf);

        using var sourceImage = await new PdfImporter(ScanningContext).Import(importPath).FirstAsync();
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.IsType<ImageMemoryStorage>(destImage.Storage);
        // Check that disposing the original doesn't interfere with rendering, i.e. not using the same image
        sourceImage.Dispose();
        ImageAsserts.Similar(PdfResources.word_p1, destImage, ignoreResolution: true);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void IncludeThumbnail(StorageConfig config)
    {
        config.Apply(this);
        using var destContext = new ScanningContext(TestImageContextFactory.Get());

        using var sourceImage = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        using var imageWithThumbnail = sourceImage.WithPostProcessingData(new PostProcessingData
        {
            Thumbnail = sourceImage.Render().PerformTransform(new ThumbnailTransform(256)),
            ThumbnailTransformState = TransformState.Empty
        }, true);

        var serializedImage = ImageSerializer.Serialize(imageWithThumbnail, new SerializeImageOptions
        {
            IncludeThumbnail = true
        });
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        ImageAsserts.Similar(ImageResources.dog_thumb_256, destImage.PostProcessingData.Thumbnail,
            ImageAsserts.XPLAT_RMSE_THRESHOLD, ignoreResolution: true);
    }

    [Fact]
    public void IncludeThumbnail_InvalidTransformState()
    {
        using var destContext = new ScanningContext(TestImageContextFactory.Get());

        using var sourceImage = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        using var imageWithThumbnail = sourceImage.WithPostProcessingData(new PostProcessingData
        {
            Thumbnail = sourceImage.Render().PerformTransform(new ThumbnailTransform(256)),
            ThumbnailTransformState = TransformState.Empty.AddOrSimplify(new BrightnessTransform(100))
        }, true);

        var serializedImage = ImageSerializer.Serialize(imageWithThumbnail, new SerializeImageOptions
        {
            IncludeThumbnail = true
        });
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.Null(destImage.PostProcessingData.Thumbnail);
    }

    private ScanningContext CreateDestContextWithFileStorage()
    {
        return new ScanningContext(TestImageContextFactory.Get(new PdfiumPdfRenderer()))
        {
            FileStorageManager = FileStorageManager.CreateFolder(Path.Combine(FolderPath, "dest"))
        };
    }
}