using NAPS2.Scan;

namespace NAPS2.Automation;

public class ConsoleDevicePrompt : IDevicePrompt
{
    public Task<ScanDevice?> PromptForDevice(ScanOptions options)
    {
        // TODO: What's best to do here? Use a GUI prompt? A console prompt? Or just do nothing like this?
        return Task.FromResult<ScanDevice?>(null);
    }
}