using NAPS2.Scan;
using NAPS2.Scan.Internal.Sane;
using NAPS2.Scan.Internal.Sane.Native;
using NSubstitute;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class SaneScanDriverOptionTests : ContextualTests
{
    private const string LIBRARY_VERSION = "1.2.3";

    private readonly SaneScanDriver _driver;
    public SaneScanDriverOptionTests()
    {
        _driver = new SaneScanDriver(ScanningContext);
    }

    // TODO: More tests

    [Theory]
    [InlineData(true, PaperSource.Feeder, BitDepth.Color, true)]
    [InlineData(false, PaperSource.Feeder, BitDepth.Color, false)]
    [InlineData(true, PaperSource.Flatbed, BitDepth.Color, false)]
    [InlineData(true, PaperSource.Duplex, BitDepth.Color, false)]
    [InlineData(true, PaperSource.Feeder, BitDepth.Grayscale, false)]
    [InlineData(true, PaperSource.Feeder, BitDepth.BlackAndWhite, false)]
    public void SetOptions_EpsondsAutoLength(bool autoCrop, PaperSource source,
        BitDepth bitDepth, bool expected)
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateBooleanForTesting(1, SaneOptionNames.EPSONDS_ES60W_AUTO_LENGTH)
        ], new() { [1] = !expected });

        _driver.SetOptions(device, new ScanOptions
        {
            AutoCrop = autoCrop, PaperSource = source, BitDepth = bitDepth
        });

        Assert.Equal(expected, device.GetValue(1));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void SetOptions_EpsondsAutoLength_AutoSource(bool hasFlatbed, bool expected)
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE,
                hasFlatbed ? ["Flatbed", "ADF"] : ["ADF"]),
            SaneOption.CreateBooleanForTesting(2, SaneOptionNames.EPSONDS_ES60W_AUTO_LENGTH)
        ], new() { [2] = false });

        _driver.SetOptions(device, new ScanOptions
        {
            AutoCrop = true, PaperSource = PaperSource.Auto, BitDepth = BitDepth.Color
        });

        Assert.Equal(expected, device.GetValue(2));
    }

    [Fact]
    public void SetOptions_EpsondsAutoLength_Absent()
    {
        var device = new DeviceOptionsMock([]);
        _driver.SetOptions(device, new ScanOptions
        {
            AutoCrop = true, PaperSource = PaperSource.Feeder, BitDepth = BitDepth.Color
        });
    }

    [Fact]
    public void SetOptions_EpsondsAutoLength_Inactive()
    {
        var option = new SaneOption(new SaneOptionDescriptor
        {
            Name = SaneOptionNames.EPSONDS_ES60W_AUTO_LENGTH,
            Type = SaneValueType.Bool,
            Capabilities = SaneCapabilities.SoftSelect | SaneCapabilities.Inactive
        }, 1);
        var device = new DeviceOptionsMock([option], new() { [1] = false });

        _driver.SetOptions(device, new ScanOptions
        {
            AutoCrop = true, PaperSource = PaperSource.Feeder, BitDepth = BitDepth.Color
        });

        Assert.Equal(false, device.GetValue(1));
    }

    [Fact]
    public void SetOptions_EpsondsAutoLength_WrongType()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.EPSONDS_ES60W_AUTO_LENGTH, ["off", "on"])
        ], new() { [1] = "off" });

        _driver.SetOptions(device, new ScanOptions
        {
            AutoCrop = true, PaperSource = PaperSource.Feeder, BitDepth = BitDepth.Color
        });

        Assert.Equal("off", device.GetValue(1));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetOptions_EpsondsAutoLength_ExplicitOverride(bool enabled)
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateBooleanForTesting(1, SaneOptionNames.EPSONDS_ES60W_AUTO_LENGTH)
        ]);

        _driver.SetOptions(device, new ScanOptions
        {
            AutoCrop = !enabled, PaperSource = PaperSource.Feeder, BitDepth = BitDepth.Color,
            KeyValueOptions = { [SaneOptionNames.EPSONDS_ES60W_AUTO_LENGTH] = enabled.ToString() }
        });

        Assert.Equal(enabled, device.GetValue(1));
    }

    [Fact]
    public void SetOptions_EpsondsAutoLength_BackendRejectsOption()
    {
        var option = SaneOption.CreateBooleanForTesting(1, SaneOptionNames.EPSONDS_ES60W_AUTO_LENGTH);
        var device = Substitute.For<ISaneDevice>();
        device.GetOptions().Returns([option]);
        device.When(x => x.SetOption(option, Arg.Any<bool>(), out Arg.Any<SaneOptionSetInfo>()))
            .Do(_ => throw new NotSupportedException());

        _driver.SetOptions(device, new ScanOptions
        {
            AutoCrop = true, PaperSource = PaperSource.Feeder, BitDepth = BitDepth.Color
        });

        device.Received(1).SetOption(option, true, out Arg.Any<SaneOptionSetInfo>());
    }

    [Fact]
    public void SetOptions_EpsondsAutoLength_ReusedDevice()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateBooleanForTesting(1, SaneOptionNames.EPSONDS_ES60W_AUTO_LENGTH)
        ]);
        var options = new ScanOptions
        {
            AutoCrop = true, PaperSource = PaperSource.Feeder, BitDepth = BitDepth.Color
        };

        _driver.SetOptions(device, options);
        Assert.Equal(true, device.GetValue(1));

        options.AutoCrop = false;
        _driver.SetOptions(device, options);
        Assert.Equal(false, device.GetValue(1));

        options.AutoCrop = true;
        _driver.SetOptions(device, options);
        Assert.Equal(true, device.GetValue(1));

        options.BitDepth = BitDepth.Grayscale;
        _driver.SetOptions(device, options);
        Assert.Equal(false, device.GetValue(1));
    }

    [Fact]
    public void SetOptions_EpsondsAutoLength_ReadOnly()
    {
        var option = new SaneOption(new SaneOptionDescriptor
        {
            Name = SaneOptionNames.EPSONDS_ES60W_AUTO_LENGTH,
            Type = SaneValueType.Bool,
            Capabilities = SaneCapabilities.SoftDetect
        }, 1);
        var device = Substitute.For<ISaneDevice>();
        device.GetOptions().Returns([option]);

        _driver.SetOptions(device, new ScanOptions
        {
            AutoCrop = true, PaperSource = PaperSource.Feeder, BitDepth = BitDepth.Color
        });

        device.DidNotReceive().SetOption(option, Arg.Any<bool>(), out Arg.Any<SaneOptionSetInfo>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SetOptions_EpsondsAutoLength_AfterGeometryReload(bool explicitOverride)
    {
        var geometry = new[]
        {
            SaneOption.CreateFixedForTesting(1, SaneOptionNames.TOP_LEFT_X,
                new SaneRange { Min = 0, Max = 216 }),
            SaneOption.CreateFixedForTesting(2, SaneOptionNames.TOP_LEFT_Y,
                new SaneRange { Min = 0, Max = 356 }),
            SaneOption.CreateFixedForTesting(3, SaneOptionNames.BOT_RIGHT_X,
                new SaneRange { Min = 0, Max = 216 }),
            SaneOption.CreateFixedForTesting(4, SaneOptionNames.BOT_RIGHT_Y,
                new SaneRange { Min = 0, Max = 356 })
        };
        var length = SaneOption.CreateBooleanForTesting(5, SaneOptionNames.EPSONDS_ES60W_AUTO_LENGTH);
        var geometrySet = false;
        var writes = new List<bool>();
        var device = Substitute.For<ISaneDevice>();
        device.GetOptions().Returns(_ => geometrySet ? geometry.Append(length) : geometry);
        device.When(x => x.SetOption(geometry[3], Arg.Any<double>(), out Arg.Any<SaneOptionSetInfo>()))
            .Do(call =>
            {
                geometrySet = true;
                call[2] = SaneOptionSetInfo.ReloadOptions;
            });
        device.When(x => x.SetOption(length, Arg.Any<bool>(), out Arg.Any<SaneOptionSetInfo>()))
            .Do(call =>
            {
                Assert.True(geometrySet);
                writes.Add(call.ArgAt<bool>(1));
            });
        var options = new ScanOptions
        {
            AutoCrop = true, PaperSource = PaperSource.Feeder, BitDepth = BitDepth.Color,
            PageSize = PageSize.A4
        };
        if (explicitOverride)
        {
            options.KeyValueOptions[SaneOptionNames.EPSONDS_ES60W_AUTO_LENGTH] = "false";
        }

        _driver.SetOptions(device, options);

        Assert.True(geometrySet);
        Assert.Equal(explicitOverride ? new[] { true, false } : new[] { true }, writes);
    }

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

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    public void SetOptions_BoolKeyValueOption(string value, bool expected)
    {
        var device = BoolOptionDeviceMock();
        var options = new ScanOptions
        {
            PaperSource = PaperSource.Feeder,
            PageSize = PageSize.A4,
            KeyValueOptions = { ["adf-crp"] = value }
        };

        _driver.SetOptions(device, options);

        // Note this must be applied after the scan area is set, as some backends (e.g. epsonds)
        // reset auto-crop when br-x/br-y is written.
        Assert.Equal(expected, device.GetValue(6));
    }

    [Theory]
    [InlineData("maybe")]
    [InlineData("yes")]
    [InlineData("1")]
    public void SetOptions_BoolKeyValueOption_IgnoresUnparseableValue(string value)
    {
        var device = BoolOptionDeviceMock();
        var options = new ScanOptions
        {
            PaperSource = PaperSource.Feeder,
            PageSize = PageSize.A4,
            KeyValueOptions = { ["adf-crp"] = value }
        };

        _driver.SetOptions(device, options);

        Assert.Throws<KeyNotFoundException>(() => device.GetValue(6));
    }

    private static DeviceOptionsMock BoolOptionDeviceMock() => new([
        SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE, ["Flatbed", "ADF"]),
        SaneOption.CreateFixedForTesting(2, SaneOptionNames.TOP_LEFT_X,
            new SaneRange { Min = 0, Max = 100, Quant = 1 }),
        SaneOption.CreateFixedForTesting(3, SaneOptionNames.TOP_LEFT_Y,
            new SaneRange { Min = 0, Max = 100, Quant = 1 }),
        SaneOption.CreateFixedForTesting(4, SaneOptionNames.BOT_RIGHT_X,
            new SaneRange { Min = 0, Max = 100, Quant = 1 }),
        SaneOption.CreateFixedForTesting(5, SaneOptionNames.BOT_RIGHT_Y,
            new SaneRange { Min = 0, Max = 100, Quant = 1 }),
        SaneOption.CreateBooleanForTesting(6, "adf-crp")
    ]);

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

        var caps = _driver.GetSaneCaps(device, "pixma", LIBRARY_VERSION);

        Assert.Equal("pixma", caps.MetadataCaps?.DriverSubtype);
        Assert.Equal(LIBRARY_VERSION, caps.MetadataCaps?.ProtocolVersion);
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
    public void GetSaneCaps_LibraryVersion()
    {
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE, ["Flatbed"])
        ]);

        var caps = _driver.GetSaneCaps(device, "", LIBRARY_VERSION);

        Assert.Equal(LIBRARY_VERSION, caps.MetadataCaps?.ProtocolVersion);
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
    public void SetOptions_FeederFrontSource()
    {
        // Settings from Epson WF-3520 with epsonscan2 backend
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE,
                ["Auto", "Flatbed", "ADF", "ADF Front"]),
            SaneOption.CreateBooleanForTesting(2, SaneOptionNames.DUPLEX)
        ]);
        var options = new ScanOptions { PaperSource = PaperSource.Feeder };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("ADF Front", device.GetValue(1));
        Assert.Throws<KeyNotFoundException>(() => device.GetValue(2));
        VerifyCapPaperSources(device, true, true, true);
    }

    [Fact]
    public void SetOptions_FeederWithStringDuplex()
    {
        // Settings from Kodak Alaris S2050 with kds_s2000 backend
        // https://github.com/cyanfish/naps2/issues/904
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE,
                ["ADF", "ADF Front", "ADF Back", "ADF Both", "ADF Duplex"]),
            SaneOption.CreateStringListForTesting(2, SaneOptionNames.DUPLEX, ["front", "back", "both"])
        ]);
        var options = new ScanOptions { PaperSource = PaperSource.Feeder };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("ADF Front", device.GetValue(1));
        Assert.Equal("front", device.GetValue(2));
        VerifyCapPaperSources(device, false, true, true);
    }

    [Fact]
    public void SetOptions_DuplexWithStringDuplex()
    {
        // No duplex source, so the duplex option is the only way to scan both sides
        var device = new DeviceOptionsMock([
            SaneOption.CreateStringListForTesting(1, SaneOptionNames.SOURCE, ["Flatbed", "ADF"]),
            SaneOption.CreateStringListForTesting(2, SaneOptionNames.DUPLEX, ["front", "back", "both"])
        ]);
        var options = new ScanOptions { PaperSource = PaperSource.Duplex };

        var optionData = _driver.SetOptions(device, options);

        Assert.True(optionData.IsFeeder);
        Assert.Equal("ADF", device.GetValue(1));
        Assert.Equal("both", device.GetValue(2));
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
        var caps = _driver.GetSaneCaps(device, "", LIBRARY_VERSION);
        Assert.NotNull(caps.PaperSourceCaps);
        Assert.Equal(flatbed, caps.PaperSourceCaps.SupportsFlatbed);
        Assert.Equal(feeder, caps.PaperSourceCaps.SupportsFeeder);
        Assert.Equal(duplex, caps.PaperSourceCaps.SupportsDuplex);
    }
}