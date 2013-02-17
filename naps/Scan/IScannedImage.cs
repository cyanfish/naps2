using System;
using System.Drawing;

namespace NAPS.Scan
{
    public interface IScannedImage : IDisposable
    {
        /// <summary>
        /// Gets a copy of the scanned image. The consumer is responsible for calling Dispose on the returned bitmap.
        /// </summary>
        /// <returns>A copy of the scanned image.</returns>
        Bitmap GetImage();

        /// <summary>
        /// Gets a thumbnail bitmap for the image. The consumer should NOT call Dispose on the returned bitmap.
        /// </summary>
        Bitmap Thumbnail { get; }

        /// <summary>
        /// Transforms (rotates and/or flips) the image.
        /// </summary>
        /// <param name="rotateFlipType">The transformation type.</param>
        void RotateFlip(RotateFlipType rotateFlipType);
    }
}
