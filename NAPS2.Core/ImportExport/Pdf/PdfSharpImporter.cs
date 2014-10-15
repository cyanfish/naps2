using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Server;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSharpImporter : IPdfImporter
    {
        private readonly IScannedImageFactory scannedImageFactory;

        public PdfSharpImporter(IScannedImageFactory scannedImageFactory)
        {
            this.scannedImageFactory = scannedImageFactory;
        }

        public IEnumerable<IScannedImage> Import(string filePath)
        {
            // TODO: Handle errors (can't open pdf, not implemented encoding, etc.)
            // TODO: Maybe add a "safety" for one image per page

            PdfDocument document = PdfReader.Open(filePath);

            // Iterate pages
            foreach (PdfPage page in document.Pages)
            {
                // Get resources dictionary
                PdfDictionary resources = page.Elements.GetDictionary("/Resources");
                if (resources != null)
                {
                    // Get external objects dictionary
                    PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject");
                    if (xObjects != null)
                    {
                        ICollection items = xObjects.Elements.Values;
                        // Iterate references to external objects
                        foreach (PdfItem item in items)
                        {
                            PdfReference reference = item as PdfReference;
                            if (reference != null)
                            {
                                PdfDictionary xObject = reference.Value as PdfDictionary;
                                // Is external object an image?
                                if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
                                {
                                    string filter = xObject.Elements.GetName("/Filter");
                                    switch (filter)
                                    {
                                        case "/DCTDecode":
                                            yield return ExportJpegImage(xObject);
                                            break;

                                        case "/FlateDecode":
                                            yield return ExportAsPngImage(xObject);
                                            break;

                                        default:
                                            throw new NotImplementedException("Unsupported image encoding");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private IScannedImage ExportJpegImage(PdfDictionary image)
        {
            // Fortunately JPEG has native support in PDF and exporting an image is just writing the stream to a file.
            using (var memoryStream = new MemoryStream(image.Stream.Value))
            {
                var bitmap = new Bitmap(memoryStream);
                return scannedImageFactory.Create(bitmap, ScanBitDepth.C24Bit, false);
            }
        }

        private IScannedImage ExportAsPngImage(PdfDictionary image)
        {
            int width = image.Elements.GetInteger(PdfImage.Keys.Width);
            int height = image.Elements.GetInteger(PdfImage.Keys.Height);
            int bitsPerComponent = image.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);
            string colorSpace = image.Elements.GetName(PdfImage.Keys.ColorSpace);

            var buffer = image.Stream.UnfilteredValue;

            if (bitsPerComponent != 8 || colorSpace != "/DeviceRGB" || (width * height * 3) != buffer.Length)
            {
                throw new NotImplementedException("Unsupported image encoding (expected 24 bpp)");
            }

            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            RgbToBitmapUnmanaged(height, width, bitmap, buffer);

            return scannedImageFactory.Create(bitmap, ScanBitDepth.C24Bit, true);
        }

        private static void RgbToBitmap(int height, int width, Bitmap bitmap, byte[] rgbBuffer)
        {
            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(255, rgbBuffer[i++], rgbBuffer[i++], rgbBuffer[i++]));
                }
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
    }
}
