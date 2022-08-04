using NAPS2.ImportExport.Pdf.Pdfium;

namespace NAPS2.ImportExport.Pdf;

public class PdfiumImageExtractor
{
    public static IMemoryImage? GetSingleImage(ImageContext imageContext, PdfPage page)
    {
        using var imageObj = GetSingleImageObject(page);
        if (imageObj != null)
        {
            var metadata = imageObj.ImageMetadata;
            var image = GetImageFromObject(imageContext, imageObj, metadata);
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
    private static IMemoryImage? GetImageFromObject(ImageContext imageContext, PdfPageObject imageObj,
        PdfImageMetadata metadata)
    {
        var bitmapFactory = new PdfiumBitmapFactory(imageContext);
        // TODO: This condition is never actually true for some reason, we need to use this code path if there is either a monochrome mask or softmask
        // TODO: Might need a pdfium fix.
        if (imageObj.HasTransparency)
        {
            // If the image has transparency, that implies the bitmap has a mask, so we need to use GetRenderedBitmap
            // to apply the mask and get the correct image.
            using var pdfBitmap = imageObj.GetRenderedBitmap();
            if (pdfBitmap.Format is ImagePixelFormat.RGB24 or ImagePixelFormat.ARGB32)
            {
                return bitmapFactory.CopyPdfBitmapToNewImage(pdfBitmap, metadata);
            }
            return null;
        }
        // First try and read the raw image data, this is most efficient if we can handle it
        if (imageObj.HasImageFilters("DCTDecode"))
        {
            return imageContext.Load(new MemoryStream(imageObj.GetImageDataRaw()));
        }
        if (imageObj.HasImageFilters("FlateDecode"))
        {
            if (metadata.BitsPerPixel == 24 && metadata.Colorspace == Colorspace.DeviceRgb)
            {
                return bitmapFactory.LoadRawRgb(imageObj.GetImageDataDecoded(), metadata);
            }
            if (metadata.BitsPerPixel == 1 && metadata.Colorspace == Colorspace.DeviceGray)
            {
                // TODO: Needs testing
                return bitmapFactory.LoadRawBlackAndWhite(imageObj.GetImageDataDecoded(), metadata);
            }
        }
        if (imageObj.HasImageFilters("CCITTFaxDecode"))
        {
            return bitmapFactory.LoadRawCcitt(imageObj.GetImageDataDecoded(), metadata);
        }
        // If we can't read the raw data ourselves, we can try and rely on Pdfium to materialize a bitmap, which is a
        // bit less efficient
        // TODO: Maybe add support for black & white here too, with tests
        // TODO: Also this won't have test coverage if everything is covered by the "raw" tests, maybe either find a
        // test case or just have a switch to test this specifically
        // TODO: Is 32 bit even possible here? As alpha is implemented with masks
        if (metadata.BitsPerPixel == 24 || metadata.BitsPerPixel == 32)
        {
            using var pdfBitmap = imageObj.GetBitmap();
            if (pdfBitmap.Format is ImagePixelFormat.RGB24 or ImagePixelFormat.ARGB32)
            {
                return bitmapFactory.CopyPdfBitmapToNewImage(pdfBitmap, metadata);
            }
        }
        // Otherwise we fall back to relying on Pdfium to render the whole page which is least efficient and won't have
        // the correct DPI
        return null;
    }

    public static PdfPageObject? GetSingleImageObject(PdfPage page)
    {
        using var pageText = page.GetText();
        PdfPageObject? imageObject = null;
        var objectCount = page.ObjectCount;
        for (int i = 0; i < objectCount; i++)
        {
            // Note that objects from PdfPage.GetObject don't need to be disposed
            var pageObj = page.GetObject(i);
            // TODO: We could consider, even in cases where we don't have an exact matrix match etc., getting a smarter dpi estimate.
            // TODO: But it's not clear how well that will render if there's a subpixel offset.
            if (pageObj.IsImage && pageObj.Matrix == PdfMatrix.FillPage(page.Width, page.Height) && imageObject == null)
            {
                imageObject = pageObj;
            }
            else if (pageObj.IsText && (imageObject == null || IsInvisibleText(pageObj)))
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