using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.ImportExport.Pdf;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport
{
    public class PdfiumTests : ContextualTexts
    {
        [Fact]
        public void RenderPdfFromWord()
        {
            var path = Path.Combine(FolderPath, "word.pdf");
            File.WriteAllBytes(path, PdfiumTestsData.word);

            var images = PdfiumPdfRenderer.Render(path, 300).ToList();

            Assert.Equal(2, images.Count);
            Assert.Equal(images[0].Width, (int)(8.5 * 300));
            Assert.Equal(images[0].Height, 11 * 300);
            Assert.Equal(images[0].HorizontalResolution, 300);
            Assert.Equal(images[0].VerticalResolution, 300);
        }
    }
}
