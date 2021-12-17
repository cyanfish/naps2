namespace NAPS2.Logging;

public class StubErrorOutput : ErrorOutput
{
    public override void DisplayError(string errorMessage)
    {
    }

    public override void DisplayError(string errorMessage, string details)
    {
    }

    public override void DisplayError(string errorMessage, Exception exception)
    {
    }
}