using NAPS2.ImportExport.Pdf;
using NAPS2.ImportExport.Pdf.Pdfium;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.Security;
using Xunit;

namespace NAPS2.Sdk.Tests.Asserts;

using PdfAValidator = PdfAValidator.PdfAValidator;

public static class PdfAsserts
{
    private static readonly Lazy<PdfAValidator> LazyPdfAValidator = new Lazy<PdfAValidator>(() => new PdfAValidator());

    public static void AssertPageCount(int count, string filePath)
    {
        Assert.True(File.Exists(filePath));
        var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.InformationOnly);
        Assert.Equal(count, doc.PageCount);
    }

    public static void AssertCompliant(string profile, string filePath)
    {
        Assert.True(File.Exists(filePath));
        var report = LazyPdfAValidator.Value.ValidateWithDetailedReport(filePath);
        Assert.True(report.Jobs.Job.ValidationReport.IsCompliant);
        Assert.StartsWith($"{profile} ", report.Jobs.Job.ValidationReport.ProfileName);
    }

    public static void AssertContainsTextOnce(string text, string filePath)
    {
        Assert.Equal(1, CountText(text, filePath));
    }

    private static int CountText(string text, string filePath)
    {
        Assert.True(File.Exists(filePath));
        int count = 0;
        foreach (var pageText in new PdfiumPdfReader().ReadTextByPage(filePath))
        {
            int startIndex = 0;
            int index;
            while ((index = pageText.IndexOf(text, startIndex, StringComparison.InvariantCulture)) != -1)
            {
                count++;
                startIndex = index + 1;
            }
        }
        return count;
    }

    public static void AssertMetadata(PdfMetadata expected, string filePath, string password = null)
    {
        Assert.True(File.Exists(filePath));
        var actual = new PdfiumPdfReader().ReadMetadata(filePath, password);
        Assert.Equal(expected.Author, actual.Author);
        Assert.Equal(expected.Creator, actual.Creator);
        Assert.Equal(expected.Keywords, actual.Keywords);
        Assert.Equal(expected.Subject, actual.Subject);
        Assert.Equal(expected.Title, actual.Title);
    }

    public static void AssertEncrypted(string filePath, string ownerPassword, string userPassword,
        Action<PdfSecuritySettings> securitySettingsAsserts = null)
    {
        Assert.True(File.Exists(filePath));
        Assert.Throws<PdfReaderException>(() => PdfReader.Open(filePath, PdfDocumentOpenMode.InformationOnly));
        var doc = PdfReader.Open(filePath, ownerPassword, PdfDocumentOpenMode.InformationOnly);
        Assert.Equal(PasswordValidity.OwnerPassword, doc.SecurityHandler.ValidatePassword(ownerPassword));
        Assert.Equal(PasswordValidity.UserPassword, doc.SecurityHandler.ValidatePassword(userPassword));
        securitySettingsAsserts?.Invoke(doc.SecuritySettings);
    }

    public static void AssertImages(string filePath, params byte[][] expectedImages) =>
        AssertImages(filePath, null, expectedImages);

    public static void AssertImages(string filePath, string password, params byte[][] expectedImages)
    {
        Assert.True(File.Exists(filePath));
        var renderer = new PdfiumPdfRenderer();
        // TODO: Optimize?
        var dpi = TestImageContextFactory.Get().Load(expectedImages[0]).HorizontalResolution;
        var actualImages = renderer.Render(TestImageContextFactory.Get(), filePath, dpi, password).ToList();
        // var actualImages = renderer.Render(
        //     TestImageContextFactory.Get(),
        //     filePath, 
        //     i => expectedImages.Length > i ? expectedImages[i].HorizontalResolution : 300).ToList();
        Assert.Equal(expectedImages.Length, actualImages.Count);
        for (int i = 0; i < expectedImages.Length; i++)
        {
            ImageAsserts.Similar(expectedImages[i], actualImages[i], ignoreFormat: true);
        }
    }

    public static void AssertImageFilter(string filePath, int pageIndex, params string[] filters)
    {
        Assert.True(File.Exists(filePath));
        lock (PdfiumNativeLibrary.Instance)
        {
            using var doc = PdfDocument.Load(filePath);
            Assert.InRange(pageIndex, 0, doc.PageCount - 1);
            using var page = doc.GetPage(pageIndex);
            using var obj = PdfiumImageExtractor.GetSingleImageObject(page);
            Assert.NotNull(obj);
            Assert.True(obj.HasImageFilters(filters),
                $"Expected filters: {string.Join(",", filters)}, actual: {string.Join(",", obj.GetImageFilters())}");
        }
    }
}