// ReSharper disable once CheckNamespace
namespace NAPS2.Util;

public static class AsyncProducers
{
#pragma warning disable CS1998
    public static async IAsyncEnumerable<T> Empty<T>()
    {
        yield break;
    }

    public static IAsyncEnumerable<T> RunProducer<T>(ItemProducer<T> producer) where T : class
    {
        return RunProducer(new AsyncItemProducer<T>(produce =>
        {
            producer(produce);
            return Task.CompletedTask;
        }));
    }

    public static IAsyncEnumerable<T> RunProducer<T>(AsyncItemProducer<T> producer) where T : class
    {
        var sink = new AsyncSink<T>();
        Task.Run(async () =>
        {
            try
            {
                await producer(item => sink.PutItem(item));
            }
            catch (Exception ex)
            {
                sink.SetError(ex);
            }
            finally
            {
                sink.SetCompleted();
            }
        });
        return sink.AsAsyncEnumerable();
    }

    public delegate void ItemProducer<out T>(Action<T> produce);
    public delegate Task AsyncItemProducer<out T>(Action<T> produce);
}