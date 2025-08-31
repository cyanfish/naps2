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
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE, ["Flatbed", "ADF", "Duplex"])
        ]);
        var options = new ScanOptions
        {
            PaperSource = PaperSource.Flatbed
        };

        var optionData = _driver.SetOptions(device, options);

        Assert.False(optionData.IsFeeder);
        Assert.Equal("Flatbed", device.GetValue(1));
        VerifyCapPaperSources(device, true, true, true);
    }

    [Fact]
    public void GetSaneCaps()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateFixedForTesting(1, SaneOptionNames.TOP_LEFT_X,
                new SaneRange { Min = 0, Max = 100, Quant = 1 }),
            SaneOption.CreateFixedForTesting(2, SaneOptionNames.TOP_LEFT_Y,
                new SaneRange { Min = 0, Max = 100, Quant = 1 }),
            SaneOption.CreateFixedForTesting(3, SaneOptionNames.BOT_RIGHT_X,
                new SaneRange { Min = 0, Max = 100, Quant = 1 }),
            SaneOption.CreateFixedForTesting(4, SaneOptionNames.BOT_RIGHT_Y,
                new SaneRange { Min = 0, Max = 100, Quant = 1 }),
            SaneOption.CreateStringListForTesting(5, SaneOptionNames.SOURCE, ["Flatbed"]),
            SaneOption.CreateWordListForTesting(6, SaneOptionNames.RESOLUTION, [100, 200, 300]),
            SaneOption.CreateStringListForTesting(7, SaneOptionNames.MODE, ["Gray", "Color"])
        ]);

        var caps = _driver.GetSaneCaps(device, "pixma");

        Assert.Equal("pixma", caps.MetadataCaps?.DriverSubtype);
        VerifyCapPaperSources(device, true, false, false);
        var flatbedCaps = caps.FlatbedCaps;
        Assert.NotNull(flatbedCaps);
        Assert.Equal(100, flatbedCaps.PageSizeCaps?.ScanArea?.Width);
        Assert.Equal(100, flatbedCaps.PageSizeCaps?.ScanArea?.Height);
        Assert.Equal(PageSizeUnit.Millimetre, flatbedCaps.PageSizeCaps?.ScanArea?.Unit);
        Assert.Equal([100, 200, 300], flatbedCaps.DpiCaps?.Values);
        Assert.Equal(true, flatbedCaps.BitDepthCaps?.SupportsColor);
        Assert.Equal(true, flatbedCaps.BitDepthCaps?.SupportsGrayscale);
        Assert.Equal(false, flatbedCaps.BitDepthCaps?.SupportsBlackAndWhite);
    }

    [Fact]
    public void SetOptions_Feeder()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE, ["Flatbed", "ADF", "Duplex"])
        ]);
        var options = new ScanOptions { PaperSource = PaperSource.Feeder };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("ADF", device.GetValue(1));
        VerifyCapPaperSources(device, true, true, true);
    }

    [Fact]
    public void SetOptions_FeederWithDuplexMatch()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE, ["Flatbed", "ADF Duplex", "ADF"])
        ]);
        var options = new ScanOptions { PaperSource = PaperSource.Feeder };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("ADF", device.GetValue(1));
        VerifyCapPaperSources(device, true, true, true);
    }

    [Fact]
    public void SetOptions_Duplex()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE, ["Flatbed", "ADF", "Duplex"])
        ]);
        var options = new ScanOptions { PaperSource = PaperSource.Duplex };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("Duplex", device.GetValue(1));
        VerifyCapPaperSources(device, true, true, true);
    }

    [Fact]
    public void SetOptions_DuplexWithAdfMode()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE, ["Flatbed", "ADF"]),
            SaneOption.CreateStringListForTesting(2, SaneOptionNames.ADF_MODE1, ["Simplex", "Duplex"])
        ]);
        var options = new ScanOptions { PaperSource = PaperSource.Duplex };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("ADF", device.GetValue(1));
        Assert.Equal("Duplex", device.GetValue(2));
        VerifyCapPaperSources(device, true, true, true);
    }

    [Fact]
    public void SetOptions_AutoWithFlatbed()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE, ["Flatbed", "ADF", "Duplex"])
        ]);
        var options = new ScanOptions { PaperSource = PaperSource.Auto };

        var optionData = _driver.SetOptions(device, options);

        Assert.False(optionData.IsFeeder);
        Assert.Equal("Flatbed", device.GetValue(1));
        VerifyCapPaperSources(device, true, true, true);
    }

    [Fact]
    public void SetOptions_AutoWithNoFlatbed()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE, ["ADF", "Duplex"])
        ]);
        var options = new ScanOptions { PaperSource = PaperSource.Auto };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("ADF", device.GetValue(1));
        VerifyCapPaperSources(device, false, true, true);
    }

    [Fact]
    public void SetOptions_DuplexWithPartialMatch()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE,
                ["Feeder(left aligned)", "Feeder(left aligned,Duplex)"])
        ]);
        var options = new ScanOptions { PaperSource = PaperSource.Duplex };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("Feeder(left aligned,Duplex)", device.GetValue(1));
        VerifyCapPaperSources(device, false, true, true);
    }

    [Fact]
    public void SetOptions_DuplexBoolean()
    {
        // Settings from Epson WF-3520 with epsonscan2 backend
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE,
                ["Auto", "Flatbed", "ADF", "ADF Front"]),
            SaneOption.CreateBooleanForTesting(2, SaneOptionNames.DUPLEX)
        ]);
        var options = new ScanOptions { PaperSource = PaperSource.Duplex };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("ADF", device.GetValue(1));
        Assert.Equal(true, device.GetValue(2));
        VerifyCapPaperSources(device, true, true, true);
    }

    [Fact]
    public void SetOptions_DuplicateOptions()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE, ["Flatbed", "ADF", "Duplex"]),
            SaneOption.CreateStringListForTesting(2, SaneOptionNames.SOURCE, ["Flatbed", "ADF", "Duplex"])
        ]);
        var options = new ScanOptions
        {
            PaperSource = PaperSource.Flatbed
        };

        var optionData = _driver.SetOptions(device, options);

        Assert.False(optionData.IsFeeder);
        Assert.Equal("Flatbed", device.GetValue(1));
        VerifyCapPaperSources(device, true, true, true);
    }

    [Fact]
    public void SetOptions_NoGrayErrorDiffusion()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.MODE, ["Gray[Error Diffusion]", "True Gray"])
        ]);
        var options = new ScanOptions
        {
            BitDepth = BitDepth.Grayscale
        };

        var optionData = _driver.SetOptions(device, options);

        Assert.Equal("True Gray", optionData.Mode);
    }

    [Fact]
    public void SetOptions_KeyValue()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.MODE, ["Lineart", "Halftone", "Gray", "Color"])
        ]);
        var options = new ScanOptions
        {
            KeyValueOptions = { ["mode"] = "Halftone" }
        };

        _driver.SetOptions(device, options);

        Assert.Equal("Halftone", device.GetValue(1));
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

        public void SetOption(SaneOption option, bool value, out SaneOptionSetInfo info)
        {
            _values[option.Index] = value;
            info = SaneOptionSetInfo.None;
        }

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

    private void VerifyCapPaperSources(DeviceOptionsMock device, bool flatbed, bool feeder, bool duplex)
    {
        var caps = _driver.GetSaneCaps(device, "");
        Assert.NotNull(caps.PaperSourceCaps);
        Assert.Equal(flatbed, caps.PaperSourceCaps.SupportsFlatbed);
        Assert.Equal(feeder, caps.PaperSourceCaps.SupportsFeeder);
        Assert.Equal(duplex, caps.PaperSourceCaps.SupportsDuplex);
    }
}