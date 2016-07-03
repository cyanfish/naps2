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
using System.Threading;
using NAPS2.Ocr;
using NAPS2.Scan.Images;
using NAPS2.Util;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Security;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSharpExporter : IPdfExporter
    {
        private readonly IOcrEngine ocrEngine;

        public PdfSharpExporter(IOcrEngine ocrEngine)
        {
            this.ocrEngine = ocrEngine;
        }

        public bool Export(string path, IEnumerable<ScannedImage> images, PdfSettings settings, string ocrLanguageCode, Func<int, bool> progressCallback)
        {
            var document = new PdfDocument();
            document.Info.Author = settings.Metadata.Author;
            document.Info.Creator = settings.Metadata.Creator;
            document.Info.Keywords = settings.Metadata.Keywords;
            document.Info.Subject = settings.Metadata.Subject;
            document.Info.Title = settings.Metadata.Title;

            if (settings.Encryption.EncryptPdf
                && (!string.IsNullOrEmpty(settings.Encryption.OwnerPassword) || !string.IsNullOrEmpty(settings.Encryption.UserPassword)))
            {
                document.SecuritySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted128Bit;
                if (!string.IsNullOrEmpty(settings.Encryption.OwnerPassword))
                {
                    document.SecuritySettings.OwnerPassword = settings.Encryption.OwnerPassword;
                }
                if (!string.IsNullOrEmpty(settings.Encryption.UserPassword))
                {
                    document.SecuritySettings.UserPassword = settings.Encryption.UserPassword;
                }
                document.SecuritySettings.PermitAccessibilityExtractContent = settings.Encryption.AllowContentCopyingForAccessibility;
                document.SecuritySettings.PermitAnnotations = settings.Encryption.AllowAnnotations;
                document.SecuritySettings.PermitAssembleDocument = settings.Encryption.AllowDocumentAssembly;
                document.SecuritySettings.PermitExtractContent = settings.Encryption.AllowContentCopying;
                document.SecuritySettings.PermitFormsFill = settings.Encryption.AllowFormFilling;
                document.SecuritySettings.PermitFullQualityPrint = settings.Encryption.AllowFullQualityPrinting;
                document.SecuritySettings.PermitModifyDocument = settings.Encryption.AllowDocumentModification;
                document.SecuritySettings.PermitPrint = settings.Encryption.AllowPrinting;
            }


            bool useOcr = false;
            if (ocrLanguageCode != null)
            {
                if (ocrEngine.CanProcess(ocrLanguageCode))
                {
                    useOcr = true;
                }
                else
                {
                    Log.Error("OCR files not available for '{0}'.", ocrLanguageCode);
                }
            }

            bool result = useOcr
                ? BuildDocumentWithOcr(progressCallback, document, images, ocrLanguageCode)
                : BuildDocumentWithoutOcr(progressCallback, document, images);
            if (!result)
            {
                return false;
            }

            PathHelper.EnsureParentDirExists(path);
            document.Save(path);
            return true;
        }

        private bool BuildDocumentWithoutOcr(Func<int, bool> progressCallback, PdfDocument document, IEnumerable<ScannedImage> images)
        {
            int progress = 0;
            foreach (var image in images)
            {
                using (Stream stream = image.GetImageStream())
                using (var img = new Bitmap(stream))
                {
                    if (!progressCallback(progress))
                    {
                        return false;
                    }

                    PdfPage page = document.AddPage();
                    DrawImageOnPage(page, img);
                }
                progress++;
            }
            return true;
        }

        private bool BuildDocumentWithOcr(Func<int, bool> progressCallback, PdfDocument document, IEnumerable<ScannedImage> images, string ocrLanguageCode)
        {
            // Use a pipeline so that multiple pages/images can be processed in parallel
            // Note: No locks needed on the document because the design of the pipeline ensures no two threads will work on it at once

            int progress = 0;
            Pipeline.For(images).Step(image =>
            {
                // Step 1: Load the image into memory, draw it on a new PDF page, and save a copy of the processed image to disk for OCR

                if (!progressCallback(progress))
                {
                    return null;
                }

                using (Stream stream = image.GetImageStream())
                using (var img = new Bitmap(stream))
                {
                    if (!progressCallback(progress))
                    {
                        return null;
                    }

                    PdfPage page = document.AddPage();
                    DrawImageOnPage(page, img);

                    if (!progressCallback(progress))
                    {
                        return null;
                    }

                    string tempImageFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
                    img.Save(tempImageFilePath);

                    return Tuple.Create(page, tempImageFilePath);
                }
            }).StepParallel((page, tempImageFilePath) =>
            {
                // Step 2: Run OCR on the processsed image file
                // This step is doubly parallel since not only can it run alongside other stages of the pipeline,
                // multiple files can also be OCR'd at once (no interdependencies, it doesn't touch the document)

                OcrResult ocrResult;
                try
                {
                    if (!progressCallback(progress))
                    {
                        return null;
                    }

                    ocrResult = ocrEngine.ProcessImage(tempImageFilePath, ocrLanguageCode);
                }
                finally
                {
                    File.Delete(tempImageFilePath);
                }

                // The final pipeline step is pretty fast, so updating progress here is more accurate
                if (progressCallback(progress))
                {
                    Interlocked.Increment(ref progress);
                    progressCallback(progress);
                }

                return Tuple.Create(page, ocrResult);
            }).StepBlock().Run((page, ocrResult) =>
            {
                // Step 3: Draw the OCR text on the PDF page

                if (ocrResult == null)
                {
                    return;
                }
                if (!progressCallback(progress))
                {
                    return;
                }
                DrawOcrTextOnPage(page, ocrResult);
            });
            return progressCallback(progress);
        }

        private static void DrawOcrTextOnPage(PdfPage page, OcrResult ocrResult)
        {
            using (XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend))
            {
                var tf = new XTextFormatter(gfx);
                foreach (var element in ocrResult.Elements)
                {
                    var adjustedBounds = AdjustBounds(element.Bounds, (float) page.Width/ocrResult.PageBounds.Width, (float) page.Height/ocrResult.PageBounds.Height);
                    var adjustedFontSize = CalculateFontSize(element.Text, adjustedBounds, gfx);
                    var font = new XFont("Times New Roman", adjustedFontSize, XFontStyle.Regular,
                        new XPdfFontOptions(PdfFontEncoding.Unicode));
                    var adjustedHeight = gfx.MeasureString(element.Text, font).Height;
                    var verticalOffset = (adjustedBounds.Height - adjustedHeight)/2;
                    adjustedBounds.Offset(0, (float) verticalOffset);
                    tf.DrawString(element.Text, font, XBrushes.Transparent, adjustedBounds);
                }
            }
        }

        private static void DrawImageOnPage(PdfPage page, Bitmap img)
        {
            Size realSize = GetRealSize(img);
            page.Width = realSize.Width;
            page.Height = realSize.Height;
            using (XGraphics gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawImage(img, 0, 0, realSize.Width, realSize.Height);
            }
        }

        private static Size GetRealSize(Bitmap img)
        {
            float hAdjust = 72 / img.HorizontalResolution;
            float vAdjust = 72 / img.VerticalResolution;
            double realWidth = img.Width * hAdjust;
            double realHeight = img.Height * vAdjust;
            return new Size((int)realWidth, (int)realHeight);
        }

        private static RectangleF AdjustBounds(Rectangle b, float hAdjust, float vAdjust)
        {
            var adjustedBounds = new RectangleF(b.X * hAdjust, b.Y * vAdjust, b.Width * hAdjust, b.Height * vAdjust);
            return adjustedBounds;
        }

        private static int CalculateFontSize(string text, RectangleF adjustedBounds, XGraphics gfx)
        {
            int fontSizeGuess = Math.Max(1, (int)(adjustedBounds.Height));
            var measuredBoundsForGuess = gfx.MeasureString(text, new XFont("Times New Roman", fontSizeGuess, XFontStyle.Regular));
            double adjustmentFactor = adjustedBounds.Width / measuredBoundsForGuess.Width;
            int adjustedFontSize = Math.Max(1, (int)Math.Round(fontSizeGuess * adjustmentFactor));
            return adjustedFontSize;
        }
    }
}
