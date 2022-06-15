using System.Collections.Immutable;
using System.Threading;
using Moq;
using NAPS2.Ocr;
using Xunit;

namespace NAPS2.Sdk.Tests.Ocr;

public class OcrRequestQueueTests : ContextualTexts
{
    private readonly OcrRequestQueue _ocrRequestQueue;
    private readonly Mock<IOcrEngine> _mockEngine;
    private readonly Mock<OperationProgress> _mockOperationProgress;

    public OcrRequestQueueTests()
    {
        _mockEngine = new Mock<IOcrEngine>(MockBehavior.Strict);
        _mockOperationProgress = new Mock<OperationProgress>();
        _ocrRequestQueue = new OcrRequestQueue(_mockOperationProgress.Object);
    }

    [Fact]
    public async Task Enqueue()
    {
        var image = CreateScannedImage();
        var tempPath = CreateTempFile();
        var ocrParams = CreateOcrParams();
        var expectedResult = CreateOcrResult();
        _mockEngine.Setup(x => x.ProcessImage(tempPath, ocrParams, It.IsAny<CancellationToken>())).Returns(expectedResult);

        var ocrResult =
            await _ocrRequestQueue.Enqueue(
                _mockEngine.Object,
                image,
                tempPath,
                ocrParams,
                OcrPriority.Foreground,
                CancellationToken.None);

        Assert.Equal(expectedResult, ocrResult);
        Assert.False(File.Exists(tempPath));
    }

    [Fact]
    public async Task EnqueueTwiceReturnsCached()
    {
        var image = CreateScannedImage();
        var tempPath1 = CreateTempFile();
        var tempPath2 = CreateTempFile();
        var ocrParams = CreateOcrParams();
        var expectedResult = CreateOcrResult();
        _mockEngine.Setup(x => x.ProcessImage(tempPath1, ocrParams, It.IsAny<CancellationToken>())).Returns(expectedResult);

        await _ocrRequestQueue.Enqueue(
                _mockEngine.Object,
                image,
                tempPath1,
                ocrParams,
                OcrPriority.Foreground,
                CancellationToken.None);
        Assert.False(File.Exists(tempPath1));

        var ocrResult2Task =
            _ocrRequestQueue.Enqueue(
                _mockEngine.Object,
                image,
                tempPath2,
                ocrParams,
                OcrPriority.Foreground,
                CancellationToken.None);
        // Verify synchronous return for cache
        Assert.True(ocrResult2Task.IsCompleted);

        var ocrResult2 = await ocrResult2Task;
        Assert.Equal(expectedResult, ocrResult2);
        Assert.False(File.Exists(tempPath2));

        // Verify only a single engine call
        _mockEngine.Verify(x => x.ProcessImage(tempPath1, ocrParams, It.IsAny<CancellationToken>()));
        _mockEngine.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnqueueSimultaneous()
    {
        var image = CreateScannedImage();
        var tempPath1 = CreateTempFile();
        var tempPath2 = CreateTempFile();
        var ocrParams = CreateOcrParams();
        var expectedResult = CreateOcrResult();
        _mockEngine.Setup(x => x.ProcessImage(tempPath1, ocrParams, It.IsAny<CancellationToken>())).Returns(expectedResult);

        var ocrResult1Task = _ocrRequestQueue.Enqueue(
            _mockEngine.Object,
            image,
            tempPath1,
            ocrParams,
            OcrPriority.Foreground,
            CancellationToken.None);

        var ocrResult2Task =
            _ocrRequestQueue.Enqueue(
                _mockEngine.Object,
                image,
                tempPath2,
                ocrParams,
                OcrPriority.Foreground,
                CancellationToken.None);

        var ocrResult1 = await ocrResult1Task;
        var ocrResult2 = await ocrResult2Task;
        Assert.Equal(expectedResult, ocrResult1);
        Assert.Equal(expectedResult, ocrResult2);
        Assert.False(File.Exists(tempPath1));
        Assert.False(File.Exists(tempPath2));

        // Verify only a single engine call
        _mockEngine.Verify(x => x.ProcessImage(tempPath1, ocrParams, It.IsAny<CancellationToken>()));
        _mockEngine.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnqueueWithDifferentImagesReturnsDifferentResult()
    {
        var image1 = CreateScannedImage();
        var tempPath1 = CreateTempFile();
        var ocrParams1 = CreateOcrParams();
        var expectedResult1 = CreateOcrResult();
        _mockEngine.Setup(x => x.ProcessImage(tempPath1, ocrParams1, It.IsAny<CancellationToken>())).Returns(expectedResult1);
        
        var image2 = CreateScannedImage();
        var tempPath2 = CreateTempFile();
        var ocrParams2 = CreateOcrParams();
        var expectedResult2 = CreateOcrResult();
        _mockEngine.Setup(x => x.ProcessImage(tempPath2, ocrParams2, It.IsAny<CancellationToken>())).Returns(expectedResult2);

        var ocrResult1 = await _ocrRequestQueue.Enqueue(
            _mockEngine.Object,
            image1,
            tempPath1,
            ocrParams1,
            OcrPriority.Foreground,
            CancellationToken.None);
        Assert.Equal(expectedResult1, ocrResult1);
        Assert.False(File.Exists(tempPath1));

        var ocrResult2 = await _ocrRequestQueue.Enqueue(
                _mockEngine.Object,
                image2,
                tempPath2,
                ocrParams2,
                OcrPriority.Foreground,
                CancellationToken.None);
        Assert.Equal(expectedResult2, ocrResult2);
        Assert.False(File.Exists(tempPath2));

        // Verify two engine calls
        _mockEngine.Verify(x => x.ProcessImage(tempPath1, ocrParams1, It.IsAny<CancellationToken>()));
        _mockEngine.Verify(x => x.ProcessImage(tempPath2, ocrParams2, It.IsAny<CancellationToken>()));
        _mockEngine.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnqueueWithDifferentParamsReturnsDifferentResult()
    {
        var image = CreateScannedImage();
        var ocrParams1 = new OcrParams("eng", OcrMode.Fast, 10);
        var ocrParams2 = new OcrParams("fra", OcrMode.Fast, 10);
        var ocrParams3 = new OcrParams("eng", OcrMode.Best, 10);
        var ocrParams4 = new OcrParams("eng", OcrMode.Fast, 0);
        var expectedResult1 = CreateOcrResult();
        var expectedResult2 = CreateOcrResult();
        var expectedResult3 = CreateOcrResult();
        var expectedResult4 = CreateOcrResult();
        _mockEngine.Setup(x => x.ProcessImage(It.IsAny<string>(), ocrParams1, It.IsAny<CancellationToken>()))
            .Returns(expectedResult1);
        _mockEngine.Setup(x => x.ProcessImage(It.IsAny<string>(), ocrParams2, It.IsAny<CancellationToken>()))
            .Returns(expectedResult2);
        _mockEngine.Setup(x => x.ProcessImage(It.IsAny<string>(), ocrParams3, It.IsAny<CancellationToken>()))
            .Returns(expectedResult3);
        _mockEngine.Setup(x => x.ProcessImage(It.IsAny<string>(), ocrParams4, It.IsAny<CancellationToken>()))
            .Returns(expectedResult4);

        Task<OcrResult?> DoEnqueue(OcrParams ocrParams)
        {
            return _ocrRequestQueue.Enqueue(
                _mockEngine.Object,
                image,
                CreateTempFile(),
                ocrParams,
                OcrPriority.Foreground,
                CancellationToken.None);
        }
        
        var ocrResult1 = await DoEnqueue(ocrParams1);
        Assert.Equal(expectedResult1, ocrResult1);
        var ocrResult2 = await DoEnqueue(ocrParams2);
        Assert.Equal(expectedResult2, ocrResult2);
        var ocrResult3 = await DoEnqueue(ocrParams3);
        Assert.Equal(expectedResult3, ocrResult3);
        var ocrResult4 = await DoEnqueue(ocrParams4);
        Assert.Equal(expectedResult4, ocrResult4);

        // Verify distinct engine calls
        _mockEngine.Verify(x => x.ProcessImage(It.IsAny<string>(), ocrParams1, It.IsAny<CancellationToken>()));
        _mockEngine.Verify(x => x.ProcessImage(It.IsAny<string>(), ocrParams2, It.IsAny<CancellationToken>()));
        _mockEngine.Verify(x => x.ProcessImage(It.IsAny<string>(), ocrParams3, It.IsAny<CancellationToken>()));
        _mockEngine.Verify(x => x.ProcessImage(It.IsAny<string>(), ocrParams4, It.IsAny<CancellationToken>()));
        _mockEngine.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnqueueWithEngineError()
    {
        var image = CreateScannedImage();
        var tempPath = CreateTempFile();
        var ocrParams = CreateOcrParams();
        _mockEngine.Setup(x => x.ProcessImage(tempPath, ocrParams, It.IsAny<CancellationToken>())).Throws<Exception>();

        var ocrResult =
            await _ocrRequestQueue.Enqueue(
                _mockEngine.Object,
                image,
                tempPath,
                ocrParams,
                OcrPriority.Foreground,
                CancellationToken.None);

        Assert.Null(ocrResult);
        _mockEngine.Verify(x => x.ProcessImage(tempPath, ocrParams, It.IsAny<CancellationToken>()));
        _mockEngine.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EnqueueTwiceWithTransientEngineError()
    {
        var image = CreateScannedImage();
        var tempPath1 = CreateTempFile();
        var tempPath2 = CreateTempFile();
        var ocrParams = CreateOcrParams();
        var expectedResult = CreateOcrResult();
        _mockEngine.Setup(x => x.ProcessImage(tempPath1, ocrParams, It.IsAny<CancellationToken>())).Throws<Exception>();
        _mockEngine.Setup(x => x.ProcessImage(tempPath2, ocrParams, It.IsAny<CancellationToken>())).Returns(expectedResult);

        var ocrResult1 =
            await _ocrRequestQueue.Enqueue(
                _mockEngine.Object,
                image,
                tempPath1,
                ocrParams,
                OcrPriority.Foreground,
                CancellationToken.None);

        var ocrResult2 =
            await _ocrRequestQueue.Enqueue(
                _mockEngine.Object,
                image,
                tempPath2,
                ocrParams,
                OcrPriority.Foreground,
                CancellationToken.None);

        Assert.Null(ocrResult1);
        Assert.Equal(expectedResult, ocrResult2);
        _mockEngine.Verify(x => x.ProcessImage(tempPath1, ocrParams, It.IsAny<CancellationToken>()));
        _mockEngine.Verify(x => x.ProcessImage(tempPath2, ocrParams, It.IsAny<CancellationToken>()));
        _mockEngine.VerifyNoOtherCalls();
    }

    private string CreateTempFile()
    {
        var path = Path.Combine(FolderPath, $"tempocr{Guid.NewGuid()}.jpg");
        File.WriteAllText(path, @"blah");
        return path;
    }

    private static OcrResult CreateOcrResult()
    {
        var uniqueElement = new OcrResultElement(Guid.NewGuid().ToString(), (0, 0, 1, 1));
        return new OcrResult((0, 0, 1, 1), new List<OcrResultElement> { uniqueElement }, false);
    }

    private static OcrParams CreateOcrParams()
    {
        return new OcrParams("eng", OcrMode.Fast, 10);
    }

    // TODO: Tests to add:
    // - Unsupported language code
    // - Many parallel tasks (more than worker threads - also # of worker threads should be configurable by the test)
    // - Can I break things by overloading task parallelization? I honestly don't remember why this is supposed to work...
    // - Priority (background vs foreground)
    // - Maybe we can parameterize some tests for background/foreground? Or maybe not necessary, priority tests are
    // probably enough.
    // - Cancellation
}