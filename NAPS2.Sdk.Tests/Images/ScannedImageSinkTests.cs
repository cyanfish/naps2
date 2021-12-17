using System;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class ScannedImageSinkTests : ContextualTexts
{
    [Fact]
    public async Task NoImages()
    {
        var sink = new ScannedImageSink();
        sink.SetCompleted();

        var source = sink.AsSource();
        Assert.Null(await source.Next());
    }

    [Fact]
    public async Task OneImage()
    {
        var sink = new ScannedImageSink();
        var img1 = CreateScannedImage();
        sink.PutImage(img1);
        sink.SetCompleted();

        var source = sink.AsSource();
        Assert.Equal(img1, await source.Next());
        Assert.Null(await source.Next());
    }

    [Fact]
    public async Task TwoImages()
    {
        var sink = new ScannedImageSink();
        var img1 = CreateScannedImage();
        var img2 = CreateScannedImage();
        sink.PutImage(img1);
        sink.PutImage(img2);
        sink.SetCompleted();

        var source = sink.AsSource();
        Assert.Equal(img1, await source.Next());
        Assert.Equal(img2, await source.Next());
        Assert.Null(await source.Next());
    }

    [Fact]
    public async Task PropagatesError()
    {
        var sink = new ScannedImageSink();
        var error = new Exception();
        sink.SetError(error);

        var source = sink.AsSource();
        await Assert.ThrowsAsync<Exception>(() => source.Next());
    }

    [Fact]
    public async Task PropagatesErrorAfterImages()
    {
        var sink = new ScannedImageSink();
        var error = new Exception();
        sink.PutImage(CreateScannedImage());
        sink.PutImage(CreateScannedImage());
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

        var sink = new ScannedImageSink();
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
            sink.PutImage(CreateScannedImage());
            sink.SetCompleted();
        });
        await t1;
        await t2;
    }
}