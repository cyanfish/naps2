namespace NAPS2.Images;

public class UndoStack : IDisposable
{
    private readonly int _maxLength;
    private readonly LinkedList<Memento> _stack;
    private LinkedListNode<Memento> _current;

    public UndoStack(int maxLength)
    {
        _maxLength = maxLength;
        _stack = new LinkedList<Memento>();
        _stack.AddFirst(new Memento(new List<ScannedImage>()));
        _current = _stack.First;
    }

    public Memento Current => _current.Value;

    public bool Push(IEnumerable<ScannedImage> images)
    {
        return Push(new Memento(images));
    }

    public bool Push(Memento memento)
    {
        if (_stack.First.Value == memento)
        {
            return false;
        }
        ClearRedo();
        _stack.AddFirst(memento);
        _current = _stack.First;
        Trim();
        return true;
    }

    private void Trim()
    {
        while (_stack.Count > _maxLength && _stack.Last != _current)
        {
            _stack.Last.Value.Dispose();
            _stack.RemoveLast();
        }
    }

    public void ClearRedo()
    {
        while (_stack.First != _current)
        {
            _stack.First.Value.Dispose();
            _stack.RemoveFirst();
        }
    }

    public void ClearUndo()
    {
        while (_stack.Last != _current)
        {
            _stack.Last.Value.Dispose();
            _stack.RemoveLast();
        }
    }
        
    public void ClearBoth()
    {
        ClearRedo();
        ClearUndo();
    }

    public bool Undo()
    {
        if (_current.Next != null)
        {
            _current = _current.Next;
            return true;
        }
        return false;
    }

    public bool Redo()
    {
        if (_current.Previous != null)
        {
            _current = _current.Previous;
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        foreach (var memento in _stack)
        {
            memento.Dispose();
        }
    }
}