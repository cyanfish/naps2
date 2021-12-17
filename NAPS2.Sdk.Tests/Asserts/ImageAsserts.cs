using Xunit;

namespace NAPS2.Sdk.Tests.Asserts;

public static class ImageAsserts
{
    public static unsafe void Similar(IImage first, IImage second, double rmseThreshold)
    {
        Assert.Equal(first.Width, second.Width);
        Assert.Equal(first.Height, second.Height);
        Assert.Equal(first.PixelFormat, second.PixelFormat);
        Assert.Equal(first.HorizontalResolution, second.HorizontalResolution);
        Assert.Equal(first.VerticalResolution, second.VerticalResolution);

        var lock1 = first.Lock(LockMode.ReadOnly, out var scan01, out var stride1);
        var lock2 = second.Lock(LockMode.ReadOnly, out var scan02, out var stride2);
        try
        {
            if (first.PixelFormat != StoragePixelFormat.RGB24)
            {
                throw new InvalidOperationException("Unsupported pixel format");
            }
            int width = first.Width;
            int height = first.Height;
            int bytesPerPixel = 3;
            long total = 0;
            long div = width * height * 3;
            byte* data1 = (byte*)scan01;
            byte* data2 = (byte*)scan02;
            for (int y = 0; y < height; y++)
            {
                byte* row1 = data1 + stride1 * y;
                byte* row2 = data2 + stride2 * y;
                for (int x = 0; x < width; x++)
                {
                    byte* pixel1 = row1 + x * bytesPerPixel;
                    byte* pixel2 = row2 + x * bytesPerPixel;

                    byte r1 = *pixel1;
                    byte g1 = *(pixel1 + 1);
                    byte b1 = *(pixel1 + 2);

                    byte r2 = *pixel2;
                    byte g2 = *(pixel2 + 1);
                    byte b2 = *(pixel2 + 2);

                    total += (r1 - r2) * (r1 - r2) + (g1 - g2) * (g1 - g2) + (b1 - b2) * (b1 - b2);
                }
            }

            double rmse = Math.Sqrt(total / (double) div);
            Assert.True(rmse <= rmseThreshold, $"RMSE was {rmse}, expected <= {rmseThreshold}");
        }
        finally
        {
            first.Unlock(lock1);
            second.Unlock(lock2);
        }
    }
}