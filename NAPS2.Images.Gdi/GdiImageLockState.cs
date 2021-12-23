using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

public record GdiImageLockState(BitmapData BitmapData) : ImageLockState;
