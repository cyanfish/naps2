using System.Collections.Generic;
using NAPS2.Images.Storage;

namespace NAPS2.ImportExport.Pdf
{
    public interface IPdfRenderer
    {
        IEnumerable<IImage> Render(string path, float dpi);
    }
}