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
        CreateFolderToRecoverFrom(_recoveryPath, 0);

        var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.Null(folder);
    }

    [Fact]
    public void FolderLocking()
    {
        CreateFolderToRecoverFrom(_recoveryPath, 1);

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
        CreateFolderToRecoverFrom(_recoveryPath, 1);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        Assert.Equal(1, folder.ImageCount);
        DateAsserts.Recent(TimeSpan.FromMilliseconds(100), folder.ScannedDateTime);
    }

    [Fact]
    public void DeleteFolder()
    {
        CreateFolderToRecoverFrom(_recoveryPath, 1);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        Assert.True(Directory.Exists(_recoveryPath));
        folder.TryDelete();
        Assert.False(Directory.Exists(_recoveryPath));
    }

    [Fact]
    public void Recover()
    {
        CreateFolderToRecoverFrom(_recoveryPath, 2);

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
    public void CancelRecover()
    {
        CreateFolderToRecoverFrom(_recoveryPath, 2);

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
        Assert.True(Directory.Exists(_recoveryPath));
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
        var uiImages = CreateFolderToRecoverFrom(_recoveryPath, 2);
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

    private List<UiImage> CreateFolderToRecoverFrom(string folderPath, int imageCount)
    {
        var imageList = new UiImageList();
        var rsm1 = RecoveryStorageManager.CreateFolderWithoutThrottle(folderPath, imageList);
        var recoveryContext = new ScanningContext(TestImageContextFactory.Get())
        {
            FileStorageManager = new FileStorageManager(folderPath)
        };
        var images = Enumerable.Range(0, imageCount).Select(x => new UiImage(CreateRecoveryImage(recoveryContext)))
            .ToList();
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