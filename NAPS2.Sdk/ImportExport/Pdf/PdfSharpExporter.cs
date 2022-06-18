using System.Globalization;
using System.Threading;
using NAPS2.Images.Gdi;
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
    private readonly ScanningContext _scanningContext;

    static PdfSharpExporter()
    {
        if (PlatformCompat.System.UseUnixFontResolver)
        {
            GlobalFontSettings.FontResolver = new UnixFontResolver();
        }
    }

    public PdfSharpExporter(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public override async Task<bool> Export(string path, ICollection<ProcessedImage> images, IConfigProvider<PdfSettings> settings, OcrParams? ocrParams = null, ProgressHandler? progressCallback = null, CancellationToken cancelToken = default)
    {
        return await Task.Run(async () =>
        {
            var compat = settings.Get(c => c.Compat);

            var document = new PdfDocument();
            document.Info.Author = settings.Get(c => c.Metadata.Author);
            document.Info.Creator = settings.Get(c => c.Metadata.Creator);
            document.Info.Keywords = settings.Get(c => c.Metadata.Keywords);
            document.Info.Subject = settings.Get(c => c.Metadata.Subject);
            document.Info.Title = settings.Get(c => c.Metadata.Title);

            if (settings.Get(c => c.Encryption.EncryptPdf)
                && (!string.IsNullOrEmpty(settings.Get(c => c.Encryption.OwnerPassword)) || !string.IsNullOrEmpty(settings.Get(c => c.Encryption.UserPassword))))
            {
                document.SecuritySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted128Bit;
                if (!string.IsNullOrEmpty(settings.Get(c => c.Encryption.OwnerPassword)))
                {
                    document.SecuritySettings.OwnerPassword = settings.Get(c => c.Encryption.OwnerPassword);
                }

                if (!string.IsNullOrEmpty(settings.Get(c => c.Encryption.UserPassword)))
                {
                    document.SecuritySettings.UserPassword = settings.Get(c => c.Encryption.UserPassword);
                }

                document.SecuritySettings.PermitAccessibilityExtractContent = settings.Get(c => c.Encryption.AllowContentCopyingForAccessibility);
                document.SecuritySettings.PermitAnnotations = settings.Get(c => c.Encryption.AllowAnnotations);
                document.SecuritySettings.PermitAssembleDocument = settings.Get(c => c.Encryption.AllowDocumentAssembly);
                document.SecuritySettings.PermitExtractContent = settings.Get(c => c.Encryption.AllowContentCopying);
                document.SecuritySettings.PermitFormsFill = settings.Get(c => c.Encryption.AllowFormFilling);
                document.SecuritySettings.PermitFullQualityPrint = settings.Get(c => c.Encryption.AllowFullQualityPrinting);
                document.SecuritySettings.PermitModifyDocument = settings.Get(c => c.Encryption.AllowDocumentModification);
                document.SecuritySettings.PermitPrint = settings.Get(c => c.Encryption.AllowPrinting);
            }

            IOcrEngine ocrEngine = null;
            if (ocrParams?.LanguageCode != null)
            {
                var activeEngine = _scanningContext.OcrEngineProvider.ActiveEngine;
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
                ? await BuildDocumentWithOcr(progressCallback, cancelToken, document, compat, images, ocrParams, ocrEngine)
                : await BuildDocumentWithoutOcr(progressCallback, cancelToken, document, compat, images);
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

            PathHelper.EnsureParentDirExists(path);
            document.Save(path);
            return true;
        });
    }

    private async Task<bool> BuildDocumentWithoutOcr(ProgressHandler? progressCallback, CancellationToken cancelToken, PdfDocument document, PdfCompat compat, ICollection<ProcessedImage> images)
    {
        int progress = 0;
        progressCallback?.Invoke(progress, images.Count);
        foreach (var image in images)
        {
            // TODO: Maybe have a PdfFileStorage?
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

    private static bool IsPdfFile(ImageFileStorage imageFileStorage) => Path.GetExtension(imageFileStorage.FullPath)?.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase) ?? false;

    private async Task<bool> BuildDocumentWithOcr(ProgressHandler? progressCallback, CancellationToken cancelToken, PdfDocument document, PdfCompat compat, ICollection<ProcessedImage> images, OcrParams ocrParams, IOcrEngine ocrEngine)
    {
        int progress = 0;
        progressCallback?.Invoke(progress, images.Count);

        List<(PdfPage, Task<OcrResult>)> ocrPairs = new List<(PdfPage, Task<OcrResult>)>();

        // Step 1: Create the pages, draw the images, and start OCR
        foreach (var image in images)
        {
            if (cancelToken.IsCancellationRequested)
            {
                break;
            }

            PdfPage page;
            bool importedPdfPassThrough = false;

            // TODO: Maybe have a PdfFileStorage?
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

            string tempImageFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());

            var format = image.Metadata.Lossless ? ImageFileFormat.Png : ImageFileFormat.Jpeg;
            using (var renderedImage = _scanningContext.ImageContext.Render(image))
            using (var stream = renderedImage.SaveToMemoryStream(format))
            using (var pdfImage = XImage.FromStream(stream))
            {
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

                if (!importedPdfPassThrough)
                {
                    DrawImageOnPage(page, pdfImage, compat);
                }

                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

                if (!_scanningContext.OcrRequestQueue.HasCachedResult(ocrEngine, image, ocrParams))
                {
                    pdfImage.GdiImage.Save(tempImageFilePath);
                }
            }

            if (cancelToken.IsCancellationRequested)
            {
                File.Delete(tempImageFilePath);
                break;
            }

            // Start OCR
            var ocrTask = _scanningContext.OcrRequestQueue.Enqueue(
                ocrEngine, image, tempImageFilePath, ocrParams, OcrPriority.Foreground, cancelToken);
            ocrTask.ContinueWith(task =>
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
            string line;
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
                         (line.EndsWith("TJ", StringComparison.InvariantCulture) || line.EndsWith("Tj", StringComparison.InvariantCulture)
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

            var adjustedBounds = AdjustBounds(element.Bounds, (float)page.Width / ocrResult.PageBounds.w, (float)page.Height / ocrResult.PageBounds.h);
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
            adjustedBounds.Offset((float)horizontalOffset, (float)verticalOffset);
            tf.DrawString(element.RightToLeft ? ReverseText(element.Text) : element.Text, font, XBrushes.Transparent, adjustedBounds);
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
        return ((int)realWidth, (int)realHeight);
    }

    private static XRect AdjustBounds((int x, int y, int w, int h) bounds, float hAdjust, float vAdjust) =>
        new XRect(bounds.x * hAdjust, bounds.y * vAdjust, bounds.w * hAdjust, bounds.h * vAdjust);

    private static int CalculateFontSize(string text, XRect adjustedBounds, XGraphics gfx)
    {
        int fontSizeGuess = Math.Max(1, (int)(adjustedBounds.Height));
        var measuredBoundsForGuess = gfx.MeasureString(text, new XFont("Times New Roman", fontSizeGuess, XFontStyle.Regular));
        double adjustmentFactor = adjustedBounds.Width / measuredBoundsForGuess.Width;
        int adjustedFontSize = Math.Max(1, (int)Math.Floor(fontSizeGuess * adjustmentFactor));
        return adjustedFontSize;
    }
        
    private class UnixFontResolver : IFontResolver
    {
        private byte[] _fontData;

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
                var fonts = proc.StandardOutput.ReadToEnd().Split('\n').Select(x => x.Split(':')[0]);
                // TODO: Maybe add more fonts here?
                var freeserif = fonts.First(f => f.EndsWith("FreeSerif.ttf", StringComparison.OrdinalIgnoreCase));
                _fontData = File.ReadAllBytes(freeserif);
            }
            return _fontData;
        }
    }
}