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
using NAPS2.Ocr;
using NAPS2.Scan.Images;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
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

        public bool Export(string path, IEnumerable<IScannedImage> images, PdfSettings settings, string ocrLanguageCode, Func<int, bool> progressCallback)
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

            int i = 1;
            foreach (IScannedImage scannedImage in images)
            {
                using (Stream stream = scannedImage.GetImageStream())
                using (var img = new Bitmap(stream))
                {
                    if (!progressCallback(i))
                    {
                        return false;
                    }

                    OcrResult ocrResult = null;
                    if (ocrLanguageCode != null && ocrEngine.CanProcess(ocrLanguageCode))
                    {
                        ocrResult = ocrEngine.ProcessImage(img, ocrLanguageCode);
                    }

                    float hAdjust = 72 / img.HorizontalResolution;
                    float vAdjust = 72 / img.VerticalResolution;
                    double realWidth = img.Width * hAdjust;
                    double realHeight = img.Height * vAdjust;
                    PdfPage newPage = document.AddPage();
                    newPage.Width = (int)realWidth;
                    newPage.Height = (int)realHeight;
                    using (XGraphics gfx = XGraphics.FromPdfPage(newPage))
                    {
                        if (ocrResult != null)
                        {
                            var tf = new XTextFormatter(gfx);
                            foreach (var element in ocrResult.Elements)
                            {
                                var adjustedBounds = AdjustBounds(element.Bounds, hAdjust, vAdjust);
                                var adjustedFontSize = CalculateFontSize(element.Text, adjustedBounds, gfx);
                                var font = new XFont("Times New Roman", adjustedFontSize, XFontStyle.Regular,
                                    new XPdfFontOptions(PdfFontEncoding.Unicode));
                                tf.DrawString(element.Text, font, XBrushes.Transparent, adjustedBounds);
                            }
                        }
                        gfx.DrawImage(img, 0, 0, (int)realWidth, (int)realHeight);
                    }
                    i++;
                }
            }
            document.Save(path);
            return true;
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
