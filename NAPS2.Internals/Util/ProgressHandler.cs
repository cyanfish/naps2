using System.Threading;

// ReSharper disable once CheckNamespace
namespace NAPS2.Util;

/// <summary>
/// Manages the progress of an operation through a callback (e.g. "3/10 done") and/or a cancellation token.
/// </summary>
public readonly struct ProgressHandler
{
    private readonly ProgressCallback? _callback;

    public ProgressHandler()
    {
        _callback = null;
        CancelToken = default;
    }

    public ProgressHandler(ProgressCallback? callback, CancellationToken cancelToken)
    {
        _callback = callback;
        CancelToken = cancelToken;
    }

    public static implicit operator ProgressHandler(ProgressCallback callback)
    {
        return new ProgressHandler(callback, default);
    }

    public static implicit operator ProgressHandler(CancellationToken cancelToken)
    {
        return new ProgressHandler(null, cancelToken);
    }

    public void Report(int current, int max)
    {
        _callback?.Invoke(current, max);
    }

    public CancellationToken CancelToken { get; }

    public bool IsCancellationRequested => CancelToken.IsCancellationRequested;
}