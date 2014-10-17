/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Clock.Pdf;
using NAPS2.Scan.Images;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace NAPS2.ImportExport.Pdf
{
    public class TestOcrExporter : IPdfExporter
    {
        public bool Export(string path, IEnumerable<IScannedImage> images, PdfInfo info, Func<int, bool> progressCallback)
        {
            var document = new PdfDocument { Layout = PdfWriterLayout.Compact };
            document.Info.Author = info.Author;
            document.Info.Creator = info.Creator;
            document.Info.Keywords = info.Keywords;
            document.Info.Subject = info.Subject;
            document.Info.Title = info.Title;

            var hocrFiles = new List<string>();

            int i = 1;
            foreach (IScannedImage scannedImage in images)
            {
                using (Image img = scannedImage.GetImage())
                {
                    string tempImageFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
                    string tempHocrFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
                    XDocument hocrDocument;
                    try
                    {
                        img.Save(tempImageFilePath);
                        var tesseractProcess = Process.Start(@"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe",
                            string.Format("\"{0}\" \"{1}\" hocr -l {2}", tempImageFilePath, tempHocrFilePath, "eng"));
                        if (tesseractProcess == null)
                        {
                            // Couldn't start tesseract for some reason
                            throw new Exception();
                        }
                        tesseractProcess.WaitForExit();
                        hocrDocument = XDocument.Load(tempHocrFilePath + ".html");
                    }
                    finally
                    {
                        File.Delete(tempImageFilePath);
                        File.Delete(tempHocrFilePath + ".html");
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
                    XTextFormatter tf = new XTextFormatter(gfx);
                    foreach (
                        var wordElement in
                            hocrDocument.Descendants()
                                .Where(x => x.Attributes("class").Any(y => y.Value == "ocrx_word")))
                    {
                        string word = wordElement.Value;
                        var bounds = new RectangleF();
                        string bbox = wordElement.Attributes("title").Select(x => x.Value).SingleOrDefault();
                        int fontSize = 10;
                        if (bbox != null)
                        {
                            string[] parts = bbox.Split(' ');
                            if (parts.Length == 5 && parts[0] == "bbox")
                            {
                                float x1 = int.Parse(parts[1]) * hAdjust, y1 = int.Parse(parts[2]) * vAdjust;
                                float x2 = int.Parse(parts[3]) * hAdjust, y2 = int.Parse(parts[4]) * vAdjust;
                                bounds = new RectangleF(x1, y1, x2 - x1, y2 - y1);
                                fontSize = Math.Max(10, (int)(y2 - y1));
                            }
                        }
                        tf.DrawString(word, new XFont("Times New Roman", fontSize, XFontStyle.Regular), XBrushes.Transparent, bounds);
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
