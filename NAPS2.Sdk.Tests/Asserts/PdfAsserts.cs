using System.Text.RegularExpressions;
using Codeuctivity;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Pdf.Pdfium;
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

    public static void AssertPageSize(PageSize pageSize, int precision, string filePath)
    {
        Assert.True(File.Exists(filePath));
        var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.InformationOnly);
        Assert.Equal((double) pageSize.WidthInInches, doc.Pages[0].Width.Inch, precision);
        Assert.Equal((double) pageSize.HeightInInches, doc.Pages[0].Height.Inch, precision);
    }

    public static async Task AssertCompliant(string profile, string filePath)
    {
        if (string.IsNullOrEmpty(profile))
        {
            return;
        }
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
            throw new XunitException($"Unexpected count for \"{text}\": expected {1}, got {value}");
        }
    }

    public static void AssertDoesNotContainText(string text, string filePath)
    {
        var value = CountText(text, filePath);
        if (value != 0)
        {
            throw new XunitException($"Unexpected count for \"{text}\": expected {0}, got {value}");
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
            var normalized = Regex.Replace(pageText, "\\s+", " ");
            while ((index = normalized.IndexOf(text, startIndex, StringComparison.Ordinal)) != -1)
            {
                count++;
                startIndex = index + 1;
            }
        }
        return count;
    }

    public static void AssertOcrText(OcrResult ocrResult, string filePath)
    {
        Assert.True(File.Exists(filePath));
        var expectedChars = ocrResult.Words
            .SelectMany(word => word.Text.Where(c => !char.IsWhiteSpace(c)).Select(c => (c, word.Bounds)))
            .ToList();
        lock (PdfiumNativeLibrary.Instance)
        {
            using var doc = PdfDocument.Load(filePath);
            using var page = doc.GetPage(0);
            using var text = page.GetText();
            int j = 0;
            for (int i = 0; i < text.CharCount; i++)
            {
                var c = (char) text.ReadUtf32(i);
                if (char.IsWhiteSpace(c) || char.IsControl(c)) continue;
                Assert.True(j < expectedChars.Count, $"Unexpected extra char '{c}'");
                var (expectedChar, bounds) = expectedChars[j++];
                Assert.Equal(expectedChar, c);
                // The center of the char box (scaled to the OCR page bounds, with the origin at the top-left) should be
                // within the word bounds. This assumes the page isn't cropped or rotated.
                var box = text.GetCharBox(i);
                var x = (box.Left + box.Right) / 2 * (ocrResult.PageBounds.w / page.Width);
                var y = (page.Height - (box.Top + box.Bottom) / 2) * (ocrResult.PageBounds.h / page.Height);
                Assert.InRange(x, bounds.x, bounds.x + bounds.w);
                Assert.InRange(y, bounds.y, bounds.y + bounds.h);
            }
            Assert.Equal(expectedChars.Count, j);
        }
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
            using var obj = PdfiumImageExtractor.GetSingleImageObject(page, true);
            Assert.NotNull(obj);
            Assert.True(obj.HasImageFilters(filters),
                $"Expected filters: {string.Join(",", filters)}, actual: {string.Join(",", obj.GetImageFilters())}");
        }
    }

    public static void AssertVersion(int version, string filePath)
    {
        lock (PdfiumNativeLibrary.Instance)
        {
            using var doc = PdfDocument.Load(filePath);
            Assert.Equal(version, doc.Version);
        }
    }
}