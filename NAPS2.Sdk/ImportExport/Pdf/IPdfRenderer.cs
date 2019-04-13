using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Dependencies;
using NAPS2.Images.Storage;

namespace NAPS2.ImportExport.Pdf
{
    public interface IPdfRenderer
    {
        IEnumerable<IImage> Render(string path);
        void ThrowIfCantRender();
        void PromptToInstallIfNeeded(IComponentInstallPrompt componentInstallPrompt);
    }
}
