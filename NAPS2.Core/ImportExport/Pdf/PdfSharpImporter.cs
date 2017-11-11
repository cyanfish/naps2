using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageRenderer scannedImageRenderer;
        private readonly IPdfRenderer pdfRenderer;

        public PdfSharpImporter(IErrorOutput errorOutput, IPdfPasswordProvider pdfPasswordProvider, ThumbnailRenderer thumbnailRenderer, ScannedImageRenderer scannedImageRenderer, IPdfRenderer pdfRenderer)
        {
            this.errorOutput = errorOutput;
            this.pdfPasswordProvider = pdfPasswordProvider;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;
            this.pdfRenderer = pdfRenderer;
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
                PdfDocument document = PdfReader.Open(filePath, PdfDocumentOpenMode.Import, args =>
                {
                    if (!pdfPasswordProvider.ProvidePassword(Path.GetFileName(filePath), passwordAttempts++, out args.Password))
                    {
                        args.Abort = true;
                        aborted = true;
                    }
                });
                if (passwordAttempts > 0
                    && !document.SecuritySettings.HasOwnerPermissions
                    && !document.SecuritySettings.PermitExtractContent)
                {
                    errorOutput.DisplayError(string.Format(MiscResources.PdfNoPermissionToExtractContent, Path.GetFileName(filePath)));
                    return Enumerable.Empty<ScannedImage>();
                }
                if (document.Info.Creator != MiscResources.NAPS2 && document.Info.Author != MiscResources.NAPS2)
                {
                    pdfRenderer.ThrowIfCantRender();
                    return document.Pages.Cast<PdfPage>().Select(ExportRawPdfPage);
                }

                int i = 0;
                return document.Pages.Cast<PdfPage>().TakeWhile(page => progressCallback(i++, document.PageCount)).SelectMany(GetImagesFromPage);
            }
            catch (ImageRenderException e)
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
            if (page.CustomValues.Elements.ContainsKey("/NAPS2ImportedPage"))
            {
                yield return ExportRawPdfPage(page);
                yield break;
            }

            // Get resources dictionary
            PdfDictionary resources = page.Elements.GetDictionary("/Resources");
            // Get external objects dictionary
            PdfDictionary xObjects = resources?.Elements.GetDictionary("/XObject");
            if (xObjects == null)
            {
                yield break;
            }
            // Iterate references to external objects
            foreach (PdfItem item in xObjects.Elements.Values)
            {
                var reference = item as PdfReference;
                var xObject = reference?.Value as PdfDictionary;
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
                            case "/CCITTFaxDecode":
                                yield return ExportG4(page, xObject);
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

        private ScannedImage ExportRawPdfPage(PdfPage page)
        {
            string pdfPath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
            var document = new PdfDocument();
            document.Pages.Add(page);
            document.Save(pdfPath);

            var image = ScannedImage.FromSinglePagePdf(pdfPath, false);
            using (var bitmap = scannedImageRenderer.Render(image))
            {
                image.SetThumbnail(thumbnailRenderer.RenderThumbnail(bitmap));
            }
            return image;
        }

        private ScannedImage ExportJpegImage(PdfPage page, byte[] imageBytes)
        {
            // Fortunately JPEG has native support in PDF and exporting an image is just writing the stream to a file.
            using (var memoryStream = new MemoryStream(imageBytes))
            {
                using (var bitmap = new Bitmap(memoryStream))
                {
                    bitmap.SetResolution(bitmap.Width / (float)page.Width.Inch, bitmap.Height / (float)page.Height.Inch);
                    var image = new ScannedImage(bitmap, ScanBitDepth.C24Bit, false, -1);
                    image.SetThumbnail(thumbnailRenderer.RenderThumbnail(bitmap));
                    return image;
                }
            }
        }

        private ScannedImage ExportAsPngImage(PdfPage page, PdfDictionary imageObject)
        {
            int width = imageObject.Elements.GetInteger(PdfImage.Keys.Width);
            int height = imageObject.Elements.GetInteger(PdfImage.Keys.Height);
            int bitsPerComponent = imageObject.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

            var buffer = imageObject.Stream.UnfilteredValue;

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
                var image = new ScannedImage(bitmap, bitDepth, true, -1);
                image.SetThumbnail(thumbnailRenderer.RenderThumbnail(bitmap));
                return image;
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

        // Sample full tiff          LEN-------------------                                                  DATA------------------                                                              WIDTH-----------------                                                  HEIGHT----------------                                                  BITS PER COMP---------                                                                                                                                                                                                                                                                          REALLEN---------------
        // { 0x49, 0x49, 0x2A, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x99, 0x99, 0x99, 0x99, 0x07, 0x00, 0x00, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x77, 0x77, 0x00, 0x00, 0x01, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x88, 0x88, 0x00, 0x00, 0x02, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x11, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x15, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x17, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        private static readonly byte[] TiffBeforeDataLen = { 0x49, 0x49, 0x2A, 0x00 };
        private static readonly byte[] TiffBeforeData = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] TiffBeforeWidth = { 0x07, 0x00, 0x00, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00 };
        private static readonly byte[] TiffBeforeHeight = { 0x01, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00 };
        private static readonly byte[] TiffBeforeBits = { 0x02, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00 };
        private static readonly byte[] TiffBeforeRealLen = { 0x03, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x11, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x15, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x17, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00 };
        private static readonly byte[] TiffTrailer = { 0x00, 0x00, 0x00, 0x00 };

        private ScannedImage ExportG4(PdfPage page, PdfDictionary imageObject)
        {
            int width = imageObject.Elements.GetInteger(PdfImage.Keys.Width);
            int height = imageObject.Elements.GetInteger(PdfImage.Keys.Height);
            int bitsPerComponent = imageObject.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

            byte[] imageBytes = imageObject.Stream.Value;

            // We don't have easy access to a standalone CCITT G4 decoder, so we'll make use of the .NET TIFF decoder
            // by constructing a valid TIFF file "manually" and directly injecting the bytestream
            var stream = new MemoryStream();
            Write(stream, TiffBeforeDataLen);
            // The bytestream is 2-padded, so we may need to append an extra zero byte
            if (imageBytes.Length % 2 == 1)
            {
                Write(stream, imageBytes.Length + 0x11);
            }
            else
            {
                Write(stream, imageBytes.Length + 0x10);
            }
            Write(stream, TiffBeforeData);
            Write(stream, imageBytes);
            if (imageBytes.Length % 2 == 1)
            {
                Write(stream, new byte[] { 0x00 });
            }
            Write(stream, TiffBeforeWidth);
            Write(stream, width);
            Write(stream, TiffBeforeHeight);
            Write(stream, height);
            Write(stream, TiffBeforeBits);
            Write(stream, bitsPerComponent);
            Write(stream, TiffBeforeRealLen);
            Write(stream, imageBytes.Length);
            Write(stream, TiffTrailer);
            stream.Seek(0, SeekOrigin.Begin);

            using (Bitmap bitmap = (Bitmap)Image.FromStream(stream))
            {
                bitmap.SetResolution(bitmap.Width / (float)page.Width.Inch, bitmap.Height / (float)page.Height.Inch);

                var image = new ScannedImage(bitmap, ScanBitDepth.BlackWhite, true, -1);
                image.SetThumbnail(thumbnailRenderer.RenderThumbnail(bitmap));
                return image;
            }
        }

        private void Write(MemoryStream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        private void Write(MemoryStream stream, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Debug.Assert(bytes.Length == 4);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
