namespace NAPS2.Scan;

public class ScanEndEventArgs : EventArgs
{
    public ScanEndEventArgs(AsyncSource<ProcessedImage> source)
    {
        Source = source;
    }

    public AsyncSource<ProcessedImage> Source { get; }
}