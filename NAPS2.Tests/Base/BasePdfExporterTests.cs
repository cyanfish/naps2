/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Tests.Integration;
using NUnit.Framework;

namespace NAPS2.Tests.Base
{
    public abstract class BasePdfExporterTests
    {
        private const String PDF_PATH = "test/test.pdf";
        private List<IScannedImage> images;
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
                    return (IScannedImage)new ScannedImage(bitmap, ScanBitDepth.C24Bit, false, new PdfSharpExporterTests.StubUserConfigManager());
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
            foreach (IScannedImage img in images)
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
