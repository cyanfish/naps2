using System;
using System.Collections.Generic;
using System.Linq;

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

        public class Memento : IEquatable<Memento>, IDisposable
        {
            private readonly List<ScannedImage.Snapshot> snapshots;
            
            public static readonly Memento Empty = new Memento(new List<ScannedImage.Snapshot>());

            public Memento(IEnumerable<ScannedImage> images)
                : this(images.Select(x => x.Preserve()))
            {
            }

            public Memento(IEnumerable<ScannedImage.Snapshot> snapshots)
            {
                this.snapshots = snapshots.ToList();
            }

            public bool Equals(Memento other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return snapshots.SequenceEqual(other.snapshots);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Memento) obj);
            }

            public override int GetHashCode() => snapshots.GetHashCode();

            public static bool operator ==(Memento left, Memento right) => Equals(left, right);

            public static bool operator !=(Memento left, Memento right) => !Equals(left, right);

            public void Dispose()
            {
                foreach (var snapshot in snapshots)
                {
                    snapshot.Dispose();
                }
            }
        }
    }
}
