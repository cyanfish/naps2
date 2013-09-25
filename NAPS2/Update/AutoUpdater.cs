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
        private readonly ILatestVersionSource latestVersionSource;
        private readonly ICurrentVersionSource currentVersionSource;
        private readonly IUrlFileDownloader urlFileDownloader;
        private readonly Edition edition;

        public AutoUpdater(ILatestVersionSource latestVersionSource, ICurrentVersionSource currentVersionSource, IUrlFileDownloader urlFileDownloader, Edition edition)
        {
            this.latestVersionSource = latestVersionSource;
            this.currentVersionSource = currentVersionSource;
            this.urlFileDownloader = urlFileDownloader;
            this.edition = edition;
        }

        public Task<UpdateInfo> CheckForUpdate()
        {
            return Task.Factory.StartNew(() =>
            {
                var versionInfos = latestVersionSource.GetLatestVersionInfo().Result;
                var currentEditionVersionInfo = versionInfos.Single(x => x.Edition == edition);
                return new UpdateInfo
                {
                    HasUpdate = true,//new Version(currentEditionVersionInfo.LatestVersion) > currentVersionSource.GetCurrentVersion(),
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

                urlFileDownloader.DownloadFile(versionInfo.DownloadUrl, tempPath);

                // Handle the somewhat tricky situation where the file to save to already exists
                if (File.Exists(savePath))
                {
                    // Try overwriting the file, which should work if it isn't locked by another process
                    try
                    {
                        File.Delete(savePath);
                    }
                    catch (IOException)
                    {
                        File.Delete(tempPath);
                        return false;
                    }
                }

                // Now that the download is complete, rename/move the temp file
                File.Move(tempPath, savePath);

                return true;
            });
        }

        public Task<bool> InstallUpdate(string installerPath, string arguments = null)
        {
            return Task.Factory.StartNew(() =>
            {
                if (installerPath == null)
                {
                    throw new ArgumentNullException("installerPath");
                }
                var extension = Path.GetExtension(installerPath).ToLowerInvariant();
                if (extension != ".exe" && extension != ".msi")
                {
                    throw new ArgumentException("The installer could not be started because it is not an executable.");
                }
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = installerPath,
                        Arguments = arguments ?? ""
                    }
                };
                if (!process.Start())
                {
                    return false;
                }
                process.WaitForExit();
                return process.ExitCode == 0;
            });
        }

        public Task<bool> DownloadAndInstallUpdate(VersionInfo versionInfo)
        {
            return Task.Factory.StartNew(() =>
            {
                var saveFolder = Path.Combine(Paths.Temp, Path.GetRandomFileName());
                var savePath = Path.Combine(saveFolder, versionInfo.FileName);
                Directory.CreateDirectory(saveFolder);
                try
                {
                    if (!DownloadUpdate(versionInfo, savePath).Result)
                    {
                        return false;
                    }
                    if (!InstallUpdate(savePath, versionInfo.InstallArguments).Result)
                    {
                        return false;
                    }
                    return true;
                }
                finally
                {
                    Directory.Delete(saveFolder, true);
                }
            });
        }
    }
}
