namespace NAPS2.Scan;

public record PaperSourceCaps(
    bool SupportsFlatbed,
    bool SupportsFeeder,
    bool SupportsDuplex,
    bool CanCheckIfFeederHasPaper
)
{
    private PaperSourceCaps() : this(false, false, false, false)
    {
    }
}