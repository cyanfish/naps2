using System.Threading;
using Moq;
using NAPS2.Images.Gdi;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Recovery;

public class RecoveryManagerTests : ContextualTexts
{
    private readonly string _recoveryBasePath;
    private readonly RecoveryManager _recoveryManager;

    public RecoveryManagerTests()
    {
        _recoveryBasePath = Path.Combine(FolderPath, "recovery");
        ScanningContext.FileStorageManager = FileStorageManager.CreateFolder(_recoveryBasePath);
        ScanningContext.RecoveryPath = _recoveryBasePath;
        _recoveryManager = new RecoveryManager(ScanningContext);
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
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 0);

        var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.Null(folder);
    }

    [Fact]
    public void FolderLocking()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 1);

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
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 1);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        Assert.Equal(1, folder.ImageCount);
        Assert.InRange(folder.ScannedDateTime, DateTime.Now - TimeSpan.FromMilliseconds(100), DateTime.Now);
    }

    [Fact]
    public void DeleteFolder()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 1);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        Assert.True(Directory.Exists(recovery1));
        folder.TryDelete();
        Assert.False(Directory.Exists(recovery1));
    }

    [Fact]
    public void Recover()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 2);

        var images = new List<ProcessedImage>();
        void ImageCallback(ProcessedImage img) => images.Add(img);
        var mockProgressCallback = new Mock<ProgressHandler>();

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        var result = folder.TryRecover(ImageCallback, new RecoveryParams(), mockProgressCallback.Object,
            CancellationToken.None);
        Assert.True(result);

        Assert.Equal(2, images.Count);
        var expectedImage = new GdiImage(SharedData.color_image);
        var actualImage = ImageContext.Render(images[0]);
        ImageAsserts.Similar(expectedImage, actualImage, ImageAsserts.GENERAL_RMSE_THRESHOLD);

        mockProgressCallback.Verify(callback => callback(0, 2));
        mockProgressCallback.Verify(callback => callback(1, 2));
        mockProgressCallback.Verify(callback => callback(2, 2));
        mockProgressCallback.VerifyNoOtherCalls();
    }

    [Fact]
    public void CancelRecover()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 2);

        var mockImageCallback = new Mock<Action<ProcessedImage>>();
        CancellationTokenSource cts = new CancellationTokenSource();

        void ProgressCallback(int current, int total)
        {
            // Cancel after the first image is recovered
            if (current == 1) cts.Cancel();
        }

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);

        var result = folder.TryRecover(mockImageCallback.Object, new RecoveryParams(), ProgressCallback,
            cts.Token);
        Assert.False(result);
        Assert.True(Directory.Exists(recovery1));
        mockImageCallback.Verify(callback => callback(It.IsAny<ProcessedImage>()));
        mockImageCallback.VerifyNoOtherCalls();

        // After a cancelled recovery, we should be able to recover from the same folder again
        folder.Dispose();
        using var folder2 = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder2);
    }

    private void CreateFolderToRecoverFrom(string folderPath, int imageCount)
    {
        var rsm1 = RecoveryStorageManager.CreateFolder(folderPath);
        var recoveryContext = new ScanningContext(new GdiImageContext(), new FileStorageManager(folderPath));
        var images = Enumerable.Range(0, imageCount).Select(x => new UiImage(CreateRecoveryImage(recoveryContext)))
            .ToArray();
        rsm1.WriteIndex(images);
        rsm1.ReleaseLockForTesting();
    }
    
    // TODO: Add tests for recovery params (i.e. thumbnail)

    private ProcessedImage CreateRecoveryImage(ScanningContext recoveryContext)
    {
        return recoveryContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
    }
}