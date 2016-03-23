using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Util
{
    public interface ISaveNotify
    {
        void PdfSaved(string path);
        void ImagesSaved(int imageCount, string path);
    }
}
