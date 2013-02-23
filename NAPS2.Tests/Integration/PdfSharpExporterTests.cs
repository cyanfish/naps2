using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using NAPS2.Pdf;
using NUnit.Framework;

namespace NAPS2.Tests.Integration
{
    [TestFixture(Category = "Integration,Fast,Pdf")]
    public class PdfSharpExporterTests : BasePdfExporterTests
    {
        public override void SetUp()
        {
            base.SetUp();
        }

        public override IPdfExporter GetPdfExporter()
        {
            return new PdfSharpExporter();
        }
    }
}
