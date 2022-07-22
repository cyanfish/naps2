namespace NAPS2.Serialization;

public class SerializeImageOptions
{
    // TODO: Document and maybe rename these
    
    public bool TransferOwnership { get; set; }

    public bool IncludeThumbnail { get; set; }

    public bool RequireFileStorage { get; set; }

    public bool RequireMemoryStorage { get; set; }

    public string? RenderedFilePath { get; set; }
}