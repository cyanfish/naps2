using System.Collections;

namespace NAPS2.Sdk.Tests;

public class StorageAwareTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        // TODO: Uncomment once working
        // yield return new object[] { new StorageConfig.Memory() };
        yield return new object[] { new StorageConfig.File() };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}