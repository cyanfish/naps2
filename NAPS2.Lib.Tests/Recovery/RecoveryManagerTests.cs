using System.Threading;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using Xunit;

namespace NAPS2.Lib.Tests.Recovery;

public class RecoveryManagerTests : ContextualTests
{
    private readonly RecoveryManager _recoveryManager;
    private readonly string _recoveryPath;

    public RecoveryManagerTests()
    {
        SetUpFileStorage();
        _recoveryManager = new RecoveryManager(ScanningContext);
        _recoveryPath = Path.Combine(ScanningContext.RecoveryPath!, Path.GetRandomFileName());
    }

    [Fact]
    public void NoFoldersAvailable()
    {
        var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.Null(folder);
    }

    [Fact]
    public void FolderWithNoImages()
    {
        CreateFolderToRecoverFrom(_recoveryPath, []);

        var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.Null(folder);
    }

    [Fact]
    public void FolderLocking()
    {
        CreateFolderToRecoverFrom(_recoveryPath, ["a"]);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);

        using var folder2 = _recoveryManager.GetLatestRecoverableFolder();
        Assert.Null(folder2);

        folder.Dispose();
        using var folder3 = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder3);
    }

    [Fact]
    public void FindSingleFolder()
    {
        CreateFolderToRecoverFrom(_recoveryPath, ["a"]);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        Assert.Equal(1, folder.ImageCount);
        DateAsserts.Recent(TimeSpan.FromMilliseconds(100), folder.ScannedDateTime);
    }

    [Fact]
    public void DeleteFolder()
    {
        CreateFolderToRecoverFrom(_recoveryPath, ["a"]);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        Assert.True(Directory.Exists(_recoveryPath));
        folder.TryDelete();
        Assert.False(Directory.Exists(_recoveryPath));
    }

    [Fact]
    public void Recover()
    {
        CreateFolderToRecoverFrom(_recoveryPath, ["a", "b"]);

        var images = new List<ProcessedImage>();
        void ImageCallback(ProcessedImage img) => images.Add(img);
        var mockProgressCallback = Substitute.For<ProgressCallback>();

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        var result = folder.TryRecover(ImageCallback, new RecoveryParams(), mockProgressCallback);
        Assert.True(result);

        Assert.Equal(2, images.Count);
        ImageAsserts.Similar(ImageResources.dog, images[0]);

        mockProgressCallback.Received()(0, 2);
        mockProgressCallback.Received()(1, 2);
        mockProgressCallback.Received()(2, 2);
        mockProgressCallback.ReceivedCallsCount(3);
    }

    [Fact]
    public void FastRecover()
    {
        CreateFolderToRecoverFrom(_recoveryPath, ["a", "b"]);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        var images = folder.FastRecover().ToList();

        Assert.Equal(2, images.Count);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }

    [Fact]
    public void CancelRecover()
    {
        CreateFolderToRecoverFrom(_recoveryPath, ["a", "b"]);

        var mockImageCallback = Substitute.For<Action<ProcessedImage>>();
        CancellationTokenSource cts = new CancellationTokenSource();

        void ProgressCallback(int current, int total)
        {
            // Cancel after the first image is recovered
            if (current == 1) cts.Cancel();
        }

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);

        var result = folder.TryRecover(mockImageCallback, new RecoveryParams(),
            new ProgressHandler(ProgressCallback, cts.Token));
        Assert.False(result);
        var recoveryDir = new DirectoryInfo(_recoveryPath);
        Assert.True(recoveryDir.Exists);
        Assert.Equal(4, recoveryDir.GetFiles().Length);
        mockImageCallback.Received()(Arg.Any<ProcessedImage>());
        mockImageCallback.ReceivedCallsCount(1);

        // After a cancelled recovery, we should be able to recover from the same folder again
        folder.Dispose();
        using var folder2 = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder2);
    }

    [Fact]
    public void RecoverWithMissingFile()
    {
        var uiImages = CreateFolderToRecoverFrom(_recoveryPath, ["a", "b"]);
        File.Delete(((ImageFileStorage) uiImages[0].GetImageWeakReference().ProcessedImage.Storage).FullPath);

        var images = new List<ProcessedImage>();
        void ImageCallback(ProcessedImage img) => images.Add(img);
        var mockProgressCallback = Substitute.For<ProgressCallback>();

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        var result = folder.TryRecover(ImageCallback, new RecoveryParams(), mockProgressCallback);
        Assert.True(result);

        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);

        mockProgressCallback.Received()(0, 2);
        mockProgressCallback.Received()(1, 2);
        mockProgressCallback.Received()(2, 2);
        mockProgressCallback.ReceivedCallsCount(3);
    }

    [Fact]
    public void FastRecoverWithMissingFile()
    {
        var uiImages = CreateFolderToRecoverFrom(_recoveryPath, ["a", "b"]);
        File.Delete(((ImageFileStorage) uiImages[0].GetImageWeakReference().ProcessedImage.Storage).FullPath);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        var images = folder.FastRecover().ToList();

        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }

    [Fact]
    public void RecoverWithSharedStorage()
    {
        CreateFolderToRecoverFrom(_recoveryPath, ["a", "a", "b", "b"]);

        var images = new List<ProcessedImage>();
        void ImageCallback(ProcessedImage img) => images.Add(img);
        var mockProgressCallback = Substitute.For<ProgressCallback>();

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        var result = folder.TryRecover(ImageCallback, new RecoveryParams(), mockProgressCallback);
        Assert.True(result);

        Assert.Equal(4, images.Count);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
        ImageAsserts.Similar(ImageResources.dog, images[1]);
        ImageAsserts.Similar(ImageResources.dog, images[2]);
        ImageAsserts.Similar(ImageResources.dog, images[3]);
        Assert.Equal(images[0].Storage, images[1].Storage);
        Assert.NotEqual(images[1].Storage, images[2].Storage);
        Assert.Equal(images[2].Storage, images[3].Storage);

        mockProgressCallback.Received()(0, 4);
        mockProgressCallback.Received()(1, 4);
        mockProgressCallback.Received()(2, 4);
        mockProgressCallback.Received()(3, 4);
        mockProgressCallback.Received()(4, 4);
        mockProgressCallback.ReceivedCallsCount(5);
    }

    [Fact]
    public void FastRecoverWithSharedStorage()
    {
        CreateFolderToRecoverFrom(_recoveryPath, ["a", "a", "b", "b"]);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        var images = folder.FastRecover().ToList();

        Assert.Equal(4, images.Count);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
        ImageAsserts.Similar(ImageResources.dog, images[1]);
        ImageAsserts.Similar(ImageResources.dog, images[2]);
        ImageAsserts.Similar(ImageResources.dog, images[3]);
        Assert.Equal(images[0].Storage, images[1].Storage);
        Assert.NotEqual(images[1].Storage, images[2].Storage);
        Assert.Equal(images[2].Storage, images[3].Storage);
    }

    private List<UiImage> CreateFolderToRecoverFrom(string folderPath, List<string> imageRefs)
    {
        var imageList = new UiImageList();
        var rsm1 = RecoveryStorageManager.CreateFolderWithoutThrottle(folderPath, imageList);
        var recoveryContext = new ScanningContext(TestImageContextFactory.Get())
        {
            FileStorageManager = new FileStorageManager(folderPath)
        };
        var processedImages = imageRefs.Distinct().ToDictionary(x => x, _ => CreateRecoveryImage(recoveryContext));
        var images = imageRefs.Select(x => new UiImage(processedImages[x].Clone()))
            .ToList();
        processedImages.Values.ToDisposableList().Dispose();
        imageList.Mutate(new ListMutation<UiImage>.Append(images));
        rsm1.ReleaseLockForTesting();
        return images;
    }

    // TODO: Add tests for recovery params (i.e. thumbnail)

    private ProcessedImage CreateRecoveryImage(ScanningContext recoveryContext)
    {
        return recoveryContext.CreateProcessedImage(ImageContext.Load(ImageResources.dog));
    }
}