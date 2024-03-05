namespace NAPS2.Images;

public class UndoStack
{
    private readonly int _maxLength;
    private readonly List<IUndoElement> _stack = [];
    private int _position = 0;

    public UndoStack(int maxLength)
    {
        _maxLength = maxLength;
    }

    public bool Push(IUndoElement element)
    {
        ClearRedo();
        _stack.Insert(0, element);
        if (_stack.Count > _maxLength)
        {
            _stack.RemoveRange(_maxLength, _stack.Count - _maxLength);
        }
        return true;
    }

    public void ClearRedo()
    {
        _stack.RemoveRange(0, _position);
        _position = 0;
    }

    public void ClearUndo()
    {
        _stack.RemoveRange(_position, _stack.Count - _position);
    }

    public void ClearBoth()
    {
        ClearRedo();
        ClearUndo();
    }

    public bool CanUndo => _position < _stack.Count;

    public bool CanRedo => _position > 0;

    public bool Undo()
    {
        if (CanUndo)
        {
            _position++;
            _stack[_position - 1].ApplyUndo();
            return true;
        }
        return false;
    }

    public bool Redo()
    {
        if (CanRedo)
        {
            _position--;
            _stack[_position].ApplyRedo();
            return true;
        }
        return false;
    }
}