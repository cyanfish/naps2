using System.Threading;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport;

// TODO: Use StorageConfig to test in-memory import (just need to figure out how to handle storage assertions, maybe delegate to StorageConfig)
public class ImageImporterTests : ContextualTests
{
    private readonly ImageImporter _imageImporter;

    public ImageImporterTests()
    {
        _imageImporter = new ImageImporter(ScanningContext);
        SetUpFileStorage();
    }

    [Fact]
    public async Task ImportPngImage()
    {
        var filePath = CopyResourceToFile(ImageResources.skewed_bw, "image.png");

        var source = _imageImporter.Import(filePath, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.True(File.Exists(storage.FullPath));
        Assert.Equal(Path.Combine(FolderPath, "recovery"), Path.GetDirectoryName(storage.FullPath));
        Assert.True(result[0].Metadata.Lossless);
        Assert.Null(result[0].PostProcessingData.Thumbnail);
        Assert.False(result[0].PostProcessingData.Barcode.IsDetectionAttempted);
        Assert.True(result[0].TransformState.IsEmpty);

        result[0].Dispose();
        Assert.False(File.Exists(storage.FullPath));
    }

    [Fact]
    public async Task ImportJpegImage()
    {
        var filePath = CopyResourceToFile(ImageResources.dog, "image.jpg");

        var source = _imageImporter.Import(filePath, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.True(File.Exists(storage.FullPath));
        Assert.Equal(Path.Combine(FolderPath, "recovery"), Path.GetDirectoryName(storage.FullPath));
        Assert.False(result[0].Metadata.Lossless);
        Assert.Null(result[0].PostProcessingData.Thumbnail);
        Assert.False(result[0].PostProcessingData.Barcode.IsDetectionAttempted);
        Assert.True(result[0].TransformState.IsEmpty);

        // Verify no re-encode happens
        var originalImage = ImageContext.Load(filePath);
        var storageImage = ImageContext.Load(storage.FullPath);
        ImageAsserts.Similar(originalImage, storageImage, 0);

        result[0].Dispose();
        Assert.False(File.Exists(storage.FullPath));
    }

    [Fact]
    public async Task ImportPngImageFromStream()
    {
        var fileStream = new MemoryStream(ImageResources.skewed_bw);

        var source = _imageImporter.Import(fileStream, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.True(File.Exists(storage.FullPath));
        Assert.Equal(Path.Combine(FolderPath, "recovery"), Path.GetDirectoryName(storage.FullPath));
        Assert.True(result[0].Metadata.Lossless);
        Assert.Null(result[0].PostProcessingData.Thumbnail);
        Assert.False(result[0].PostProcessingData.Barcode.IsDetectionAttempted);
        Assert.True(result[0].TransformState.IsEmpty);

        result[0].Dispose();
        Assert.False(File.Exists(storage.FullPath));
    }

    [Fact]
    public async Task ImportJpegImageFromStream()
    {
        var fileStream = new MemoryStream(ImageResources.dog);

        var source = _imageImporter.Import(fileStream, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.True(File.Exists(storage.FullPath));
        Assert.Equal(Path.Combine(FolderPath, "recovery"), Path.GetDirectoryName(storage.FullPath));
        Assert.False(result[0].Metadata.Lossless);
        Assert.Null(result[0].PostProcessingData.Thumbnail);
        Assert.False(result[0].PostProcessingData.Barcode.IsDetectionAttempted);
        Assert.True(result[0].TransformState.IsEmpty);

        // Verify no re-encode happens
        var originalImage = ImageContext.Load(fileStream);
        var storageImage = ImageContext.Load(storage.FullPath);
        ImageAsserts.Similar(originalImage, storageImage, 0);

        result[0].Dispose();
        Assert.False(File.Exists(storage.FullPath));
    }

    [PlatformFact(exclude: PlatformFlags.ImageSharpImage)]
    public async Task ImportTiffImage()
    {
        var filePath = CopyResourceToFile(ImageResources.animals_tiff, "image.tiff");

        var source = _imageImporter.Import(filePath, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Equal(3, result.Count);
        AssertUsesRecoveryStorage(result[0].Storage, "00001.jpg");
        Assert.False(result[0].Metadata.Lossless);
        ImageAsserts.Similar(ImageResources.dog, result[0]);

        AssertUsesRecoveryStorage(result[2].Storage, "00003.jpg");
        Assert.False(result[2].Metadata.Lossless);
        ImageAsserts.Similar(ImageResources.stock_cat, result[2]);

        result[0].Dispose();
        AssertRecoveryStorageCleanedUp(result[0].Storage);
        AssertUsesRecoveryStorage(result[2].Storage, "00003.jpg");
        result[2].Dispose();
        AssertRecoveryStorageCleanedUp(result[2].Storage);
    }

    [Fact]
    public async Task ImportWithThumbnailGeneration()
    {
        var filePath = CopyResourceToFile(ImageResources.dog, "image.jpg");

        var source = _imageImporter.Import(filePath, new ImportParams { ThumbnailSize = 256 });
        var result = await source.ToListAsync();

        Assert.Single(result);
        Assert.NotNull(result[0].PostProcessingData.Thumbnail);
        Assert.Equal(256, result[0].PostProcessingData.Thumbnail.Width);
    }

    [Fact(Skip = "Flaky")]
    public async Task SingleFrameProgress()
    {
        var filePath = CopyResourceToFile(ImageResources.dog, "image.jpg");

        var progressMock = Substitute.For<ProgressCallback>();
        var source = _imageImporter.Import(filePath, new ImportParams(), progressMock);

        progressMock.ReceivedCallsCount(0);
        await source.ToListAsync();
        progressMock.Received()(0, 1);
        progressMock.Received()(1, 1);
        progressMock.ReceivedCallsCount(2);
    }

    // TODO: Why is this flaking on Linux?
    [Fact(Skip = "Flaky")]
    public async Task MultiFrameProgress()
    {
        var filePath = CopyResourceToFile(ImageResources.animals_tiff, "image.tiff");

        var progressMock = Substitute.For<ProgressCallback>();
        var source = _imageImporter.Import(filePath, new ImportParams(), progressMock);
        var enumerator = source.GetAsyncEnumerator();

        progressMock.ReceivedCallsCount(0);
        Assert.True(await enumerator.MoveNextAsync());
        progressMock.Received()(0, 3);
        progressMock.Received()(1, 3);
        progressMock.ReceivedCallsCount(2);
        Assert.True(await enumerator.MoveNextAsync());
        progressMock.Received()(2, 3);
        progressMock.ReceivedCallsCount(3);
        Assert.True(await enumerator.MoveNextAsync());
        progressMock.Received()(3, 3);
        progressMock.ReceivedCallsCount(4);
        Assert.False(await enumerator.MoveNextAsync());
        progressMock.ReceivedCallsCount(5);
    }

    // TODO: Why is this flaking (at least on Linux)?
    [Fact(Skip = "Flaky")]
    public async Task SingleFrameCancellation()
    {
        var filePath = CopyResourceToFile(ImageResources.dog, "image.jpg");

        var cts = new CancellationTokenSource();
        var source = _imageImporter.Import(filePath, new ImportParams(), cts.Token);
        var enumerator = source.GetAsyncEnumerator();

        cts.Cancel();
        Assert.False(await enumerator.MoveNextAsync());
    }

    // This test doesn't work on Mac (and flaky on Linux) as the full file is loaded first, making enumeration instant
    [Fact(Skip = "Flaky")]
    public async Task MultiFrameCancellation()
    {
        var filePath = CopyResourceToFile(ImageResources.animals_tiff, "image.tiff");

        var cts = new CancellationTokenSource();
        var source = _imageImporter.Import(filePath, new ImportParams(), cts.Token);
        var enumerator = source.GetAsyncEnumerator();

        Assert.True(await enumerator.MoveNextAsync());
        Assert.True(await enumerator.MoveNextAsync());
        cts.Cancel();
        Assert.False(await enumerator.MoveNextAsync());
    }

    private void AssertUsesRecoveryStorage(IImageStorage storage, string expectedFileName)
    {
        var fileStorage = Assert.IsType<ImageFileStorage>(storage);
        Assert.EndsWith(expectedFileName, fileStorage.FullPath);
        Assert.True(File.Exists(fileStorage.FullPath));
        Assert.Equal(Path.Combine(FolderPath, "recovery"), Path.GetDirectoryName(fileStorage.FullPath));
    }

    private void AssertRecoveryStorageCleanedUp(IImageStorage storage)
    {
        var fileStorage = Assert.IsType<ImageFileStorage>(storage);
        Assert.False(File.Exists(fileStorage.FullPath));
    }

    [Fact]
    public async Task ImportMissingFile()
    {
        var filePath = Path.Combine(FolderPath, "missing.png");
        var source = _imageImporter.Import(filePath, new ImportParams());

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(async () => await source.ToListAsync());
        Assert.Contains("Could not find", ex.Message);
    }

    [Fact]
    public async Task ImportInUseFile()
    {
        var filePath = CopyResourceToFile(ImageResources.dog, "image.png");
        using var stream = File.OpenWrite(filePath);
        var source = _imageImporter.Import(filePath, new ImportParams());

        var ex = await Assert.ThrowsAsync<IOException>(async () => await source.ToListAsync());
        Assert.Contains("being used by another process", ex.Message);
    }

    [Fact]
    public async Task ImportWithBarcodeDetection()
    {
        var filePath = CopyResourceToFile(ImageResources.patcht, "image.jpg");

        var importParams = new ImportParams
        {
            BarcodeDetectionOptions = new BarcodeDetectionOptions { DetectBarcodes = true }
        };
        var source = _imageImporter.Import(filePath, importParams);
        var result = await source.ToListAsync();

        Assert.Single(result);
        Assert.True(result[0].PostProcessingData.Barcode.IsDetectionAttempted);
        Assert.True(result[0].PostProcessingData.Barcode.IsDetected);
        Assert.True(result[0].PostProcessingData.Barcode.IsPatchT);
        Assert.Equal("PATCHT", result[0].PostProcessingData.Barcode.DetectedText);
    }
}