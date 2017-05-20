using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ghostscript.NET.Rasterizer;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Ocr;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.ImportExport.Pdf
{
    public class GhostscriptPdfRenderer : IPdfRenderer
    {
        private readonly OcrDependencyManager ocrDependencyManager;
        private readonly IFormFactory formFactory;
        private readonly AppConfigManager appConfigManager;
        private readonly IErrorOutput errorOutput;
        private readonly ThumbnailRenderer thumbnailRenderer;

        private readonly Lazy<byte[]> gsLibBytes;

        public GhostscriptPdfRenderer(OcrDependencyManager ocrDependencyManager, IFormFactory formFactory, AppConfigManager appConfigManager, IErrorOutput errorOutput, ThumbnailRenderer thumbnailRenderer)
        {
            this.ocrDependencyManager = ocrDependencyManager;
            this.formFactory = formFactory;
            this.appConfigManager = appConfigManager;
            this.errorOutput = errorOutput;
            this.thumbnailRenderer = thumbnailRenderer;

            gsLibBytes = new Lazy<byte[]>(() => File.ReadAllBytes(ocrDependencyManager.Components.Ghostscript921.Path));
        }

        public IEnumerable<Bitmap> Render(string path)
        {
            if (appConfigManager.Config.DisableGenericPdfImport || !VerifyDependencies())
            {
                errorOutput.DisplayError(string.Format(MiscResources.ImportErrorNAPS2Pdf, Path.GetFileName(path)));
                yield break;
            }

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
            if (ocrDependencyManager.Components.Ghostscript921.IsInstalled)
            {
                return true;
            }
            if (appConfigManager.Config.NoUpdatePrompt)
            {
                return false;
            }
            // TODO: Change behaviour for console
            if (MessageBox.Show(MiscResources.PdfImportComponentNeeded, MiscResources.DownloadNeeded, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var progressForm = formFactory.Create<FDownloadProgress>();
                progressForm.QueueFile(ocrDependencyManager.Downloads.Ghostscript921,
                    path => ocrDependencyManager.Components.Ghostscript921.Install(path));
                progressForm.ShowDialog();
            }
            return ocrDependencyManager.Components.Ghostscript921.IsInstalled;
        }
    }
}
