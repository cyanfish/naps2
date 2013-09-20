using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NAPS2.Update
{
    public class AutoUpdater : IAutoUpdater
    {
        private static readonly Edition Edition = Edition.
#if STANDALONE // TODO: Provide more variables to differentiate between all 4 editions
StamdaloneZIP
#else
InstallerEXE
#endif
;

        private readonly ILatestVersionSource latestVersionSource;

        public AutoUpdater(ILatestVersionSource latestVersionSource)
        {
            this.latestVersionSource = latestVersionSource;
        }

        public Task<UpdateInfo> CheckForUpdate()
        {
            return new Task<UpdateInfo>(() =>
            {
                //var versionInfos = await latestVersionSource.GetLatestVersionInfo();
                return new UpdateInfo
                {

                };
            });
        }

        public void DownloadUpdate(UpdateInfo updateInfo, string downloadPath = null)
        {
            throw new NotImplementedException();
        }

        public void InstallUpdate(string installerPath)
        {
            throw new NotImplementedException();
        }

        public void DownloadAndInstallUpdate(UpdateInfo updateInfo)
        {
            throw new NotImplementedException();
        }
    }
}