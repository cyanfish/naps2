using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Pdf;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport.Pdf;

public class PdfBenchmarkTests : ContextualTests
{
    [BenchmarkFact]
    public async Task PdfSharpExport300()
    {
        var filePath = Path.Combine(FolderPath, "test");
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image), BitDepth.Color,
            false, -1, Enumerable.Empty<Transform>());

        var pdfExporter = new PdfExporter(ScanningContext);
        for (int i = 0; i < 300; i++)
        {
            await pdfExporter.Export(filePath + i + ".pdf", new[] { image }, new PdfExportParams());
        }
    }

    [BenchmarkFact]
    public async Task PdfSharpExportHuge()
    {
        var filePath = Path.Combine(FolderPath, "test");
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image_huge),
            BitDepth.Color, true, -1, Enumerable.Empty<Transform>());

        var pdfExporter = new PdfExporter(ScanningContext);
        await pdfExporter.Export(filePath + ".pdf", new[] { image }, new PdfExportParams());
    }

    [BenchmarkFact]
    public async Task PdfSharpExportHugePng()
    {
        var filePath = Path.Combine(FolderPath, "test");
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image_huge_png),
            BitDepth.Color, true, -1, Enumerable.Empty<Transform>());

        var pdfExporter = new PdfExporter(ScanningContext);
        await pdfExporter.Export(filePath + ".pdf", new[] { image }, new PdfExportParams());
    }

    [BenchmarkFact]
    public async Task PdfiumExport300()
    {
        var filePath = Path.Combine(FolderPath, "test");
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image), BitDepth.Color,
            false, -1, Enumerable.Empty<Transform>());

        var pdfExporter = new PdfiumPdfExporter(ScanningContext);
        for (int i = 0; i < 300; i++)
        {
            await pdfExporter.Export(filePath + i + ".pdf", new[] { image }, new PdfExportParams());
        }
    }

    [BenchmarkFact]
    public async Task PdfiumExportHuge()
    {
        var filePath = Path.Combine(FolderPath, "test");
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image_huge),
            BitDepth.Color, true, -1, Enumerable.Empty<Transform>());

        var pdfExporter = new PdfiumPdfExporter(ScanningContext);
        await pdfExporter.Export(filePath + ".pdf", new[] { image }, new PdfExportParams());
    }

    [BenchmarkFact]
    public async Task PdfiumExportHugePng()
    {
        var filePath = Path.Combine(FolderPath, "test");
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image_huge_png),
            BitDepth.Color, true, -1, Enumerable.Empty<Transform>());

        var pdfExporter = new PdfiumPdfExporter(ScanningContext);
        await pdfExporter.Export(filePath + ".pdf", new[] { image }, new PdfExportParams());
    }

    [BenchmarkFact]
    public async Task Import300Naps2()
    {
        ScanningContext.FileStorageManager = FileStorageManager.CreateFolder("recovery");
        var filePath = CopyResourceToFile(PdfResources.image_pdf, "test.pdf");

        var pdfExporter = new PdfImporter(ScanningContext);
        for (int i = 0; i < 300; i++)
        {
            await pdfExporter.Import(filePath).ToList();
        }
    }

    [BenchmarkFact]
    public async Task Import300NonNaps2()
    {
        ScanningContext.FileStorageManager = FileStorageManager.CreateFolder("recovery");
        var filePath = CopyResourceToFile(PdfResources.word_generated_pdf, "test.pdf");

        var pdfExporter = new PdfImporter(ScanningContext);
        for (int i = 0; i < 300; i++)
        {
            await pdfExporter.Import(filePath).ToList();
        }
    }

    public class BenchmarkFact : FactAttribute
    {
        public BenchmarkFact()
        {
            Skip = "comment out this line to run benchmarks";
        }
    }
}