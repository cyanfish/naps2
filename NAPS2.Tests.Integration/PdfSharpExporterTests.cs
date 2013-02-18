using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using NAPS2.Pdf;
using NUnit.Framework;

namespace NAPS2.Tests.Integration
{
    [TestFixture(Category = "Medium,Pdf")]
    public class PdfSharpExporterTests : IPdfExporterTests
    {
        public override void SetUp()
        {
            base.SetUp();
            pdfExporter = new PdfSharpExporter();
        }
    }
}
