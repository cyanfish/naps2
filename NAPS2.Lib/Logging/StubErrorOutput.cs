namespace NAPS2.Logging;

public class StubErrorOutput : ErrorOutput
{
    public override void DisplayError(string errorMessage)
    {
    }

    public override void DisplayError(string errorMessage, string details, string? link = null)
    {
    }

    public override void DisplayError(string errorMessage, Exception exception, string? link = null)
    {
    }
}