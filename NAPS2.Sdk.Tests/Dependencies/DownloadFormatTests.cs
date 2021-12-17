using NAPS2.Dependencies;
using Xunit;

namespace NAPS2.Sdk.Tests.Dependencies;

public class DownloadFormatTests : ContextualTexts
{
    [Fact]
    public void Gzip()
    {
        var path = Path.Combine(FolderPath, "f.gz");
        var gzData = DownloadFormatTestsData.stock_dog_jpeg;
        File.WriteAllBytes(path, gzData);

        var extractedPath = DownloadFormat.Gzip.Prepare(path);

        var expectedDog = DownloadFormatTestsData.stock_dog;
        Assert.Equal(expectedDog, File.ReadAllBytes(extractedPath));
    }

    [Fact]
    public void Zip()
    {
        var path = Path.Combine(FolderPath, "f.zip");
        var zipData = DownloadFormatTestsData.animals;
        File.WriteAllBytes(path, zipData);

        var extractedPath = DownloadFormat.Zip.Prepare(path);

        var dogPath = Path.Combine(extractedPath, "animals/dogs/stock-dog.jpeg");
        var catPath = Path.Combine(extractedPath, "animals/cats/stock-cat.jpeg");
        Assert.True(File.Exists(dogPath));
        Assert.True(File.Exists(catPath));
        var expectedDog = DownloadFormatTestsData.stock_dog;
        var expectedCat = DownloadFormatTestsData.stock_cat;
        Assert.Equal(expectedDog, File.ReadAllBytes(dogPath));
        Assert.Equal(expectedCat, File.ReadAllBytes(catPath));
    }
}