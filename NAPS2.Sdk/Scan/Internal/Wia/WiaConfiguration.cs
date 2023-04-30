namespace NAPS2.Scan.Internal.Wia;

internal record WiaConfiguration
{
    public Dictionary<int, object> DeviceProps { get; init; } = new();

    public Dictionary<int, object> ItemProps { get; init; } = new();

    public string ItemName { get; init; } = "";
};