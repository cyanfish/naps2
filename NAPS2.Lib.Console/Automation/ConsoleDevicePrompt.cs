using NAPS2.Scan;

namespace NAPS2.Automation;

public class ConsoleDevicePrompt : IDevicePrompt
{
    public ScanDevice? PromptForDevice(List<ScanDevice> deviceList, IntPtr dialogParent)
    {
        // TODO: What's best to do here? Use a GUI prompt? A console prompt? Or just do nothing like this?
        return null;
    }
}