using System.Threading;

namespace NAPS2.Util;

internal class ExpFallback
{
    public ExpFallback(int min, int max)
    {
        Min = min;
        Max = max;
        Value = Min;
    }

    public int Min { get; }

    public int Max { get; }

    public int Value { get; private set; }

    public void Reset()
    {
        Value = Min;
    }

    public void Increase()
    {
        Value = Math.Min(Value * 2, Max);
    }

    public async Task DelayTask(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(Value, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }
}