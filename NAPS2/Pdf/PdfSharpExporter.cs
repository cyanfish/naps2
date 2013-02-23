/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

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
using System.Text;

using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace NAPS2.Pdf
{
    public class PdfSharpExporter : IPdfExporter
    {
        public bool Export(string path, List<Image> images, PdfInfo info, Func<int, bool> progressCallback)
        {
            PdfDocument document = new PdfDocument();
            document.Layout = PdfSharp.Pdf.IO.PdfWriterLayout.Compact;
            document.Info.Author = info.Author;
            document.Info.Creator = info.Creator;
            document.Info.Keywords = info.Keywords;
            document.Info.Subject = info.Subject;
            document.Info.Title = info.Title;
            int i = 1;
            foreach (Image img in images)
            {
                if (!progressCallback(i))
                {
                    return false;
                }
                double realWidth = img.Width / img.HorizontalResolution * 72;
                double realHeight = img.Height / img.VerticalResolution * 72;
                PdfPage newPage = document.AddPage();
                newPage.Width = (int)realWidth;
                newPage.Height = (int)realHeight;
                XGraphics gfx = XGraphics.FromPdfPage(newPage);
                gfx.DrawImage(img, 0, 0, (int)realWidth, (int)realHeight);
                i++;
            }
            document.Save(path);
            return true;
        }
    }
}
