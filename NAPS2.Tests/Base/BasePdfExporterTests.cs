using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NUnit.Framework;

namespace NAPS2.Tests.Base
{
    public abstract class BasePdfExporterTests
    {
        private const String PDF_PATH = "test/test.pdf";
        private List<ScannedImage> images;
        private PdfSettings settings;
        protected IPdfExporter pdfExporter;

        [SetUp]
        public virtual void SetUp()
        {
            pdfExporter = GetPdfExporter();
            settings = new PdfSettings
            {
                Metadata =
                {
                    Author = "Test Author",
                    Creator = "Test Creator",
                    Keywords = "Test Keywords",
                    Subject = "Test Subject",
                    Title = "Test Title"
                }
            };
            images = new List<Bitmap> {
                ColorBitmap(100, 100, Color.Red),
                ColorBitmap(100, 100, Color.Yellow),
                ColorBitmap(200, 100, Color.Green),
            }.Select(bitmap =>
            {
                using (bitmap)
                {
                    return new ScannedImage(bitmap, ScanBitDepth.C24Bit, false, -1);
                }
            }).ToList();
            if (!Directory.Exists("test"))
            {
                Directory.CreateDirectory("test");
            }
            if (File.Exists(PDF_PATH))
            {
                File.Delete(PDF_PATH);
            }
        }

        [TearDown]
        public void TearDown()
        {
            pdfExporter = null;
            settings = null;
            foreach (ScannedImage img in images)
            {
                img.Dispose();
            }
            images = null;
        }

        public abstract IPdfExporter GetPdfExporter();

        private Bitmap ColorBitmap(int w, int h, Color color)
        {
            var result = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.FillRectangle(new SolidBrush(color), 0, 0, w, h);
            }
            return result;
        }

        [Test]
        public void Export_Normal_CreatesFile()
        {
            Assert.IsFalse(File.Exists(PDF_PATH), "Error setting up test (test file not deleted)");
            pdfExporter.Export(PDF_PATH, images, settings, null, num => true);
            Assert.IsTrue(File.Exists(PDF_PATH));
        }

        [Test]
        public void Export_Normal_ReturnsTrue()
        {
            bool result = pdfExporter.Export(PDF_PATH, images, settings, null, num => true);
            Assert.IsTrue(result);
        }

        [Test]
        public void Export_Progress_IsLinear()
        {
            int i = 0;
            pdfExporter.Export(PDF_PATH, images, settings, null, num =>
            {
                Assert.AreEqual(++i, num);
                return true;
            });
            Assert.AreEqual(i, images.Count);
        }

        [Test]
        public void Export_Cancel_DoesntContinue()
        {
            bool first = true;
            pdfExporter.Export(PDF_PATH, images, settings, null, num =>
            {
                Assert.IsTrue(first);
                first = false;
                return false;
            });
        }

        [Test]
        public void Export_Cancel_ReturnsFalse()
        {
            bool result = pdfExporter.Export(PDF_PATH, images, settings, null, num => false);
            Assert.IsFalse(result);
        }
    }
}
