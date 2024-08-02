namespace NAPS2.Scan;

public interface IDevicePrompt
{
    public Task<DeviceChoice> PromptForDevice(ScanOptions options, bool allowAlwaysAsk);
}