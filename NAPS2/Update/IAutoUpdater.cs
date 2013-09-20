using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAPS2.Update
{
    public interface IAutoUpdater
    {
        Task<UpdateInfo> CheckForUpdate();
        Task<bool> DownloadUpdate(VersionInfo versionInfo, string savePath);
        Task<bool> InstallUpdate(string installerPath);
        Task<bool> DownloadAndInstallUpdate(VersionInfo versionInfo);
    }
}
