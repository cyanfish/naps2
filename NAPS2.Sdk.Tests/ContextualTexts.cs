using System.Drawing;
using System.Threading;
using NAPS2.Images.Gdi;
using NAPS2.Scan;

namespace NAPS2.Sdk.Tests;

// TODO: Fix typo (texts -> tests)
public class ContextualTexts : IDisposable
{
    public ContextualTexts()
    {
        FolderPath = Path.GetFullPath(Path.Combine("naps2_test_temp", Path.GetRandomFileName()));
        Folder = Directory.CreateDirectory(FolderPath);

        ImageContext = new GdiImageContext();
        ScanningContext = new ScanningContext(ImageContext);
        ScanningContext.TempFolderPath = Path.Combine(FolderPath, "temp");
        Directory.CreateDirectory(ScanningContext.TempFolderPath);
    }

    public ImageContext ImageContext { get; }

    public ScanningContext ScanningContext { get; }

    public string FolderPath { get; }

    public DirectoryInfo Folder { get; }

    public ProcessedImage CreateScannedImage()
    {
        return ScanningContext.CreateProcessedImage(new GdiImage(new Bitmap(100, 100)));
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

    public bool IsDisposed(GdiImage image)
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
}