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
        Assert.Equal(new[] { "a", "b", "d", "c" }, list);
        CollectionAsserts.SameItems(sel, new[] { "c" });

        // End of list, don't do anything
        m.Apply(list, ref sel);
        Assert.Equal(new[] { "a", "b", "d", "c" }, list);
        CollectionAsserts.SameItems(sel, new[] { "c" });
            
        // Move multiple down
        sel = ListSelection.Of("a", "d");
        m.Apply(list, ref sel);
        Assert.Equal(new[] { "b", "a", "c", "d" }, list);
        CollectionAsserts.SameItems(sel, new[] { "a", "d" });
            
        // Multiple down, one at end of list
        m.Apply(list, ref sel);
        Assert.Equal(new[] { "b", "c", "a", "d" }, list);
        CollectionAsserts.SameItems(sel, new[] { "a", "d" });
            
        // Multiple down, both at end of list
        m.Apply(list, ref sel);
        Assert.Equal(new[] { "b", "c", "a", "d" }, list);
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
        Assert.Equal(new[] { "b", "a", "c", "d" }, list);
        CollectionAsserts.SameItems(sel, new[] { "b" });

        // End of list, don't do anything
        m.Apply(list, ref sel);
        Assert.Equal(new[] { "b", "a", "c", "d" }, list);
        CollectionAsserts.SameItems(sel, new[] { "b" });
            
        // Move multiple up
        sel = ListSelection.Of("a", "d");
        m.Apply(list, ref sel);
        Assert.Equal(new[] { "a", "b", "d", "c" }, list);
        CollectionAsserts.SameItems(sel, new[] { "a", "d" });
            
        // Multiple up, one at end of list
        m.Apply(list, ref sel);
        Assert.Equal(new[] { "a", "d", "b", "c" }, list);
        CollectionAsserts.SameItems(sel, new[] { "a", "d" });
            
        // Multiple up, both at end of list
        m.Apply(list, ref sel);
        Assert.Equal(new[] { "a", "d", "b", "c" }, list);
        CollectionAsserts.SameItems(sel, new[] { "a", "d" });
    }

    [Fact]
    public void MoveDown_SelectionNotInListOrder()
    {
        var list = new List<string> { "a", "b", "c", "d" };
        var sel = ListSelection.Of("c", "a");

        new ListMutation<string>.MoveDown().Apply(list, ref sel);
        Assert.Equal(new[] { "b", "a", "d", "c" }, list);

        sel = ListSelection.Of("c", "d", "a", "b");
        new ListMutation<string>.MoveDown().Apply(list, ref sel);
        Assert.Equal(new[] { "b", "a", "d", "c" }, list);
    }

    [Fact]
    public void MoveUp_SelectionNotInListOrder()
    {
        var list = new List<string> { "a", "b", "c", "d" };
        var sel = ListSelection.Of("d", "b");

        new ListMutation<string>.MoveUp().Apply(list, ref sel);
        Assert.Equal(new[] { "b", "a", "d", "c" }, list);

        sel = ListSelection.Of("c", "d", "a", "b");
        new ListMutation<string>.MoveUp().Apply(list, ref sel);
        Assert.Equal(new[] { "b", "a", "d", "c" }, list);
    }

    [Theory]
    [InlineData(new[] { "a" }, 3, new[] { "b", "c", "a", "d", "e" })]
    [InlineData(new[] { "e" }, 1, new[] { "a", "e", "b", "c", "d" })]
    [InlineData(new[] { "a", "e" }, 2, new[] { "b", "a", "e", "c", "d" })]
    [InlineData(new[] { "b", "d" }, 0, new[] { "b", "d", "a", "c", "e" })]
    [InlineData(new[] { "a", "c" }, 5, new[] { "b", "d", "e", "a", "c" })]
    [InlineData(new[] { "c" }, 2, new[] { "a", "b", "c", "d", "e" })]
    [InlineData(new[] { "c" }, 3, new[] { "a", "b", "c", "d", "e" })]
    public void MoveTo(string[] selected, int destination, string[] expected)
    {
        var list = new List<string> { "a", "b", "c", "d", "e" };
        var sel = ListSelection.From(selected);

        new ListMutation<string>.MoveTo(destination).Apply(list, ref sel);

        Assert.Equal(expected, list);
        CollectionAsserts.SameItems(sel, selected);
    }

    [Theory]
    [InlineData(new string[0], new string[0])]
    [InlineData(new[] { "1" }, new[] { "1" })]
    [InlineData(new[] { "1", "3", "5", "2", "4", "6" }, new[] { "1", "2", "3", "4", "5", "6" })]
    [InlineData(new[] { "1", "3", "5", "2", "4" }, new[] { "1", "2", "3", "4", "5" })]
    public void Interleave(string[] input, string[] expected)
    {
        AssertMutation(new ListMutation<string>.Interleave(), input, expected);
    }

    [Theory]
    [InlineData(new string[0], new string[0])]
    [InlineData(new[] { "1" }, new[] { "1" })]
    [InlineData(new[] { "1", "2", "3", "4", "5", "6" }, new[] { "1", "3", "5", "2", "4", "6" })]
    [InlineData(new[] { "1", "2", "3", "4", "5" }, new[] { "1", "3", "5", "2", "4" })]
    public void Deinterleave(string[] input, string[] expected)
    {
        AssertMutation(new ListMutation<string>.Deinterleave(), input, expected);
    }

    [Theory]
    [InlineData(new string[0], new string[0])]
    [InlineData(new[] { "1" }, new[] { "1" })]
    [InlineData(new[] { "1", "3", "5", "6", "4", "2" }, new[] { "1", "2", "3", "4", "5", "6" })]
    [InlineData(new[] { "1", "3", "5", "4", "2" }, new[] { "1", "2", "3", "4", "5" })]
    public void AltInterleave(string[] input, string[] expected)
    {
        AssertMutation(new ListMutation<string>.AltInterleave(), input, expected);
    }

    [Theory]
    [InlineData(new string[0], new string[0])]
    [InlineData(new[] { "1" }, new[] { "1" })]
    [InlineData(new[] { "1", "2", "3", "4", "5", "6" }, new[] { "1", "3", "5", "6", "4", "2" })]
    [InlineData(new[] { "1", "2", "3", "4", "5" }, new[] { "1", "3", "5", "4", "2" })]
    public void AltDeinterleave(string[] input, string[] expected)
    {
        AssertMutation(new ListMutation<string>.AltDeinterleave(), input, expected);
    }

    [Theory]
    [InlineData(false, new[] { "1", "3", "5", "2", "4", "6" }, new[] { "1", "2", "3", "4", "5", "6" })]
    [InlineData(false, new[] { "1", "3", "5", "2", "4" }, new[] { "1", "2", "3", "4", "5" })]
    [InlineData(true, new[] { "1", "3", "5", "6", "4", "2" }, new[] { "1", "2", "3", "4", "5", "6" })]
    [InlineData(true, new[] { "1", "3", "5", "4", "2" }, new[] { "1", "2", "3", "4", "5" })]
    [InlineData(true, new string[0], new string[0])]
    public void ManualDuplex(bool reverseBackSides, string[] input, string[] expected)
    {
        AssertMutation(new ListMutation<string>.ManualDuplex(reverseBackSides), input, expected);
    }

    [Fact]
    public void InterleaveRoundTrips()
    {
        for (int count = 0; count < 10; count++)
        {
            var original = Enumerable.Range(1, count).Select(x => x.ToString()).ToList();

            var list = original.ToList();
            var sel = ListSelection.Empty<string>();
            new ListMutation<string>.Deinterleave().Apply(list, ref sel);
            new ListMutation<string>.Interleave().Apply(list, ref sel);
            Assert.Equal(original, list);

            new ListMutation<string>.AltDeinterleave().Apply(list, ref sel);
            new ListMutation<string>.AltInterleave().Apply(list, ref sel);
            Assert.Equal(original, list);
        }
    }

    [Fact]
    public void ReverseAll()
    {
        var list = new List<string> { "a", "b", "c" };
        var sel = ListSelection.Of("b");

        new ListMutation<string>.ReverseAll().Apply(list, ref sel);

        Assert.Equal(new[] { "c", "b", "a" }, list);
        CollectionAsserts.SameItems(sel, new[] { "b" });
    }

    [Theory]
    [InlineData(new[] { "b", "d" }, new[] { "a", "d", "c", "b", "e" })]
    [InlineData(new[] { "a", "c", "e" }, new[] { "e", "b", "c", "d", "a" })]
    [InlineData(new[] { "a", "b", "c", "d" }, new[] { "d", "c", "b", "a", "e" })]
    [InlineData(new[] { "c" }, new[] { "a", "b", "c", "d", "e" })]
    [InlineData(new string[0], new[] { "a", "b", "c", "d", "e" })]
    [InlineData(new[] { "e", "a", "c" }, new[] { "e", "b", "c", "d", "a" })]
    [InlineData(new[] { "d", "b", "a" }, new[] { "d", "b", "c", "a", "e" })]
    public void ReverseSelection(string[] selected, string[] expected)
    {
        var list = new List<string> { "a", "b", "c", "d", "e" };
        var sel = ListSelection.From(selected);

        new ListMutation<string>.ReverseSelection().Apply(list, ref sel);

        Assert.Equal(expected, list);
        CollectionAsserts.SameItems(sel, selected);
    }

    [Fact]
    public void DeleteAll()
    {
        var list = new List<string> { "a", "b", "c" };
        var sel = ListSelection.Of("b");

        new ListMutation<string>.DeleteAll().Apply(list, ref sel);

        Assert.Empty(list);
        Assert.Empty(sel);
    }

    [Fact]
    public void DeleteSelected()
    {
        var list = new List<string> { "a", "b", "c", "d", "e" };
        var sel = ListSelection.Of("d", "a", "e");

        new ListMutation<string>.DeleteSelected().Apply(list, ref sel);

        Assert.Equal(new[] { "b", "c" }, list);
        Assert.Empty(sel);
    }

    [Fact]
    public void InsertAt()
    {
        var list = new List<string> { "a", "b" };
        var sel = ListSelection.Of("a");

        new ListMutation<string>.InsertAt(1, "x").Apply(list, ref sel);
        new ListMutation<string>.InsertAt(3, "y").Apply(list, ref sel);

        Assert.Equal(new[] { "a", "x", "b", "y" }, list);
        CollectionAsserts.SameItems(sel, new[] { "a" });
    }

    [Theory]
    [InlineData("a", new[] { "a", "x", "b", "c" })]
    [InlineData("c", new[] { "a", "b", "c", "x" })]
    [InlineData(null, new[] { "a", "b", "c", "x" })]
    [InlineData("missing", new[] { "a", "b", "c", "x" })]
    public void InsertAfter(string? predecessor, string[] expected)
    {
        var list = new List<string> { "a", "b", "c" };
        var sel = ListSelection.Empty<string>();

        new ListMutation<string>.InsertAfter("x", predecessor).Apply(list, ref sel);

        Assert.Equal(expected, list);
    }

    [Theory]
    [InlineData("a", new[] { "x", "a", "b", "c" })]
    [InlineData("c", new[] { "a", "b", "x", "c" })]
    [InlineData(null, new[] { "x", "a", "b", "c" })]
    [InlineData("missing", new[] { "x", "a", "b", "c" })]
    public void InsertBefore(string? successor, string[] expected)
    {
        var list = new List<string> { "a", "b", "c" };
        var sel = ListSelection.Empty<string>();

        new ListMutation<string>.InsertBefore("x", successor).Apply(list, ref sel);

        Assert.Equal(expected, list);
    }

    [Theory]
    [InlineData(new[] { "b", "c" }, new[] { "a", "x", "d", "e" })]
    [InlineData(new[] { "e", "b", "d" }, new[] { "a", "x", "c" })]
    [InlineData(new[] { "a" }, new[] { "x", "b", "c", "d", "e" })]
    [InlineData(new string[0], new[] { "a", "b", "c", "d", "e", "x" })]
    public void ReplaceWith(string[] selected, string[] expected)
    {
        var list = new List<string> { "a", "b", "c", "d", "e" };
        var sel = ListSelection.From(selected);

        new ListMutation<string>.ReplaceWith("x").Apply(list, ref sel);

        Assert.Equal(expected, list);
        CollectionAsserts.SameItems(sel, new[] { "x" });
    }

    [Theory]
    [InlineData(new[] { "b", "c" }, new[] { "x" }, new[] { "a", "x", "d" })]
    [InlineData(new[] { "a" }, new[] { "x", "y" }, new[] { "x", "y", "b", "c", "d" })]
    [InlineData(new[] { "c", "d" }, new[] { "x", "y", "z" }, new[] { "a", "b", "x", "y", "z" })]
    public void ReplaceRange(string[] oldRange, string[] newRange, string[] expected)
    {
        var list = new List<string> { "a", "b", "c", "d" };
        var sel = ListSelection.Of("a");

        new ListMutation<string>.ReplaceRange(oldRange.ToList(), newRange.ToList()).Apply(list, ref sel);

        Assert.Equal(expected, list);
        CollectionAsserts.SameItems(sel, new[] { "a" });
    }

    [Theory]
    [InlineData("x")]
    [InlineData("b,d")]
    [InlineData("d,e")]
    [InlineData("a,b,c,d,e")]
    public void ReplaceRange_NotPresent_DoesNothing(string oldRange)
    {
        var list = new List<string> { "a", "b", "c", "d" };
        var sel = ListSelection.Empty<string>();

        new ListMutation<string>.ReplaceRange(oldRange.Split(',').ToList(), ["x"]).Apply(list, ref sel);

        Assert.Equal(new[] { "a", "b", "c", "d" }, list);
    }

    [Fact]
    public void ReplaceRange_EmptyRange_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ListMutation<string>.ReplaceRange([], ["x"]));
        Assert.Throws<ArgumentException>(() => new ListMutation<string>.ReplaceRange(["x"], []));
    }

    [Fact]
    public void Append()
    {
        var list = new List<string> { "a" };
        var sel = ListSelection.Of("a");

        new ListMutation<string>.Append("b", "c").Apply(list, ref sel);

        Assert.Equal(new[] { "a", "b", "c" }, list);
        CollectionAsserts.SameItems(sel, new[] { "a" });
    }

    [Fact]
    public void AppendAndSelect()
    {
        var list = new List<string> { "a" };
        var sel = ListSelection.Of("a");

        new ListMutation<string>.AppendAndSelect("b", "c").Apply(list, ref sel);

        Assert.Equal(new[] { "a", "b", "c" }, list);
        CollectionAsserts.SameItems(sel, new[] { "b", "c" });
    }

    [Fact]
    public void ApplyToSelectable_UpdatesSelection()
    {
        var list = new List<string> { "a", "b" };
        var selectable = new Selectable<string> { Selection = ListSelection.Of("a") };

        new ListMutation<string>.DeleteSelected().Apply(list, selectable);

        Assert.Equal(new[] { "b" }, list);
        Assert.Empty(selectable.Selection);
    }

    private static void AssertMutation(ListMutation<string> mutation, string[] input, string[] expected)
    {
        var list = input.ToList();
        var sel = ListSelection.From(input.Take(1));

        mutation.Apply(list, ref sel);

        Assert.Equal(expected, list);
        Assert.Empty(sel);
    }
}