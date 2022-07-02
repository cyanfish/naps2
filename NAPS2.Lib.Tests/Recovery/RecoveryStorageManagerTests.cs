using System.Drawing;
using NAPS2.Images.Gdi;
using NAPS2.Recovery;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Recovery;

public class RecoveryStorageManagerTests : ContextualTexts
{
    private readonly string _recoveryFolder;
    private readonly UiImageList _imageList; // TODO: Add tests for this
    private readonly RecoveryStorageManager _recoveryStorageManager;

    public RecoveryStorageManagerTests()
    {
        _recoveryFolder = Path.Combine(FolderPath, "recovery");
        ScanningContext.FileStorageManager = new FileStorageManager(_recoveryFolder);
        _imageList = new UiImageList();
        _recoveryStorageManager = RecoveryStorageManager.CreateFolder(_recoveryFolder, _imageList);
    }

    public override void Dispose()
    {
        _recoveryStorageManager.Dispose();
        base.Dispose();
    }

    [Fact]
    public void RecoveryFolderStructure()
    {
        Assert.True(Directory.Exists(_recoveryFolder));
        Assert.True(File.Exists(Path.Combine(_recoveryFolder, ".lock")));
        Assert.False(File.Exists(Path.Combine(_recoveryFolder, "index.xml")));
    }
    
    [Fact]
    public void RecoveryFolderCleanup()
    {
        var image1 = new UiImage(CreateScannedImage());
        _recoveryStorageManager.WriteIndex(new[] {image1});

        _recoveryStorageManager.Dispose();
        Assert.False(Directory.Exists(_recoveryFolder));
    }

    [Fact]
    public void IndexFileDefaultMetadata()
    {
        var image1 = new UiImage(CreateScannedImage());
        var image2 = new UiImage(CreateScannedImage());
        _recoveryStorageManager.WriteIndex(new[] {image1, image2});
        var indexFileContent = File.ReadAllText(Path.Combine(_recoveryFolder, "index.xml"));

        Assert.Contains("00001.png</FileName>", indexFileContent);
        Assert.Contains("00002.png</FileName>", indexFileContent);
        Assert.Contains("<BitDepth>C24Bit</BitDepth>", indexFileContent);
        Assert.Contains("<HighQuality>false</HighQuality>", indexFileContent);
    }

    [Fact]
    public void IndexFileMultipleWrites()
    {
        var image1 = new UiImage(CreateScannedImage());
        var image2 = new UiImage(CreateScannedImage());

        _recoveryStorageManager.WriteIndex(new[] {image1, image2});
        var indexFileContent = File.ReadAllText(Path.Combine(_recoveryFolder, "index.xml"));
        Assert.Contains("00001.png", indexFileContent);
        Assert.Contains("00002.png", indexFileContent);

        _recoveryStorageManager.WriteIndex(new[] {image2});
        var indexFileContent2 = File.ReadAllText(Path.Combine(_recoveryFolder, "index.xml"));
        Assert.DoesNotContain("00001.png", indexFileContent2);
        Assert.Contains("00002.png", indexFileContent2);
    }

    [Fact]
    public void IndexFileCustomMetadata()
    {
        var image1 = new UiImage(
            ScanningContext.CreateProcessedImage(
                new GdiImage(new Bitmap(100, 100)),
                BitDepth.Grayscale,
                true,
                -1,
                Enumerable.Empty<Transform>()));

        _recoveryStorageManager.WriteIndex(new[] {image1});
        var indexFileContent = File.ReadAllText(Path.Combine(_recoveryFolder, "index.xml"));
        Assert.Contains("<BitDepth>Grayscale</BitDepth>", indexFileContent);
        Assert.Contains("<HighQuality>true</HighQuality>", indexFileContent);
    }

    [Fact]
    public void IndexFileWithTransform()
    {
        var image1 = new UiImage(
            ScanningContext.CreateProcessedImage(
                new GdiImage(new Bitmap(100, 100)),
                new[] {new BrightnessTransform(100)}));

        _recoveryStorageManager.WriteIndex(new[] {image1});
        var indexFileContent3 = File.ReadAllText(Path.Combine(_recoveryFolder, "index.xml"));
        Assert.Contains("<Transform xsi:type=\"BrightnessTransform\">", indexFileContent3);
        Assert.Contains("<Brightness>100</Brightness>", indexFileContent3);
    }
}