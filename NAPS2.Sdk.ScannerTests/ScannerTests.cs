using NAPS2.Scan;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Sdk.ScannerTests;

public class ScannerTests
{
    // TODO: Make real tests for WIA/TWAIN, Flatbed/Feeder, Color/Gray/BW, DPIs, etc.

    [ScannerFact]
    public async Task Test1()
    {
        var imageContext = TestImageContextFactory.Get();
        using var scanningContext = new ScanningContext(imageContext);

        var scanController = new ScanController(scanningContext);
        var devices = await scanController.GetDeviceList();
        var device = GetUserDevice(devices);

        var options = new ScanOptions
        {
            Device = device,
            Driver = Driver.Wia,
            PaperSource = PaperSource.Flatbed,
            Dpi = 100
        };

        var source = scanController.Scan(options);
        var image = await source.Next();

        Assert.NotNull(image);
        
        using var rendered = imageContext.Render(image);
        
        // TODO: Aside from generating the relevant files/resources, we also need to consider how to compare images when ImageAsserts assumes perfect pixel alignment.
        // TODO: One possibility is having a section of the test page with gradual gradients and only compare that subsection of the images. 
        // ImageAsserts.Similar(ScannerTestResources.naps2_test_page, rendered);
    }

    // TODO: Generalize the common infrastructure into helper classes (ScannerTests as a base class, FlatbedTests, FeederTests, etc.?)
    private static ScanDevice GetUserDevice(List<ScanDevice> devices)
    {
        if (devices.Count == 0)
        {
            throw new InvalidOperationException("No scanner available");
        }
        foreach (var device in devices)
        {
            if (device.Name!.IndexOf(HowToRunScannerTests.SCANNER_NAME, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return device;
            }
        }
        throw new InvalidOperationException("Set SCANNER_NAME to one of: " +
                                            string.Join(",", devices.Select(x => x.Name)));
    }
}