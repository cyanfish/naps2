using NAPS2.Scan;
using NAPS2.Scan.Internal.Sane;
using NAPS2.Scan.Internal.Sane.Native;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class SaneScanDriverOptionTests : ContextualTests
{
    private readonly SaneScanDriver _driver;

    public SaneScanDriverOptionTests()
    {
        _driver = new SaneScanDriver(ScanningContext);
    }
    
    // TODO: More tests

    [Fact]
    public void SetOptions_Flatbed()
    {
        var device = new DeviceOptionsMock(new[]
        {
            SaneOption.CreateForTesting(1, SaneOptionNames.SOURCE, new[] { "Flatbed", "ADF", "Duplex" })
        });
        var options = new ScanOptions
        {
            PaperSource = PaperSource.Flatbed
        };

        var optionData = _driver.SetOptions(device, options);

        Assert.False(optionData.IsFeeder);
        Assert.Equal("Flatbed", device.GetValue(1));
    }

    [Fact]
    public void SetOptions_Feeder()
    {
        var device = new DeviceOptionsMock(new[]
        {
            SaneOption.CreateForTesting(1, SaneOptionNames.SOURCE, new[] { "Flatbed", "ADF", "Duplex" })
        });
        var options = new ScanOptions { PaperSource = PaperSource.Feeder };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("ADF", device.GetValue(1));
    }

    [Fact]
    public void SetOptions_FeederWithDuplexMatch()
    {
        var device = new DeviceOptionsMock(new[]
        {
            SaneOption.CreateForTesting(1, SaneOptionNames.SOURCE, new[] { "Flatbed", "ADF Duplex", "ADF" })
        });
        var options = new ScanOptions { PaperSource = PaperSource.Feeder };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("ADF", device.GetValue(1));
    }

    [Fact]
    public void SetOptions_Duplex()
    {
        var device = new DeviceOptionsMock(new[]
        {
            SaneOption.CreateForTesting(1, SaneOptionNames.SOURCE, new[] { "Flatbed", "ADF", "Duplex" })
        });
        var options = new ScanOptions { PaperSource = PaperSource.Duplex };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("Duplex", device.GetValue(1));
    }

    [Fact]
    public void SetOptions_AutoWithFlatbed()
    {
        var device = new DeviceOptionsMock(new[]
        {
            SaneOption.CreateForTesting(1, SaneOptionNames.SOURCE, new[] { "Flatbed", "ADF", "Duplex" })
        });
        var options = new ScanOptions { PaperSource = PaperSource.Auto };

        var optionData = _driver.SetOptions(device, options);

        Assert.False(optionData.IsFeeder);
        Assert.Equal("Flatbed", device.GetValue(1));
    }

    [Fact]
    public void SetOptions_AutoWithNoFlatbed()
    {
        var device = new DeviceOptionsMock(new[]
        {
            SaneOption.CreateForTesting(1, SaneOptionNames.SOURCE, new[] { "ADF", "Duplex" })
        });
        var options = new ScanOptions { PaperSource = PaperSource.Auto };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("ADF", device.GetValue(1));
    }

    [Fact]
    public void SetOptions_DuplexWithPartialMatch()
    {
        var device = new DeviceOptionsMock(new[]
        {
            SaneOption.CreateForTesting(1, SaneOptionNames.SOURCE,
                new[] { "Feeder(left aligned)", "Feeder(left aligned,Duplex)" })
        });
        var options = new ScanOptions { PaperSource = PaperSource.Duplex };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("Feeder(left aligned,Duplex)", device.GetValue(1));
    }

    [Fact]
    public void SetOptions_DuplicateOptions()
    {
        var device = new DeviceOptionsMock(new[]
        {
            SaneOption.CreateForTesting(1, SaneOptionNames.SOURCE, new[] { "Flatbed", "ADF", "Duplex" }),
            SaneOption.CreateForTesting(2, SaneOptionNames.SOURCE, new[] { "Flatbed", "ADF", "Duplex" })
        });
        var options = new ScanOptions
        {
            PaperSource = PaperSource.Flatbed
        };

        var optionData = _driver.SetOptions(device, options);

        Assert.False(optionData.IsFeeder);
        Assert.Equal("Flatbed", device.GetValue(1));
    }

    private class DeviceOptionsMock : ISaneDevice
    {
        private readonly IEnumerable<SaneOption> _options;
        private readonly Dictionary<int, object> _values;

        public DeviceOptionsMock(IEnumerable<SaneOption> options, Dictionary<int, object> defaultValues = null)
        {
            _options = options;
            _values = defaultValues ?? new();
        }

        public object GetValue(int index) => _values[index];

        public void Cancel() => throw new NotSupportedException();
        public void Start() => throw new NotSupportedException();
        public SaneReadParameters GetParameters() => throw new NotSupportedException();
        public bool Read(byte[] buffer, out int len) => throw new NotSupportedException();

        public IEnumerable<SaneOption> GetOptions() => _options;

        public void SetOption(SaneOption option, double value, out SaneOptionSetInfo info)
        {
            _values[option.Index] = value;
            info = SaneOptionSetInfo.None;
        }

        public void SetOption(SaneOption option, string value, out SaneOptionSetInfo info)
        {
            _values[option.Index] = value;
            info = SaneOptionSetInfo.None;
        }

        public void GetOption(SaneOption option, out double value)
        {
            value = (double) _values[option.Index];
        }
    }
}