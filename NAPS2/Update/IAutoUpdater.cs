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
        void DownloadUpdate(UpdateInfo updateInfo, string downloadPath = null);
        void InstallUpdate(string installerPath);
        void DownloadAndInstallUpdate(UpdateInfo updateInfo);
    }
}
