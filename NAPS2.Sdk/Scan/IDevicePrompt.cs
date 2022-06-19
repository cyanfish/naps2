namespace NAPS2.Scan;

public interface IDevicePrompt
{
    public ScanDevice? PromptForDevice(List<ScanDevice> deviceList, IntPtr dialogParent);
}