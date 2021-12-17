using NAPS2.Scan;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Sdk.Tests.Serialization;

public class ScanOptionsSerializationTests
{
    [Fact]
    public void ScanOptions()
    {
        // TODO: More stuff
        var original = new ScanOptions
        {
            PageSize = PageSize.Letter
        };
        var serializer = new XmlSerializer<ScanOptions>();
        var doc = serializer.SerializeToXDocument(original);

        var copy = serializer.DeserializeFromXDocument(doc);
        Assert.Equal(8.5m, copy.PageSize.Width);
        Assert.Equal(11m, copy.PageSize.Height);
        Assert.Equal(PageSizeUnit.Inch, copy.PageSize.Unit);
    }
}