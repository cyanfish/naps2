using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Images
{
    public static class ListSelection
    {
        public static ListSelection<T> From<T>(List<T> list)
        {
            return new ListSelection<T>(list);
        }
        
        public static ListSelection<T> FromSelectedIndices<T>(List<T> list, IEnumerable<int> selectedIndices)
        {
            return new ListSelection<T>(list.ElementsAt(selectedIndices));
        }

        public static ListSelection<T> Empty<T>()
        {
            return new ListSelection<T>(Enumerable.Empty<T>());
        }

        public static ListSelection<T> Single<T>(T item)
        {
            return new ListSelection<T>(Enumerable.Repeat(item, 1));
        }
    }

    public class ListSelection<T> : IEnumerable<T>
    {
        private readonly HashSet<T> internalSelection;

        public ListSelection(IEnumerable<T> selectedItems)
        {
            internalSelection = new HashSet<T>(selectedItems);
        }

        public IEnumerable<int> ToSelectedIndices(List<T> list) => list.IndiciesOf(internalSelection);

        public IEnumerator<T> GetEnumerator() => internalSelection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
