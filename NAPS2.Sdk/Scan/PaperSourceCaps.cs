namespace NAPS2.Scan;

public class PaperSourceCaps
{
    public bool SupportsFlatbed { get; init; }

    public bool SupportsFeeder { get; init; }

    public bool SupportsDuplex { get; init; }

    public bool CanCheckIfFeederHasPaper { get; init; }
}