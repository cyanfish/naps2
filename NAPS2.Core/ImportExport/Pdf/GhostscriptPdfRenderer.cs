using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ghostscript.NET.Rasterizer;
using NAPS2.Config;
using NAPS2.Dependencies;
using NAPS2.Lang.Resources;
using NAPS2.Scan;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.ImportExport.Pdf
{
    public class GhostscriptPdfRenderer : IPdfRenderer
    {
        private readonly IComponentInstallPrompt componentInstallPrompt;
        private readonly AppConfigManager appConfigManager;
        private readonly IErrorOutput errorOutput;

        private readonly Lazy<byte[]> gsLibBytes;

        public GhostscriptPdfRenderer(IComponentInstallPrompt componentInstallPrompt, AppConfigManager appConfigManager, IErrorOutput errorOutput)
        {
            this.componentInstallPrompt = componentInstallPrompt;
            this.appConfigManager = appConfigManager;
            this.errorOutput = errorOutput;

            gsLibBytes = new Lazy<byte[]>(() => File.ReadAllBytes(Dependencies.GhostscriptComponent.Path));
            ExternalComponent.InitBasePath(appConfigManager);
        }

        public void ThrowIfCantRender()
        {
            if (appConfigManager.Config.DisableGenericPdfImport || !VerifyDependencies())
            {
                throw new ImageRenderException();
            }
        }

        public IEnumerable<Bitmap> Render(string path)
        {
            ThrowIfCantRender();

            // TODO: Maybe allow this to be configured
            int dpi = ScanDpi.Dpi300.ToIntDpi();

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var rasterizer = new GhostscriptRasterizer();
                rasterizer.Open(stream, gsLibBytes.Value);

                for (int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
                {
                    var bitmap = (Bitmap)rasterizer.GetPage(dpi, dpi, pageNumber);
                    bitmap.SetResolution(dpi, dpi);
                    yield return bitmap;
                }
            }
        }

        private bool VerifyDependencies()
        {
            if (Dependencies.GhostscriptComponent.IsInstalled)
            {
                return true;
            }
            if (appConfigManager.Config.NoUpdatePrompt)
            {
                return false;
            }
            return componentInstallPrompt.PromptToInstall(Dependencies.GhostscriptDownload, Dependencies.GhostscriptComponent, MiscResources.PdfImportComponentNeeded);
        }

        public static class Dependencies
        {
            private const string DOWNLOAD_URL_FORMAT = @"https://sourceforge.net/projects/naps2/files/components/gs-9.21/{0}/download";
            private const string XP_DOWNLOAD_URL_FORMAT = @"http://xp-mirror.naps2.com/gs-9.21/{0}";

            private static readonly DownloadInfo GhostscriptDownload32 = new DownloadInfo("gsdll32.dll.gz", DOWNLOAD_URL_FORMAT, XP_DOWNLOAD_URL_FORMAT, 10.39, "fd7446a05efaf467f5f6a7123c525b0fc7bde711", DownloadFormat.Gzip);

            private static readonly DownloadInfo GhostscriptDownload64 = new DownloadInfo("gsdll64.dll.gz", DOWNLOAD_URL_FORMAT, XP_DOWNLOAD_URL_FORMAT, 10.78, "de173f9020c21784727f8c749190d610e4856a0c", DownloadFormat.Gzip);

            public static DownloadInfo GhostscriptDownload => Environment.Is64BitProcess ? GhostscriptDownload64 : GhostscriptDownload32;

            private static readonly ExternalComponent GhostscriptComponent32 = new ExternalComponent("generic-import", @"gs-9.21\gsdll32.dll", PlatformSupport.Windows);

            private static readonly ExternalComponent GhostscriptComponent64 = new ExternalComponent("generic-import", @"gs-9.21\gsdll64.dll", PlatformSupport.Windows);

            public static ExternalComponent GhostscriptComponent => Environment.Is64BitProcess ? GhostscriptComponent64 : GhostscriptComponent32;
        }
    }
}
