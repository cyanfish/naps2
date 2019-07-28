using System.Collections.Generic;
using NAPS2.Images;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Util
{
    public class ListMutationTests
    {
        [Fact]
        public void MoveDown()
        {
            var m = new ListMutation<string>.MoveDown();
            var list = new List<string> { "a", "b", "c" };
            var sel = ListSelection.Single("b");

            m.Apply(list, ref sel);

            Assert.Equal(list, new[] { "a", "c", "b" });
            CollectionAsserts.SameItems(sel, new[] { "b" });
        }

        [Fact]
        public void MoveDown_EndOfList()
        {
            var m = new ListMutation<string>.MoveDown();
            var list = new List<string> { "a", "b", "c" };
            var sel = ListSelection.Single("c");

            m.Apply(list, ref sel);

            Assert.Equal(list, new[] { "a", "b", "c" });
            CollectionAsserts.SameItems(sel, new[] { "c" });
        }

        [Fact]
        public void MoveDown_Multiple()
        {
            var m = new ListMutation<string>.MoveDown();
            var list = new List<string> { "a", "b", "c", "d" };
            var sel = ListSelection.From(new[] { "a", "c" });

            m.Apply(list, ref sel);

            Assert.Equal(list, new[] { "b", "a", "d", "c" });
            CollectionAsserts.SameItems(sel, new[] { "a", "c" });
        }
        
        // TODO: More
    }
}
