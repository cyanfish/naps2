using System.Threading;
using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
using NAPS2.Scan;

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

    public ScanningContext ScanningContext { get; }

    public string FolderPath { get; }

    public DirectoryInfo Folder { get; }

    public IMemoryImage LoadImage(byte[] resource)
    {
        return ImageContext.Load(new MemoryStream(resource));
    }

    public ProcessedImage CreateScannedImage()
    {
        // TODO: A different placeholder image here?
        return ScanningContext.CreateProcessedImage(LoadImage(ImageResources.color_image));
    }

    public void SetUpOcr()
    {
        var best = Path.Combine(FolderPath, "best");
        Directory.CreateDirectory(best);
        var fast = Path.Combine(FolderPath, "fast");
        Directory.CreateDirectory(fast);
        
        var tesseractPath = CopyResourceToFile(BinaryResources.tesseract_x64, FolderPath, "tesseract.exe");
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsMacOS())
        {
            // TODO: We should try and not rely on tesseract being installed on the system
            tesseractPath = "tesseract";
        }
#endif
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