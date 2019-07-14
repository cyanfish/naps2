using System.Collections.Generic;
using System.Threading.Tasks;
using NAPS2.Scan;
using NAPS2.Images;

namespace NAPS2.ImportExport
{
    public interface IAutoSave
    {
        Task<bool> Save(AutoSaveSettings settings, List<ScannedImage> images);
    }
}
