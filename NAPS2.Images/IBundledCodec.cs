using System.Diagnostics.CodeAnalysis;

namespace NAPS2.Images;

/// <summary>
/// A codec bundled with NAPS2 (e.g. a native library) that's used instead of the platform codec for a particular file
/// format. See ImageContext.BundledEncoders and ImageContext.BundledDecoders.
/// </summary>
internal interface IBundledCodec
{
    /// <summary>
    /// Encodes the image to the stream. Returns false (without writing to the stream) if the codec doesn't support
    /// the image, in which case the platform encoder is used instead.
    /// </summary>
    bool TryEncode(IMemoryImage image, Stream stream, ImageSaveOptions options);

    /// <summary>
    /// Decodes an image from the stream. Returns false (without reading from the stream) if the codec doesn't support
    /// the image, in which case the platform decoder is used instead.
    /// </summary>
    bool TryDecode(ImageContext imageContext, Stream stream, [NotNullWhen(true)] out IMemoryImage? image);
}
