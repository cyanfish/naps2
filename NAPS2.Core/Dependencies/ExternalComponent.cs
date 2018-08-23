using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Util;

namespace NAPS2.Dependencies
{
    public class ExternalComponent
    {
        public static string BasePath { get; set; }

        public static void InitBasePath(AppConfigManager appConfigManager)
        {
            var customPath = appConfigManager.Config.ComponentsPath;
            BasePath = string.IsNullOrWhiteSpace(customPath)
                ? Paths.Components
                : Environment.ExpandEnvironmentVariables(customPath);
        }

        private readonly PlatformSupport platformSupport;
        
        public ExternalComponent(string id, string path, PlatformSupport platformSupport, DownloadInfo downloadInfo)
        {
            Id = id;
            Path = System.IO.Path.Combine(BasePath, path);
            this.platformSupport = platformSupport;
            DownloadInfo = downloadInfo;
        }

        public string Id { get; }

        public string Path { get; }

        public DownloadInfo DownloadInfo { get; }

        public bool IsInstalled => File.Exists(Path);

        public bool IsSupported => platformSupport == null || platformSupport.Validate();

        public void Install(string sourcePath)
        {
            PathHelper.EnsureParentDirExists(Path);
            File.Move(sourcePath, Path);
        }
    }
}
