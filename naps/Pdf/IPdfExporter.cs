using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace NAPS.Pdf
{
    public interface IPdfExporter
    {
        bool Export(string path, List<Image> images, PdfInfo info, Func<int, bool> progressCallback);
    }
}
