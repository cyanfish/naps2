using System.Globalization;
using System.Text;
using System.Threading;
using NAPS2.Ocr;
using NAPS2.Scan;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;

namespace NAPS2.ImportExport.Pdf;

public class PdfSharpExporter : PdfExporter
{
    static PdfSharpExporter()
    {
        if (PlatformCompat.System.UseUnixFontResolver)
        {
            GlobalFontSettings.FontResolver = new UnixFontResolver();
        }
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private readonly ScanningContext _scanningContext;

    public PdfSharpExporter(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public override async Task<bool> Export(string path, ICollection<ProcessedImage> images,
        PdfExportParams exportParams, OcrParams? ocrParams = null, ProgressHandler? progressCallback = null,
        CancellationToken cancelToken = default)
    {
        return await Task.Run(() =>
        {
            var compat = exportParams.Compat;

            var document = new PdfDocument();
            // TODO: Defaults for API use
            document.Info.Author = exportParams.Metadata?.Author;
            document.Info.Creator = exportParams.Metadata?.Creator;
            document.Info.Keywords = exportParams.Metadata?.Keywords;
            document.Info.Subject = exportParams.Metadata?.Subject;
            document.Info.Title = exportParams.Metadata?.Title;

            if (exportParams.Encryption?.EncryptPdf == true
                && (!string.IsNullOrEmpty(exportParams.Encryption.OwnerPassword) ||
                    !string.IsNullOrEmpty(exportParams.Encryption.UserPassword)))
            {
                document.SecuritySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted128Bit;
                if (!string.IsNullOrEmpty(exportParams.Encryption.OwnerPassword))
                {
                    document.SecuritySettings.OwnerPassword = exportParams.Encryption.OwnerPassword;
                }

                if (!string.IsNullOrEmpty(exportParams.Encryption.UserPassword))
                {
                    document.SecuritySettings.UserPassword = exportParams.Encryption.UserPassword;
                }

                document.SecuritySettings.PermitAccessibilityExtractContent =
                    exportParams.Encryption.AllowContentCopyingForAccessibility;
                document.SecuritySettings.PermitAnnotations = exportParams.Encryption.AllowAnnotations;
                document.SecuritySettings.PermitAssembleDocument =
                    exportParams.Encryption.AllowDocumentAssembly;
                document.SecuritySettings.PermitExtractContent = exportParams.Encryption.AllowContentCopying;
                document.SecuritySettings.PermitFormsFill = exportParams.Encryption.AllowFormFilling;
                document.SecuritySettings.PermitFullQualityPrint =
                    exportParams.Encryption.AllowFullQualityPrinting;
                document.SecuritySettings.PermitModifyDocument =
                    exportParams.Encryption.AllowDocumentModification;
                document.SecuritySettings.PermitPrint = exportParams.Encryption.AllowPrinting;
            }

            IOcrEngine? ocrEngine = null;
            if (ocrParams?.LanguageCode != null)
            {
                var activeEngine = _scanningContext.OcrEngine;
                if (activeEngine == null)
                {
                    Log.Error("Supported OCR engine not installed.", ocrParams.LanguageCode);
                }
                else
                {
                    ocrEngine = activeEngine;
                }
            }

            bool result = ocrEngine != null
                ? BuildDocumentWithOcr(progressCallback, cancelToken, document, compat, images, ocrParams!,
                    ocrEngine)
                : BuildDocumentWithoutOcr(progressCallback, cancelToken, document, compat, images);
            if (!result)
            {
                return false;
            }

            var now = DateTime.Now;
            document.Info.CreationDate = now;
            document.Info.ModificationDate = now;
            if (compat == PdfCompat.PdfA1B)
            {
                PdfAHelper.SetCidStream(document);
                PdfAHelper.DisableTransparency(document);
            }

            if (compat != PdfCompat.Default)
            {
                PdfAHelper.SetColorProfile(document);
                PdfAHelper.SetCidMap(document);
                PdfAHelper.CreateXmpMetadata(document, compat);
            }

            FileSystemHelper.EnsureParentDirExists(path);
            document.Save(path);
            return true;
        });
    }

    private bool BuildDocumentWithoutOcr(ProgressHandler? progressCallback, CancellationToken cancelToken,
        PdfDocument document, PdfCompat compat, ICollection<ProcessedImage> images)
    {
        int progress = 0;
        progressCallback?.Invoke(progress, images.Count);
        foreach (var image in images)
        {
            // TODO: To avoid the issue of file size explosion for transformed pdfs (as they get rendered at a default
            // 300 dpi), we could consider using Pdfium to apply the transform to the underlying PDF file. For example,
            // doing color shifting on individual text + image objects, or applying matrix changes.
            // TODO: We also can consider doing this even for scanned image transforms - e.g. for deskew, maybe rather
            // than rasterize that, rely on the pdf to do the skew transform, which should render better at different
            // scaling.
            if (image.Storage is ImageFileStorage fileStorage && IsPdfFile(fileStorage) && image.TransformState.IsEmpty)
            {
                CopyPdfPageToDoc(document, fileStorage);
            }
            else
            {
                // TODO: Dedup from other method
                var format = image.Metadata.Lossless ? ImageFileFormat.Png : ImageFileFormat.Jpeg;
                using var renderedImage = _scanningContext.ImageContext.Render(image);
                using Stream stream = renderedImage.SaveToMemoryStream(format);
                using var img = XImage.FromStream(stream);
                if (cancelToken.IsCancellationRequested)
                {
                    return false;
                }

                PdfPage page = document.AddPage();
                DrawImageOnPage(page, img, compat);
            }
            progress++;
            progressCallback?.Invoke(progress, images.Count);
        }
        return true;
    }

    private static bool IsPdfFile(ImageFileStorage imageFileStorage) => Path.GetExtension(imageFileStorage.FullPath)
        ?.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase) ?? false;

    // TODO: Try to combine/simplify this with the non-ocr method
    private bool BuildDocumentWithOcr(ProgressHandler? progressCallback, CancellationToken cancelToken,
        PdfDocument document, PdfCompat compat, ICollection<ProcessedImage> images, OcrParams ocrParams,
        IOcrEngine ocrEngine)
    {
        int progress = 0;
        progressCallback?.Invoke(progress, images.Count);

        List<(PdfPage, Task<OcrResult?>)> ocrPairs = new();

        // Step 1: Create the pages, draw the images, and start OCR
        foreach (var image in images)
        {
            if (cancelToken.IsCancellationRequested)
            {
                break;
            }

            PdfPage page;
            bool importedPdfPassThrough = false;

            if (image.Storage is ImageFileStorage fileStorage && IsPdfFile(fileStorage) && image.TransformState.IsEmpty)
            {
                importedPdfPassThrough = true;
                page = CopyPdfPageToDoc(document, fileStorage);
                if (PageContainsText(page))
                {
                    // Since this page already contains text, don't use OCR
                    continue;
                }
            }
            else
            {
                page = document.AddPage();
            }

            var format = image.Metadata.Lossless ? ImageFileFormat.Png : ImageFileFormat.Jpeg;
            var ext = format == ImageFileFormat.Png ? ".png" : ".jpg";
            string ocrTempFilePath = Path.Combine(_scanningContext.TempFolderPath, Path.GetRandomFileName() + ext);
            using var renderedImage = _scanningContext.ImageContext.Render(image);
            using var stream = renderedImage.SaveToMemoryStream(format);
            using var img = XImage.FromStream(stream);
            if (cancelToken.IsCancellationRequested)
            {
                break;
            }
            if (!importedPdfPassThrough)
            {
                DrawImageOnPage(page, img, compat);
            }

            if (cancelToken.IsCancellationRequested)
            {
                break;
            }

            if (!_scanningContext.OcrRequestQueue.HasCachedResult(ocrEngine, image, ocrParams))
            {
                // Save the image to a file for use in OCR.
                // We don't need to delete this file as long as we pass it to OcrRequestQueue.Enqueue, which takes 
                // ownership and guarantees its eventual deletion.
                using var fileStream = new FileStream(ocrTempFilePath, FileMode.CreateNew);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            if (cancelToken.IsCancellationRequested)
            {
                File.Delete(ocrTempFilePath);
                break;
            }

            // Start OCR
            var ocrTask = _scanningContext.OcrRequestQueue.Enqueue(
                ocrEngine, image, ocrTempFilePath, ocrParams, OcrPriority.Foreground, cancelToken);
            ocrTask.ContinueWith(_ =>
            {
                // This is the best place to put progress reporting
                // Long-running OCR is done, and drawing text on the page (step 2) is very fast
                if (!cancelToken.IsCancellationRequested)
                {
                    Interlocked.Increment(ref progress);
                    progressCallback?.Invoke(progress, images.Count);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).AssertNoAwait();
            // Record the page and task for step 2
            ocrPairs.Add((page, ocrTask));
        }

        // Step 2: Wait for all the OCR results, and draw the text on each page
        foreach (var (page, ocrTask) in ocrPairs)
        {
            if (cancelToken.IsCancellationRequested)
            {
                break;
            }
            if (ocrTask.Result == null)
            {
                continue;
            }
            DrawOcrTextOnPage(page, ocrTask.Result);
        }

        return !cancelToken.IsCancellationRequested;
    }

    private bool PageContainsText(PdfPage page)
    {
        var elements = page.Contents.Elements;
        for (int i = 0; i < elements.Count; i++)
        {
            string textAndFormatting = elements.GetDictionary(i).Stream.ToString();
            var reader = new StringReader(textAndFormatting);
            bool inTextBlock = false;
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.EndsWith("BT", StringComparison.InvariantCulture))
                {
                    inTextBlock = true;
                }
                else if (line.EndsWith("ET", StringComparison.InvariantCulture))
                {
                    inTextBlock = false;
                }
                else if (inTextBlock &&
                         (line.EndsWith("TJ", StringComparison.InvariantCulture) ||
                          line.EndsWith("Tj", StringComparison.InvariantCulture)
                          || line.EndsWith("\"", StringComparison.InvariantCulture) ||
                          line.EndsWith("'", StringComparison.InvariantCulture)))
                {
                    // Text-showing operators
                    return true;
                }
            }
        }
        return false;
    }

    private PdfPage CopyPdfPageToDoc(PdfDocument destDoc, ImageFileStorage imageFileStorage)
    {
        // Pull the PDF content directly to maintain objects, dpi, etc.
        PdfDocument sourceDoc = PdfReader.Open(imageFileStorage.FullPath, PdfDocumentOpenMode.Import);
        PdfPage sourcePage = sourceDoc.Pages.Cast<PdfPage>().Single();
        PdfPage destPage = destDoc.AddPage(sourcePage);
        destPage.CustomValues["/NAPS2ImportedPage"] = new PdfCustomValue(new byte[] { 0xFF });
        return destPage;
    }

    private static void DrawOcrTextOnPage(PdfPage page, OcrResult ocrResult)
    {
#if DEBUG && DEBUGOCR
            using XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
#else
        using XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend);
#endif
        var tf = new XTextFormatter(gfx);
        foreach (var element in ocrResult.Elements)
        {
            if (string.IsNullOrEmpty(element.Text)) continue;

            var adjustedBounds = AdjustBounds(element.Bounds, (float) page.Width / ocrResult.PageBounds.w,
                (float) page.Height / ocrResult.PageBounds.h);
#if DEBUG && DEBUGOCR
                    gfx.DrawRectangle(new XPen(XColor.FromArgb(255, 0, 0)), adjustedBounds);
#endif
            var adjustedFontSize = CalculateFontSize(element.Text, adjustedBounds, gfx);
            // Special case to avoid accidentally recognizing big lines as dashes/underscores
            if (adjustedFontSize > 100 && (element.Text == "-" || element.Text == "_")) continue;
            var font = new XFont("Times New Roman", adjustedFontSize, XFontStyle.Regular,
                new XPdfFontOptions(PdfFontEncoding.Unicode));
            var adjustedTextSize = gfx.MeasureString(element.Text, font);
            var verticalOffset = (adjustedBounds.Height - adjustedTextSize.Height) / 2;
            var horizontalOffset = (adjustedBounds.Width - adjustedTextSize.Width) / 2;
            adjustedBounds.Offset((float) horizontalOffset, (float) verticalOffset);
            tf.DrawString(element.RightToLeft ? ReverseText(element.Text) : element.Text, font, XBrushes.Transparent,
                adjustedBounds);
        }
    }

    private static string ReverseText(string text)
    {
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
        List<string> elements = new List<string>();
        while (enumerator.MoveNext())
        {
            elements.Add(enumerator.GetTextElement());
        }
        elements.Reverse();
        return string.Concat(elements);
    }

    private static void DrawImageOnPage(PdfPage page, XImage img, PdfCompat compat)
    {
        if (compat != PdfCompat.Default)
        {
            img.Interpolate = false;
        }
        var (realWidth, realHeight) = GetRealSize(img);
        page.Width = realWidth;
        page.Height = realHeight;
        using XGraphics gfx = XGraphics.FromPdfPage(page);
        gfx.DrawImage(img, 0, 0, realWidth, realHeight);
    }

    private static (int width, int height) GetRealSize(XImage img)
    {
        double hAdjust = 72 / img.HorizontalResolution;
        double vAdjust = 72 / img.VerticalResolution;
        if (double.IsInfinity(hAdjust) || double.IsInfinity(vAdjust))
        {
            hAdjust = vAdjust = 0.75;
        }
        double realWidth = img.PixelWidth * hAdjust;
        double realHeight = img.PixelHeight * vAdjust;
        return ((int) realWidth, (int) realHeight);
    }

    private static XRect AdjustBounds((int x, int y, int w, int h) bounds, float hAdjust, float vAdjust) =>
        new XRect(bounds.x * hAdjust, bounds.y * vAdjust, bounds.w * hAdjust, bounds.h * vAdjust);

    private static int CalculateFontSize(string text, XRect adjustedBounds, XGraphics gfx)
    {
        int fontSizeGuess = Math.Max(1, (int) (adjustedBounds.Height));
        var measuredBoundsForGuess =
            gfx.MeasureString(text, new XFont("Times New Roman", fontSizeGuess, XFontStyle.Regular));
        double adjustmentFactor = adjustedBounds.Width / measuredBoundsForGuess.Width;
        int adjustedFontSize = Math.Max(1, (int) Math.Floor(fontSizeGuess * adjustmentFactor));
        return adjustedFontSize;
    }

    private class UnixFontResolver : IFontResolver
    {
        private byte[]? _fontData;

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo(familyName, isBold, isItalic);
        }

        public byte[] GetFont(string faceName)
        {
            if (_fontData == null)
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = "fc-list",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                });
                if (proc == null)
                {
                    throw new InvalidOperationException("Could not get font data from fc-list");
                }
                var fonts = proc.StandardOutput.ReadToEnd().Split('\n').Select(x => x.Split(':')[0]);
                // TODO: Maybe add more fonts here?
                var freeserif = fonts.First(f => f.EndsWith("FreeSerif.ttf", StringComparison.OrdinalIgnoreCase));
                _fontData = File.ReadAllBytes(freeserif);
            }
            return _fontData;
        }
    }
}