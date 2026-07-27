using NAPS2.Images;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.Sdk.Tests;
using NAPS2.Util;
using NSubstitute;
using Xunit;

namespace NAPS2.Lib.Tests.Images;

public class SaveImagesOperationTests : ContextualTests
{
    [Fact]
    public async Task SaveWithResolutionScale50Percent()
    {
        var overwritePrompt = Substitute.For<IOverwritePrompt>();
        var operation = new SaveImagesOperation(overwritePrompt, ImageContext);

        var original = CreateScannedImage();
        var originalImage = original.Render();
        int originalWidth = originalImage.Width;
        int originalHeight = originalImage.Height;
        originalImage.Dispose();

        var tempFile = Path.Combine(FolderPath, "scaled.jpg");
        var imageSettings = new ImageSettings
        {
            ResolutionScale = 50,
            JpegQuality = 75
        };

        var started = operation.Start(tempFile, Placeholders.None, new[] { original }, imageSettings);
        Assert.True(started);
        
        bool success = await operation.Success;
        Assert.True(success);
        Assert.True(File.Exists(tempFile));

        using var savedImage = ImageContext.Load(tempFile);
        Assert.Equal((int)Math.Round(originalWidth * 0.5), savedImage.Width);
        Assert.Equal((int)Math.Round(originalHeight * 0.5), savedImage.Height);
    }

    [Fact]
    public async Task SaveWithResolutionScale100Percent()
    {
        var overwritePrompt = Substitute.For<IOverwritePrompt>();
        var operation = new SaveImagesOperation(overwritePrompt, ImageContext);

        var original = CreateScannedImage();
        var originalImage = original.Render();
        int originalWidth = originalImage.Width;
        int originalHeight = originalImage.Height;
        originalImage.Dispose();

        var tempFile = Path.Combine(FolderPath, "unscaled.jpg");
        var imageSettings = new ImageSettings
        {
            ResolutionScale = 100,
            JpegQuality = 75
        };

        var started = operation.Start(tempFile, Placeholders.None, new[] { original }, imageSettings);
        Assert.True(started);

        bool success = await operation.Success;
        Assert.True(success);
        Assert.True(File.Exists(tempFile));

        using var savedImage = ImageContext.Load(tempFile);
        Assert.Equal(originalWidth, savedImage.Width);
        Assert.Equal(originalHeight, savedImage.Height);
    }
}
