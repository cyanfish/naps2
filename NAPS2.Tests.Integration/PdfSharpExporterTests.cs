using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using NAPS.Pdf;
using NUnit.Framework;

namespace NAPS2.Tests.Integration
{
    [TestFixture]
    public class PdfSharpExporterTests : IPdfExporterTests
    {
        public override void SetUp()
        {
            base.SetUp();
            pdfExporter = new PdfSharpExporter();
        }
    }
}
