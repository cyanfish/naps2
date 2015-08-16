/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2015       Luca De Petrillo
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
using System.Drawing.Imaging;

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
            var document = new PdfDocument { Layout = PdfWriterLayout.Compact };
            document.Info.Author = settings.Metadata.Author;
            document.Info.Creator = settings.Metadata.Creator;
            document.Info.Keywords = settings.Metadata.Keywords;
            document.Info.Subject = settings.Metadata.Subject;
            document.Info.Title = settings.Metadata.Title;

            if (settings.Encryption.EncryptPdf)
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
                    XGraphics gfx = XGraphics.FromPdfPage(newPage);
                    if (ocrResult != null)
                    {
                        var tf = new XTextFormatter(gfx);
                        foreach (var element in ocrResult.Elements)
                        {
                            var adjustedBounds = AdjustBounds(element.Bounds, hAdjust, vAdjust);
                            var adjustedFontSize = CalculateFontSize(element.Text, adjustedBounds, gfx);
                            tf.DrawString(element.Text, new XFont("Times New Roman", adjustedFontSize, XFontStyle.Regular), XBrushes.Transparent, adjustedBounds);
                        }
                    }

                    if (scannedImage.IsHighQuality() && settings.ImageSettings.CompressImages)
                    {
                        // Compress the image to JPEG and use it if smaller than the original one.
                        var quality = Math.Max(Math.Min(settings.ImageSettings.JpegQuality, 100), 0);
                        var encoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
                        var encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                        using (var streamJpg = new MemoryStream())
                        {
                            img.Save(streamJpg, encoder, encoderParams);
                            if (streamJpg.Length < stream.Length) 
                            {
                                using (var imgJpg = Bitmap.FromStream(streamJpg))
                                {
                                    gfx.DrawImage(imgJpg, 0, 0, (int)realWidth, (int)realHeight);
                                }
                            }
                            else
                            {
                                gfx.DrawImage(img, 0, 0, (int)realWidth, (int)realHeight);
                            }
                            
                        }
                    }
                    else
                    {
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
