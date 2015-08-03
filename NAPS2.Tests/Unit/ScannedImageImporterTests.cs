using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NUnit.Framework;

namespace NAPS2.Tests.Unit
{
    [TestFixture(Category = "unit,fast")]
    class ScannedImageImporterTests
    {
        [Test]
        public void Import_NullPath_Throws()
        {
            var mockPdfImport = new Mock<IPdfImporter>();
            var mockImageImport = new Mock<IImageImporter>();
            var importer = new ScannedImageImporter(mockPdfImport.Object, mockImageImport.Object);
            Assert.Throws<ArgumentNullException>(() => importer.Import(null));
        }

        [Test]
        public void Import_PdfExtension_CallsPdfImporter()
        {
            const string filePath = "something.pdf";
            var mockPdfImport = new Mock<IPdfImporter>();
            var mockImageImport = new Mock<IImageImporter>();
            var importer = new ScannedImageImporter(mockPdfImport.Object, mockImageImport.Object);
            importer.Import(filePath);
            mockPdfImport.Verify(x => x.Import(filePath));
        }

        [Test]
        public void Import_ImageExtension_CallsImageImporter()
        {
            const string filePath = "something.png";
            var mockPdfImport = new Mock<IPdfImporter>();
            var mockImageImport = new Mock<IImageImporter>();
            var importer = new ScannedImageImporter(mockPdfImport.Object, mockImageImport.Object);
            importer.Import(filePath);
            mockImageImport.Verify(x => x.Import(filePath));
        }

        [Test]
        public void Import_NoExtension_CallsImageImporter()
        {
            const string filePath = "something";
            var mockPdfImport = new Mock<IPdfImporter>();
            var mockImageImport = new Mock<IImageImporter>();
            var importer = new ScannedImageImporter(mockPdfImport.Object, mockImageImport.Object);
            importer.Import(filePath);
            mockImageImport.Verify(x => x.Import(filePath));
        }
    }
}
