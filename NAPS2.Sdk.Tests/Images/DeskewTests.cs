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
            var deskewer = new HoughLineDeskewer();
            var skewAngle = deskewer.GetSkewAngle(image);
            Assert.InRange(skewAngle, 15.5, 16.5);
        }
    }
}
