using System.Drawing;
using System.Threading;
using Moq;
using NAPS2.Images.Gdi;
using NAPS2.Recovery;
using NAPS2.Scan;
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
    public void EmptyRecovery()
    {
        var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.Null(folder);
    }

    [Fact]
    public void NoImages()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 0);

        var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.Null(folder);
    }

    [Fact]
    public void Locking()
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
    public void FindRecoveryFolder()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 1);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        Assert.Equal(1, folder.ImageCount);
        Assert.InRange(folder.ScannedDateTime, DateTime.Now - TimeSpan.FromMilliseconds(100), DateTime.Now);
    }

    [Fact]
    public void DeleteRecoveryFolder()
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
        CreateFolderToRecoverFrom(recovery1, 1);

        var mockImageCallback = new Mock<Action<ProcessedImage>>();
        var mockProgressCallback = new Mock<ProgressHandler>();

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        var result = folder.TryRecover(mockImageCallback.Object, new RecoveryParams(), mockProgressCallback.Object,
            CancellationToken.None);
        Assert.True(result);
        // TODO: More asserts
    }

    // TODO: Cancellation tests. Pdf storage tests. Plus whatever else I think of.

    private void CreateFolderToRecoverFrom(string folderPath, int imageCount)
    {
        var rsm1 = RecoveryStorageManager.CreateFolder(folderPath);
        var recoveryContext = new ScanningContext(new GdiImageContext(), new FileStorageManager(folderPath));
        var images = Enumerable.Range(0, imageCount).Select(x => new UiImage(CreateRecoveryImage(recoveryContext)))
            .ToArray();
        rsm1.WriteIndex(images);
        rsm1.ReleaseLockForTesting();
    }

    private ProcessedImage CreateRecoveryImage(ScanningContext recoveryContext)
    {
        return recoveryContext.CreateProcessedImage(new GdiImage(new Bitmap(100, 100)));
    }
}