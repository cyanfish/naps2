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
            MoveRecursive(new DirectoryInfo(sourcePath), new DirectoryInfo(RootPath));
        }

        private void MoveRecursive(DirectoryInfo sourceDir, DirectoryInfo destDir)
        {
            if (!destDir.Exists)
            {
                destDir.Create();
            }
            foreach (var srcFile in sourceDir.EnumerateFiles())
            {
                srcFile.MoveTo(Path.Combine(destDir.FullName, srcFile.Name));
            }
            foreach (var subDir in sourceDir.EnumerateDirectories())
            {
                MoveRecursive(subDir, new DirectoryInfo(Path.Combine(destDir.FullName, subDir.Name)));
            }
        }
    }
}
