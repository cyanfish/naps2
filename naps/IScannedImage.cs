using System;
namespace NAPS
{
    public interface IScannedImage : IDisposable
    {
        System.Drawing.Bitmap GetBaseImage();
        void RotateFlip(System.Drawing.RotateFlipType rotateFlipType);
        System.Drawing.Bitmap Thumbnail { get; }
    }
}
