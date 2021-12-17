namespace NAPS2.ImportExport.Images;

public class ImageSettings
{
    public string? DefaultFileName { get; set; }

    public bool? SkipSavePrompt { get; set; }

    public int? JpegQuality { get; set; }

    public TiffCompression? TiffCompression { get; set; }

    public bool? SinglePageTiff { get; set; }
}