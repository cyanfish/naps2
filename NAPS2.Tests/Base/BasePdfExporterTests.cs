using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using NAPS2.Pdf;
using NUnit.Framework;

namespace NAPS2.Tests
{
    public abstract class BasePdfExporterTests
    {
        private readonly String pdfPath = "test/test.pdf";
        protected IPdfExporter pdfExporter;
        private PdfInfo info;
        private List<Image> images;

        [SetUp]
        public virtual void SetUp()
        {
            pdfExporter = GetPdfExporter();
            info = new PdfInfo
            {
                Author = "Test Author",
                Creator = "Test Creator",
                Keywords = "Test Keywords",
                Subject = "Test Subject",
                Title = "Test Title"
            };
            images = new List<Image> {
                ColorBitmap(100, 100, Color.Red),
                ColorBitmap(100, 100, Color.Yellow),
                ColorBitmap(200, 100, Color.Green),
            };
            if (!Directory.Exists("test"))
            {
                Directory.CreateDirectory("test");
            }
            if (File.Exists(pdfPath))
            {
                File.Delete(pdfPath);
            }
        }

        public abstract IPdfExporter GetPdfExporter();

        private Bitmap ColorBitmap(int w, int h, Color color)
        {
            Bitmap result = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.FillRectangle(new SolidBrush(color), 0, 0, w, h);
            }
            return result;
        }

        [TearDown]
        public void TearDown()
        {
            pdfExporter = null;
            info = null;
            foreach (var img in images)
            {
                img.Dispose();
            }
            images = null;
        }

        [Test]
        public void Export_Normal_CreatesFile()
        {
            Assert.IsFalse(File.Exists(pdfPath), "Error setting up test (test file not deleted)");
            pdfExporter.Export(pdfPath, images, info, num => true);
            Assert.IsTrue(File.Exists(pdfPath));
        }

        [Test]
        public void Export_Normal_ReturnsTrue()
        {
            var result = pdfExporter.Export(pdfPath, images, info, num => true);
            Assert.IsTrue(result);
        }

        [Test]
        public void Export_Progress_IsLinear()
        {
            var i = 0;
            pdfExporter.Export(pdfPath, images, info, num =>
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
            pdfExporter.Export(pdfPath, images, info, num =>
            {
                Assert.IsTrue(first);
                first = false;
                return false;
            });
        }

        [Test]
        public void Export_Cancel_ReturnsFalse()
        {
            var result = pdfExporter.Export(pdfPath, images, info, num => false);
            Assert.IsFalse(result);
        }
    }
}
