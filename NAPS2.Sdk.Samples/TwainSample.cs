using NAPS2.Images.Gdi;
using NAPS2.Scan;

namespace NAPS2.Sdk.Samples;

public class TwainSample
{
    public static async Task Run()
    {
        // Scanning with TWAIN on Windows must happen from a 32-bit process. You can do this by building your exe as
        // 32-bit, but the better solution is to install the NAPS2.Sdk.Worker.Win32 Nuget package, which includes a
        // pre-compiled 32-bit NAPS2.Worker.exe. Then you only need to call ScanningContext.SetUpWin32Worker and you
        // should be able to scan with TWAIN.
        //
        // If you want to use a worker process but don't want to use a pre-compiled exe (or want to set up your own
        // logging etc), you can also build your own worker exe with the same name (and call WorkerServer.Run in its
        // Main method).

        using var scanningContext = new ScanningContext(new GdiImageContext());

        // Set up the worker; this includes starting a worker process in the background so it will be ready to respond
        // when we need it
        scanningContext.SetUpWin32Worker();

        var controller = new ScanController(scanningContext);

        // As we're not using the default (WIA) driver, we need to specify it when listing devices or scanning
        ScanDevice device = (await controller.GetDeviceList(Driver.Twain)).First();
        var options = new ScanOptions { Device = device, Driver = Driver.Twain };

        await foreach (var image in controller.Scan(options))
        {
            Console.WriteLine("Scanned a page!");
        }
    }
}