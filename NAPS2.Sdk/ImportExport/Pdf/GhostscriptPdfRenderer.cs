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
using PdfSharp.Pdf.IO;

namespace NAPS2.ImportExport.Pdf
{
    public class GhostscriptPdfRenderer : IPdfRenderer
    {
        private static byte[] _gsLibBytes;

        public IEnumerable<Bitmap> Render(string path)
        {
            ThrowIfCantRender();

            if (_gsLibBytes == null)
            {
                _gsLibBytes = File.ReadAllBytes(GhostscriptManager.GhostscriptComponent.Path);
            }

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
                rasterizer.Open(stream, _gsLibBytes);

                for (int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
                {
                    var bitmap = (Bitmap)rasterizer.GetPage(dpi, dpi, pageNumber);
                    bitmap.SafeSetResolution(dpi, dpi);
                    yield return bitmap;
                }
            }
        }

        public void PromptToInstallIfNeeded(IComponentInstallPrompt componentInstallPrompt)
        {
            if (AppConfig.Current.NoUpdatePrompt || AppConfig.Current.DisableGenericPdfImport)
            {
                return;
            }
            componentInstallPrompt.PromptToInstall(GhostscriptManager.GhostscriptComponent, MiscResources.PdfImportComponentNeeded);
        }

        public void ThrowIfCantRender()
        {
            if (AppConfig.Current.DisableGenericPdfImport || !GhostscriptManager.GhostscriptComponent.IsInstalled)
            {
                throw new ImageRenderException();
            }
        }
    }
}
