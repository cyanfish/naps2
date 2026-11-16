using NAPS2.Serialization;

namespace NAPS2.ImportExport.Images;

[GeneratePropertyAccessors]
public record ImageSettings
{
    public string? DefaultFileName { get; init; }

    public bool SkipSavePrompt { get; init; }

    public int JpegQuality { get; init; } = 75;

    public TiffCompression TiffCompression { get; init; } = TiffCompression.Auto;

    public bool SinglePageTiff { get; init; }
}