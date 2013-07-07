using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NAPS2.Tests.Unit
{
    [TestFixture(Category = "unit,fast")]
    class ImageFileNamerTests
    {
        private const string BaseFileName = @"C:\something\base.jpg";

        [Test]
        public void GetFileNames_NoImages_ZeroResults()
        {
            var namer = new ImageFileNamer();
            var result = namer.GetFileNames(BaseFileName, 0);
            CollectionAssert.AreEqual(new List<string>() { }, result);
        }

        [Test]
        public void GetFileNames_OneImage_OneResultWithNoNumber()
        {
            var namer = new ImageFileNamer();
            var result = namer.GetFileNames(BaseFileName, 1);
            CollectionAssert.AreEqual(new List<string>()
            {
                @"C:\something\base.jpg"
            }, result);
        }

        [Test]
        public void GetFileNames_TwoImages_TwoResultsWithNumbers()
        {
            var namer = new ImageFileNamer();
            var result = namer.GetFileNames(BaseFileName, 2);
            CollectionAssert.AreEqual(new List<string>()
            {
                @"C:\something\base1.jpg",
                @"C:\something\base2.jpg"
            }, result);
        }

        [Test]
        public void GetFileNames_NineImages_NineResultsWithSingleDigitNumbers()
        {
            var namer = new ImageFileNamer();
            var result = namer.GetFileNames(BaseFileName, 9);
            CollectionAssert.AreEqual(new List<string>()
            {
                @"C:\something\base1.jpg",
                @"C:\something\base2.jpg",
                @"C:\something\base3.jpg",
                @"C:\something\base4.jpg",
                @"C:\something\base5.jpg",
                @"C:\something\base6.jpg",
                @"C:\something\base7.jpg",
                @"C:\something\base8.jpg",
                @"C:\something\base9.jpg"
            }, result);
        }

        [Test]
        public void GetFileNames_TenImages_TenResultsWithPaddedNumbers()
        {
            var namer = new ImageFileNamer();
            var result = namer.GetFileNames(BaseFileName, 10);
            CollectionAssert.AreEqual(new List<string>()
            {
                @"C:\something\base01.jpg",
                @"C:\something\base02.jpg",
                @"C:\something\base03.jpg",
                @"C:\something\base04.jpg",
                @"C:\something\base05.jpg",
                @"C:\something\base06.jpg",
                @"C:\something\base07.jpg",
                @"C:\something\base08.jpg",
                @"C:\something\base09.jpg",
                @"C:\something\base10.jpg"
            }, result);
        }

        [Test]
        public void GetFileNames_99Images_TwoDigitPadding()
        {
            var namer = new ImageFileNamer();
            var result = namer.GetFileNames(BaseFileName, 99);
            Assert.AreEqual(@"C:\something\base01.jpg", result.ElementAt(0));
        }

        [Test]
        public void GetFileNames_100Images_ThreeDigitPadding()
        {
            var namer = new ImageFileNamer();
            var result = namer.GetFileNames(BaseFileName, 100);
            Assert.AreEqual(@"C:\something\base001.jpg", result.ElementAt(0));
        }
    }
}
