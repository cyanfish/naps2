using System.Drawing;
using System.Threading;
using NAPS2.Images.Gdi;
using NAPS2.Scan;

namespace NAPS2.Sdk.Tests;

public class ContextualTexts : IDisposable
{
    public ContextualTexts()
    {
        FolderPath = $"naps2_test_temp/{Path.GetRandomFileName()}";
        Folder = Directory.CreateDirectory(FolderPath);
        var tempPath = Path.Combine(FolderPath, "temp");
        Directory.CreateDirectory(tempPath);

        ProfileManager = new StubProfileManager();
        ImageContext = new GdiImageContext();
        ScanningContext = new ScanningContext(ImageContext)
        {
            FileStorageManager = new FileStorageManager(tempPath)
        };
    }

    public IProfileManager ProfileManager { get; }

    public ImageContext ImageContext { get; }
    
    public ScanningContext ScanningContext { get; }

    public string FolderPath { get; }

    public DirectoryInfo Folder { get; }

    public void UseFileStorage()
    {
        ImageContext.ConfigureBackingStorage<ImageFileStorage>();
    }

    public void UseRecovery()
    {
        // TODO: Figure out recovery
        var recoveryFolderPath = Path.Combine(FolderPath, "recovery", Path.GetRandomFileName());
        //ScanningContext.UseRecovery(recoveryFolderPath);
    }

    public ProcessedImage CreateScannedImage()
    {
        return new ProcessedImage(
            new GdiImage(new Bitmap(100, 100)),
            ImageMetadata.DefaultForTesting,
            TransformState.Empty);
    }

    public virtual void Dispose()
    {
        ImageContext.Dispose();
        try
        {
            Directory.Delete(FolderPath, true);
        }
        catch (IOException)
        {
            Thread.Sleep(100);
            Directory.Delete(FolderPath, true);
        }
    }
}