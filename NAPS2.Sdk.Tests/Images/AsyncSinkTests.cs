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

        var source = sink.AsSource();
        Assert.Null(await source.Next());
    }

    [Fact]
    public async Task OneImage()
    {
        var sink = new AsyncSink<ProcessedImage>();
        var img1 = CreateScannedImage();
        sink.PutItem(img1);
        sink.SetCompleted();

        var source = sink.AsSource();
        Assert.Equal(img1, await source.Next());
        Assert.Null(await source.Next());
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

        var source = sink.AsSource();
        Assert.Equal(img1, await source.Next());
        Assert.Equal(img2, await source.Next());
        Assert.Null(await source.Next());
    }

    [Fact]
    public async Task PropagatesError()
    {
        var sink = new AsyncSink<ProcessedImage>();
        var error = new Exception();
        sink.SetError(error);

        var source = sink.AsSource();
        await Assert.ThrowsAsync<Exception>(() => source.Next());
    }

    [Fact]
    public async Task PropagatesErrorAfterImages()
    {
        var sink = new AsyncSink<ProcessedImage>();
        var error = new Exception();
        sink.PutItem(CreateScannedImage());
        sink.PutItem(CreateScannedImage());
        sink.SetError(error);

        var source = sink.AsSource();
        await source.Next();
        await source.Next();
        await Assert.ThrowsAsync<Exception>(() => source.Next());
    }

    [Fact]
    public async Task RaceCondition()
    {
        var wait = new ManualResetEvent(false);

        var sink = new AsyncSink<ProcessedImage>();
        var source = sink.AsSource();
        var t1 = Task.Run(async () =>
        {
            wait.Set();
            Assert.NotNull(await source.Next());
            Assert.Null(await source.Next());
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