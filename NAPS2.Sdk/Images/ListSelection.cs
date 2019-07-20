using System.Collections;
using System.Collections.Generic;
using NAPS2.Util;

namespace NAPS2.Images
{
    public static class ListSelection
    {
        public static ListSelection<T> FromSelectedIndices<T>(List<T> list, IEnumerable<int> selectedIndices)
        {
            return new ListSelection<T>(list.ElementsAt(selectedIndices));
        }
    }

    public class ListSelection<T> : IEnumerable<T>
    {
        private readonly HashSet<T> internalSelection;

        public ListSelection(IEnumerable<T> selectedItems)
        {
            internalSelection = new HashSet<T>(selectedItems);
        }

        public void Clear()
        {
            internalSelection.Clear();
        }

        public IEnumerable<int> ToSelectedIndices(List<T> list) => list.IndiciesOf(internalSelection);

        public IEnumerator<T> GetEnumerator() => internalSelection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
