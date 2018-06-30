using System.Collections.Generic;
using System.Drawing;

namespace NAPS2.ImportExport.Pdf
{
    public interface IPdfRenderer
    {
        IEnumerable<Bitmap> Render(string path);

        void ThrowIfCantRender();
    }
}