using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.ImportExport.Profiles;

internal class ProfileTransfer : TransferHelper<ScanProfile, ProfileTransferData>
{
    protected override ProfileTransferData AsData(ScanProfile profile)
    {
        var profileCopy = profile.Clone();
        profileCopy.IsDefault = false;
        profileCopy.IsLocked = false;
        profileCopy.IsDeviceLocked = false;
        return new ProfileTransferData
        {
            ProcessId = Process.GetCurrentProcess().Id,
            ScanProfileXml = profileCopy.ToXml(),
            Locked = profile.IsLocked
        };
    }
}