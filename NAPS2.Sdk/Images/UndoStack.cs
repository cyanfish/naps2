using System;
using System.Collections.Generic;

namespace NAPS2.Images
{
    public class UndoStack : IDisposable
    {
        private readonly int maxLength;
        private readonly LinkedList<Memento> stack;
        private LinkedListNode<Memento> current;

        public UndoStack(int maxLength)
        {
            this.maxLength = maxLength;
            stack = new LinkedList<Memento>();
            stack.AddFirst(new Memento(new List<ScannedImage>()));
            current = stack.First;
        }

        public Memento Current => current.Value;

        public bool Push(IEnumerable<ScannedImage> images)
        {
            return Push(new Memento(images));
        }

        public bool Push(Memento memento)
        {
            if (stack.First.Value == memento)
            {
                return false;
            }
            ClearRedo();
            stack.AddFirst(memento);
            current = stack.First;
            Trim();
            return true;
        }

        private void Trim()
        {
            while (stack.Count > maxLength && stack.Last != current)
            {
                stack.Last.Value.Dispose();
                stack.RemoveLast();
            }
        }

        public void ClearRedo()
        {
            while (stack.First != current)
            {
                stack.First.Value.Dispose();
                stack.RemoveFirst();
            }
        }

        public void ClearUndo()
        {
            while (stack.Last != current)
            {
                stack.Last.Value.Dispose();
                stack.RemoveLast();
            }
        }
        
        public void ClearBoth()
        {
            ClearRedo();
            ClearUndo();
        }

        public bool Undo()
        {
            if (current.Next != null)
            {
                current = current.Next;
                return true;
            }
            return false;
        }

        public bool Redo()
        {
            if (current.Previous != null)
            {
                current = current.Previous;
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            foreach (var memento in stack)
            {
                memento.Dispose();
            }
        }
    }
}
