using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Dependencies
{
    public class MultiFileExternalComponent : IExternalComponent
    {
        private readonly string[] subPaths;

        public MultiFileExternalComponent(string id, string rootPath, string[] subPaths, DownloadInfo downloadInfo)
        {
            Id = id;
            RootPath = rootPath;
            DownloadInfo = downloadInfo;
            this.subPaths = subPaths;
        }

        public string Id { get; }

        public string RootPath { get; }

        public DownloadInfo DownloadInfo { get; }

        public bool IsInstalled => subPaths.Any(sub => File.Exists(Path.Combine(RootPath, sub)));

        public void Install(string sourcePath)
        {
            foreach (var sub in subPaths)
            {
                var srcFile = Path.Combine(sourcePath, sub);
                var dstFile = Path.Combine(RootPath, sub);
                if (File.Exists(srcFile))
                {
                    PathHelper.EnsureParentDirExists(dstFile);
                    File.Move(srcFile, dstFile);
                }
            }
        }
    }
}
