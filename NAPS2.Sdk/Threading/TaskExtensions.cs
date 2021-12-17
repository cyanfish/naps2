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
}