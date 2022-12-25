using System.Threading;
using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
using NAPS2.Scan;
using NAPS2.Unmanaged;

namespace NAPS2.Sdk.Tests;

public class ContextualTests : IDisposable
{
    public ContextualTests()
    {
        FolderPath = Path.GetFullPath(Path.Combine("naps2_test_temp", Path.GetRandomFileName()));
        Folder = Directory.CreateDirectory(FolderPath);

        ImageContext = TestImageContextFactory.Get(new PdfiumPdfRenderer());
        ScanningContext = new ScanningContext(ImageContext);
        ScanningContext.TempFolderPath = Path.Combine(FolderPath, "temp");
        Directory.CreateDirectory(ScanningContext.TempFolderPath);
    }

    public ImageContext ImageContext { get; }

    // TODO: We can probably do some processed image lifecycle checking by ensuring the scanning context has no
    // registered images after running a test.
    public ScanningContext ScanningContext { get; }

    public string FolderPath { get; }

    public DirectoryInfo Folder { get; }

    public IMemoryImage LoadImage(byte[] resource)
    {
        return ImageContext.Load(new MemoryStream(resource));
    }

    public ProcessedImage CreateScannedImage()
    {
        return ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
    }

    public IEnumerable<ProcessedImage> CreateScannedImages(params byte[][] images)
    {
        foreach (var image in images)
        {
            yield return ScanningContext.CreateProcessedImage(LoadImage(image));
        }
    }

    public void SetUpOcr()
    {
        var best = Path.Combine(FolderPath, "best");
        Directory.CreateDirectory(best);
        var fast = Path.Combine(FolderPath, "fast");
        Directory.CreateDirectory(fast);

        var depsRoot = Environment.GetEnvironmentVariable("NAPS2_TEST_DEPS");
        var tesseractPath = NativeLibrary.FindExePath(PlatformCompat.System.TesseractExecutableName, depsRoot);
        CopyResourceToFile(BinaryResources.eng_traineddata, fast, "eng.traineddata");
        CopyResourceToFile(BinaryResources.heb_traineddata, fast, "heb.traineddata");
        ScanningContext.OcrEngine = new TesseractOcrEngine(tesseractPath, FolderPath, FolderPath);
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

    public bool IsDisposed(IMemoryImage image)
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