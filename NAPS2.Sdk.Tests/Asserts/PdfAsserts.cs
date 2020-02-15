using PdfSharp.Pdf.IO;
using Xunit;

namespace NAPS2.Sdk.Tests.Asserts
{
    public static class PdfAsserts
    {
        public static void AssertPageCount(int count, string filePath)
        {
            var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.InformationOnly);
            Assert.Equal(count, doc.PageCount);
        }

        public static void AssertCompliant(string profile, string filePath)
        {
            using var validator = new PdfAValidator.PdfAValidator();
            var report = validator.ValidateWithDetailedReport(filePath);
            Assert.True(report.Jobs.Job.ValidationReport.IsCompliant);
            Assert.StartsWith($"{profile} ", report.Jobs.Job.ValidationReport.ProfileName);
        }
    }
}
