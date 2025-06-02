namespace NAPS2.Scan;

public class DeviceUriChangedEventArgs : EventArgs
{
    public DeviceUriChangedEventArgs(string? iconUri, string? connectionUri)
    {
        IconUri = iconUri;
        ConnectionUri = connectionUri;
    }

    /// <summary>
    /// Gets the new icon URI.
    /// </summary>
    public string? IconUri { get; }

    /// <summary>
    /// Gets the new connection URI.
    /// </summary>
    public string? ConnectionUri { get; }
}