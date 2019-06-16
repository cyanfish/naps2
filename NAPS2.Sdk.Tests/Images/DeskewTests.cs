using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images;
using NAPS2.Images.Storage;
using Xunit;

namespace NAPS2.Sdk.Tests.Images
{
    public class DeskewTests
    {
        // Just a sanity check...
        // TODO: More thorough deskew performance testing.
        [Fact]
        public void Deskew()
        {
            var image = new GdiImage(DeskewTestsData.skewed);
            Assert.Equal(StoragePixelFormat.RGB24, image.PixelFormat);
            var deskewer = new HoughLineDeskewer();
            var skewAngle = deskewer.GetSkewAngle(image);
            Assert.InRange(skewAngle, 15.5, 16.5);
        }

        [Fact]
        public void DeskewBlackAndWhite()
        {
            var image = new GdiImage(DeskewTestsData.skewed_bw);
            Assert.Equal(StoragePixelFormat.BW1, image.PixelFormat);
            var deskewer = new HoughLineDeskewer();
            var skewAngle = deskewer.GetSkewAngle(image);
            Assert.InRange(skewAngle, 15.5, 16.5);
        }
    }
}
