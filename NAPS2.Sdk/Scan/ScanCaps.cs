namespace NAPS2.Scan;

public record ScanCaps(
    MetadataCaps? MetadataCaps,
    PaperSourceCaps? PaperSourceCaps,
    PerSourceCaps? FlatbedCaps,
    PerSourceCaps? FeederCaps,
    PerSourceCaps? DuplexCaps
);