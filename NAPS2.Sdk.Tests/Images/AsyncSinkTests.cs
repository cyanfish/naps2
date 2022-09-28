using System.Threading;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class AsyncSinkTests : ContextualTests
{
    [Fact]
    public async Task NoImages()
    {
        var sink = new AsyncSink<ProcessedImage>();
        sink.SetCompleted();

        var enumerator = sink.AsAsyncEnumerable().GetAsyncEnumerator();
        Assert.False(await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task OneImage()
    {
        var sink = new AsyncSink<ProcessedImage>();
        var img1 = CreateScannedImage();
        sink.PutItem(img1);
        sink.SetCompleted();

        var enumerator = sink.AsAsyncEnumerable().GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(img1, enumerator.Current);
        Assert.False(await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task TwoImages()
    {
        var sink = new AsyncSink<ProcessedImage>();
        var img1 = CreateScannedImage();
        var img2 = CreateScannedImage();
        sink.PutItem(img1);
        sink.PutItem(img2);
        sink.SetCompleted();

        var enumerator = sink.AsAsyncEnumerable().GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(img1, enumerator.Current);
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(img2, enumerator.Current);
        Assert.False(await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task PropagatesError()
    {
        var sink = new AsyncSink<ProcessedImage>();
        var error = new Exception();
        sink.SetError(error);

        var enumerator = sink.AsAsyncEnumerable().GetAsyncEnumerator();
        await Assert.ThrowsAsync<Exception>(async () => await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task PropagatesErrorAfterImages()
    {
        var sink = new AsyncSink<ProcessedImage>();
        var error = new Exception();
        sink.PutItem(CreateScannedImage());
        sink.PutItem(CreateScannedImage());
        sink.SetError(error);

        var enumerator = sink.AsAsyncEnumerable().GetAsyncEnumerator();
        await enumerator.MoveNextAsync();
        await enumerator.MoveNextAsync();
        await Assert.ThrowsAsync<Exception>(async () => await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task RaceCondition()
    {
        var wait = new ManualResetEvent(false);

        var sink = new AsyncSink<ProcessedImage>();
        var enumerator = sink.AsAsyncEnumerable().GetAsyncEnumerator();
        var t1 = Task.Run(async () =>
        {
            wait.Set();
            Assert.True(await enumerator.MoveNextAsync());
            Assert.False(await enumerator.MoveNextAsync());
        });
        var t2 = Task.Run(() =>
        {
            wait.WaitOne();
            sink.PutItem(CreateScannedImage());
            sink.SetCompleted();
        });
        await t1;
        await t2;
    }
}