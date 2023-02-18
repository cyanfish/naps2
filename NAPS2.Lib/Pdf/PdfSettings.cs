using NAPS2.Config.Model;

namespace NAPS2.Pdf;

public class PdfSettings
{
    private PdfMetadata _metadata;
    private PdfEncryption _encryption;

    public PdfSettings()
    {
        _metadata = new PdfMetadata();
        _encryption = new PdfEncryption();
    }

    public string? DefaultFileName { get; set; }

    public bool SkipSavePrompt { get; set; }

    public bool SinglePagePdfs { get; set; }

    [Config]
    public PdfMetadata Metadata
    {
        get => _metadata;
        set => _metadata = value ?? throw new ArgumentNullException(nameof(value));
    }

    [Config]
    public PdfEncryption Encryption
    {
        get => _encryption;
        set => _encryption = value ?? throw new ArgumentNullException(nameof(value));
    }

    public PdfCompat Compat { get; set; }
}