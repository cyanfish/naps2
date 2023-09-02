using NAPS2.Images.Bitwise;
using NAPS2.Pdf.Pdfium;

namespace NAPS2.Pdf;

internal static class PdfiumImageExtractor
{
    public static IMemoryImage? GetSingleImage(ImageContext imageContext, PdfPage page, bool ignoreHiddenText)
    {
        using var imageObj = GetSingleImageObject(page, ignoreHiddenText);
        if (imageObj != null)
        {
            var metadata = imageObj.ImageMetadata;
            var image = GetImageFromObject(imageContext, page, imageObj, metadata);
            if (image != null)
            {
                image.SetResolution((int) Math.Round(metadata.HorizontalDpi), (int) Math.Round(metadata.VerticalDpi));
                return image;
            }
        }
        return null;
    }

    // TODO: This could be wrong if the image object has a mask, but GetRenderedBitmap does a re-encode which we don't really want
    // Ideally we would be do this conditionally based on the presence of a mask, if pdfium could provide us that info
    private static IMemoryImage? GetImageFromObject(ImageContext imageContext, PdfPage page, PdfPageObject imageObj,
        PdfImageMetadata metadata)
    {
        // Otherwise we render the entire page with the known image dimensions
        return ExtractRawImageData(imageContext, imageObj, metadata) ??
               RenderPdfPageToNewImage(imageContext, page, metadata);
    }

    private static IMemoryImage? ExtractRawImageData(ImageContext imageContext, PdfPageObject imageObj,
        PdfImageMetadata metadata)
    {
        if (metadata.Colorspace is not (Colorspace.DeviceRgb or Colorspace.DeviceGray or Colorspace.Indexed))
        {
            return null;
        }
        // TODO: This condition is never actually true for some reason, we need to use this code path if there is either a monochrome mask or softmask
        // TODO: Might need a pdfium fix.
        if (imageObj.HasTransparency)
        {
            // If the image has transparency, that implies the bitmap has a mask, so we need to use GetRenderedBitmap
            // to apply the mask and get the correct image.
            return null;
        }
        // First try and read the raw image data, this is most efficient if we can handle it
        if (imageObj.HasImageFilters("DCTDecode"))
        {
            return imageContext.Load(new MemoryStream(imageObj.GetImageDataRaw()));
        }
        if (imageObj.HasImageFilters("FlateDecode"))
        {
            // TODO: Add tests for these cases to PdfiumPdfRendererTests
            if (metadata.BitsPerPixel == 24 && metadata.Colorspace == Colorspace.DeviceRgb)
            {
                return LoadRaw(imageContext, imageObj.GetImageDataDecoded(), metadata, ImagePixelFormat.RGB24,
                    SubPixelType.Rgb);
            }
            if (metadata.BitsPerPixel == 1 && metadata.Colorspace == Colorspace.DeviceGray)
            {
                return LoadRaw(imageContext, imageObj.GetImageDataDecoded(), metadata, ImagePixelFormat.BW1,
                    SubPixelType.Bit);
            }
        }
        // Previously we also had a way to load the raw CCITTFaxDecode filter with a custom CcittReader class, but that
        // failed in some cases (https://github.com/cyanfish/naps2/issues/117)
        return null;
    }

    private static IMemoryImage RenderPdfPageToNewImage(ImageContext imageContext, PdfPage page,
        PdfImageMetadata metadata)
    {
        // This maintains the correct image dimensions/resolution as we have that info from the metadata.
        using var pdfBitmap = RenderPdfPageToBitmap(page, metadata);
        return CopyPdfBitmapToNewImage(imageContext, pdfBitmap, metadata);
    }

    private static PdfBitmap RenderPdfPageToBitmap(PdfPage page, PdfImageMetadata imageMetadata)
    {
        var w = imageMetadata.Width;
        var h = imageMetadata.Height;
        var format = imageMetadata.BitsPerPixel switch
        {
            1 or 8 => PdfiumNativeLibrary.FPDFBitmap_Gray,
            24 => PdfiumNativeLibrary.FPDFBitmap_BGR,
            32 => PdfiumNativeLibrary.FPDFBitmap_BGRA,
            _ => throw new ArgumentException()
        };
        var pdfiumBitmap = PdfBitmap.Create(w, h, format);
        pdfiumBitmap.FillRect(0, 0, w, h, PdfBitmap.WHITE);
        pdfiumBitmap.RenderPage(page, 0, 0, w, h);
        return pdfiumBitmap;
    }

    private static IMemoryImage CopyPdfBitmapToNewImage(ImageContext imageContext, PdfBitmap pdfBitmap,
        PdfImageMetadata imageMetadata)
    {
        var (targetPixelFormat, subPixelType) = imageMetadata.BitsPerPixel switch
        {
            1 => (ImagePixelFormat.BW1, SubPixelType.Gray),
            8 => (ImagePixelFormat.Gray8, SubPixelType.Gray),
            24 => (ImagePixelFormat.RGB24, SubPixelType.Bgr),
            32 => (ImagePixelFormat.ARGB32, SubPixelType.Bgra),
            _ => throw new ArgumentException()
        };
        var dstImage = imageContext.Create(pdfBitmap.Width, pdfBitmap.Height, targetPixelFormat);
        dstImage.SetResolution(imageMetadata.HorizontalDpi, imageMetadata.VerticalDpi);
        var srcPixelInfo = new PixelInfo(pdfBitmap.Width, pdfBitmap.Height, subPixelType, pdfBitmap.Stride);
        new CopyBitwiseImageOp().Perform(pdfBitmap.Buffer, srcPixelInfo, dstImage);
        return dstImage;
    }

    private static IMemoryImage LoadRaw(ImageContext imageContext, byte[] buffer, PdfImageMetadata metadata,
        ImagePixelFormat newImagePixelFormat, SubPixelType bufferSubPixelType)
    {
        var image = imageContext.Create(metadata.Width, metadata.Height, newImagePixelFormat);
        image.OriginalFileFormat = ImageFileFormat.Png;
        var srcPixelInfo = new PixelInfo(image.Width, image.Height, bufferSubPixelType);
        new CopyBitwiseImageOp().Perform(buffer, srcPixelInfo, image);
        return image;
    }

    public static PdfPageObject? GetSingleImageObject(PdfPage page, bool ignoreHiddenText)
    {
        if (page.AnnotCount > 0)
        {
            return null;
        }
        using var pageText = page.GetText();
        PdfPageObject? imageObject = null;
        var objectCount = page.ObjectCount;
        for (int i = 0; i < objectCount; i++)
        {
            // Note that objects from PdfPage.GetObject don't need to be disposed
            var pageObj = page.GetObject(i);
            // TODO: We could consider, even in cases where we don't have an exact matrix match etc., getting a smarter dpi estimate.
            // TODO: But it's not clear how well that will render if there's a subpixel offset.
            if (pageObj.IsImage &&
                PdfMatrix.EqualsWithinTolerance(pageObj.Matrix, PdfMatrix.FillPage(page.Width, page.Height)) &&
                imageObject == null)
            {
                imageObject = pageObj;
            }
            else if (ignoreHiddenText && pageObj.IsText && (imageObject == null || IsInvisibleText(pageObj)))
            {
                // Skip invisible text or text that's underneath the image
                // TODO: This could be wrong if the image object has transparency
            }
            else
            {
                // We have other visible objects than a single image
                return null;
            }
        }
        return imageObject;
    }

    private static bool IsInvisibleText(PdfPageObject pageObj)
    {
        return pageObj.TextRenderMode == TextRenderMode.Invisible
               || pageObj.TextRenderMode == TextRenderMode.Fill && pageObj.GetFillColor().a == 0
               || pageObj.TextRenderMode == TextRenderMode.Stroke && pageObj.GetStrokeColor().a == 0;
    }
}