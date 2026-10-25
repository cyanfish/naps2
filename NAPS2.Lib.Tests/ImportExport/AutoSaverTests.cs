using NAPS2.EtoForms;
using NAPS2.EtoForms.Notifications;
using NAPS2.ImportExport;
using NAPS2.Pdf;
using NAPS2.Scan;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using PdfSharpCore.Pdf.IO;
using Xunit;

namespace NAPS2.Lib.Tests.ImportExport;

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
            new UiImageList(),
            new NameManager()
        );
    }

    [Fact]
    public async Task NoPages()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).jpg")
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
            FilePath = P("test$(n).pdf")
        };

        var scanned = CreateScannedImages(ImageResources.dog);
        var output = await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        Assert.Single(output);
        Assert.False(IsDisposed(output[0]));
        Assert.True(IsDisposed(scanned[0]));
        Assert.Single(Folder.GetFiles());
        PdfAsserts.AssertImages(P("test1.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task SingleJpeg()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).jpg")
        };

        var scanned = CreateScannedImages(ImageResources.dog);
        var output = await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        Assert.Single(output);
        Assert.False(IsDisposed(output[0]));
        Assert.True(IsDisposed(scanned[0]));
        Assert.Single(Folder.GetFiles());
        ImageAsserts.Similar(ImageResources.dog, P("test1.jpg"));
    }

    [Fact]
    public async Task PdfFilePerScan()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).pdf"),
            Separator = SaveSeparator.FilePerScan
        };
        var scanned = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray).ToAsyncEnumerable();

        var output = _autoSaver.Save(settings, scanned);

        Assert.Equal(2, (await output.ToListAsync()).Count);
        Assert.Single(Folder.GetFiles());
        PdfAsserts.AssertImages(P("test1.pdf"),
            ImageResources.dog, ImageResources.dog_gray);
    }

    [Fact]
    public async Task JpegFilePerScan()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).jpg"),
            Separator = SaveSeparator.FilePerScan
        };
        var scanned = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray).ToAsyncEnumerable();

        var output = _autoSaver.Save(settings, scanned);

        // Jpeg can't store multiple pages so we split anyway
        Assert.Equal(2, (await output.ToListAsync()).Count);
        Assert.Equal(2, Folder.GetFiles().Length);
        ImageAsserts.Similar(ImageResources.dog, P("test1.jpg"));
        ImageAsserts.Similar(ImageResources.dog_gray, P("test2.jpg"));
    }

    [Fact]
    public async Task JpegFilePerPage()
    {
        File.WriteAllText(P("test1.jpg"), "blah");
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).jpg"),
            Separator = SaveSeparator.FilePerPage
        };
        var scanned = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray).ToAsyncEnumerable();

        var output = _autoSaver.Save(settings, scanned);

        Assert.Equal(2, (await output.ToListAsync()).Count);
        Assert.Equal("blah", File.ReadAllText(P("test1.jpg")));
        ImageAsserts.Similar(ImageResources.dog, P("test2.jpg"));
        ImageAsserts.Similar(ImageResources.dog_gray, P("test3.jpg"));
    }

    [PlatformFact(exclude: PlatformFlags.ImageSharpImage)]
    public async Task TiffFilePerScan()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).tiff"),
            Separator = SaveSeparator.FilePerScan
        };
        var scanned = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray).ToAsyncEnumerable();

        var output = _autoSaver.Save(settings, scanned);

        Assert.Equal(2, (await output.ToListAsync()).Count);
        Assert.Single(Folder.GetFiles());
        var frames = await ImageContext.LoadFrames(P("test1.tiff")).ToListAsync();
        Assert.Equal(2, frames.Count);
        ImageAsserts.Similar(ImageResources.dog, frames[0], ignoreResolution: true);
        ImageAsserts.Similar(ImageResources.dog_gray, frames[1], ignoreResolution: true);
    }

    [Fact]
    public async Task PdfFilePerPage()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).pdf"),
            Separator = SaveSeparator.FilePerPage
        };

        var scanned = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray).ToAsyncEnumerable();
        var output = _autoSaver.Save(settings, scanned);
        Assert.Equal(2, (await output.ToListAsync()).Count);
        Assert.Equal(2, Folder.GetFiles().Length);
        PdfAsserts.AssertImages(P("test1.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test2.pdf"), ImageResources.dog_gray);
    }

    [Fact]
    public async Task PdfFilePerPage_SkipsExisting()
    {
        File.WriteAllText(P("test1.pdf"), "blah");
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).pdf"),
            Separator = SaveSeparator.FilePerPage
        };

        var scanned = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray).ToAsyncEnumerable();
        var output = _autoSaver.Save(settings, scanned);
        Assert.Equal(2, (await output.ToListAsync()).Count);
        Assert.Equal("blah", File.ReadAllText(P("test1.pdf")));
        PdfAsserts.AssertImages(P("test2.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test3.pdf"), ImageResources.dog_gray);
    }

    [Fact]
    public async Task PdfFilePerPage_NoPlaceholder()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test.pdf"),
            Separator = SaveSeparator.FilePerPage
        };

        var scanned = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray).ToAsyncEnumerable();
        var output = _autoSaver.Save(settings, scanned);
        Assert.Equal(2, (await output.ToListAsync()).Count);
        Assert.Equal(2, Folder.GetFiles().Length);
        PdfAsserts.AssertImages(P("test.1.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test.2.pdf"), ImageResources.dog_gray);
    }

    [Fact]
    public async Task PdfFilePerPage_NoPlaceholder_ContinuesSequence()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test.pdf"),
            Separator = SaveSeparator.FilePerPage
        };

        var scanned = Enumerable.Range(0, 10).Select(_ => CreateScannedImage()).ToList();
        await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        // Numbers aren't padded to the batch size so the next save can continue the sequence
        Assert.Equal(10, Folder.GetFiles().Length);
        Assert.True(File.Exists(P("test.1.pdf")));
        Assert.True(File.Exists(P("test.10.pdf")));

        scanned = CreateScannedImages(ImageResources.dog, ImageResources.dog_gray);
        await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        Assert.Equal(12, Folder.GetFiles().Length);
        PdfAsserts.AssertImages(P("test.11.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test.12.pdf"), ImageResources.dog_gray);
    }

    [Fact]
    public async Task PdfNoPlaceholder_Existing()
    {
        File.WriteAllText(P("test.pdf"), "blah");
        File.WriteAllText(P("test.1.pdf"), "blah");
        var settings = new AutoSaveSettings
        {
            FilePath = P("test.pdf")
        };

        var scanned = CreateScannedImages(ImageResources.dog);
        await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        Assert.Equal("blah", File.ReadAllText(P("test.pdf")));
        Assert.Equal("blah", File.ReadAllText(P("test.1.pdf")));
        PdfAsserts.AssertImages(P("test.2.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task JpegNoPlaceholder_Existing()
    {
        File.WriteAllText(P("test.jpg"), "blah");
        var settings = new AutoSaveSettings
        {
            FilePath = P("test.jpg")
        };

        var scanned = CreateScannedImages(ImageResources.dog);
        await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        Assert.Equal("blah", File.ReadAllText(P("test.jpg")));
        ImageAsserts.Similar(ImageResources.dog, P("test.1.jpg"));
    }

    [Fact]
    public async Task DatePlaceholders()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test_$(YYYY).pdf")
        };

        var scanned = CreateScannedImages(ImageResources.dog);
        await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        PdfAsserts.AssertImages(P($"test_{DateTime.Now:yyyy}.1.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task PdfNoPlaceholder_FirstFileIsNumbered()
    {
        // Auto save is a sequence of saves to the same name, so even the first file is numbered
        var settings = new AutoSaveSettings
        {
            FilePath = P("test.pdf")
        };

        await _autoSaver.Save(settings, CreateScannedImages(ImageResources.dog).ToAsyncEnumerable()).ToListAsync();
        await _autoSaver.Save(settings, CreateScannedImages(ImageResources.dog_gray).ToAsyncEnumerable())
            .ToListAsync();

        Assert.Equal(2, Folder.GetFiles().Length);
        PdfAsserts.AssertImages(P("test.1.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test.2.pdf"), ImageResources.dog_gray);
    }

    [Fact]
    public async Task ConcurrentPdfs()
    {
        // The first file isn't written until OCR finishes, so the second save can't rely on the file existing
        SetUpFakeOcr(ifNoMatch: "hello", delay: 500);
        _config.Run.Set(c => c.EnableOcr, true);
        _config.Run.Set(c => c.OcrLanguageCode, "eng");
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).pdf"),
            Separator = SaveSeparator.FilePerScan
        };

        var save1 = _autoSaver.Save(settings,
            CreateScannedImages(ImageResources.dog, ImageResources.dog_gray).ToAsyncEnumerable()).ToListAsync();
        var save2 = _autoSaver.Save(settings,
            CreateScannedImages(ImageResources.dog_bw).ToAsyncEnumerable()).ToListAsync();
        await save1;
        await save2;

        // Either save may get the first number depending on timing, but neither should overwrite the other
        Assert.Equal(2, Folder.GetFiles().Length);
        var (twoPagePath, onePagePath) = PdfReader.Open(P("test1.pdf"), PdfDocumentOpenMode.InformationOnly)
            .PageCount == 2
            ? (P("test1.pdf"), P("test2.pdf"))
            : (P("test2.pdf"), P("test1.pdf"));
        PdfAsserts.AssertImages(twoPagePath, ImageResources.dog, ImageResources.dog_gray);
        PdfAsserts.AssertImages(onePagePath, ImageResources.dog_bw);
    }

    [Fact]
    public async Task PdfSplitPatchT()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).pdf"),
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
        PdfAsserts.AssertImages(P("test1.pdf"), ImageResources.dog, ImageResources.dog_gray);
        PdfAsserts.AssertImages(P("test2.pdf"), ImageResources.dog_h_n300);
    }

    [Fact]
    public async Task PromptForFilePath()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test_a_$(n).pdf"),
            PromptForFilePath = true
        };
        _dialogHelper.PromptToSavePdfOrImage(Arg.Any<string>(), out Arg.Any<string>()).Returns(x =>
        {
            x[1] = P("test_b_$(n).pdf");
            return true;
        });

        var scanned = CreateScannedImages(ImageResources.dog);
        var output = await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        Assert.Single(output);
        Assert.False(IsDisposed(output[0]));
        Assert.True(IsDisposed(scanned[0]));
        Assert.Single(Folder.GetFiles());
        PdfAsserts.AssertImages(P("test_b_1.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task PromptForFilePath_DefaultPathIgnoresExisting()
    {
        File.WriteAllText(P("test.pdf"), "blah");
        var settings = new AutoSaveSettings
        {
            FilePath = P("test.pdf"),
            PromptForFilePath = true
        };
        string defaultPath = null;
        _dialogHelper.PromptToSavePdfOrImage(Arg.Any<string>(), out Arg.Any<string>()).Returns(x =>
        {
            defaultPath = (string) x[0];
            x[1] = P("other.pdf");
            return true;
        });

        var scanned = CreateScannedImages(ImageResources.dog);
        await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        Assert.Equal(P("test.pdf"), defaultPath);
        PdfAsserts.AssertImages(P("other.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task PromptForFilePath_FilePerPage()
    {
        // The user chooses each file name, so the chosen names aren't numbered
        var settings = new AutoSaveSettings
        {
            FilePath = P("test.pdf"),
            Separator = SaveSeparator.FilePerPage,
            PromptForFilePath = true
        };
        var dialogPaths = new Queue<string>([P("first.pdf"), P("second.pdf")]);
        _dialogHelper.PromptToSavePdfOrImage(Arg.Any<string>(), out Arg.Any<string>()).Returns(x =>
        {
            x[1] = dialogPaths.Dequeue();
            return true;
        });

        var scanned = CreateScannedImages(ImageResources.dog, ImageResources.dog_gray);
        await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        Assert.Equal(2, Folder.GetFiles().Length);
        PdfAsserts.AssertImages(P("first.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("second.pdf"), ImageResources.dog_gray);
    }

    [Fact]
    public async Task PromptForFilePath_Existing()
    {
        File.WriteAllText(P("test.pdf"), "blah");
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).pdf"),
            PromptForFilePath = true
        };
        _dialogHelper.PromptToSavePdfOrImage(Arg.Any<string>(), out Arg.Any<string>()).Returns(x =>
        {
            x[1] = P("test.pdf");
            return true;
        });

        var scanned = CreateScannedImages(ImageResources.dog);
        await _autoSaver.Save(settings, scanned.ToAsyncEnumerable()).ToListAsync();

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        Assert.Single(Folder.GetFiles());
        PdfAsserts.AssertImages(P("test.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task CancelPromptForFilePath()
    {
        var settings = new AutoSaveSettings
        {
            FilePath = P("test$(n).pdf"),
            PromptForFilePath = true
        };
        _dialogHelper.PromptToSavePdfOrImage(Arg.Any<string>(), out Arg.Any<string>()).Returns(x =>
        {
            x[1] = P("test$(n).pdf");
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
    //         FilePath = P("test$(n).pdf"),
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
    //         FilePath = P("test$(n).jpg")
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
    //         FilePath = P("test$(n).jpg"),
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