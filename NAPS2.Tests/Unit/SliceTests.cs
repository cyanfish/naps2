using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;
using NUnit.Framework;

namespace NAPS2.Tests.Unit
{
    [TestFixture(Category = "unit,fast")]
    class SliceTests
    {
        [Test]
        public void Parse()
        {
            AssertSlice(Slice.Parse("[]", out _), null, null, null, null);

            AssertSlice(Slice.Parse("[0]", out _), 0, null, null, null);
            AssertSlice(Slice.Parse("[1]", out _), 1, null, null, null);
            AssertSlice(Slice.Parse("[-1]", out _), -1, null, null, null);
            AssertSlice(Slice.Parse("[-2]", out _), -2, null, null, null);
            AssertSlice(Slice.Parse("[asdpsakoid2238(*S(D)*A(]", out _), null, null, null, null);
            
            AssertSlice(Slice.Parse("[2:]", out _), null, 2, null, null);
            AssertSlice(Slice.Parse("[:2]", out _), null, null, 2, null);
            AssertSlice(Slice.Parse("[2:-2]", out _), null, 2, -2, null);
            AssertSlice(Slice.Parse("[::2]", out _), null, null, null, 2);
            AssertSlice(Slice.Parse("[::-2]", out _), null, null, null, -2);
            AssertSlice(Slice.Parse("[2:3:4]", out _), null, 2, 3, 4);
        }

        private void AssertSlice(Slice s, int? index, int? start, int? end, int? step)
        {
            Assert.AreEqual(index, s.Index);
            Assert.AreEqual(start, s.Start);
            Assert.AreEqual(end, s.End);
            Assert.AreEqual(step, s.Step);
        }

        [Test]
        public void Indices()
        {
            CollectionAssert.AreEqual(new [] { 0 }, Slice.Item(0).Indices(7));
            CollectionAssert.AreEqual(new [] { 1 }, Slice.Item(1).Indices(7));
            CollectionAssert.AreEqual(new [] { 6 }, Slice.Item(-1).Indices(7));
            CollectionAssert.AreEqual(new [] { 5 }, Slice.Item(-2).Indices(7));
            CollectionAssert.AreEqual(new int[] { }, Slice.Item(7).Indices(7));
            CollectionAssert.AreEqual(new [] { 0 }, Slice.Item(-7).Indices(7));
            CollectionAssert.AreEqual(new int[] { }, Slice.Item(-8).Indices(7));
            
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 5, 6 }, Slice.Range(null, null, null).Indices(7));

            CollectionAssert.AreEqual(new[] { 2, 3, 4, 5, 6 }, Slice.Range(2, null, null).Indices(7));
            CollectionAssert.AreEqual(new int[] { }, Slice.Range(7, null, null).Indices(7));
            CollectionAssert.AreEqual(new[] { 5, 6 }, Slice.Range(-2, null, null).Indices(7));

            CollectionAssert.AreEqual(new[] { 0, 1 }, Slice.Range(null, 2, null).Indices(7));
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, Slice.Range(null, -2, null).Indices(7));
            CollectionAssert.AreEqual(new int[] { }, Slice.Range(null, -7, null).Indices(7));

            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 5, 6 }, Slice.Range(null, null, 0).Indices(7));
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 5, 6 }, Slice.Range(null, null, 1).Indices(7));
            CollectionAssert.AreEqual(new[] { 6, 5, 4, 3, 2, 1, 0 }, Slice.Range(null, null, -1).Indices(7));
            CollectionAssert.AreEqual(new[] { 0, 2, 4, 6 }, Slice.Range(null, null, 2).Indices(7));
            CollectionAssert.AreEqual(new[] { 6, 4, 2, 0 }, Slice.Range(null, null, -2).Indices(7));

            CollectionAssert.AreEqual(new[] { 1, 4 }, Slice.Range(1, 5, 3).Indices(7));
            CollectionAssert.AreEqual(new[] { 6, 3 }, Slice.Range(6, 2, -3).Indices(7));
        }
    }
}
