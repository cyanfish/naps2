using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;

namespace NAPS2.Update
{
    public class AutoUpdater : IAutoUpdater
    {
        private const Edition CURRENT_EDITION = Edition.
#if STANDALONE // TODO: Provide more variables to differentiate between all 4 editions
StamdaloneZIP
#else
InstallerEXE
#endif
;

        private readonly ILatestVersionSource latestVersionSource;
        private readonly ICurrentVersionSource currentVersionSource;
        private readonly IUrlFileDownloader urlFileDownloader;

        public AutoUpdater(ILatestVersionSource latestVersionSource, ICurrentVersionSource currentVersionSource, IUrlFileDownloader urlFileDownloader)
        {
            this.latestVersionSource = latestVersionSource;
            this.currentVersionSource = currentVersionSource;
            this.urlFileDownloader = urlFileDownloader;
        }

        public Task<UpdateInfo> CheckForUpdate()
        {
            return Task.Factory.StartNew(() =>
            {
                var versionInfos = latestVersionSource.GetLatestVersionInfo().Result;
                var currentEditionVersionInfo = versionInfos.Single(x => x.Edition == CURRENT_EDITION);
                return new UpdateInfo
                {
                    HasUpdate = new Version(currentEditionVersionInfo.LatestVersion) > currentVersionSource.GetCurrentVersion(),
                    VersionInfo = currentEditionVersionInfo
                };
            });
        }

        public Task<bool> DownloadUpdate(VersionInfo versionInfo, string savePath)
        {
            return Task.Factory.StartNew(() =>
            {
                // Store to a temp file while downloading
                string tempPath = Path.Combine(Paths.Temp, Path.GetRandomFileName());

                urlFileDownloader.DownloadFile(versionInfo.DownloadUrl, savePath);

                // Now that the download is complete, rename/move the temp file
                File.Move(tempPath, savePath);

                return true;
            });
        }

        public Task<bool> InstallUpdate(string installerPath)
        {
            return Task.Factory.StartNew(() =>
            {
                if (installerPath == null)
                {
                    throw new ArgumentNullException("installerPath");
                }
                var extension = Path.GetExtension(installerPath).ToLowerInvariant();
                if (extension != "exe" && extension != "msi")
                {
                    throw new ArgumentException("The installer could not be started because it is not an executable.");
                }
                var process = new Process();
                process.Exited += (sender, args) =>
                {
                    lock (process)
                    {
                        Monitor.Pulse(process);
                    }
                };
                process.StartInfo.FileName = installerPath;
                lock (process)
                {
                    if (!process.Start())
                    {
                        return false;
                    }
                    Monitor.Wait(process);
                }
                return process.ExitCode == 0;
            });
        }

        public Task<bool> DownloadAndInstallUpdate(VersionInfo versionInfo)
        {
            return Task.Factory.StartNew(() =>
            {
                var savePath = Path.Combine(Paths.Temp, versionInfo.FileName);
                if (!DownloadUpdate(versionInfo, savePath).Result)
                {
                    return false;
                }
                if (!InstallUpdate(savePath).Result)
                {
                    return false;
                }
                return true;
            });
        }
    }
}