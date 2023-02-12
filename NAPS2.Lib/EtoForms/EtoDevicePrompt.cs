using NAPS2.EtoForms.Ui;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;

namespace NAPS2.EtoForms;

public class EtoDevicePrompt : IDevicePrompt
{
    private readonly IFormFactory _formFactory;
    private readonly ScanningContext _scanningContext;

    public EtoDevicePrompt(IFormFactory formFactory, ScanningContext scanningContext)
    {
        _formFactory = formFactory;
        _scanningContext = scanningContext;
    }

    public async Task<ScanDevice?> PromptForDevice(ScanOptions options)
    {
        // TWAIN and WIA get devices very fast (<1s) so it's better UX to just wait until we load the devices before
        // showing the selection dialog.
        // On the other hand, SANE can take a long time (10s), and Apple/ESCL wait a couple seconds for potential
        // network responses, so it's better to show the selection dialog and lazily populate it.
        bool waitForDevices = options.Driver is Driver.Wia or Driver.Twain;
        if (waitForDevices)
        {
            var deviceList = await new ScanController(_scanningContext).GetDeviceList(options);
            if (deviceList.Count == 0)
            {
                throw new NoDevicesFoundException();
            }
            return Invoker.Current.InvokeGet(() =>
            {
                var deviceForm = _formFactory.Create<SelectDeviceForm>();
                deviceForm.DeviceList = deviceList;
                deviceForm.ShowModal();
                return deviceForm.SelectedDevice;
            });
        }
        else
        {
            var devices = new ScanController(_scanningContext).GetDevices(options);
            return Invoker.Current.InvokeGet(() =>
            {
                var deviceForm = _formFactory.Create<SelectDeviceForm>();
                deviceForm.AsyncDevices = devices;
                deviceForm.ShowModal();
                return deviceForm.SelectedDevice;
            });
        }
    }
}