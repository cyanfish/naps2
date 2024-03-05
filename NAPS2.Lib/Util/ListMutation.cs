namespace NAPS2.Util;

public abstract class ListMutation<T> where T : notnull
{
    public void Apply(List<T> list, ISelectable<T> selectable)
    {
        var selection = selectable.Selection;
        Apply(list, ref selection);
        if (selection != selectable.Selection)
        {
            selectable.Selection = selection;
        }
    }
        
    public abstract void Apply(List<T> list, ref ListSelection<T> selection);

    /// <summary>
    /// Moves the selection one element later in the list.
    /// </summary>
    public class MoveDown : ListMutation<T>
    {
        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            int upperBound = list.Count - 1;
            foreach (int i in selection.ToSelectedIndices(list).Reverse())
            {
                // TODO: How do we want to handle this in mutations? Is it a real error case?
                if (i == -1) break;
                if (i != upperBound--)
                {
                    var item = list[i];
                    list.RemoveAt(i);
                    list.Insert(i + 1, item);
                }
            }
        }
    }

    /// <summary>
    /// Moves the selection one earlier in the list.
    /// </summary>
    public class MoveUp : ListMutation<T>
    {
        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            int lowerBound = 0;
            foreach (int i in selection.ToSelectedIndices(list))
            {
                if (i == -1) break;
                if (i != lowerBound++)
                {
                    var item = list[i];
                    list.RemoveAt(i);
                    list.Insert(i - 1, item);
                }
            }
        }
    }

    /// <summary>
    /// Moves the selection to the specified index in the list.
    /// </summary>
    public class MoveTo : ListMutation<T>
    {
        private readonly int _destinationIndex;

        public MoveTo(int destinationIndex)
        {
            _destinationIndex = destinationIndex;
        }
            
        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            var indexList = selection.ToSelectedIndices(list).ToList();
            var bottom = indexList.Where(x => x != -1 && x < _destinationIndex).OrderByDescending(x => x).ToList();
            var top = indexList.Where(x =>  x != -1 &&x >= _destinationIndex).OrderBy(x => x).ToList();

            int offset = 1;
            foreach (int i in bottom)
            {
                var item = list[i];
                list.RemoveAt(i);
                list.Insert(_destinationIndex - offset, item);
                offset++;
            }

            offset = 0;
            foreach (int i in top)
            {
                var item = list[i];
                list.RemoveAt(i);
                list.Insert(_destinationIndex + offset, item);
                offset++;
            }
        }
    }

    /// <summary>
    /// Converts lists in the order 1,3,5,2,4,6 to 1,2,3,4,5,6.
    /// </summary>
    public class Interleave : ListMutation<T>
    {
        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            // Partition the list in two
            int count = list.Count;
            int split = (count + 1) / 2;
            var p1 = list.Take(split).ToList();
            var p2 = list.Skip(split).ToList();

            // Rebuild the list, taking alternating items from each the partitions
            list.Clear();
            for (int i = 0; i < count; ++i)
            {
                list.Add(i % 2 == 0 ? p1[i / 2] : p2[i / 2]);
            }

            selection = ListSelection.Empty<T>();
        }
    }

    /// <summary>
    /// Converts lists in the order 1,2,3,4,5,6 to 1,3,5,2,4,6.
    /// </summary>
    public class Deinterleave : ListMutation<T>
    {
        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            // Duplicate the list
            int count = list.Count;
            int split = (count + 1) / 2;
            var copy = list.ToList();

            // Rebuild the list, even-indexed items first
            list.Clear();
            for (int i = 0; i < split; ++i)
            {
                list.Add(copy[i * 2]);
            }

            for (int i = 0; i < (count - split); ++i)
            {
                list.Add(copy[i * 2 + 1]);
            }

            selection = ListSelection.Empty<T>();
        }
    }

    /// <summary>
    /// Converts lists in the order 1,3,5,6,4,2 to 1,2,3,4,5,6.
    /// </summary>
    public class AltInterleave : ListMutation<T>
    {
        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            // Partition the list in two
            int count = list.Count;
            int split = (count + 1) / 2;
            var p1 = list.Take(split).ToList();
            var p2 = list.Skip(split).ToList();

            // Rebuild the list, taking alternating items from each the partitions (the latter in reverse order)
            list.Clear();
            for (int i = 0; i < count; ++i)
            {
                list.Add(i % 2 == 0 ? p1[i / 2] : p2[p2.Count - 1 - i / 2]);
            }

            selection = ListSelection.Empty<T>();
        }
    }

    /// <summary>
    /// Converts lists in the order 1,2,3,4,5,6 to 1,3,5,6,4,2.
    /// </summary>
    public class AltDeinterleave : ListMutation<T>
    {
        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            // Duplicate the list
            int count = list.Count;
            int split = (count + 1) / 2;
            var copy = list.ToList();

            // Rebuild the list, even-indexed items first (odd-indexed items in reverse order)
            list.Clear();
            for (int i = 0; i < split; ++i)
            {
                list.Add(copy[i * 2]);
            }

            for (int i = count - split - 1; i >= 0; --i)
            {
                list.Add(copy[i * 2 + 1]);
            }

            selection = ListSelection.Empty<T>();
        }
    }

    /// <summary>
    /// Reverses the order of the entire list.
    /// </summary>
    public class ReverseAll : ListMutation<T>
    {
        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            list.Reverse();
        }
    }

    /// <summary>
    /// Reverses the order of the selection.
    /// </summary>
    public class ReverseSelection : ListMutation<T>
    {
        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            var indexList = selection.ToSelectedIndices(list).Where(x => x != -1).ToList();
            int pairCount = indexList.Count / 2;

            // Swap pairs in the selection, excluding the middle element (if the total count is odd)
            for (int i = 0; i < pairCount; i++)
            {
                int x = indexList[i];
                int y = indexList[indexList.Count - i - 1];
                (list[x], list[y]) = (list[y], list[x]);
            }
        }
    }

    /// <summary>
    /// Deletes all elements from the list.
    /// </summary>
    public class DeleteAll : ListMutation<T>
    {
        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            foreach (var item in list)
            {
                (item as IDisposable)?.Dispose();
            }
            list.Clear();
            selection = ListSelection.Empty<T>();
        }
    }

    /// <summary>
    /// Deletes the selection from the list.
    /// </summary>
    public class DeleteSelected : ListMutation<T>
    {
        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            foreach (var item in selection)
            {
                (item as IDisposable)?.Dispose();
            }
            list.RemoveAll(selection);
            selection = ListSelection.Empty<T>();
        }
    }

    /// <summary>
    /// Inserts the given item at the given index.
    /// </summary>
    public class InsertAt : ListMutation<T>
    {
        private readonly int _index;
        private readonly T _item;

        public InsertAt(int index, T item)
        {
            _index = index;
            _item = item;
        }

        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            list.Insert(_index, _item);
        }
    }

    /// <summary>
    /// Inserts the given item after the given predecessor (or at the end of the list if none).
    /// </summary>
    public class InsertAfter : ListMutation<T>
    {
        private readonly T _itemToInsert;
        private readonly T? _predecessor;

        public InsertAfter(T itemToInsert, T? predecessor)
        {
            _itemToInsert = itemToInsert;
            _predecessor = predecessor;
        }

        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            // Default to the end of the list
            int index = list.Count;
            // Use the index after the last item from the same source (if it exists)
            if (_predecessor != null)
            {
                int lastIndex = list.IndexOf(_predecessor);
                if (lastIndex != -1)
                {
                    index = lastIndex + 1;
                }
            }
            list.Insert(index, _itemToInsert);
        }
    }

    /// <summary>
    /// Replaces the selection with the given item.
    /// </summary>
    public class ReplaceWith : ListMutation<T>
    {
        private readonly T _newItem;

        public ReplaceWith(T newItem)
        {
            _newItem = newItem;
        }

        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            int firstIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (selection.Contains(list[i]))
                {
                    if (firstIndex == -1)
                    {
                        firstIndex = i;
                    }
                    list.RemoveAt(i);
                    i--;
                }
            }
            if (firstIndex == -1)
            {
                firstIndex = list.Count;
            }
            list.Insert(firstIndex, _newItem);
                
            selection = ListSelection.Of(_newItem);
        }
    }

    /// <summary>
    /// Replaces the given sequence of items with a different sequence of items. Both sequences must be non-empty,
    /// and if the original sequence isn't present nothing happens.
    /// </summary>
    public class ReplaceRange : ListMutation<T>
    {
        private readonly List<T> _oldRange;
        private readonly List<T> _newRange;

        public ReplaceRange(List<T> oldRange, List<T> newRange)
        {
            if (oldRange.Count == 0 || newRange.Count == 0) throw new ArgumentException();
            _oldRange = oldRange;
            _newRange = newRange;
        }

        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i], _oldRange[0]))
                {
                    bool match = true;
                    for (int j = 0; j < _oldRange.Count; j++)
                    {
                        if (!EqualityComparer<T>.Default.Equals(list[i + j], _oldRange[j]))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        index = i;
                        break;
                    }
                }
            }
            list.RemoveRange(index, _oldRange.Count);
            list.InsertRange(index, _newRange);
        }
    }

    /// <summary>
    /// Appends the given item(s) to the end of the list.
    /// </summary>
    public class Append : ListMutation<T>
    {
        private readonly List<T> _items;

        public Append(IEnumerable<T> items)
        {
            _items = items.ToList();
        }

        public Append(params T[] items)
        {
            _items = items.ToList();
        }

        public override void Apply(List<T> list, ref ListSelection<T> selection)
        {
            list.AddRange(_items);
            selection = ListSelection.From(_items);
        }
    }
}