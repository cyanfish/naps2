using System.Collections;
using NAPS2.Ocr;

namespace NAPS2.Sdk.Tests.ImportExport.Pdf;

public class OcrTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new OcrTestConfig(new StorageConfig.Memory(), null) };
        yield return new object[] { new OcrTestConfig(new StorageConfig.File(), null) };
        yield return new object[] { new OcrTestConfig(new StorageConfig.Memory(), new OcrParams("eng", OcrMode.Default, 10)) };
        yield return new object[] { new OcrTestConfig(new StorageConfig.File(), new OcrParams("eng", OcrMode.Default, 10)) };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}