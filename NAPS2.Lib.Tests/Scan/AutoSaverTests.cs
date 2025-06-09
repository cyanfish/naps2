using NAPS2.EtoForms;
using NAPS2.EtoForms.Notifications;
using NAPS2.ImportExport;
using NAPS2.Pdf;
using NAPS2.Scan;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using Xunit;

namespace NAPS2.Lib.Tests.Scan;

public class AutoSaverTests : ContextualTests
{
    private readonly AutoSaver _autoSaver;

    private readonly ErrorOutput _errorOutput;
    private readonly DialogHelper _dialogHelper;
    private readonly OperationProgress _operationProgress;
    private readonly ISaveNotify _saveNotify;
    private readonly IOverwritePrompt _overwritePrompt;
    private readonly Naps2Config _config;

    public AutoSaverTests()
    {
        _errorOutput = Substitute.For<ErrorOutput>();
        _dialogHelper = Substitute.For<DialogHelper>();
        _operationProgress = Substitute.For<OperationProgress>();
        _saveNotify = Substitute.For<ISaveNotify>();
        _overwritePrompt = Substitute.For<IOverwritePrompt>();
        _config = Naps2Config.Stub();
        _autoSaver = new AutoSaver(
            _errorOutput,
            _dialogHelper,
            _operationProgress,
            _saveNotify,
            new PdfExporter(ScanningContext),
            _overwritePrompt,
            _config,
            ImageContext,
            new UiImageList()
        );
    }

    [Fact]
    public async Task NoPages()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = Path.Combine(FolderPath, "test$(n).jpg")
        };
        var scanned = CreateScannedImages().ToAsyncEnumerable();

        var output = _autoSaver.Save(settings, scanned);

        Assert.Empty(await output.ToListAsync());
        Assert.Empty(Folder.GetFiles());
    }

    [Fact]
    public async Task SinglePdf()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = Path.Combine(FolderPath, "test$(n).pdf")
        };

        var scanned = CreateScannedImages(ImageResources.dog);
        var output = await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        Assert.Single(output);
        Assert.False(IsDisposed(output[0]));
        Assert.True(IsDisposed(scanned[0]));
        Assert.Single(Folder.GetFiles());
        PdfAsserts.AssertImages(Path.Combine(FolderPath, "test1.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task SingleJpeg()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = Path.Combine(FolderPath, "test$(n).jpg")
        };

        var scanned = CreateScannedImages(ImageResources.dog);
        var output = await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        Assert.Single(output);
        Assert.False(IsDisposed(output[0]));
        Assert.True(IsDisposed(scanned[0]));
        Assert.Single(Folder.GetFiles());
        ImageAsserts.Similar(ImageResources.dog, Path.Combine(FolderPath, "test1.jpg"));
    }

    [Fact]
    public async Task PdfFilePerScan()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = Path.Combine(FolderPath, "test$(n).pdf"),
            Separator = SaveSeparator.FilePerScan
        };
        var scanned = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray).ToAsyncEnumerable();

        var output = _autoSaver.Save(settings, scanned);

        Assert.Equal(2, (await output.ToListAsync()).Count);
        Assert.Single(Folder.GetFiles());
        PdfAsserts.AssertImages(Path.Combine(FolderPath, "test1.pdf"),
            ImageResources.dog, ImageResources.dog_gray);
    }

    // TODO: Fix
    // [Fact]
    // public async Task JpegFilePerScan()
    // {
    //     var settings = new AutoSaveSettings
    //     {
    //         FilePath = Path.Combine(FolderPath, "test$(n).jpg"),
    //         Separator = SaveSeparator.FilePerScan
    //     };
    //     var scanned = CreateScannedImages(
    //         ImageResources.dog,
    //         ImageResources.dog_gray).ToAsyncEnumerable();
    //
    //     var output = _autoSaver.Save(settings, scanned);
    //
    //     // Jpeg can't store multiple pages so we split anyway
    //     Assert.Equal(2, (await output.ToListAsync()).Count);
    //     Assert.Equal(2, Folder.GetFiles().Length);
    //     ImageAsserts.Similar(ImageResources.dog, Path.Combine(FolderPath, "test1.jpg"));
    //     ImageAsserts.Similar(ImageResources.dog_gray, Path.Combine(FolderPath, "test2.jpg"));
    // }

    [PlatformFact(exclude: PlatformFlags.ImageSharpImage)]
    public async Task TiffFilePerScan()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = Path.Combine(FolderPath, "test$(n).tiff"),
            Separator = SaveSeparator.FilePerScan
        };
        var scanned = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray).ToAsyncEnumerable();

        var output = _autoSaver.Save(settings, scanned);

        Assert.Equal(2, (await output.ToListAsync()).Count);
        Assert.Single(Folder.GetFiles());
        var frames = await ImageContext.LoadFrames(Path.Combine(FolderPath, "test1.tiff")).ToListAsync();
        Assert.Equal(2, frames.Count);
        ImageAsserts.Similar(ImageResources.dog, frames[0], ignoreResolution: true);
        ImageAsserts.Similar(ImageResources.dog_gray, frames[1], ignoreResolution: true);
    }

    [Fact]
    public async Task PdfFilePerPage()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = Path.Combine(FolderPath, "test$(n).pdf"),
            Separator = SaveSeparator.FilePerPage
        };

        var scanned = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray).ToAsyncEnumerable();
        var output = _autoSaver.Save(settings, scanned);
        Assert.Equal(2, (await output.ToListAsync()).Count);
        Assert.Equal(2, Folder.GetFiles().Length);
        PdfAsserts.AssertImages(Path.Combine(FolderPath, "test1.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(Path.Combine(FolderPath, "test2.pdf"), ImageResources.dog_gray);
    }

    [Fact]
    public async Task PdfSplitPatchT()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = Path.Combine(FolderPath, "test$(n).pdf"),
            Separator = SaveSeparator.PatchT
        };
        var scanned = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray,
            ImageResources.patcht,
            ImageResources.dog_h_n300);
        scanned[2] = scanned[2].WithPostProcessingData(scanned[2].PostProcessingData with
        {
            Barcode = new Barcode(true, true, "PATCHT")
        }, true);
        var output = _autoSaver.Save(settings, scanned.ToAsyncEnumerable());
        Assert.Equal(4, (await output.ToListAsync()).Count);
        Assert.Equal(2, Folder.GetFiles().Length);
        PdfAsserts.AssertImages(Path.Combine(FolderPath, "test1.pdf"), ImageResources.dog, ImageResources.dog_gray);
        PdfAsserts.AssertImages(Path.Combine(FolderPath, "test2.pdf"), ImageResources.dog_h_n300);
    }

    [Fact]
    public async Task PromptForFilePath()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = Path.Combine(FolderPath, "test_a_$(n).pdf"),
            PromptForFilePath = true
        };
        _dialogHelper.PromptToSavePdfOrImage(Arg.Any<string>(), out Arg.Any<string>()).Returns(x =>
        {
            x[1] = Path.Combine(FolderPath, "test_b_$(n).pdf");
            return true;
        });

        var scanned = CreateScannedImages(ImageResources.dog);
        var output = await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        Assert.Single(output);
        Assert.False(IsDisposed(output[0]));
        Assert.True(IsDisposed(scanned[0]));
        Assert.Single(Folder.GetFiles());
        PdfAsserts.AssertImages(Path.Combine(FolderPath, "test_b_1.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task CancelPromptForFilePath()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = Path.Combine(FolderPath, "test$(n).pdf"),
            PromptForFilePath = true
        };
        _dialogHelper.PromptToSavePdfOrImage(Arg.Any<string>(), out Arg.Any<string>()).Returns(x =>
        {
            x[1] = Path.Combine(FolderPath, "test$(n).pdf");
            return false;
        });

        var scanned = CreateScannedImages(ImageResources.dog);
        var output = await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        Assert.Single(output);
        Assert.False(IsDisposed(output[0]));
        Assert.True(IsDisposed(scanned[0]));
        Assert.Empty(Folder.GetFiles());
    }

    // TODO: Finish out tests

    //
    // [Fact]
    // public async Task TwoImagesTwoPdfs()
    // {
    //     var errorOutput = Substitute.For<ErrorOutput>();
    //     var driver = Driver(errorOutput.Object, 2);
    //
    //     var scanProfile = Profile(new AutoSaveSettings
    //     {
    //         FilePath = Path.Combine(FolderPath, "test$(n).pdf"),
    //         Separator = SaveSeparator.FilePerPage
    //     });
    //     var scanParams = new ScanParams();
    //     var scannedImages = await driver.Scan(scanProfile, scanParams).ToList();
    //     var files = Folder.GetFiles();
    //
    //     Assert.Equal(2, scannedImages.Count);
    //     Assert.Equal(2, files.Length);
    //     PdfAsserts.AssertPageCount(1, files[0].FullName);
    //     PdfAsserts.AssertPageCount(1, files[1].FullName);
    //     errorOutput.VerifyNoOtherCalls();
    // }
    //
    // [Fact]
    // public async Task TwoImagesTwoJpegs()
    // {
    //     var errorOutput = Substitute.For<ErrorOutput>();
    //     var driver = Driver(errorOutput.Object, 2);
    //
    //     var scanProfile = Profile(new AutoSaveSettings
    //     {
    //         FilePath = Path.Combine(FolderPath, "test$(n).jpg")
    //     });
    //     var scanParams = new ScanParams();
    //     var scannedImages = await driver.Scan(scanProfile, scanParams).ToList();
    //     var files = Folder.GetFiles();
    //
    //     Assert.Equal(2, scannedImages.Count);
    //     Assert.Equal(2, files.Length);
    //     errorOutput.VerifyNoOtherCalls();
    // }
    //
    // [Fact]
    // public async Task ClearAfterSaving()
    // {
    //     var errorOutput = Substitute.For<ErrorOutput>();
    //     var driver = Driver(errorOutput.Object, 2);
    //
    //     var scanProfile = Profile(new AutoSaveSettings
    //     {
    //         FilePath = Path.Combine(FolderPath, "test$(n).jpg"),
    //         ClearImagesAfterSaving = true
    //     });
    //     var scanParams = new ScanParams();
    //     var scannedImages = await driver.Scan(scanProfile, scanParams).ToList();
    //     var files = Folder.GetFiles();
    //
    //     Assert.Empty(scannedImages);
    //     Assert.Equal(2, files.Length);
    //     errorOutput.VerifyNoOtherCalls();
    // }
    //
    // // TODO: ClearAfterSaving with error, PromptForFilePath, SaveSeparator
    //
    // private ScanDevice Device => new ScanDevice("test_id", "test_name");
    //
    // private MockScanDriver Driver(ErrorOutput errorOutput, int images) =>
    //     new MockScanDriver(errorOutput, CreateAutoSaver(errorOutput))
    //     {
    //         MockDevices = new List<ScanDevice> { Device },
    //         MockOutput = Enumerable.Range(0, images).Select(i => CreateScannedImage()).ToList()
    //     };
    //
    // private ScanProfile Profile(AutoSaveSettings autoSaveSettings) => new ScanProfile
    // {
    //     Device = Device,
    //     EnableAutoSave = true,
    //     AutoSaveSettings = autoSaveSettings
    // };
    //
    // private AutoSaver CreateAutoSaver(ErrorOutput errorOutput)
    // {
    //     return new AutoSaver(
    //         new StubConfigProvider<PdfSettings>(new PdfSettings()),
    //         new StubConfigProvider<ImageSettings>(new ImageSettings()),
    //         new OcrEngineManager(),
    //         new OcrRequestQueue(new OcrEngineManager(), new StubOperationProgress()),
    //         errorOutput,
    //         new StubDialogHelper(),
    //         new StubOperationProgress(),
    //         null,
    //         new PdfSharpExporter(new MemoryStreamRenderer(ImageContext)),
    //         new StubOverwritePrompt(),
    //         new BitmapRenderer(ImageContext),
    //         new StubConfigProvider<CommonConfig>(InternalDefaults.GetCommonConfig()));
    // }
}