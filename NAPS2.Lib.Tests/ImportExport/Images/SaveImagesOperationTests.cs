using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using Xunit;

namespace NAPS2.Lib.Tests.ImportExport.Images;

public class SaveImagesOperationTests : ContextualTests
{
    private static readonly DateTime TestDate = new(2024, 3, 7, 21, 5, 2);

    private readonly IOverwritePrompt _overwritePrompt;
    private readonly NameManager _nameManager;

    public SaveImagesOperationTests()
    {
        _overwritePrompt = Substitute.For<IOverwritePrompt>();
        _nameManager = new NameManager();
    }

    private SaveImagesOperation CreateOperation() => new(_overwritePrompt, ImageContext, _nameManager);

    private bool Start(SaveImagesOperation op, string fileName, IList<ProcessedImage> images,
        ImageSettings imageSettings = null, OverwriteMode overwriteMode = OverwriteMode.Prompt, int? batchCount = null)
    {
        return op.Start(fileName, images, imageSettings ?? new ImageSettings(),
            new SaveParams { OverwriteMode = overwriteMode, BatchCount = batchCount, DateTime = TestDate });
    }

    private async Task<bool> Save(string fileName, IList<ProcessedImage> images,
        ImageSettings imageSettings = null, OverwriteMode overwriteMode = OverwriteMode.Prompt, int? batchCount = null)
    {
        var op = CreateOperation();
        Assert.True(Start(op, fileName, images, imageSettings, overwriteMode, batchCount));
        return await op.Success;
    }

    [Fact]
    public async Task SingleImage()
    {
        var op = CreateOperation();
        Assert.True(Start(op, P("test.jpg"), CreateScannedImages(ImageResources.dog)));
        Assert.True(await op.Success);

        Assert.Equal(P("test.jpg"), op.FirstFileSaved);
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.jpg")));
        Assert.Single(Folder.GetFiles());
    }

    [Fact]
    public async Task DatePlaceholders()
    {
        Assert.True(await Save(P("test_$(YYYY)-$(MM)-$(DD).jpg"), CreateScannedImages(ImageResources.dog)));

        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test_2024-03-07.jpg")));
    }

    [Fact]
    public async Task NumberPlaceholder_SkipsExisting()
    {
        File.WriteAllText(P("test1.jpg"), "blah");

        Assert.True(await Save(P("test$(n).jpg"), CreateScannedImages(ImageResources.dog)));

        Assert.Equal("blah", File.ReadAllText(P("test1.jpg")));
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test2.jpg")));
    }

    [Fact]
    public async Task MultipleImages()
    {
        var op = CreateOperation();
        Assert.True(Start(op, P("test.jpg"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray)));
        Assert.True(await op.Success);

        Assert.Equal(P("test.1.jpg"), op.FirstFileSaved);
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.1.jpg")));
        ImageAsserts.Similar(ImageResources.dog_gray, ImageContext.Load(P("test.2.jpg")));
        Assert.Equal(2, Folder.GetFiles().Length);
    }

    [Fact]
    public async Task MultipleImages_DigitsFromCount()
    {
        var images = Enumerable.Range(0, 10).Select(_ => CreateScannedImage()).ToList();

        Assert.True(await Save(P("test.png"), images));

        Assert.True(File.Exists(P("test.01.png")));
        Assert.True(File.Exists(P("test.10.png")));
        Assert.Equal(10, Folder.GetFiles().Length);
    }

    [Fact]
    public async Task MultipleImages_NumberPlaceholder()
    {
        Assert.True(await Save(P("test$(nn).jpg"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray)));

        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test01.jpg")));
        ImageAsserts.Similar(ImageResources.dog_gray, ImageContext.Load(P("test02.jpg")));
        Assert.Equal(2, Folder.GetFiles().Length);
    }

    [Fact]
    public async Task MultipleImages_SkipsExisting()
    {
        File.WriteAllText(P("test.1.jpg"), "blah");

        Assert.True(await Save(P("test.jpg"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray)));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        Assert.Equal("blah", File.ReadAllText(P("test.1.jpg")));
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.2.jpg")));
        ImageAsserts.Similar(ImageResources.dog_gray, ImageContext.Load(P("test.3.jpg")));
    }

    [Fact]
    public async Task Existing_PromptYes()
    {
        File.WriteAllText(P("test.jpg"), "blah");
        _overwritePrompt.ConfirmOverwrite(P("test.jpg")).Returns(OverwriteResponse.Yes);

        Assert.True(await Save(P("test.jpg"), CreateScannedImages(ImageResources.dog)));

        _overwritePrompt.Received().ConfirmOverwrite(P("test.jpg"));
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.jpg")));
    }

    [Fact]
    public async Task Existing_PromptNo()
    {
        File.WriteAllText(P("test.jpg"), "blah");
        _overwritePrompt.ConfirmOverwrite(P("test.jpg")).Returns(OverwriteResponse.No);

        Assert.False(await Save(P("test.jpg"), CreateScannedImages(ImageResources.dog)));

        Assert.Equal("blah", File.ReadAllText(P("test.jpg")));
    }

    [Fact]
    public async Task Existing_AlreadyConfirmed()
    {
        // e.g. the user already confirmed overwriting in the save dialog
        File.WriteAllText(P("test.jpg"), "blah");

        Assert.True(await Save(P("test.jpg"), CreateScannedImages(ImageResources.dog),
            overwriteMode: OverwriteMode.Confirmed));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.jpg")));
    }

    [Fact]
    public async Task Existing_Never()
    {
        File.WriteAllText(P("test.jpg"), "blah");

        Assert.True(await Save(P("test.jpg"), CreateScannedImages(ImageResources.dog),
            overwriteMode: OverwriteMode.Never));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        Assert.Equal("blah", File.ReadAllText(P("test.jpg")));
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.1.jpg")));
    }

    [Fact]
    public async Task Existing_Force()
    {
        File.WriteAllText(P("test.jpg"), "blah");

        Assert.True(await Save(P("test.jpg"), CreateScannedImages(ImageResources.dog),
            overwriteMode: OverwriteMode.Force));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.jpg")));
    }

    [Fact]
    public async Task MultipleImages_Force_OverwritesExisting()
    {
        File.WriteAllText(P("test1.jpg"), "blah");
        File.WriteAllText(P("test2.jpg"), "blah");

        Assert.True(await Save(P("test$(n).jpg"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray),
            overwriteMode: OverwriteMode.Force));

        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test1.jpg")));
        ImageAsserts.Similar(ImageResources.dog_gray, ImageContext.Load(P("test2.jpg")));
        Assert.Equal(2, Folder.GetFiles().Length);
    }

    [Fact]
    public async Task BatchCount_SingleFileIsNumbered()
    {
        Assert.True(await Save(P("test.jpg"), CreateScannedImages(ImageResources.dog), batchCount: 2));
        Assert.True(await Save(P("test.jpg"), CreateScannedImages(ImageResources.dog_gray), batchCount: 2));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.1.jpg")));
        ImageAsserts.Similar(ImageResources.dog_gray, ImageContext.Load(P("test.2.jpg")));
        Assert.Equal(2, Folder.GetFiles().Length);
    }

    [Fact]
    public async Task BatchCount_PadsDigits()
    {
        Assert.True(await Save(P("test.jpg"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray),
            batchCount: 10));

        Assert.True(File.Exists(P("test.01.jpg")));
        Assert.True(File.Exists(P("test.02.jpg")));
    }

    [Fact]
    public async Task Concurrent_NumberPlaceholder_ChoosesDistinctFiles()
    {
        var op1 = CreateOperation();
        Assert.True(Start(op1, P("test$(n).jpg"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray)));
        var op2 = CreateOperation();
        Assert.True(Start(op2, P("test$(n).jpg"), CreateScannedImages(ImageResources.dog_bw)));
        Assert.True(await op1.Success);
        Assert.True(await op2.Success);

        Assert.Equal(P("test1.jpg"), op1.FirstFileSaved);
        Assert.Equal(P("test3.jpg"), op2.FirstFileSaved);
        Assert.Equal(3, Folder.GetFiles().Length);
    }

    [PlatformFact(exclude: PlatformFlags.ImageSharpImage)]
    public async Task MultiPageTiff()
    {
        Assert.True(await Save(P("test.tiff"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray)));

        Assert.Single(Folder.GetFiles());
        var frames = await ImageContext.LoadFrames(P("test.tiff")).ToListAsync();
        Assert.Equal(2, frames.Count);
    }

    [PlatformFact(exclude: PlatformFlags.ImageSharpImage)]
    public async Task MultiPageTiff_Existing_PromptNo()
    {
        File.WriteAllText(P("test.tiff"), "blah");
        _overwritePrompt.ConfirmOverwrite(P("test.tiff")).Returns(OverwriteResponse.No);

        Assert.False(await Save(P("test.tiff"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray)));

        Assert.Equal("blah", File.ReadAllText(P("test.tiff")));
    }

    [Fact]
    public async Task SinglePageTiff()
    {
        Assert.True(await Save(P("test.tiff"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray),
            new ImageSettings { SinglePageTiff = true }));

        Assert.True(File.Exists(P("test.1.tiff")));
        Assert.True(File.Exists(P("test.2.tiff")));
        Assert.Equal(2, Folder.GetFiles().Length);
    }

    [Fact]
    public async Task Directory_UsesNumberedFileName()
    {
        File.WriteAllText(P("1.jpg"), "blah");

        Assert.True(await Save(FolderPath, CreateScannedImages(ImageResources.dog)));

        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("2.jpg")));
    }

    [Fact]
    public async Task UnknownExtension_Fails()
    {
        Assert.False(await Save(P("test.xyz"), CreateScannedImages(ImageResources.dog)));

        Assert.Empty(Folder.GetFiles());
    }
}
