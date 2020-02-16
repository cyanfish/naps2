using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Images
{
    public static class ListSelection
    {
        public static ListSelection<T> From<T>(IEnumerable<T> list)
        {
            return new ListSelection<T>(list);
        }

        public static ListSelection<T> Of<T>(params T[] items)
        {
            return new ListSelection<T>(items);
        }
        
        public static ListSelection<T> FromSelectedIndices<T>(IList<T> list, IEnumerable<int> selectedIndices)
        {
            return new ListSelection<T>(list.ElementsAt(selectedIndices));
        }

        public static ListSelection<T> Empty<T>()
        {
            return new ListSelection<T>(Enumerable.Empty<T>());
        }
    }

    public class ListSelection<T> : IEnumerable<T>, IEquatable<ListSelection<T>>
    {
        private readonly HashSet<T> _internalSelection;

        public ListSelection(IEnumerable<T> selectedItems)
        {
            _internalSelection = new HashSet<T>(selectedItems);
        }

        public IEnumerable<int> ToSelectedIndices(List<T> list) => list.IndiciesOf(_internalSelection);

        public int Count => _internalSelection.Count;
        
        public bool Contains(T item) => _internalSelection.Contains(item);

        public IEnumerator<T> GetEnumerator() => _internalSelection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(ListSelection<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _internalSelection.SetEquals(other._internalSelection);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ListSelection<T>) obj);
        }

        public override int GetHashCode() => _internalSelection.GetHashCode();

        public static bool operator ==(ListSelection<T> left, ListSelection<T> right) => Equals(left, right);

        public static bool operator !=(ListSelection<T> left, ListSelection<T> right) => !Equals(left, right);
    }

    public interface ISelectable<T>
    {
        ListSelection<T> Selection { get; set; }
    }
    
    public class Selectable<T>
    {
        private ListSelection<T> _selection = ListSelection.Empty<T>();

        public ListSelection<T> Selection
        {
            get => _selection;
            set => _selection = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
