namespace NAPS2.Util;

internal class DebugTimer : IDisposable
{
    private readonly string? _label;
    private readonly Stopwatch _stopwatch;

    public DebugTimer(string? label = null)
    {
        _label = label;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        Debug.WriteLine(_label == null
            ? $"{_stopwatch.ElapsedMilliseconds} ms"
            : $"{_stopwatch.ElapsedMilliseconds} ms : {_label}");
    }
}