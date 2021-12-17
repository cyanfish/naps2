using NAPS2.Scan;

namespace NAPS2.ImportExport;

public interface IAutoSave
{
    Task<bool> Save(AutoSaveSettings settings, List<ScannedImage> images);
}