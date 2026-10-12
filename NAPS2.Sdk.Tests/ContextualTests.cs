using System.Collections.Immutable;
using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Unmanaged;
using NSubstitute;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests;

public class ContextualTests : IDisposable
{
    public ContextualTests()
    {
        FolderPath = Path.GetFullPath(Path.Combine("naps2_test_temp", Path.GetRandomFileName()));
        Folder = Directory.CreateDirectory(FolderPath);

        ImageContext = TestImageContextFactory.Get();
        ScanningContext = new ScanningContext(ImageContext);
        ScanningContext.TempFolderPath = Path.Combine(FolderPath, "temp");
        Directory.CreateDirectory(ScanningContext.TempFolderPath);
    }

    public ContextualTests(ITestOutputHelper testOutputHelper)
        : this()
    {
        ScanningContext.Logger = new TestLogger(testOutputHelper);
    }

    public ImageContext ImageContext { get; }

    // TODO: We can probably do some processed image lifecycle checking by ensuring the scanning context has no
    // registered images after running a test.
    public ScanningContext ScanningContext { get; }

    public ILogger Logger => ScanningContext.Logger;

    public string FolderPath { get; }

    public DirectoryInfo Folder { get; }

    public IMemoryImage LoadImage(byte[] resource)
    {
        return ImageContext.Load(new MemoryStream(resource));
    }

    public ProcessedImage CreateScannedImage()
    {
        return ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
    }

    public List<ProcessedImage> CreateScannedImages(params byte[][] images)
    {
        return images.Select(image => ScanningContext.CreateProcessedImage(LoadImage(image))).ToList();
    }

    public void SetUpFileStorage()
    {
        ScanningContext.RecoveryPath = Path.Combine(FolderPath, "recovery");
        ScanningContext.FileStorageManager = FileStorageManager.CreateFolder(ScanningContext.RecoveryPath);
    }

    public void SetUpOcr()
    {
        var best = Path.Combine(FolderPath, "best");
        Directory.CreateDirectory(best);
        var fast = Path.Combine(FolderPath, "fast");
        Directory.CreateDirectory(fast);

        var depsRoot = Environment.GetEnvironmentVariable("NAPS2_TEST_DEPS");
        var tesseractPath = NativeLibrary.FindExePath(PlatformCompat.System.TesseractExecutableName, depsRoot);
        CopyResourceToFile(BinaryResources.eng_traineddata, fast, "eng.traineddata");
        CopyResourceToFile(BinaryResources.heb_traineddata, fast, "heb.traineddata");
        ScanningContext.OcrEngine = TesseractOcrEngine.CustomWithModes(tesseractPath, FolderPath);
    }

    public void SetUpFakeOcr(Dictionary<IMemoryImage, string> ocrTextByImage = null, string ifNoMatch = null, int delay = 200,
        bool splitWords = false)
    {
        var ocrMock = Substitute.For<IOcrEngine>();
        ocrMock.ProcessImage(ScanningContext, Arg.Any<string>(), Arg.Any<OcrParams>(), Arg.Any<CancellationToken>())
            .Returns(
                async x =>
                {
                    var path = (string) x[1]!;
                    var ocrParams = (OcrParams) x[2]!;
                    var ocrImage = ImageContext.Load(path);
                    await Task.Delay(delay);

                    OcrResult CreateOcrResult(string text) =>
                        CreateFakeOcrResult(text, ocrParams.LanguageCode!, splitWords);

                    if (ocrTextByImage != null)
                    {
                        // Lock so we don't try to access images simultaneously
                        lock (ocrTextByImage)
                        {
                            foreach (var image in ocrTextByImage.Keys)
                            {
                                if (ImageAsserts.IsSimilar(image, ocrImage))
                                {
                                    return CreateOcrResult(ocrTextByImage[image]);
                                }
                            }
                        }
                    }
                    if (ifNoMatch != null)
                    {
                        return CreateOcrResult(ifNoMatch);
                    }
                    return null;
                });
        ScanningContext.OcrEngine = ocrMock;
    }

    /// <summary>
    /// Creates the OCR result used by SetUpFakeOcr, with page bounds of 100x100. If splitWords is true, the text is
    /// split into lines (by "\n") and words (by " "), with each word's width proportional to its length.
    /// Otherwise, the whole text is a single word.
    /// </summary>
    public static OcrResult CreateFakeOcrResult(string text, string languageCode, bool splitWords)
    {
        if (splitWords)
        {
            var allWords = new List<OcrResultElement>();
            var lines = new List<OcrResultElement>();
            int y = 10;
            foreach (var lineText in text.Split('\n'))
            {
                var words = new List<OcrResultElement>();
                int x = 10;
                foreach (var wordText in lineText.Split(' '))
                {
                    // Width proportional to the word length, with a gap between words
                    int w = wordText.Length * 2;
                    words.Add(new OcrResultElement(wordText, languageCode, false,
                        (x, y, w, 10), y + 10, 10, ImmutableList<OcrResultElement>.Empty));
                    x += w + 2;
                }
                lines.Add(new OcrResultElement(lineText, languageCode, false,
                    (10, y, x - 12, 10), y + 10, 10, words.ToImmutableList()));
                allWords.AddRange(words);
                y += 15;
            }
            return new((0, 0, 100, 100), allWords.ToImmutableList(), lines.ToImmutableList());
        }
        var word = new OcrResultElement(text, languageCode, false,
            (10, 10, 10, 10), 20, 10, ImmutableList<OcrResultElement>.Empty);
        var line = new OcrResultElement(text, languageCode, false,
            (10, 10, 10, 10), 20, 10, ImmutableList.Create(word));
        var list = ImmutableList.Create(line);
        return new((0, 0, 100, 100), list, list);
    }

    public string CopyResourceToFile(byte[] resource, string folder, string fileName)
    {
        string path = Path.Combine(folder, fileName);
        File.WriteAllBytes(path, resource);
        return path;
    }

    public string CopyResourceToFile(byte[] resource, string fileName)
    {
        return CopyResourceToFile(resource, FolderPath, fileName);
    }

    public virtual void Dispose()
    {
        ScanningContext.Dispose();
        try
        {
            Directory.Delete(FolderPath, true);
        }
        catch (IOException)
        {
            Thread.Sleep(100);
            try
            {
                Directory.Delete(FolderPath, true);
            }
            catch (IOException)
            {
            }
        }
    }

    public bool IsDisposed(ProcessedImage image)
    {
        try
        {
            using var image2 = image.Clone();
            return false;
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }

    public bool IsDisposed(IMemoryImage image)
    {
        try
        {
            using var image2 = image.Clone();
            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }

    private class TestLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestLogger(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // We don't log tracing to the test output as it fills up the limited output space too quickly
            if (logLevel == LogLevel.Trace) return;

            try
            {
                _testOutputHelper.WriteLine(state.ToString());
                if (exception != null)
                {
                    _testOutputHelper.WriteLine(exception.ToString());
                }
            }
            catch (InvalidOperationException)
            {
                // Background work (e.g. timers) can log after the test has finished, which xunit doesn't allow
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}