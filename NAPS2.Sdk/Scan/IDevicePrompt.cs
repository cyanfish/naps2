namespace NAPS2.Scan;

public interface IDevicePrompt
{
    public Task<ScanDevice?> PromptForDevice(ScanOptions options);
}