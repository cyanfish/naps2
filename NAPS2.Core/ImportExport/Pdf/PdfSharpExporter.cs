using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using NAPS2.Config;
using NAPS2.Ocr;
using NAPS2.Scan.Images;
using NAPS2.Util;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSharpExporter : IPdfExporter
    {
        static PdfSharpExporter()
        {
            GlobalFontSettings.FontResolver = new FontResolver();
        }

        private readonly OcrManager ocrManager;
        private readonly ScannedImageRenderer scannedImageRenderer;
        private readonly AppConfigManager appConfigManager;

        public PdfSharpExporter(OcrManager ocrManager, ScannedImageRenderer scannedImageRenderer, AppConfigManager appConfigManager)
        {
            this.ocrManager = ocrManager;
            this.scannedImageRenderer = scannedImageRenderer;
            this.appConfigManager = appConfigManager;
        }

        public bool Export(string path, ICollection<ScannedImage.Snapshot> snapshots, PdfSettings settings, OcrParams ocrParams, ProgressHandler progressCallback)
        {
            var forced = appConfigManager.Config.ForcePdfCompat;
            var compat = forced == PdfCompat.Default ? settings.Compat : forced;

            var document = new PdfDocument();
            document.Info.Author = settings.Metadata.Author;
            document.Info.Creator = settings.Metadata.Creator;
            document.Info.Keywords = settings.Metadata.Keywords;
            document.Info.Subject = settings.Metadata.Subject;
            document.Info.Title = settings.Metadata.Title;

            if (settings.Encryption.EncryptPdf
                && (!string.IsNullOrEmpty(settings.Encryption.OwnerPassword) || !string.IsNullOrEmpty(settings.Encryption.UserPassword)))
            {
                document.SecuritySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted128Bit;
                if (!string.IsNullOrEmpty(settings.Encryption.OwnerPassword))
                {
                    document.SecuritySettings.OwnerPassword = settings.Encryption.OwnerPassword;
                }
                if (!string.IsNullOrEmpty(settings.Encryption.UserPassword))
                {
                    document.SecuritySettings.UserPassword = settings.Encryption.UserPassword;
                }
                document.SecuritySettings.PermitAccessibilityExtractContent = settings.Encryption.AllowContentCopyingForAccessibility;
                document.SecuritySettings.PermitAnnotations = settings.Encryption.AllowAnnotations;
                document.SecuritySettings.PermitAssembleDocument = settings.Encryption.AllowDocumentAssembly;
                document.SecuritySettings.PermitExtractContent = settings.Encryption.AllowContentCopying;
                document.SecuritySettings.PermitFormsFill = settings.Encryption.AllowFormFilling;
                document.SecuritySettings.PermitFullQualityPrint = settings.Encryption.AllowFullQualityPrinting;
                document.SecuritySettings.PermitModifyDocument = settings.Encryption.AllowDocumentModification;
                document.SecuritySettings.PermitPrint = settings.Encryption.AllowPrinting;
            }

            IOcrEngine ocrEngine = null;
            if (ocrParams?.LanguageCode != null)
            {
                var activeEngine = ocrManager.ActiveEngine;
                if (activeEngine == null)
                {
                    Log.Error("Supported OCR engine not installed.", ocrParams.LanguageCode);
                }
                else if (!activeEngine.CanProcess(ocrParams.LanguageCode))
                {
                    Log.Error("OCR files not available for '{0}'.", ocrParams.LanguageCode);
                }
                else
                {
                    ocrEngine = activeEngine;
                }
            }
            
            bool result = ocrEngine != null
                ? BuildDocumentWithOcr(progressCallback, document, compat, snapshots, ocrEngine, ocrParams)
                : BuildDocumentWithoutOcr(progressCallback, document, compat, snapshots);
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
        }

        private bool BuildDocumentWithoutOcr(ProgressHandler progressCallback, PdfDocument document, PdfCompat compat, ICollection<ScannedImage.Snapshot> snapshots)
        {
            int progress = 0;
            foreach (var snapshot in snapshots)
            {
                bool importedPdfPassThrough = snapshot.Source.FileFormat == null && !snapshot.TransformList.Any();

                if (importedPdfPassThrough)
                {
                    CopyPdfPageToDoc(document, snapshot.Source);
                }
                else
                {
                    using (Stream stream = scannedImageRenderer.RenderToStream(snapshot))
                    using (var img = XImage.FromStream(stream))
                    {
                        if (!progressCallback(progress, snapshots.Count))
                        {
                            return false;
                        }

                        PdfPage page = document.AddPage();
                        DrawImageOnPage(page, img, compat);
                    }
                }
                progress++;
            }
            return true;
        }

        private bool BuildDocumentWithOcr(ProgressHandler progressCallback, PdfDocument document, PdfCompat compat, ICollection<ScannedImage.Snapshot> snapshots, IOcrEngine ocrEngine, OcrParams ocrParams)
        {
            // Use a pipeline so that multiple pages/images can be processed in parallel
            // Note: No locks needed on the document because the design of the pipeline ensures no two threads will work on it at once

            int progress = 0;
            Pipeline.For(snapshots).Step(snapshot =>
            {
                // Step 1: Load the image into memory, draw it on a new PDF page, and save a copy of the processed image to disk for OCR

                if (!progressCallback(progress, snapshots.Count))
                {
                    return null;
                }

                bool importedPdfPassThrough = snapshot.Source.FileFormat == null && !snapshot.TransformList.Any();

                PdfPage page;
                if (importedPdfPassThrough)
                {
                    page = CopyPdfPageToDoc(document, snapshot.Source);

                    // Scan through the page looking for text
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
                                           || line.EndsWith("\"", StringComparison.InvariantCulture) || line.EndsWith("'", StringComparison.InvariantCulture)))
                            {
                                // Text-showing operators
                                // Since this page already contains text, don't use OCR
                                return null;
                            }
                        }
                    }
                }
                else
                {
                    page = document.AddPage();
                }

                using (Stream stream = scannedImageRenderer.RenderToStream(snapshot))
                using (var img = XImage.FromStream(stream))
                {
                    if (!progressCallback(progress, snapshots.Count))
                    {
                        return null;
                    }

                    if (!importedPdfPassThrough)
                    {
                        DrawImageOnPage(page, img, compat);
                    }

                    if (!progressCallback(progress, snapshots.Count))
                    {
                        return null;
                    }

                    string tempImageFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
                    img.GdiImage.Save(tempImageFilePath);

                    return Tuple.Create(page, tempImageFilePath);
                }
            }).StepParallel((page, tempImageFilePath) =>
            {
                // Step 2: Run OCR on the processsed image file
                // This step is doubly parallel since not only can it run alongside other stages of the pipeline,
                // multiple files can also be OCR'd at once (no interdependencies, it doesn't touch the document)

                OcrResult ocrResult;
                try
                {
                    if (!progressCallback(progress, snapshots.Count))
                    {
                        return null;
                    }
                    
                    // ReSharper disable once AccessToModifiedClosure
                    ocrResult = ocrEngine.ProcessImage(tempImageFilePath, ocrParams, () => !progressCallback(progress, snapshots.Count));
                }
                finally
                {
                    File.Delete(tempImageFilePath);
                }

                // The final pipeline step is pretty fast, so updating progress here is more accurate
                if (progressCallback(progress, snapshots.Count))
                {
                    Interlocked.Increment(ref progress);
                    progressCallback(progress, snapshots.Count);
                }

                return Tuple.Create(page, ocrResult);
            }).StepBlock().Run((page, ocrResult) =>
            {
                // Step 3: Draw the OCR text on the PDF page

                if (ocrResult == null)
                {
                    return;
                }
                if (!progressCallback(progress, snapshots.Count))
                {
                    return;
                }
                DrawOcrTextOnPage(page, ocrResult);
            });
            return progressCallback(progress, snapshots.Count);
        }

        private PdfPage CopyPdfPageToDoc(PdfDocument destDoc, ScannedImage image)
        {
            // Pull the PDF content directly to maintain objects, dpi, etc.
            PdfDocument sourceDoc = PdfReader.Open(image.RecoveryFilePath, PdfDocumentOpenMode.Import);
            PdfPage sourcePage = sourceDoc.Pages.Cast<PdfPage>().Single();
            PdfPage destPage = destDoc.AddPage(sourcePage);
            destPage.CustomValues["/NAPS2ImportedPage"] = new PdfCustomValue(new byte[] { 0xFF });
            return destPage;
        }

        private static void DrawOcrTextOnPage(PdfPage page, OcrResult ocrResult)
        {
#if DEBUG && DEBUGOCR
            using (XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
#else
            using (XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend))
#endif
            {
                var tf = new XTextFormatter(gfx);
                foreach (var element in ocrResult.Elements)
                {
                    var adjustedBounds = AdjustBounds(element.Bounds, (float)page.Width / ocrResult.PageBounds.Width, (float)page.Height / ocrResult.PageBounds.Height);
#if DEBUG && DEBUGOCR
                    gfx.DrawRectangle(new XPen(XColor.FromArgb(255, 0, 0)), adjustedBounds);
#endif
                    var adjustedFontSize = CalculateFontSize(element.Text, adjustedBounds, gfx);
                    var font = new XFont("Verdana", adjustedFontSize, XFontStyle.Regular,
                        new XPdfFontOptions(PdfFontEncoding.Unicode));
                    var adjustedTextSize = gfx.MeasureString(element.Text, font);
                    var verticalOffset = (adjustedBounds.Height - adjustedTextSize.Height) / 2;
                    var horizontalOffset = (adjustedBounds.Width - adjustedTextSize.Width) / 2;
                    adjustedBounds.Offset((float)horizontalOffset, (float)verticalOffset);
                    tf.DrawString(ocrResult.RightToLeft ? ReverseText(element.Text) : element.Text, font, XBrushes.Transparent, adjustedBounds);
                }
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
            Size realSize = GetRealSize(img);
            page.Width = realSize.Width;
            page.Height = realSize.Height;
            using (XGraphics gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawImage(img, 0, 0, realSize.Width, realSize.Height);
            }
        }

        private static Size GetRealSize(XImage img)
        {
            double hAdjust = 72 / img.HorizontalResolution;
            double vAdjust = 72 / img.VerticalResolution;
            if (double.IsInfinity(hAdjust) || double.IsInfinity(vAdjust))
            {
                hAdjust = vAdjust = 0.75;
            }
            double realWidth = img.PixelWidth * hAdjust;
            double realHeight = img.PixelHeight * vAdjust;
            return new Size((int)realWidth, (int)realHeight);
        }

        private static XRect AdjustBounds(Rectangle b, float hAdjust, float vAdjust)
        {
            var adjustedBounds = new XRect(b.X * hAdjust, b.Y * vAdjust, b.Width * hAdjust, b.Height * vAdjust);
            return adjustedBounds;
        }

        private static int CalculateFontSize(string text, XRect adjustedBounds, XGraphics gfx)
        {
            int fontSizeGuess = Math.Max(1, (int)(adjustedBounds.Height));
            var measuredBoundsForGuess = gfx.MeasureString(text, new XFont("Verdana", fontSizeGuess, XFontStyle.Regular));
            double adjustmentFactor = adjustedBounds.Width / measuredBoundsForGuess.Width;
            int adjustedFontSize = Math.Max(1, (int)Math.Floor(fontSizeGuess * adjustmentFactor));
            return adjustedFontSize;
        }
        
        private class FontResolver : IFontResolver
        {
            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            {
                return new FontResolverInfo(familyName, isBold, isItalic);
            }

            public byte[] GetFont(string faceName) => Fonts.verdana;
        }
    }
}
