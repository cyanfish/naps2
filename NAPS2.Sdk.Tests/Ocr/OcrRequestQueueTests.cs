using System.Collections.Immutable;
using System.Threading;
using NAPS2.Ocr;
using NSubstitute;
using Xunit;

namespace NAPS2.Sdk.Tests.Ocr;

public class OcrRequestQueueTests : ContextualTests
{
    private readonly OcrRequestQueue _ocrRequestQueue;
    private readonly IOcrEngine _mockEngine;
    private readonly ProcessedImage _image;
    private readonly string _tempPath;
    private readonly OcrParams _ocrParams;
    private readonly OcrResult _expectedResult;
    private readonly Task<OcrResult> _expectedResultTask;

    public OcrRequestQueueTests()
    {
        _mockEngine = Substitute.For<IOcrEngine>();
        _ocrRequestQueue = new OcrRequestQueue
        {
            WorkerCount = 4
        };

        _image = CreateScannedImage();
        _tempPath = CreateTempFile();
        _ocrParams = CreateOcrParams();
        _expectedResult = CreateOcrResult();
        _expectedResultTask = Task.FromResult(_expectedResult);
    }

    [Fact]
    public async Task Enqueue()
    {
        _mockEngine.ProcessImage(ScanningContext, _tempPath, _ocrParams, Arg.Any<CancellationToken>())
            .Returns(_expectedResultTask);

        var ocrResult = await DoEnqueueForeground(_image, _tempPath, _ocrParams);

        Assert.Equal(_expectedResult, ocrResult);
        Assert.False(File.Exists(_tempPath));
        _mockEngine.ReceivedCallsCount(1);
    }

    [Fact]
    public async Task EnqueueTwiceReturnsCached()
    {
        var tempPath1 = CreateTempFile();
        var tempPath2 = CreateTempFile();
        _mockEngine.ProcessImage(ScanningContext, tempPath1, _ocrParams, Arg.Any<CancellationToken>())
            .Returns(_expectedResultTask);

        await DoEnqueueForeground(_image, tempPath1, _ocrParams);
        Assert.False(File.Exists(tempPath1));

        var ocrResult2Task = DoEnqueueForeground(_image, tempPath2, _ocrParams);
        // Verify synchronous return for cache
        Assert.True(ocrResult2Task.IsCompleted);

        var ocrResult2 = await ocrResult2Task;
        Assert.Equal(_expectedResult, ocrResult2);
        Assert.False(File.Exists(tempPath2));

        // Verify only a single engine call
        _ = _mockEngine.Received().ProcessImage(ScanningContext, tempPath1, _ocrParams, Arg.Any<CancellationToken>());
        _mockEngine.ReceivedCallsCount(1);
    }

    [Fact]
    public async Task EnqueueSimultaneous()
    {
        var tempPath1 = CreateTempFile();
        var tempPath2 = CreateTempFile();
        _mockEngine.ProcessImage(ScanningContext, tempPath1, _ocrParams, Arg.Any<CancellationToken>())
            .Returns(_expectedResultTask);

        var ocrResult1Task = DoEnqueueForeground(_image, tempPath1, _ocrParams);

        var ocrResult2Task = DoEnqueueForeground(_image, tempPath2, _ocrParams);

        var ocrResult1 = await ocrResult1Task;
        var ocrResult2 = await ocrResult2Task;
        Assert.Equal(_expectedResult, ocrResult1);
        Assert.Equal(_expectedResult, ocrResult2);
        Assert.False(File.Exists(tempPath1));
        Assert.False(File.Exists(tempPath2));

        // Verify only a single engine call
        _ = _mockEngine.Received().ProcessImage(ScanningContext, tempPath1, _ocrParams, Arg.Any<CancellationToken>());
        _mockEngine.ReceivedCallsCount(1);
    }

    [Fact]
    public async Task EnqueueWithDifferentImagesReturnsDifferentResult()
    {
        var image1 = CreateScannedImage();
        var tempPath1 = CreateTempFile();
        var expectedResult1 = CreateOcrResult();
         _mockEngine.ProcessImage(ScanningContext, tempPath1, _ocrParams, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult1));

        var image2 = CreateScannedImage();
        var tempPath2 = CreateTempFile();
        var expectedResult2 = CreateOcrResult();
         _mockEngine.ProcessImage(ScanningContext, tempPath2, _ocrParams, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult2));

        var ocrResult1 = await DoEnqueueForeground(image1, tempPath1, _ocrParams);
        Assert.Equal(expectedResult1, ocrResult1);
        Assert.False(File.Exists(tempPath1));

        var ocrResult2 = await DoEnqueueForeground(image2, tempPath2, _ocrParams);
        Assert.Equal(expectedResult2, ocrResult2);
        Assert.False(File.Exists(tempPath2));

        // Verify two engine calls
        _ = _mockEngine.Received().ProcessImage(ScanningContext, tempPath1, _ocrParams, Arg.Any<CancellationToken>());
        _ = _mockEngine.Received().ProcessImage(ScanningContext, tempPath2, _ocrParams, Arg.Any<CancellationToken>());
        _mockEngine.ReceivedCallsCount(2);
    }

    [Fact]
    public async Task EnqueueWithDifferentParamsReturnsDifferentResult()
    {
        var ocrParams1 = new OcrParams("eng", OcrMode.Fast, 10);
        var ocrParams2 = new OcrParams("fra", OcrMode.Fast, 10);
        var ocrParams3 = new OcrParams("eng", OcrMode.Best, 10);
        var ocrParams4 = new OcrParams("eng", OcrMode.Fast, 0);
        var expectedResult1 = CreateOcrResult();
        var expectedResult2 = CreateOcrResult();
        var expectedResult3 = CreateOcrResult();
        var expectedResult4 = CreateOcrResult();
         _mockEngine.ProcessImage(ScanningContext, Arg.Any<string>(), ocrParams1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult1));
         _mockEngine.ProcessImage(ScanningContext, Arg.Any<string>(), ocrParams2, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult2));
         _mockEngine.ProcessImage(ScanningContext, Arg.Any<string>(), ocrParams3, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult3));
         _mockEngine.ProcessImage(ScanningContext, Arg.Any<string>(), ocrParams4, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult4));

        var ocrResult1 = await DoEnqueueForeground(_image, CreateTempFile(), ocrParams1);
        Assert.Equal(expectedResult1, ocrResult1);
        var ocrResult2 = await DoEnqueueForeground(_image, CreateTempFile(), ocrParams2);
        Assert.Equal(expectedResult2, ocrResult2);
        var ocrResult3 = await DoEnqueueForeground(_image, CreateTempFile(), ocrParams3);
        Assert.Equal(expectedResult3, ocrResult3);
        var ocrResult4 = await DoEnqueueForeground(_image, CreateTempFile(), ocrParams4);
        Assert.Equal(expectedResult4, ocrResult4);

        // Verify distinct engine calls
        _ = _mockEngine.Received().ProcessImage(ScanningContext, Arg.Any<string>(), ocrParams1, Arg.Any<CancellationToken>());
        _ = _mockEngine.Received().ProcessImage(ScanningContext, Arg.Any<string>(), ocrParams2, Arg.Any<CancellationToken>());
        _ = _mockEngine.Received().ProcessImage(ScanningContext, Arg.Any<string>(), ocrParams3, Arg.Any<CancellationToken>());
        _ = _mockEngine.Received().ProcessImage(ScanningContext, Arg.Any<string>(), ocrParams4, Arg.Any<CancellationToken>());
        _mockEngine.ReceivedCallsCount(4);
    }

    [Fact]
    public async Task EnqueueWithEngineError()
    {
         _mockEngine.When(x => x.ProcessImage(ScanningContext, _tempPath, _ocrParams, Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception());

        var ocrResult = await DoEnqueueForeground(_image, _tempPath, _ocrParams);

        Assert.Null(ocrResult);
        _ = _mockEngine.Received().ProcessImage(ScanningContext, _tempPath, _ocrParams, Arg.Any<CancellationToken>());
        _mockEngine.ReceivedCallsCount(1);
    }

    [Fact]
    public async Task EnqueueTwiceWithTransientEngineError()
    {
        var tempPath1 = CreateTempFile();
        var tempPath2 = CreateTempFile();
         _mockEngine.When(x => x.ProcessImage(ScanningContext, tempPath1, _ocrParams, Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception());
         _mockEngine.ProcessImage(ScanningContext, tempPath2, _ocrParams, Arg.Any<CancellationToken>())
            .Returns(_expectedResultTask);

        var ocrResult1 = await DoEnqueueForeground(_image, tempPath1, _ocrParams);
        var ocrResult2 = await DoEnqueueForeground(_image, tempPath2, _ocrParams);

        Assert.Null(ocrResult1);
        Assert.Equal(_expectedResult, ocrResult2);
        _ = _mockEngine.Received().ProcessImage(ScanningContext, tempPath1, _ocrParams, Arg.Any<CancellationToken>());
        _ = _mockEngine.Received().ProcessImage(ScanningContext, tempPath2, _ocrParams, Arg.Any<CancellationToken>());
        _mockEngine.ReceivedCallsCount(2);
    }

    [Fact(Skip = "flaky")]
    public async Task ImmediateCancel()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var ocrResult = await DoEnqueueForeground(_image, _tempPath, _ocrParams, cts.Token);

        Assert.Null(ocrResult);
        _mockEngine.ReceivedCallsCount(0);
    }

    [Fact]
    public async Task CancelDuringProcessing()
    {
        Task<OcrResult> mockEngineTask = null;
        var cancelledAtEngineStart = false;
        var cancelledAtEngineEnd = false;
        var cts = new CancellationTokenSource();
         _mockEngine.ProcessImage(ScanningContext, _tempPath, _ocrParams, Arg.Any<CancellationToken>()).Returns(
            x =>
            {
                mockEngineTask = Task.Run(() =>
                {
                    var cancelToken = (CancellationToken) x[3];
                    cancelledAtEngineStart = cancelToken.IsCancellationRequested;
                    cts.Cancel();
                    cancelledAtEngineEnd = cancelToken.IsCancellationRequested;
                    return (OcrResult) null;
                });
                return mockEngineTask;
            });

        var ocrResult = await DoEnqueueForeground(_image, _tempPath, _ocrParams, cts.Token);
        await mockEngineTask;

        Assert.Null(ocrResult);
        Assert.False(cancelledAtEngineStart);
        Assert.True(cancelledAtEngineEnd);
        _ = _mockEngine.Received().ProcessImage(ScanningContext, _tempPath, _ocrParams, Arg.Any<CancellationToken>());
        _mockEngine.ReceivedCallsCount(1);
    }

    [Fact(Skip = "flaky")]
    public async Task CancelOnceWithTwoReferences()
    {
        var tempPath1 = CreateTempFile();
        var tempPath2 = CreateTempFile();
         _mockEngine.ProcessImage(ScanningContext, tempPath1, _ocrParams, Arg.Any<CancellationToken>())
            .Returns(_ => Task.Run(async () =>
            {
                await Task.Delay(200);
                return _expectedResult;
            }));
        // Delay the workers so we can cancel before processing starts
        _ocrRequestQueue.WorkerAddedLatency = 50;

        var cts = new CancellationTokenSource();
        var ocrResult1Task = DoEnqueueForeground(_image, tempPath1, _ocrParams, cts.Token);
        var ocrResult2Task = DoEnqueueForeground(_image, tempPath2, _ocrParams);
        cts.Cancel();
        
        var ocrResult1 = await ocrResult1Task;
        var ocrResult2 = await ocrResult2Task;
        Assert.Null(ocrResult1);
        Assert.Equal(_expectedResult, ocrResult2);
        _ = _mockEngine.Received().ProcessImage(ScanningContext, tempPath1, _ocrParams, Arg.Any<CancellationToken>());
        _mockEngine.ReceivedCallsCount(1);
    }

    [Fact(Skip = "flaky")]
    public async Task CancelTwiceWithTwoReferences()
    {
        // Delay the workers so we can cancel before processing starts
        _ocrRequestQueue.WorkerAddedLatency = 50;

        var cts = new CancellationTokenSource();
        var ocrResult1Task = DoEnqueueForeground(_image, _tempPath, _ocrParams, cts.Token);
        var ocrResult2Task = DoEnqueueForeground(_image, _tempPath, _ocrParams, cts.Token);
        cts.Cancel();
        
        var ocrResult1 = await ocrResult1Task;
        var ocrResult2 = await ocrResult2Task;
        Assert.Null(ocrResult1);
        Assert.Null(ocrResult2);
        await Task.Delay(200);
        _mockEngine.ReceivedCallsCount(0);
    }

    // TODO: Deflake
    [Fact(Skip = "flaky")]
    public async Task ForegroundPrioritized()
    {
         _mockEngine.ProcessImage(ScanningContext, _tempPath, _ocrParams, Arg.Any<CancellationToken>())
            .Returns(_expectedResultTask);
        _ocrRequestQueue.WorkerAddedLatency = 50;

        var foregroundTasks = EnqueueMany(OcrPriority.Foreground, 8);
        var backgroundTasks = EnqueueMany(OcrPriority.Background, 8);
        foregroundTasks.AddRange(EnqueueMany(OcrPriority.Foreground, 8));
        backgroundTasks.AddRange(EnqueueMany(OcrPriority.Background, 8));

        await Task.WhenAny(backgroundTasks);

        int foregroundCompletedCount = foregroundTasks.Count(x => x.IsCompleted);
        // With 4 worker tasks and 16 foreground tasks, at least 13 should be completed before moving to background tasks
        Assert.InRange(foregroundCompletedCount, 13, 16);
        await Task.WhenAll(backgroundTasks);
        await Task.WhenAll(foregroundTasks);
    }

    // This seems to cause some crash (task/thread starvation related?) on Linux
    [PlatformFact(exclude: PlatformFlags.Linux)]
    public async Task StressTest()
    {
         _mockEngine.ProcessImage(ScanningContext, Arg.Any<string>(), _ocrParams, Arg.Any<CancellationToken>())
            .Returns(_expectedResultTask);

        var tasks = EnqueueMany(OcrPriority.Foreground, 1000);
        int completedCount = 0;
        for (int i = 0; i < 10 && completedCount < 1000; i++)
        {
            await Task.Delay(500);
            completedCount = tasks.Count(x => x.IsCompleted);
        }
        Assert.Equal(1000, completedCount);
    }
    
    [Fact]
    public async Task HasCachedResult()
    {
         _mockEngine.ProcessImage(ScanningContext, _tempPath, _ocrParams, Arg.Any<CancellationToken>())
            .Returns(_expectedResultTask);

        await DoEnqueueForeground(_image, _tempPath, _ocrParams);

        var ocrParams2 = new OcrParams("fra", OcrMode.Fast, 10);
        
        Assert.True(_ocrRequestQueue.HasCachedResult(_mockEngine, _image, _ocrParams));
        Assert.False(_ocrRequestQueue.HasCachedResult(_mockEngine, _image, ocrParams2));
        Assert.False(_ocrRequestQueue.HasCachedResult(_mockEngine, CreateScannedImage(), _ocrParams));
        Assert.False(_ocrRequestQueue.HasCachedResult(Substitute.For<IOcrEngine>(), _image, _ocrParams));
    }
    
    [Fact(Skip = "flaky")]
    public async Task HasCachedResult_WhileProcessing()
    {
         _mockEngine.ProcessImage(ScanningContext, _tempPath, _ocrParams, Arg.Any<CancellationToken>())
            .Returns(_expectedResultTask);

        _ocrRequestQueue.WorkerAddedLatency = 50;
        var queuedTask = DoEnqueueForeground(_image, _tempPath, _ocrParams);

        Assert.False(_ocrRequestQueue.HasCachedResult(_mockEngine, _image, _ocrParams));
        await queuedTask;
        Assert.True(_ocrRequestQueue.HasCachedResult(_mockEngine, _image, _ocrParams));
    }
    
    [Fact]
    public async Task HasCachedResult_WithError()
    {
         _mockEngine.When(x => x.ProcessImage(ScanningContext, _tempPath, _ocrParams, Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception());

        await DoEnqueueForeground(_image, _tempPath, _ocrParams);

        Assert.False(_ocrRequestQueue.HasCachedResult(_mockEngine, _image, _ocrParams));
    }

    private List<Task<OcrResult>> EnqueueMany(OcrPriority priority, int count)
    {
        return Enumerable.Range(0, count)
            .Select(x => DoEnqueue(priority, CreateScannedImage(), CreateTempFile(), _ocrParams)).ToList();
    }

    private string CreateTempFile()
    {
        var path = Path.Combine(FolderPath, $"tempocr{Guid.NewGuid()}.jpg");
        File.WriteAllText(path, @"blah");
        return path;
    }

    private static OcrResult CreateOcrResult()
    {
        var uniqueElement = new OcrResultElement(Guid.NewGuid().ToString(), "eng", false, (0, 0, 1, 1), 0, 10, ImmutableList<OcrResultElement>.Empty);
        return new OcrResult((0, 0, 1, 1), ImmutableList.Create(uniqueElement), ImmutableList.Create(uniqueElement));
    }

    private static OcrParams CreateOcrParams()
    {
        return new OcrParams("eng", OcrMode.Fast, 10);
    }

    private Task<OcrResult> DoEnqueueForeground(ProcessedImage image, string tempPath, OcrParams ocrParams,
        CancellationToken cancellationToken = default)
    {
        return DoEnqueue(OcrPriority.Foreground, image, tempPath, ocrParams, cancellationToken);
    }

    private Task<OcrResult> DoEnqueue(OcrPriority priority, ProcessedImage image, string tempPath, OcrParams ocrParams,
        CancellationToken cancellationToken = default)
    {
        return _ocrRequestQueue.Enqueue(
            ScanningContext,
            _mockEngine,
            image,
            tempPath,
            ocrParams,
            priority,
            cancellationToken);
    }
}