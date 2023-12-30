namespace NAPS2.Scan;

/// <summary>
/// WIA version used for scanning (1.0 or 2.0). Generally 2.0 is preferred as it has better support for feeders.
/// </summary>
public enum WiaApiVersion
{
    Default,
    Wia10,
    Wia20
}