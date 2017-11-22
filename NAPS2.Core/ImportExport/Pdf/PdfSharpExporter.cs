using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using NAPS2.Ocr;
using NAPS2.Scan.Images;
using NAPS2.Util;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSharpExporter : IPdfExporter
    {
        private readonly IOcrEngine ocrEngine;
        private readonly ScannedImageRenderer scannedImageRenderer;

        public PdfSharpExporter(IOcrEngine ocrEngine, ScannedImageRenderer scannedImageRenderer)
        {
            this.ocrEngine = ocrEngine;
            this.scannedImageRenderer = scannedImageRenderer;
        }

        public bool Export(string path, IEnumerable<ScannedImage> images, PdfSettings settings, string ocrLanguageCode, Func<int, bool> progressCallback)
        {
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


            bool useOcr = false;
            if (ocrLanguageCode != null)
            {
                if (ocrEngine.CanProcess(ocrLanguageCode))
                {
                    useOcr = true;
                }
                else
                {
                    Log.Error("OCR files not available for '{0}'.", ocrLanguageCode);
                }
            }

            bool result = useOcr
                ? BuildDocumentWithOcr(progressCallback, document, images, ocrLanguageCode)
                : BuildDocumentWithoutOcr(progressCallback, document, images);
            if (!result)
            {
                return false;
            }

            PathHelper.EnsureParentDirExists(path);
            document.Save(path);
            return true;
        }

        private bool BuildDocumentWithoutOcr(Func<int, bool> progressCallback, PdfDocument document, IEnumerable<ScannedImage> images)
        {
            int progress = 0;
            foreach (var image in images)
            {
                bool importedPdfPassThrough = image.FileFormat == null && !image.RecoveryIndexImage.TransformList.Any();

                if (importedPdfPassThrough)
                {
                    CopyPdfPageToDoc(document, image);
                }
                else
                {
                    using (Stream stream = scannedImageRenderer.RenderToStream(image))
                    using (var img = XImage.FromStream(stream))
                    {
                        if (!progressCallback(progress))
                        {
                            return false;
                        }

                        PdfPage page = document.AddPage();
                        DrawImageOnPage(page, img);
                    }
                }
                progress++;
            }
            return true;
        }

        private bool BuildDocumentWithOcr(Func<int, bool> progressCallback, PdfDocument document, IEnumerable<ScannedImage> images, string ocrLanguageCode)
        {
            // Use a pipeline so that multiple pages/images can be processed in parallel
            // Note: No locks needed on the document because the design of the pipeline ensures no two threads will work on it at once

            int progress = 0;
            Pipeline.For(images).Step(image =>
            {
                // Step 1: Load the image into memory, draw it on a new PDF page, and save a copy of the processed image to disk for OCR

                if (!progressCallback(progress))
                {
                    return null;
                }

                bool importedPdfPassThrough = image.FileFormat == null && !image.RecoveryIndexImage.TransformList.Any();

                PdfPage page;
                if (importedPdfPassThrough)
                {
                    page = CopyPdfPageToDoc(document, image);

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
                            if (line.EndsWith("BT"))
                            {
                                inTextBlock = true;
                            }
                            else if (line.EndsWith("ET"))
                            {
                                inTextBlock = false;
                            }
                            else if (inTextBlock &&
                                          (line.EndsWith("TJ") || line.EndsWith("Tj")
                                           || line.EndsWith("\"") || line.EndsWith("'")))
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

                using (Stream stream = scannedImageRenderer.RenderToStream(image))
                using (var img = XImage.FromStream(stream))
                {
                    if (!progressCallback(progress))
                    {
                        return null;
                    }

                    if (!importedPdfPassThrough)
                    {
                        DrawImageOnPage(page, img);
                    }

                    if (!progressCallback(progress))
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
                    if (!progressCallback(progress))
                    {
                        return null;
                    }
                    
                    // ReSharper disable once AccessToModifiedClosure
                    ocrResult = ocrEngine.ProcessImage(tempImageFilePath, ocrLanguageCode, () => !progressCallback(progress));
                }
                finally
                {
                    File.Delete(tempImageFilePath);
                }

                // The final pipeline step is pretty fast, so updating progress here is more accurate
                if (progressCallback(progress))
                {
                    Interlocked.Increment(ref progress);
                    progressCallback(progress);
                }

                return Tuple.Create(page, ocrResult);
            }).StepBlock().Run((page, ocrResult) =>
            {
                // Step 3: Draw the OCR text on the PDF page

                if (ocrResult == null)
                {
                    return;
                }
                if (!progressCallback(progress))
                {
                    return;
                }
                DrawOcrTextOnPage(page, ocrResult);
            });
            return progressCallback(progress);
        }

        private PdfPage CopyPdfPageToDoc(PdfDocument destDoc, ScannedImage image)
        {
            // Pull the PDF content directly to maintain objects, dpi, etc.
            PdfDocument sourceDoc = PdfReader.Open(image.RecoveryFilePath, PdfDocumentOpenMode.Import);
            PdfPage sourcePage = sourceDoc.Pages.Cast<PdfPage>().Single();
            PdfPage destPage = destDoc.AddPage(sourcePage);
            destPage.CustomValues["/NAPS2ImportedPage"] = new PdfCustomValue();
            return destPage;
        }

        private static void DrawOcrTextOnPage(PdfPage page, OcrResult ocrResult)
        {
            using (XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend))
            {
                var tf = new XTextFormatter(gfx);
                foreach (var element in ocrResult.Elements)
                {
                    var adjustedBounds = AdjustBounds(element.Bounds, (float)page.Width / ocrResult.PageBounds.Width, (float)page.Height / ocrResult.PageBounds.Height);
                    var adjustedFontSize = CalculateFontSize(element.Text, adjustedBounds, gfx);
                    var font = new XFont("Times New Roman", adjustedFontSize, XFontStyle.Regular,
                        new XPdfFontOptions(PdfFontEncoding.Unicode));
                    var adjustedHeight = gfx.MeasureString(element.Text, font).Height;
                    var verticalOffset = (adjustedBounds.Height - adjustedHeight) / 2;
                    adjustedBounds.Offset(0, (float)verticalOffset);
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

        private static void DrawImageOnPage(PdfPage page, XImage img)
        {
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
            var measuredBoundsForGuess = gfx.MeasureString(text, new XFont("Times New Roman", fontSizeGuess, XFontStyle.Regular));
            double adjustmentFactor = adjustedBounds.Width / measuredBoundsForGuess.Width;
            int adjustedFontSize = Math.Max(1, (int)Math.Round(fontSizeGuess * adjustmentFactor));
            return adjustedFontSize;
        }
    }
}
