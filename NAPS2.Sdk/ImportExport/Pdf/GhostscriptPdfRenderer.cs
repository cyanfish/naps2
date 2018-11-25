using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Ghostscript.NET.Rasterizer;
using NAPS2.Config;
using NAPS2.Dependencies;
using NAPS2.Lang.Resources;
using NAPS2.Scan;
using NAPS2.Util;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace NAPS2.ImportExport.Pdf
{
    public class GhostscriptPdfRenderer : IPdfRenderer
    {
        private readonly IComponentInstallPrompt componentInstallPrompt;
        private readonly AppConfigManager appConfigManager;
        private readonly IErrorOutput errorOutput;
        private readonly GhostscriptManager ghostscriptManager;

        private readonly Lazy<byte[]> gsLibBytes;

        public GhostscriptPdfRenderer(IComponentInstallPrompt componentInstallPrompt, AppConfigManager appConfigManager, IErrorOutput errorOutput, GhostscriptManager ghostscriptManager)
        {
            this.componentInstallPrompt = componentInstallPrompt;
            this.appConfigManager = appConfigManager;
            this.errorOutput = errorOutput;
            this.ghostscriptManager = ghostscriptManager;

            gsLibBytes = new Lazy<byte[]>(() => File.ReadAllBytes(ghostscriptManager.GhostscriptComponent.Path));
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

            using (var doc = PdfReader.Open(path, PdfDocumentOpenMode.InformationOnly))
            {
                // Cap the resolution to 10k pixels in each dimension
                dpi = Math.Min(dpi, (int) (10000 / doc.Pages[0].Height.Inch));
                dpi = Math.Min(dpi, (int) (10000 / doc.Pages[0].Width.Inch));
            }

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var rasterizer = new GhostscriptRasterizer();
                rasterizer.Open(stream, gsLibBytes.Value);

                for (int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
                {
                    var bitmap = (Bitmap)rasterizer.GetPage(dpi, dpi, pageNumber);
                    bitmap.SafeSetResolution(dpi, dpi);
                    yield return bitmap;
                }
            }
        }

        private bool VerifyDependencies()
        {
            if (ghostscriptManager.GhostscriptComponent.IsInstalled)
            {
                return true;
            }
            if (appConfigManager.Config.NoUpdatePrompt)
            {
                return false;
            }
            return componentInstallPrompt.PromptToInstall(ghostscriptManager.GhostscriptComponent, MiscResources.PdfImportComponentNeeded);
        }
    }
}
