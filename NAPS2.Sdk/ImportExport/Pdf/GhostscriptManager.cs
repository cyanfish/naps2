using System;
using System.Collections.Generic;
using System.IO;
using NAPS2.Dependencies;

namespace NAPS2.ImportExport.Pdf
{
    public static class GhostscriptManager
    {
        private static readonly List<DownloadMirror> Mirrors = new List<DownloadMirror>
            {
                new DownloadMirror(PlatformSupport.ModernWindows, @"https://github.com/cyanfish/naps2-components/releases/download/gs-9.21/{0}"),
                new DownloadMirror(PlatformSupport.ModernWindows, @"https://sourceforge.net/projects/naps2/files/components/gs-9.21/{0}/download"),
                new DownloadMirror(PlatformSupport.WindowsXp, @"http://xp-mirror.naps2.com/gs-9.21/{0}")
            };

        private static ExternalComponent GhostscriptComponent32 =>
            new ExternalComponent("generic-import", Path.Combine(BasePath, "gs-9.21", "gsdll32.dll"),
            new DownloadInfo("gsdll32.dll.gz", Mirrors, 10.39, "fd7446a05efaf467f5f6a7123c525b0fc7bde711", DownloadFormat.Gzip));

        private static ExternalComponent GhostscriptComponent64 =>
            new ExternalComponent("generic-import", Path.Combine(BasePath, "gs-9.21", "gsdll64.dll"),
                new DownloadInfo("gsdll64.dll.gz", Mirrors, 10.78, "de173f9020c21784727f8c749190d610e4856a0c", DownloadFormat.Gzip));

        public static ExternalComponent GhostscriptComponent => Environment.Is64BitProcess ? GhostscriptComponent64 : GhostscriptComponent32;

        public static bool IsSupported => PlatformSupport.Windows.Validate();

        public static string BasePath { get; set; }
    }
}
