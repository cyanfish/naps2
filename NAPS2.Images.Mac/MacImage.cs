using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Mac;

public class MacImage : IMemoryImage
{
    internal readonly NSBitmapImageRep _imageRep;

    public MacImage(ImageContext imageContext, NSImage image)
    {
        ImageContext = imageContext ?? throw new ArgumentNullException(nameof(imageContext));
        NsImage = image;
        // TODO: Better error checking
        lock (MacImageContext.ConstructorLock)
        {
#if MONOMAC
            _imageRep = new NSBitmapImageRep(Image.Representations()[0].Handle, false);
#else
            _imageRep = (NSBitmapImageRep) NsImage.Representations()[0];
#endif
        }
        // TODO: Also verify color spaces.
        // TODO: How to handle samplesperpixel = 3 here?
        if (_imageRep.BitsPerPixel == 32 && _imageRep.BitsPerSample == 8) // && _imageRep.SamplesPerPixel == 4)
        {
            PixelFormat = ImagePixelFormat.ARGB32;
        }
        else if (_imageRep.BitsPerPixel == 24 && _imageRep.BitsPerSample == 8 && _imageRep.SamplesPerPixel == 3)
        {
            PixelFormat = ImagePixelFormat.RGB24;
        }
        else if (_imageRep.BitsPerPixel == 8 && _imageRep.BitsPerSample == 8 && _imageRep.SamplesPerPixel == 1)
        {
            PixelFormat = ImagePixelFormat.Gray8;
        }
        else if (_imageRep.BitsPerPixel == 1 && _imageRep.BitsPerSample == 1 && _imageRep.SamplesPerPixel == 1)
        {
            PixelFormat = ImagePixelFormat.BW1;
        }
        else
        {
            // TODO: Draw on a known format image
            // See https://stackoverflow.com/questions/52675655/how-to-keep-nsbitmapimagerep-from-creating-lots-of-intermediate-cgimages
            throw new Exception("Unexpected image representation");
        }
        LogicalPixelFormat = PixelFormat;
    }

    public ImageContext ImageContext { get; }

    public NSImage NsImage { get; }

    public void Dispose()
    {
        Image.Dispose();
        // TODO: Does this need to dispose the imageRep?
    }

    public int Width => (int) _imageRep.PixelsWide;
    public int Height => (int) _imageRep.PixelsHigh;
    public float HorizontalResolution => (float) Image.Size.Width.ToDouble() / Width * 72;
    public float VerticalResolution => (float) Image.Size.Height.ToDouble() / Height * 72;

    public void SetResolution(float xDpi, float yDpi)
    {
        // TODO: Image size or imagerep size?
        if (xDpi > 0 && yDpi > 0)
        {
            Image.Size = new CGSize(xDpi / 72 * Width, yDpi / 72 * Height);
        }
    }

    public ImagePixelFormat PixelFormat { get; }

    public ImageLockState Lock(LockMode lockMode, out BitwiseImageData imageData)
    {
        var ptr = _imageRep.BitmapData;
        var stride = (int) _imageRep.BytesPerRow;
        var subPixelType = PixelFormat switch
        {
            // TODO: Base subpixel type on _imageRep.BitmapFormat
            ImagePixelFormat.RGB24 => SubPixelType.Rgb,
            ImagePixelFormat.ARGB32 => SubPixelType.Rgba,
            ImagePixelFormat.Gray8 => SubPixelType.Gray,
            ImagePixelFormat.BW1 => SubPixelType.Bit,
            _ => throw new InvalidOperationException("Unsupported pixel format")
        };
        imageData = new BitwiseImageData(ptr, new PixelInfo(Width, Height, subPixelType, stride));
        return new MacImageLockState();
    }

    // TODO: Should we implement some kind of actual locking?
    public class MacImageLockState : ImageLockState
    {
        public override void Dispose()
        {
        }
    }

    public ImageFileFormat OriginalFileFormat { get; set; }

    public ImagePixelFormat LogicalPixelFormat { get; set; }

    public NSImage Image => NsImage;

    public void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unspecified, int quality = -1)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            imageFormat = ImageContext.GetFileFormatFromExtension(path);
        }
        var rep = GetRepForSaving(imageFormat, quality);
        if (!rep.Save(path, false, out var error))
        {
            throw new IOException(error!.Description);
        }
    }

    public void Save(Stream stream, ImageFileFormat imageFormat, int quality = -1)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFormat));
        }
        var rep = GetRepForSaving(imageFormat, quality);
        rep.AsStream().CopyTo(stream);
    }

    private NSData GetRepForSaving(ImageFileFormat imageFormat, int quality)
    {
        lock (MacImageContext.ConstructorLock)
        {
            if (imageFormat == ImageFileFormat.Jpeg)
            {
                var props = quality == -1
                    ? null
                    : NSDictionary.FromObjectAndKey(NSNumber.FromDouble(quality / 100.0),
                        NSBitmapImageRep.CompressionFactor);
                return _imageRep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Jpeg, props);
            }
            if (imageFormat == ImageFileFormat.Png)
            {
                return _imageRep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, null);
            }
            if (imageFormat == ImageFileFormat.Bmp)
            {
                return _imageRep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Bmp, null);
            }
            // TODO: Do we need/want to handle tiff saving?
            throw new InvalidOperationException("Unsupported image format");
        }
    }

    public IMemoryImage Clone()
    {
        lock (MacImageContext.ConstructorLock)
        {
            if (PixelFormat == ImagePixelFormat.BW1)
            {
                // TODO: Trying to copy the NSImage seems to fail specifically for black and white images.
                // I'm not sure why.
                return this.Copy();
            }

#if MONOMAC
            var nsImage = new NSImage(Image.Copy().Handle, true);
#else
            var nsImage = (NSImage) NsImage.Copy();
#endif
            return new MacImage(ImageContext, nsImage)
            {
                OriginalFileFormat = OriginalFileFormat
            };
        }
    }
}