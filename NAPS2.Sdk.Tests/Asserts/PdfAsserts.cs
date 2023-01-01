using Codeuctivity;
using NAPS2.ImportExport.Pdf;
using NAPS2.ImportExport.Pdf.Pdfium;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.Security;
using Xunit;
using Xunit.Sdk;

namespace NAPS2.Sdk.Tests.Asserts;

public static class PdfAsserts
{
    private static readonly Lazy<PdfAValidator> LazyPdfAValidator = new(() => new PdfAValidator());

    public static void AssertPageCount(int count, string filePath)
    {
        Assert.True(File.Exists(filePath));
        var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.InformationOnly);
        Assert.Equal(count, doc.PageCount);
    }

    public static async Task AssertCompliant(string profile, string filePath)
    {
        Assert.True(File.Exists(filePath));
        var report = await LazyPdfAValidator.Value.ValidateWithDetailedReportAsync(filePath);
        Assert.True(report.Jobs.Job.ValidationReport.IsCompliant);
        Assert.StartsWith($"{profile} ", report.Jobs.Job.ValidationReport.ProfileName);
    }

    public static void AssertContainsTextOnce(string text, string filePath)
    {
        var value = CountText(text, filePath);
        if (value != 1)
        {
            throw new AssertActualExpectedException(1, value, $"Unexpected count for \"{text}\"");
        }
    }

    public static void AssertDoesNotContainText(string text, string filePath)
    {
        var value = CountText(text, filePath);
        if (value != 0)
        {
            throw new AssertActualExpectedException(0, value, $"Unexpected count for \"{text}\"");
        }
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
        var renderer = new PdfiumPdfRenderer { NoExtraction = true };
        using var expectedImagesRendered =
            expectedImages.Select(data => TestImageContextFactory.Get().Load(data)).ToDisposableList();
        var renderSizes = PdfRenderSize.FromIndividualPageSizes(
            expectedImagesRendered.Select(image => PdfRenderSize.FromDimensions(image.Width, image.Height)));
        var actualImages = renderer.Render(TestImageContextFactory.Get(), filePath, renderSizes, password).ToList();
        Assert.Equal(expectedImages.Length, actualImages.Count);
        for (int i = 0; i < expectedImages.Length; i++)
        {
            // TODO: Try and fix resolution here
            ImageAsserts.Similar(expectedImagesRendered[i], actualImages[i], ignoreResolution: true);
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