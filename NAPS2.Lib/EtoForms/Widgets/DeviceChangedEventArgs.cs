using NAPS2.Scan;

namespace NAPS2.EtoForms.Widgets;

public class DeviceChangedEventArgs : EventArgs
{
    public required DeviceChoice PreviousChoice { get; init; }

    public required DeviceChoice NewChoice { get; init; }
}