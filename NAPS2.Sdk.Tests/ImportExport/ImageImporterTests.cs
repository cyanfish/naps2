using System.Threading;
using Moq;
using NAPS2.Images.Gdi;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport;

public class ImageImporterTests : ContextualTexts
{
    private readonly ImageImporter _imageImporter;

    public ImageImporterTests()
    {
        _imageImporter = new ImageImporter(ScanningContext, ImageContext, new ImportPostProcessor(ImageContext));
        ScanningContext.FileStorageManager = FileStorageManager.CreateFolder(Path.Combine(FolderPath, "recovery"));
    }
    
    [Fact]
    public async Task ImportPngImage()
    {
        var filePath = Path.Combine(FolderPath, "image.png");
        ImageImporterTestsData.skewed_bw.Save(filePath);
        
        var source = _imageImporter.Import(filePath, new ImportParams(), (current, max) => { }, CancellationToken.None);
        var result = await source.ToList();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.True(File.Exists(storage.FullPath));
        Assert.Equal(Path.Combine(FolderPath, "recovery"), Path.GetDirectoryName(storage.FullPath));
        Assert.True(result[0].Metadata.Lossless);
        Assert.Equal(BitDepth.Color, result[0].Metadata.BitDepth);
        Assert.Null(result[0].PostProcessingData.Thumbnail);
        Assert.False(result[0].PostProcessingData.BarcodeDetection.IsAttempted);
        Assert.True(result[0].TransformState.IsEmpty);
        
        result[0].Dispose();
        Assert.False(File.Exists(storage.FullPath));
    }
    
    [Fact]
    public async Task ImportJpegImage()
    {
        var filePath = Path.Combine(FolderPath, "image.jpg");
        ImageImporterTestsData.color_image.Save(filePath);
        
        var source = _imageImporter.Import(filePath, new ImportParams(), (current, max) => { }, CancellationToken.None);
        var result = await source.ToList();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.True(File.Exists(storage.FullPath));
        Assert.Equal(Path.Combine(FolderPath, "recovery"), Path.GetDirectoryName(storage.FullPath));
        Assert.False(result[0].Metadata.Lossless);
        Assert.Equal(BitDepth.Color, result[0].Metadata.BitDepth);
        Assert.Null(result[0].PostProcessingData.Thumbnail);
        Assert.False(result[0].PostProcessingData.BarcodeDetection.IsAttempted);
        Assert.True(result[0].TransformState.IsEmpty);
        
        result[0].Dispose();
        Assert.False(File.Exists(storage.FullPath));
    }
    
    [Fact]
    public async Task ImportTiffImage()
    {
        var filePath = Path.Combine(FolderPath, "image.tiff");
        // We use a byte array for this resource instead of a bitmap so it's easier to save all 3 tiff frames
        File.WriteAllBytes(filePath, ImageImporterTestsData.color_image_set);
        
        var source = _imageImporter.Import(filePath, new ImportParams(), (current, max) => { }, CancellationToken.None);
        var result = await source.ToList();

        Assert.Equal(3, result.Count);
        AssertUsesRecoveryStorage(result[0].Storage, "00001.jpg");
        Assert.False(result[0].Metadata.Lossless);
        Assert.Equal(BitDepth.Color, result[0].Metadata.BitDepth);
        ImageAsserts.Similar(
            new GdiImage(ImageImporterTestsData.color_image),
            result[0].RenderToImage(),
            ImageAsserts.GENERAL_RMSE_THRESHOLD);
        
        AssertUsesRecoveryStorage(result[2].Storage, "00003.jpg");
        Assert.False(result[2].Metadata.Lossless);
        Assert.Equal(BitDepth.Color, result[2].Metadata.BitDepth);
        ImageAsserts.Similar(
            new GdiImage(ImageImporterTestsData.stock_cat),
            result[2].RenderToImage(),
            ImageAsserts.GENERAL_RMSE_THRESHOLD);
        
        result[0].Dispose();
        AssertRecoveryStorageCleanedUp(result[0].Storage);
        AssertUsesRecoveryStorage(result[2].Storage, "00003.jpg");
        result[2].Dispose();
        AssertRecoveryStorageCleanedUp(result[2].Storage);
    }
    
    [Fact]
    public async Task ImportWithThumbnailGeneration()
    {
        var filePath = Path.Combine(FolderPath, "image.jpg");
        ImageImporterTestsData.color_image.Save(filePath);
        
        var source = _imageImporter.Import(filePath, new ImportParams { ThumbnailSize = 256 }, (current, max) => { }, CancellationToken.None);
        var result = await source.ToList();

        Assert.Single(result);
        Assert.NotNull(result[0].PostProcessingData.Thumbnail);
        Assert.Equal(256, result[0].PostProcessingData.Thumbnail.Width);
    }
    
    [Fact]
    public async Task SingleFrameProgress()
    {
        var filePath = Path.Combine(FolderPath, "image.jpg");
        ImageImporterTestsData.color_image.Save(filePath);

        var progressMock = new Mock<ProgressHandler>();
        
        var source = _imageImporter.Import(filePath, new ImportParams(), progressMock.Object, CancellationToken.None);
        
        progressMock.VerifyNoOtherCalls();
        await source.ToList();
        progressMock.Verify(x => x(0, 1));
        progressMock.Verify(x => x(1, 1));
        progressMock.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task MultiFrameProgress()
    {
        var filePath = Path.Combine(FolderPath, "image.tiff");
        File.WriteAllBytes(filePath, ImageImporterTestsData.color_image_set);

        var progressMock = new Mock<ProgressHandler>();
        var source = _imageImporter.Import(filePath, new ImportParams(), progressMock.Object, CancellationToken.None);
        
        progressMock.VerifyNoOtherCalls();
        Assert.NotNull(await source.Next());
        progressMock.Verify(x => x(0, 3));
        progressMock.Verify(x => x(1, 3));
        progressMock.VerifyNoOtherCalls();
        Assert.NotNull(await source.Next());
        progressMock.Verify(x => x(2, 3));
        progressMock.VerifyNoOtherCalls();
        Assert.NotNull(await source.Next());
        progressMock.Verify(x => x(3, 3));
        progressMock.VerifyNoOtherCalls();
        Assert.Null(await source.Next());
        progressMock.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task SingleFrameCancellation()
    {
        var filePath = Path.Combine(FolderPath, "image.jpg");
        ImageImporterTestsData.color_image.Save(filePath);

        var cts = new CancellationTokenSource();
        var source = _imageImporter.Import(filePath, new ImportParams(), (current, max) => { }, cts.Token);
        
        cts.Cancel();
        Assert.Null(await source.Next());
    }
    
    [Fact]
    public async Task MultiFrameCancellation()
    {
        var filePath = Path.Combine(FolderPath, "image.tiff");
        File.WriteAllBytes(filePath, ImageImporterTestsData.color_image_set);

        var cts = new CancellationTokenSource();
        var source = _imageImporter.Import(filePath, new ImportParams(), (current, max) => { }, cts.Token);
        
        Assert.NotNull(await source.Next());
        Assert.NotNull(await source.Next());
        cts.Cancel();
        Assert.Null(await source.Next());
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
        var source = _imageImporter.Import(filePath, new ImportParams(), (current, max) => { }, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(async () => await source.ToList());
        Assert.Contains("Could not find image file", ex.Message);
    }

    [Fact]
    public async Task ImportInUseFile()
    {
        var filePath = Path.Combine(FolderPath, "image.png");
        ImageImporterTestsData.color_image.Save(filePath);
        using var stream = File.OpenWrite(filePath);
        var source = _imageImporter.Import(filePath, new ImportParams(), (current, max) => { }, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<IOException>(async () => await source.ToList());
        Assert.Contains("Error reading image file", ex.Message);
    }
    
    [Fact]
    public async Task ImportWithBarcodeDetection()
    {
        var filePath = Path.Combine(FolderPath, "image.jpg");
        ImageImporterTestsData.patcht.Save(filePath);

        var importParams = new ImportParams
        {
            BarcodeDetectionOptions = new BarcodeDetectionOptions { DetectBarcodes = true }
        };
        var source = _imageImporter.Import(filePath, importParams, (current, max) => { }, CancellationToken.None);
        var result = await source.ToList();

        Assert.Single(result);
        Assert.True(result[0].PostProcessingData.BarcodeDetection.IsAttempted);
        Assert.True(result[0].PostProcessingData.BarcodeDetection.IsBarcodePresent);
        Assert.True(result[0].PostProcessingData.BarcodeDetection.IsPatchT);
        Assert.Equal("PATCHT", result[0].PostProcessingData.BarcodeDetection.DetectionResult?.Text);
    }
}