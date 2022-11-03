// ReSharper disable once CheckNamespace
namespace NAPS2.Util;

public class LeakTracer
{
    private const bool ENABLE_TRACING = false;

    private static readonly Dictionary<object, string> _traces = new();

    public static void StartTracking(object obj)
    {
        if (!ENABLE_TRACING) return;
        _traces.Add(obj, GetTrace());
    }

    private static string GetTrace()
    {
        var lines = Environment.StackTrace.Split('\n');
        return string.Join("\n", lines.Skip(3).Take(10));
    }

    public static void StopTracking(object obj)
    {
        if (!ENABLE_TRACING) return;
        _traces.Remove(obj);
    }

    public static void PrintTraces()
    {
        var traceCounts = new Dictionary<string, int>();
        foreach (var trace in _traces.Values)
        {
            if (!traceCounts.ContainsKey(trace))
            {
                traceCounts.Add(trace, 0);
            }
            traceCounts[trace]++;
        }
        foreach (var kvp in traceCounts)
        {
            Console.WriteLine($"Potential leak (count: {kvp.Value}):");
            Console.WriteLine(kvp.Key);
        }
    }
}