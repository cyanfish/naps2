namespace NAPS2.Scan.Wia;

public record WiaConfiguration(
    Dictionary<int, object> DeviceProps,
    Dictionary<int, object> ItemProps,
    string ItemName);