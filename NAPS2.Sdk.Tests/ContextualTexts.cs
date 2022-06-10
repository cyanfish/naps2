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

        ProfileManager = new StubProfileManager();
        ImageContext = new GdiImageContext();
        ScanningContext = new ScanningContext(ImageContext);
    }

    public IProfileManager ProfileManager { get; }

    public ImageContext ImageContext { get; }
    
    public ScanningContext ScanningContext { get; }

    public string FolderPath { get; }

    public DirectoryInfo Folder { get; }

    public ProcessedImage CreateScannedImage()
    {
        return ScanningContext.CreateProcessedImage(new GdiImage(new Bitmap(100, 100)));
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
            Directory.Delete(FolderPath, true);
        }
    }
}