using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public interface IImageFactory
    {
        /// <summary>
        /// Decodes an image from the given stream and file extension.
        /// </summary>
        /// <param name="stream">The image data, in a common format (JPEG, PNG, etc).</param>
        /// <param name="ext">A file extension hinting at the image format. When possible, the contents of the stream should be used to definitively determine the image format.</param>
        /// <returns></returns>
        IImage Decode(Stream stream, string ext);

        /// <summary>
        /// Decodes an image from the given file path.
        /// </summary>
        /// <param name="path">The image path.</param>
        /// <returns></returns>
        IImage Decode(string path);

        /// <summary>
        /// Decodes an image from the given stream and file extension.
        /// If there are multiple images (e.g. TIFF), multiple results will be returned;
        /// however, only the enumerator's current IStorage is guaranteed to be valid.
        /// </summary>
        /// <param name="stream">The image data, in a common format (JPEG, PNG, etc).</param>
        /// <param name="ext">A file extension hinting at the image format. When possible, the contents of the stream should be used to definitively determine the image format.</param>
        /// <param name="count">The number of returned images.</param>
        /// <returns></returns>
        IEnumerable<IImage> DecodeMultiple(Stream stream, string ext, out int count);

        /// <summary>
        /// Decodes an image from the given file path.
        /// </summary>
        /// <param name="path">The image path.</param>
        /// <param name="count">The number of returned images.</param>
        /// <returns></returns>
        IEnumerable<IImage> DecodeMultiple(string path, out int count);

        IImage FromDimensions(int width, int height, StoragePixelFormat pixelFormat);
    }
}
