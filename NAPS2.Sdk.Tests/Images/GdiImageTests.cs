using System.Drawing;
using NAPS2.Images.Gdi;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class GdiImageTests
{
    [Fact]
    public void LockAndDisposeTwice()
    {
        var image = new GdiImage(new Bitmap(100, 100));
        var lockState = image.Lock(LockMode.ReadWrite, out var scan0, out var stride);
        lockState.Dispose();
        lockState.Dispose();
    }
}