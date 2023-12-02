using NAPS2.Escl.Server;
using NAPS2.Images.Gdi;
using NAPS2.Remoting.Server;
using NAPS2.Scan;

namespace NAPS2.Sdk.Samples;

public class NetworkSharingSample
{
    public static async Task Server()
    {
        // NAPS2 can share scanners across the local network using the ESCL protocol with the NAPS2.Escl.Server package.
        // On the server, you need to set up ScanServer with the device(s) to share.
        // On the client, you just scan as usual using Driver.Escl.

        using var scanningContext = new ScanningContext(new GdiImageContext());

        // Get the device to share
        var controller = new ScanController(scanningContext);
        ScanDevice device = (await controller.GetDeviceList()).First();

        // Set up the server (you'll need to reference NAPS2.Escl.Server to be able to create an EsclServer object).
        using var scanServer = new ScanServer(scanningContext, new EsclServer());

        // Register a device to be shared
        scanServer.RegisterDevice(new SharedDevice
        {
            Name = device.Name,
            Device = device
        });

        // Run the server until the user presses Enter
        scanServer.Start();
        Console.ReadLine();
        scanServer.Stop();
    }

    public static async Task Client()
    {
        using var scanningContext = new ScanningContext(new GdiImageContext());
        var controller = new ScanController(scanningContext);

        // Find the shared device using Driver.Escl
        ScanDevice device = (await controller.GetDeviceList(Driver.Escl)).First();

        // Set up options using Driver.Escl
        var options = new ScanOptions { Device = device, Driver = Driver.Escl };

        // Do the scan
        await foreach (var image in controller.Scan(options))
        {
            Console.WriteLine("Scanned a page!");
        }
    }
}