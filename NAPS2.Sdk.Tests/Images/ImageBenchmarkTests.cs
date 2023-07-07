using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class ImageBenchmarkTests : ContextualTests
{
    [BenchmarkFact]
    public void Save300Jpeg()
    {
        var filePath = Path.Combine(FolderPath, "test");
        var image = LoadImage(ImageResources.dog);

        for (int i = 0; i < 300; i++)
        {
            image.Save(filePath + i + ".jpg");
        }
    }

    [BenchmarkFact]
    public void SaveHugeJpeg()
    {
        var filePath = Path.Combine(FolderPath, "test");
        var image = LoadImage(ImageResources.dog_huge);

        image.Save(filePath + ".jpg");
    }

    public class BenchmarkFact : FactAttribute
    {
        public BenchmarkFact()
        {
            Skip = "comment out this line to run benchmarks";
        }
    }
}