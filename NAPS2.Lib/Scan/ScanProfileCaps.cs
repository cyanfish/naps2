using NAPS2.Serialization;

namespace NAPS2.Scan;

[GeneratePropertyAccessors]
public class ScanProfileCaps
{
    public PaperSourceProfileCaps? PaperSources { get; set; }
    public bool? FeederCheck { get; set; }
    public PerSourceProfileCaps? Glass { get; set; }
    public PerSourceProfileCaps? Feeder { get; set; }
    public PerSourceProfileCaps? Duplex { get; set; }
}