namespace NAPS2.Automation;

public class ConsoleErrorOutput : ErrorOutput
{
    private readonly ConsoleOutput _output;

    public ConsoleErrorOutput(ConsoleOutput output)
    {
        _output = output;
    }

    public bool HasError { get; private set; }

    public override void DisplayError(string errorMessage)
    {
        HasError = true;
        _output.Writer.WriteLine(errorMessage);
    }

    public override void DisplayError(string errorMessage, string details)
    {
        DisplayError(errorMessage);
    }

    public override void DisplayError(string errorMessage, Exception exception)
    {
        DisplayError(errorMessage);
    }
}