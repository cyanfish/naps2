using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Util;

namespace NAPS2.Dependencies
{
    public class ExternalComponent : IExternalComponent
    {
        private readonly PlatformSupport platformSupport;
        
        public ExternalComponent(string id, string path, PlatformSupport platformSupport, DownloadInfo downloadInfo)
        {
            Id = id;
            Path = path;
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
