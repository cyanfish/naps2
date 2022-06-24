using System.Windows.Forms;
using System.Xml.Serialization;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
using NAPS2.Scan;
using NAPS2.Scan.Batch;

namespace NAPS2.Config.ObsoleteTypes;

[XmlType("UserConfig")]
public class UserConfigV0
{
    public int Version { get; set; }

    public string? Culture { get; set; }

    public List<FormState> FormStates { get; set; } = new();

    public HashSet<string> BackgroundOperations { get; set; } = new();

    public bool CheckForUpdates { get; set; }

    public DateTime? LastUpdateCheckDate { get; set; }

    public DateTime? FirstRunDate { get; set; }

    public DateTime? LastDonatePromptDate { get; set; }

    public bool EnableOcr { get; set; }

    public string? OcrLanguageCode { get; set; }

    public OcrMode OcrMode { get; set; }

    public bool OcrAfterScanning { get; set; }

    public string? LastImageExt { get; set; }

    public PdfSettings PdfSettings { get; set; } = new();

    public ImageSettings ImageSettings { get; set; } = new();

    public EmailSettings EmailSettings { get; set; } = new();

    public EmailSetup EmailSetup { get; set; } = new();

    public int ThumbnailSize { get; set; } = ThumbnailSizes.DEFAULT_SIZE;

    public BatchSettings? LastBatchSettings { get; set; }

    public DockStyle DesktopToolStripDock { get; set; }

    public KeyboardShortcuts KeyboardShortcuts { get; set; } = new();

    public List<NamedPageSize> CustomPageSizePresets { get; set; } = new();

    public List<ScanProxyConfig> SavedProxies { get; set; } = new();
}