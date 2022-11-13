namespace NAPS2.Tools;

public static class Output
{
    public static bool EnableVerbose { get; set; }

    public static void Info(string text) => Console.WriteLine(text);

    public static void Verbose(string text)
    {
        if (EnableVerbose)
        {
            Console.WriteLine(text);
        }
    }

    public static void OperationEnd(string text)
    {
        if (EnableVerbose)
        {
            Console.WriteLine(text);
        }
        else
        {
            Console.WriteLine("Done.");
        }
    }
}