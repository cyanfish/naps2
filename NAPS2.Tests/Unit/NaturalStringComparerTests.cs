using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;
using NUnit.Framework;

namespace NAPS2.Tests.Unit
{
    [TestFixture(Category = "unit,fast")]
    class NaturalStringComparerTests
    {
        [Test]
        public void Compare()
        {
            var comparer = new NaturalStringComparer();

            Assert.That(comparer.Compare("abc", "abc"), Is.EqualTo(0));
            Assert.That(comparer.Compare("ABC", "abc"), Is.EqualTo(0));

            Assert.That(comparer.Compare("aac", "abc"), Is.LessThan(0));
            Assert.That(comparer.Compare("abc", "aac"), Is.GreaterThan(0));

            Assert.That(comparer.Compare("acc", "abc"), Is.GreaterThan(0));
            Assert.That(comparer.Compare("abc", "acc"), Is.LessThan(0));

            Assert.That(comparer.Compare("a01", "a10"), Is.LessThan(0));
            Assert.That(comparer.Compare("a10", "a01"), Is.GreaterThan(0));

            Assert.That(comparer.Compare("a1b", "a10b"), Is.LessThan(0));
            Assert.That(comparer.Compare("a10b", "a1b"), Is.GreaterThan(0));

            Assert.That(comparer.Compare("a١b", "a١٠b"), Is.LessThan(0));
            Assert.That(comparer.Compare("a١٠b", "a١b"), Is.GreaterThan(0));
        }
    }
}
