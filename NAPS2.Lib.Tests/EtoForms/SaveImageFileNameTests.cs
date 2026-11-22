using NAPS2.EtoForms;
using Xunit;

namespace NAPS2.Lib.Tests.EtoForms;

public class SaveImageFileNameTests
{
    [Theory]
    [InlineData("scan", new[] { ".jpg", ".jpeg" }, "scan.jpg")]
    [InlineData("scan", new[] { ".jpeg", ".jpg" }, "scan.jpeg")]
    [InlineData("scan", new[] { ".png" }, "scan.png")]
    [InlineData("scan", new[] { ".tiff", ".tif" }, "scan.tiff")]
    [InlineData("scan", new[] { ".bmp" }, "scan.bmp")]
    [InlineData("scan", new[] { ".jp2", ".jpx" }, "scan.jp2")]
    [InlineData("scan", new[] { ".JPG", ".jpeg" }, "scan.JPG")]
    [InlineData("scan.jpg", new[] { ".jpg", ".jpeg" }, "scan.jpg")]
    [InlineData("scan.JPEG", new[] { ".jpg", ".jpeg" }, "scan.JPEG")]
    [InlineData("scan.png", new[] { ".jpg", ".jpeg" }, "scan.png")]
    [InlineData("scan.png", new[] { ".jpg", ".png" }, "scan.png")]
    [InlineData("scan", new[] { ".jpg", ".png" }, null)]
    [InlineData("scan", new[] { ".*" }, null)]
    [InlineData("scan", new[] { ".unknown" }, null)]
    [InlineData("scan", new string[] { }, null)]
    [InlineData("folder.with.dots/scan", new[] { ".jpg", ".jpeg" }, "folder.with.dots/scan.jpg")]
    public void CompletesOnlyUnambiguousExtensionlessNames(string fileName, string[] extensions, string expected)
    {
        Assert.Equal(expected, EtoDialogHelper.CompleteImageFileName(fileName, extensions));
    }
}
