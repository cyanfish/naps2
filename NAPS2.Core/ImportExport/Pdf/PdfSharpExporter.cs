/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

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
using System.Linq;
using NAPS2.Config;
using NAPS2.Ocr;
using NAPS2.Scan.Images;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSharpExporter : IPdfExporter
    {
        private readonly IUserConfigManager userConfigManager;
        private readonly IOcrEngine ocrEngine;

        public PdfSharpExporter(IOcrEngine ocrEngine, IUserConfigManager userConfigManager)
        {
            this.ocrEngine = ocrEngine;
            this.userConfigManager = userConfigManager;
        }

        public bool Export(string path, IEnumerable<IScannedImage> images, PdfInfo info, Func<int, bool> progressCallback)
        {
            var document = new PdfDocument { Layout = PdfWriterLayout.Compact };
            document.Info.Author = info.Author;
            document.Info.Creator = info.Creator;
            document.Info.Keywords = info.Keywords;
            document.Info.Subject = info.Subject;
            document.Info.Title = info.Title;
            int i = 1;
            foreach (IScannedImage scannedImage in images)
            {
                using (Image img = scannedImage.GetImage())
                {
                    OcrResult ocrResult = null;
                    if (userConfigManager.Config.EnableOcr && ocrEngine.CanProcess(userConfigManager.Config.OcrLanguageCode))
                    {
                        ocrResult = ocrEngine.ProcessImage(img, userConfigManager.Config.OcrLanguageCode);
                    }

                    if (!progressCallback(i))
                    {
                        return false;
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
                            var b = element.Bounds;
                            var adjustedBounds = new RectangleF(b.X * hAdjust, b.Y * vAdjust, b.Width * hAdjust, b.Height * vAdjust);
                            int fontSize = Math.Max(10, element.Bounds.Height);
                            tf.DrawString(element.Text, new XFont("Times New Roman", fontSize, XFontStyle.Regular), XBrushes.Transparent, adjustedBounds);
                        }
                    }
                    gfx.DrawImage(img, 0, 0, (int)realWidth, (int)realHeight);
                    i++;
                }
            }
            document.Save(path);
            return true;
        }
    }
}
