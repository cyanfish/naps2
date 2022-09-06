using NAPS2.EtoForms.Ui;
using NAPS2.Scan;
using NAPS2.WinForms;

namespace NAPS2.EtoForms;

public class EtoDevicePrompt : IDevicePrompt
{
    private readonly IFormFactory _formFactory;

    public EtoDevicePrompt(IFormFactory formFactory)
    {
        _formFactory = formFactory;
    }

    public ScanDevice? PromptForDevice(List<ScanDevice> deviceList, IntPtr dialogParent)
    {
        var deviceForm = _formFactory.Create<SelectDeviceForm>();
        deviceForm.DeviceList = deviceList;
        deviceForm.ShowModal();
        return deviceForm.SelectedDevice;
    }
}