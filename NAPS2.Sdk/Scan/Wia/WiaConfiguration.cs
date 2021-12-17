using System.Collections.Generic;

namespace NAPS2.Scan.Wia;

public class WiaConfiguration
{
    public Dictionary<int, object>? DeviceProps { get; set; }

    public Dictionary<int, object>? ItemProps { get; set; }

    public string? ItemName { get; set; }
}