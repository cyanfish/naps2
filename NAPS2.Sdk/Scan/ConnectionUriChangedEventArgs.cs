namespace NAPS2.Scan;

public class ConnectionUriChangedEventArgs : EventArgs
{
    public ConnectionUriChangedEventArgs(string newConnectionUri)
    {
        NewConnectionUri = newConnectionUri;
    }

    /// <summary>
    /// Gets the new connection URI.
    /// </summary>
    public string NewConnectionUri { get; }
}