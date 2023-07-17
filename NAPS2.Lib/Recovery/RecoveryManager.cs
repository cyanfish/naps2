using NAPS2.ImportExport.Images;
using NAPS2.Scan;

namespace NAPS2.Recovery;

public class RecoveryManager
{
    private readonly ScanningContext _scanningContext;

    public RecoveryManager(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public RecoverableFolder? GetLatestRecoverableFolder()
    {
        if (_scanningContext.RecoveryPath == null)
        {
            throw new InvalidOperationException("ScanningContext.RecoveryPath must be set to use RecoveryManager");
        }
        if (_scanningContext.FileStorageManager == null)
        {
            throw new InvalidOperationException(
                "ScanningContext.FileStorageManager must be set to use RecoveryManager");
        }
        // Find the most recent recovery folder that can be locked (i.e. isn't in use already)
        return new DirectoryInfo(_scanningContext.RecoveryPath)
            .EnumerateDirectories()
            .OrderByDescending(x => x.LastWriteTime)
            .Select(TryLockRecoverableFolder)
            .WhereNotNull()
            .FirstOrDefault();
    }

    private RecoverableFolder? TryLockRecoverableFolder(DirectoryInfo directory)
    {
        try
        {
            return RecoverableFolder.TryCreate(_scanningContext, directory);
        }
        catch (Exception)
        {
            // Some problem, e.g. the folder is already locked
            return null;
        }
    }
}