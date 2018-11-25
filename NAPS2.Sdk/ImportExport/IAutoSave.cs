using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport
{
    public interface IAutoSave
    {
        Task<bool> Save(AutoSaveSettings settings, List<ScannedImage> images, ISaveNotify notify);
    }
}
