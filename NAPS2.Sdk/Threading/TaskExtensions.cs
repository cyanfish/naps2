using System.Threading;
using NAPS2.Scan.Internal;

namespace NAPS2.Threading;

public static class TaskExtensions
{
    /// <summary>
    /// Does nothing. This is used to hide warnings for not awaiting async methods.
    /// </summary>
    /// <param name="task"></param>
    public static void AssertNoAwait(this Task task)
    {
    }

    /// <summary>
    /// Does nothing. This is used to hide warnings for not awaiting async methods.
    /// </summary>
    /// <param name="task"></param>
    public static void AssertNoAwait<T>(this Task<T> task)
    {
    }

    // https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/interop-with-other-asynchronous-patterns-and-types?redirectedfrom=MSDN#from-wait-handles-to-tap
    public static Task WaitOneAsync(this WaitHandle waitHandle)
    {
        if (waitHandle == null)
            throw new ArgumentNullException(nameof(waitHandle));

        var tcs = new TaskCompletionSource<bool>();
        var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle,
            delegate { tcs.TrySetResult(true); }, null, -1, true);
        var t = tcs.Task;
        t.ContinueWith(_ => rwh.Unregister(null));
        return t;
    }

    public static Task WaitForExitAsync(this Process process)
    {
        var tcs = new TaskCompletionSource<bool>();
        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => tcs.TrySetResult(true);
        if (process.HasExited)
        {
            tcs.TrySetResult(true);
        }
        return tcs.Task;
    }
}