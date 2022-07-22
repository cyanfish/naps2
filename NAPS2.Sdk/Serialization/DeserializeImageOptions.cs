namespace NAPS2.Serialization;

public class DeserializeImageOptions
{
    /// <summary>
    /// If true, the Deserialize caller guarantees that the file storage will not be used for longer than the duration of the RPC call.
    /// In this way, files can be safely reused even if ownership isn't transferred to the callee.
    /// This should not be true outside of an RPC context.
    /// </summary>
    public bool ShareFileStorage { get; set; }
}