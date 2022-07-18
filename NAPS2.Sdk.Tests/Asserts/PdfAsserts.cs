using System.Drawing;
using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using Xunit;

namespace NAPS2.Sdk.Tests.Asserts;

using PdfAValidator = PdfAValidator.PdfAValidator;

public static class PdfAsserts
{
    private static readonly Lazy<PdfAValidator> LazyPdfAValidator = new Lazy<PdfAValidator>(() => new PdfAValidator());

    public static void AssertPageCount(int count, string filePath)
    {
        var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.InformationOnly);
        Assert.Equal(count, doc.PageCount);
    }

    public static void AssertCompliant(string profile, string filePath)
    {
        var report = LazyPdfAValidator.Value.ValidateWithDetailedReport(filePath);
        Assert.True(report.Jobs.Job.ValidationReport.IsCompliant);
        Assert.StartsWith($"{profile} ", report.Jobs.Job.ValidationReport.ProfileName);
    }

    public static void AssertContainsText(string text, string filePath)
    {
        bool containsText = false;
        foreach (var pageText in new PdfiumPdfReader().ReadTextByPage(filePath))
        {
            if (pageText.Contains(text))
            {
                containsText = true;
            }
        }
        Assert.True(containsText);
    }

    public static void AssertMetadata(PdfMetadata metadata, string filePath)
    {
        var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.InformationOnly);
        Assert.Equal(metadata.Author, doc.Info.Author);
        Assert.Equal(metadata.Creator, doc.Info.Creator);
        Assert.Equal(metadata.Keywords, doc.Info.Keywords);
        Assert.Equal(metadata.Subject, doc.Info.Subject);
        Assert.Equal(metadata.Title, doc.Info.Title);
    }

    public static void AssertEncrypted(string filePath, string ownerPassword, string userPassword,
        Action<PdfSecuritySettings> securitySettingsAsserts)
    {
        Assert.Throws<PdfReaderException>(() => PdfReader.Open(filePath, PdfDocumentOpenMode.InformationOnly));
        var doc = PdfReader.Open(filePath, ownerPassword, PdfDocumentOpenMode.InformationOnly);
        Assert.Equal(PasswordValidity.OwnerPassword, doc.SecurityHandler.ValidatePassword(ownerPassword));
        Assert.Equal(PasswordValidity.UserPassword, doc.SecurityHandler.ValidatePassword(userPassword));
        securitySettingsAsserts(doc.SecuritySettings);
    }

    public static void AssertImages(string path, params Bitmap[] expectedImages)
    {
        var renderer = new PdfiumPdfRenderer(new GdiImageContext());
        var dpi = expectedImages[0].HorizontalResolution;
        var actualImages = renderer.Render(path, dpi).ToList();
        Assert.Equal(expectedImages.Length, actualImages.Count);
        for (int i = 0; i < expectedImages.Length; i++)
        {
            ImageAsserts.Similar(new GdiImage(expectedImages[i]), actualImages[i], ImageAsserts.GENERAL_RMSE_THRESHOLD);
        }
    }
}