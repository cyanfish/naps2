using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Lib.Tests.Util;

public class ListMutationTests
{
    [Fact]
    public void MoveDown()
    {
        var m = new ListMutation<string>.MoveDown();
        var list = new List<string> { "a", "b", "c", "d" };
        var sel = ListSelection.Of("c");

        // Move single down
        m.Apply(list, ref sel);
        Assert.Equal(list, new[] { "a", "b", "d", "c" });
        CollectionAsserts.SameItems(sel, new[] { "c" });

        // End of list, don't do anything
        m.Apply(list, ref sel);
        Assert.Equal(list, new[] { "a", "b", "d", "c" });
        CollectionAsserts.SameItems(sel, new[] { "c" });
            
        // Move multiple down
        sel = ListSelection.Of("a", "d");
        m.Apply(list, ref sel);
        Assert.Equal(list, new[] { "b", "a", "c", "d" });
        CollectionAsserts.SameItems(sel, new[] { "a", "d" });
            
        // Multiple down, one at end of list
        m.Apply(list, ref sel);
        Assert.Equal(list, new[] { "b", "c", "a", "d" });
        CollectionAsserts.SameItems(sel, new[] { "a", "d" });
            
        // Multiple down, both at end of list
        m.Apply(list, ref sel);
        Assert.Equal(list, new[] { "b", "c", "a", "d" });
        CollectionAsserts.SameItems(sel, new[] { "a", "d" });
    }
        
    [Fact]
    public void MoveUp()
    {
        var m = new ListMutation<string>.MoveUp();
        var list = new List<string> { "a", "b", "c", "d" };
        var sel = ListSelection.Of("b");

        // Move single up
        m.Apply(list, ref sel);
        Assert.Equal(list, new[] { "b", "a", "c", "d" });
        CollectionAsserts.SameItems(sel, new[] { "b" });

        // End of list, don't do anything
        m.Apply(list, ref sel);
        Assert.Equal(list, new[] { "b", "a", "c", "d" });
        CollectionAsserts.SameItems(sel, new[] { "b" });
            
        // Move multiple up
        sel = ListSelection.Of("a", "d");
        m.Apply(list, ref sel);
        Assert.Equal(list, new[] { "a", "b", "d", "c" });
        CollectionAsserts.SameItems(sel, new[] { "a", "d" });
            
        // Multiple up, one at end of list
        m.Apply(list, ref sel);
        Assert.Equal(list, new[] { "a", "d", "b", "c" });
        CollectionAsserts.SameItems(sel, new[] { "a", "d" });
            
        // Multiple up, both at end of list
        m.Apply(list, ref sel);
        Assert.Equal(list, new[] { "a", "d", "b", "c" });
        CollectionAsserts.SameItems(sel, new[] { "a", "d" });
    }
        
    // TODO: More
}