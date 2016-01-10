using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Lang.Resources;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Util;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.IO;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSharpImporter : IPdfImporter
    {
        private readonly IErrorOutput errorOutput;
        private readonly IPdfPasswordProvider pdfPasswordProvider;

        public PdfSharpImporter(IErrorOutput errorOutput, IPdfPasswordProvider pdfPasswordProvider)
        {
            this.errorOutput = errorOutput;
            this.pdfPasswordProvider = pdfPasswordProvider;
        }

        public IEnumerable<ScannedImage> Import(string filePath, Func<int, int, bool> progressCallback)
        {
            if (!progressCallback(0, 0))
            {
                return Enumerable.Empty<ScannedImage>();
            }
            int passwordAttempts = 0;
            bool aborted = false;
            try
            {
                PdfDocument document = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly, args =>
                {
                    if (!pdfPasswordProvider.ProvidePassword(Path.GetFileName(filePath), passwordAttempts++, out args.Password))
                    {
                        args.Abort = true;
                        aborted = true;
                    }
                });
                if (document.Info.Creator != MiscResources.NAPS2 && document.Info.Author != MiscResources.NAPS2)
                {
                    errorOutput.DisplayError(string.Format(MiscResources.ImportErrorNAPS2Pdf, Path.GetFileName(filePath)));
                    return Enumerable.Empty<ScannedImage>();
                }
                if (passwordAttempts > 0
                    && !document.SecuritySettings.HasOwnerPermissions
                    && !document.SecuritySettings.PermitExtractContent)
                {
                    errorOutput.DisplayError(string.Format(MiscResources.PdfNoPermissionToExtractContent, Path.GetFileName(filePath)));
                    return Enumerable.Empty<ScannedImage>();
                }

                int i = 0;
                return document.Pages.Cast<PdfPage>().TakeWhile(page => progressCallback(i++, document.PageCount)).SelectMany(GetImagesFromPage);
            }
            catch (NotImplementedException e)
            {
                errorOutput.DisplayError(string.Format(MiscResources.ImportErrorNAPS2Pdf, Path.GetFileName(filePath)));
                Log.ErrorException("Error importing PDF file.", e);
                return Enumerable.Empty<ScannedImage>();
            }
            catch (Exception e)
            {
                if (!aborted)
                {
                    errorOutput.DisplayError(string.Format(MiscResources.ImportErrorCouldNot, Path.GetFileName(filePath)));
                    Log.ErrorException("Error importing PDF file.", e);
                }
                return Enumerable.Empty<ScannedImage>();
            }
        }

        private IEnumerable<ScannedImage> GetImagesFromPage(PdfPage page)
        {
            // Get resources dictionary
            PdfDictionary resources = page.Elements.GetDictionary("/Resources");
            if (resources == null)
            {
                yield break;
            }
            // Get external objects dictionary
            PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject");
            if (xObjects == null)
            {
                yield break;
            }
            // Iterate references to external objects
            foreach (PdfItem item in xObjects.Elements.Values)
            {
                var reference = item as PdfReference;
                if (reference == null)
                {
                    continue;
                }
                var xObject = reference.Value as PdfDictionary;
                // Is external object an image?
                if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
                {
                    // Support multiple filter schemes
                    // For JPEG: "/DCTDecode" OR ["/DCTDecode", "/FlateDecode"]
                    // For PNG: "/FlateDecode"
                    var element = xObject.Elements.Single(x => x.Key == "/Filter");
                    var elementAsArray = element.Value as PdfArray;
                    var elementAsName = element.Value as PdfName;
                    if (elementAsArray != null)
                    {
                        // JPEG ["/DCTDecode", "/FlateDecode"]
                        yield return ExportJpegImage(page, Filtering.Decode(xObject.Stream.Value, "/FlateDecode"));
                    }
                    else if (elementAsName != null)
                    {
                        switch (elementAsName.Value)
                        {
                            case "/DCTDecode":
                                yield return ExportJpegImage(page, xObject.Stream.Value);
                                break;
                            case "/FlateDecode":
                                yield return ExportAsPngImage(page, xObject);
                                break;
                            default:
                                throw new NotImplementedException("Unsupported image encoding");
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("Unsupported filter");
                    }
                }
            }
        }

        private ScannedImage ExportJpegImage(PdfPage page, byte[] imageBytes)
        {
            // Fortunately JPEG has native support in PDF and exporting an image is just writing the stream to a file.
            using (var memoryStream = new MemoryStream(imageBytes))
            {
                using (var bitmap = new Bitmap(memoryStream))
                {
                    bitmap.SetResolution(bitmap.Width / (float)page.Width.Inch, bitmap.Height / (float)page.Height.Inch);
                    return new ScannedImage(bitmap, ScanBitDepth.C24Bit, false, -1);
                }
            }
        }

        private ScannedImage ExportAsPngImage(PdfPage page, PdfDictionary image)
        {
            int width = image.Elements.GetInteger(PdfImage.Keys.Width);
            int height = image.Elements.GetInteger(PdfImage.Keys.Height);
            int bitsPerComponent = image.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

            var buffer = image.Stream.UnfilteredValue;

            Bitmap bitmap;
            ScanBitDepth bitDepth;
            switch (bitsPerComponent)
            {
                case 8:
                    bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                    bitDepth = ScanBitDepth.C24Bit;
                    RgbToBitmapUnmanaged(height, width, bitmap, buffer);
                    break;
                case 1:
                    bitmap = new Bitmap(width, height, PixelFormat.Format1bppIndexed);
                    bitDepth = ScanBitDepth.BlackWhite;
                    BlackAndWhiteToBitmapUnmanaged(height, width, bitmap, buffer);
                    break;
                default:
                    throw new NotImplementedException("Unsupported image encoding (expected 24 bpp or 1bpp)");
            }

            using (bitmap)
            {
                bitmap.SetResolution(bitmap.Width / (float)page.Width.Inch, bitmap.Height / (float)page.Height.Inch);
                return new ScannedImage(bitmap, bitDepth, true, -1);
            }
        }

        private static void RgbToBitmapUnmanaged(int height, int width, Bitmap bitmap, byte[] rgbBuffer)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        IntPtr pixelData = data.Scan0 + y * data.Stride + x * 3;
                        int bufferIndex = (y * width + x) * 3;
                        Marshal.WriteByte(pixelData, rgbBuffer[bufferIndex + 2]);
                        Marshal.WriteByte(pixelData + 1, rgbBuffer[bufferIndex + 1]);
                        Marshal.WriteByte(pixelData + 2, rgbBuffer[bufferIndex]);
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        private static void BlackAndWhiteToBitmapUnmanaged(int height, int width, Bitmap bitmap, byte[] bwBuffer)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
            try
            {
                int bytesPerRow = (width - 1) / 8 + 1;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < bytesPerRow; x++)
                    {
                        IntPtr pixelData = data.Scan0 + y * data.Stride + x;
                        Marshal.WriteByte(pixelData, bwBuffer[y * bytesPerRow + x]);
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
    }
}
