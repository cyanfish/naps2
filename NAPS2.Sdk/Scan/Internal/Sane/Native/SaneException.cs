namespace NAPS2.Scan.Internal.Sane.Native;

internal class SaneException : Exception
{
    public SaneException(SaneStatus status) : base($"SANE error: {status}")
    {
        Status = status;
    }

    public SaneStatus Status { get; }
}