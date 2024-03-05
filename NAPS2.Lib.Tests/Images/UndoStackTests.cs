using NAPS2.Sdk.Tests;
using NSubstitute;
using Xunit;

namespace NAPS2.Lib.Tests.Images;

public class UndoStackTests
{
    [Fact]
    public void Initial_NoUndoOrRedo()
    {
        var stack = new UndoStack(10);
        Assert.False(stack.Undo());
        Assert.False(stack.Redo());
    }

    [Fact]
    public void PushUndoRedoUndo()
    {
        var stack = new UndoStack(10);
        var element = Substitute.For<IUndoElement>();

        Assert.True(stack.Push(element));
        Assert.True(stack.CanUndo);
        Assert.False(stack.CanRedo);
        element.ReceivedCallsCount(0);

        Assert.True(stack.Undo());
        Assert.False(stack.CanUndo);
        Assert.True(stack.CanRedo);
        element.Received().ApplyUndo();
        element.ReceivedCallsCount(1);

        Assert.True(stack.Redo());
        Assert.True(stack.CanUndo);
        Assert.False(stack.CanRedo);
        element.Received().ApplyRedo();
        element.ReceivedCallsCount(2);

        Assert.True(stack.Undo());
        Assert.False(stack.CanUndo);
        Assert.True(stack.CanRedo);
        element.ReceivedCallsCount(3);
    }

    [Fact]
    public void StackLimit()
    {
        var stack = new UndoStack(4);
        for (int i = 1; i <= 10; i++)
        {
            stack.Push(Substitute.For<IUndoElement>());
        }
        for (int i = 10; i >= 7; i--)
        {
            Assert.True(stack.Undo());
        }
        Assert.False(stack.Undo());
        for (int i = 7; i <= 10; i++)
        {
            Assert.True(stack.Redo());
        }
        Assert.False(stack.Redo());
    }

    [Fact]
    public void ClearUndo()
    {
        var stack = new UndoStack(10);
        for (int i = 1; i <= 5; i++)
        {
            stack.Push(Substitute.For<IUndoElement>());
        }
        stack.ClearUndo();
        Assert.False(stack.Undo());
    }

    [Fact]
    public void ClearRedo()
    {
        var stack = new UndoStack(10);
        for (int i = 1; i <= 5; i++)
        {
            stack.Push(Substitute.For<IUndoElement>());
        }
        for (int i = 1; i <= 5; i++)
        {
            stack.Undo();
        }
        stack.ClearRedo();
        Assert.False(stack.Redo());
    }

    [Fact]
    public void ClearBoth()
    {
        var stack = new UndoStack(10);
        for (int i = 1; i <= 5; i++)
        {
            stack.Push(Substitute.For<IUndoElement>());
        }
        for (int i = 1; i <= 2; i++)
        {
            stack.Undo();
        }
        stack.ClearBoth();
        Assert.False(stack.Undo());
        Assert.False(stack.Redo());
    }
}