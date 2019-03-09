using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
