using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Dependencies;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Scan;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Util;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.IO;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSharpImporter : IPdfImporter
    {
        private readonly ErrorOutput errorOutput;
        private readonly IPdfPasswordProvider pdfPasswordProvider;
        private readonly ImageRenderer imageRenderer;
        private readonly IPdfRenderer pdfRenderer;
        private readonly IComponentInstallPrompt componentInstallPrompt;

        public PdfSharpImporter(ErrorOutput errorOutput, IPdfPasswordProvider pdfPasswordProvider, ImageRenderer imageRenderer, IPdfRenderer pdfRenderer, IComponentInstallPrompt componentInstallPrompt)
        {
            this.errorOutput = errorOutput;
            this.pdfPasswordProvider = pdfPasswordProvider;
            this.imageRenderer = imageRenderer;
            this.pdfRenderer = pdfRenderer;
            this.componentInstallPrompt = componentInstallPrompt;
        }

        public ScannedImageSource Import(string filePath, ImportParams importParams, ProgressHandler progressCallback, CancellationToken cancelToken)
        {
            var sink = new ScannedImageSink();
            Task.Run(async () =>
            {
                if (cancelToken.IsCancellationRequested)
                {
                    sink.SetCompleted();
                }

                int passwordAttempts = 0;
                bool aborted = false;
                int i = 0;
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
                        sink.SetCompleted();
                    }

                    var pages = importParams.Slice.Indices(document.PageCount)
                        .Select(index => document.Pages[index])
                        .TakeWhile(page =>
                        {
                            progressCallback(i++, document.PageCount);
                            return !cancelToken.IsCancellationRequested;
                        });
                    if (document.Info.Creator != MiscResources.NAPS2 && document.Info.Author != MiscResources.NAPS2)
                    {
                        pdfRenderer.PromptToInstallIfNeeded(componentInstallPrompt);
                        pdfRenderer.ThrowIfCantRender();
                        foreach (var page in pages)
                        {
                            sink.PutImage(await ExportRawPdfPage(page, importParams));
                        }
                    }
                    else
                    {
                        // TODO: Maybe can parallelize this
                        foreach (var page in pages)
                        {
                            await GetImagesFromPage(page, importParams, sink);
                        }
                    }
                }
                catch (ImageRenderException e)
                {
                    errorOutput.DisplayError(string.Format(MiscResources.ImportErrorNAPS2Pdf, Path.GetFileName(filePath)));
                    Log.ErrorException("Error importing PDF file.", e);
                }
                catch (Exception e)
                {
                    if (!aborted)
                    {
                        errorOutput.DisplayError(string.Format(MiscResources.ImportErrorCouldNot, Path.GetFileName(filePath)));
                        Log.ErrorException("Error importing PDF file.", e);
                    }
                }
                finally
                {
                    sink.SetCompleted();
                }
            });
            return sink.AsSource();
        }

        private async Task GetImagesFromPage(PdfPage page, ImportParams importParams, ScannedImageSink sink)
        {
            if (page.CustomValues.Elements.ContainsKey("/NAPS2ImportedPage"))
            {
                sink.PutImage(await ExportRawPdfPage(page, importParams));
                return;
            }

            // Get resources dictionary
            PdfDictionary resources = page.Elements.GetDictionary("/Resources");
            // Get external objects dictionary
            PdfDictionary xObjects = resources?.Elements.GetDictionary("/XObject");
            if (xObjects == null)
            {
                return;
            }
            // Iterate references to external objects
            foreach (PdfItem item in xObjects.Elements.Values)
            {
                // Is external object an image?
                if ((item as PdfReference)?.Value is PdfDictionary xObject && xObject.Elements.GetString("/Subtype") == "/Image")
                {
                    // Support multiple filter schemes
                    var element = xObject.Elements.Single(x => x.Key == "/Filter");
                    if (element.Value is PdfArray elementAsArray)
                    {
                        string[] arrayElements = elementAsArray.Elements.Select(x => x.ToString()).ToArray();
                        if (arrayElements.Length == 2)
                        {
                            sink.PutImage(DecodeImage(arrayElements[1], page, xObject, Filtering.Decode(xObject.Stream.Value, arrayElements[0]), importParams));
                        }
                    }
                    else if (element.Value is PdfName elementAsName)
                    {
                        sink.PutImage(DecodeImage(elementAsName.Value, page, xObject, xObject.Stream.Value, importParams));
                    }
                    else
                    {
                        throw new NotImplementedException("Unsupported filter");
                    }
                }
            }
        }

        private ScannedImage DecodeImage(string encoding, PdfPage page, PdfDictionary xObject, byte[] stream, ImportParams importParams)
        {
            switch (encoding)
            {
                case "/DCTDecode":
                    return ExportJpegImage(page, stream, importParams);
                case "/FlateDecode":
                    return ExportAsPngImage(page, xObject, importParams);
                case "/CCITTFaxDecode":
                    return ExportG4(page, xObject, stream, importParams);
                default:
                    throw new NotImplementedException("Unsupported image encoding");
            }
        }

        private async Task<ScannedImage> ExportRawPdfPage(PdfPage page, ImportParams importParams)
        {
            string pdfPath = FileStorageManager.Current.NextFilePath();
            var document = new PdfDocument();
            document.Pages.Add(page);
            document.Save(pdfPath);

            // TODO: It would make sense to have in-memory PDFs be an option.
            // TODO: Really, ConvertToBacking should convert PdfStorage -> PdfFileStorage.
            // TODO: Then we wouldn't need a static FileStorageManager.
            var image = new ScannedImage(new FileStorage(pdfPath));
            if (!importParams.NoThumbnails || importParams.DetectPatchCodes)
            {
                using (var bitmap = await imageRenderer.Render(image))
                {
                    if (!importParams.NoThumbnails)
                    {
                        image.SetThumbnail(Transform.Perform(bitmap, new ThumbnailTransform()));
                    }
                    if (importParams.DetectPatchCodes)
                    {
                        image.PatchCode = PatchCodeDetector.Detect(bitmap);
                    }
                }
            }
            return image;
        }

        private ScannedImage ExportJpegImage(PdfPage page, byte[] imageBytes, ImportParams importParams)
        {
            // Fortunately JPEG has native support in PDF and exporting an image is just writing the stream to a file.
            using (var memoryStream = new MemoryStream(imageBytes))
            {
                using (var storage = StorageManager.ImageFactory.Decode(memoryStream, ".jpg"))
                {
                    storage.SetResolution(storage.Width / (float)page.Width.Inch, storage.Height / (float)page.Height.Inch);
                    var image = new ScannedImage(storage, ScanBitDepth.C24Bit, false, -1);
                    if (!importParams.NoThumbnails)
                    {
                        image.SetThumbnail(Transform.Perform(storage, new ThumbnailTransform()));
                    }
                    if (importParams.DetectPatchCodes)
                    {
                        image.PatchCode = PatchCodeDetector.Detect(storage);
                    }
                    return image;
                }
            }
        }

        private ScannedImage ExportAsPngImage(PdfPage page, PdfDictionary imageObject, ImportParams importParams)
        {
            int width = imageObject.Elements.GetInteger(PdfImage.Keys.Width);
            int height = imageObject.Elements.GetInteger(PdfImage.Keys.Height);
            int bitsPerComponent = imageObject.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

            var buffer = imageObject.Stream.UnfilteredValue;

            IImage storage;
            ScanBitDepth bitDepth;
            switch (bitsPerComponent)
            {
                case 8:
                    storage = StorageManager.ImageFactory.FromDimensions(width, height, StoragePixelFormat.RGB24);
                    bitDepth = ScanBitDepth.C24Bit;
                    RgbToBitmapUnmanaged(storage, buffer);
                    break;
                case 1:
                    storage = StorageManager.ImageFactory.FromDimensions(width, height, StoragePixelFormat.BW1);
                    bitDepth = ScanBitDepth.BlackWhite;
                    BlackAndWhiteToBitmapUnmanaged(storage, buffer);
                    break;
                default:
                    throw new NotImplementedException("Unsupported image encoding (expected 24 bpp or 1bpp)");
            }

            using (storage)
            {
                storage.SetResolution(storage.Width / (float)page.Width.Inch, storage.Height / (float)page.Height.Inch);
                var image = new ScannedImage(storage, bitDepth, true, -1);
                if (!importParams.NoThumbnails)
                {
                    image.SetThumbnail(Transform.Perform(storage, new ThumbnailTransform()));
                }
                if (importParams.DetectPatchCodes)
                {
                    image.PatchCode = PatchCodeDetector.Detect(storage);
                }
                return image;
            }
        }

        private static void RgbToBitmapUnmanaged(IImage image, byte[] rgbBuffer)
        {
            var data = image.Lock(LockMode.WriteOnly, out var scan0, out var stride);
            int height = image.Height;
            int width = image.Width;
            try
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        IntPtr pixelData = scan0 + y * stride + x * 3;
                        int bufferIndex = (y * width + x) * 3;
                        Marshal.WriteByte(pixelData, rgbBuffer[bufferIndex + 2]);
                        Marshal.WriteByte(pixelData + 1, rgbBuffer[bufferIndex + 1]);
                        Marshal.WriteByte(pixelData + 2, rgbBuffer[bufferIndex]);
                    }
                }
            }
            finally
            {
                image.Unlock(data);
            }
        }

        private static void BlackAndWhiteToBitmapUnmanaged(IImage image, byte[] bwBuffer)
        {
            var data = image.Lock(LockMode.WriteOnly, out var scan0, out var stride);
            int height = image.Height;
            int width = image.Width;
            try
            {
                int bytesPerRow = (width - 1) / 8 + 1;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < bytesPerRow; x++)
                    {
                        IntPtr pixelData = scan0 + y * stride + x;
                        Marshal.WriteByte(pixelData, bwBuffer[y * bytesPerRow + x]);
                    }
                }
            }
            finally
            {
                image.Unlock(data);
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

        private ScannedImage ExportG4(PdfPage page, PdfDictionary imageObject, byte[] imageBytes, ImportParams importParams)
        {
            int width = imageObject.Elements.GetInteger(PdfImage.Keys.Width);
            int height = imageObject.Elements.GetInteger(PdfImage.Keys.Height);
            int bitsPerComponent = imageObject.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

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

            using (var storage = StorageManager.ImageFactory.Decode(stream, ".tiff"))
            {
                storage.SetResolution(storage.Width / (float)page.Width.Inch, storage.Height / (float)page.Height.Inch);

                var image = new ScannedImage(storage, ScanBitDepth.BlackWhite, true, -1);
                if (!importParams.NoThumbnails)
                {
                    image.SetThumbnail(Transform.Perform(storage, new ThumbnailTransform()));
                }
                if (importParams.DetectPatchCodes)
                {
                    image.PatchCode = PatchCodeDetector.Detect(storage);
                }
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
