using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Dependencies
{
    public class ExternalComponent : IExternalComponent
    {
        public ExternalComponent(string id, string path, DownloadInfo downloadInfo)
        {
            Id = id;
            Path = path;
            DownloadInfo = downloadInfo;
        }

        public string Id { get; }

        public string Path { get; }

        public DownloadInfo DownloadInfo { get; }

        public bool IsInstalled => File.Exists(Path);

        public void Install(string sourcePath)
        {
            PathHelper.EnsureParentDirExists(Path);
            File.Move(sourcePath, Path);
        }
    }
}
