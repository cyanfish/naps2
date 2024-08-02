namespace NAPS2.Scan;

public record DeviceChoice
{
    public static DeviceChoice ForDevice(ScanDevice device) => new() { Device = device, Driver = device.Driver };

    public static DeviceChoice ForAlwaysAsk(Driver driver) => new() { AlwaysAsk = true, Driver = driver };

    public static readonly DeviceChoice None = new();

    private DeviceChoice()
    {
    }

    public ScanDevice? Device { get; private init; }
    public Driver Driver { get; private init; }
    public bool AlwaysAsk { get; private init; }
}