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
    public class GhostscriptPdfImporter : IGenericPdfImporter
    {
        private readonly OcrDependencyManager ocrDependencyManager;
        private readonly IFormFactory formFactory;
        private readonly AppConfigManager appConfigManager;
        private readonly IErrorOutput errorOutput;
        private readonly ThumbnailRenderer thumbnailRenderer;

        public GhostscriptPdfImporter(OcrDependencyManager ocrDependencyManager, IFormFactory formFactory, AppConfigManager appConfigManager, IErrorOutput errorOutput, ThumbnailRenderer thumbnailRenderer)
        {
            this.ocrDependencyManager = ocrDependencyManager;
            this.formFactory = formFactory;
            this.appConfigManager = appConfigManager;
            this.errorOutput = errorOutput;
            this.thumbnailRenderer = thumbnailRenderer;
        }

        public IEnumerable<ScannedImage> Import(string filePath, Func<int, int, bool> progressCallback)
        {
            // TODO: Pass in password somehow - perhaps add an optional parameter to the parent interface?
            // Or cache the value somehow in pdfpasswordprovider

            if (appConfigManager.Config.DisableGenericPdfImport || !VerifyDependencies())
            {
                errorOutput.DisplayError(string.Format(MiscResources.ImportErrorNAPS2Pdf, Path.GetFileName(filePath)));
                yield break;
            }

            // TODO: Maybe allow this to be configured
            int dpi = ScanDpi.Dpi300.ToIntDpi();

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var rasterizer = new GhostscriptRasterizer();
                rasterizer.Open(stream, File.ReadAllBytes(ocrDependencyManager.Components.Ghostscript921.Path));

                for (int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
                {
                    if (!progressCallback(pageNumber - 1, rasterizer.PageCount))
                    {
                        break;
                    }
                    using (var bitmap = (Bitmap) rasterizer.GetPage(dpi, dpi, pageNumber))
                    {
                        var image = new ScannedImage(bitmap, ScanBitDepth.C24Bit, false, -1);
                        image.SetThumbnail(thumbnailRenderer.RenderThumbnail(bitmap));
                        image.Source = new ScannedImage.SourceInfo
                        {
                            FilePath = filePath,
                            PageNumber = pageNumber,
                            FileLock = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                        };
                        yield return image;
                    }
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
