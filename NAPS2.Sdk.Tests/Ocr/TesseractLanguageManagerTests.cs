using NAPS2.Ocr;
using Xunit;

namespace NAPS2.Sdk.Tests.Ocr;

public class TesseractLanguageManagerTests : ContextualTests
{
    private readonly string _legacyBasePath;
    private readonly string _newBasePath;

    public TesseractLanguageManagerTests()
    {
        _legacyBasePath = Path.Combine(FolderPath, "tesseract-4.0.0b4");
        _newBasePath = Path.Combine(FolderPath, "tesseract4");
    }

    [Fact]
    public void UsesNewBasePathOnCleanInstall()
    {
        var manager = new TesseractLanguageManager(FolderPath);

        Assert.Equal(_newBasePath, manager.TessdataBasePath);
    }

    [Fact]
    public void MovesToNewBasePath()
    {
        Directory.CreateDirectory(_legacyBasePath);

        var manager = new TesseractLanguageManager(FolderPath);

        Assert.False(Directory.Exists(_legacyBasePath));
        Assert.True(Directory.Exists(_newBasePath));
        Assert.Equal(_newBasePath, manager.TessdataBasePath);
    }

    [Fact]
    public void UsesNewBasePathWhenBothPresent()
    {
        Directory.CreateDirectory(_legacyBasePath);
        Directory.CreateDirectory(_newBasePath);

        var manager = new TesseractLanguageManager(FolderPath);

        Assert.True(Directory.Exists(_legacyBasePath));
        Assert.True(Directory.Exists(_newBasePath));
        Assert.Equal(_newBasePath, manager.TessdataBasePath);
    }

    // Locking only stops directory moves on Windows, there isn't an easy way to test this on Mac/Linux
    // (and it's not really relevant for Mac/Linux anyway)
    [PlatformFact(include: PlatformFlags.Windows)]
    public void KeepsLegacyBasePathOnMoveError()
    {
        Directory.CreateDirectory(_legacyBasePath);
        using var lockedFile = File.OpenWrite(Path.Combine(_legacyBasePath, "blah.txt"));

        var manager = new TesseractLanguageManager(FolderPath);

        Assert.True(Directory.Exists(_legacyBasePath));
        Assert.False(Directory.Exists(_newBasePath));
        Assert.Equal(_legacyBasePath, manager.TessdataBasePath);
    }
}