using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NAPS2.ImportExport.Images;
using NAPS2.Scan;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.IO;

namespace NAPS2.ImportExport.Pdf;

// TODO: We should have a "nicer" name (PdfImporter) for SDK users, or maybe have this be internal and have the public ScannedImageImporter (which should also maybe be renamed...)
// TODO: Add tests
public class PdfSharpImporter : IPdfImporter
{
    static PdfSharpImporter()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private readonly ScanningContext _scanningContext;
    private readonly IPdfPasswordProvider? _pdfPasswordProvider;
    private readonly ImportPostProcessor _importPostProcessor;

    public PdfSharpImporter(ScanningContext scanningContext)
        : this(scanningContext, null)
    {
    }

    public PdfSharpImporter(ScanningContext scanningContext, IPdfPasswordProvider? pdfPasswordProvider)
        : this(scanningContext, pdfPasswordProvider, new ImportPostProcessor(scanningContext.ImageContext))
    {
    }

    internal PdfSharpImporter(ScanningContext scanningContext, IPdfPasswordProvider? pdfPasswordProvider, 
        ImportPostProcessor importPostProcessor)
    {
        _scanningContext = scanningContext;
        _pdfPasswordProvider = pdfPasswordProvider;
        _importPostProcessor = importPostProcessor;
    }

    public ScannedImageSource Import(string filePath, ImportParams? importParams = null, ProgressHandler? progressCallback = null,
        CancellationToken cancelToken = default)
    {
        importParams ??= new ImportParams();
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
                    if (importParams.Password != null)
                    {
                        args.Password = importParams.Password;
                        return;
                    }
                    if (_pdfPasswordProvider == null)
                    {
                        // TODO: Resource?
                        throw new PdfImportException("The PDF document needs a password to import");
                    }
                    if (!_pdfPasswordProvider.ProvidePassword(Path.GetFileName(filePath), passwordAttempts++,
                            out args.Password))
                    {
                        args.Abort = true;
                        aborted = true;
                    }
                });
                if (passwordAttempts > 0
                    && !document.SecuritySettings.HasOwnerPermissions
                    && !document.SecuritySettings.PermitExtractContent)
                {
                    throw new PdfImportException(string.Format(SdkResources.PdfNoPermissionToExtractContent,
                        Path.GetFileName(filePath)));
                }

                progressCallback?.Invoke(0, document.PageCount);
                var pages = importParams.Slice.Indices(document.PageCount)
                    .Select(index => document.Pages[index]);
                if (document.Info.Creator != SdkResources.NAPS2 && document.Info.Author != SdkResources.NAPS2)
                {
                    await Pipeline.For(pages, cancelToken)
                        .StepParallel(async page => await ExportRawPdfPage(page, importParams))
                        .Run(image =>
                        {
                            progressCallback?.Invoke(++i, document.PageCount);
                            sink.PutImage(image);
                        });
                }
                else
                {
                    await Pipeline.For(pages, cancelToken)
                        // Getting CustomValues is not thread safe, so we need to do it in a separate step
                        .Step(page => (page, page.CustomValues.Elements.ContainsKey("/NAPS2ImportedPage")))
                        .StepManyParallel(async tuple =>
                        {
                            var (page, isImportedPage) = tuple;
                            if (isImportedPage)
                            {
                                return new[] {await ExportRawPdfPage(page, importParams)};
                            }

                            return GetImagesFromPage(page, importParams);
                        })
                        .Run(image =>
                        {
                            progressCallback?.Invoke(++i, document.PageCount);
                            sink.PutImage(image);
                        });
                }
            }
            catch (Exception e)
            {
                if (!aborted)
                {
                    sink.SetError(e);
                }
            }
            finally
            {
                sink.SetCompleted();
            }
        });
        return sink.AsSource();
    }

    private IEnumerable<ProcessedImage> GetImagesFromPage(PdfPage page, ImportParams importParams)
    {
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
            // Is external object an image?
            if ((item as PdfReference)?.Value is PdfDictionary xObject &&
                xObject.Elements.GetString("/Subtype") == "/Image")
            {
                // Support multiple filter schemes
                var element = xObject.Elements.Single(x => x.Key == "/Filter");
                if (element.Value is PdfArray elementAsArray)
                {
                    string[] arrayElements = elementAsArray.Elements.Select(x => x.ToString()!).ToArray();
                    if (arrayElements.Length == 2)
                    {
                        yield return DecodeImage(arrayElements[1], page, xObject,
                            Filtering.Decode(xObject.Stream.Value, arrayElements[0]), importParams);
                    }
                }
                else if (element.Value is PdfName elementAsName)
                {
                    yield return DecodeImage(elementAsName.Value, page, xObject, xObject.Stream.Value, importParams);
                }
                else
                {
                    throw new NotImplementedException("Unsupported filter");
                }
            }
        }
    }

    private ProcessedImage DecodeImage(string encoding, PdfPage page, PdfDictionary xObject, byte[] stream,
        ImportParams importParams)
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

    private async Task<ProcessedImage> ExportRawPdfPage(PdfPage page, ImportParams importParams)
    {
        IImageStorage storage;
        var document = new PdfDocument();
        document.Pages.Add(page);
        if (_scanningContext.FileStorageManager != null)
        {
            string pdfPath = _scanningContext.FileStorageManager.NextFilePath() + ".pdf";
            document.Save(pdfPath);
            storage = new ImageFileStorage(pdfPath);
        }
        else
        {
            var stream = new MemoryStream();
            document.Save(stream);
            storage = new ImageMemoryStorage(stream, ".pdf");
        }

        var image = new ProcessedImage(
            storage,
            new ImageMetadata(BitDepth.Color, false),
            new PostProcessingData(),
            TransformState.Empty);
        return _importPostProcessor.AddPostProcessingData(
            image,
            null,
            importParams.ThumbnailSize,
            importParams.BarcodeDetectionOptions,
            true);
    }

    private ProcessedImage ExportJpegImage(PdfPage page, byte[] imageBytes, ImportParams importParams)
    {
        // Fortunately JPEG has native support in PDF and exporting an image is just writing the stream to a file.
        using var memoryStream = new MemoryStream(imageBytes);
        using var storage = _scanningContext.ImageContext.Load(memoryStream);
        storage.SetResolution(storage.Width / (float) page.Width.Inch, storage.Height / (float) page.Height.Inch);
        var image = _scanningContext.CreateProcessedImage(storage, BitDepth.Color, false, -1);
        return _importPostProcessor.AddPostProcessingData(
            image,
            storage,
            importParams.ThumbnailSize,
            importParams.BarcodeDetectionOptions,
            true);
    }

    private ProcessedImage ExportAsPngImage(PdfPage page, PdfDictionary imageObject, ImportParams importParams)
    {
        int width = imageObject.Elements.GetInteger(PdfImage.Keys.Width);
        int height = imageObject.Elements.GetInteger(PdfImage.Keys.Height);
        int bitsPerComponent = imageObject.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

        var buffer = imageObject.Stream.UnfilteredValue;

        IMemoryImage storage;
        BitDepth bitDepth;
        switch (bitsPerComponent)
        {
            case 8:
                storage = _scanningContext.ImageContext.Create(width, height, ImagePixelFormat.RGB24);
                bitDepth = BitDepth.Color;
                RgbToBitmapUnmanaged(storage, buffer);
                break;
            case 1:
                storage = _scanningContext.ImageContext.Create(width, height, ImagePixelFormat.BW1);
                bitDepth = BitDepth.BlackAndWhite;
                BlackAndWhiteToBitmapUnmanaged(storage, buffer);
                break;
            default:
                throw new NotImplementedException("Unsupported image encoding (expected 24 bpp or 1bpp)");
        }

        using (storage)
        {
            storage.SetResolution(storage.Width / (float) page.Width.Inch, storage.Height / (float) page.Height.Inch);
            var image = _scanningContext.CreateProcessedImage(storage, bitDepth, true, -1);
            return _importPostProcessor.AddPostProcessingData(
                image,
                storage,
                importParams.ThumbnailSize,
                importParams.BarcodeDetectionOptions,
                true);
        }
    }

    private static void RgbToBitmapUnmanaged(IMemoryImage image, byte[] rgbBuffer)
    {
        using var data = image.Lock(LockMode.WriteOnly, out var scan0, out var stride);
        int height = image.Height;
        int width = image.Width;
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

    private static void BlackAndWhiteToBitmapUnmanaged(IMemoryImage image, byte[] bwBuffer)
    {
        using var data = image.Lock(LockMode.WriteOnly, out var scan0, out var stride);
        int height = image.Height;
        int width = image.Width;
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

    // Sample full tiff          LEN-------------------                                                  DATA------------------                                                              WIDTH-----------------                                                  HEIGHT----------------                                                  BITS PER COMP---------                                                                                                                                                                                                                                                                          REALLEN---------------
    // { 0x49, 0x49, 0x2A, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x99, 0x99, 0x99, 0x99, 0x07, 0x00, 0x00, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x77, 0x77, 0x00, 0x00, 0x01, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x88, 0x88, 0x00, 0x00, 0x02, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x11, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x15, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x17, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

    private static readonly byte[] TiffBeforeDataLen = {0x49, 0x49, 0x2A, 0x00};
    private static readonly byte[] TiffBeforeData = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
    private static readonly byte[] TiffBeforeWidth = {0x07, 0x00, 0x00, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00};
    private static readonly byte[] TiffBeforeHeight = {0x01, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00};
    private static readonly byte[] TiffBeforeBits = {0x02, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00};

    private static readonly byte[] TiffBeforeRealLen =
    {
        0x03, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x11, 0x01, 0x04, 0x00, 0x01, 0x00,
        0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x15, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x17, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00
    };

    private static readonly byte[] TiffTrailer = {0x00, 0x00, 0x00, 0x00};

    private ProcessedImage ExportG4(PdfPage page, PdfDictionary imageObject, byte[] imageBytes,
        ImportParams importParams)
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
            Write(stream, new byte[] {0x00});
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

        // TODO: If we need a TIFF hint for loading, it should go here.
        using var storage = _scanningContext.ImageContext.Load(stream);
        storage.SetResolution(storage.Width / (float) page.Width.Inch, storage.Height / (float) page.Height.Inch);

        // TODO: Use CreateProcessedImage?
        var image = new ProcessedImage(
            storage,
            new ImageMetadata(BitDepth.BlackAndWhite, true),
            new PostProcessingData(),
            TransformState.Empty);
        return _importPostProcessor.AddPostProcessingData(
            image,
            storage,
            importParams.ThumbnailSize,
            importParams.BarcodeDetectionOptions,
            true);
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