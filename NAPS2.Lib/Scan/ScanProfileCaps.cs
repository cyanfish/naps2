namespace NAPS2.Scan;

public class ScanProfileCaps
{
    public PaperSourceProfileCaps? PaperSources { get; set; }
    public bool? FeederCheck { get; set; }
    public PerSourceProfileCaps? Glass { get; set; }
    public PerSourceProfileCaps? Feeder { get; set; }
    public PerSourceProfileCaps? Duplex { get; set; }
}