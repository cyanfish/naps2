using System.Xml.Linq;
using NAPS2.Escl.Client;
using Xunit;

namespace NAPS2.Escl.Tests;

public class CapabilitiesParserTests
{
    [Fact]
    public void TestBasicCaps()
    {
        string input =
            """
            <?xml version="1.0" encoding="UTF8"?>
            <scan:ScannerCapabilities xmlns:scan="http://schemas.hp.com/imaging/escl/2011/05/03"
                                      xmlns:pwg="http://www.pwg.org/schemas/2010/12/sm">
                <pwg:Version>2.6</pwg:Version>
                <pwg:MakeAndModel>Hewlett Packard Photosmart C4760</pwg:MakeAndModel>
                <pwg:SerialNumber>CN01 7971874378PJ</pwg:SerialNumber>
                <scan:UUID>96a4b400 2a9e 012f 6165 0025559efbc6f</scan:UUID>
                <scan:AdminURI>http://192.168.1.2/index.html</scan:AdminURI>
                <scan:IconURI>http://192.168.1.2/scanner.png</scan:IconURI>
                <scan:SettingProfiles>
                    <scan:SettingProfile name="p1">
                        <scan:ColorModes>
                            <scan:ColorMode>BlackAndWhite1</scan:ColorMode>
                            <scan:ColorMode>Grayscale8</scan:ColorMode>
                        </scan:ColorModes>
                        <scan:DocumentFormats>
                            <pwg:DocumentFormat>application/pdf</pwg:DocumentFormat>
                            <pwg:DocumentFormat>image/jpeg</pwg:DocumentFormat>
                            <scan:DocumentFormatExt>application/pdf</scan:DocumentFormatExt>
                            <scan:DocumentFormatExt>image/jpeg</scan:DocumentFormatExt>
                        </scan:DocumentFormats>
                        <scan:SupportedResolutions>
                            <scan:DiscreteResolutions>
                                <scan:DiscreteResolution>
                                    <scan:XResolution>100</scan:XResolution>
                                    <scan:YResolution>100</scan:YResolution>
                                </scan:DiscreteResolution>
                                <scan:DiscreteResolution>
                                    <scan:XResolution>200</scan:XResolution>
                                    <scan:YResolution>200</scan:YResolution>
                                </scan:DiscreteResolution>
                                <scan:DiscreteResolution>
                                    <scan:XResolution>300</scan:XResolution>
                                    <scan:YResolution>300</scan:YResolution>
                                </scan:DiscreteResolution>
                            </scan:DiscreteResolutions>
                        </scan:SupportedResolutions>
                    </scan:SettingProfile>
                </scan:SettingProfiles>
                <scan:Platen>
                    <scan:PlatenInputCaps>
                        <scan:MinWidth>1</scan:MinWidth>
                        <scan:MaxWidth>3000</scan:MaxWidth>
                        <scan:MinHeight>1</scan:MinHeight>
                        <scan:MaxHeight>3600</scan:MaxHeight>
                        <scan:MaxScanRegions>2</scan:MaxScanRegions>
                        <scan:SettingProfiles>
                            <scan:SettingProfile>
                                <scan:ColorModes>
                                    <scan:ColorMode>BlackAndWhite1</scan:ColorMode>
                                    <scan:ColorMode>Grayscale8</scan:ColorMode>
                                </scan:ColorModes>
                                <scan:DocumentFormats>
                                    <pwg:DocumentFormat>application/pdf</pwg:DocumentFormat>
                                    <pwg:DocumentFormat>image/jpeg</pwg:DocumentFormat>
                                    <scan:DocumentFormatExt>application/pdf</scan:DocumentFormatExt>
                                    <scan:DocumentFormatExt>image/jpeg</scan:DocumentFormatExt>
                                </scan:DocumentFormats>
                                <scan:SupportedResolutions>
                                    <scan:ResolutionRange>
                                        <scan:XResolutionRange>
                                            <scan:Min>75</scan:Min>
                                            <scan:Max>1200</scan:Max>
                                            <scan:Normal>300</scan:Normal>
                                            <scan:Step>10</scan:Step>
                                        </scan:XResolutionRange>
                                        <scan:YResolutionRange>
                                            <scan:Min>75</scan:Min>
                                            <scan:Max>1200</scan:Max>
                                            <scan:Normal>300</scan:Normal>
                                            <scan:Step>10</scan:Step>
                                        </scan:YResolutionRange>
                                    </scan:ResolutionRange>
                                </scan:SupportedResolutions>
                            </scan:SettingProfile>
                        </scan:SettingProfiles>
                    </scan:PlatenInputCaps>
                </scan:Platen>
                <scan:Adf>
                    <scan:AdfSimplexInputCaps>
                        <scan:MinWidth>1</scan:MinWidth>
                        <scan:MaxWidth>2600</scan:MaxWidth>
                        <scan:MinHeight>1</scan:MinHeight>
                        <scan:MaxHeight>3400</scan:MaxHeight>
                        <scan:SettingProfiles>
                            <scan:SettingProfile ref="p1"/>
                        </scan:SettingProfiles>
                    </scan:AdfSimplexInputCaps>
                    <scan:AdfOptions>
                        <scan:AdfOption>DetectPaperLoaded</scan:AdfOption>
                        <scan:AdfOption>SelectSinglePage</scan:AdfOption>
                    </scan:AdfOptions>
                </scan:Adf>
            </scan:ScannerCapabilities>
            """;
        var caps = CapabilitiesParser.Parse(XDocument.Parse(input));
        Assert.Equal("2.6", caps.Version);
        Assert.Equal("Hewlett Packard Photosmart C4760", caps.MakeAndModel);
        Assert.Equal("CN01 7971874378PJ", caps.SerialNumber);
        Assert.Equal("96a4b400 2a9e 012f 6165 0025559efbc6f", caps.Uuid);
        Assert.Equal("http://192.168.1.2/index.html", caps.AdminUri);
        Assert.Equal("http://192.168.1.2/scanner.png", caps.IconUri);
    }

    [Fact]
    public void TestWithDifferentSchemaVersion()
    {
        string input =
            """
            <?xml version="1.0" encoding="UTF8"?>
            <scan:ScannerCapabilities xmlns:scan="http://schemas.hp.com/imaging/escl/2011/02/08"
                                      xmlns:pwg="http://www.pwg.org/schemas/2010/12/sm">
                <pwg:MakeAndModel>Hewlett Packard Photosmart C4760</pwg:MakeAndModel>
            </scan:ScannerCapabilities>
            """;
        var caps = CapabilitiesParser.Parse(XDocument.Parse(input));
        Assert.Equal("Hewlett Packard Photosmart C4760", caps.MakeAndModel);
    }
}
