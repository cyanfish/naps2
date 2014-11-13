/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAPS2.ImportExport.Images;
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

        [Test]
        public void GetFileNames_RelativePath_StaysRelative()
        {
            var namer = new ImageFileNamer();
            var result = namer.GetFileNames("base.jpg", 1);
            Assert.AreEqual(@"base.jpg", result.ElementAt(0));
        }
    }
}
