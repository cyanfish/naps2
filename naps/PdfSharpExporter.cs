using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace NAPS
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
