/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NAPS2.Scan;
using NUnit.Framework;

namespace NAPS2.Tests.Unit
{
    [TestFixture(Category = "Unit,Fast")]
    public class ScannedImageListTests
    {
        [SetUp]
        public void SetUp()
        {
            imageList = new ScannedImageList();
        }

        [TearDown]
        public void TearDown()
        {
            imageList = null;
        }

        private ScannedImageList imageList;

        private void AddImages(int items)
        {
            foreach (int i in Enumerable.Range(0, items))
            {
                imageList.Images.Add(new ScannedImageStub { Number = i });
            }
        }

        private void AssertItems(IEnumerable<int> expectedItems)
        {
            CollectionAssert.AreEqual(expectedItems, imageList.Images.Select(x => ((ScannedImageStub)x).Number), "Order not correct");
        }

        private IEnumerable MoveDownCases
        {
            get
            {
                yield return new TestCaseData(0, new int[] { }, new int[] { }, new int[] { }).SetName("Do nothing (no items)");
                yield return new TestCaseData(1, new int[] { }, new[] { 0 }, new int[] { }).SetName("Do nothing (no selection)");
                yield return new TestCaseData(1, new[] { 0 }, new[] { 0 }, new[] { 0 }).SetName("Do nothing (1 item)");
                yield return new TestCaseData(2, new[] { 1 }, new[] { 0, 1 }, new[] { 1 }).SetName("Do nothing (bottom item)");
                yield return new TestCaseData(2, new[] { 0 }, new[] { 1, 0 }, new[] { 1 }).SetName("Move 1");
                yield return new TestCaseData(3, new[] { 0, 2 }, new[] { 1, 0, 2 }, new[] { 1, 2 }).SetName("Move 1/2");
                yield return new TestCaseData(3, new[] { 0, 1 }, new[] { 2, 0, 1 }, new[] { 1, 2 }).SetName("Move 2/2");
                yield return new TestCaseData(3, new[] { 1, 2 }, new[] { 0, 1, 2 }, new[] { 1, 2 }).SetName("Move none (bottom 2)");
                yield return new TestCaseData(3, new[] { 0, 1, 2 }, new[] { 0, 1, 2 }, new[] { 0, 1, 2 }).SetName("Move none (all 3)");
            }
        }

        [TestCaseSource("MoveDownCases")]
        public void MoveDown_Cases_NewSelectionCorrect(int items, int[] selection, int[] expectedItems, int[] expectedSelection)
        {
            AddImages(items);
            IEnumerable<int> newSelection = imageList.MoveDown(selection);
            CollectionAssert.AreEquivalent(expectedSelection, newSelection, "Selection not correct");
        }

        [TestCaseSource("MoveDownCases")]
        public void MoveDown_Cases_ItemsCorrect(int items, int[] selection, int[] expectedItems, int[] expectedSelection)
        {
            AddImages(items);
            imageList.MoveDown(selection);
            AssertItems(expectedItems);
        }

        private IEnumerable MoveUpCases
        {
            get
            {
                yield return new TestCaseData(0, new int[] { }, new int[] { }, new int[] { }).SetName("Do nothing (no items)");
                yield return new TestCaseData(1, new int[] { }, new[] { 0 }, new int[] { }).SetName("Do nothing (no selection)");
                yield return new TestCaseData(1, new[] { 0 }, new[] { 0 }, new[] { 0 }).SetName("Do nothing (1 item)");
                yield return new TestCaseData(2, new[] { 1 }, new[] { 1, 0 }, new[] { 0 }).SetName("Move 1");
                yield return new TestCaseData(2, new[] { 0 }, new[] { 0, 1 }, new[] { 0 }).SetName("Do nothing (top item)");
                yield return new TestCaseData(3, new[] { 0, 2 }, new[] { 0, 2, 1 }, new[] { 0, 1 }).SetName("Move 1/2");
                yield return new TestCaseData(3, new[] { 0, 1 }, new[] { 0, 1, 2 }, new[] { 0, 1 }).SetName("Move none (bottom 2)");
                yield return new TestCaseData(3, new[] { 1, 2 }, new[] { 1, 2, 0 }, new[] { 0, 1 }).SetName("Move 2/2");
                yield return new TestCaseData(3, new[] { 0, 1, 2 }, new[] { 0, 1, 2 }, new[] { 0, 1, 2 }).SetName("Move none (all 3)");
            }
        }

        [TestCaseSource("MoveUpCases")]
        public void MoveUp_Cases_NewSelectionCorrect(int items, int[] selection, int[] expectedItems, int[] expectedSelection)
        {
            AddImages(items);
            IEnumerable<int> newSelection = imageList.MoveUp(selection);
            CollectionAssert.AreEquivalent(expectedSelection, newSelection, "Selection not correct");
        }

        [TestCaseSource("MoveUpCases")]
        public void MoveUp_Cases_ItemsCorrect(int items, int[] selection, int[] expectedItems, int[] expectedSelection)
        {
            AddImages(items);
            imageList.MoveUp(selection);
            AssertItems(expectedItems);
        }

        private IEnumerable RotateFlipCases
        {
            get
            {
                yield return new TestCaseData(0, new int[] { }).SetName("Do nothing (no items)");
                yield return new TestCaseData(1, new int[] { }).SetName("Do nothing (no selection)");
                yield return new TestCaseData(1, new[] { 0 }).SetName("Flip 1/1");
                yield return new TestCaseData(2, new[] { 0 }).SetName("Flip 1/2 (first)");
                yield return new TestCaseData(2, new[] { 1 }).SetName("Flip 1/2 (last)");
                yield return new TestCaseData(2, new[] { 0, 1 }).SetName("Flip 2/2");
            }
        }

        [TestCaseSource("RotateFlipCases")]
        public void RotateFlip_Cases_NewSelectionCorrect(int items, int[] selection)
        {
            AddImages(items);
            IEnumerable<int> newSelection = imageList.RotateFlip(selection, RotateFlipType.Rotate90FlipNone);
            CollectionAssert.AreEquivalent(selection, newSelection);
        }

        [TestCaseSource("RotateFlipCases")]
        public void RotateFlip_Cases_RotateFlipCalledOnce(int items, int[] selection)
        {
            AddImages(items);
            imageList.RotateFlip(selection, RotateFlipType.Rotate90FlipNone);
            foreach (int i in selection)
            {
                Assert.AreEqual(((ScannedImageStub)imageList.Images[i]).RotateFlipCalled, 1);
            }
        }

        [TestCaseSource("RotateFlipCases")]
        public void RotateFlip_Cases_RotateFlipNotCalled(int items, int[] selection)
        {
            AddImages(items);
            imageList.RotateFlip(selection, RotateFlipType.Rotate90FlipNone);
            IEnumerable<int> notSelected = Enumerable.Range(0, items).Except(selection);
            foreach (int i in notSelected)
            {
                Assert.AreEqual(((ScannedImageStub)imageList.Images[i]).RotateFlipCalled, 0);
            }
        }

        private IEnumerable DeleteCases
        {
            get
            {
                yield return new TestCaseData(0, new int[] { }).SetName("Do nothing (no items)");
                yield return new TestCaseData(1, new int[] { }).SetName("Do nothing (no selection)");
                yield return new TestCaseData(1, new[] { 0 }).SetName("Delete 1/1");
                yield return new TestCaseData(2, new[] { 0 }).SetName("Delete 1/2 (first)");
                yield return new TestCaseData(2, new[] { 1 }).SetName("Delete 1/2 (last)");
                yield return new TestCaseData(2, new[] { 0, 1 }).SetName("Delete 2/2");
            }
        }

        [TestCaseSource("DeleteCases")]
        public void Delete_Cases_DisposeCalled(int items, int[] selection)
        {
            AddImages(items);
            List<IScannedImage> images = imageList.Images.ToList();
            imageList.Delete(selection);
            foreach (int i in selection)
            {
                Assert.True(((ScannedImageStub)images[i]).DisposeCalled);
            }
        }

        [TestCaseSource("DeleteCases")]
        public void Delete_Cases_ItemsCorrect(int items, int[] selection)
        {
            AddImages(items);
            imageList.Delete(selection);
            IEnumerable<int> notSelected = Enumerable.Range(0, items).Except(selection);
            AssertItems(notSelected);
        }

        [TestCaseSource("DeleteCases")]
        public void Delete_Cases_DisposeNotCalled(int items, int[] selection)
        {
            AddImages(items);
            List<IScannedImage> images = imageList.Images.ToList();
            imageList.Delete(selection);
            IEnumerable<int> notSelected = Enumerable.Range(0, items).Except(selection);
            foreach (int i in notSelected)
            {
                Assert.False(((ScannedImageStub)images[i]).DisposeCalled);
            }
        }

    }

    internal class ScannedImageStub : IScannedImage
    {
        public int Number { get; set; }

        public int RotateFlipCalled { get; private set; }

        public bool DisposeCalled { get; private set; }

        public Bitmap GetImage()
        {
            return null;
        }

        public Bitmap Thumbnail
        {
            get { return null; }
        }

        public void RotateFlip(RotateFlipType rotateFlipType)
        {
            RotateFlipCalled += 1;
        }

        public void MovedTo(int index)
        {
        }

        public void Dispose()
        {
            DisposeCalled = true;
        }
    }
}
