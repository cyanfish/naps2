using NAPS2.Platform.Windows;
using NAPS2.Scan;

namespace NAPS2.WinForms;

public class WinFormsDevicePrompt : IDevicePrompt
{
    private readonly IFormFactory _formFactory;

    public WinFormsDevicePrompt(IFormFactory formFactory)
    {
        _formFactory = formFactory;
    }

    public ScanDevice? PromptForDevice(List<ScanDevice> deviceList, IntPtr dialogParent)
    {
        var deviceForm = _formFactory.Create<FSelectDevice>();
        deviceForm.DeviceList = deviceList;
        deviceForm.ShowDialog(new Win32Window(dialogParent));
        return deviceForm.SelectedDevice;
    }
}